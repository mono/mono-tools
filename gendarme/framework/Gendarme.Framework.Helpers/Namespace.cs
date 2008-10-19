// 
// Gendarme.Framework.Helpers.Namespace
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Namespaces do not really exists in the CLR, at least not like other first level citizens.
	/// Since we want to report defects against them we need something to fill this void.
	/// </summary>
	public class Namespace : IMetadataTokenProvider {

		private string ns;

		public Namespace (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			// "" (empty) is a valid namespace
			ns = name;
		}

		public string Name {
			get { return ns; }
		}

		/// <summary>
		/// This is not a true, CLR-wise, metadata object so this always return
		/// <c>MetadataToken.Zero</c> and cannot be set to any other value.
		/// </summary>
		public MetadataToken MetadataToken {
			get { return MetadataToken.Zero; }
			set { ; }
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
				if (name.EndsWith (s))
					return true;
			}
			return false;
		}
	}
}
