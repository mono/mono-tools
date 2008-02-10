using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gtk;

namespace GuiCompare {

	static class CecilUtils {
		public static string PrettyType (string type)
		{
			/* handle suffixes first, recursively */
			if (type.EndsWith ("[]"))
				return String.Concat (PrettyType (type.Substring (0, type.Length - 2)), "[]");
			else if (type.EndsWith ("*"))
				return String.Concat (PrettyType (type.Substring (0, type.Length - 1)), "*");
			else if (type.EndsWith ("&"))
				return String.Concat (PrettyType (type.Substring (0, type.Length - 1)), "&");

			/* handle the system types we know about */
			switch (type) {
			case "System.Boolean": return "bool";
			case "System.Byte": return "byte";
			case "System.Char": return "char";
			case "System.Decimal": return "decimal";
			case "System.Double": return "double";
			case "System.Int16": return "short";
			case "System.Int32": return "int";
			case "System.Int64": return "long";
			case "System.Object": return "object";
			case "System.SByte": return "sbyte";
			case "System.Single": return "float";
			case "System.String": return "string";
			case "System.UInt16": return "ushort";
			case "System.UInt32": return "uint";
			case "System.UInt64": return "ulong";
			case "System.Void": return "void";
			}

			/* for other types, just return the type name */
			return type.Substring (type.LastIndexOf ('.')+1);
		}
		
		// the corcompare xml output uses a different formatting than Cecil.
		// Cecil uses / for nested classes, ala:
		//  Namespace.Class/NestedClass
		// while corcompare uses:
		//  Namespace.Class+NestedClass
		// also, generic methods are done differently as well.
		// cecil:  Foo<T>
		// corcompare: Foo[T]
		//
		// so let's just convert everything to corcompare's way of thinking for comparisons.
		//
		public static string FormatTypeLikeCorCompare (string type)
		{
			string rv = type.Replace ('/', '+');
			rv = rv.Replace ('<', '[');
			rv = rv.Replace ('>', ']');
			return rv;
		}
		
		public static void PopulateMemberLists (TypeDefinition fromDef,
		                                        List<CompNamed> interface_list,
		                                        List<CompNamed> constructor_list,
		                                        List<CompNamed> method_list,
		                                        List<CompNamed> property_list,
		                                        List<CompNamed> field_list,
		                                        List<CompNamed> event_list)
		{
			if (interface_list != null) {
				TypeDefinition td = fromDef;
				TypeReference fromRef;
				
				do {
					foreach (TypeReference ifc in td.Interfaces) {
						TypeDefinition ifc_def = CecilUtils.Resolver.Resolve (ifc);
						if (ifc_def.IsNotPublic)
							continue;
							
						CecilInterface new_ifc = new CecilInterface (ifc);
						bool found = false;
						for (int i = 0; i < interface_list.Count; i ++) {
							if (interface_list[i].Name == new_ifc.Name) {
								found = true;
								break;
							}
						}
						if (!found)
							interface_list.Add (new_ifc);
					}
					fromRef = td.BaseType;
					if (fromRef != null)
						td = CecilUtils.Resolver.Resolve (fromRef);
				} while (fromRef != null);
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
					if (md.IsSpecialName) {
						if (!md.Name.StartsWith("op_"))
							continue;
					}
					if (md.IsPrivate || md.IsAssembly)
						continue;
					method_list.Add (new CecilMethod (md));
				}
			}
			if (property_list != null) {
				foreach (PropertyDefinition pd in fromDef.Properties) {
					bool include_set = true;
					bool include_get = true;
					if (pd.SetMethod == null || (pd.SetMethod.IsPrivate || pd.SetMethod.IsAssembly))
						include_set = false;
					if (pd.GetMethod == null || (pd.GetMethod.IsPrivate || pd.GetMethod.IsAssembly))
						include_get = false;
					if (include_set || include_get)
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
					if (ed.IsSpecialName)
						continue;

					if (ed.AddMethod == null || ed.AddMethod.IsPrivate || ed.AddMethod.IsAssembly)
						continue;
					
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
				//Console.WriteLine ("Got {0}.{1} => {2}", type_def.Namespace, type_def.Name, type_def.Attributes & TypeAttributes.VisibilityMask);
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
					interface_list.Add (new CecilInterface (type_def));
				}
				else if ((type_def.FullName == "System.MulticastDelegate" ||
				          type_def.BaseType.FullName == "System.MulticastDelegate")
				         || (type_def.FullName == "System.Delegate" ||
				             type_def.BaseType.FullName == "System.Delegate")) {
					delegate_list.Add (new CecilDelegate (type_def));
				}
				else {
					class_list.Add (new CecilClass (type_def, CompType.Class));
				}
			}
		}

