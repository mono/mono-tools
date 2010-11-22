//
// Unit tests for AvoidDeepNamespaceHierarchyRule
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
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace One {
	public interface I1 { }
}

namespace One.Two {
	public interface I2 { }
}

namespace One.Two.Three {
	public interface I3 { }
}

namespace One.Two.Three.Four {
	public interface I4 { }
}

// bad
namespace One.Two.Three.Four.Five {
	public interface I5 { }
}

// exceptions
namespace One.Two.Three.Four.Design {
	public interface IDesign { }
}

namespace One.Two.Three.Four.Interop {
	public interface IInterop { }
}

namespace One.Two.Three.Four.Permissions {
	public interface IPermissions { }
}

namespace One.Two.Three.Four.Impl {
	internal interface IImpl { }
}

namespace One.Two.Three.Four.Internal {
	internal interface Internal { }
}

namespace Test.Rules.Naming {

	[TestFixture]
	public class AvoidDeepNamespaceHierarchyTest : AssemblyRuleTestFixture<AvoidDeepNamespaceHierarchyRule> {

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
			// 1. Cancelled.ComPlus.Indices.ShouldntBe.Writeable (from UsePreferredTermsTest)
			// 2. One.Two.Three.Four.Five (from here)
			AssertRuleFailure (assembly, 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Min ()
		{
			Rule.MaxDepth = Int32.MinValue;
		}

		[Test]
		public void Max ()
		{
			int depth = Rule.MaxDepth;
			try {
				Rule.MaxDepth = Int32.MaxValue;
				AssertRuleSuccess (assembly);
			}
			finally {
				Rule.MaxDepth = depth;
			}
		}
	}
}
