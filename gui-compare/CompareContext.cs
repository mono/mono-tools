
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {
	
	// A delegate used to load a CompAssembly
	public delegate CompAssembly LoadCompAssembly ();
	
	public class CompareContext
	{
		LoadCompAssembly reference_loader, target_loader;
		
		public CompareContext (LoadCompAssembly reference, LoadCompAssembly target)
		{
			reference_loader = reference;
			target_loader = target;
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
			ProgressOnGuiThread (Double.NaN, "Loading reference...");

			try {
				reference = reference_loader ();
			}
			catch (Exception e) {
				ErrorOnGuiThread (e.ToString());
				return;
			}

			ProgressOnGuiThread (Double.NaN, "Loading target...");

			try {
				target = target_loader ();
			}
			catch (Exception e) {
				ErrorOnGuiThread (e.ToString());
				return;
			}

			ProgressOnGuiThread (0.0, "Comparing...");

			comparison = target.GetComparisonNode ();

			CompareTypeLists (comparison, reference.GetNamespaces(), target.GetNamespaces());

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

		void CompareNestedTypes (ComparisonNode parent, ICompTypeContainer master_container, ICompTypeContainer assembly_container)
		{
			CompareTypeLists (parent,
					  master_container.GetNestedInterfaces(), assembly_container.GetNestedInterfaces());
			CompareTypeLists (parent,
					  master_container.GetNestedClasses(), assembly_container.GetNestedClasses());
			CompareTypeLists (parent,
					  master_container.GetNestedStructs(), assembly_container.GetNestedStructs());
			CompareTypeLists (parent,
					  master_container.GetNestedEnums(), assembly_container.GetNestedEnums());
			CompareTypeLists (parent,
					  master_container.GetNestedDelegates(), assembly_container.GetNestedDelegates());
		}

		void CompareTypeLists (ComparisonNode parent,
				       List<CompNamed> master_list,
				       List<CompNamed> assembly_list)
		{
			int m = 0, a = 0;

			master_list.Sort (CompNamed.Compare);
			assembly_list.Sort (CompNamed.Compare);

			while (m < master_list.Count || a < assembly_list.Count) {
				if (m == master_list.Count) {
					AddExtra (parent, assembly_list[a]);
					a++;
					continue;
				}
				else if (a == assembly_list.Count) {
					AddMissing (parent, master_list[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_list[m].Name, assembly_list[a].Name);

				if (c == 0) {
					ProgressOnGuiThread (0.0, String.Format ("Comparing {0} {0}", master_list[m].Type, master_list[m].Name));

					/* the names match, further investigation is required */
//  					Console.WriteLine ("{0} {1} is in both, doing more comparisons", master_list[m].Type, master_list[m].Name);
					ComparisonNode comparison = master_list[m].GetComparisonNode();
					parent.AddChild (comparison);

					// compare nested types
					if (master_list[m] is ICompTypeContainer && assembly_list[a] is ICompTypeContainer) {
						CompareNestedTypes (comparison,
								    (ICompTypeContainer)master_list[m],
								    (ICompTypeContainer)assembly_list[a]);
					}
					if (master_list[m] is ICompMemberContainer && assembly_list[a] is ICompMemberContainer) {
						CompareMembers (comparison,
								(ICompMemberContainer)master_list[m],
								(ICompMemberContainer)assembly_list[a]);
					}

					// XXX compare members
					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissing (parent, master_list[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtra (parent, assembly_list[a]);
					a++;
				}
			}
		}

		void CompareAttributes (ComparisonNode parent,
		                        ICompAttributeContainer master_container, ICompAttributeContainer assembly_container)
		{
			int m = 0, a = 0;
			
			List<CompNamed> master_attrs = master_container.GetAttributes ();
			List<CompNamed> assembly_attrs = assembly_container.GetAttributes ();
			
			master_attrs.Sort (CompNamed.Compare);
			assembly_attrs.Sort (CompNamed.Compare);
			
			while (m < master_attrs.Count || a < assembly_attrs.Count) {
				if (m == master_attrs.Count) {
					AddExtra (parent, assembly_attrs[a]);
					a++;
					continue;
				}
				else if (a == assembly_attrs.Count) {
					AddMissing (parent, master_attrs[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_attrs[m].Name, assembly_attrs[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
// 					Console.WriteLine ("method {0} is in both, doing more comparisons", master_list[m].Name);
					ComparisonNode comparison = master_attrs[m].GetComparisonNode();
					parent.AddChild (comparison);

#if notyet
					if (master_list[m] is ICompAttributeContainner && assembly_list[a] is ICompAttributeContainer)
						CompareAttributes (master_list[m], assembly_list[a]);
					
					if (master_list[m] is ICompMemberContainer && assembly_list[a] is ICompMemberContainer) {
						CompareMembers (comparison,
								(ICompMemberContainer)master_list[m],
								(ICompMemberContainer)assembly_list[a]);
					}
#endif
					//CompareParameters (comparison, master_list[m], assembly_namespace [assembly_list[a]]);
					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissing (parent, master_attrs[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtra (parent, assembly_attrs[a]);
					a++;
				}
			}
		}
		
		void CompareMembers (ComparisonNode parent,
				     ICompMemberContainer master_container, ICompMemberContainer assembly_container)
		{
			CompareMemberLists (parent,
					    master_container.GetInterfaces(), assembly_container.GetInterfaces());
			CompareMemberLists (parent,
					    master_container.GetMethods(), assembly_container.GetMethods());
			CompareMemberLists (parent,
					    master_container.GetProperties(), assembly_container.GetProperties());
			CompareMemberLists (parent,
					    master_container.GetFields(), assembly_container.GetFields());
		}

		void CompareMemberLists (ComparisonNode parent,
					 List<CompNamed> master_list,
					 List<CompNamed> assembly_list)
		{
			int m = 0, a = 0;

			master_list.Sort (CompNamed.Compare);
			assembly_list.Sort (CompNamed.Compare);

			while (m < master_list.Count || a < assembly_list.Count) {
				if (m == master_list.Count) {
					AddExtra (parent, assembly_list[a]);
					a++;
					continue;
				}
				else if (a == assembly_list.Count) {
					AddMissing (parent, master_list[m]);
					m++;
					continue;
				}

				int c = String.Compare (master_list[m].Name, assembly_list[a].Name);

				if (c == 0) {
					/* the names match, further investigation is required */
// 					Console.WriteLine ("method {0} is in both, doing more comparisons", master_list[m].Name);
					ComparisonNode comparison = master_list[m].GetComparisonNode();
					parent.AddChild (comparison);

					if (master_list[m] is ICompAttributeContainer && assembly_list[a] is ICompAttributeContainer) {
						Console.WriteLine ("Comparing attributes for {0}", master_list[m].Name);
						CompareAttributes (comparison,
						                   (ICompAttributeContainer)master_list[m],
						                   (ICompAttributeContainer)assembly_list[a]);
					}
					
					if (master_list[m] is ICompMemberContainer && assembly_list[a] is ICompMemberContainer) {
						CompareMembers (comparison,
								(ICompMemberContainer)master_list[m],
								(ICompMemberContainer)assembly_list[a]);
					}

					//CompareParameters (comparison, master_list[m], assembly_namespace [assembly_list[a]]);
					m++;
					a++;
				}
				else if (c < 0) {
					/* master name is before assembly name, master name is missing from assembly */
					AddMissing (parent, master_list[m]);
					m++;
				}
				else {
					/* master name is after assembly name, assembly name is extra */
					AddExtra (parent, assembly_list[a]);
					a++;
				}
			}
		}

		void AddExtra (ComparisonNode parent, CompNamed item)
		{
			ComparisonNode node = item.GetComparisonNode ();
			parent.AddChild (node);
			node.status = ComparisonStatus.Extra;

			if (item is ICompTypeContainer) {
				ICompTypeContainer c = (ICompTypeContainer)item;
				foreach (CompNamed ifc in c.GetNestedInterfaces ())
					AddExtra (node, ifc);
				foreach (CompNamed cls in c.GetNestedClasses())
					AddExtra (node, cls);
				foreach (CompNamed cls in c.GetNestedStructs())
					AddExtra (node, cls);
				foreach (CompNamed en in c.GetNestedEnums())
					AddExtra (node, en);
			}
		}

		void AddMissing (ComparisonNode parent, CompNamed item)
		{
			ComparisonNode node = item.GetComparisonNode ();
			parent.AddChild (node);
			node.status = ComparisonStatus.Missing;

			if (item is ICompTypeContainer) {
				ICompTypeContainer c = (ICompTypeContainer)item;

				foreach (CompNamed ifc in c.GetNestedInterfaces ())
					AddMissing (node, ifc);
				foreach (CompNamed cls in c.GetNestedClasses())
					AddMissing (node, cls);
				foreach (CompNamed cls in c.GetNestedStructs())
					AddMissing (node, cls);
				foreach (CompNamed en in c.GetNestedEnums())
					AddMissing (node, en);
			}
		}

		// This is the reference assembly that we will be comparing to.
		CompAssembly reference;
		
		// This is the new API.
		CompAssembly target;

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
		ComparisonNode comparison;
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
