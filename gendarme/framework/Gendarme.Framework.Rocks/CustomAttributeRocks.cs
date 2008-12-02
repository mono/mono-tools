//
// Gendarme.Framework.Rocks.CustomAttributeRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

namespace Gendarme.Framework.Rocks {

	// add CustomAttribute[Collection], ICustomAttributeProvider extensions 
	// methods here only if:
	// * you supply minimal documentation for them (xml)
	// * you supply unit tests for them
	// * they are required somewhere to simplify, even indirectly, the rules
	//   (i.e. don't bloat the framework in case of x, y or z in the future)

	/// <summary>
	/// CustomAttributeRocks contains extensions methods for CustomAttribute
	/// and the related collection classes.
	/// </summary>
	public static class CustomAttributeRocks {

		internal static string [] GeneratedCodeAttributes = {
			 "System.CodeDom.Compiler.GeneratedCodeAttribute",
			 "System.Runtime.CompilerServices.CompilerGeneratedAttribute"
		};

		/// <summary>
		/// Check if the custom attribute collection contains an attribute of a specified type.
		/// </summary>
		/// <param name="self">The CustomAttributeCollection on which the extension method can be called.</param>
		/// <param name="attributeTypeName">Full type name of the attribute class.</param>
		/// <returns>True if the collection contains an attribute of the same name,
		/// False otherwise.</returns>
		public static bool ContainsType (this CustomAttributeCollection self, string attributeTypeName)
		{
			if (attributeTypeName == null)
				throw new ArgumentNullException ("attributeTypeName");

			foreach (CustomAttribute ca in self) {
				if (ca.Constructor.DeclaringType.FullName == attributeTypeName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Check if the custom attribute collection contains any of the specified type.
		/// </summary>
		/// <param name="self">The CustomAttributeCollection on which the extension method can be called.</param>
		/// <param name="attributeTypeNames">A strings array of full type names of the attributes.</param>
		/// <returns>True if the collection contains any attribute matching one specified,
		/// False otherwise.</returns>
		public static bool ContainsAnyType (this CustomAttributeCollection self, string[] attributeTypeNames)
		{
			if (attributeTypeNames == null)
				throw new ArgumentNullException ("attributeTypeNames");

			foreach (CustomAttribute ca in self) {
				string fullname = ca.Constructor.DeclaringType.FullName;
				foreach (string attribute_full_name in attributeTypeNames) {
					if (fullname == attribute_full_name)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Check if the type contains an attribute of a specified type.
		/// </summary>
		/// <param name="self">The ICustomAttributeProvider (e.g. AssemblyDefinition, TypeReference, MethodReference,
		/// FieldReference...) on which the extension method can be called.</param>
		/// <param name="attributeName">Full name of the attribute class</param>
		/// <returns>True if the provider contains an attribute of the same name,
		/// False otherwise.</returns>
		public static bool HasAttribute (this ICustomAttributeProvider self, string attributeName)
		{
			if (attributeName == null)
				throw new ArgumentNullException ("attributeName");

			if ((self == null) || !self.HasCustomAttributes)
				return false;
			return self.CustomAttributes.ContainsType (attributeName);
		}
	}
}
