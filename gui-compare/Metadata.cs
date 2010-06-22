//
// Metadata.cs
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

namespace GuiCompare {

	public enum CompType {
		Assembly,
		Namespace,
		Attribute,
		Interface,
		Class,
		Struct,
		Enum,
		Method,
		Property,
		Field,
		Delegate,
		Event,
		GenericParameter
	}

	public interface ICompAttributeContainer
	{
		List<CompNamed> GetAttributes ();
	}

	public interface ICompHasBaseType
	{
		string GetBaseType ();

		bool IsSealed { get; }
		bool IsAbstract { get; }
	}
	
	public interface ICompTypeContainer
	{
		List<CompNamed> GetNestedClasses();
		List<CompNamed> GetNestedInterfaces ();
		List<CompNamed> GetNestedStructs ();
		List<CompNamed> GetNestedEnums ();
		List<CompNamed> GetNestedDelegates ();
	}

	public interface ICompMemberContainer
	{
		List<CompNamed> GetInterfaces ();
		List<CompNamed> GetConstructors();
		List<CompNamed> GetMethods();
 		List<CompNamed> GetProperties();
 		List<CompNamed> GetFields();
 		List<CompNamed> GetEvents();
	}
	
	public interface ICompGenericParameter
	{
		List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompNamed {
		public CompNamed (string name, CompType type)
		{
			this.DisplayName = null;
			this.name = name;
			this.type = type;
			this.todos = new List<string>();
		}

		public string MemberName {
			set { memberName = value; }
			get { return memberName; }
		}
		
		public string Name {
			set { name = value; }
			get { return name; }
		}

		public string DisplayName {
			set { displayName = value; }
			get { return displayName == null ? name : displayName; }
		}

		public string ExtraInfo {
			set { extraInfo = value; }
			get { return extraInfo; }
		}

		public CompType Type {
			set { type = value; }
			get { return type; }
		}

		public ComparisonNode GetComparisonNode ()
		{
			ComparisonNode node = new ComparisonNode (type, DisplayName, MemberName, ExtraInfo);
			node.Todos.AddRange (todos);
			return node;
		}

		public static int Compare (CompNamed x, CompNamed y)
		{
			int res = string.Compare (x.Name, y.Name);
			if (res != 0)
				return res;

			var x_g = x as CompMethod;
			var y_g = y as CompMethod;
			if (x_g == null || y_g == null)
				return res;

			var x_tp = x_g.GetTypeParameters ();
			var y_tp = y_g.GetTypeParameters ();
			if (x_tp == null && y_tp != null)
				return -1;

			if (x_tp != null && y_tp == null)
				return 1;

			if (x_tp == null && y_tp == null)
				return res;

			return x_tp.Count.CompareTo (y_tp.Count);
		}

		string displayName;
		string name;
		string memberName;
		string extraInfo;
		CompType type;
		public List<string> todos;
	}

	public abstract class CompAssembly : CompNamed, ICompAttributeContainer {
		public CompAssembly (string name)
			: base (name, CompType.Assembly)
		{
		}

		public abstract List<CompNamed> GetNamespaces ();
		public abstract List<CompNamed> GetAttributes ();
	}

	public abstract class CompNamespace : CompNamed, ICompTypeContainer {
		public CompNamespace (string name)
			: base (name, CompType.Namespace)
		{
		}

		// ICompTypeContainer implementation
		public abstract List<CompNamed> GetNestedClasses();
		public abstract List<CompNamed> GetNestedInterfaces ();
		public abstract List<CompNamed> GetNestedStructs ();
		public abstract List<CompNamed> GetNestedEnums ();
		public abstract List<CompNamed> GetNestedDelegates ();
	}

	public abstract class CompInterface : CompNamed, ICompAttributeContainer, ICompMemberContainer, ICompHasBaseType, ICompGenericParameter {
		public CompInterface (string name)
			: base (name, CompType.Interface)
		{
		}

		public abstract List<CompNamed> GetAttributes ();

		public abstract List<CompNamed> GetInterfaces ();
		public abstract List<CompNamed> GetConstructors();
		public abstract List<CompNamed> GetMethods();
 		public abstract List<CompNamed> GetProperties();
 		public abstract List<CompNamed> GetFields();
 		public abstract List<CompNamed> GetEvents();
		
		public abstract string GetBaseType();
		
