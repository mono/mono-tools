using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public class CecilAssembly : CompAssembly {
		public CecilAssembly (string path)
			: base (path)
		{
			Dictionary<string, Dictionary <string, TypeDefinition>> namespaces
				= new Dictionary<string, Dictionary <string, TypeDefinition>> ();

			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(path);

			foreach (TypeDefinition t in assembly.MainModule.Types) {
				if (t.Name == "<Module>")
					continue;

				// XXX do we really want this?
				if (t.Namespace == "")
					continue;

				if (t.IsSpecialName || t.IsRuntimeSpecialName)
					continue;

				if (ShouldSkip (t.Name))
					continue;

				if (!namespaces.ContainsKey (t.Namespace))
					namespaces[t.Namespace] = new Dictionary <string, TypeDefinition> ();

				namespaces[t.Namespace][t.Name] = t;
			}

			namespace_list = new List<CompNamespace>();
			foreach (string ns_name in namespaces.Keys) {
				namespace_list.Add (new CecilNamespace (ns_name, namespaces[ns_name]));
			}

			namespace_list.Sort (delegate (CompNamespace x, CompNamespace y) { return String.Compare (x.Name, y.Name); });
		}

		public override List<CompNamespace> GetNamespaces()
		{
			return namespace_list;
		}

		bool ShouldSkip (string name)
		{
			return (name == "MonoLimitationAttribute" ||
				name == "MonoDocumentationNoteAttribute" ||
				name == "MonoTODOAttribute" ||
				name == "MonoExtensionAttribute" ||
				name == "MonoNotSupportedAttribute" ||
				name == "MonoInternalNoteAttribute");
				
		}

		List<CompNamespace> namespace_list;
	}

	public class CecilNamespace : CompNamespace {
		public CecilNamespace (string name, Dictionary<string, TypeDefinition> class_mapping)
			: base (name)
		{
			class_list = new List<CompClass>();
// 			enum_list = new List<CompEnum>();
// 			delegate_list = new List<CompDelegate>();
			interface_list = new List<CompInterface>();
			struct_list = new List<CompClass>();

			foreach (string cls_name in class_mapping.Keys) {
				TypeDefinition type_def = class_mapping[cls_name];
				if (type_def.IsNotPublic)
					continue;
				if (type_def.IsValueType) {
					if (type_def.IsEnum) {
						Console.WriteLine ("enum -> {0}", type_def.FullName);
					}
					else {
						struct_list.Add (new CecilClass (class_mapping[cls_name]));
					}
				}
				else if (type_def.IsInterface) {
					interface_list.Add (new CecilInterface (class_mapping[cls_name]));
				}
				else {
					class_list.Add (new CecilClass (class_mapping[cls_name]));
				}
			}

			class_list.Sort (delegate (CompClass x, CompClass y) { return String.Compare (x.Name, y.Name); });
		}

		public override List<CompClass> GetClasses()
		{
			return class_list;
		}

		public override List<CompInterface> GetInterfaces ()
		{
			return interface_list;
		}

		public override List<CompClass> GetStructs ()
		{
			return struct_list;
		}

		List<CompClass> class_list;
		List<CompInterface> interface_list;
		List<CompClass> struct_list;
	}

	public class CecilInterface : CompInterface {
		public CecilInterface (TypeReference type_ref)
			: base (type_ref.FullName)
		{
			this.type_ref = type_ref;
		}

		TypeReference type_ref;
	}

	public class CecilClass : CompClass {
		public CecilClass (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;
		}

		public override List<CompInterface> GetInterfaces ()
		{
			List<CompInterface> rv = new List<CompInterface>();

			foreach (TypeReference md in type_def.Interfaces) {
				rv.Add (new CecilInterface (md));
			}

			rv.Sort (delegate (CompInterface x, CompInterface y) { return String.Compare (x.Name, y.Name); });

			return rv;
		}

		public override List<CompMethod> GetMethods ()
		{
			List<CompMethod> rv = new List<CompMethod>();

			foreach (MethodDefinition md in type_def.Methods) {
				if (md.IsSpecialName)
					continue;
				rv.Add (new CecilMethod (md));
			}

			rv.Sort (delegate (CompMethod x, CompMethod y) { return String.Compare (x.Name, y.Name); });

			return rv;
		}

		public override List<CompMethod> GetConstructors ()
		{
			return new List<CompMethod>();
		}

		public override List<CompClass> GetNestedClasses()
		{
			List<CompClass> rv = new List<CompClass>();
			foreach (TypeDefinition td in type_def.NestedTypes) {
				rv.Add (new CecilClass (td));
			}

			rv.Sort (delegate (CompClass x, CompClass y) { return String.Compare (x.Name, y.Name); });

			return rv;
		}

		TypeDefinition type_def;
	}

	public class CecilMethod : CompMethod {
		public CecilMethod (MethodDefinition method_def)
			: base (MasterinfoFormattedName (method_def))
		{
			this.method_def = method_def;
		}

		static string MasterinfoFormattedName (MethodDefinition method_def)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (method_def.Name);
			sb.Append ('(');
			bool comma = false;
			foreach (ParameterDefinition p in method_def.Parameters) {
				if (comma)
					sb.Append (", ");
				sb.Append (p.ParameterType.FullName);
				comma = true;
			}
			sb.Append (')');

			return sb.ToString();
		}

		MethodDefinition method_def;
	}
}
