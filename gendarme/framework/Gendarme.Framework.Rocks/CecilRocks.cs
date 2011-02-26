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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Metadata;

using Gendarme.Framework.Helpers;

namespace Gendarme.Framework.Rocks {

	// Here we keep general, Cecil related, rocks

	public static class MetadataTokenProviderRock {

		/// <summary>
		/// Return the AssemblyDefinition that correspond to the IMetadataTokenProvider
		/// based object.
		/// </summary>
		/// <param name="self">The IMetadataTokenProvider instance where the method is applied.</param>
		/// <returns>The AssemblyDefinition associated with the IMetadataTokenProvider or null
		/// if none can be found</returns>
		public static AssemblyDefinition GetAssembly (this IMetadataTokenProvider self)
		{
			if (self == null)
				return null;

			MetadataToken token = self.MetadataToken;
			switch (token.TokenType) {
			case TokenType.Assembly:
				return (self as AssemblyDefinition);
			case TokenType.Module:
				// Module == 0, so we need to handle MetadataToken.Zero here
				if (token.RID == 0) {
					// if we don't have a valid token then we take the slow path
					return GetAssemblyUsingCasts (self);
				} else {
					return (self as ModuleDefinition).Assembly;
				}
			case TokenType.GenericParam:
				return GetAssembly ((self as GenericParameter).DeclaringType);
			case TokenType.TypeRef:
			case TokenType.TypeDef:
				return GetAssembly (self as TypeReference);
			case TokenType.Method:
				return GetAssembly (self as MethodReference);
			case TokenType.Event:
				return GetAssembly ((self as EventDefinition).DeclaringType);
			case TokenType.Field:
				return GetAssembly ((self as FieldDefinition).DeclaringType);
			case TokenType.Property:
				return GetAssembly ((self as PropertyDefinition).DeclaringType);
			case TokenType.Param:
				return GetAssembly ((self as ParameterDefinition).Method);
			// NamespaceDefinition is a Gendarme "extention", i.e. not real metadata, and does not belong in a single assembly
			case NamespaceDefinition.NamespaceTokenType:
				return null;
			default:
				return GetAssemblyUsingCasts (self);
			}
		}

		static AssemblyDefinition GetAssemblyUsingCasts (IMetadataTokenProvider metadata)
		{
			AssemblyDefinition ad = (metadata as AssemblyDefinition);
			if (ad != null)
				return ad;
			ModuleDefinition md = (metadata as ModuleDefinition);
			if (md != null)
				return md.Assembly;
			GenericParameter gp = (metadata as GenericParameter); // needs to be before TypeReference
			if (gp != null)
				return GetAssembly (gp.DeclaringType);
			TypeReference tr = (metadata as TypeReference);
			if (tr != null)
				return GetAssembly (tr);
			MethodReference mr = (metadata as MethodReference);
			if (mr != null)
				return GetAssembly (mr);
			EventDefinition ed = (metadata as EventDefinition);
			if (ed != null)
				return GetAssembly (ed.DeclaringType);
			FieldDefinition fd = (metadata as FieldDefinition);
			if (fd != null)
				return GetAssembly (fd.DeclaringType);
			PropertyDefinition pd = (metadata as PropertyDefinition);
			if (pd != null)
				return GetAssembly (pd.DeclaringType);
			ParameterDefinition paramd = (metadata as ParameterDefinition);
			if (paramd != null)
				return GetAssembly (paramd.Method);
			MethodReturnType mrt = (metadata as MethodReturnType);
			if (mrt != null)
				return GetAssembly (mrt.Method);
			return null;
		}

		static AssemblyDefinition GetAssembly (MemberReference method)
		{
			return method.Module.Assembly;
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
				return (self == null);

			MetadataToken token = self.MetadataToken;
			if (!token.Equals (other.MetadataToken))
				return false;

			// metadata token is unique per assembly
			AssemblyDefinition self_assembly = GetAssembly (self);
			if (self_assembly == null) {
				// special case for Namespace (where GetAssembly would return null)
				if (token.TokenType == NamespaceDefinition.NamespaceTokenType)
					return (self as NamespaceDefinition).Name == (other as NamespaceDefinition).Name;
				else
					return false;
			}
			AssemblyDefinition other_assembly = GetAssembly (other);
			// compare assemblies tokens (but do not recurse)
			return other == null ? false : self_assembly.MetadataToken.Equals (other_assembly.MetadataToken);
		}

		static Dictionary<MemberReference, string> full_name_cache = new Dictionary<MemberReference, string> ();

		/// <summary>
		/// Get the string value of the MemberReference FullName property without the cost 
		/// of allocating a new string for each (or most) calls. 
		/// </summary>
		/// <param name="self">The MemberReference instance where the method is applied.</param>
		/// <returns>The cached FullName property of the MemberReference</returns>
		/// <remarks>Cecil needs to rebuild most of the FullName properties on each call in order to
		/// be able to write assemblies. However this is a waste of memory when an application, like 
		/// Gendarme, use it for read-only purposes.</remarks>
		public static string GetFullName (this MemberReference self)
		{
			if (self == null)
				return String.Empty;

			string full_name;
			if (!full_name_cache.TryGetValue (self, out full_name)) {
				full_name = self.FullName;
				full_name_cache.Add (self, full_name);
			}

			return full_name;
		}
	}
}
