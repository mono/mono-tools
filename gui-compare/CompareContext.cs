
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

			//			DumpComparison (comparison, 0);
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

			while (m < master_namespaces.Count || a < assembly_namespaces.Count) {
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
// 					Console.WriteLine ("namespace {0} is in both, doing more comparisons", master_namespaces[m].Name);
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

		void CompareClassLists (ComparisonNode parent,
					List<CompClass> master_types, List<CompClass> assembly_types)
		{
			int m = 0, a = 0;
			while (m < master_types.Count || a < assembly_types.Count) {
				if (master_types == null || m == master_types.Count) {
					AddExtraClass (parent, assembly_types[a]);
					a++;
					continue;
				}
				else if (assembly_types == null || a == assembly_types.Count) {
					AddMissingClass (parent, master_types[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_types[m].Name, assembly_types[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
					ClassComparison comparison = new ClassComparison (master_types[m].Name);
					parent.AddChild (comparison);
					CompareInterfaces (comparison, master_types[m], assembly_types[a]);
					CompareMethods (comparison, master_types[m], assembly_types[a]);
					// XXX more comparison stuff here
					CompareClassLists (comparison,
							   master_types[m].GetNestedClasses(),
							   assembly_types[a].GetNestedClasses());
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

		void CompareTypes (NamespaceComparison parent,
				   CompNamespace master_namespace, CompNamespace assembly_namespace)
		{
			CompareClassLists (parent,
					   master_namespace.GetClasses(), assembly_namespace.GetClasses());
			CompareStructs (parent,
					master_namespace, assembly_namespace);
		}

		void CompareStructs (ComparisonNode parent,
				     CompNamespace master_namespace, CompNamespace assembly_namespace)
		{
			List<CompClass> assembly_structs = assembly_namespace.GetStructs();
			List<CompClass> master_structs = master_namespace.GetStructs();

			int m = 0, a = 0;
			while (m < master_structs.Count || a < assembly_structs.Count) {
				if (m == master_structs.Count) {
					Console.WriteLine ("{0}", assembly_namespace.Name);
					AddExtraStruct (parent, assembly_structs[a]);
					a++;
					continue;
				}
				else if (a == assembly_structs.Count) {
					Console.WriteLine ("{0}", master_namespace.Name);
					AddMissingStruct (parent, master_structs[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_structs[m].Name, assembly_structs[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
 					Console.WriteLine ("struct {0} is in both, doing more comparisons", master_structs[m].Name);
					StructComparison comparison = new StructComparison (master_structs[m].Name);
					parent.AddChild (comparison);
					m++;
					a++;
				}
				else if (c < 0) {
					Console.WriteLine ("{0}", master_namespace.Name);
					/* master name is before assembly name, master name is missing from assembly */
					AddMissingStruct (parent, master_structs[m]);
					m++;
				}
				else {
					Console.WriteLine ("{0}", assembly_namespace.Name);
					/* master name is after assembly name, assembly name is extra */
					AddExtraStruct (parent, assembly_structs[a]);
					a++;
				}
			}
		}

		void CompareInterfaces (ComparisonNode parent,
					CompClass master_class, CompClass assembly_class)
		{
			List<CompInterface> assembly_interfaces = assembly_class.GetInterfaces();
			List<CompInterface> master_interfaces = master_class.GetInterfaces();

			int m = 0, a = 0;
			while (m < master_interfaces.Count || a < assembly_interfaces.Count) {
				if (m == master_interfaces.Count) {
					Console.WriteLine ("{0}", assembly_class.Name);
					AddExtraInterface (parent, assembly_interfaces[a]);
					a++;
					continue;
				}
				else if (a == assembly_interfaces.Count) {
					Console.WriteLine ("{0}", master_class.Name);
					AddMissingInterface (parent, master_interfaces[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_interfaces[m].Name, assembly_interfaces[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
 					Console.WriteLine ("interface {0} is in both, doing more comparisons", master_interfaces[m].Name);
					InterfaceComparison comparison = new InterfaceComparison (master_interfaces[m].Name);
					parent.AddChild (comparison);
					m++;
					a++;
				}
				else if (c < 0) {
					Console.WriteLine ("{0}", master_class.Name);
					/* master name is before assembly name, master name is missing from assembly */
					AddMissingInterface (parent, master_interfaces[m]);
					m++;
				}
				else {
					Console.WriteLine ("{0}", assembly_class.Name);
					/* master name is after assembly name, assembly name is extra */
					AddExtraInterface (parent, assembly_interfaces[a]);
					a++;
				}
			}
		}

		void CompareMethods (ComparisonNode parent,
				     CompClass master_class, CompClass assembly_class)
		{
			List<CompMethod> assembly_methods = assembly_class.GetMethods();
			List<CompMethod> master_methods = master_class.GetMethods();

			int m = 0, a = 0;
			while (m < master_methods.Count || a < assembly_methods.Count) {
				if (m == master_methods.Count) {
					AddExtraMethod (parent, assembly_methods[a]);
					a++;
					continue;
				}
				else if (a == assembly_methods.Count) {
					AddMissingMethod (parent, master_methods[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_methods[m].Name, assembly_methods[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
// 					Console.WriteLine ("method {0} is in both, doing more comparisons", master_methods[m].Name);
					MethodComparison comparison = new MethodComparison (master_methods[m].Name);
					parent.AddChild (comparison);
					//CompareParameters (comparison, master_methods[m], assembly_namespace [assembly_methods[a]]);
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
		}

		void AddExtraNamespace (ComparisonNode parent, CompNamespace ns)
		{
			ComparisonNode namespace_node = new NamespaceComparison (ns.Name);
			parent.AddChild (namespace_node);
			namespace_node.status = ComparisonStatus.Extra;

			List<CompClass> classes = ns.GetClasses();
			foreach (CompClass cls in classes)
				AddExtraClass (namespace_node, cls);
		}

		void AddMissingNamespace (ComparisonNode parent, CompNamespace ns)
		{
			ComparisonNode namespace_node = new NamespaceComparison (ns.Name);
			parent.AddChild (namespace_node);
			namespace_node.status = ComparisonStatus.Missing;

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

		void AddExtraStruct (ComparisonNode parent, CompClass cls)
		{
			StructComparison comparison = new StructComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Extra;
		}

		void AddMissingStruct (ComparisonNode parent, CompClass cls)
		{
			StructComparison comparison = new StructComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Missing;
		}

		void AddExtraInterface (ComparisonNode parent, CompInterface cls)
		{
			Console.WriteLine ("extra interface {0}", cls.Name);
			InterfaceComparison comparison = new InterfaceComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Extra;
		}

		void AddMissingInterface (ComparisonNode parent, CompInterface cls)
		{
			Console.WriteLine ("missing interface {0}", cls.Name);
			InterfaceComparison comparison = new InterfaceComparison (cls.Name);
			parent.AddChild (comparison);
			comparison.status = ComparisonStatus.Missing;
		}

		void AddExtraMethod (ComparisonNode parent, CompMethod extra)
		{
			ComparisonNode method_node = new MethodComparison (extra.Name);
			parent.AddChild (method_node);
			method_node.status = ComparisonStatus.Extra;
		}

		void AddMissingMethod (ComparisonNode parent, CompMethod missing)
		{
			ComparisonNode method_node = new MethodComparison (missing.Name);
			parent.AddChild (method_node);
			method_node.status = ComparisonStatus.Missing;
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
