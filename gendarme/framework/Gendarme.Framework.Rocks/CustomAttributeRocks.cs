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
using System.Collections.Generic;

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

		internal static bool HasAnyGeneratedCodeAttribute (this ICustomAttributeProvider self)
		{
			if ((self == null) || !self.HasCustomAttributes)
				return false;

			foreach (CustomAttribute ca in self.CustomAttributes) {
				TypeReference cat = ca.AttributeType;
				if (cat.IsNamed ("System.CodeDom.Compiler", "GeneratedCodeAttribute") ||
					cat.IsNamed ("System.Runtime.CompilerServices", "CompilerGeneratedAttribute")) {
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
		/// <param name="nameSpace">The namespace of the attribute to be matched</param>
		/// <param name="name">The name of the attribute to be matched</param>
		/// <returns>True if the provider contains an attribute of the same name,
		/// False otherwise.</returns>
		public static bool HasAttribute (this ICustomAttributeProvider self, string nameSpace, string name)
		{
			if (nameSpace == null)
				throw new ArgumentNullException ("nameSpace");
			if (name == null)
				throw new ArgumentNullException ("name");

			if ((self == null) || !self.HasCustomAttributes)
				return false;

			foreach (CustomAttribute ca in self.CustomAttributes) {
				if (ca.AttributeType.IsNamed (nameSpace, name))
					return true;
			}
			return false;
		}
	}
}
