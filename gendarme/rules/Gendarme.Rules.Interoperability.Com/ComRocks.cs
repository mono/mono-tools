//
// Gendarme.Rules.Interoperability.Com.ComRocks
//
// Authors:
//	N Lum <nol888@gmail.com>
//	Yuri Stuken <stuken.yuri@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 N Lum
// Copyright (C) 2010 Yuri Stuken
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// ComRocks contains extensions methods for COM-related methods.
	/// </summary>
	public static class ComRocks {
		/// <summary>
		/// Check if the type is explicitly declared to be ComVisible.
		/// </summary>
		/// <param name="self">The ICustomAttributeProvider (e.g. AssemblyDefinition, TypeReference, MethodReference,
		/// FieldReference...) on which the extension method can be called.</param>
		/// <returns><code>null</code> no ComVisible attribute is present, <code>true</code> if ComVisible is set to true, <code>false</code> otherwise.</returns>
		public static bool? IsComVisible (this ICustomAttributeProvider self)
		{
			if (self == null)
				return null;

			if (self.HasCustomAttributes) {
				foreach (CustomAttribute attribute in self.CustomAttributes) {
					// ComVisibleAttribute has a single ctor taking a boolean value
					// http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.comvisibleattribute.comvisibleattribute.aspx
					// any attribute without arguments can be skipped
					if (!attribute.HasConstructorArguments)
						continue;
					if (!attribute.Constructor.DeclaringType.IsNamed ("System.Runtime.InteropServices", "ComVisibleAttribute"))
						continue;
					return (bool) attribute.ConstructorArguments[0].Value;
				}
			}

			// special case for types, check if this is a nested type inside a [ComVisible] type
			TypeDefinition type = (self as TypeDefinition);
			if (type == null)
				return null;

			return type.DeclaringType.IsComVisible ();
		}

		// Checks whether specific type is COM visible or not
		// considering nested types, assemblies attributes and default values
		public static bool IsTypeComVisible (this TypeDefinition self)
		{
			// [ComVisible] attribute will be ignored on non-visible types
			if (!self.IsVisible ())
				return false;

			return (self.IsComVisible () ?? self.Module.Assembly.IsComVisible () ?? true);
		}
	}
}
