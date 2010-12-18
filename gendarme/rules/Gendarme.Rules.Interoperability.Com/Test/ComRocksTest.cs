// 
// Unit tests for CustomAttributeRocks
//
// Authors:
//	N Lum <nol888@gmail.com>
//
// Copyright (C) 2010 N Lum
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


using Gendarme.Rules.Interoperability.Com;

using Mono.Cecil;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Helpers;

namespace Test.Rules.Interoperability.Com {

	[System.Runtime.InteropServices.ComVisible (true)]
	public class ExplicitComVisibleClass {
	}
	[System.Runtime.InteropServices.ComVisible (false)]
	public class ExplicitComInvisibleClass {
	}

	[TestFixture]
	public class ComRocksTest {

		[Test]
		public void IsComVisible()
		{
			TypeDefinition type = DefinitionLoader.GetTypeDefinition<ExplicitComVisibleClass> ();
			Assert.IsTrue (type.IsComVisible () ?? false, "true");
			type = SimpleTypes.Class;
			Assert.IsNull (type.IsComVisible (), "null");
			type = DefinitionLoader.GetTypeDefinition<ExplicitComInvisibleClass> ();
			Assert.IsFalse (type.IsComVisible () ?? true, "false");
		}

	}
}
