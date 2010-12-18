//
// Gendarme.Rules.Interoperability.Com.ComRocks
//
// Authors:
//	N Lum <nol888@gmail.com>
// 
// Copyright (C) 2010 N Lum
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

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// ComRocks contains extensions methods for Com-related methods.
	/// </summary>
	public static class ComRocks {
		/// <summary>
		/// Check if the type is explicitly declared to be ComVisible.
		/// </summary>
		/// <param name="self">The ICustomAttributeProvider (e.g. AssemblyDefinition, TypeReference, MethodReference,
		/// FieldReference...) on which the extension method can be called.</param>
		/// <returns><code>null</code> no ComVisible attribute is present, <code>true</code> if ComVisible is set to true, <code>false</code> otherwise.</returns>
		public static bool? IsComVisible(this ICustomAttributeProvider self)
		{
			if ((self == null) || !self.HasCustomAttributes)
				return null;

			foreach (CustomAttribute attribute in self.CustomAttributes) {
				if (attribute.Constructor.DeclaringType.FullName != "System.Runtime.InteropServices.ComVisibleAttribute")
					continue;
				return (bool) attribute.ConstructorArguments[0].Value;
			}

			return null;
		}
	}
}
