
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {
	public class CompareContext
	{
		public CompareContext (string masterinfoPath, string assemblyPath)
		{
			this.masterinfoPath = masterinfoPath;
			this.assemblyPath = assemblyPath;
		}

		public ComparisonNode Comparison {
			get { return comparison; }
		}

		public void Compare ()
		{
			if (t != null)
				throw new InvalidOperationException ("compare already running");

			t = new Thread (CompareThread);
			t.Start ();
		}

		public void StopCompare ()
		{
		}

		void CompareThread ()
		{
			ProgressOnGuiThread (Double.NaN, "Loading masterinfo...");

			try {
				LoadMasterinfo ();
			}
			catch (Exception e) {
				ErrorOnGuiThread (e.ToString());
				return;
			}

			ProgressOnGuiThread (Double.NaN, "Loading assembly...");

			try {
				LoadAssembly ();
			}
			catch (Exception e) {
				ErrorOnGuiThread (e.ToString());
				return;
			}

			ProgressOnGuiThread (0.0, "Comparing...");

			comparison = new AssemblyComparison (Path.GetFileName (assemblyPath));

			CompareNamespaces (comparison);

			FinishedOnGuiThread ();

			DumpComparison (comparison, 0);
		}

		char StatusToChar (ComparisonStatus r)
		{
			switch (r) {
			case ComparisonStatus.Missing: return '-';
			case ComparisonStatus.Extra:   return '+';
			case ComparisonStatus.Todo:    return 'o';
			case ComparisonStatus.Error:   return '!';
			default:
			case ComparisonStatus.None:    return ' ';
			}
		}


		void DumpComparison (ComparisonNode c, int indent)
		{
			for (int i = 0; i < indent; i ++)
				Console.Write (" ");
			Console.WriteLine ("{0} {1} {2}", StatusToChar (c.status), c.type, c.name);
			foreach (ComparisonNode child in c.children) {
				DumpComparison (child, indent + 2);
			}
		}

		void CompareNamespaces (ComparisonNode parent)
		{
			List<CompNamespace> assembly_namespaces = assembly.GetNamespaces();
			List<CompNamespace> master_namespaces = masterinfo.GetNamespaces();

			int m = 0, a = 0;

			while (m < master_namespaces.Count && a < assembly_namespaces.Count) {
				if (m == master_namespaces.Count) {
					AddExtraNamespace (parent, assembly_namespaces[a]);
					a++;
					continue;
				}
				else if (a == assembly_namespaces.Count) {
					AddMissingNamespace (parent, master_namespaces[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_namespaces[m].Name, assembly_namespaces[a].Name);

				if (c == 0) {
					ProgressOnGuiThread (0.0, String.Format ("Comparing namespace {0}", master_namespaces[m].Name));

					/* the names match, further investigation is required */
					Console.WriteLine ("namespace {0} is in both, doing more comparisons", master_namespaces[m].Name);
					NamespaceComparison comparison = new NamespaceComparison (master_namespaces[m].Name);
					parent.AddChild (comparison);
					CompareTypes (comparison, master_namespaces[m], assembly_namespaces[a]);
					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissingNamespace (parent, master_namespaces[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtraNamespace (parent, assembly_namespaces[a]);
					a++;
				}
			}
		}

		void CompareTypes (NamespaceComparison parent,
				   CompNamespace master_namespace, CompNamespace assembly_namespace)
		{
			List<CompClass> assembly_types = assembly_namespace.GetClasses();
			List<CompClass> master_types = master_namespace.GetClasses();

			int m = 0, a = 0;
			while (m < master_types.Count && a < assembly_types.Count) {
				if (m == master_types.Count) {
					AddExtraClass (parent, assembly_types[a]);
					a++;
					continue;
				}
				else if (a == assembly_types.Count) {
					AddMissingClass (parent, master_types[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_types[m].Name, assembly_types[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
					Console.WriteLine ("type {0} is in both, doing more comparisons", master_types[m].Name);
					ClassComparison comparison = new ClassComparison (master_types[m].Name);
					parent.AddChild (comparison);
					CompareMethods (comparison, master_types[m], assembly_types[a]);
					//CompareMembers (comparison, master_types[m], assembly_namespace [assembly_types[a]]);
					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissingClass (parent, master_types[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtraClass (parent, assembly_types[a]);
					a++;
				}
			}
		}

		void CompareMethods (ComparisonNode parent,
				     CompClass master_class, CompClass assembly_class)
		{
#if false
			MethodDefinition[] assembly_methods = new MethodDefinition [assembly_class.Methods.Count];
			((ICollection)assembly_class.Methods).CopyTo (assembly_methods, 0);
			Array.Sort (assembly_methods, new MethodDefinitionComparer());

			XMLMethods [] master_methods = new XMLMethods [master_class.methods.Length];
			master_class.methods.CopyTo (master_methods, 0);
			Array.Sort (master_methods, new XMLMethodComparer());

			int m = 0, a = 0;
			while (m < master_methods.Length && a < assembly_methods.Length) {
				if (m == master_methods.Length) {
					AddExtraMethod (parent, assembly_methods[a]);
					a++;
					continue;
				}
				else if (a == assembly_methods.Length) {
					AddMissingMethod (parent, master_methods[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_methods[m].name, assembly_methods[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
					Console.WriteLine ("method {0} is in both, doing more comparisons", master_methods[m].name);
					MethodComparison comparison = new MethodComparison (master_methods[m].name);
					parent.AddChild (comparison);

					// XXX compare attributes

					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissingMethod (parent, master_methods[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtraMethod (parent, assembly_methods[a]);
					a++;
				}
			}
#endif
		}

		void AddExtraNamespace (ComparisonNode parent, CompNamespace ns)
		{
			ComparisonNode namespace_node = new NamespaceComparison (ns.Name);
			parent.AddChild (namespace_node);
			namespace_node.status = ComparisonStatus.Extra;

			Console.WriteLine ("extra namespace: {0}", ns.Name);
			List<CompClass> classes = ns.GetClasses();
			foreach (CompClass cls in classes)
				AddExtraClass (namespace_node, cls);
		}

		void AddMissingNamespace (ComparisonNode parent, CompNamespace ns)
		{
			ComparisonNode namespace_node = new NamespaceComparison (ns.Name);
			parent.AddChild (namespace_node);
			namespace_node.status = ComparisonStatus.Missing;

			Console.WriteLine ("missing namespace: {0}", ns.Name);

			List<CompClass> classes = ns.GetClasses();
			foreach (CompClass cls in classes)
				AddMissingClass (namespace_node, cls);
		}

		void AddExtraClass (ComparisonNode parent, CompClass cls)
		{
			ClassComparison comparison = new ClassComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Extra;
		}

		void AddMissingClass (ComparisonNode parent, CompClass cls)
		{
			ClassComparison comparison = new ClassComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Missing;
		}

		void AddExtraMethod (ComparisonNode parent, MethodDefinition assembly_method)
		{
// 			Console.WriteLine ("extra method: {0}", assembly_method.Name);
		}

		void AddMissingMethod (ComparisonNode parent, XMLMethods master_method)
		{
// 			Console.WriteLine ("missing method: {0}", master_method.name);
		}

		void LoadMasterinfo ()
		{
			masterinfo = new MasterAssembly (masterinfoPath);
		}

		void LoadAssembly ()
		{
			assembly = new CecilAssembly (assemblyPath);
		}

		CompAssembly masterinfo;
		CompAssembly assembly;

		void ProgressOnGuiThread (double progress, string message)
		{
			Application.Invoke (delegate (object sender, EventArgs e) {
				if (ProgressChanged != null)
					ProgressChanged (this, new CompareProgressChangedEventArgs (message, progress));
			});
		}

		void ErrorOnGuiThread (string message)
		{
			Application.Invoke (delegate (object sender, EventArgs e) {
				if (Error != null)
					Error (this, new CompareErrorEventArgs (message));
			});
		}

		void FinishedOnGuiThread ()
		{
			Application.Invoke (delegate (object sender, EventArgs e) {
				if (Finished != null)
					Finished (this, EventArgs.Empty);
			});
		}

		public event CompareProgressChangedEventHandler ProgressChanged;
		public event CompareErrorEventHandler Error;
		public event EventHandler Finished;

		string masterinfoPath;
		string assemblyPath;
		AssemblyComparison comparison;
		Thread t;
	}






	public delegate void CompareProgressChangedEventHandler (object sender, CompareProgressChangedEventArgs args);
	public delegate void CompareErrorEventHandler (object sender, CompareErrorEventArgs args);

	public class CompareProgressChangedEventArgs : EventArgs
	{
		public CompareProgressChangedEventArgs (string message, double progress)
		{
			this.message = message;
			this.progress = progress;
		}

		public string Message {
			get { return message; }
		}

		public double Progress {
			get { return progress; }
		}

		string message;
		double progress;
	}

	public class CompareErrorEventArgs : EventArgs
	{
		public CompareErrorEventArgs (string message)
		{
			this.message = message;
		}

		public string Message {
			get { return message; }
		}

		string message;
	}
}
