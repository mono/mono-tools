//
// Gendarme.Framework.MethodSignature
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

using Mono.Cecil;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Used to match methods. Properties that are set to null are ignored
	/// </summary>
	/// <example>
	/// <code>
	/// MethodDefinition method = ...
	/// MethodSignature sig = new MethodSignature ("Dispose");
	/// if (sig.Match (method)) { 
	///     //matches any method named "Dispose" with any (or no) return value and any number of parameters
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Gendarme.Framework.Helpers.MethodSignatures"/>
	public class MethodSignature {

		/// <summary>
		/// The name of the method to match. Ignored if null.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The FullName (Namespace.Type) of the return type. Ignored if null.
		/// </summary>
		public string ReturnType { get; private set; }

		/// <summary>
		/// An array of FullNames (Namespace.Type) of parameter types. Ignored if null. Null entries act as wildcards.
		/// </summary>
		public ReadOnlyCollection<string> Parameters { get; private set; }

		/// <summary>
		/// An attribute mask matched against the attributes of the method.
		/// </summary>
		public MethodAttributes Attributes { get; private set; }


		public MethodSignature ()
		{
		}

		public MethodSignature (string name)
			: this (name, null, null)
		{
		}

		public MethodSignature (string name, string returnType)
			: this (name, returnType, null)
		{
		}

		public MethodSignature (string name, string returnType, string[] parameters)
		{
			Name = name;
			ReturnType = returnType;
			if (parameters != null)
				Parameters = new ReadOnlyCollection<string> (new List<string> (parameters));
		}

		public MethodSignature (string name, string returnType, string [] parameters, MethodAttributes attributes)
			: this (name, returnType, parameters)
		{
			Attributes = attributes;
		}

		/// <summary>
		/// Checks if a MethodReference match the signature.
		/// </summary>
		/// <param name="method">The method to check.</param>
		/// <returns>True if the MethodReference matches all aspects of the MethodSignature.</returns>
		public bool Matches (MethodReference method)
		{
			if (method == null)
				return false;

			if (Name != null && method.Name != Name)
				return false;

			if (ReturnType != null && method.ReturnType.ReturnType.FullName != ReturnType)
				return false;

			if (Parameters != null) {
				if (method.HasParameters) {
					if (Parameters.Count != method.Parameters.Count)
						return false;
					for (int i = 0; i < Parameters.Count; i++) {
						if (Parameters [i] == null)
							continue;//ignore parameter
						if (Parameters [i] != method.Parameters [i].ParameterType.FullName) {
							return false;
						}
					}
				} else if (Parameters.Count > 0) {
					return false;
				}
			}

			// skip last check if no attributes are part of the signature
			if (((int) Attributes) == 0)
				return true;

			// put this at last step so we avoid the cast as much as possible
			MethodDefinition md = (method as MethodDefinition);
			return ((md == null) || ((md.Attributes & Attributes) == Attributes));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString ()
		{
			// if we do not have enough useful information return an empty string
			if (Name == null)
				return String.Empty;

			StringBuilder sb = new StringBuilder ();
			if (ReturnType != null) {
				sb.Append (ReturnType);
				sb.Append (' ');
			}

			sb.Append (Name);
			sb.Append ('(');
			if (Parameters != null) {
				for (int i = 0; i < Parameters.Count; i++) {
					sb.Append (Parameters [i]);
					if (i < Parameters.Count - 1)
						sb.Append (',');
				}
			}
			sb.Append (')');

			return sb.ToString ();
		}
	}
}
