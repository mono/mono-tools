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
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.fields != null) {
				foreach (object key in xml_cls.fields.keys.Keys) {
					rv.Add (new MasterField ((string)xml_cls.fields.keys[key]));
				}
			}

			return rv;
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
					XMLMethods.SignatureFlags signatureFlags = (xml_cls.methods.signatureFlags != null &&
										    xml_cls.methods.signatureFlags.ContainsKey (key) ?
										    (XMLMethods.SignatureFlags) xml_cls.methods.signatureFlags [key] :
										     XMLMethods.SignatureFlags.None);

					XMLParameters parameters = (xml_cls.methods.parameters == null ? null
								    : (XMLParameters)xml_cls.methods.parameters[key]);
					XMLGenericMethodConstraints genericConstraints = (xml_cls.methods.genericConstraints == null ? null
											  : (XMLGenericMethodConstraints)xml_cls.methods.genericConstraints[key]);
					rv.Add (new MasterMethod ((string)xml_cls.methods.keys[key],
								  signatureFlags,
								  parameters,
								  genericConstraints));
				}
			}

			return rv;
		}

		public override List<CompNamed> GetConstructors()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.constructors != null) {
				foreach (object key in xml_cls.constructors.keys.Keys) {
					rv.Add (new MasterMethod ((string)xml_cls.constructors.keys[key],
								  (XMLConstructors.SignatureFlags)xml_cls.constructors.signatureFlags[key],
								  (XMLParameters)xml_cls.constructors.parameters[key],
								  (XMLGenericMethodConstraints)xml_cls.constructors.genericConstraints[key]));
				}
			}

			return rv;
		}

 		public override List<CompNamed> GetProperties()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.properties != null) {
				foreach (object key in xml_cls.properties.keys.Keys) {
					rv.Add (new MasterProperty ((string)xml_cls.properties.keys[key]));
				}
			}

			return rv;
		}

 		public override List<CompNamed> GetFields()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.fields != null) {
				foreach (object key in xml_cls.fields.keys.Keys) {
					rv.Add (new MasterField ((string)xml_cls.fields.keys[key]));
				}
			}

			return rv;
		}

 		public override List<CompNamed> GetEvents()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (xml_cls.events != null) {
				foreach (object key in xml_cls.events.keys.Keys) {
					rv.Add (new MasterEvent ((string)xml_cls.events.keys[key],
					                         (string)xml_cls.events.eventTypes[key]));
				}
			}

			return rv;
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

	public class MasterEvent : CompEvent {
		public MasterEvent (string name,
		                    string eventType)
			: base (name)
		{
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
	}
	

	public class MasterField : CompField {
		public MasterField (string name)
			: base (name)
		{
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
	}
	
	public class MasterProperty : CompProperty {
		public MasterProperty (string name)
			: base (name)
		{
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
	}
	
	// XXX more stuff is needed here besides the name
	public class MasterMethod : CompMethod {
		public MasterMethod (string name,
				     XMLMethods.SignatureFlags signatureFlags,
				     XMLParameters parameters,
				     XMLGenericMethodConstraints genericConstraints)
			: base (name)
		{
			this.signatureFlags = signatureFlags;
			this.parameters = parameters;
			this.genericConstraints = genericConstraints;
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}

		XMLMethods.SignatureFlags signatureFlags;
		XMLParameters parameters;
		XMLGenericMethodConstraints genericConstraints;
	}
}
