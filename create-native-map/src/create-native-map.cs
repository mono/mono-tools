//
// create-native-map.cs: Builds a C map of constants defined on C# land
//
// Authors:
//  Miguel de Icaza (miguel@novell.com)
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Novell, Inc.
// (C) 2004-2005 Jonathan Pryor
//

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
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Mono.Unix.Native;

delegate void CreateFileHandler (string assembly_name, string file_prefix);
delegate void AssemblyAttributesHandler (Assembly assembly);
delegate void TypeHandler (Type t, string ns, string fn);
delegate void CloseFileHandler (string file_prefix);

class MakeMap {

	public static int Main (string [] args)
	{
		FileGenerator[] generators = new FileGenerator[]{
			new HeaderFileGenerator (),
			new SourceFileGenerator (),
			new ConvertFileGenerator (),
			new ConvertDocFileGenerator (),
		};

		Configuration config = new Configuration ();
		bool exit = false;
		try {
			exit = !config.Parse (args);
		}
		catch (Exception e) {
			Console.WriteLine ("{0}: error: {1}", 
					Environment.GetCommandLineArgs () [0], e.Message);
			exit = true;
		}
		if (exit) {
			Configuration.ShowHelp ();
			return 1;
		}

		MapUtils.config = config;

		MakeMap composite = new MakeMap ();
		foreach (FileGenerator g in generators) {
			g.Configuration = config;
			composite.FileCreators += new CreateFileHandler (g.CreateFile);
			composite.AssemblyAttributesHandler += 
				new AssemblyAttributesHandler (g.WriteAssemblyAttributes);
			composite.TypeHandler += new TypeHandler (g.WriteType);
			composite.FileClosers += new CloseFileHandler (g.CloseFile);
		}

		return composite.Run (config);
	}

	event CreateFileHandler FileCreators;
	event AssemblyAttributesHandler AssemblyAttributesHandler;
	event TypeHandler TypeHandler;
	event CloseFileHandler FileClosers;

	int Run (Configuration config)
	{
		FileCreators (config.AssemblyFileName, config.OutputPrefix);

		Assembly assembly = Assembly.LoadFrom (config.AssemblyFileName);
		AssemblyAttributesHandler (assembly);
		
		Type [] exported_types = assembly.GetTypes ();
		Array.Sort (exported_types, new TypeFullNameComparer ());
			
		foreach (Type t in exported_types) {
			string ns = MapUtils.GetNamespace (t);
			/*
			if (ns == null || !ns.StartsWith ("Mono"))
				continue;
			 */
			string fn = MapUtils.GetManagedType (t);

			TypeHandler (t, ns, fn);
		}
		FileClosers (config.OutputPrefix);

		return 0;
	}

	private class TypeFullNameComparer : IComparer<Type> {
		public int Compare (Type t1, Type t2)
		{
			if (t1 == t2)
				return 0;
			if (t1 == null)
				return 1;
			if (t2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (
					t1.FullName, t2.FullName, CompareOptions.Ordinal);
		}
	}
}

class Configuration {
	Dictionary<string, string> renameMembers = new Dictionary<string, string> ();
	Dictionary<string, string> renameNamespaces = new Dictionary<string, string> ();
	List<string> libraries = new List<string>();
	List<string> optionals = new List<string> ();
	List<string> excludes  = new List<string> ();
	List<string> iheaders  = new List<string> ();
	List<string> pheaders  = new List<string> ();
	List<string> imacros   = new List<string> ();
	List<string> pmacros   = new List<string> ();
	string assembly_name;
	string output;

	delegate void ArgumentHandler (Configuration c, string name, string value);
	static Dictionary<string, ArgumentHandler> handlers;

	static Configuration ()
	{
		handlers = new Dictionary <string, ArgumentHandler> ();
		handlers ["autoconf-header"] = delegate (Configuration c, string name, string value) {
			c.iheaders.Add ("ah:" + name);
		};
		handlers ["autoconf-member"] = delegate (Configuration c, string name, string value) {
			c.optionals.Add (name);
		};
		handlers ["impl-header"]  = delegate (Configuration c, string name, string value) {
			c.iheaders.Add (name);
		};
		handlers ["impl-macro"] = delegate (Configuration c, string name, string value) {
			if (value != null)
				name += "=" + value;
			c.imacros.Add (name);
		};
		handlers ["library"] = delegate (Configuration c, string name, string value) {
			c.libraries.Add (name);
		};
		handlers ["exclude-native-symbol"] = delegate (Configuration c, string name, string value) {
			c.excludes.Add (name);
		};
		handlers ["public-header"] = delegate (Configuration c, string name, string value) {
			c.pheaders.Add (name);
		};
		handlers ["public-macro"] = delegate (Configuration c, string name, string value) {
			if (value != null)
				name += "=" + value;
			c.pmacros.Add (name);
		};
		handlers ["rename-member"] = delegate (Configuration c, string name, string value) {
			if (value == null) {
				throw new Exception ("missing rename value");
			}
			c.renameMembers [name] = value;
		};
		handlers ["rename-namespace"] = delegate (Configuration c, string name, string value) {
			if (value == null) {
				throw new Exception ("missing rename value");
			}
			value = value.Replace (".", "_");
			c.renameNamespaces [name] = value;
		};
	}

	public Configuration ()
	{
	}

	public List<string> NativeLibraries {
		get {return libraries;}
	}

	public List<string> AutoconfMembers {
		get {return optionals;}
	}

	public List<string> NativeExcludeSymbols {
		get {return excludes;}
	}

	public List<string> PublicHeaders {
		get {return pheaders;}
	}

	public List<string> PublicMacros {
		get {return pmacros;}
	}

	public List<string> ImplementationHeaders {
		get {return iheaders;}
	}

	public List<string> ImplementationMacros {
		get {return imacros;}
	}

	public IDictionary<string, string> MemberRenames {
		get {return renameMembers;}
	}

	public IDictionary<string, string> NamespaceRenames {
		get {return renameNamespaces;}
	}

	public string AssemblyFileName {
		get {return assembly_name;}
	}

	public string OutputPrefix {
		get {return output;}
	}

	const string NameValue = @"(?<Name>[^=]+)(=(?<Value>.*))?";
	const string Argument  = @"^--(?<Argument>[\w-]+)([=:]" + NameValue + ")?$";

	public bool Parse (string[] args)
	{
		Regex argRE = new Regex (Argument);
		Regex valRE = new Regex (NameValue);

		for (int i = 0; i < args.Length; ++i) {
			Match m = argRE.Match (args [i]);
			if (m.Success) {
				string arg = m.Groups ["Argument"].Value;
				if (arg == "help")
					return false;
				if (!m.Groups ["Name"].Success) {
					if ((i+1) >= args.Length)
						throw new Exception (
								string.Format ("missing value for argument {0}", args [i]));
					m = valRE.Match (args [++i]);
					if (!m.Success) {
						throw new Exception (
								string.Format ("invalid value for argument {0}: {1}", 
									args [i-1], args[i]));
					}
				}
				string name  = m.Groups ["Name"].Value;
				string value = m.Groups ["Value"].Success ? m.Groups ["Value"].Value : null;
				if (handlers.ContainsKey (arg)) {
					handlers [arg] (this, name, value);
				}
				else {
					throw new Exception ("invalid argument " +  arg);
				}
			}
			else if (assembly_name == null) {
				assembly_name = args [i];
			}
			else {
				output = args [i];
			}
		}

		if (assembly_name == null)
			throw new Exception ("missing ASSEMBLY");
		if (output == null)
			throw new Exception ("missing OUTPUT-PREFIX");

		libraries.Sort ();
		optionals.Sort ();
		excludes.Sort ();

		return true;
	}

