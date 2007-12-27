using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using Mono.Cecil;
using Gtk;

namespace GuiCompare {

	static class Utils {
		public static void PopulateMemberLists (TypeDefinition fromDef,
							List<CompNamed> interface_list,
							List<CompNamed> constructor_list,
							List<CompNamed> method_list,
							List<CompNamed> property_list,
							List<CompNamed> field_list,
							List<CompNamed> event_list)
		{
			Console.WriteLine ("21Populating {0}.{1}", fromDef.Namespace, fromDef.Name);
			if (interface_list != null) {
				foreach (TypeReference ifc in fromDef.Interfaces) {
					interface_list.Add (new CecilInterface (ifc, true));
				}
			}
			if (constructor_list != null) {
				foreach (MethodDefinition md in fromDef.Constructors) {
					if (md.IsPrivate || md.IsAssembly)
						continue;
					constructor_list.Add (new CecilMethod (md));
				}
			}
			if (method_list != null) {
				foreach (MethodDefinition md in fromDef.Methods) {
					if (md.IsSpecialName)
						continue;
					if (md.IsPrivate || md.IsAssembly)
						continue;
					method_list.Add (new CecilMethod (md));
				}
			}
			if (property_list != null) {
				foreach (PropertyDefinition pd in fromDef.Properties) {
					if (/*pd.IsPrivate || pd.IsAssembly*/true)
						property_list.Add (new CecilProperty (pd));
				}
			}
			if (field_list != null) {
				foreach (FieldDefinition fd in fromDef.Fields) {
					if (fd.IsSpecialName)
						continue;
					if (fd.IsPrivate || fd.IsAssembly){
						//Console.WriteLine ("    Skipping over {0}.{1} {2}", fromDef.Namespace, fromDef.Name, fd.Name);
						continue;
					}
					//Console.WriteLine ("    Adding {0}.{1} {2}", fromDef.Namespace, fromDef.Name, fd.Name);
					field_list.Add (new CecilField (fd));
				}
			}
			if (event_list != null) {
				foreach (EventDefinition ed in fromDef.Events) {
					if (/*ed.IsPrivate || ed.IsAssembly*/true)
						event_list.Add (new CecilEvent (ed));
				}
			}
		}

		public static void PopulateTypeLists (TypeDefinition fromDef,
						      List<CompNamed> class_list,
						      List<CompNamed> enum_list,
						      List<CompNamed> delegate_list,
						      List<CompNamed> interface_list,
						      List<CompNamed> struct_list)
		{
			foreach (TypeDefinition type_def in fromDef.NestedTypes) {
				Console.WriteLine ("Got {0}.{1} => {2}", type_def.Namespace, type_def.Name, type_def.Attributes & TypeAttributes.VisibilityMask);
				if (type_def.IsNestedPrivate || type_def.IsNestedAssembly || type_def.IsNotPublic){
					continue;
				}
				
				if (type_def.IsValueType) {
					if (type_def.IsEnum) {
						enum_list.Add (new CecilEnum (type_def));
					}
					else {
						struct_list.Add (new CecilClass (type_def, CompType.Struct));
					}
				}
				else if (type_def.IsInterface) {
					interface_list.Add (new CecilInterface (type_def, false));
				}
				else if (type_def.BaseType.FullName == "System.MulticastDelegate"
					 || type_def.BaseType.FullName == "System.Delegate") {
					delegate_list.Add (new CecilDelegate (type_def));
				}
				else {
					class_list.Add (new CecilClass (type_def, CompType.Class));
				}
			}
		}
	}

	public class CecilAssembly : CompAssembly {
		public CecilAssembly (string path)
			: base (Path.GetFileName (path))
		{
			Dictionary<string, Dictionary <string, TypeDefinition>> namespaces
				= new Dictionary<string, Dictionary <string, TypeDefinition>> ();

			AssemblyDefinition assembly = AssemblyFactory.GetAssembly(path);

			foreach (TypeDefinition t in assembly.MainModule.Types) {
				if (t.Name == "<Module>")
					continue;

				if (t.IsNotPublic)
					continue;
				
				if (t.IsNested)
					continue;
				
				if (t.IsSpecialName || t.IsRuntimeSpecialName)
					continue;

				if (ShouldSkip (t.Name))
					continue;

				if (!namespaces.ContainsKey (t.Namespace))
					namespaces[t.Namespace] = new Dictionary <string, TypeDefinition> ();

				namespaces[t.Namespace][t.Name] = t;
			}

			namespace_list = new List<CompNamed>();
			foreach (string ns_name in namespaces.Keys) {
				namespace_list.Add (new CecilNamespace (ns_name, namespaces[ns_name]));
			}
		}

		public override List<CompNamed> GetNamespaces()
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

		List<CompNamed> namespace_list;
	}

