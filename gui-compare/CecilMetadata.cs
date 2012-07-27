//
// CecilMetadata.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace GuiCompare {

	static class CecilUtils {

		public static string PrettyType (TypeReference type)
		{
			var gen_instance = type as GenericInstanceType;
			if (gen_instance != null) {
				if (gen_instance.ElementType.FullName == "System.Nullable`1")
					return PrettyType (gen_instance.GenericArguments [0]) + "?";

				var signature = new StringBuilder ();
				signature.Append (PrettyType (gen_instance.ElementType));
				signature.Append ("<");
				for (int i = 0; i < gen_instance.GenericArguments.Count; i++) {
					if (i > 0)
						signature.Append (",");

					signature.Append (PrettyType (gen_instance.GenericArguments [i]));
				}
				signature.Append (">");

				return signature.ToString ();
			}

			var array = type as ArrayType;
			if (array != null)
				return PrettyType (array.ElementType) + "[]";

			var reference = type as ByReferenceType;
			if (reference != null)
				return PrettyType (reference.ElementType) + "&";

			var pointer = type as PointerType;
			if (pointer != null)
				return PrettyType (pointer.ElementType) + "*";

			switch (type.FullName) {
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

			return type.Name;
		}
		
		public static string PrettyTypeDefinition (TypeDefinition td)
		{
			if (td.GenericParameters.Count > 0) {
				int arity_start = td.Name.IndexOf ('`');
				if (arity_start > 0) {
					var sb = new StringBuilder (td.Name);
					sb.Remove (arity_start, td.Name.Length - arity_start);
					sb.Append ("<");
					bool first_gp = true;
					foreach (GenericParameter gp in td.GenericParameters) {
						if (!first_gp)
							sb.Append (',');
						first_gp = false;
						sb.Append (gp.Name);
					}
					sb.Append (">");
				
					return sb.ToString ();
				}
			}
			
			return td.Name;
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
		public static string FormatTypeLikeCorCompare (TypeReference type)
		{
			return type.FullName.Replace ('/', '+')
				.Replace ('<', '[')
				.Replace ('>', ']');
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
				foreach (TypeReference ifc in GetInterfaces (fromDef)) {
					TypeDefinition ifc_def = ifc.Resolve ();
					if (ifc_def == null || ifc_def.IsNotPublic)
						continue;

					interface_list.Add (new CecilInterface (ifc));
				}
			}

			if (constructor_list != null) {
				foreach (MethodDefinition md in fromDef.Methods.Where (m => m.IsConstructor)) {
					if (md.IsPrivate || md.IsAssembly)
						continue;
					constructor_list.Add (new CecilMethod (md));
				}
			}
			if (method_list != null) {
				foreach (MethodDefinition md in fromDef.Methods.Where (m => !m.IsConstructor)) {
					if (md.IsSpecialName) {
						if (!md.Name.StartsWith("op_"))
							continue;
					}

					if (IsFinalizer (md)) {
						string name = md.DeclaringType.Name;
						int arity = name.IndexOf ('`');
						if (arity > 0)
							name = name.Substring (0, arity);

						md.Name = "~" + name;
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

		static IEnumerable<TypeDefinition> WalkHierarchy (TypeReference type)
		{
			for (var definition = type.Resolve (); definition != null; definition = GetBaseType (definition))
				yield return definition;
		}

		static TypeDefinition GetBaseType (TypeDefinition type)
		{
			if (type.BaseType == null)
				return null;

			return type.BaseType.Resolve ();
		}

		static IEnumerable<TypeReference> GetInterfaces (TypeReference type)
		{
			var cache = new Dictionary<string, TypeReference> ();

			foreach (var definition in WalkHierarchy (type))
				foreach (TypeReference iface in definition.Interfaces)
					cache [iface.FullName] = iface;

			return cache.Values;
		}

		static bool IsFinalizer (MethodDefinition method)
		{
			if (method.Name != "Finalize")
				return false;

			if (!method.IsVirtual)
				return false;

			if (method.Parameters.Count != 0)
				return false;

			return true;
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
				else if (type_def.BaseType.FullName == "System.MulticastDelegate") {
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
			foreach (var argument in ca.ConstructorArguments) {
				if (!first)
					sb.Append (", ");
				first = false;
				sb.Append (argument.Value.ToString());
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
			
			return IsTODOAttribute (GetBaseType (typedef));
		}
		
		public static List<CompNamed> GetCustomAttributes (ICustomAttributeProvider provider, List<string> todos)
		{
			List<CompNamed> rv = new List<CompNamed>();
			foreach (CustomAttribute ca in provider.CustomAttributes) {
				TypeDefinition resolved = ca.Constructor.DeclaringType.Resolve ();

				if (resolved != null) {
					if (IsTODOAttribute (resolved)) {
						todos.Add (String.Format ("[{0} ({1})]", ca.Constructor.DeclaringType.Name, CecilUtils.GetTODOText (ca)));					
						continue;
					}

					if (resolved.IsNotPublic)
						continue;
				}

				if (!MasterUtils.IsImplementationSpecificAttribute (ca.Constructor.DeclaringType.FullName))
					rv.Add (new CecilAttribute (ca));
			}
			return rv;
		}
		
		public static List<CompGenericParameter> GetTypeParameters (IGenericParameterProvider provider)
		{
			if (provider.GenericParameters.Count == 0)
				return null;
				
			var l = new List<CompGenericParameter> ();
			
			var gparameters = provider.GenericParameters;
			foreach (GenericParameter gp in gparameters) {
				l.Add (new CecilGenericParameter (gp));
			}			
			
			return l;			
		}
		
		public static List<CompParameter> GetParameters (IMethodSignature provider)
		{
			var l = new List<CompParameter> ();
			foreach (ParameterDefinition pd in provider.Parameters)
			{
				l.Add (new CecilParameter (pd));
			}
					
			return l;
		}
		
		public static readonly IAssemblyResolver Resolver = new DefaultAssemblyResolver();
	}

	public class CecilAssembly : CompAssembly {
		public CecilAssembly (string path)
			: base(Path.GetFileName (path))
		{
			var namespaces = new Dictionary<string, Dictionary<string, TypeDefinition>> ();

			var assembly = AssemblyDefinition.ReadAssembly (path, new ReaderParameters { AssemblyResolver = CecilUtils.Resolver });
			
			foreach (TypeDefinition t in assembly.MainModule.Types) {
				if (t.Name == "<Module>")
					continue;
				
				if (t.IsNotPublic)
					continue;

				if (t.IsSpecialName || t.IsRuntimeSpecialName)
					continue;

				if (CecilUtils.IsTODOAttribute (t))
					continue;

				Dictionary<string, TypeDefinition> ns;

				if (!namespaces.TryGetValue (t.Namespace, out ns)) {
					ns = new Dictionary<string, TypeDefinition> ();
					namespaces.Add (t.Namespace, ns);
				}

				ns[t.Name] = t;
			}

			namespace_list = new List<CompNamed> ();
			foreach (string ns_name in namespaces.Keys)
				namespace_list.Add (new CecilNamespace (ns_name, namespaces[ns_name]));

			attributes = CecilUtils.GetCustomAttributes (assembly, todos);

			// TypeForwardedToAttributes are created by checking if assembly contains
			// extern forwarder types and using them to construct fake custom attributes
			foreach (ExportedType t in assembly.MainModule.ExportedTypes) {
				if (t.IsForwarder)
					attributes.Add (new PseudoCecilAttribute (t));
			}
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
			MemberName = name;
			
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
				else if (type_def.BaseType != null && type_def.BaseType.FullName == "System.MulticastDelegate") {
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
			: base (type_def.Name)
		{
			this.type_def = type_def;
			DisplayName = CecilUtils.PrettyTypeDefinition (type_def);
			
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
			if (!type_def.IsNotPublic || type_def.IsPublic || type_def.IsNestedPublic || type_def.IsNestedFamily ||
			    type_def.IsNestedFamilyAndAssembly || type_def.IsNestedFamilyOrAssembly)
				MemberName = type_def.FullName;
			
			CecilUtils.PopulateMemberLists (type_def,
			                                interfaces,
			                                constructors,
			                                methods,
			                                properties,
			                                fields,
			                                events);
			
			attributes = CecilUtils.GetCustomAttributes (type_def, todos);
			tparams = CecilUtils.GetTypeParameters (type_def);
		}

		public CecilInterface (TypeReference type_ref)
			: base (CecilUtils.FormatTypeLikeCorCompare (type_ref))
		{
			interfaces = new List<CompNamed>();
			constructors = new List<CompNamed>();
			methods = new List<CompNamed>();
			properties = new List<CompNamed>();
			fields = new List<CompNamed>();
			events = new List<CompNamed>();
			
			attributes = new List<CompNamed>();
			tparams = new List<CompGenericParameter>();
		}

		public override string GetBaseType ()
		{
			return (type_def == null || type_def.BaseType == null) ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType);
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
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return tparams;
		}
		
		List<CompNamed> interfaces;
		List<CompNamed> constructors;
		List<CompNamed> methods;
		List<CompNamed> properties;
		List<CompNamed> fields;
		List<CompNamed> events;
		List<CompNamed> attributes;
		List<CompGenericParameter> tparams;
		TypeDefinition type_def;
	}

	public class CecilDelegate : CompDelegate {
		public CecilDelegate (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;
			DisplayName = CecilUtils.PrettyTypeDefinition (type_def);

			if (!type_def.IsNotPublic || type_def.IsPublic || type_def.IsNestedPublic || type_def.IsNestedFamily ||
			    type_def.IsNestedFamilyAndAssembly || type_def.IsNestedFamilyOrAssembly)
				MemberName = type_def.FullName;
				
			attributes = CecilUtils.GetCustomAttributes (type_def, todos);				
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}

		public override string GetBaseType ()
		{
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType);
		}
		
		public override List<CompNamed> GetConstructors ()
		{
			List<CompNamed> l = new List<CompNamed> ();
			foreach (MethodDefinition md in type_def.Methods) {
				if (md.IsConstructor)
					l.Add (new CecilMethod (md));
			}
			
			return l;
		}
		
		public override List<CompNamed> GetMethods ()
		{
			List<CompNamed> l = new List<CompNamed> ();
			foreach (MethodDefinition md in type_def.Methods) {
				if (!md.IsConstructor)
					l.Add (new CecilMethod (md));
			}
			
			return l;
		}
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return CecilUtils.GetTypeParameters (type_def);
		}
		
		TypeDefinition type_def;
		List<CompNamed> attributes;
	}

	public class CecilEnum : CompEnum {
		public CecilEnum (TypeDefinition type_def)
			: base (type_def.Name)
		{
			this.type_def = type_def;

			fields = new List<CompNamed>();
			if (!type_def.IsNotPublic || type_def.IsPublic || type_def.IsNestedPublic || type_def.IsNestedFamily ||
			    type_def.IsNestedFamilyAndAssembly || type_def.IsNestedFamilyOrAssembly)
				MemberName = type_def.FullName;
			
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
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType);
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
			DisplayName = CecilUtils.PrettyTypeDefinition (type_def);

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

			if (!type_def.IsNotPublic || type_def.IsPublic || type_def.IsNestedPublic || type_def.IsNestedFamily ||
			    type_def.IsNestedFamilyAndAssembly || type_def.IsNestedFamilyOrAssembly)
				MemberName = type_def.FullName;
			
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
			return type_def.BaseType == null ? null : CecilUtils.FormatTypeLikeCorCompare (type_def.BaseType);
		}
		
		public override bool IsAbstract { get { return type_def.IsAbstract; } }
		public override bool IsSealed { get { return type_def.IsSealed; } }

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
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return CecilUtils.GetTypeParameters (type_def);
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
			if (field_def.IsPublic || field_def.IsFamily || field_def.IsFamilyAndAssembly || field_def.IsFamilyOrAssembly) {
				TypeDefinition declType = field_def.DeclaringType;
				if (declType != null)
					MemberName = declType.FullName + "." + field_def.Name;
			}
		}

		public override string GetMemberType ()
		{
			return CecilUtils.FormatTypeLikeCorCompare (field_def.FieldType);
		}
		
		const FieldAttributes masterInfoFieldMask = (FieldAttributes.FieldAccessMask | 
		                                             FieldAttributes.Static | 
		                                             FieldAttributes.InitOnly | 
		                                             FieldAttributes.Literal | 
		                                             FieldAttributes.HasDefault | 
		                                             FieldAttributes.HasFieldMarshal |
		                                             FieldAttributes.NotSerialized );
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
			if (field_def.IsLiteral && field_def.Constant != null) {
				if (field_def.Constant is char)
					return string.Format (CultureInfo.InvariantCulture, (char) field_def.Constant < 0x80 ? "\\x{0:X02}" : "\\u{0:X04}", (int)(char) field_def.Constant);
				
				return Convert.ToString (field_def.Constant, CultureInfo.InvariantCulture);
			}
			
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
			if (method_def.IsFamily || method_def.IsFamilyAndAssembly || method_def.IsFamilyOrAssembly || method_def.IsPublic) {
				TypeReference declType = method_def.DeclaringType;
				if (declType != null)
					MemberName = declType.FullName + "." + method_def.Name;
			}
		}

		public override string GetMemberType ()
		{
			if (method_def.IsConstructor)
				return null;
			
			return CecilUtils.FormatTypeLikeCorCompare (method_def.ReturnType);
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
//		                                               MethodAttributes.HasSecurity |
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
		
		public override List<CompGenericParameter> GetTypeParameters ()
		{
			return CecilUtils.GetTypeParameters (method_def);
		}
		
		public override List<CompParameter> GetParameters ()
		{
			return CecilUtils.GetParameters (method_def);
		}
		
		static string FormatName (MethodDefinition method_def, bool beautify)
		{
			StringBuilder sb = new StringBuilder ();
			if (!method_def.IsConstructor)
				sb.Append (beautify
				           ? CecilUtils.PrettyType (method_def.ReturnType)
				           : CecilUtils.FormatTypeLikeCorCompare (method_def.ReturnType));
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
					case "op_BitwiseAnd": sb.Append ("operator &"); break;
					case "op_BitwiseOr": sb.Append ("operator |"); break;
					case "op_ExclusiveOr": sb.Append ("operator ^"); break;
					case "op_LogicalNot": sb.Append ("operator !"); break;
					case "op_OnesComplement": sb.Append ("operator ~"); break;
					case "op_True": sb.Append ("operator true"); break;
					case "op_False": sb.Append ("operator false"); break;
					case "op_LeftShift": sb.Append ("operator <<"); break;
					case "op_RightShift": sb.Append ("operator >>"); break;
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
				TypeReference paramType = p.ParameterType;
				if (!first_p)
					sb.Append (", ");
				first_p = false;
				if (p.IsIn)
					sb.Append ("in ");
				else if (paramType.IsByReference) {
					if (beautify) {
						sb.Append (p.IsOut ? "out " : "ref ");
						paramType = paramType.GetElementType ();
					} else if (p.IsOut) {
						sb.Append ("out ");
					}
				} else if (p.IsOut) {
					sb.Append ("out ");
				}
				sb.Append (beautify
				           ? CecilUtils.PrettyType (paramType)
				           : CecilUtils.FormatTypeLikeCorCompare (p.ParameterType));
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

			MethodDefinition getMethod = pd.GetMethod, setMethod = pd.SetMethod;			
			if (getMethod != null || setMethod != null) {
				bool interesting = false;

				if (getMethod != null && (getMethod.IsPublic || getMethod.IsFamily || getMethod.IsFamilyAndAssembly || getMethod.IsFamilyOrAssembly))
					interesting = true;
				else if (setMethod != null && (setMethod.IsPublic || setMethod.IsFamily || setMethod.IsFamilyAndAssembly || setMethod.IsFamilyOrAssembly))
					interesting = true;

				if (interesting) {
					TypeDefinition declType = pd.DeclaringType;
					if (declType != null)
						MemberName = declType.FullName + "." + pd.Name;
				}
			}
		}

		public override string GetMemberType()
		{
			return CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType);
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
				           ? CecilUtils.PrettyType (pd.PropertyType)
				           : CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType));
			sb.Append (" ");
#else
			if (!beautify) {
				sb.Append (CecilUtils.FormatTypeLikeCorCompare (pd.PropertyType));
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
						   ? CecilUtils.PrettyType (p.ParameterType)
						   : CecilUtils.FormatTypeLikeCorCompare (p.ParameterType));
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

			MethodDefinition addMethod = ed.AddMethod, removeMethod = ed.RemoveMethod;
			if (addMethod != null || removeMethod != null) {
				bool interesting = false;

				if (addMethod != null && (addMethod.IsPublic || addMethod.IsFamily || addMethod.IsFamilyAndAssembly || addMethod.IsFamilyOrAssembly))
					interesting = true;
				else if (removeMethod != null && (removeMethod.IsPublic || removeMethod.IsFamily || removeMethod.IsFamilyAndAssembly || removeMethod.IsFamilyOrAssembly))
					interesting = true;

				if (interesting) {
					TypeDefinition declType = ed.DeclaringType;
					if (declType != null)
						MemberName = declType.FullName + "." + ed.Name;
				}
			}
		}

		public override string GetMemberType()
		{
			return CecilUtils.FormatTypeLikeCorCompare (ed.EventType);
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
			Dictionary<string, object> attribute_mapping = CreateAttributeMapping (ca);

			foreach (string name in attribute_mapping.Keys) {
				if (name == "TypeId")
					continue;

				object o = attribute_mapping[name];
				Properties.Add (name, o == null ? "null" : o.ToString ());
			}
		}

		static Dictionary<string, object> CreateAttributeMapping (CustomAttribute attribute)
		{
			var mapping = new Dictionary<string, object> ();

			PopulateMapping (mapping, attribute);

			var constructor = attribute.Constructor.Resolve ();
			if (constructor == null || constructor.Parameters.Count == 0)
				return mapping;

			PopulateMapping (mapping, constructor, attribute);

			return mapping;
		}

		static void PopulateMapping (Dictionary<string, object> mapping, CustomAttribute attribute)
		{
			foreach (var named_argument in attribute.Properties) {
				var name = named_argument.Name;
				var arg = named_argument.Argument;

				if (arg.Value is CustomAttributeArgument)
					arg = (CustomAttributeArgument) arg.Value;

				mapping.Add (name, GetArgumentValue (arg.Type, arg.Value));
			}
		}

		static Dictionary<FieldReference, int> CreateArgumentFieldMapping (MethodDefinition constructor)
		{
			Dictionary<FieldReference, int> field_mapping = new Dictionary<FieldReference, int> ();

			int? argument = null;

			foreach (Instruction instruction in constructor.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ldarg_1:
					argument = 1;
					break;
				case Code.Ldarg_2:
					argument = 2;
					break;
				case Code.Ldarg_3:
					argument = 3;
					break;
				case Code.Ldarg:
				case Code.Ldarg_S:
					argument = ((ParameterDefinition) instruction.Operand).Index + 1;
					break;

				case Code.Stfld:
					FieldReference field = (FieldReference) instruction.Operand;
					if (field.DeclaringType.FullName != constructor.DeclaringType.FullName)
						continue;

					if (!argument.HasValue)
						break;

					if (!field_mapping.ContainsKey (field))
						field_mapping.Add (field, (int) argument - 1);

					argument = null;
					break;
				}
			}

			return field_mapping;
		}

		static Dictionary<PropertyDefinition, FieldReference> CreatePropertyFieldMapping (TypeDefinition type)
		{
			Dictionary<PropertyDefinition, FieldReference> property_mapping = new Dictionary<PropertyDefinition, FieldReference> ();

			foreach (PropertyDefinition property in type.Properties) {
				if (property.GetMethod == null)
					continue;
				if (!property.GetMethod.HasBody)
					continue;

				foreach (Instruction instruction in property.GetMethod.Body.Instructions) {
					if (instruction.OpCode.Code != Code.Ldfld)
						continue;

					FieldReference field = (FieldReference) instruction.Operand;
					if (field.DeclaringType.FullName != type.FullName)
						continue;

					property_mapping.Add (property, field);
					break;
				}
			}

			return property_mapping;
		}

		static void PopulateMapping (Dictionary<string, object> mapping, MethodDefinition constructor, CustomAttribute attribute)
		{
			if (!constructor.HasBody)
				return;

			if (constructor.DeclaringType.FullName == "System.Runtime.CompilerServices.DecimalConstantAttribute") {
				var ca = attribute.ConstructorArguments;
				var dca = constructor.Parameters[2].ParameterType == constructor.Module.TypeSystem.Int32 ?
					new DecimalConstantAttribute ((byte) ca[0].Value, (byte) ca[1].Value, (int) ca[2].Value, (int) ca[3].Value, (int) ca[4].Value) :
					new DecimalConstantAttribute ((byte) ca[0].Value, (byte) ca[1].Value, (uint) ca[2].Value, (uint) ca[3].Value, (uint) ca[4].Value);

				mapping.Add ("Value", dca.Value);
				return;
			}

			var field_mapping = CreateArgumentFieldMapping (constructor);
			var property_mapping = CreatePropertyFieldMapping ((TypeDefinition) constructor.DeclaringType);

			foreach (var pair in property_mapping) {
				int argument;
				if (!field_mapping.TryGetValue (pair.Value, out argument))
					continue;

				var ca_arg = attribute.ConstructorArguments[argument];
				if (ca_arg.Value is CustomAttributeArgument)
					ca_arg = (CustomAttributeArgument) ca_arg.Value;
				
				mapping[pair.Key.Name] = GetArgumentValue (ca_arg.Type, ca_arg.Value);
			}
		}

		static object GetArgumentValue (TypeReference reference, object value)
		{
			var type = reference.Resolve ();
			if (type == null)
				return value;

			if (type.IsEnum) {
				if (IsFlaggedEnum (type))
					return GetFlaggedEnumValue (type, value);

				return GetEnumValue (type, value);
			}

			return value;
		}

		static bool IsFlaggedEnum (TypeDefinition type)
		{
			if (!type.IsEnum)
				return false;

			if (type.CustomAttributes.Count == 0)
				return false;

			foreach (CustomAttribute attribute in type.CustomAttributes)
				if (attribute.Constructor.DeclaringType.FullName == "System.FlagsAttribute")
					return true;

			return false;
		}

		static object GetFlaggedEnumValue (TypeDefinition type, object value)
		{
			long flags = Convert.ToInt64 (value);
			var signature = new StringBuilder ();

			for (int i = type.Fields.Count - 1; i >= 0; i--) {
				FieldDefinition field = type.Fields[i];

				if (!field.HasConstant)
					continue;

				long flag = Convert.ToInt64 (field.Constant);

				if (flag == 0)
					continue;

				if ((flags & flag) == flag) {
					if (signature.Length != 0)
						signature.Append (", ");

					signature.Append (field.Name);
					flags -= flag;
				}
			}

			return signature.ToString ();
		}

		static object GetEnumValue (TypeDefinition type, object value)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (!field.HasConstant)
					continue;

				if (Comparer.Default.Compare (field.Constant, value) == 0)
					return field.Name;
			}

			return value;
		}
	}

	public class PseudoCecilAttribute : CompAttribute
	{
		public PseudoCecilAttribute (ExportedType type)
			: base (typeof (TypeForwardedToAttribute).FullName)
		{
			ExtraInfo = "[assembly: TypeForwardedToAttribute (typeof (" + type.ToString () + "))]";
		}
	}
	
	public class CecilGenericParameter : CompGenericParameter
	{
		List<CompNamed> attributes;	
		IList<TypeReference> constraints;
		
		public CecilGenericParameter (GenericParameter gp)
			: base (gp.Name, gp.Attributes)
		{
			attributes = CecilUtils.GetCustomAttributes (gp, todos);
			
			var constraints = gp.Constraints;
			if (constraints.Count == 0)
				return;
			
			// TODO: finish constraints loading
			this.constraints = constraints;
		}
		
		public override bool HasConstraints {
			get {
				return constraints != null;
			}
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
	}
	
	public class CecilParameter : CompParameter
	{
		List<CompNamed> attributes;
		
		public CecilParameter (ParameterDefinition pd)
			: base (pd.Name, CecilUtils.FormatTypeLikeCorCompare (pd.ParameterType), pd.IsOptional)
		{
			attributes = CecilUtils.GetCustomAttributes (pd, todos);
		}
		
		public override List<CompNamed> GetAttributes ()
		{
			return attributes;
		}
	}
}
