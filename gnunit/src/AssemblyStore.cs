//
// AssemblyStore.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using GLib;
using Gtk;
using NUnit.Core;
using Thread = System.Threading.Thread;

namespace Mono.NUnit.GUI
{
	delegate void FixtureAddedEventHandler (object sender, FixtureAddedEventArgs args);
	delegate void FixtureLoadErrorHandler (object sender, FixtureLoadErrorEventArgs args);

	class FixtureAddedEventArgs : EventArgs
	{
		int total;
		int current;

		public FixtureAddedEventArgs (int current, int total)
		{
			this.total = total;
			this.current = current;
		}

		public int Total {
			get { return total; }
		}

		public int Current {
			get { return current; }
		}
	}
	
	delegate void TestStartHandler (TestCase test);
	delegate void TestFinishHandler (TestCaseResult result);
	delegate void SuiteStartHandler (TestSuite test);
	delegate void SuiteFinishHandler (TestSuiteResult result);
	delegate void TestCaseResultHandler (TestResult result);

	abstract class QueuedEvent
	{
		public abstract void DoCallback ();
	}
	
	class QueuedTestEvent : QueuedEvent
	{
		Delegate te;
		object arg;

		public QueuedTestEvent (TestCaseResultHandler te, object arg)
		{
			this.te = te;
			this.arg = arg;
		}

		public override void DoCallback ()
		{
			te.DynamicInvoke (new object [] {arg});
		}
	}
	
	class QueuedSuiteStart : QueuedEvent
	{
		SuiteStartHandler handler;
		TestSuite suite;

		public QueuedSuiteStart (SuiteStartHandler handler, TestSuite suite)
		{
			this.handler = handler;
			this.suite = suite;
		}

		public override void DoCallback ()
		{
			handler (suite);
		}
	}

	class QueuedSuiteFinish : QueuedEvent
	{
		SuiteFinishHandler handler;
		TestSuiteResult result;

		public QueuedSuiteFinish (SuiteFinishHandler handler, TestSuiteResult result)
		{
			this.handler = handler;
			this.result = result;
		}

		public override void DoCallback ()
		{
			handler (result);
		}
	}

	class QueuedTestStart : QueuedEvent
	{
		TestStartHandler handler;
		TestCase test;

		public QueuedTestStart (TestStartHandler handler, TestCase test)
		{
			this.handler = handler;
			this.test = test;
		}

		public override void DoCallback ()
		{
			handler (test);
		}
	}

	class QueuedTestFinish : QueuedEvent
	{
		TestFinishHandler handler;
		TestCaseResult result;

		public QueuedTestFinish (TestFinishHandler handler, TestCaseResult result)
		{
			this.handler = handler;
			this.result = result;
		}

		public override void DoCallback ()
		{
			handler (result);
		}
	}

	class FixtureLoadErrorEventArgs : EventArgs
	{
		string message;
		string filename;

		public FixtureLoadErrorEventArgs (string filename, Exception e)
		{
			this.filename = filename;
			message = e.Message;
		}

		public string FileName {
			get { return filename; }
		}

		public string Message {
			get { return message; }
		}
	}

	class CategoriesEventArgs : EventArgs {
		ICollection categories;

		public CategoriesEventArgs (ICollection categories)
		{
			this.categories = categories;
		}

		public ICollection Categories {
			get { return categories; }
		}
	}

	class AssemblyStore : TreeStore, EventListener
	{
		string assemblyName;
		Hashtable iters;
		TestSuite rootTS;
		TestResult lastResult;
		int totalTests;
		int currentTest;

		bool runningTest;
		bool cancelled;
		EventListener listener;
		Test test;
		ManualResetEvent idle;
		Queue pending;
		System.Threading.Thread th;
		string location;
		IFilter filter;

		Exception exception;
		static GLib.GType gtype = GLib.GType.Invalid;

		public event FixtureAddedEventHandler FixtureAdded;
		public event FixtureLoadErrorHandler FixtureLoadError;
		public event EventHandler FinishedRunning;
		public event EventHandler FinishedLoad;
		public event EventHandler IdleCallback;

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}

