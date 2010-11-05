//
// Unit tests for DecorateThreadsRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Test.Rules.Helpers;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DecorateThreadsTest {
		
		#region Test Cases
		// No threaded code.
		internal sealed class Good1 {
			public Good1 ()
			{
			}
			
			public void Greeting ()
			{
				Console.WriteLine ("hello");
			}
		}
		
		// Thread entry points cannot be main thread.
		internal sealed class Good2 {
			public void Spawn ()
			{
				new Thread (this.Thread1).Start ();
				new Thread (this.Thread2).Start ();
				new Thread (this.Thread3).Start ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Thread1 ()
			{
			}
			
			[ThreadModel (ThreadModel.Serializable)]
			private void Thread2 ()
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			private void Thread3 ()
			{
			}
		}
		
		// MainThread code can call everything, AllowEveryCaller code can be called by 
		// everything, SingleThread can call SingleThread/Serializable/Concurrent, and Serializable/
		// Concurrent can call Serializable/Concurrent.
		internal sealed class Good3 {
			public void Stuff ()
			{
				MainStuff ();
				SingleStuff ();
				SerializableStuff ();
				ConcurrentStuff ();
				AllStuff ();
			}
			
			[ThreadModel (ThreadModel.MainThread)]
			private void MainStuff ()
			{
				MainStuff ();
				SingleStuff ();
				SerializableStuff ();
				ConcurrentStuff ();
				AllStuff ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void SingleStuff ()
			{
				SingleStuff ();
				SerializableStuff ();
				ConcurrentStuff ();
				AllStuff ();
			}
			
			[ThreadModel (ThreadModel.Serializable)]
			private void SerializableStuff ()
			{
				SerializableStuff ();
				ConcurrentStuff ();
				AllStuff ();
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			private void ConcurrentStuff ()
			{
				SerializableStuff ();
				ConcurrentStuff ();
				AllStuff ();
			}
			
			[ThreadModel (ThreadModel.MainThread | ThreadModel.AllowEveryCaller)]
			private void AllStuff ()
			{
			}
		}
		
		// An override of a base method or an implementation of an interface method must 
		// use the same threading model as the original method.
		internal class Base {
			public virtual void MainStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread | ThreadModel.AllowEveryCaller)]
			public virtual void AllSingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public virtual void SingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			public virtual void ConcurrentStuff ()
			{
			}
		}
		
		internal interface Interface {
			void MainStuff ();
						
			[ThreadModel (ThreadModel.SingleThread)]
			void SingleStuff ();
			
			[ThreadModel (ThreadModel.Concurrent)]
			void ConcurrentStuff ();
		}
		
		internal sealed class Good4 : Base {
			[ThreadModel (ThreadModel.MainThread)]
			public override void MainStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public override void SingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			public override void ConcurrentStuff ()
			{
			}
		}
		
		internal class Good5 : Interface {
			[ThreadModel (ThreadModel.MainThread)]
			public void MainStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public void SingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			public virtual void ConcurrentStuff ()
			{
			}
		}
		
		// A delegate used with a threaded event must use the same threading model as the 
		// event.
		internal class Good6 {
			public delegate void DataHandler1 (object sender, EventArgs e);
			
			[ThreadModel (ThreadModel.Concurrent)]
			public delegate void DataHandler2 (object sender, EventArgs e);
			
			[ThreadModel (ThreadModel.MainThread)]
			public event DataHandler1 RecvData1;
			
			[ThreadModel (ThreadModel.Concurrent)]
			public event DataHandler2 RecvData2;
		}
		
		// Delegates must be able to call the methods they are bound to.
		internal class Good7 {
			public void Setup ()
			{
				single_callback = this.SingleStuff;
				single_callback (null);
				
				single_callback = this.ConcurrentStuff;
				single_callback (null);

				concurrent_callback = this.ConcurrentStuff;
				concurrent_callback (null);
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public void SingleStuff (object data)
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			public void ConcurrentStuff (object data)
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			internal delegate void SingleCallback (object data);
			
			[ThreadModel (ThreadModel.Concurrent)]
			internal delegate void ConcurrentCallback (object data);
			
			internal SingleCallback single_callback;
			internal ConcurrentCallback concurrent_callback;
		}
		
		// Static ctors of concurrent and serializable types are SingleThread.
		[ThreadModel (ThreadModel.Concurrent)]
		internal sealed class Good8  {
			static Good8 ()
			{
				Stuff ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private static void Stuff ()
			{
			}
		}
		
		// Static operators inherit serializable thread from the type.
		[ThreadModel (ThreadModel.Serializable)]
		internal sealed class Good9 : IEquatable<Good9>  {
			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				
				Good9 rhs = obj as Good9;
				return this == rhs;
			}
			
			public bool Equals (Good9 rhs)
			{
				return this == rhs;
			}
			
			public static bool operator== (Good9 lhs, Good9 rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return true;
				
				if ((object) lhs == null || (object) rhs == null)
					return false;
				
				if (lhs.data != rhs.data)
					return false;
				
				return true;
			}
			
			public static bool operator!= (Good9 lhs, Good9 rhs)
			{
				return !(lhs == rhs);
			}
			
			public override int GetHashCode ()
			{
				int hash = 0;
				
				unchecked {
					hash += data.GetHashCode ();
				}
				
				return hash;
			}
			
			internal int data;
		}
		
		// Methods used within anonymous thread roots must be threaded.
		internal sealed class Good10 {
			public string path;
			private System.Threading.Thread thread;
			
			public void Spawn ()
			{
				thread = new System.Threading.Thread (() => SingleStuff (path));
				thread.Start ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void SingleStuff (string path)
			{
			}
		}
		
		// Process.Exited uses the type's thread if SynchronizingObject is set to this.
		public sealed class Good11 : System.ComponentModel.ISynchronizeInvoke {
			public void Spawn ()
			{
				var p = new System.Diagnostics.Process ();
				p.SynchronizingObject = this;
				p.Exited += this.Thread1;
				p.ErrorDataReceived += this.Thread2;
				p.Start ();
			}
			
			public bool InvokeRequired {
				get { return true; }
			}
			
			public IAsyncResult BeginInvoke (Delegate method, object [] args)
			{
				return null;
			}
			
			public object EndInvoke (IAsyncResult result)
			{
				return null;
			}
			
			public object Invoke (Delegate method, object [] args)
			{
				return null;
			}
			
			[ThreadModel (ThreadModel.MainThread)]
			private void Thread1 (object sender, EventArgs e)
			{
				Stuff ();
			}
			
			private void Stuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Thread2 (object sender, System.Diagnostics.DataReceivedEventArgs e)
			{
			}
		}
		
		// Process.Exited is threaded if SynchronizingObject is set to non-this.
		public sealed class Good12 {
			public void Spawn1 ()
			{
				var p = new System.Diagnostics.Process ();
				p.SynchronizingObject = null;
				p.Exited += this.Thread1;
				p.Start ();
			}
			
			public void Spawn2 (System.ComponentModel.ISynchronizeInvoke obj)
			{
				var p = new System.Diagnostics.Process ();
				p.SynchronizingObject = obj;
				p.Exited += this.Thread2;
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Thread1 (object sender, EventArgs e)
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			private void Thread2 (object sender, EventArgs e)
			{
			}
		}
		
		// (Non-threaded) system delegates work with anything.
		internal sealed class Good13 {
			public string path;
			private System.Threading.Thread thread;
			
			public void Spawn ()
			{
				Action a = () => SingleStuff (path);
				a ();
				
				Action b = () => MainStuff (path);
				b ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void SingleStuff (string path)
			{
			}
			
			private void MainStuff (string path)
			{
			}
		}
		
		// Another anonymous method case.
		internal sealed class Good14 {
			public void Spawn (object data)
			{
				Process (() => Stuff (data));
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public delegate void Callback ();
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Process (Callback callback)
			{
				callback ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Stuff (object data)
			{
			}
		}
		
		// Thread entry points cannot be main thread.
		internal sealed class Bad1 {
			public void Spawn ()
			{
				new Thread (this.Thread1).Start ();
				new Thread (this.Thread2).Start ();
			}
			
			[ThreadModel (ThreadModel.MainThread)]
			private void Thread1 ()
			{
			}
			
			private void Thread2 ()
			{
			}
		}
		
		// MainThread code can call everything, AllowEveryCaller code can be called by 
		// everything, SingleThread can call SingleThread/Serializable/Concurrent, and Serializable/
		// Concurrent can call Serializable/Concurrent.
		internal sealed class Bad2 {
			[ThreadModel (ThreadModel.MainThread)]
			private void MainStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void SingleStuff ()
			{
				MainStuff ();
			}
			
			[ThreadModel (ThreadModel.Serializable)]
			private void SerializableStuff ()
			{
				MainStuff ();
				SingleStuff ();
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			private void ConcurrentStuff ()
			{
				MainStuff ();
				SingleStuff ();
			}
		}
		
		// An override of a base method or an implementation of an interface method must 
		// use the same threading model as the original method.
		internal sealed class Bad3 : Base {
			[ThreadModel (ThreadModel.SingleThread)]
			public override void AllSingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			public override void MainStuff ()
			{
			}
			
			public override void SingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.MainThread)]
			public override void ConcurrentStuff ()
			{
			}
		}
		
		internal class Bad4 : Interface {
			[ThreadModel (ThreadModel.Concurrent)]
			public void MainStuff ()
			{
			}
			
			public void SingleStuff ()
			{
			}
			
			[ThreadModel (ThreadModel.MainThread)]
			public virtual void ConcurrentStuff ()
			{
			}
		}
		
		// A delegate used with a threaded event must use the same threading model as the 
		// event.
		internal class Bad5 {
			[ThreadModel (ThreadModel.Concurrent)]
			public delegate void DataHandler1 (object sender, EventArgs e);
			
			public delegate void DataHandler2 (object sender, EventArgs e);
			
			[ThreadModel (ThreadModel.MainThread)]
			public event DataHandler1 RecvData1;
			
			[ThreadModel (ThreadModel.Concurrent)]
			public event DataHandler2 RecvData2;
		}
		
		// Delegates must be able to call the methods they are bound to.
		internal class Bad6 {
			public void Setup ()
			{
				concurrent_callback = this.SingleStuff;
				concurrent_callback (null);
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public void SingleStuff (object data)
			{
			}
			
			[ThreadModel (ThreadModel.Concurrent)]
			internal delegate void ConcurrentCallback (object data);
			
			internal ConcurrentCallback concurrent_callback;
		}
		
		// Serializable cannot be applied to static methods and static methods of serializeable 
		// types do not inherit it from their types.		
		[ThreadModel (ThreadModel.Serializable)]
		internal class Bad7 {
			[ThreadModel (ThreadModel.Serializable)]
			public static void BadDecoration ()
			{
			}
			
			public void SerialStuff1 ()
			{
				NotSerial ();
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public void SerialStuff2 ()
			{
				NotSerial ();
			}
			
			public static void NotSerial ()
			{
			}
		}
		
		// Methods used within anonymous thread roots must be threaded.
		internal sealed class Bad8 {
			public string path;
			private System.Threading.Thread thread;
			
			public void Spawn ()
			{
				thread = new System.Threading.Thread (() => Work (path));
				thread.Start ();
			}
			
			private void Work (string path)
			{
			}
		}
		
		// Delegates can be constructed in different ways.
		public sealed class Bad9 {
			public void Spawn ()
			{
				new Thread (Bad9.Thread1).Start ();
				new Thread (o => Thread2 ()).Start (null);
				new Thread (new ThreadStart (Thread3)).Start ();
			}
			
			private static void Thread1 ()
			{
			}
			
			private void Thread2 ()
			{
			}
			
			private void Thread3 ()
			{
			}
		}
		
		// Process event handlers are threaded.
		public sealed class Bad10 {
			public void Spawn ()
			{
				var p = new System.Diagnostics.Process ();
				p.Exited += this.Thread1;
				p.ErrorDataReceived += this.Thread2;
				p.OutputDataReceived += this.Thread3;
				p.Start ();
			}
			
			private void Thread1 (object sender, EventArgs e)
			{
			}
			
			private void Thread2 (object sender, System.Diagnostics.DataReceivedEventArgs e)
			{
			}
			
			private void Thread3 (object sender, System.Diagnostics.DataReceivedEventArgs e)
			{
			}
		}
		
		// Process.Exited uses the type's thread if SynchronizingObject is set to this and
		// ErrorDataReceived is not affected.
		[ThreadModel (ThreadModel.Serializable)]
		public sealed class Bad11 : System.ComponentModel.ISynchronizeInvoke {
			public void Spawn ()
			{
				var p = new System.Diagnostics.Process ();
				p.SynchronizingObject = this;
				p.Exited += this.Thread1;
				p.ErrorDataReceived += this.Thread2;
				p.Start ();
			}
			
			public bool InvokeRequired {
				get { return true; }
			}
			
			public IAsyncResult BeginInvoke (Delegate method, object [] args)
			{
				return null;
			}
			
			public object EndInvoke (IAsyncResult result)
			{
				return null;
			}
			
			public object Invoke (Delegate method, object [] args)
			{
				return null;
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Thread1 (object sender, EventArgs e)
			{
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Thread2 (object sender, EventArgs e)
			{
			}
		}
		
		// Methods passed to the thread pool are entry points.
		public sealed class Bad12 {
			public void Spawn ()
			{
				ThreadPool.QueueUserWorkItem (this.Thread1);
				ThreadPool.RegisterWaitForSingleObject (null, this.Thread2, null, TimeSpan.Zero, true);
			}
			
			private void Thread1 (object data)
			{
			}
			
			private void Thread2 (object data, bool timedOut)
			{
			}
		}
		
		// Methods passed to the thread timer are entry points.
		public sealed class Bad13 {
			public void Spawn ()
			{
				Timer timer = new Timer (this.Thread1);
				timer.Dispose ();
			}
			
			private void Thread1 (object data)
			{
			}
		}
		
		// Async methods are thread entry points.
		public sealed class Bad14 {
			public void Read (System.IO.Stream stream)
			{
				stream.BeginRead (buffer, 0, buffer.Length, this.DoRead, null);
			}
			
			private void DoRead (IAsyncResult result)
			{
			}
			
			private byte [] buffer = new byte [256];
		}
		
		// EventLog.EntryWritten is threaded.
		public sealed class Bad15 {
			public void Spawn1 ()
			{
				var x = new System.Diagnostics.EventLog ();
				x.EntryWritten += this.Thread1;
			}
			
			private void Thread1 (object sender, System.Diagnostics.EntryWrittenEventArgs e)
			{
			}
		}
		
		// Finalizers must be threaded.
		public sealed class Bad16 {
			~Bad16 ()
			{
			}
		}
		
		// Another anonymous method case.
		internal sealed class Bad17 {
			public void Spawn (object data)
			{
				Process (() => Stuff (data));
			}
			
			[ThreadModel (ThreadModel.SingleThread)]
			public delegate void Callback ();
			
			[ThreadModel (ThreadModel.SingleThread)]
			private void Process (Callback callback)
			{
				callback ();
			}
			
			// This is called within a anonymous method bound to a threaded
			// delegate so it must be threaded.
			private void Stuff (object data)
			{
			}
		}
		
		// BackgroundWorker events are threaded.
		public sealed class Bad18 {
			public void Spawn ()
			{
				var worker = new System.ComponentModel.BackgroundWorker ();
				worker.DoWork += this.Thread1;
			}
			
			private void Thread1 (object sender, System.ComponentModel.DoWorkEventArgs e)
			{
			}
		}
		#endregion
		
		public DecorateThreadsTest ()
		{
			rule = new Gendarme.Rules.Concurrency.DecorateThreadsRule ();
			runner = new TestRunner (rule);
		}
		
		[Test]
		public void Cases ()
		{
			AssertFailureCount<Good1> (0);
			AssertFailureCount<Good2> (0);
			AssertFailureCount<Good3> (0);
			AssertFailureCount<Good4> (0);
			AssertFailureCount<Good5> (0);
			AssertFailureCount<Good6> (0);
			AssertFailureCount<Good7> (0);
			AssertFailureCount<Good8> (0);
			AssertFailureCount<Good9> (0);
			AssertFailureCount<Good10> (0);
			AssertFailureCount<Good11> (0);
			AssertFailureCount<Good12> (0);
			AssertFailureCount<Good13> (0);
			AssertFailureCount<Good14> (0);
			
			AssertFailureCount<Bad1> (2);
			AssertFailureCount<Bad2> (5);
			AssertFailureCount<Bad3> (4);
			AssertFailureCount<Bad4> (3);
			AssertFailureCount<Bad5> (2);
			AssertFailureCount<Bad6> (1);
			AssertFailureCount<Bad7> (3);
			AssertFailureCount<Bad8> (1);
			AssertFailureCount<Bad9> (3);
			AssertFailureCount<Bad10> (3);
			AssertFailureCount<Bad11> (1);
			AssertFailureCount<Bad12> (2);
			AssertFailureCount<Bad13> (1);
			AssertFailureCount<Bad14> (1);
			AssertFailureCount<Bad15> (1);
			AssertFailureCount<Bad16> (1);
			AssertFailureCount<Bad17> (1);
			AssertFailureCount<Bad18> (1);
		}
		
		#region Private Methods
		// Unfortunately the standard test fixtures don't run the tests in anything like their
		// normal environment  (in particular the events aren't fired, the runner isn't torn
		// down, and rules aren't (re)initialized). This hoses our test so we have to hand roll
		// an alternative.
		private void PreCheck (IMetadataTokenProvider obj)
		{
			runner.Reset ();
			rule.Initialize (runner);
			AssemblyDefinition assembly = obj.GetAssembly ();
			if (!runner.Assemblies.Contains (assembly)) {
				runner.Assemblies.Clear ();
				runner.Assemblies.Add (assembly);
				runner.Engines.Build (runner.Assemblies);
			}
		}
		
		private void PostCheck (IRule rule)
		{
			runner.TearDown (rule);
		}
		
		private void AssertFailureCount<T> (int expectedCount)
		{
			TypeDefinition type = DefinitionLoader.GetTypeDefinition<T> ();
			PreCheck (type);
			
//			runner.OnType (type);
			foreach (MethodDefinition method in type.Methods) {
				runner.OnMethod (method);
			}
			
			PostCheck (rule);
			
			if (expectedCount != rule.DefectCount)
				Assert.Fail ("{0} failed: should have {1} defects but got {2}.",
					typeof (T).Name, expectedCount, rule.DefectCount);
		}
		#endregion
		
		#region Fields
		private Gendarme.Rules.Concurrency.DecorateThreadsRule rule;
		private TestRunner runner;
		#endregion
	}
}