		public static string GetTODOText (CustomAttribute ca)
		{
			StringBuilder sb = new StringBuilder();
			bool first = true;
			foreach (object o in ca.ConstructorParameters) {
				if (!first)
					sb.Append (", ");
				first = false;
				sb.Append (o.ToString());
			}
			
			return sb.ToString();
		}
		
		public static bool IsTODOAttribute (TypeDefinition typedef)
		{
			if (typedef == null)
				return false;
			
			if (typedef.Name == "MonoTODOAttribute")
				return true;
			
			if (typedef.BaseType == null)
				return false;
			
			return IsTODOAttribute (CecilUtils.Resolver.Resolve (typedef.BaseType));
		}
		
		public static bool ShouldSkipAttribute (string name)
		{
			if (name == "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute")
				return true;
			
			return false;
		}
		
		public static List<CompNamed> GetCustomAttributes (ICustomAttributeProvider provider, List<string> todos)
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in provider.CustomAttributes) {
				TypeDefinition resolved = CecilUtils.Resolver.Resolve (ca.Constructor.DeclaringType);
				
				if (resolved.IsNotPublic)
					continue;
				
				if (IsTODOAttribute (resolved))
					todos.Add (String.Format ("[{0} ({1})]", ca.Constructor.DeclaringType.Name, CecilUtils.GetTODOText (ca)));
				else if (!ShouldSkipAttribute (ca.Constructor.DeclaringType.FullName))
					rv.Add (new CecilAttribute (ca));
			}
			return rv;
		}

		
		public static readonly AssemblyResolver Resolver = new AssemblyResolver();
	}

	public class CecilAssembly : CompAssembly {
		public CecilAssembly (string path)
			: base (Path.GetFileName (path))
		{
			Dictionary<string, Dictionary <string, TypeDefinition>> namespaces = new Dictionary<string, Dictionary <string, TypeDefinition>> ();

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

				if (CecilUtils.IsTODOAttribute (t))
					continue;

				if (!namespaces.ContainsKey (t.Namespace))
					namespaces[t.Namespace] = new Dictionary <string, TypeDefinition> ();

				namespaces[t.Namespace][t.Name] = t;
			}

			namespace_list = new List<CompNamed>();
			foreach (string ns_name in namespaces.Keys) {
				namespace_list.Add (new CecilNamespace (ns_name, namespaces[ns_name]));
			}
			
			attributes = CecilUtils.GetCustomAttributes (assembly, todos);
		}

		public override List<CompNamed> GetNamespaces()
		{
			return namespace_list;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
		
		List<CompNamed> namespace_list;
		List<CompNamed> attributes;
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
						if (type_def.FullName == "System.Enum")
							class_list.Add (new CecilClass (type_def, CompType.Class));
						else
							struct_list.Add (new CecilClass (type_def, CompType.Struct));
					}
				}
				else if (type_def.IsInterface) {
					interface_list.Add (new CecilInterface (type_def));
				}
				else if ((type_def.FullName == "System.MulticastDelegate" ||
				          (type_def.BaseType != null && type_def.BaseType.FullName == "System.MulticastDelegate"))
				         || (type_def.FullName == "System.Delegate" ||
				             (type_def.BaseType != null && type_def.BaseType.FullName == "System.Delegate"))) {
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
		public CecilInterface (TypeDefinition type_def)
			: base (CecilUtils.FormatTypeLikeCorCompare (type_def.Name))
		{
			this.type_def = type_def;
			
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();

			CecilUtils.PopulateMemberLists (type_def,
			                                interfaces,
			                                constructors,
			                                methods,
			                                properties,
			                                fields,
			                                events);
			
			attributes = CecilUtils.GetCustomAttributes (type_def, todos);
		}
		
		public CecilInterface (TypeReference type_ref)
			: base (CecilUtils.FormatTypeLikeCorCompare (type_ref.FullName))
		{
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
			
			attributes = new List<CompNamed>();
		}

		public override string GetBaseType ()
		{
			return (type_def == null || type_def.BaseType == null) ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType.FullName);
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
			return attributes;
		}
		
		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
		List<CompNamed> attributes;
		TypeDefinition type_def;
	}

	public class CecilDelegate : CompDelegate {
		public CecilDelegate (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;
		}

		public override string GetBaseType ()
		{
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType.FullName);
		}
		
		TypeDefinition type_def;
	}

	public class CecilEnum : CompEnum {
		public CecilEnum (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;

			fields = new List<CompNamed>();

			CecilUtils.PopulateMemberLists (type_def,
						   null,
						   null,
						   null,
						   null,
						   fields,
						   null);
			
			attributes = CecilUtils.GetCustomAttributes (type_def, todos); 
		}

		public override string GetBaseType ()
		{
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType.FullName);
		}

 		public override List<CompNamed> GetFields()
		{
			return fields;
		}

		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}

		TypeDefinition type_def;
		List<CompNamed> fields;
		List<CompNamed> attributes;
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

			CecilUtils.PopulateTypeLists (type_def,
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

			CecilUtils.PopulateMemberLists (type_def,
			                           interfaces,
			                           constructors,
			                           methods,
			                           properties,
			                           fields,
			                           events);

			attributes = CecilUtils.GetCustomAttributes (type_def, todos);
		}

		public override string GetBaseType ()
		{
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType.FullName);
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
			return attributes;
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
		List<CompNamed> attributes;
	}

	public class CecilField : CompField {
		public CecilField (FieldDefinition field_def)
			: base (field_def.Name)
		{
			this.field_def = field_def;
			this.attributes = CecilUtils.GetCustomAttributes (field_def, todos);
		}

		public override string GetMemberType ()
		{
			return CecilUtils.FormatTypeLikeCorCompare (field_def.FieldType.FullName);
		}
		
		const FieldAttributes masterInfoFieldMask = (FieldAttributes.FieldAccessMask | 
		                                             FieldAttributes.Static | 
		                                             FieldAttributes.InitOnly | 
		                                             FieldAttributes.Literal | 
		                                             FieldAttributes.HasDefault | 
		                                             FieldAttributes.HasFieldMarshal);
		public override string GetMemberAccess ()
		{
			FieldAttributes fa = field_def.Attributes & masterInfoFieldMask;

			// remove the Assem from FamORAssem
			if ((fa & FieldAttributes.FamORAssem) == FieldAttributes.FamORAssem)
				fa = (fa & ~(FieldAttributes.FamORAssem)) | (FieldAttributes.Family);

			return fa.ToString();
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}

		public override string GetLiteralValue ()
		{
			if (field_def.IsLiteral)
				return field_def.Constant.ToString();
			return null;
		}
		
		FieldDefinition field_def;
		List<CompNamed> attributes;
	}

	public class CecilMethod : CompMethod {
		public CecilMethod (MethodDefinition method_def)
			: base (FormatName (method_def, false))
		{
			this.method_def = method_def;
			this.attributes = CecilUtils.GetCustomAttributes (method_def, todos);
			DisplayName = FormatName (method_def, true);
		}

		public override string GetMemberType ()
		{
			if (method_def.IsConstructor)
				return null;
			
			return CecilUtils.FormatTypeLikeCorCompare (method_def.ReturnType.ReturnType.FullName);
		}

		public override bool ThrowsNotImplementedException ()
		{
                        if (method_def.Body != null)
                                foreach (Instruction i in method_def.Body.Instructions)
                                        if (i.OpCode == OpCodes.Throw)
                                                if (i.Previous.Operand != null && i.Previous.Operand.ToString ().StartsWith ("System.Void System.NotImplementedException"))
                                                        return true;

                        return false;
		}

		const MethodAttributes masterInfoMethodMask = (MethodAttributes.MemberAccessMask |
		                                               MethodAttributes.Virtual |
		                                               MethodAttributes.Final |
		                                               MethodAttributes.Static |
		                                               MethodAttributes.Abstract |
		                                               MethodAttributes.HideBySig |
		                                               MethodAttributes.SpecialName);
		public override string GetMemberAccess ()
		{
			MethodAttributes ma = method_def.Attributes & masterInfoMethodMask;

			// remove the Assem from FamORAssem
			if ((ma & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem)
				ma = (ma & ~(MethodAttributes.FamORAssem)) | (MethodAttributes.Family);

			return ma.ToString();
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}

		void SetDisplayName ()
		{

		}
		
		static string FormatName (MethodDefinition method_def, bool beautify)
		{
			StringBuilder sb = new StringBuilder ();
			if (!method_def.IsConstructor)
				sb.Append (beautify
				           ? CecilUtils.PrettyType (method_def.ReturnType.ReturnType.FullName)
				           : CecilUtils.FormatTypeLikeCorCompare (method_def.ReturnType.ReturnType.FullName));
			sb.Append (" ");
			if (beautify) {
				if (method_def.IsSpecialName && method_def.Name.StartsWith ("op_")) {
					switch (method_def.Name) {
					case "op_Explicit": sb.Append ("operator explicit"); break;
					case "op_Implicit": sb.Append ("operator implicit"); break;
					case "op_Equality":  sb.Append ("operator =="); break;
					case "op_Inequality": sb.Append ("operator !="); break;
					case "op_Addition": sb.Append ("operator +"); break;
					case "op_Subtraction": sb.Append ("operator -"); break;
					case "op_Division": sb.Append ("operator /"); break;
					case "op_Multiply": sb.Append ("operator *"); break;
					case "op_Modulus": sb.Append ("operator %"); break;
					case "op_GreaterThan": sb.Append ("operator >"); break;
					case "op_GreaterThanOrEqual": sb.Append ("operator >="); break;
					case "op_LessThan": sb.Append ("operator <"); break;
					case "op_LessThanOrEqual": sb.Append ("operator <="); break;
					case "op_UnaryNegation": sb.Append ("operator -"); break;
					case "op_UnaryPlus": sb.Append ("operator +"); break;
					case "op_Decrement": sb.Append ("operator --"); break;
					case "op_Increment": sb.Append ("operator ++"); break;
					default: Console.WriteLine ("unhandled operator named {0}", method_def.Name); sb.Append (method_def.Name); break;
					}
				}
				else {
					sb.Append (method_def.Name);
				}
			}
			else {
				sb.Append (method_def.Name);
			}
			if (beautify && method_def.GenericParameters.Count > 0) {
				sb.Append ("<");
				bool first_gp = true;
				foreach (GenericParameter gp in method_def.GenericParameters) {
					if (!first_gp)
						sb.Append (',');
					first_gp = false;
					sb.Append (gp.Name);
				}
				sb.Append (">");
			}
			sb.Append ('(');
			bool first_p = true;
			foreach (ParameterDefinition p in method_def.Parameters) {
				if (!first_p)
					sb.Append (", ");
				first_p = false;
				if (p.IsIn)
					sb.Append ("in ");
				else if (p.IsOut)
					sb.Append ("out ");
				sb.Append (beautify
				           ? CecilUtils.PrettyType (p.ParameterType.FullName)
				           : CecilUtils.FormatTypeLikeCorCompare (p.ParameterType.FullName));
				if (beautify) {
					sb.Append (" ");
					sb.Append (p.Name);
				}
			}
			sb.Append (')');

			return sb.ToString();
		}

		MethodDefinition method_def;
		List<CompNamed> attributes;
	}

	public class CecilProperty : CompProperty
	{
		public CecilProperty (PropertyDefinition pd)
			: base (FormatName (pd, false))
		{
			this.pd = pd;
			this.attributes = CecilUtils.GetCustomAttributes (pd, todos);
			this.DisplayName = FormatName (pd, true);
		}

		public override string GetMemberType()
		{
			return CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType.FullName);
		}
		
		public override string GetMemberAccess()
		{
			return pd.Attributes == 0 ? null : pd.Attributes.ToString();
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
		
		public override List<CompNamed> GetMethods()
		{
			List<CompNamed> rv = new List<CompNamed>();

			if (pd.GetMethod != null && !pd.GetMethod.IsPrivate && !pd.GetMethod.IsAssembly)
				rv.Add (new CecilMethod (pd.GetMethod));
			if (pd.SetMethod != null && !pd.SetMethod.IsPrivate && !pd.SetMethod.IsAssembly)
				rv.Add (new CecilMethod (pd.SetMethod));
			
			return rv;
		}

		static string FormatName (PropertyDefinition pd, bool beautify)
		{
			StringBuilder sb = new StringBuilder ();

#if INCLUDE_TYPE_IN_PROPERTY_DISPLAYNAME
			sb.Append (beautify
				           ? CecilUtils.PrettyType (pd.PropertyType.FullName)
				           : CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType.FullName));
			sb.Append (" ");
#else
			if (!beautify) {
				sb.Append (CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType.FullName));
				sb.Append (" ");
			}
#endif
			sb.Append (pd.Name);

			if (pd.Parameters.Count > 0) {
				sb.Append ('[');
				bool first_p = true;
				foreach (ParameterDefinition p in pd.Parameters) {
					if (!first_p)
						sb.Append (", ");
					first_p = false;
					sb.Append (beautify
						   ? CecilUtils.PrettyType (p.ParameterType.FullName)
						   : CecilUtils.FormatTypeLikeCorCompare (p.ParameterType.FullName));
					if (beautify) {
						sb.Append (" ");
						sb.Append (p.Name);
					}
				}
				sb.Append (']');
			}

			return sb.ToString ();
		}
		
		PropertyDefinition pd;
		List<CompNamed> attributes;
	}
	
	public class CecilEvent : CompEvent
	{
		public CecilEvent (EventDefinition ed)
			: base (ed.Name)
		{
			this.ed = ed;
			this.attributes = CecilUtils.GetCustomAttributes (ed, todos);
		}

		public override string GetMemberType()
		{
			return CecilUtils.FormatTypeLikeCorCompare (ed.EventType.FullName);
		}
		
		public override string GetMemberAccess()
		{
			return ed.Attributes == 0 ? "None" : ed.Attributes.ToString();
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
		
		EventDefinition ed;
		List<CompNamed> attributes;
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