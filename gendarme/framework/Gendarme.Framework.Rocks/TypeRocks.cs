//
// Gendarme.Framework.Rocks.TypeRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Daniel Abramov <ex@vingrad.ru>
//	Adrian Tsai <adrian_tsai@hotmail.com>
//	Andreas Noever <andreas.noever@gmail.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
// (C) 2007 Daniel Abramov
// Copyright (c) 2007 Adrian Tsai
// (C) 2008 Andreas Noever
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework.Helpers;

namespace Gendarme.Framework.Rocks {

	// add Type[Definition|Reference] extensions methods here
	// only if:
	// * you supply minimal documentation for them (xml)
	// * you supply unit tests for them
	// * they are required somewhere to simplify, even indirectly, the rules
	//   (i.e. don't bloat the framework in case of x, y or z in the future)

	/// <summary>
	/// TypeRocks contains extensions methods for Type[Definition|Reference]
	/// and the related collection classes.
	/// 
	/// Note: whenever possible try to use TypeReference since it's extend the
	/// reach/usability of the code.
	/// </summary>
	public static class TypeRocks {

		/// <summary>
		/// Return an IEnumerable that allows a single loop (like a foreach) to
		/// traverse all MethodDefinition in the type. That includes the Constructors
		/// and Methods collections.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>An IEnumerable to traverse all constructors and methods</returns>
		public static IEnumerable<MethodDefinition> AllMethods (this TypeReference self)
		{
			TypeDefinition type = self.Resolve ();
			foreach (MethodDefinition ctor in type.Constructors)
				yield return ctor;
			foreach (MethodDefinition method in type.Methods)
				yield return method;
		}