	public static void ShowHelp ()
	{
		Console.WriteLine (
				"Usage: create-native-map \n" +
				"\t[--autoconf-header=HEADER]* \n" +
				"\t[--autoconf-member=MEMBER]* \n" +
				"\t[--exclude-native-symbol=SYMBOL]*\n" +
				"\t[--impl-header=HEADER]* \n" +
				"\t[--impl-macro=MACRO]* \n" +
				"\t[--library=LIBRARY]+ \n" + 
				"\t[--public-header=HEADER]* \n" +
				"\t[--public-macro=MACRO]* \n" +
				"\t[--rename-member=FROM=TO]* \n" + 
				"\t[--rename-namespace=FROM=TO]*\n" +
				"\tASSEMBLY OUTPUT-PREFIX"
		);
	}
}

static class MapUtils {
	internal static Configuration config;

	public static T GetCustomAttribute <T> (MemberInfo element) where T : Attribute
	{
		return (T) Attribute.GetCustomAttribute (element, typeof(T), true);
	}

	public static T GetCustomAttribute <T> (Assembly assembly) where T : Attribute
	{
		return (T) Attribute.GetCustomAttribute (assembly, typeof(T), true);
	}

	public static T[] GetCustomAttributes <T> (MemberInfo element) where T : Attribute
	{
		return (T[]) Attribute.GetCustomAttributes (element, typeof(T), true);
	}

	public static T[] GetCustomAttributes <T> (Assembly assembly) where T : Attribute
	{
		return (T[]) Attribute.GetCustomAttributes (assembly, typeof(T), true);
	}

	public static MapAttribute GetMapAttribute (ICustomAttributeProvider element)
	{
		foreach (object o in element.GetCustomAttributes (true)) {
			if (!IsMapAttribute (o))
				continue;
			string nativeType   = GetPropertyValueAsString (o, "NativeType");
			MapAttribute map = nativeType == null 
				? new MapAttribute () 
				: new MapAttribute (nativeType);
			map.SuppressFlags   = GetPropertyValueAsString (o, "SuppressFlags");
			return map;
		}
		return null;
	}

	private static bool IsMapAttribute (object o)
	{
		Type t = o.GetType ();
		do {
			if (t.Name == "MapAttribute") {
				return true;
			}
			t = t.BaseType;
		} while (t != null);
		return false;
	}

	private static string GetPropertyValueAsString (object o, string property)
	{
		object v = GetPropertyValue (o, property);
		string s = v == null ? null : v.ToString ();
		if (s != null)
			return s.Length == 0 ? null : s;
		return null;
	}

	private static object GetPropertyValue (object o, string property)
	{
		PropertyInfo p = o.GetType().GetProperty (property);
		if (p == null)
			return null;
		if (!p.CanRead)
			return null;
		return p.GetValue (o, new object[0]);
	}

	public static bool IsIntegralType (Type t)
	{
		return t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) ||
			t == typeof(short) || t == typeof(ushort) || 
			t == typeof(int) || t == typeof(uint) || 
			t == typeof(long) || t == typeof(ulong);
	}

	public static bool IsBlittableType (Type t)
	{
		return IsIntegralType (t) || t == typeof(IntPtr) || t == typeof(UIntPtr);
	}

	public static string GetNativeType (Type t)
	{
		Type et = GetElementType (t);
		string ut = et.Name;
		if (et.IsEnum)
			ut = Enum.GetUnderlyingType (et).Name;

		string type = null;

		switch (ut) {
			case "Boolean":       type = "int";             break;
			case "Byte":          type = "unsigned char";   break;
			case "SByte":         type = "signed char";     break;
			case "Int16":         type = "short";           break;
			case "UInt16":        type = "unsigned short";  break;
			case "Int32":         type = "int";             break;
			case "UInt32":        type = "unsigned int";    break;
			case "Int64":         type = "gint64";          break;
			case "UInt64":        type = "guint64";         break;
			case "IntPtr":        type = "void*";           break;
			case "UIntPtr":       type = "void*";           break;
			case "String":        type = "const char";      break; /* ref type */
			case "StringBuilder": type = "char";            break; /* ref type */
			case "Void":          type = "void";            break;
			case "HandleRef":     type = "void*";           break;
		}
		bool isDelegate = IsDelegate (t);
		if (type == null)
			type = isDelegate ? t.Name : GetStructName (t);
		if (!et.IsValueType && !isDelegate) {
			type += "*";
		}
		while (t.HasElementType) {
			t = t.GetElementType ();
			type += "*";
		}
		return type;
		//return (t.IsByRef || t.IsArray || (!t.IsValueType && !isDelegate)) ? type + "*" : type;
	}

	public static bool IsDelegate (Type t)
	{
		return typeof(Delegate).IsAssignableFrom (t);
	}

	private static string GetStructName (Type t)
	{
		t = GetElementType (t);
		return "struct " + GetManagedType (t);
	}

	public static Type GetElementType (Type t)
	{
		while (t.HasElementType) {
			t = t.GetElementType ();
		}
		return t;
	}

	public static string GetNamespace (Type t)
	{
		if (t.Namespace == null)
			return "";
		if (config.NamespaceRenames.ContainsKey (t.Namespace))
			return config.NamespaceRenames [t.Namespace];
		return t.Namespace.Replace ('.', '_');
	}

	public static string GetManagedType (Type t)
	{
		string ns = GetNamespace (t);
		string tn = 
			(t.DeclaringType != null ? t.DeclaringType.Name + "_" : "") + t.Name;
		return ns + "_" + tn;
	}

	public static string GetNativeType (FieldInfo field)
	{
		MapAttribute map = 
			GetMapAttribute (field)
			??
			GetMapAttribute (field.FieldType);
		if (map != null)
			return map.NativeType;
		return null;
	}

	public static string GetFunctionDeclaration (string name, MethodInfo method)
	{
		StringBuilder sb = new StringBuilder ();
#if false
		Console.WriteLine (t);
		foreach (object o in t.GetMembers ())
			Console.WriteLine ("\t" + o);
#endif
		sb.Append (method.ReturnType == typeof(string) 
				? "char*" 
				: MapUtils.GetNativeType (method.ReturnType));
		sb.Append (" ").Append (name).Append (" (");


		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0) {
			sb.Append ("void");
		}
		else {
			if (parameters.Length > 0) {
				WriteParameterDeclaration (sb, parameters [0]);
			}
			for (int i = 1; i < parameters.Length; ++i) {
				sb.Append (", ");
				WriteParameterDeclaration (sb, parameters [i]);
			}
		}
		sb.Append (")");
		return sb.ToString ();
	}

	private static void WriteParameterDeclaration (StringBuilder sb, ParameterInfo pi)
	{
		// DumpTypeInfo (pi.ParameterType);
		string nt = GetNativeType (pi.ParameterType);
		sb.AppendFormat ("{0} {1}", nt, pi.Name);
	}

	internal class _MemberNameComparer : IComparer<MemberInfo>, IComparer <FieldInfo> {
		public int Compare (FieldInfo m1, FieldInfo m2)
		{
			return Compare ((MemberInfo) m1, (MemberInfo) m2);
		}

		public int Compare (MemberInfo m1, MemberInfo m2)
		{
			if (m1 == m2)
				return 0;
			if (m1 == null)
				return 1;
			if (m2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (
					m1.Name, m2.Name, CompareOptions.Ordinal);
		}
	}

	private class _OrdinalStringComparer : IComparer<string> {
		public int Compare (string s1, string s2)
		{
			if (object.ReferenceEquals (s1, s2))
				return 0;
			if (s1 == null)
				return 1;
			if (s2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s2, 
					CompareOptions.Ordinal);
		}
	}

	internal static _MemberNameComparer MemberNameComparer = new _MemberNameComparer ();
	internal static IComparer<string> OrdinalStringComparer = new _OrdinalStringComparer ();
}

