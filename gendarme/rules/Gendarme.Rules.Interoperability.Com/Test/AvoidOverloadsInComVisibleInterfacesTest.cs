//
// Unit Tests for AvoidOverloadsInComVisibleInterfacesRule
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

using System.Runtime.InteropServices;

using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Interoperability.Com {

	/** pass */
	[ComVisible (true)]
	public interface ComVisibleInterfaceGood {
		void SomeMethod(int Value);
		void SomeMethodWithValues(int ValueOne, int ValueTwo);
	}
	[ComVisible (true)]
	public interface ComVisibleInterfaceGood2 {
		void SomeMethod(int Value);
		[ComVisible (false)]
		void SomeMethod(int ValueOne, int ValueTwo);
	}

	/** does not apply */
	[ComVisible (false)]
	public interface NonComVisibleInterface {
		void SomeMethod();
		void SomeMethod(int IAmOverloaded);
	}

	[ComVisible (true)]
	public interface ComVisibleInterfaceNoMethods {
	}

	[ComVisible (true)]
	internal interface ComVisibleInterfaceInternal {
		void SomeMethod(int Value);
		void SomeMethod(int ValueOne, int ValueTwo);
	}

	/** fail */
	[ComVisible (true)]
	public interface ComVisibleInterfaceBad {
		void SomeMethod(int Value);
		void SomeMethod(int ValueOne, int ValueTwo);
	}
	[ComVisible (true)]
	public interface ComVisibleInterfaceBad2 {
		void SomeMethod(int Value);
		void SomeMethod(int ValueOne, int ValueTwo);
		void SomeMethod(int ValueOne, int ValueTwo, int ValueThree);
		void SomeMethodOther(int Value);
		void SomeMethodOther(int ValueOne, int ValueTwo);
	}

	[TestFixture]
	public class AvoidOverloadsInComVisibleInterfacesTest : TypeRuleTestFixture<AvoidOverloadsInComVisibleInterfacesRule> {

		[Test]
		public void DoesNotApply()
		{
			// Interface with no methods.
			AssertRuleDoesNotApply<ComVisibleInterfaceNoMethods> ();

			// Internal interface.
			AssertRuleDoesNotApply<ComVisibleInterfaceInternal> ();

			// Non-interface.
			AssertRuleDoesNotApply<ComVisibleGood> ();

			// Non ComVisible interface.
			AssertRuleDoesNotApply<NonComVisibleInterface> ();
		}

		[Test]
		public void Good()
		{
			// ComVisible interface with no method overloading.
			AssertRuleSuccess<ComVisibleInterfaceGood> ();

			// ComVisible interface with non-ComVisible overload
			AssertRuleSuccess<ComVisibleInterfaceGood2> ();
		}

		[Test]
		public void Bad()
		{
			// ComVisible interface with overloaded methods.
			AssertRuleFailure<ComVisibleInterfaceBad> (1);

			// ComVisible interface with many overloaded methods.
			AssertRuleFailure<ComVisibleInterfaceBad2> (3);
		}
	}
}
