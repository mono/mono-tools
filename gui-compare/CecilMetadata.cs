using System;
using System.Collections.Generic;
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
			foreach (string cls_name in class_mapping.Keys) {
				class_list.Add (new CecilClass (class_mapping[cls_name]));
			}

			class_list.Sort (delegate (CompClass x, CompClass y) { return String.Compare (x.Name, y.Name); });
		}

		public override List<CompClass> GetClasses()
		{
			return class_list;
		}

		List<CompClass> class_list;
	}

	public class CecilClass : CompClass {
		public CecilClass (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;
		}

		public override List<CompMethod> GetMethods ()
		{
			List<CompMethod> rv = new List<CompMethod>();

			foreach (MethodDefinition md in type_def.Methods) {
				rv.Add (new CecilMethod (md));
			}

			rv.Sort (delegate (CompMethod x, CompMethod y) { return String.Compare (x.Name, y.Name); });

			return rv;
		}

		public override List<CompMethod> GetConstructors ()
		{
			return null;
		}

		TypeDefinition type_def;
	}

	public class CecilMethod : CompMethod {
		public CecilMethod (MethodDefinition method_def)
			: base (method_def.Name)
		{
			this.method_def = method_def;
		}

		MethodDefinition method_def;
	}
}
