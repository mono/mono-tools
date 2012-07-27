//
// CompareContext.cs
//
// (C) 2007 - 2008 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace GuiCompare {

	public class CompareContext
	{
		Func<CompAssembly> reference_loader, target_loader;
		
		public CompareContext (Func<CompAssembly> reference, Func<CompAssembly> target)
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

		bool TryLoad (ref CompAssembly assembly, Func<CompAssembly> loader)
		{
			try {
				assembly = loader ();
				return true;
			} catch (Exception e) {
				OnError (e.ToString ());
				return false;
			}
		}

		void CompareThread ()
		{
			try {
				ProgressChange (Double.NaN, "Loading reference...");

				if (!TryLoad (ref reference, reference_loader))
					return;

				ProgressChange (Double.NaN, "Loading target...");

				if (!TryLoad (ref target, target_loader))
					return;

				ProgressChange (0.0, "Comparing...");

				comparison = target.GetComparisonNode ();

				List<CompNamed> ref_namespaces = reference.GetNamespaces();
				
				total_comparisons = CountComparisons (ref_namespaces);
				comparisons_performed = 0;
				
				CompareTypeLists (comparison, reference.GetNamespaces(), target.GetNamespaces());

				CompareAttributes (comparison, reference, target);
			} catch (Exception exc) {
				OnError (exc.Message);
			} finally {
				Finish ();
			}
		}

		int total_comparisons;
		int comparisons_performed;

		int CountComparisons (List<CompNamed> list)
		{
			int rv = 0;
			foreach (CompNamed l in list) {
				rv += CountComparisons (l);
			}
			return rv;
		}
		
		int CountComparisons (CompNamed named)
		{
			int rv = 1;
			if (named is ICompMemberContainer) {
				ICompMemberContainer container = (ICompMemberContainer)named;
				rv += CountComparisons (container.GetInterfaces());
				rv += CountComparisons (container.GetConstructors());
				rv += CountComparisons (container.GetMethods());
				rv += CountComparisons (container.GetProperties());
				rv += CountComparisons (container.GetFields());
			}
			if (named is ICompTypeContainer) {
				ICompTypeContainer container = (ICompTypeContainer)named;
				rv += CountComparisons (container.GetNestedInterfaces());
				rv += CountComparisons (container.GetNestedClasses());
				rv += CountComparisons (container.GetNestedStructs());
				rv += CountComparisons (container.GetNestedEnums());
				rv += CountComparisons (container.GetNestedDelegates());
			}
			if (named is ICompAttributeContainer) {
				rv += CountComparisons (((ICompAttributeContainer)named).GetAttributes());
			}
			return rv;
		}
		
		void CompareNestedTypes (ComparisonNode parent, ICompTypeContainer reference_container, ICompTypeContainer target_container)
		{
			CompareTypeLists (parent,
			                  reference_container.GetNestedInterfaces(), target_container.GetNestedInterfaces());
			CompareTypeLists (parent,
			                  reference_container.GetNestedClasses(), target_container.GetNestedClasses());
			CompareTypeLists (parent,
			                  reference_container.GetNestedStructs(), target_container.GetNestedStructs());
			CompareTypeLists (parent,
			                  reference_container.GetNestedEnums(), target_container.GetNestedEnums());
			CompareTypeLists (parent,
			                  reference_container.GetNestedDelegates(), target_container.GetNestedDelegates());
		}

		void CompareBaseTypes (ComparisonNode parent, ICompHasBaseType reference_type, ICompHasBaseType target_type)
		{
			if (reference_type.GetBaseType() != target_type.GetBaseType()) {
				parent.AddError (String.Format ("Expected base class of {0} but found {1}",
								reference_type.GetBaseType(),
								target_type.GetBaseType()));
			}
			
			if (reference_type.IsAbstract != target_type.IsAbstract) {
				string ref_mod = (reference_type.IsAbstract && reference_type.IsSealed) ? "static" : "abstract";
				string tar_mod = (target_type.IsAbstract && target_type.IsSealed) ? "static" : "abstract";

				parent.AddError (String.Format ("reference is {0} {2}, is {1} {3}",
								reference_type.IsAbstract ? null : "not", target_type.IsAbstract ? null : "not",
								ref_mod, tar_mod));
			} else if (reference_type.IsSealed != target_type.IsSealed) {
				string ref_mod = (reference_type.IsAbstract && reference_type.IsSealed) ? "static" : "sealed";
				string tar_mod = (target_type.IsAbstract && target_type.IsSealed) ? "static" : "sealed";
				
				parent.AddError (String.Format ("reference is {0} {2}, target is {1} {3}",
								reference_type.IsSealed ? null : "not", target_type.IsSealed ? null : "not",
								ref_mod, tar_mod));
			}
		}
		
		void CompareParameters (ComparisonNode parent, ICompParameters reference, ICompParameters target)
		{
			var r = reference.GetParameters ();
			var t = target.GetParameters ();
			
			if (r.Count != t.Count) {
				throw new NotImplementedException (string.Format ("Should never happen with valid data ({0} != {1})", r.Count, t.Count));
			}
			
			for (int i = 0; i < r.Count; ++i) {
				var r_i = r [i];
				var t_i = t [i];
				
				if (r_i.TypeReference != t_i.TypeReference) {
					parent.AddError (string.Format ("Parameter `{0}' type mismatch", t_i.Name));
				}
				
				if (r_i.Name != t_i.Name) {
					parent.AddError (string.Format ("Parameter name `{0}' should be `{1}'", t_i.Name, r_i.Name));
				}
				
				if (r_i.IsOptional != t_i.IsOptional) {
					if (r_i.IsOptional)
						parent.AddError (string.Format ("Parameter `{0}' is missing a default value", t_i.Name));
					else
						parent.AddError (string.Format ("Parameter `{0}' should not have a default value", t_i.Name));
				}
				
				CompareAttributes (parent, r_i, t_i);
			}
		}
		
		void CompareTypeParameters (ComparisonNode parent, ICompGenericParameter reference, ICompGenericParameter target)
		{
			var r = reference.GetTypeParameters ();
			var t = target.GetTypeParameters ();
			if (r == null && t == null || (r == null && t != null) || (r != null && t == null))
				return;

			for (int i = 0; i < r.Count; ++i) {
				var r_i = r [i];
				var t_i = t [i];
				
				if (r_i.GenericAttributes != t_i.GenericAttributes) {
					parent.AddError (string.Format ("Expected type parameter {2} with {0} generic attributes but found type parameter {3} with {1} generic attributes",
						CompGenericParameter.GetGenericAttributeDesc (r_i.GenericAttributes),
						CompGenericParameter.GetGenericAttributeDesc (t_i.GenericAttributes),
						r_i.Name,
						t_i.Name));
				}

				// TODO: Compare constraints properly			
				if (r_i.HasConstraints != t_i.HasConstraints) {
					parent.AddError (string.Format ("Type parameter `{0}' constraints mismatch", r_i.Name));
				}

				CompareAttributes (parent, r_i, t_i);
			}
		}

		void CompareTypeLists (ComparisonNode parent,
		                       List<CompNamed> reference_list,
		                       List<CompNamed> target_list)
		{
			int m = 0, a = 0;

			reference_list.Sort (CompNamed.Compare);
			target_list.Sort (CompNamed.Compare);

			while (m < reference_list.Count || a < target_list.Count) {
				if (m == reference_list.Count) {
					AddExtra (parent, target_list[a]);
					a++;
					continue;
				}
				else if (a == target_list.Count) {
					AddMissing (parent, reference_list[m]);
					m++;
					continue;
				}

				int c = String.Compare (reference_list[m].Name, target_list[a].Name);
				comparisons_performed ++;
				
				if (c == 0) {
					ProgressChange ((double)comparisons_performed / total_comparisons * 100.0, String.Format ("Comparing {0} {1}", reference_list[m].Type, reference_list[m].Name));

					/* the names match, further investigation is required */
					ComparisonNode comparison = target_list[a].GetComparisonNode();
					parent.AddChild (comparison);

					// compare base types
					if (reference_list[m] is ICompHasBaseType && target_list[a] is ICompHasBaseType) {
						CompareBaseTypes (comparison,
								  (ICompHasBaseType)reference_list[m],
								  (ICompHasBaseType)target_list[a]);
					}
					
					// compares generic type parameters
					if (reference_list[m] is ICompGenericParameter && target_list[a] is ICompGenericParameter) {
						CompareTypeParameters (comparison,
								(ICompGenericParameter)reference_list[m],
								(ICompGenericParameter)target_list[a]);
					}
					
					// compare nested types
					if (reference_list[m] is ICompTypeContainer && target_list[a] is ICompTypeContainer) {
						CompareNestedTypes (comparison,
						                    (ICompTypeContainer)reference_list[m],
						                    (ICompTypeContainer)target_list[a]);
					}
					if (reference_list[m] is ICompMemberContainer && target_list[a] is ICompMemberContainer) {
						CompareMembers (comparison,
						                (ICompMemberContainer)reference_list[m],
						                (ICompMemberContainer)target_list[a]);
					}
					if (reference_list[m] is ICompAttributeContainer && target_list[a] is ICompAttributeContainer) {
						CompareAttributes (comparison,
								   (ICompAttributeContainer)reference_list[m],
								   (ICompAttributeContainer)target_list[a]);
					}

					m++;
					a++;
				}
				else if (c < 0) {
					/* reference name is before target name, reference name is missing from target */
					AddMissing (parent, reference_list[m]);
					m++;
				}
				else {
					/* reference name is after target name, target name is extra */
					AddExtra (parent, target_list[a]);
					a++;
				}
			}
		}

		void CompareAttributes (ComparisonNode parent,
		                        ICompAttributeContainer reference_container, ICompAttributeContainer target_container)
		{
			int m = 0, a = 0;
			
			List<CompNamed> reference_attrs = reference_container.GetAttributes ();
			List<CompNamed> target_attrs = target_container.GetAttributes ();

			Comparison<CompNamed> comp = (x, y) => {
				var r = CompNamed.Compare (x, y);
				if (r != 0)
					return r;

				var xa = ((CompAttribute)x).Properties.Values.ToList ();
				var ya = ((CompAttribute)y).Properties.Values.ToList ();

				for (int i = 0; i < Math.Min (xa.Count, ya.Count); ++i) {
					r = xa[i].CompareTo (ya[i]);
					if (r != 0)
						return r;
				}

				return 0;
			};

			reference_attrs.Sort (comp);
			target_attrs.Sort (comp);
			
			while (m < reference_attrs.Count || a < target_attrs.Count) {
				if (m == reference_attrs.Count) {
					
					switch (target_attrs[a].Name) {
						case "System.Diagnostics.DebuggerDisplayAttribute":
						case "System.Runtime.CompilerServices.AsyncStateMachineAttribute":
						case "System.Runtime.CompilerServices.IteratorStateMachineAttribute":
						case "System.Diagnostics.DebuggerBrowsableAttribute":
							// Ignore extra attributes in Mono source code
						break;
					default:
						AddExtra (parent, target_attrs[a]);
						break;
					}
					
					a++;
					continue;
				}
				else if (a == target_attrs.Count) {
					AddMissing (parent, reference_attrs[m]);
					m++;
					continue;
				}

				int c = String.Compare (reference_attrs[m].Name, target_attrs[a].Name);
				comparisons_performed ++;

				if (c == 0) {
					/* the names match, further investigation is required */
					ComparisonNode comparison = target_attrs[a].GetComparisonNode();
					parent.AddChild (comparison);
					CompareAttributeArguments (comparison, (CompAttribute)reference_attrs[m], (CompAttribute)target_attrs[a]);
					m++;
					a++;
				}
				else if (c < 0) {
					/* reference name is before target name, reference name is missing from target */
					AddMissing (parent, reference_attrs[m]);
					m++;
				}
				else {
					/* reference name is after target name, target name is extra */
					AddExtra (parent, target_attrs[a]);
					a++;
				}
			}
		}

		void CompareAttributeArguments (ComparisonNode parent, CompAttribute referenceAttribute, CompAttribute actualAttribute)
		{
			// Ignore all parameter differences for some attributes
			switch (referenceAttribute.Name) {
			case "System.Diagnostics.DebuggerDisplayAttribute":
			case "System.Diagnostics.DebuggerTypeProxyAttribute":
			case "System.Runtime.CompilerServices.CompilationRelaxationsAttribute":
			case "System.Reflection.AssemblyFileVersionAttribute":
			case "System.Reflection.AssemblyCompanyAttribute":
			case "System.Reflection.AssemblyCopyrightAttribute":
			case "System.Reflection.AssemblyProductAttribute":
			case "System.Reflection.AssemblyTrademarkAttribute":
			case "System.Reflection.AssemblyInformationalVersionAttribute":
			case "System.Reflection.AssemblyKeyFileAttribute":

			// Don't care about these for now
			case "System.ComponentModel.EditorAttribute":
			case "System.ComponentModel.DesignerAttribute":
				return;
			}

			foreach (var entry in referenceAttribute.Properties) {
				if (!actualAttribute.Properties.ContainsKey (entry.Key)) {

					//
					// Ignore missing value difference for default values
					//
					switch (referenceAttribute.Name) {
					case "System.AttributeUsageAttribute":
						// AllowMultiple defaults to false
						if (entry.Key == "AllowMultiple" && entry.Value == "False")
							continue;
						// Inherited defaults to true
						if (entry.Key == "Inherited" && entry.Value == "True")
							continue;
						break;
					case "System.ObsoleteAttribute":
						if (entry.Key == "IsError" && entry.Value == "False")
							continue;

						if (entry.Key == "Message")
							continue;

						break;
					}

					parent.AddError (String.Format ("Property `{0}' value is not set. Expected value: {1}", entry.Key, entry.Value));
					parent.Status = ComparisonStatus.Error;
					continue;
				}

				var target_value = actualAttribute.Properties[entry.Key];

				switch (referenceAttribute.Name) {
				case "System.Runtime.CompilerServices.TypeForwardedFromAttribute":
					if (entry.Key == "AssemblyFullName")
						target_value = target_value.Replace ("neutral", "Neutral");
					break;
				case "System.Runtime.InteropServices.GuidAttribute":
					if (entry.Key == "Value")
						target_value = target_value.ToUpperInvariant ();
					break;
				case "System.ObsoleteAttribute":
					if (entry.Key == "Message")
						continue;

					break;
				}

				if (target_value != entry.Value) {
					parent.AddError (String.Format ("Expected value `{0}' for attribute property `{1}' but found `{2}'", entry.Value, entry.Key, target_value));
					parent.Status = ComparisonStatus.Error;
				}
			}

			
			if (referenceAttribute.Properties.Count != actualAttribute.Properties.Count) {
				foreach (var entry in actualAttribute.Properties) {
					if (!referenceAttribute.Properties.ContainsKey (entry.Key)) {
						parent.AddError (String.Format ("Property `{0}' should not be set", entry.Key));
						parent.Status = ComparisonStatus.Error;
						break;
					}
				}
			}
			

			return;
		}
		
		void CompareMembers (ComparisonNode parent,
		                     ICompMemberContainer reference_container, ICompMemberContainer target_container)
		{
			bool is_sealed = reference_container.IsSealed;
			
			CompareMemberLists (parent,
			                    reference_container.GetInterfaces(), target_container.GetInterfaces(), is_sealed);
			CompareMemberLists (parent,
			                    reference_container.GetConstructors(), target_container.GetConstructors(), is_sealed);
			CompareMemberLists (parent,
			                    reference_container.GetMethods(), target_container.GetMethods(), is_sealed);
			CompareMemberLists (parent,
			                    reference_container.GetProperties(), target_container.GetProperties(), is_sealed);
			CompareMemberLists (parent,
			                    reference_container.GetFields(), target_container.GetFields(), is_sealed);
			CompareMemberLists (parent,
			                    reference_container.GetEvents(), target_container.GetEvents(), is_sealed);
		}

		void CompareMemberLists (ComparisonNode parent,
		                         List<CompNamed> reference_list,
		                         List<CompNamed> target_list,
		                         bool isSealed)
		{
			int m = 0, a = 0;

			reference_list.Sort (CompNamed.Compare);
			target_list.Sort (CompNamed.Compare);

			while (m < reference_list.Count || a < target_list.Count) {
				if (m == reference_list.Count) {
					AddExtra (parent, target_list[a]);
					a++;
					continue;
				}
				else if (a == target_list.Count) {
					AddMissing (parent, reference_list[m]);
					m++;
					continue;
				}

				int c = CompNamed.Compare (reference_list[m], target_list[a]);
				comparisons_performed ++;

				if (c == 0) {
					/* the names match, further investigation is required */
// 					Console.WriteLine ("method {0} is in both, doing more comparisons", reference_list[m].Name);
					ComparisonNode comparison = target_list[a].GetComparisonNode();
					parent.AddChild (comparison);

					if (reference_list[m] is CompMember && target_list[a] is CompMember) {
						string reference_type = ((CompMember)reference_list[m]).GetMemberType();
						string target_type = ((CompMember)target_list[a]).GetMemberType();
						
						if (reference_type != target_type) {
							comparison.AddError (String.Format ("reference type is <i>{0}</i>, target type is <i>{1}</i>",
							                                    reference_type, target_type));
						}
						
						string reference_access = ((CompMember)reference_list[m]).GetMemberAccess();
						string target_access = ((CompMember)target_list[a]).GetMemberAccess();
						if (reference_access != target_access) {
							// Try to give some hints to the developer, best we can do with
							// strings.
							string extra_msg = "";
							if (reference_access.IndexOf ("Private, Final, Virtual, HideBySig") != -1 &&
							    target_access.IndexOf ("Public, HideBySig") != -1){
								extra_msg = "\n\t\t<b>Hint:</b> reference uses an explicit interface implementation, target doesn't";
							}

							comparison.AddError (String.Format ("reference access is '<i>{0}</i>', target access is '<i>{1}</i>'{2}",
							                                    reference_access, target_access, extra_msg));
							comparison.Status = ComparisonStatus.Error;
						}
					}
					
					var r_method = reference_list[m] as CompMethod;
					if (r_method != null) {
						var t_method = (CompMethod)target_list[a];
						if (t_method.ThrowsNotImplementedException () && !r_method.ThrowsNotImplementedException ()) {
							comparison.ThrowsNIE = true;
						}

						CompareTypeParameters (comparison, r_method, t_method);
						CompareParameters (comparison, r_method, t_method);
					} else if (reference_list[m] is CompProperty) {
						var m1 = ((CompProperty) reference_list[m]).GetMethods ();
						var m2 = ((CompProperty) target_list[a]).GetMethods ();
						if (m1.Count != m2.Count) {
							comparison.AddError (String.Format ("Expected {0} accessors but found {1}", m1.Count, m2.Count));
							comparison.Status = ComparisonStatus.Error;
						} else {
							for (int i = 0; i < m1.Count; ++i) {
								string reference_access = ((CompMember) m1[i]).GetMemberAccess();
								string target_access = ((CompMember) m2[i]).GetMemberAccess();
								if (reference_access != target_access) {
									// Try to give some hints to the developer, best we can do with
									// strings.
									string extra_msg = "";
									if (reference_access.IndexOf ("Private, Final, Virtual, HideBySig") != -1 &&
										target_access.IndexOf ("Public, HideBySig") != -1){
										extra_msg = "\n\t\t<b>Hint:</b> reference uses an explicit interface implementation, target doesn't";
									}

									comparison.AddError (String.Format ("reference access is '<i>{0}</i>', target access is '<i>{1}</i>'{2}",
																		reference_access, target_access, extra_msg));
									comparison.Status = ComparisonStatus.Error;
									break;
								}
							}
							
							if (m1[0].Name[0] == m2[0].Name[0]) {
								CompareAttributes (comparison, (ICompAttributeContainer)m1[0], (ICompAttributeContainer)m2[0]);
								if (m1.Count > 1)
									CompareAttributes (comparison, (ICompAttributeContainer)m1[1], (ICompAttributeContainer)m2[1]);
							} else {
								CompareAttributes (comparison, (ICompAttributeContainer)m1[0], (ICompAttributeContainer)m2[1]);
								if (m1.Count > 1)
									CompareAttributes (comparison, (ICompAttributeContainer)m1[1], (ICompAttributeContainer)m2[0]);
							}
						}

						// Compare indexer parameters
						if (m1.Count == m2.Count)
							CompareParameters (comparison, (ICompParameters) m1[0], (ICompParameters) m2[0]);
					}

					if (reference_list[m] is CompField) {
						var v_ref = ((CompField)reference_list[m]).GetLiteralValue();
						var v_tar = ((CompField)target_list[a]).GetLiteralValue();
						if (v_ref != v_tar) {
							comparison.AddError (String.Format ("Expected field value {0} but found value {1}", v_ref, v_tar));
							comparison.Status = ComparisonStatus.Error;
						}
					}
					
					if (reference_list[m] is ICompAttributeContainer) {
						//Console.WriteLine ("Comparing attributes for {0}", reference_list[m].Name);
						CompareAttributes (comparison,
						                   (ICompAttributeContainer)reference_list[m],
						                   (ICompAttributeContainer)target_list[a]);
					}
					
					if (reference_list[m] is ICompMemberContainer) {
						CompareMembers (comparison,
						                (ICompMemberContainer)reference_list[m],
						                (ICompMemberContainer)target_list[a]);
					}

					//CompareParameters (comparison, reference_list[m], target_namespace [target_list[a]]);
					m++;
					a++;
				}
				else if (c < 0) {
					if (isSealed && reference_list[m].Name.Contains ("~")) {
						// Ignore finalizer differences in sealed classes
					} else {
						/* reference name is before target name, reference name is missing from target */
						AddMissing (parent, reference_list[m]);
					}
					
					m++;
				}
				else {
					if (isSealed && target_list[a].Name.Contains ("~")) {
						// Ignore finalizer differences in sealed classes
					} else {
						/* reference name is after target name, target name is extra */
						AddExtra (parent, target_list[a]);
					}
					
					a++;
				}
			}
		}

		void AddExtra (ComparisonNode parent, CompNamed item)
		{
			ComparisonNode node = item.GetComparisonNode ();
			parent.AddChild (node);
			node.Status = ComparisonStatus.Extra;

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
			node.Status = ComparisonStatus.Missing;

			comparisons_performed ++;

			if (item is ICompHasBaseType) {
				string baseTypeName = ((ICompHasBaseType)item).GetBaseType();
				if (!string.IsNullOrEmpty (baseTypeName)) {
					ComparisonNode baseTypeNode = new ComparisonNode (CompType.Class,
											  string.Format ("BaseType: {0}",
													 baseTypeName),
											  baseTypeName);
					baseTypeNode.Status = ComparisonStatus.Missing;
					node.AddChild (baseTypeNode);
				}
			}

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
			if (item is ICompMemberContainer) {
				ICompMemberContainer c = (ICompMemberContainer)item;
				foreach (CompNamed ifc in c.GetInterfaces())
					AddMissing (node, ifc);
				foreach (CompNamed m in c.GetConstructors())
					AddMissing (node, m);
				foreach (CompNamed m in c.GetMethods())
					AddMissing (node, m);
				foreach (CompNamed p in c.GetProperties())
					AddMissing (node, p);
				foreach (CompNamed f in c.GetFields())
					AddMissing (node, f);
				foreach (CompNamed e in c.GetEvents())
					AddMissing (node, e);
			}
			if (item is ICompAttributeContainer) {
				ICompAttributeContainer c = (ICompAttributeContainer)item;
				foreach (CompNamed attr in c.GetAttributes())
					AddMissing (node, attr);
			}
		}

		// This is the reference assembly that we will be comparing to.
		CompAssembly reference;
		
		// This is the new API.
		CompAssembly target;

		void ProgressChange (double progress, string message)
		{
			if (ProgressChanged != null)
				ProgressChanged (this, new CompareProgressChangedEventArgs (message, progress));
		}

		void OnError (string message)
		{
			if (Error != null)
				Error (this, new CompareErrorEventArgs (message));
		}

		void Finish ()
		{
			if (Finished != null)
				Finished (this, EventArgs.Empty);
		}

		public event CompareProgressChangedEventHandler ProgressChanged;
		public event CompareErrorEventHandler Error;
		public event EventHandler Finished;

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