abstract class FileGenerator {
	private Configuration config;

	public Configuration Configuration {
		get {return config;}
		set {config = value;}
	}

	public abstract void CreateFile (string assembly_name, string file_prefix);

	public virtual void WriteAssemblyAttributes (Assembly assembly)
	{
	}

	public abstract void WriteType (Type t, string ns, string fn);
	public abstract void CloseFile (string file_prefix);

	protected static void WriteHeader (StreamWriter s, string assembly)
	{
		WriteHeader (s, assembly, false);
	}

	protected static void WriteHeader (StreamWriter s, string assembly, bool noConfig)
	{
		s.WriteLine (
			"/*\n" +
			" * This file was automatically generated by create-native-map from {0}.\n" +
			" *\n" +
			" * DO NOT MODIFY.\n" +
			" */",
			assembly);
		if (!noConfig) {
			s.WriteLine ("#ifdef HAVE_CONFIG_H");
			s.WriteLine ("#include <config.h>");
			s.WriteLine ("#endif /* ndef HAVE_CONFIG_H */");
		}
		s.WriteLine ();
	}

	protected static bool CanMapType (Type t)
	{
		return MapUtils.GetMapAttribute (t) != null;
	}

	protected static bool IsFlagsEnum (Type t)
	{
		return t.IsEnum && 
			MapUtils.GetCustomAttributes <FlagsAttribute> (t).Length > 0;
	}

	protected static void SortFieldsInOffsetOrder (Type t, FieldInfo[] fields)
	{
		Array.Sort (fields, delegate (FieldInfo f1, FieldInfo f2) {
				long o1 = (long) Marshal.OffsetOf (f1.DeclaringType, f1.Name);
				long o2 = (long) Marshal.OffsetOf (f2.DeclaringType, f2.Name);
				return o1.CompareTo (o2);
		});
	}

	protected static void WriteMacroDefinition (TextWriter writer, string macro)
	{
		if (macro == null || macro.Length == 0)
			return;
		string[] val = macro.Split ('=');
		writer.WriteLine ("#ifndef {0}", val [0]);
		writer.WriteLine ("#define {0}{1}", val [0], 
				val.Length > 1 ? " " + val [1] : "");
		writer.WriteLine ("#endif /* ndef {0} */", val [0]);
		writer.WriteLine ();
	}

	private static Regex includeRegex = new Regex (@"^(?<AutoHeader>ah:)?(?<Include>(""|<)(?<IncludeFile>.*)(""|>))$");

	protected static void WriteIncludeDeclaration (TextWriter writer, string inc)
	{
		if (inc == null || inc.Length == 0)
			return;
		Match m = includeRegex.Match (inc);
		if (!m.Groups ["Include"].Success) {
			Console.WriteLine ("warning: invalid PublicIncludeFile: {0}", inc);
			return;
		}
		if (m.Success && m.Groups ["AutoHeader"].Success) {
			string i = m.Groups ["IncludeFile"].Value;
			string def = "HAVE_" + i.ToUpper ().Replace ("/", "_").Replace (".", "_");
			writer.WriteLine ("#ifdef {0}", def);
			writer.WriteLine ("#include {0}", m.Groups ["Include"]);
			writer.WriteLine ("#endif /* ndef {0} */", def);
		}
		else
			writer.WriteLine ("#include {0}", m.Groups ["Include"]);
	}

	protected string GetNativeMemberName (FieldInfo field)
	{
		if (!Configuration.MemberRenames.ContainsKey (field.Name))
			return field.Name;
		return Configuration.MemberRenames [field.Name];
	}
}

class HeaderFileGenerator : FileGenerator {
	StreamWriter sh;
	string assembly_file;
	Dictionary<string, MethodInfo>  methods   = new Dictionary <string, MethodInfo> ();
	Dictionary<string, Type>        structs   = new Dictionary <string, Type> ();
	Dictionary<string, MethodInfo>  delegates = new Dictionary <string, MethodInfo> ();
	List<string>                    decls     = new List <string> ();

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		sh = File.CreateText (file_prefix + ".h");
		file_prefix = file_prefix.Replace ("../", "").Replace ("/", "_");
		this.assembly_file = assembly_name = Path.GetFileName (assembly_name);
		WriteHeader (sh, assembly_name, true);
		assembly_name = assembly_name.Replace (".dll", "").Replace (".", "_");
		sh.WriteLine ("#ifndef INC_" + assembly_name + "_" + file_prefix + "_H");
		sh.WriteLine ("#define INC_" + assembly_name + "_" + file_prefix + "_H\n");
		sh.WriteLine ("#include <glib.h>\n");
		sh.WriteLine ("G_BEGIN_DECLS\n");

