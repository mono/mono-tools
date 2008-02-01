// 
// Unit tests for EqualShouldHandleNullArgRule
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

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class EqualShouldHandleNullArgTest {

		public class EqualsChecksForNullArg {
			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				else
					return this == obj;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}
		
		public class EqualsDoesNotReturnFalseForNullArg {
			public override bool Equals (object obj)
			{
				if (obj == null)
					return true;

				return this == obj;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}
		
		public class EqualsNotOverriddenNotCheckingNull {
			public bool Equals (object obj)
			{
				return this == obj;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}
		
		public class EqualsNotOverriddenNotReturningFalseForNull {
			public new bool Equals (object obj)
			{
				if (obj != null)
					return this == obj;

				return true;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public class EqualsReturnsFalse {
			public override bool Equals (object obj)
			{
				return false;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public class EqualsReturnsTrue {
			public override bool Equals (object obj)
			{
				return true;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public struct EqualsUsingIsReturnFalse {
			public override bool Equals (object obj)
			{
				if (obj is EqualsUsingIsReturnFalse)
					return Object.ReferenceEquals (this, obj);
				return false;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public struct EqualsUsingIsReturnTrue {
			public override bool Equals (object obj)
			{
				if (obj is EqualsUsingIsReturnTrue)
					return Object.ReferenceEquals (this, obj);
				return true;
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public class EqualsCallBase : EqualsReturnsTrue {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
			public override int GetHashCode ()
			{
				return 1;
			}
		}

		public class EqualsCheckThis {
			// System.Object does this
			public override bool Equals (object obj)
			{
				return (this == obj);
			}
			public override int GetHashCode ()
			{
				return 1;
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
			public override int GetHashCode ()
			{
				return 1;
			}
		}
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new EqualShouldHandleNullArgRule ();
			runner = new MinimalRunner ();
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.BadPractice.EqualShouldHandleNullArgTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void equalsChecksForNullArgTest ()
		{
			type = GetTest ("EqualsChecksForNullArg");
			Assert.IsNull (rule.CheckType (type, runner));
		}
		
		[Test]
		public void equalsDoesNotReturnFalseForNullArgTest ()
		{
			type = GetTest ("EqualsDoesNotReturnFalseForNullArg");
			Assert.IsNotNull (rule.CheckType (type, runner));
		}
		
		[Test]
		public void equalsNotOverriddenNotCheckingNullTest ()
		{
			type = GetTest ("EqualsNotOverriddenNotCheckingNull");
			Assert.IsNull (rule.CheckType (type, runner));
		}
		
		[Test]
		public void equalsNotOverriddenNotReturningFalseForNullTest ()
		{
			type = GetTest ("EqualsNotOverriddenNotReturningFalseForNull");
			Assert.IsNotNull (rule.CheckType (type, runner));
		}

		[Test]
		public void EqualsReturnConstant ()
		{
			type = GetTest ("EqualsReturnsFalse");
			Assert.IsNull (rule.CheckType (type, runner), "EqualsReturnsFalse");

			type = GetTest ("EqualsReturnsTrue");
			Assert.IsNotNull (rule.CheckType (type, runner), "EqualsReturnsTrue");
		}

		[Test]
		public void EqualsUsingIs ()
		{
			type = GetTest ("EqualsUsingIsReturnFalse");
			Assert.IsNull (rule.CheckType (type, runner), "EqualsUsingIsReturnFalse");

			type = GetTest ("EqualsUsingIsReturnTrue");
			Assert.IsNotNull (rule.CheckType (type, runner), "EqualsUsingIsReturnTrue");
		}

		[Test]
		public void EqualsCallBaseClass ()
		{
			type = GetTest ("EqualsCallBase");
			// we can't be sure so we shut up (else false positives gets really bad)
			Assert.IsNull (rule.CheckType (type, runner), "EqualsCallBase");
		}

		[Test]
		public void EqualsCheckThisTest ()
		{
			type = GetTest ("EqualsCheckThis");
			Assert.IsNull (rule.CheckType (type, runner), "EqualsCheckThis");
		}

		[Test]
		public void EqualsCheckTypeTest ()
		{
			type = GetTest ("EqualsCheckType");
			Assert.IsNull (rule.CheckType (type, runner), "EqualsCheckType");
		}
	}
}
