// 
// Unit tests for CloneMethodShouldNotReturnNullRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.BadPractice;
using NUnit.Framework;

using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class CloneMethodShouldNotReturnNullTest : MethodRuleTestFixture<CloneMethodShouldNotReturnNullRule> {

		abstract class CloneAbstract : ICloneable {
			public abstract object Clone ();
		}

		[Test]
		public void NoIL ()
		{
			AssertRuleDoesNotApply<CloneAbstract> ();
		}

		public class CloneMethodReturningNull: ICloneable {
			public object Clone ()
			{
				return null;
			}
		}

		[Test]
		public void CloneMethodReturningNullTest ()
		{
			AssertRuleFailure<CloneMethodReturningNull> ("Clone", 1);
		}

		public class CloneMethodNotReturningNull: ICloneable
		{
			public object Clone ()
			{
				return this.MemberwiseClone ();
			}
		}

		[Test]
		public void CloneMethodNotReturningNullTest ()
		{
			// no LDNULL
			AssertRuleDoesNotApply<CloneMethodNotReturningNull> ();
		}

		public class NotUsingICloneableClone
		{
			public object Clone ()
			{
				return null;
			}
		}

		[Test]
		public void NotUsingICloneableCloneTest ()
		{
			AssertRuleDoesNotApply<NotUsingICloneableClone> ("Clone");
		}

		public class CloneWithDifferentArgsReturningNull: ICloneable
		{
			public virtual object Clone ()
			{
				return this.MemberwiseClone ();
			}
			
			public object Clone (int j)
			{
				return null;
			}
		}

		[Test]
		public void cloneWithDifferentArgsReturningNullTest ()
		{
			// no LDNULL in Clone() and wrong signature for Clone(int)
			AssertRuleDoesNotApply<CloneWithDifferentArgsReturningNull> ();
		}

		public class CloneReturningNullInSomeConditions: ICloneable
		{
			public bool test (int j)
			{
				if (j > 10)
					return true;
				else
					return false;
			}
			
			public object Clone()
			{
				if (test (11))
					return MemberwiseClone();
				else
					return null;
			}
		}
			
		[Test]
		public void CloneReturningNullInSomeConditionsTest ()
		{
			AssertRuleFailure<CloneReturningNullInSomeConditions> ("Clone", 1);
		}

		public class CloneWithInlineIf: ICloneable {
			public object Clone ()
			{
				return (this is ICloneable) ? null : MemberwiseClone ();
			}
		}

		[Test]
		public void InlineIf ()
		{
			AssertRuleFailure<CloneWithInlineIf> ("Clone", 1);
		}
	}
}

