//
// Test.Rules.Definitions.SimpleMethods
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Text;

using Test.Rules.Helpers;

using Mono.Cecil;

namespace Test.Rules.Definitions {

	/// <summary>
	/// Class that holds references to some widely-used methods while testing.
	/// </summary>
	public class SimpleMethods {

		/// <summary>
		/// A method to be used as an external one.
		/// </summary>
		[DllImport ("libc.so")]
		private static extern void strncpy (StringBuilder dest, string src, uint n);

		private SimpleMethods ()
		{
		}

		/// <summary>
		/// Just a simple instance method with empty body.
		/// </summary>
		private void DoNothing ()
		{
		}

		/// <summary>
		/// A method decorated with the [GeneratedCode] attribute
		/// </summary>
		[GeneratedCode ("Gendarme", "2.2")]
		private string Generated ()
		{
			return String.Empty;
		}

		private static MethodDefinition external_method;
		private static MethodDefinition empty_method;
		private static MethodDefinition generated_method;

		/// <value>
		/// Gets a MethodDefinition to private and static external method.
		/// </value>
		public static MethodDefinition ExternalMethod {
			get {
				if (external_method == null)
					external_method = DefinitionLoader.GetMethodDefinition<SimpleMethods> ("strncpy");
				return external_method;
			}
		}

		/// <value>
		/// Gets a MethodDefinition to private instance empty method. 
		/// </value>
		public static MethodDefinition EmptyMethod {
			get {
				if (empty_method == null)
					empty_method = DefinitionLoader.GetMethodDefinition<SimpleMethods> ("DoNothing");
				return empty_method;
			}
		}

		/// <value>
		/// Gets a MethodDefinition to private instance generated method. 
		/// </value>
		public static MethodDefinition GeneratedCodeMethod {
			get {
				if (generated_method == null)
					generated_method = DefinitionLoader.GetMethodDefinition<SimpleMethods> ("Generated");
				return generated_method;
			}
		}
	}
}