	public class CecilNamespace : CompNamespace {
		public CecilNamespace (string name, Dictionary<string, TypeDefinition> type_mapping)
			: base (name)
		{
			class_list = new List<CompNamed>();
			enum_list = new List<CompNamed>();
 			delegate_list = new List<CompNamed>();
			interface_list = new List<CompNamed>();
			struct_list = new List<CompNamed>();

			foreach (string type_name in type_mapping.Keys) {
				TypeDefinition type_def = type_mapping[type_name];
				if (type_def.IsNotPublic)
					continue;
				if (type_def.IsValueType) {
					if (type_def.IsEnum) {
						enum_list.Add (new CecilEnum (type_def));
					}
					else {
						struct_list.Add (new CecilClass (type_def, CompType.Struct));
					}
				}
				else if (type_def.IsInterface) {
					interface_list.Add (new CecilInterface (type_def, false));
				}
				else if (type_def.BaseType != null && (type_def.BaseType.FullName == "System.MulticastDelegate"
				                              || type_def.BaseType.FullName == "System.Delegate")) {
					delegate_list.Add (new CecilDelegate (type_def));
				}
				else {
					class_list.Add (new CecilClass (type_def, CompType.Class));
				}
			}
		}

		public override List<CompNamed> GetNestedClasses()
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

		List<CompNamed> class_list;
		List<CompNamed> interface_list;
		List<CompNamed> struct_list;
		List<CompNamed> delegate_list;
		List<CompNamed> enum_list;
	}

	public class CecilInterface : CompInterface {
		public CecilInterface (TypeReference type_ref, bool full_name)
			: base (full_name ? type_ref.FullName : type_ref.Name)
		{
			this.type_ref = type_ref;
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
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in type_ref.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}

		TypeReference type_ref;
	}

	public class CecilDelegate : CompDelegate {
		public CecilDelegate (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;
		}

		TypeDefinition type_def;
	}

	public class CecilEnum : CompEnum {
		public CecilEnum (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;

			fields = new List<CompNamed>();

			Utils.PopulateMemberLists (type_def,
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
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in type_def.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}

		TypeDefinition type_def;
		List<CompNamed> fields;
	}

	public class CecilClass : CompClass {
		public CecilClass (TypeDefinition type_def, CompType type)
			: base (type_def.Name, type)
		{
			this.type_def = type_def;

			nested_classes = new List<CompNamed>();
			nested_enums = new List<CompNamed>();
 			nested_delegates = new List<CompNamed>();
			nested_interfaces = new List<CompNamed>();
			nested_structs = new List<CompNamed>();

			Utils.PopulateTypeLists (type_def,
						 nested_classes,
						 nested_enums,
						 nested_delegates,
						 nested_interfaces,
						 nested_structs);

			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();

			Utils.PopulateMemberLists (type_def,
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
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in type_def.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}

		public override List<CompNamed> GetNestedClasses()
		{
			return nested_classes;
		}

		public override List<CompNamed> GetNestedInterfaces ()
		{
			return nested_interfaces;
		}

		public override List<CompNamed> GetNestedStructs ()
		{
			return nested_structs;
		}

		public override List<CompNamed> GetNestedEnums ()
		{
			return nested_enums;
		}

		public override List<CompNamed> GetNestedDelegates ()
		{
			return nested_delegates;
		}

		TypeDefinition type_def;
		List<CompNamed> nested_classes;
		List<CompNamed> nested_interfaces;
		List<CompNamed> nested_structs;
		List<CompNamed> nested_delegates;
		List<CompNamed> nested_enums;

		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
	}

	public class CecilField : CompField {
		public CecilField (FieldDefinition field_def)
			: base (field_def.Name)
		{
			this.field_def = field_def;
		}

		public override List<CompNamed> GetAttributes ()
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in field_def.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}

		FieldDefinition field_def;
	}

	public class CecilMethod : CompMethod {
		public CecilMethod (MethodDefinition method_def)
			: base (MasterinfoFormattedName (method_def))
		{
			this.method_def = method_def;
		}

		public override List<CompNamed> GetAttributes ()
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in method_def.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
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

	public class CecilProperty : CompProperty
	{
		public CecilProperty (PropertyDefinition pd)
			: base (pd.Name)
		{
			this.pd = pd;
		}

		public override List<CompNamed> GetAttributes ()
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in pd.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}
		
		public override List<CompNamed> GetMethods()
		{
			List<CompNamed> rv = new List<CompNamed>();

			if (pd.GetMethod != null)
				rv.Add (new CecilMethod (pd.GetMethod));
			if (pd.SetMethod != null)
				rv.Add (new CecilMethod (pd.SetMethod));
			
			return rv;
		}
		
		PropertyDefinition pd;
	}
	
	public class CecilEvent : CompEvent
	{
		public CecilEvent (EventDefinition ed)
			: base (ed.Name)
		{
			this.ed = ed;
		}

		public override List<CompNamed> GetAttributes ()
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in ed.CustomAttributes)
				rv.Add (new CecilAttribute (ca));
			return rv;
		}
		
		EventDefinition ed;
	}
	
	public class CecilAttribute : CompAttribute
	{
		public CecilAttribute (CustomAttribute ca)
			: base (ca.Constructor.DeclaringType.FullName)
		{
			this.ca = ca;
		}
		
		CustomAttribute ca;
	}
}