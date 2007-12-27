using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	static class MasterUtils {
		public static void PopulateMethodList (XMLMethods methods, List<CompNamed> method_list)
		{
			foreach (object key in methods.keys.Keys) {
				XMLMethods.SignatureFlags signatureFlags = (methods.signatureFlags != null &&
				                                            methods.signatureFlags.ContainsKey (key) ?
				                                            (XMLMethods.SignatureFlags) methods.signatureFlags [key] :
				                                            XMLMethods.SignatureFlags.None);

				XMLParameters parameters = (methods.parameters == null ? null
				                            : (XMLParameters)methods.parameters[key]);
				XMLGenericMethodConstraints genericConstraints = (methods.genericConstraints == null ? null
				                                                  : (XMLGenericMethodConstraints)methods.genericConstraints[key]);
				XMLAttributes attributes = (methods.attributeMap == null ? null
				                            : (XMLAttributes)methods.attributeMap[key]);
				method_list.Add (new MasterMethod ((string)methods.keys[key],
				                                   signatureFlags,
				                                   parameters,
				                                   genericConstraints,
				                                   attributes));
			}
		}
		                                       
		public static void PopulateMemberLists (XMLClass xml_cls,
		                                        List<CompNamed> interface_list,
		                                        List<CompNamed> constructor_list,
		                                        List<CompNamed> method_list,
		                                        List<CompNamed> property_list,
		                                        List<CompNamed> field_list,
		                                        List<CompNamed> event_list)
		{
			if (interface_list != null && xml_cls.interfaces != null) {
				foreach (object i in xml_cls.interfaces.keys.Keys) {
					interface_list.Add (new MasterInterface ((string)xml_cls.interfaces.keys[i]));
				}
			}
			
			if (constructor_list != null && xml_cls.constructors != null) {
				PopulateMethodList (xml_cls.constructors, constructor_list);
			}
			
			if (method_list != null && xml_cls.methods != null) {
				PopulateMethodList (xml_cls.methods, method_list);
			}
			
			if (property_list != null && xml_cls.properties != null) {
				foreach (object key in xml_cls.properties.keys.Keys) {
					property_list.Add (new MasterProperty ((string)xml_cls.properties.keys[key],
					                                       (XMLMethods)xml_cls.properties.nameToMethod[key]));
				}
			}
			
			if (field_list != null && xml_cls.fields != null) {
				foreach (object key in xml_cls.fields.keys.Keys) {
					field_list.Add (new MasterField ((string)xml_cls.fields.keys[key]));
				}
			}
			
			if (event_list != null && xml_cls.events != null) {
				foreach (object key in xml_cls.events.keys.Keys) {
					event_list.Add (new MasterEvent ((string)xml_cls.events.keys[key],
					                                 (string)xml_cls.events.eventTypes[key]));
				}
			}
		}
	}
	
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
					interface_list.Add (new MasterInterface (cls));
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
		public MasterInterface (XMLClass xml_cls)
			: base (xml_cls.name)
		{
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();

			MasterUtils.PopulateMemberLists (xml_cls,
			                                 interfaces,
			                                 constructors,
			                                 methods,
			                                 properties,
			                                 fields,
			                                 events);
		}
		
		public MasterInterface (string name)
			: base (name)
		{
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
		}

		public override List<CompNamed> GetInterfaces ()
		{
			return interfaces;
		}

		public override List<CompNamed> GetMethods ()
		{
			return methods;
		}

		public override List<CompNamed> GetConstructors ()
		{
			return constructors;
		}

 		public override List<CompNamed> GetProperties()
		{
			return properties;
		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

 		public override List<CompNamed> GetEvents()
		{
			return events;
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
		
		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
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
			
			fields = new List<CompNamed>();

			MasterUtils.PopulateMemberLists (xml_cls,
			                                 null,
			                                 null,
			                                 null,
			                                 null,
			                                 fields,
			                                 null);

		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}

		List<CompNamed> fields;
		XMLClass xml_cls;
	}

	public class MasterClass : CompClass {
		public MasterClass (XMLClass cls, CompType type)
			: base (cls.name, type)
		{
			xml_cls = cls;

			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();

			MasterUtils.PopulateMemberLists (xml_cls,
			                                 interfaces,
			                                 constructors,
			                                 methods,
			                                 properties,
			                                 fields,
			                                 events);
		}

		public override List<CompNamed> GetInterfaces ()
		{
			return interfaces;
		}

		public override List<CompNamed> GetMethods()
		{
			return methods;
		}

		public override List<CompNamed> GetConstructors()
		{
			return constructors;
		}

 		public override List<CompNamed> GetProperties()
		{
			return properties;
		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

 		public override List<CompNamed> GetEvents()
		{
			return events;
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

		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
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
		public MasterProperty (string name, XMLMethods xml_methods)
			: base (name)
		{	
			methods = new List<CompNamed>();
			
			MasterUtils.PopulateMethodList (xml_methods, methods);
		}

		public override List<CompNamed> GetAttributes ()
		{
			// XXX
			return new List<CompNamed>();
		}
		
		public override List<CompNamed> GetMethods()
		{
			return methods;
		}

		List<CompNamed> methods;
	}
	
	public class MasterMethod : CompMethod {
		public MasterMethod (string name,
		                     XMLMethods.SignatureFlags signatureFlags,
		                     XMLParameters parameters,
		                     XMLGenericMethodConstraints genericConstraints,
		                     XMLAttributes attributes)
			: base (name)
		{
			this.signatureFlags = signatureFlags;
			this.parameters = parameters;
			this.genericConstraints = genericConstraints;
			this.attributes = attributes;
		}

		public override List<CompNamed> GetAttributes ()
		{
			List<CompNamed> rv = new List<CompNamed>();
			if (attributes != null) {
				foreach (object key in attributes.keys.Keys) {
					rv.Add (new MasterAttribute ((string)attributes.keys[key]));
				}
			}
			return rv;
		}

		XMLMethods.SignatureFlags signatureFlags;
		XMLParameters parameters;
		XMLGenericMethodConstraints genericConstraints;
		XMLAttributes attributes;
	}
			         
	public class MasterAttribute : CompAttribute {
		public MasterAttribute (string name)
			: base (name)
		{
		}
	}
}