		public bool IsSealed { get { return false; } }
		public bool IsAbstract { get { return false; } }
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompEnum : CompNamed, ICompAttributeContainer, ICompMemberContainer, ICompHasBaseType {
		public CompEnum (string name)
			: base (name, CompType.Enum)
		{
		}

		public List<CompNamed> GetInterfaces () { return new List<CompNamed>(); }
		public List<CompNamed> GetConstructors() { return new List<CompNamed>(); }
		public List<CompNamed> GetMethods() { return new List<CompNamed>(); }
 		public List<CompNamed> GetProperties() { return new List<CompNamed>(); }
 		public List<CompNamed> GetEvents() { return new List<CompNamed>(); }

 		public abstract List<CompNamed> GetFields();

		public abstract List<CompNamed> GetAttributes ();
		
		public abstract string GetBaseType();
		
		public bool IsSealed { get { return true; } }
		public bool IsAbstract { get { return false; } }
	}

	public abstract class CompDelegate : CompNamed, ICompAttributeContainer, ICompHasBaseType, ICompGenericParameter {
		public CompDelegate (string name)
			: base (name, CompType.Delegate)
		{
		}
		
		public abstract List<CompNamed> GetAttributes ();

		public abstract string GetBaseType();
		
		public bool IsSealed { get { return true; } }
		public bool IsAbstract { get { return false; } }		
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompClass : CompNamed, ICompAttributeContainer, ICompTypeContainer, ICompMemberContainer, ICompHasBaseType, ICompGenericParameter {
		public CompClass (string name, CompType type)
			: base (name, type)
		{
		}

		public abstract List<CompNamed> GetInterfaces();
		public abstract List<CompNamed> GetConstructors();
		public abstract List<CompNamed> GetMethods();
 		public abstract List<CompNamed> GetProperties();
 		public abstract List<CompNamed> GetFields();
 		public abstract List<CompNamed> GetEvents();

		public abstract List<CompNamed> GetAttributes ();

		public abstract List<CompNamed> GetNestedClasses();
		public abstract List<CompNamed> GetNestedInterfaces ();
		public abstract List<CompNamed> GetNestedStructs ();
		public abstract List<CompNamed> GetNestedEnums ();
		public abstract List<CompNamed> GetNestedDelegates ();
		
		public abstract string GetBaseType();
		public abstract bool IsSealed { get; }
		public abstract bool IsAbstract { get; }
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompMember : CompNamed, ICompAttributeContainer {
		public CompMember (string name, CompType type)
			: base (name, type)
		{
		}

		public abstract string GetMemberAccess();
		public abstract string GetMemberType();
		
		public abstract List<CompNamed> GetAttributes ();
	}

	public abstract class CompMethod : CompMember, ICompGenericParameter {
		public CompMethod (string name)
			: base (name, CompType.Method)
		{
		}

		public abstract bool ThrowsNotImplementedException ();
		
		public abstract List<CompGenericParameter> GetTypeParameters ();
	}

	public abstract class CompProperty : CompMember, ICompMemberContainer {
		public CompProperty (string name)
			: base (name, CompType.Property)
		{
		}
		
		public abstract List<CompNamed> GetMethods();
		public List<CompNamed> GetInterfaces() { return new List<CompNamed>(); }
		public List<CompNamed> GetConstructors() { return new List<CompNamed>(); }
		public List<CompNamed> GetEvents() { return new List<CompNamed>(); }
		public List<CompNamed> GetFields() { return new List<CompNamed>(); }
		public List<CompNamed> GetProperties() { return new List<CompNamed>(); }
	}

	public abstract class CompField : CompMember {
		public CompField (string name)
			: base (name, CompType.Field)
		{
		}
		public abstract string GetLiteralValue ();
	}

	public abstract class CompEvent : CompMember {
		public CompEvent (string name)
			: base (name, CompType.Event)
		{
		}		
	}
	
	public abstract class CompAttribute : CompNamed {
		public CompAttribute (string typename)
			: base (typename, CompType.Attribute)
		{
		}
	}
	
	public abstract class CompGenericParameter : CompNamed, ICompAttributeContainer {
		
		public readonly Mono.Cecil.GenericParameterAttributes GenericAttributes;
		
		public CompGenericParameter (string name, Mono.Cecil.GenericParameterAttributes attr)
			: base (name, CompType.GenericParameter)
		{
			GenericAttributes = attr;
		}
		
		public abstract List<CompNamed> GetAttributes ();
		
		public static string GetGenericAttributeDesc (Mono.Cecil.GenericParameterAttributes ga)
		{
			return ga.ToString ();
		}
	}
}
