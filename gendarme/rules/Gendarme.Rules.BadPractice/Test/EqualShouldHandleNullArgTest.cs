// 
// Unit tests for EqualsShouldHandleNullArgRule
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

#pragma warning disable 114, 649, 659

	[TestFixture]
	public class EqualsShouldHandleNullArgTest : TypeRuleTestFixture<EqualsShouldHandleNullArgRule> {

		public class EqualsChecksForNullArg {
			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				else
					return this == obj;
			}
		}
		
		public class EqualsDoesNotReturnFalseForNullArg {
			public override bool Equals (object obj)
			{
				if (obj == null)
					return true;

				return this == obj;
			}
		}
		
		public class EqualsNotOverriddenNotCheckingNull {
			public bool Equals (object obj)
			{
				return this == obj;
			}
		}
		
		public class EqualsNotOverriddenNotReturningFalseForNull {
			public new bool Equals (object obj)
			{
				if (obj != null)
					return this == obj;

				return true;
			}
		}

		[Test]
		public void Basic ()
		{
			AssertRuleSuccess<EqualsChecksForNullArg> ();
			AssertRuleFailure<EqualsDoesNotReturnFalseForNullArg> (1);
			AssertRuleSuccess<EqualsNotOverriddenNotCheckingNull> ();
			AssertRuleFailure<EqualsNotOverriddenNotReturningFalseForNull> ();
		}

		public class EqualsReturnsFalse {
			public override bool Equals (object obj)
			{
				return false;
			}
		}

		public class EqualsReturnsTrue {
			public override bool Equals (object obj)
			{
				return true;
			}
		}

		[Test]
		public void Constants ()
		{
			AssertRuleSuccess<EqualsReturnsFalse> ();
			AssertRuleFailure<EqualsReturnsTrue> (1);
		}

		public struct EqualsUsingIsReturnFalse {
			public override bool Equals (object obj)
			{
				if (obj is EqualsUsingIsReturnFalse)
					return Object.ReferenceEquals (this, obj);
				return false;
			}
		}

		public struct EqualsUsingIsReturnTrue {
			public override bool Equals (object obj)
			{
				if (obj is EqualsUsingIsReturnTrue)
					return Object.ReferenceEquals (this, obj);
				return true;
			}
		}

		// from /mcs/class/corlib/System.Reflection.Emit/SignatureToken.cs
		public struct EqualsUsingIsReturnVariable {
			internal int tokValue;
			public override bool Equals (object obj)
			{
				bool res = obj is EqualsUsingIsReturnVariable;
				if (res) {
					EqualsUsingIsReturnVariable that = (EqualsUsingIsReturnVariable) obj;
					res = (this.tokValue == that.tokValue);
				}
				return res;
			}
		}

		[Test]
		public void EqualsUsingIs ()
		{
			AssertRuleSuccess<EqualsUsingIsReturnFalse> ();
			AssertRuleFailure<EqualsUsingIsReturnTrue> (1);
			AssertRuleSuccess<EqualsUsingIsReturnVariable> ();
		}

		public class EqualsCallBase : EqualsReturnsTrue {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
		}

		public class EqualsCheckThis {
			// System.Object does this
			public override bool Equals (object obj)
			{
				return (this == obj);
			}
		}

		public class EqualsCheckType {
			// common pattern in corlib
			public override bool Equals (object obj)
			{
				if (obj == null || GetType () != obj.GetType ())
					return false;
				return true;
			}
		}

		// from /mcs/class/System/System.ComponentModel/DisplayNameAttribute.cs
		public class CheckThisFirst {
			string DisplayName;
			public override bool Equals (object obj)
			{
				if (obj == this)
					return true;

				CheckThisFirst dna = obj as CheckThisFirst;

				if (dna == null)
					return false;
				return dna.DisplayName == DisplayName;
			}
		}

		[Test]
		public void CommonPatterns ()
		{
			AssertRuleSuccess<EqualsCheckThis> ();
			AssertRuleSuccess<EqualsCheckType> ();
			AssertRuleSuccess<CheckThisFirst> ();
		}

		public class StaticEquals {

			static public bool Equals (object obj)
			{
				return false;
			}
		}

		public class EqualsTwoParameters {

			public new bool Equals (object left, object right)
			{
				return (left == right);
			}
		}

		public class EqualsReference {

			public bool Equals (EqualsReference obj)
			{
				return this == obj;
			}
		}

		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply<StaticEquals> ();
			AssertRuleDoesNotApply<EqualsTwoParameters> ();
			AssertRuleDoesNotApply<EqualsReference> ();
		}

		public class Throw {

			public bool Equals (object obj)
			{
				throw new NotSupportedException ();
			}
		}

		[Test]
		public void Special ()
		{
			AssertRuleSuccess<Throw> ();
		}
	}
}
