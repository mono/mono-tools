// 
// Unit tests for TypesShouldBeInsideNamespacesRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

using System.Reflection;

using Mono.Cecil;

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

public class PublicTypeOutsideNamescape {
	public class NestedPublicTypeOutsideNamescape {
	}

	protected class NestedProtectedTypeOutsideNamespace {
	}

	internal class NestedInternalTypeOutsideNamespace {
	}

	private class NestedPrivateClassOutsideNamespace {
	}
}

internal class InternalTypeOutsideNamespace {
}

class PrivateClassOutsideNamespace {
}

namespace Test.Rules.Design {

	public class PublicTypeInsideNamescape {
		public class NestedPublicTypeInsideNamescape {
		}

		protected class NestedProtectedTypeInsideNamespace {
		}

		internal class NestedInternalTypeInsideNamespace {
		}

		private class NestedPrivateTypeInsideNamespace {
		}
	}

	internal class InternalTypeInsideNamespace {
	}

	class PrivateClassInsideNamespace {
	}

	[TestFixture]
	public class TypesShouldBeInsideNamespacesTest : TypeRuleTestFixture<TypesShouldBeInsideNamespacesRule> {

		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		private TypeDefinition GetType (string name)
		{
			return assembly.MainModule.GetType (name);
		}

		[Test]
		public void OutsideNamespace ()
		{
			AssertRuleFailure<PublicTypeOutsideNamescape> (1);
			AssertRuleDoesNotApply<InternalTypeOutsideNamespace> ();
			AssertRuleDoesNotApply<PrivateClassOutsideNamespace> ();
		}

		[Test]
		public void NestedOutsideNamespace ()
		{
			AssertRuleDoesNotApply<PublicTypeOutsideNamescape.NestedPublicTypeOutsideNamescape> ();

			AssertRuleDoesNotApply<PublicTypeOutsideNamescape.NestedInternalTypeOutsideNamespace> ();

			AssertRuleDoesNotApply (GetType ("PublicTypeOutsideNamescape/NestedProtectedTypeOutsideNamespace"));

			AssertRuleDoesNotApply (GetType ("PublicTypeOutsideNamescape/NestedPrivateClassOutsideNamespace"));
		}

		[Test]
		public void InsideNamespace ()
		{
			AssertRuleSuccess<PublicTypeInsideNamescape> ();
			AssertRuleDoesNotApply<InternalTypeInsideNamespace> ();
			AssertRuleDoesNotApply<PrivateClassInsideNamespace> ();
		}

		[Test]
		public void NestedInsideNamespace ()
		{
			AssertRuleDoesNotApply<PublicTypeInsideNamescape.NestedPublicTypeInsideNamescape> ();

			AssertRuleDoesNotApply<PublicTypeInsideNamescape.NestedInternalTypeInsideNamespace> ();

			AssertRuleDoesNotApply (GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedProtectedTypeInsideNamespace"));

			AssertRuleDoesNotApply (GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedPrivateTypeInsideNamespace"));
		}
	}
}
