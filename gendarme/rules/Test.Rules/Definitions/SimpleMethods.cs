//
// Test.Rules.Definitions.SimpleMethods
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Runtime.InteropServices;
using System.Text;

using Test.Rules.Fixtures;

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
		private delegate void ExternalMethodDelegate (StringBuilder dest, string src, uint n);

		private static SimpleMethods instance = new SimpleMethods ();

		private SimpleMethods ()
		{
		}

		/// <summary>
		/// Just a simple instance method with empty body.
		/// </summary>
		private void DoNothing ()
		{
		}

		/// <value>
		/// Gets a delegate to private and static external method.
		/// </value>
		public static Delegate ExternalMethod {
			get {
				return new ExternalMethodDelegate (strncpy);
			}
		}

		/// <value>
		/// Gets a delegate to private instance empty method. 
		/// </value>
		public static Delegate EmptyMethod {
			get {
				return new NoReturnMethod (instance.DoNothing);
			}
		}
	}
}