		public AssemblyStore (string assemblyName)
			: base (GType.Int, GType.String)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			this.assemblyName = assemblyName;
			location = "";
		}

		public static new GLib.GType GType {
			get {
				if (gtype == GLib.GType.Invalid)
					gtype = RegisterGType (typeof (AssemblyStore));
				return gtype;
			}
		}

		public IFilter Filter {
			get { return filter; }
			set { filter = value; }
		}
		
		public string Location {
			get { return location; }
		}
		
		public bool Running {
			get { return runningTest; }
		}
		
		public bool CancelRequest ()
		{
			if (runningTest) {
				// TODO: why the heck does this wreak havoc in the runtime?
				// by now, we use the filter to stop running tests, which,
				// btw, works perfectly well.
				// th.Abort ();
				cancelled = true;
				return true;
			}

			return false;
		}
		
		public bool Cancelled {
			get { return cancelled; }
		}
		
		public TestResult LastResult {
			get { return lastResult; }
		}
		
		public int SearchColumn {
			get { return 1; }
		}
		
		public void Load ()
		{
			// I don't like this...
			Assembly a;
			try {
				a = Assembly.Load (assemblyName);
				location = a.Location;
			} catch (Exception e) {
				try {
					a = Assembly.LoadFrom (assemblyName);
					location = a.Location;
				} catch {
					location = "";
				}
			}

			Idle.Add (new IdleHandler (Populate));
		}

		void GrayOut (TreeIter iter)
		{
			SetValue (iter, 0, (int) CircleColor.None);
			TreeIter child;
			if (!IterChildren (out child, iter))
				return;

			do {
				GrayOut (child);
			} while (IterNext (ref child));
		}

		public void RunTestAtIter (TreeIter iter, EventListener listener, ref int ntests)
		{
			if (iter.Equals (TreeIter.Zero))
				return;
			
			RunTestAtPath (GetPath (iter), listener, ref ntests);
		}

		public void RunTestAtPath (TreePath path, EventListener listener, ref int ntests)
		{
			if (runningTest)
				throw new InvalidOperationException (_("Already running some test(s)."));
			
			cancelled = false;
			if (idle == null) {
				idle = new ManualResetEvent (false);
				pending = new Queue ();
			}

			TreeIter iter;
			GetIter (out iter, path);
			GrayOut (iter);

			if (iter.Equals (TreeIter.Zero))
				return;

			string p = GetPath (iter).ToString ();
			if (p == null || p == "")
				return;

			test = LookForTestByPath (p, null);
			if (test == null)
				return;

			ntests = test.CountTestCases (filter);
			runningTest = true;
			this.listener = listener;
			th = new System.Threading.Thread (new ThreadStart (InternalRunTest));
			th.IsBackground = true;
			th.Start ();
			Idle.Add (new IdleHandler (Updater));
		}

		public new void Clear ()
		{
			base.Clear ();
			iters = null;
			lastResult = null;
		}
		
		void DoPending ()
		{
			QueuedEvent [] events;
			lock (pending) {
				events = new QueuedEvent [pending.Count];
				pending.CopyTo (events, 0);
				pending.Clear ();
			}

			foreach (QueuedEvent e in events)
				e.DoCallback ();
		}

		bool Updater ()
		{
			if (!idle.WaitOne (0, true)) {
				if (IdleCallback != null)
					IdleCallback (this, EventArgs.Empty);

				return true;
			}

			DoPending ();
			if (runningTest == false) {
				DoPending ();
				OnFinishedRunning ();
			}

			idle.Reset ();
			return runningTest;
		}

		void InternalRunTest ()
		{
			lastResult = null;
			try {
				lastResult = test.Run (this, filter);
			} catch (ThreadAbortException) {
				Thread.ResetAbort ();
				cancelled = true;
			} finally {
				runningTest = false;
				idle.Set ();
			}
		}

		Test LookForTestByPath (string path, Test t)
		{
			string [] parts = path.Split (':');
			if (t == null) {
				if (parts.Length > 1)
					return LookForTestByPath (String.Join (":", parts, 1, parts.Length - 1), rootTS);

				return rootTS;
			}

			Test ret;
			//Console.WriteLine ("Count: {0} Index: {1} path: '{2}'", t.Tests.Count, parts [0], path);
			if (parts.Length == 1) {
				ret = (Test) t.Tests [Int32.Parse (path)];
				return ret;
			}

			ret = (Test) t.Tests [Int32.Parse (parts [0])];
			//Console.WriteLine ("Recurse: " + ret.FullName + " " + String.Join (":", parts, 1, parts.Length - 1));
			return LookForTestByPath (String.Join (":", parts, 1, parts.Length - 1), ret);
						  
		}

		TreeIter AddFixture (TreeIter parent, string fullName)
		{
			string typeName = fullName;
			string [] parts = typeName.Split ('.');
			string index = "";

			foreach (string s in parts) {
				if (index == "")
					index = s;
				else
					index += "." + s;

				if (iters.ContainsKey (index)) {
					parent = (TreeIter) iters [index];
					continue;
				}
				
				parent = AppendValues (parent, (int) CircleColor.None, s);
				iters [index] = parent;
			}

			return parent;
		}

		void AddSuite (TreeIter parent, TestSuite suite, int n)
		{
			TreeIter next;
			foreach (Test t in suite.Tests) {
				next = AddFixture (parent, t.FullName);
				if ((n % 5) == 0) {
					while (GLib.MainContext.Iteration ());
				}

				n++;
				if (t.IsSuite)
					AddSuite (next, (TestSuite) t, n);
				else if (FixtureAdded != null)
					FixtureAdded (this, new FixtureAddedEventArgs (++currentTest, totalTests));

			}
		}

		bool Populate ()
		{
			Clear ();
			iters = new Hashtable ();
			TreeIter first;
			// gtk-sharp 2.0
			// first = AppendNode ();
			Append (out first);
			SetValue (first, 0, (int) CircleColor.None);
			SetValue (first, 1, assemblyName);
			iters [assemblyName] = first;
			ResolveEventHandler reh = new ResolveEventHandler (TryLoad);
			AppDomain.CurrentDomain.AssemblyResolve += reh;

			try {
				rootTS = new TestSuiteBuilder ().Build (assemblyName);
			} catch (Exception e) {
				if (FixtureLoadError != null) {
					exception = e;
					Idle.Add (new IdleHandler (TriggerError));
				}
				return false;
			} finally {
				AppDomain.CurrentDomain.AssemblyResolve -= reh;
			}

			currentTest = 0;
			totalTests = rootTS.CountTestCases ();
			AddSuite (first, rootTS, 0);
			OnFinishedLoad (CategoryManager.Categories);

			return false;
		}

		void OnFinishedLoad (ICollection categories)
		{
			if (FinishedLoad != null)
				FinishedLoad (this, new CategoriesEventArgs (categories));
		}
		
		void OnFinishedRunning ()
		{
			if (FinishedRunning != null)
				FinishedRunning (this, EventArgs.Empty);
		}

		bool TriggerError ()
		{
			FixtureLoadError (this, new FixtureLoadErrorEventArgs (assemblyName, exception));
			exception = null;
			return false;
		}

		Assembly TryLoad (object sender, ResolveEventArgs args)
		{
			try {
				// NUnit2 uses Assembly.Load on the filename without extension.
				// This is done just to allow loading from a full path name.
				return Assembly.LoadFrom (assemblyName);
			} catch { }

			return null;
		}

		// Interface NUnit.Core.EventListener
		void EventListener.RunStarted (Test [] tests)
		{
		}

		void EventListener.RunFinished (TestResult [] results)
		{
		}

		void EventListener.UnhandledException (Exception exception)
		{
		}

		void EventListener.RunFinished (Exception exc)
		{
		}

		void EventListener.TestStarted (TestCase testCase)
		{
			if (listener != null) {
				Monitor.Enter (pending);
				pending.Enqueue (new QueuedTestStart (new TestStartHandler (listener.TestStarted), testCase));
				Monitor.Exit (pending);
				idle.Set ();
			}
		}
			
		void EventListener.TestFinished (TestCaseResult result)
		{
			Monitor.Enter (pending);
			if (listener != null)
				pending.Enqueue (new QueuedTestFinish (new TestFinishHandler (listener.TestFinished), result));

			pending.Enqueue (new QueuedTestEvent (new TestCaseResultHandler (SetIconFromResult), result));
			Monitor.Exit (pending);
			idle.Set ();
		}

		void EventListener.SuiteStarted (TestSuite suite)
		{
			if (listener != null) {
				Monitor.Enter (pending);
				pending.Enqueue (new QueuedSuiteStart (new SuiteStartHandler (listener.SuiteStarted), suite));
				Monitor.Exit (pending);
				idle.Set ();
			}
		}

		void EventListener.SuiteFinished (TestSuiteResult result)
		{
			Monitor.Enter (pending);
			if (listener != null)
				pending.Enqueue (new QueuedSuiteFinish (new SuiteFinishHandler (listener.SuiteFinished), result));

			pending.Enqueue (new QueuedTestEvent (new TestCaseResultHandler (SetIconFromResult), result));
			Monitor.Exit (pending);
			idle.Set ();
		}

		void SetIconFromResult (TestResult result)
		{
			CircleColor color;
			if (!result.Executed)
				color = CircleColor.NotRun;
			else if (result.IsFailure)
				color = CircleColor.Failure;
			else if (result.IsSuccess)
				color = CircleColor.Success;
			else {
				Console.WriteLine (_("Warning: unexpected combination."));
				color = CircleColor.None;
			}

			string fullname = result.Test.FullName;
			if (iters.ContainsKey (fullname)) {
				TreeIter iter = (TreeIter) iters [fullname];
				SetValue (iter, 0, (int) color);
			} else {
				Console.WriteLine (_("Don't know anything about {0}"), fullname);
			}
		}
	}
}

