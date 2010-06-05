// 
// Gendarme.Framework.Helpers.NamespaceDefinition
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Namespaces do not really exists in the CLR, at least not like other first level citizens.
	/// Since we want to report defects against them we need something to fill this void.
	/// </summary>
	public class NamespaceDefinition : IMetadataTokenProvider {

		internal const TokenType NamespaceTokenType = (TokenType) 0xFF000000;

		private string ns;

		private NamespaceDefinition (string name)
		{
			ns = name;
			MetadataToken = new MetadataToken (NamespaceTokenType, (uint) name.GetHashCode ());
		}

		public string Name {
			get { return ns; }
		}

		/// <summary>
		/// This is not a true, CLR-wise, metadata object but it 
		/// returns a fake token so other API, like rocks, can use it
		/// like any real <c>IMetadataTokenProvider</c>
		/// </summary>
		public MetadataToken MetadataToken {
			get; set;
		}

		public override string ToString ()
		{
			return ns;
		}


		static string [] Specializations = { 
			".Design", 
			".Interop", 
			".Permissions"
		};

		/// <summary>
		/// Check if the specified namespace is a 'specialized' namespace, i.e. a 
		/// namespace that the framework suggest you to use.
		/// </summary>
		/// <param name="name">Namespace to be verified.</param>
		/// <returns>True if the namespace is a specialized namespace.</returns>
		static public bool IsSpecialized (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			foreach (string s in Specializations) {
				if (name.EndsWith (s, StringComparison.Ordinal))
					return true;
			}
			return false;
		}

		static Dictionary<string, NamespaceDefinition> cache = new Dictionary<string, NamespaceDefinition> ();

		/// <summary>
		/// Get the NamespaceDefinition that correspond to the specified namespace.
		/// </summary>
		/// <param name="name">Name of the namespace</param>
		/// <returns>A global NamespaceDefinition corresponding to the specified namespace</returns>
		static public NamespaceDefinition GetDefinition (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			// note: "" (empty) is a valid namespace
			
			NamespaceDefinition nd;
			if (!cache.TryGetValue (name, out nd)) {
				nd = new NamespaceDefinition (name);
				cache.Add (name, nd);
			}
			return nd;
		}
	}
}