		// Kill warning about unused method
		DumpTypeInfo (null);
	}

	public override void WriteAssemblyAttributes (Assembly assembly)
	{
		sh.WriteLine ("/*\n * Public Macros\n */");
		foreach (string def in Configuration.PublicMacros) {
			WriteMacroDefinition (sh, def);
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Public Includes\n */");
		foreach (string inc in Configuration.PublicHeaders) {
			WriteIncludeDeclaration (sh, inc);
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Enumerations\n */");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		WriteEnum (t, ns, fn);
		CacheStructs (t, ns, fn);
		CacheExternalMethods (t, ns, fn);
	}

	private void WriteEnum (Type t, string ns, string fn)
	{
		if (!CanMapType (t) || !t.IsEnum)
			return;

		string etype = MapUtils.GetNativeType (t);

		WriteLiteralValues (sh, t, fn);
		sh.WriteLine ("int {1}_From{2} ({0} x, {0} *r);", etype, ns, t.Name);
		sh.WriteLine ("int {1}_To{2} ({0} x, {0} *r);", etype, ns, t.Name);
		Configuration.NativeExcludeSymbols.Add (
				string.Format ("{1}_From{2}", etype, ns, t.Name));
		Configuration.NativeExcludeSymbols.Add (
				string.Format ("{1}_To{2}", etype, ns, t.Name));
		Configuration.NativeExcludeSymbols.Sort ();
		sh.WriteLine ();
	}

	static void WriteLiteralValues (StreamWriter sh, Type t, string n)
	{
		object inst = Activator.CreateInstance (t);
		int max_field_length = 0;
		FieldInfo[] fields = t.GetFields ();
		Array.Sort (fields, delegate (FieldInfo f1, FieldInfo f2) {
				max_field_length = Math.Max (max_field_length, f1.Name.Length);
				max_field_length = Math.Max (max_field_length, f2.Name.Length);
				return MapUtils.MemberNameComparer.Compare (f1, f2);
		});
		max_field_length += 1 + n.Length;
		sh.WriteLine ("enum {0} {{", n);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			string e = n + "_" + fi.Name;
			sh.WriteLine ("\t{0,-" + max_field_length + "}       = 0x{1:x},", 
					e, fi.GetValue (inst));
			sh.WriteLine ("\t#define {0,-" + max_field_length + "} {0}", e);
		}
		sh.WriteLine ("};");
	}


	private void CacheStructs (Type t, string ns, string fn)
	{
		if (t.IsEnum)
			return;
		MapAttribute map = MapUtils.GetMapAttribute (t);
		if (map != null) {
			if (map.NativeType != null && map.NativeType.Length > 0)
				decls.Add (map.NativeType);
			RecordTypes (t);
		}
	}

	private void CacheExternalMethods (Type t, string ns, string fn)
	{
		BindingFlags bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		foreach (MethodInfo m in t.GetMethods (bf)) {
			if ((m.Attributes & MethodAttributes.PinvokeImpl) == 0)
				continue;
			DllImportAttribute dia = GetDllImportInfo (m);
			if (dia == null) {
				Console.WriteLine ("warning: unable to emit native prototype for P/Invoke " + 
						"method: {0}", m);
				continue;
			}
			// we shouldn't declare prototypes for POSIX, etc. functions.
			if (Configuration.NativeLibraries.BinarySearch (dia.Value) < 0 ||
					IsOnExcludeList (dia.EntryPoint))
				continue;
			methods [dia.EntryPoint] = m;
			RecordTypes (m);
		}
	}

	private static DllImportAttribute GetDllImportInfo (MethodInfo method)
	{
		// .NET 2.0 synthesizes pseudo-attributes such as DllImport
		DllImportAttribute dia = MapUtils.GetCustomAttribute <DllImportAttribute> (method);
		if (dia != null)
			return dia;

		// We're not on .NET 2.0; assume we're on Mono and use some internal
		// methods...
		Type MonoMethod = Type.GetType ("System.Reflection.MonoMethod", false);
		if (MonoMethod == null) {
			Console.WriteLine ("warning: cannot find MonoMethod");
			return null;
		}
		MethodInfo GetDllImportAttribute = 
			MonoMethod.GetMethod ("GetDllImportAttribute", 
					BindingFlags.Static | BindingFlags.NonPublic);
		if (GetDllImportAttribute == null) {
			Console.WriteLine ("warning: cannot find GetDllImportAttribute");
			return null;
		}
		IntPtr mhandle = method.MethodHandle.Value;
		return (DllImportAttribute) GetDllImportAttribute.Invoke (null, 
				new object[]{mhandle});
	}

	private bool IsOnExcludeList (string method)
	{
		int idx = Configuration.NativeExcludeSymbols.BinarySearch (method);
		return (idx < 0) ? false : true;
	}

	private void RecordTypes (MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters ();
		foreach (ParameterInfo pi in parameters) {
			RecordTypes (pi.ParameterType);
		}
	}

	private void RecordTypes (Type st)
	{
		if (typeof(Delegate).IsAssignableFrom (st) && !delegates.ContainsKey (st.Name)) {
			MethodInfo mi = st.GetMethod ("Invoke");
			delegates [st.Name] = mi;
			RecordTypes (mi);
			return;
		}
		Type et = MapUtils.GetElementType (st);
		string s = MapUtils.GetNativeType (et);
		if (s.StartsWith ("struct ") && !structs.ContainsKey (et.Name)) {
			structs [et.Name] = et;
			foreach (FieldInfo fi in et.GetFields (BindingFlags.Instance | 
					BindingFlags.Public | BindingFlags.NonPublic)) {
				RecordTypes (fi.FieldType);
			}
		}
	}

	public override void CloseFile (string file_prefix)
	{
		IEnumerable<string> structures = Sort (structs.Keys);
		sh.WriteLine ();
		sh.WriteLine ("/*\n * Managed Structure Declarations\n */\n");
		foreach (string s in structures) {
			sh.WriteLine ("struct {0};", MapUtils.GetManagedType (structs [s]));
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Inferred Structure Declarations\n */\n");
		foreach (string s in decls) {
			sh.WriteLine ("{0};", s);
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Delegate Declarations\n */\n");
		foreach (string s in Sort (delegates.Keys)) {
			sh.WriteLine ("typedef {0};",
					MapUtils.GetFunctionDeclaration ("(*" + s + ")", delegates [s]));
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Structures\n */\n");
		foreach (string s in structures) {
			WriteStructDeclarations (s);
		}
		sh.WriteLine ();

		sh.WriteLine ("/*\n * Functions\n */");
		foreach (string method in Configuration.NativeExcludeSymbols) {
			if (methods.ContainsKey (method))
				methods.Remove (method);
		}
		foreach (string method in Sort (methods.Keys)) {
			WriteMethodDeclaration ((MethodInfo) methods [method], method);
		}

		sh.WriteLine ("\nG_END_DECLS\n");
		sh.WriteLine ("#endif /* ndef INC_Mono_Posix_" + file_prefix + "_H */\n");
		sh.Close ();
	}

	private static IEnumerable<string> Sort (ICollection<string> c)
	{
		List<string> al = new List<string> (c);
		al.Sort (MapUtils.OrdinalStringComparer);
		return al;
	}

	private void WriteStructDeclarations (string s)
	{
		Type t = structs [s];
#if false
		if (!t.Assembly.CodeBase.EndsWith (this.assembly_file)) {
			return;
		}
#endif
		sh.WriteLine ("struct {0} {{", MapUtils.GetManagedType (t));
		FieldInfo[] fields = t.GetFields (BindingFlags.Instance | 
				BindingFlags.Public | BindingFlags.NonPublic);
		int max_type_len = 0, max_name_len = 0, max_native_len = 0;
		Array.ForEach (fields, delegate (FieldInfo f) {
				max_type_len    = Math.Max (max_type_len, HeaderFileGenerator.GetType (f.FieldType).Length);
				max_name_len    = Math.Max (max_name_len, GetNativeMemberName (f).Length);
				string native_type = MapUtils.GetNativeType (f);
				if (native_type != null)
					max_native_len  = Math.Max (max_native_len, native_type.Length);
		});
		SortFieldsInOffsetOrder (t, fields);
		foreach (FieldInfo field in fields) {
			string fname = GetNativeMemberName (field);
			sh.Write ("\t{0,-" + max_type_len + "} {1};", 
					GetType (field.FieldType), fname);
			string native_type = MapUtils.GetNativeType (field);
			if (native_type != null) {
				sh.Write (new string (' ', max_name_len - fname.Length));
				sh.Write ("  /* {0,-" + max_native_len + "} */", native_type);
			}
			sh.WriteLine ();
		}
		sh.WriteLine ("};");
		MapAttribute map = MapUtils.GetMapAttribute (t);
		if (map != null && map.NativeType != null && map.NativeType.Length != 0 &&
				t.Assembly.CodeBase.EndsWith (this.assembly_file)) {
			sh.WriteLine ();
			sh.WriteLine (
					"int\n{0}_From{1} ({3}{4} from, {2} *to);\n" + 
					"int\n{0}_To{1} ({2} *from, {3}{4} to);\n",
					MapUtils.GetNamespace (t), t.Name, map.NativeType, 
					MapUtils.GetNativeType (t), t.IsValueType ? "*" : "");
			Configuration.NativeExcludeSymbols.Add (
					string.Format ("{0}_From{1}", MapUtils.GetNamespace (t), t.Name));
			Configuration.NativeExcludeSymbols.Add (
					string.Format ("{0}_To{1}", MapUtils.GetNamespace (t), t.Name));
			Configuration.NativeExcludeSymbols.Sort ();
		}
		sh.WriteLine ();
	}

	private static string GetType (Type t)
	{
		if (typeof(Delegate).IsAssignableFrom (t))
			return t.Name;
		return MapUtils.GetNativeType (t);
	}

	private void WriteMethodDeclaration (MethodInfo method, string entryPoint)
	{
		if (method.ReturnType.IsClass) {
			Console.WriteLine ("warning: {0} has a return type of {1}, which is a reference type",
					entryPoint, method.ReturnType.FullName);
		}
		sh.Write (MapUtils.GetFunctionDeclaration (entryPoint, method));
		sh.WriteLine (";");
	}

	private void DumpTypeInfo (Type t)
	{
		if (t == null)
			return;

		sh.WriteLine ("\t\t/* Type Info for " + t.FullName + ":");
		foreach (MemberInfo mi in typeof(Type).GetMembers()) {
			sh.WriteLine ("\t\t\t{0}={1}", mi.Name, GetMemberValue (mi, t));
		}
		sh.WriteLine ("\t\t */");
	}

	private static string GetMemberValue (MemberInfo mi, Type t)
	{
		try {
			switch (mi.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Method: {
					MethodBase b = (MethodBase) mi;
					if (b.GetParameters().Length == 0)
						return b.Invoke (t, new object[]{}).ToString();
					return "<<cannot invoke>>";
				}
				case MemberTypes.Field:
					return ((FieldInfo) mi).GetValue (t).ToString ();
				case MemberTypes.Property: {
					PropertyInfo pi = (PropertyInfo) mi;
					if (!pi.CanRead)
						return "<<cannot read>>";
					return pi.GetValue (t, null).ToString ();
				}
				default:
					return "<<unknown value>>";
			}
		}
		catch (Exception e) {
			return "<<exception reading member: " + e.Message + ">>";
		}
	}
}

class SourceFileGenerator : FileGenerator {
	StreamWriter sc;
	string file_prefix;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		sc = File.CreateText (file_prefix + ".c");
		WriteHeader (sc, assembly_name);

		if (file_prefix.IndexOf ("/") != -1)
			file_prefix = file_prefix.Substring (file_prefix.IndexOf ("/") + 1);
		this.file_prefix = file_prefix;
		sc.WriteLine ("#include <stdlib.h>");
		sc.WriteLine ("#include <string.h>");
		sc.WriteLine ();
	}

	public override void WriteAssemblyAttributes (Assembly assembly)
	{
		sc.WriteLine ("/*\n * Implementation Macros\n */");
		foreach (string def in Configuration.ImplementationMacros) {
			WriteMacroDefinition (sc, def);
		}
		sc.WriteLine ();

		sc.WriteLine ("/*\n * Implementation Includes\n */");
		foreach (string inc in Configuration.ImplementationHeaders) {
			WriteIncludeDeclaration (sc, inc);
		}
		sc.WriteLine ();

		sc.WriteLine ("#include \"{0}.h\"", file_prefix);

		sc.WriteLine (@"
#include <errno.h>    /* errno, EOVERFLOW */
#include <glib.h>     /* g* types, g_assert_not_reached() */");

		WriteFallbackMacro ("CNM_MININT8", "G_MININT8", sbyte.MinValue.ToString ());
		WriteFallbackMacro ("CNM_MAXINT8", "G_MAXINT8", sbyte.MaxValue.ToString ());
		WriteFallbackMacro ("CNM_MAXUINT8", "G_MAXUINT8", byte.MaxValue.ToString ());
		WriteFallbackMacro ("CNM_MININT16", "G_MININT16", short.MinValue.ToString ());
		WriteFallbackMacro ("CNM_MAXINT16", "G_MAXINT16", short.MaxValue.ToString ());
		WriteFallbackMacro ("CNM_MAXUINT16", "G_MAXUINT16", ushort.MaxValue.ToString ());
		WriteFallbackMacro ("CNM_MININT32", "G_MININT32", int.MinValue.ToString ());
		WriteFallbackMacro ("CNM_MAXINT32", "G_MAXINT32", int.MaxValue.ToString ());
		WriteFallbackMacro ("CNM_MAXUINT32", "G_MAXUINT32", uint.MaxValue.ToString () + "U");
		WriteFallbackMacro ("CNM_MININT64", "G_MININT64", long.MinValue.ToString () + "LL");
		WriteFallbackMacro ("CNM_MAXINT64", "G_MAXINT64", long.MaxValue.ToString () + "LL");
		WriteFallbackMacro ("CNM_MAXUINT64", "G_MAXUINT64", ulong.MaxValue.ToString () + "ULL");

		sc.WriteLine (@"

/* returns TRUE if @type is an unsigned type */
#define _cnm_integral_type_is_unsigned(type) \
    (sizeof(type) == sizeof(gint8)           \
      ? (((type)-1) > CNM_MAXINT8)             \
      : sizeof(type) == sizeof(gint16)       \
        ? (((type)-1) > CNM_MAXINT16)          \
        : sizeof(type) == sizeof(gint32)     \
          ? (((type)-1) > CNM_MAXINT32)        \
          : sizeof(type) == sizeof(gint64)   \
            ? (((type)-1) > CNM_MAXINT64)      \
            : (g_assert_not_reached (), 0))

/* returns the minimum value of @type as a gint64 */
#define _cnm_integral_type_min(type)          \
    (_cnm_integral_type_is_unsigned (type)    \
      ? 0                                     \
      : sizeof(type) == sizeof(gint8)         \
        ? CNM_MININT8                           \
        : sizeof(type) == sizeof(gint16)      \
          ? CNM_MININT16                        \
          : sizeof(type) == sizeof(gint32)    \
            ? CNM_MININT32                      \
            : sizeof(type) == sizeof(gint64)  \
              ? CNM_MININT64                    \
              : (g_assert_not_reached (), 0))

/* returns the maximum value of @type as a guint64 */
#define _cnm_integral_type_max(type)            \
    (_cnm_integral_type_is_unsigned (type)      \
      ? sizeof(type) == sizeof(gint8)           \
        ? CNM_MAXUINT8                            \
        : sizeof(type) == sizeof(gint16)        \
          ? CNM_MAXUINT16                         \
          : sizeof(type) == sizeof(gint32)      \
            ? CNM_MAXUINT32                       \
            : sizeof(type) == sizeof(gint64)    \
              ? CNM_MAXUINT64                     \
              : (g_assert_not_reached (), 0)    \
      : sizeof(type) == sizeof(gint8)           \
          ? CNM_MAXINT8                           \
          : sizeof(type) == sizeof(gint16)      \
            ? CNM_MAXINT16                        \
            : sizeof(type) == sizeof(gint32)    \
              ? CNM_MAXINT32                      \
              : sizeof(type) == sizeof(gint64)  \
                ? CNM_MAXINT64                    \
                : (g_assert_not_reached (), 0))

#ifdef _CNM_DUMP
#define _cnm_dump(to_t,from)                                             \
  printf (""# %s -> %s: uns=%i; min=%llx; max=%llx; value=%llx; lt=%i; l0=%i; gt=%i; e=%i\n"", \
    #from, #to_t,                                                        \
    (int) _cnm_integral_type_is_unsigned (to_t),                         \
    (gint64) (_cnm_integral_type_min (to_t)),                            \
    (gint64) (_cnm_integral_type_max (to_t)),                            \
    (gint64) (from),                                                     \
    (((gint64) _cnm_integral_type_min (to_t)) <= (gint64) from),         \
    (from < 0),                                                          \
    (((guint64) from) <= (guint64) _cnm_integral_type_max (to_t)),       \
    !((int) _cnm_integral_type_is_unsigned (to_t)                        \
      ? ((0 <= from) &&                                                  \
         ((guint64) from <= (guint64) _cnm_integral_type_max (to_t)))    \
      : ((gint64) _cnm_integral_type_min(to_t) <= (gint64) from &&       \
         (guint64) from <= (guint64) _cnm_integral_type_max (to_t)))     \
  )
#else /* ndef _CNM_DUMP */
#define _cnm_dump(to_t, from) do {} while (0)
#endif /* def _CNM_DUMP */

#ifdef DEBUG
#define _cnm_return_val_if_overflow(to_t,from,val)  G_STMT_START {   \
    int     uns = _cnm_integral_type_is_unsigned (to_t);             \
    gint64  min = (gint64)  _cnm_integral_type_min (to_t);           \
    guint64 max = (guint64) _cnm_integral_type_max (to_t);           \
    gint64  sf  = (gint64)  from;                                    \
    guint64 uf  = (guint64) from;                                    \
    if (!(uns ? ((0 <= from) && (uf <= max))                         \
              : (min <= sf && (from < 0 || uf <= max)))) {           \
      _cnm_dump(to_t, from);                                         \
      errno = EOVERFLOW;                                             \
      return (val);                                                  \
    }                                                                \
  } G_STMT_END
#else /* !def DEBUG */
/* don't do any overflow checking */
#define _cnm_return_val_if_overflow(to_t,from,val)  G_STMT_START {   \
  } G_STMT_END
#endif /* def DEBUG */
");
	}

	private void WriteFallbackMacro (string target, string glib, string def)
	{
		sc.WriteLine (@"
#if defined ({1})
#define {0} {1}
#else
#define {0} ({2})
#endif", target, glib, def);
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		if (!CanMapType (t))
			return;

		string etype = MapUtils.GetNativeType (t);

		if (t.IsEnum) {
			bool bits = IsFlagsEnum (t);

			WriteFromManagedEnum (t, ns, fn, etype, bits);
			WriteToManagedEnum (t, ns, fn, etype, bits);
		}
		else {
			WriteFromManagedClass (t, ns, fn, etype);
			WriteToManagedClass (t, ns, fn, etype);
		}
	}

	private void WriteFromManagedEnum (Type t, string ns, string fn, string etype, bool bits)
	{
		sc.WriteLine ("int {1}_From{2} ({0} x, {0} *r)", etype, ns, t.Name);
		sc.WriteLine ("{");
		sc.WriteLine ("\t*r = 0;");
		FieldInfo[] fields = t.GetFields ();
		Array.Sort<FieldInfo> (fields, MapUtils.MemberNameComparer);
		Array values = Enum.GetValues (t);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			if (MapUtils.GetCustomAttribute<ObsoleteAttribute> (fi) != null) {
				sc.WriteLine ("\t/* {0}_{1} is obsolete or optional; ignoring */", fn, fi.Name);
				continue;
			}
			MapAttribute map = MapUtils.GetMapAttribute (fi);
			bool is_bits = bits && (map != null ? map.SuppressFlags == null : true);
			if (is_bits)
				// properly handle case where [Flags] enumeration has helper
				// synonyms.  e.g. DEFFILEMODE and ACCESSPERMS for mode_t.
				sc.WriteLine ("\tif ((x & {0}_{1}) == {0}_{1})", fn, fi.Name);
			else if (GetSuppressFlags (map) == null)
				sc.WriteLine ("\tif (x == {0}_{1})", fn, fi.Name);
			else
				sc.WriteLine ("\tif ((x & {0}_{1}) == {0}_{2})", fn, map.SuppressFlags, fi.Name);
			sc.WriteLine ("#ifdef {0}", fi.Name);
			if (is_bits || GetSuppressFlags (map) != null)
				sc.WriteLine ("\t\t*r |= {1};", fn, fi.Name);
			else
				sc.WriteLine ("\t\t{{*r = {1}; return 0;}}", fn, fi.Name);
			sc.WriteLine ("#else /* def {0} */", fi.Name);
			if (is_bits && IsRedundant (t, fi, values)) {
				sc.WriteLine ("\t\t{{/* Ignoring {0}_{1}, as it is constructed from other values */}}",
						fn, fi.Name);
			}
			else {
				sc.WriteLine ("\t\t{errno = EINVAL; return -1;}");
			}
			sc.WriteLine ("#endif /* ndef {0} */", fi.Name);
		}
		// For many values, 0 is a valid value, but doesn't have it's own symbol.
		// Examples: Error (0 means "no error"), WaitOptions (0 means "no options").
		// Make 0 valid for all conversions.
		sc.WriteLine ("\tif (x == 0)\n\t\treturn 0;");
		if (bits)
			sc.WriteLine ("\treturn 0;");
		else
			sc.WriteLine ("\terrno = EINVAL; return -1;"); // return error if not matched
		sc.WriteLine ("}\n");
	}

	private static string GetSuppressFlags (MapAttribute map)
	{
		if (map != null) {
			return map.SuppressFlags == null
				? null
				: map.SuppressFlags.Length == 0
					? null
					: map.SuppressFlags;
		}
		return null;
	}

	private static bool IsRedundant (Type t, FieldInfo fi, Array values)
	{
		long v = Convert.ToInt64 (fi.GetValue (null));
		long d = v;
		if (v == 0)
			return false;
		foreach (object o in values) {
			long e = Convert.ToInt64 (o);
			if (((d & e) != 0) && (e < d)) {
				v &= ~e;
			}
		}
		if (v == 0) {
			return true;
		}
		return false;
	}

	private void WriteToManagedEnum (Type t, string ns, string fn, string etype, bool bits)
	{
		sc.WriteLine ("int {1}_To{2} ({0} x, {0} *r)", etype, ns, t.Name);
		sc.WriteLine ("{");
		sc.WriteLine ("\t*r = 0;", etype);
		// For many values, 0 is a valid value, but doesn't have it's own symbol.
		// Examples: Error (0 means "no error"), WaitOptions (0 means "no options").
		// Make 0 valid for all conversions.
		sc.WriteLine ("\tif (x == 0)\n\t\treturn 0;");
		FieldInfo[] fields = t.GetFields ();
		Array.Sort<FieldInfo> (fields, MapUtils.MemberNameComparer);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			MapAttribute map = MapUtils.GetMapAttribute (fi);
			bool is_bits = bits && (map != null ? map.SuppressFlags == null: true);
			sc.WriteLine ("#ifdef {0}", fi.Name);
			if (is_bits)
				// properly handle case where [Flags] enumeration has helper
				// synonyms.  e.g. DEFFILEMODE and ACCESSPERMS for mode_t.
				sc.WriteLine ("\tif ((x & {1}) == {1})\n\t\t*r |= {0}_{1};", fn, fi.Name);
			else if (GetSuppressFlags (map) == null)
				sc.WriteLine ("\tif (x == {1})\n\t\t{{*r = {0}_{1}; return 0;}}", fn, fi.Name);
			else
				sc.WriteLine ("\tif ((x & {2}) == {1})\n\t\t*r |= {0}_{1};", fn, fi.Name, map.SuppressFlags);
			sc.WriteLine ("#endif /* ndef {0} */", fi.Name);
		}
		if (bits)
			sc.WriteLine ("\treturn 0;");
		else
			sc.WriteLine ("\terrno = EINVAL; return -1;");
		sc.WriteLine ("}\n");
	}

	private void WriteFromManagedClass (Type t, string ns, string fn, string etype)
	{
		MapAttribute map = MapUtils.GetMapAttribute (t);
		if (map == null || map.NativeType == null || map.NativeType.Length == 0)
			return;
		string nativeMacro = GetAutoconfDefine (map.NativeType);
		sc.WriteLine ("#ifdef {0}", nativeMacro);
		sc.WriteLine ("int\n{0}_From{1} (struct {0}_{1} *from, {2} *to)",
				MapUtils.GetNamespace (t), t.Name, map.NativeType);
		WriteManagedClassConversion (t, delegate (FieldInfo field) {
				MapAttribute ft = MapUtils.GetMapAttribute (field);
				if (ft != null)
					return ft.NativeType;
				return MapUtils.GetNativeType (field.FieldType);
			},
			delegate (FieldInfo field) {
				return GetNativeMemberName (field);
			},
			delegate (FieldInfo field) {
				return field.Name;
			},
			delegate (FieldInfo field) {
				return string.Format ("{0}_From{1}",
					MapUtils.GetNamespace (field.FieldType),
					field.FieldType.Name);
			}
		);
		sc.WriteLine ("#endif /* ndef {0} */\n\n", nativeMacro);
	}

	private static string GetAutoconfDefine (string nativeType)
	{
		return string.Format ("HAVE_{0}",
				nativeType.ToUpperInvariant ().Replace (" ", "_"));
	}

	private delegate string GetFromType (FieldInfo field);
	private delegate string GetToFieldName (FieldInfo field);
	private delegate string GetFromFieldName (FieldInfo field);
	private delegate string GetFieldCopyMethod (FieldInfo field);

	private void WriteManagedClassConversion (Type t, GetFromType gft, 
			GetFromFieldName gffn, GetToFieldName gtfn, GetFieldCopyMethod gfc)
	{
		MapAttribute map = MapUtils.GetMapAttribute (t);
		sc.WriteLine ("{");
		FieldInfo[] fields = GetFieldsToCopy (t);
		SortFieldsInOffsetOrder (t, fields);
		int max_len = 0;
		foreach (FieldInfo f in fields) {
			max_len = Math.Max (max_len, f.Name.Length);
			if (!MapUtils.IsIntegralType (f.FieldType))
				continue;
			string d = GetAutoconfDefine (map, f);
			if (d != null)
				sc.WriteLine ("#ifdef " + d);
			sc.WriteLine ("\t_cnm_return_val_if_overflow ({0}, from->{1}, -1);",
					gft (f), gffn (f));
			if (d != null)
				sc.WriteLine ("#endif /* ndef " + d + " */");
		}
		sc.WriteLine ("\n\tmemset (to, 0, sizeof(*to));\n");
		foreach (FieldInfo f in fields) {
			string d = GetAutoconfDefine (map, f);
			if (d != null)
				sc.WriteLine ("#ifdef " + d);
			if (MapUtils.IsBlittableType (f.FieldType)) {
				sc.WriteLine ("\tto->{0,-" + max_len + "} = from->{1};", 
						gtfn (f), gffn (f));
			}
			else if (f.FieldType.IsEnum) {
				sc.WriteLine ("\tif ({0} (from->{1}, &to->{2}) != 0) {{", gfc (f),
						gffn (f), gtfn (f));
				sc.WriteLine ("\t\treturn -1;");
				sc.WriteLine ("\t}");
			}
			else if (f.FieldType.IsValueType) {
				sc.WriteLine ("\tif ({0} (&from->{1}, &to->{2}) != 0) {{", gfc (f),
						gffn (f), gtfn (f));
				sc.WriteLine ("\t\treturn -1;");
				sc.WriteLine ("\t}");
			}
			if (d != null)
				sc.WriteLine ("#endif /* ndef " + d + " */");
		}
		sc.WriteLine ();
		sc.WriteLine ("\treturn 0;");
		sc.WriteLine ("}");
	}

	private void WriteToManagedClass (Type t, string ns, string fn, string etype)
	{
		MapAttribute map = MapUtils.GetMapAttribute (t);
		if (map == null || map.NativeType == null || map.NativeType.Length == 0)
			return;
		string nativeMacro = GetAutoconfDefine (map.NativeType);
		sc.WriteLine ("#ifdef {0}", nativeMacro);
		sc.WriteLine ("int\n{0}_To{1} ({2} *from, struct {0}_{1} *to)", 
				MapUtils.GetNamespace (t), t.Name, map.NativeType);
		WriteManagedClassConversion (t, delegate (FieldInfo field) {
				return MapUtils.GetNativeType (field.FieldType);
			},
			delegate (FieldInfo field) {
				return field.Name;
			},
			delegate (FieldInfo field) {
				return GetNativeMemberName (field);
			},
			delegate (FieldInfo field) {
				return string.Format ("{0}_To{1}",
					MapUtils.GetNamespace (field.FieldType),
					field.FieldType.Name);
			}
		);
		sc.WriteLine ("#endif /* ndef {0} */\n\n", nativeMacro);
	}

	private static FieldInfo[] GetFieldsToCopy (Type t)
	{
		FieldInfo[] fields = t.GetFields (BindingFlags.Instance | 
				BindingFlags.Public | BindingFlags.NonPublic);
		int count = 0;
		for (int i = 0; i < fields.Length; ++i)
			if (MapUtils.GetCustomAttribute <NonSerializedAttribute> (fields [i]) == null)
				++count;
		FieldInfo[] rf = new FieldInfo [count];
		for (int i = 0, j = 0; i < fields.Length; ++i) {
			if (MapUtils.GetCustomAttribute <NonSerializedAttribute> (fields [i]) == null)
				rf [j++] = fields [i];
		}
		return rf;
	}

	private string GetAutoconfDefine (MapAttribute typeMap, FieldInfo field)
	{
		if (Configuration.AutoconfMembers.BinarySearch (field.Name) < 0 &&
				Configuration.AutoconfMembers.BinarySearch (field.DeclaringType.Name + "." + field.Name) < 0)
			return null;
		return string.Format ("HAVE_{0}_{1}", 
				typeMap.NativeType.ToUpperInvariant().Replace (" ", "_"),
				field.Name.ToUpperInvariant ());
	}

	public override void CloseFile (string file_prefix)
	{
		sc.Close ();
	}
}

class ConvertFileGenerator : FileGenerator {
	StreamWriter scs;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		scs = File.CreateText (file_prefix + ".cs");
		WriteHeader (scs, assembly_name, true);
		scs.WriteLine ("using System;");
		scs.WriteLine ("using System.Runtime.InteropServices;");
		scs.WriteLine ("using Mono.Unix.Native;\n");
		scs.WriteLine ("namespace Mono.Unix.Native {\n");
		scs.WriteLine ("\tpublic sealed /* static */ partial class NativeConvert");
		scs.WriteLine ("\t{");
		scs.WriteLine ("\t\tprivate NativeConvert () {}\n");
		scs.WriteLine ("\t\tprivate const string LIB = \"{0}\";\n", Configuration.NativeLibraries[0]);
		scs.WriteLine ("\t\tprivate static void ThrowArgumentException (object value)");
		scs.WriteLine ("\t\t{");
		scs.WriteLine ("\t\t\tthrow new ArgumentOutOfRangeException (\"value\", value,");
		scs.WriteLine ("\t\t\t\tLocale.GetText (\"Current platform doesn't support this value.\"));");
		scs.WriteLine ("\t\t}\n");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		if (!CanMapType (t))
			return;
		if (t.IsEnum)
			WriteEnum (t, ns, fn);
		else
			WriteStruct (t, ns, fn);
	}

	private void WriteEnum (Type t, string ns, string fn)
	{
		string mtype = Enum.GetUnderlyingType(t).Name;
		ObsoleteAttribute oa = MapUtils.GetCustomAttribute <ObsoleteAttribute> (t);
		string obsolete = "";
		if (oa != null) {
			obsolete = string.Format ("[Obsolete (\"{0}\", {1})]\n\t\t",
					oa.Message, oa.IsError ? "true" : "false");
		}
		scs.WriteLine (
				"\t\t{0}[DllImport (LIB, EntryPoint=\"{1}_From{2}\")]\n" +
				"\t\tprivate static extern int From{2} ({2} value, out {3} rval);\n" +
				"\n" +
				"\t\t{0}public static bool TryFrom{2} ({2} value, out {3} rval)\n" +
				"\t\t{{\n" +
				"\t\t\treturn From{2} (value, out rval) == 0;\n" +
				"\t\t}}\n" +
				"\n" +
				"\t\t{0}public static {3} From{2} ({2} value)\n" +
				"\t\t{{\n" +
				"\t\t\t{3} rval;\n" +
				"\t\t\tif (From{2} (value, out rval) == -1)\n" + 
				"\t\t\t\tThrowArgumentException (value);\n" +
				"\t\t\treturn rval;\n" +
				"\t\t}}\n" +
				"\n" +
				"\t\t{0}[DllImport (LIB, EntryPoint=\"{1}_To{2}\")]\n" +
				"\t\tprivate static extern int To{2} ({3} value, out {2} rval);\n" +
				"\n" +
				"\t\t{0}public static bool TryTo{2} ({3} value, out {2} rval)\n" +
				"\t\t{{\n" +
				"\t\t\treturn To{2} (value, out rval) == 0;\n" +
				"\t\t}}\n" +
				"\n" +
				"\t\t{0}public static {2} To{2} ({3} value)\n" +
				"\t\t{{\n" +
				"\t\t\t{2} rval;\n" +
				"\t\t\tif (To{2} (value, out rval) == -1)\n" + 
				"\t\t\t\tThrowArgumentException (value);\n" +
				"\t\t\treturn rval;\n" +
				"\t\t}}\n",
			obsolete, ns, t.Name, mtype
		);
	}

	private void WriteStruct (Type t, string ns, string fn)
	{
		if (MapUtils.IsDelegate (t))
			return;
		MapAttribute map = MapUtils.GetMapAttribute (t);
		if (map == null || map.NativeType == null || map.NativeType.Length == 0)
			return;
		ObsoleteAttribute oa = MapUtils.GetCustomAttribute <ObsoleteAttribute> (t);
		string obsolete = "";
		if (oa != null) {
			obsolete = string.Format ("[Obsolete (\"{0}\", {1})]\n\t\t",
					oa.Message, oa.IsError ? "true" : "false");
		}
		string _ref = t.IsValueType ? "ref " : "";
		string _out = t.IsValueType ? "out " : "";
		scs.WriteLine (
				"\t\t{0}[DllImport (LIB, EntryPoint=\"{1}_From{2}\")]\n" +
				"\t\tprivate static extern int From{2} ({3}{2} source, IntPtr destination);\n" +
				"\n" +
				"\t\t{0}public static bool TryCopy ({3}{2} source, IntPtr destination)\n" +
				"\t\t{{\n" +
				"\t\t\treturn From{2} ({3}source, destination) == 0;\n" +
				"\t\t}}\n" +
				"\n" +
				"\t\t{0}[DllImport (LIB, EntryPoint=\"{1}_To{2}\")]\n" +
				"\t\tprivate static extern int To{2} (IntPtr source, {4}{2} destination);\n" +
				"\n" +
				"\t\t{0}public static bool TryCopy (IntPtr source, {4}{2} destination)\n" +
				"\t\t{{\n" +
				"\t\t\treturn To{2} (source, {4}destination) == 0;\n" +
				"\t\t}}\n",
			obsolete, ns, t.Name, _ref, _out
		);
	}

	public override void CloseFile (string file_prefix)
	{
		scs.WriteLine ("\t}");
		scs.WriteLine ("}\n");
		scs.Close ();
	}
}

