using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	public class MasterAssembly : CompAssembly {
		public MasterAssembly (string path)
			: base (path)
		{
			masterinfo = XMLAssembly.CreateFromFile (path);
		}

		public override List<CompNamed> GetNamespaces ()
		{
			List<CompNamed> namespaces = new List<CompNamed>();
			foreach (XMLNamespace ns in masterinfo.namespaces)
				namespaces.Add (new MasterNamespace (ns));

			return namespaces;
		}

		XMLAssembly masterinfo;
	}

	public class MasterNamespace : CompNamespace {
		public MasterNamespace (XMLNamespace ns)
			: base (ns.name)
		{
			this.ns = ns;

			delegate_list = new List<CompNamed>();
			enum_list = new List<CompNamed>();
			class_list = new List<CompNamed>();
			struct_list = new List<CompNamed>();
			interface_list = new List<CompNamed>();

			foreach (XMLClass cls in ns.types) {
				if (cls.type == "class")
					class_list.Add (new MasterClass (cls, CompType.Class));
				else if (cls.type == "enum")
					enum_list.Add (new MasterEnum (cls));
				else if (cls.type == "delegate")
					delegate_list.Add (new MasterDelegate (cls));
				else if (cls.type == "interface")
					interface_list.Add (new MasterInterface (cls.name));
				else if (cls.type == "struct")
					struct_list.Add (new MasterClass (cls, CompType.Struct));
			}
		}

		public override List<CompNamed> GetNestedClasses ()
		{
			return class_list;
		}

		public override List<CompNamed> GetNestedInterfaces ()
		{
			return interface_list;
		}

		public override List<CompNamed> GetNestedStructs ()
		{
			return struct_list;
		}

		public override List<CompNamed> GetNestedEnums ()
		{
			return enum_list;
		}

		public override List<CompNamed> GetNestedDelegates ()
		{
			return delegate_list;
		}


		XMLNamespace ns;
		List<CompNamed> delegate_list;
		List<CompNamed> enum_list;
		List<CompNamed> class_list;
		List<CompNamed> struct_list;
		List<CompNamed> interface_list;
	}

	public class MasterInterface : CompInterface {
		public MasterInterface (string name)
			: base (name)
		{
		}

		public override List<CompNamed> GetInterfaces ()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetMethods ()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetConstructors ()
		{
			// XXX
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetProperties()
		{
			// XXX
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetFields()
		{
			// XXX
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetEvents()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
	}

	public class MasterDelegate : CompDelegate {
		public MasterDelegate (XMLClass cls)
			: base (cls.name)
		{
			xml_cls = cls;
		}

		XMLClass xml_cls;
	}

	public class MasterEnum : CompEnum {
		public MasterEnum (XMLClass cls)
			: base (cls.name)
		{
			xml_cls = cls;
		}

 		public override List<CompNamed> GetFields()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}


		XMLClass xml_cls;
	}

	public class MasterClass : CompClass {
		public MasterClass (XMLClass cls, CompType type)
			: base (cls.name, type)
		{
			xml_cls = cls;
		}

		public override List<CompNamed> GetInterfaces ()
		{
			List<CompNamed> rv = new List<CompNamed>();

			if (xml_cls.interfaces != null) {
				foreach (object i in xml_cls.interfaces.keys.Keys) {
					rv.Add (new MasterInterface ((string)xml_cls.interfaces.keys[i]));
				}
			}
					

			return rv;
		}

		public override List<CompNamed> GetMethods()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.methods != null) {
				foreach (object key in xml_cls.methods.keys.Keys) {
					rv.Add (new MasterMethod ((string)xml_cls.methods.keys[key]));
				}
			}

			return rv;
		}

		public override List<CompNamed> GetConstructors()
		{
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetProperties()
		{
			// XXX
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetFields()
		{
			// XXX
			return new List<CompNamed>();
		}

 		public override List<CompNamed> GetEvents()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetNestedClasses()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.nested != null) {
				foreach (XMLClass nested in xml_cls.nested)
					rv.Add (new MasterClass (nested, CompType.Class));
			}

			return rv;
		}

		public override List<CompNamed> GetNestedInterfaces ()
		{
			return new List<CompNamed>();;
		}

		public override List<CompNamed> GetNestedStructs ()
		{
			return new List<CompNamed>();;
		}

		public override List<CompNamed> GetNestedEnums ()
		{
			// XXX
			return new List<CompNamed>();
		}

		public override List<CompNamed> GetNestedDelegates ()
		{
			// XXX
			return new List<CompNamed>();
		}

		XMLClass xml_cls;
	}

	// XXX more stuff is needed here besides the name
	public class MasterMethod : CompMethod {
		public MasterMethod (string name)
			: base (name)
		{
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}

	}
}
