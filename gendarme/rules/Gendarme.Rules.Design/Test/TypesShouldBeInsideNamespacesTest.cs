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

using System;
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;

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

		private class NestedPrivateClassInsideNamespace {
		}
	}

	internal class InternalTypeInsideNamespace {
	}

	class PrivateClassInsideNamespace {
	}

	[TestFixture]
	public class TypesShouldBeInsideNamespacesTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new TypesShouldBeInsideNamespacesRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetType (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void OutsideNamespace ()
		{
			TypeDefinition type = GetType ("PublicTypeOutsideNamescape");
			Assert.IsNotNull (rule.CheckType (type, runner), "PublicTypeOutsideNamescape");

			type = GetType ("InternalTypeOutsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "InternalTypeOutsideNamespace");

			type = GetType ("PrivateClassOutsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "PrivateClassOutsideNamespace");
		}

		[Test]
		public void NestedOutsideNamespace ()
		{
			TypeDefinition type = GetType ("PublicTypeOutsideNamescape/NestedPublicTypeOutsideNamescape");
			Assert.IsNull (rule.CheckType (type, runner), "NestedPublicTypeOutsideNamescape");

			type = GetType ("PublicTypeOutsideNamescape/NestedProtectedTypeOutsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedProtectedTypeOutsideNamespace");

			type = GetType ("PublicTypeOutsideNamescape/NestedInternalTypeOutsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedInternalTypeInsideNamespace");

			type = GetType ("PublicTypeOutsideNamescape/NestedPrivateClassOutsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedPrivateClassInsideNamespace");
		}

		[Test]
		public void InsideNamespace ()
		{
			TypeDefinition type = GetType ("Test.Rules.Design.PublicTypeInsideNamescape");
			Assert.IsNull (rule.CheckType (type, runner), "PublicTypeInsideNamescape");

			type = GetType ("Test.Rules.Design.InternalTypeInsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "InternalTypeInsideNamespace");

			type = GetType ("Test.Rules.Design.PrivateClassInsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "PrivateClassInsideNamespace");
		}

		[Test]
		public void NestedInsideNamespace ()
		{
			TypeDefinition type = GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedPublicTypeInsideNamescape");
			Assert.IsNull (rule.CheckType (type, runner), "NestedPublicTypeInsideNamescape");

			type = GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedProtectedTypeInsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedProtectedTypeInsideNamespace");

			type = GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedInternalTypeInsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedInternalTypeInsideNamespace");

			type = GetType ("Test.Rules.Design.PublicTypeInsideNamescape/NestedPrivateClassInsideNamespace");
			Assert.IsNull (rule.CheckType (type, runner), "NestedPrivateClassInsideNamespace");
		}
	}
}