		/// <summary>
		/// Check if a type reference collection contains a type of a specific name.
		/// </summary>
		/// <param name="self">The TypeReferenceCollection on which the extension method can be called.</param>
		/// <param name="typeName">Full name of the type.</param>
		/// <returns>True if the collection contains an type of the same name,
		/// False otherwise.</returns>
		public static bool ContainsType (this TypeReferenceCollection self, string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			foreach (TypeReference type in self) {
				if (type.FullName == typeName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Check if a type reference collection contains any of the specified type names.
		/// </summary>
		/// <param name="self">The TypeReferenceCollection on which the extension method can be called.</param>
		/// <param name="typeNames">A string array of full type names.</param>
		/// <returns>True if the collection contains any types matching one specified,
		/// False otherwise.</returns>
		public static bool ContainsAnyType (this TypeReferenceCollection self, string [] typeNames)
		{
			if (typeNames == null)
				throw new ArgumentNullException ("typeNames");

			foreach (TypeReference type in self) {
				string fullname = type.FullName;
				foreach (string type_full_name in typeNames) {
					if (fullname == type_full_name)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns the first MethodDefinition that satisfies a given MethodSignature.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="signature">The MethodSignature to match.</param>
		/// <returns>The first MethodDefinition for wich signature.Matches returns true.</returns>
		/// <remarks>
		/// Do not allocate a MethodSignature for only one call. Use one of the other GetMethod overloads instead.
		/// </remarks>
		public static MethodDefinition GetMethod (this TypeReference self, MethodSignature signature)
		{
			foreach (MethodDefinition method in self.AllMethods ()) {
				if (signature.Matches (method))
					return method;
			}
			return null;
		}

		/// <summary>
		/// Searches for a method.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="attributes">An attribute mask matched against the attributes of the method.</param>
		/// <param name="name">The name of the method to match. Ignored if null.</param>
		/// <param name="returnType">The full name (Namespace.Type) of the return type. Ignored if null.</param>
		/// <param name="parameters">An array of full names (Namespace.Type) of parameter types. Ignored if null. Null entries act as wildcards.</param>
		/// <param name="customCondition">A custom condition that is called for each MethodDefinition that satisfies all other conditions. Ignored if null.</param>
		/// <returns>The first MethodDefinition that satisfies all conditions.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, MethodAttributes attributes, string name, string returnType, string [] parameters, Func<MethodDefinition, bool> customCondition)
		{
			foreach (MethodDefinition method in self.AllMethods ()) {
				if (name != null && method.Name != name)
					continue;
				if ((method.Attributes & attributes) != attributes)
					continue;
				if (returnType != null && method.ReturnType.ReturnType.FullName != returnType)
					continue;
				if (parameters != null) {
					if (parameters.Length != method.Parameters.Count)
						continue;
					bool parameterError = false;
					for (int i = 0; i < parameters.Length; i++) {
						if (parameters [i] == null)
							continue;//ignore parameter
						if (parameters [i] != method.Parameters [i].ParameterType.FullName) {
							parameterError = true;
							break;
						}
					}
					if (parameterError)
						continue; // there could be an overload with the "right" parameters
				}
				if (customCondition != null && !customCondition (method))
					continue;
				return method;
			}
			return null;
		}

		/// <summary>
		/// Searches for a method by name, returnType, parameters and attributes.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="attributes">An attribute mask matched against the attributes of the method.</param>
		/// <param name="name">The name of the method to match. Ignored if null.</param>
		/// <param name="returnType">The full name (Namespace.Type) of the return type. Ignored if null.</param>
		/// <param name="parameters">An array of full names (Namespace.Type) of parameter types. Ignored if null. Null entries act as wildcard.</param>
		/// <returns>The first MethodDefinition that satisfies all conditions.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, MethodAttributes attributes, string name, string returnType, string [] parameters)
		{
			return self.GetMethod (attributes, name, returnType, parameters, null);
		}

		/// <summary>
		/// Searches for a method by attributes and by name.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="attributes">An attribute mask matched against the attributes of the method.</param>
		/// <param name="name">The name of the method to match. Ignored if null.</param>
		/// <returns>The first MethodDefinition that satisfies all conditions.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, MethodAttributes attributes, string name)
		{
			return self.GetMethod (attributes, name, null, null, null);
		}

		/// <summary>
		/// Searches for a method by name, returnType and parameters.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="name">The name of the method to match. Ignored if null.</param>
		/// <param name="returnType">The full name (Namespace.Type) of the return type. Ignored if null.</param>
		/// <param name="parameters">An array of full names (Namespace.Type) of parameter types. Ignored if null. Null entries act as wildcards.</param>
		/// <returns>The first MethodDefinition that satisfies all conditions.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, string name, string returnType, string [] parameters)
		{
			return self.GetMethod (0, name, returnType, parameters, null);
		}

		/// <summary>
		/// Searches for a method with a specific name.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="name">The name of the method to match.</param>
		/// <returns>The first MethodDefinition with a specifiy name.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, string name)
		{
			return self.GetMethod (0, name, null, null, null);
		}

		/// <summary>
		/// Searches for a method using a custom condition.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="customCondition">A custom condition that is called for each MethodDefinition.</param>
		/// <returns>The first MethodDefinition that satisfies the customCondition.</returns>
		public static MethodDefinition GetMethod (this TypeReference self, Func<MethodDefinition, bool> customCondition)
		{
			return self.GetMethod (0, null, null, null, customCondition);
		}

		/// <summary>
		/// Check if the type contains an attribute of a specified type.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="attributeName">Full name of the attribute class</param>
		/// <returns>True if the type contains an attribute of the same name,
		/// False otherwise.</returns>
		public static bool HasAttribute (this TypeReference self, string attributeName)
		{
			return self.CustomAttributes.ContainsType (attributeName);
		}

		/// <summary>
		/// Checks if at least one Method satisfies a given MethodSignature.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="signature">The MethodSignature to match.</param>
		/// <returns>True if at least one method matches the signature. Otherwise false.</returns>
		public static bool HasMethod (this TypeReference self, MethodSignature signature)
		{
			return (self.GetMethod (signature) != null);
		}

		/// <summary>
		/// Check if the type implemented a specified interface. Note that it is possible that
		/// we might now be able to know all implementations since the assembly where 
		/// the information resides could be unavailable.
		/// </summary>
		/// <param name="self">The TypeDefinition on which the extension method can be called.</param>
		/// <param name="interfaceName">Full name of the interface</param>
		/// <returns>True if the type implements the interface, False otherwise.</returns>
		public static bool Implements (this TypeReference self, string interfaceName)
		{
			if (interfaceName == null)
				throw new ArgumentNullException ("interfaceName");

			bool generic = (interfaceName.IndexOf ('`') >= 0);

			TypeDefinition type = self.Resolve ();
			if (type == null)
				return false;		// could not resolve

			// special case, check if we implement ourselves
			if (type.IsInterface && (type.FullName == interfaceName))
				return true;

			// does the type implements it itself
			foreach (TypeReference iface in type.Interfaces) {
				string fullname = (generic) ? iface.GetOriginalType ().FullName : iface.FullName;
				if (fullname == interfaceName)
					return true;
			}
			
			// if not, then maybe it's parent does
			TypeReference parent = type.BaseType;
			if (parent != null)
				return parent.Implements (interfaceName);

			return false;
		}

		/// <summary>
		/// Check if the type inherits from the specified type. Note that it is possible that
		/// we might not be able to know the complete inheritance chain since the assembly 
		/// where the information resides could be unavailable.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <param name="className">Full name of the base class</param>
		/// <returns>True if the type inherits from specified class, False otherwise</returns>
		public static bool Inherits (this TypeReference self, string className)
		{
			if (className == null)
				throw new ArgumentNullException ("className");

			TypeReference current = self.Resolve ();
			while ((current != null) && (current.FullName != "System.Object")) {
				if (current.FullName == className)
					return true;

				TypeDefinition td = current.Resolve ();
				if (td == null)
					return false;		// could not resolve type
				current = td.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Check if the type represent an array (of any other type).
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type is an array, False otherwise</returns>
		public static bool IsArray (this TypeReference self)
		{
			return (self is ArrayType);
		}

		/// <summary>
		/// Checks if type is attribute. Note that it is possible that
		/// we might now be able to know all inheritance since the assembly where 
		/// the information resides could be unavailable.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type inherits from <c>System.Attribute</c>, 
		/// False otherwise.</returns>
		public static bool IsAttribute (this TypeReference self)
		{
			return self.Inherits ("System.Attribute");
		}

		/// <summary>
		/// Check if the type is a delegate.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type is a delegate, False otherwise.</returns>
		public static bool IsDelegate (this TypeReference self)
		{
			TypeDefinition type = self.Resolve ();
			// e.g. this occurs for <Module> or GenericParameter
			if (null == type || type.BaseType == null)
				return false;

			switch (type.BaseType.FullName) {
			case "System.Delegate":
			case "System.MulticastDelegate":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check if the type is a enumeration flags.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type as the [Flags] attribute, false otherwise.</returns>
		public static bool IsFlags (this TypeReference self)
		{
			TypeDefinition type = self.Resolve ();
			if (!type.IsEnum)
				return false;

			return type.HasAttribute ("System.FlagsAttribute");
		}

		/// <summary>
		/// Check if the type represent a floating-point type.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type is System.Single (C# float) or System.Double (C3 double), False otherwise.</returns>
		public static bool IsFloatingPoint (this TypeReference self)
		{
			return ((self.FullName == Mono.Cecil.Constants.Single) ||
				(self.FullName == Mono.Cecil.Constants.Double));
		}

		/// <summary>
		/// Check if the type is generated code, either by the compiler or by a tool.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the code is not generated directly by the developer, 
		/// False otherwise (e.g. compiler or tool generated)</returns>
		public static bool IsGeneratedCode (this TypeReference self)
		{
			// both helpful attributes only exists in 2.0 and more recent frameworks
			if (self.Module.Assembly.Runtime >= TargetRuntime.NET_2_0) {
				if (self.CustomAttributes.ContainsAnyType (CustomAttributeRocks.GeneratedCodeAttributes))
					return true;
			}

			// sadly <Module> still shows up for 2.0, so the 1.x logic still applies
			switch (self.Name [0]) {
			case '<': // e.g. <Module>, <PrivateImplementationDetails>
			case '$': // e.g. $ArrayType$1 nested inside <PrivateImplementationDetails>
				return true;
			}

			// the type could be nested (inside a generated one) and not marked itself
			if (self.IsNested)
				return self.DeclaringType.IsGeneratedCode ();
			return false;
		}

		/// <summary>
		/// Check if the type refers to native code.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type refers to native code, False otherwise</returns>
		public static bool IsNative (this TypeReference self)
		{
			switch (self.FullName) {
			case "System.IntPtr":
			case "System.UIntPtr":
			case "System.Runtime.InteropServices.HandleRef":
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Check if the type is static (2.0+)
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type is static, false otherwise.</returns>
		public static bool IsStatic (this TypeReference self)
		{
			TypeDefinition type = self.Resolve ();
			if (type == null)
				return false;
			return (type.IsSealed && type.IsAbstract);
		}

		/// <summary>
		/// Check if the type is visible outside of the assembly.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>True if the type can be used from outside of the assembly, false otherwise.</returns>
		public static bool IsVisible (this TypeReference self)
		{
			TypeDefinition type = self.Resolve ();
			if (type == null)
				return true; // it's probably visible since we have a reference to it

			while (type.IsNested) {
				if (type.IsNestedPrivate || type.IsNestedAssembly)
					return false;
				// Nested classes are always inside the same assembly, so the cast is ok
				type = type.DeclaringType.Resolve ();
			}
			return type.IsPublic;
		}

		/// <summary>
		/// Resolve a TypeReference into a TypeDefinition.
		/// </summary>
		/// <param name="self">The TypeReference on which the extension method can be called.</param>
		/// <returns>A TypeDefinition if resolved, null otherwise.</returns>
		public static TypeDefinition Resolve (this TypeReference self)
		{
			// this can occurs, e.g. generic parameters that needs recursive resolves
			if (self == null)
				return null;

			TypeDefinition type = (self as TypeDefinition);
			if (type == null)
				type = AssemblyResolver.Resolver.Resolve (self);
			return type;
		}
	}
}
