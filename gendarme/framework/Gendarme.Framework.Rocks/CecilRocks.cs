//
// Gendarme.Framework.Rocks.CecilRocks
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

namespace Gendarme.Framework.Rocks {

	// Here we keep general, Cecil related, rocks

	public static class IMetadataTokenProviderRock {

		/// <summary>
		/// Return the AssemblyDefinition that correspond to the IMetadataTokenProvider
		/// based object.
		/// </summary>
		/// <param name="self">The IMetadataTokenProvider instance where the method is applied.</param>
		/// <returns>The AssemblyDefinition associated with the IMetadataTokenProvider or null
		/// if none can be found</returns>
		public static AssemblyDefinition GetAssembly (this IMetadataTokenProvider self)
		{
			AssemblyDefinition ad = (self as AssemblyDefinition);
			if (ad != null)
				return ad;

			TypeDefinition td = (self as TypeDefinition);
			if (td != null)
				return td.Module.Assembly;

			MethodDefinition md = (self as MethodDefinition);
			if (md != null)
				return md.DeclaringType.Module.Assembly;

			FieldDefinition fd = (self as FieldDefinition);
			if (fd != null)
				return fd.DeclaringType.Module.Assembly;

			ParameterDefinition pd = (self as ParameterDefinition);
			if (pd != null)
				return pd.Method.DeclaringType.Module.Assembly;

			return null;
		}

		/// <summary>
		/// Compare IMetadataTokenProvider instances based on their metadata token and their
		/// assembly.
		/// </summary>
		/// <param name="self">The IMetadataTokenProvider instance where the method is applied.</param>
		/// <param name="other">The IMetadataTokenProvider instance to compare to</param>
		/// <returns>True if the metadata tokens and assembly are identical, False otherwise</returns>
		public static bool Equals (this IMetadataTokenProvider self, IMetadataTokenProvider other)
		{
			if (self == other)
				return true;
			if (other == null)
				return false;
			if (!self.MetadataToken.Equals (other.MetadataToken))
				return false;
			// metadata token is unique per assembly
			return GetAssembly (self).ToString () == GetAssembly (other).ToString ();
		}
	}
}