class ConvertDocFileGenerator : FileGenerator {
	StreamWriter scs;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		scs = File.CreateText (file_prefix + ".xml");
		scs.WriteLine ("    <!-- BEGIN GENERATED CONTENT");
		WriteHeader (scs, assembly_name, true);
		scs.WriteLine ("      -->");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		if (!CanMapType (t) || !t.IsEnum)
			return;

		bool bits = IsFlagsEnum (t);

		string type = GetCSharpType (t);
		string mtype = Enum.GetUnderlyingType(t).FullName;
		string member = t.Name;
		string ftype = t.FullName;

		string to_returns = "";
		string to_remarks = "";
		string to_exception = "";

		if (bits) {
			to_returns = "<returns>An approximation of the equivalent managed value.</returns>";
			to_remarks = @"<para>The current conversion functions are unable to determine
        if a value in a <c>[Flags]</c>-marked enumeration <i>does not</i> 
        exist on the current platform.  As such, if <paramref name=""value"" /> 
        contains a flag value which the current platform doesn't support, it 
        will not be present in the managed value returned.</para>
        <para>This should only be a problem if <paramref name=""value"" /> 
        <i>was not</i> previously returned by 
        <see cref=""M:Mono.Unix.Native.NativeConvert.From" + member + "\" />.</para>\n";
		}
		else {
			to_returns = "<returns>The equivalent managed value.</returns>";
			to_exception = @"
        <exception cref=""T:System.ArgumentOutOfRangeException"">
          <paramref name=""value"" /> has no equivalent managed value.
        </exception>
";
		}
		scs.WriteLine (@"
    <Member MemberName=""TryFrom{1}"">
      <MemberSignature Language=""C#"" Value=""public static bool TryFrom{1} ({0} value, out {2} rval);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>System.Boolean</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{0}"" />
        <Parameter Name=""rval"" Type=""{3}&amp;"" RefType=""out"" />
      </Parameters>
      <Docs>
        <param name=""value"">The managed value to convert.</param>
        <param name=""rval"">The OS-specific equivalent value.</param>
        <summary>Converts a <see cref=""T:{0}"" /> 
          enumeration value to an OS-specific value.</summary>
        <returns><see langword=""true"" /> if the conversion was successful; 
        otherwise, <see langword=""false"" />.</returns>
        <remarks><para>This is an exception-safe alternative to 
        <see cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />.</para>
        <para>If successful, this method stores the OS-specific equivalent
        value of <paramref name=""value"" /> into <paramref name=""rval"" />.
        Otherwise, <paramref name=""rval"" /> will contain <c>0</c>.</para>
        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""From{1}"">
      <MemberSignature Language=""C#"" Value=""public static {2} From{1} ({0} value);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>{3}</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{0}"" />
      </Parameters>
      <Docs>
        <param name=""value"">The managed value to convert.</param>
        <summary>Converts a <see cref=""T:{0}"" /> 
          to an OS-specific value.</summary>
        <returns>The equivalent OS-specific value.</returns>
        <exception cref=""T:System.ArgumentOutOfRangeException"">
          <paramref name=""value"" /> has no equivalent OS-specific value.
        </exception>
        <remarks></remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""TryTo{1}"">
      <MemberSignature Language=""C#"" Value=""public static bool TryTo{1} ({2} value, out {0} rval);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>System.Boolean</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{3}"" />
        <Parameter Name=""rval"" Type=""{0}&amp;"" RefType=""out"" />
      </Parameters>
      <Docs>
        <param name=""value"">The OS-specific value to convert.</param>
        <param name=""rval"">The managed equivalent value</param>
        <summary>Converts an OS-specific value to a 
          <see cref=""T:{0}"" />.</summary>
        <returns><see langword=""true"" /> if the conversion was successful; 
        otherwise, <see langword=""false"" />.</returns>
        <remarks><para>This is an exception-safe alternative to 
        <see cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />.</para>
        <para>If successful, this method stores the managed equivalent
        value of <paramref name=""value"" /> into <paramref name=""rval"" />.
        Otherwise, <paramref name=""rval"" /> will contain a <c>0</c>
        cast to a <see cref=""T:{0}"" />.</para>
        " + to_remarks + 
@"        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""To{1}"">
      <MemberSignature Language=""C#"" Value=""public static {0} To{1} ({2} value);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>{0}</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{3}"" />
      </Parameters>
      <Docs>
        <param name=""value"">The OS-specific value to convert.</param>
        <summary>Converts an OS-specific value to a 
          <see cref=""T:{0}"" />.</summary>
					" + to_returns + "\n" + 
			to_exception + 
@"        <remarks>
        " + to_remarks + @"
        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
", ftype, member, type, mtype
		);
	}

	private string GetCSharpType (Type t)
	{
		string ut = t.Name;
		if (t.IsEnum)
			ut = Enum.GetUnderlyingType (t).Name;
		Type et = t.GetElementType ();
		if (et != null && et.IsEnum)
			ut = Enum.GetUnderlyingType (et).Name;

		string type = null;

		switch (ut) {
			case "Boolean":       type = "bool";    break;
			case "Byte":          type = "byte";    break;
			case "SByte":         type = "sbyte";   break;
			case "Int16":         type = "short";   break;
			case "UInt16":        type = "ushort";  break;
			case "Int32":         type = "int";     break;
			case "UInt32":        type = "uint";    break;
			case "Int64":         type = "long";    break;
			case "UInt64":        type = "ulong";   break;
		}

		return type;
	}

	public override void CloseFile (string file_prefix)
	{
		scs.WriteLine ("    <!-- END GENERATED CONTENT -->");
		scs.Close ();
	}
}

// vim: noexpandtab
