// 
// Unit tests for AvoidPropertiesWithoutGetAccessorRule
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	public abstract class PublicAbstract {
		public abstract int Value { get; set; }
	}

	public abstract class PublicAbstractGetOnly {
		public abstract int Value { get; }
	}

	public abstract class PublicAbstractSetOnly {
		public abstract int Value { set; }
	}

	public interface IPublic {
		int Value { get; set; }
	}

	public interface IPublicGetOnly {
		int Value { get; }
	}

	public interface IPublicSetOnly {
		int Value { set; }
	}

	public class PublicClassInterface : IPublic {
		public int Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicClassExplicitInterface : IPublic {
		int IPublic.Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicClass {
		public int Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicSetOnlyInheritClass : PublicAbstractSetOnly {
		public override int Value {
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicSetOnlyImplementClass : IPublicSetOnly {
		public int Value {
			set { throw new NotImplementedException (); }
		}
	}

#if false
	// this cannot be compiled with CSC - error CS0082
	public class PublicGetIsNotAGetterClass : IPublicSetOnly {

		// try to confuse the rule
		public int get_Value ()
		{
			return 42;
		}

		public int Value {
			set { throw new NotImplementedException (); }
		}
	}
#endif
	public class PublicSetIsNotASetterClass {

		public void set_Value ()
		{
		}
	}

	[TestFixture]
	public class AvoidPropertiesWithoutGetAccessorTest : TypeRuleTestFixture<AvoidPropertiesWithoutGetAccessorRule> {

		[Test]
		public void WithNoProperties ()
		{
			AssertRuleDoesNotApply<AvoidPropertiesWithoutGetAccessorTest> ();
		}

		[Test]
		public void WithBothGetAndSet ()
		{
			AssertRuleSuccess<PublicAbstract> ();
			AssertRuleSuccess<IPublic> ();
			AssertRuleSuccess<PublicClass> ();
			AssertRuleSuccess<PublicClassExplicitInterface> ();
			AssertRuleSuccess<PublicClassInterface> ();
		}

		[Test]
		public void WithOnlyGet ()
		{
			AssertRuleSuccess<PublicAbstractGetOnly> ();
			AssertRuleSuccess<IPublicGetOnly> ();
		}

		[Test]
		public void WithOnlySet ()
		{
			AssertRuleFailure<PublicAbstractSetOnly> (1);
			AssertRuleFailure<IPublicSetOnly> (1);
			AssertRuleFailure<PublicSetOnlyInheritClass> (1);
			AssertRuleFailure<PublicSetOnlyImplementClass> (1);

			AssertRuleDoesNotApply<PublicSetIsNotASetterClass> ();
		}
	}
}
