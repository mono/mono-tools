//
// Unit tests for InternalNamespacesShouldNotExposeTypesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Reflection;

using Mono.Cecil;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Ok.Internal {
	internal interface InternalInterface {
	}

	class PrivateType {
		public class WithNestPublicType {
		}
	}
}

namespace Bad.Enum.Internal {

	public enum Internal {
		Private,
		Internal
	}
}

namespace Bad.Delegate.Impl {

	public delegate void Internal (object sender, EventArgs e);
}

namespace Test.Rules.Design {

	[TestFixture]
	public class InternalNamespacesShouldNotExposeTypesTest : AssemblyRuleTestFixture<InternalNamespacesShouldNotExposeTypesRule> {

		AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		[Test]
		public void Namespaces ()
		{
			AssertRuleFailure (assembly, 2);

			string e1 = "Bad.Enum.Internal.Internal";
			string e2 = "Bad.Delegate.Impl.Internal";
			string a1 = (Runner.Defects [0].Location as TypeDefinition).FullName;
			string a2 = (Runner.Defects [1].Location as TypeDefinition).FullName;
			Assert.IsTrue (a1 == e1 || a2 == e1, e1);
			Assert.IsTrue (a1 == e2 || a2 == e2, e2);
		}
	}
}
