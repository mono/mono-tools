// 
// Unit tests for DoNotDeclareSettersOnCollectionPropertiesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Security;

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Design {

	[TestFixture]
	public class DoNotDeclareSettersOnCollectionPropertiesTest : TypeRuleTestFixture<DoNotDeclareSettersOnCollectionPropertiesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		public interface IGoodInterface {
			ICollection Collection { get; }
			ICollection<string> GenericCollection { get; }
		}

		public class TypeImplementingGoodInterface : IGoodInterface {
			public ICollection Collection { get; private set; }
			public ICollection<string> GenericCollection { get; private set; }
		}

		public struct GoodStruct {
			private ArrayList list;

			public IDictionary Dictionary { get; private set; }
			public ArrayList List { 
				get { return list; }
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<IGoodInterface> ();
			AssertRuleSuccess<TypeImplementingGoodInterface> ();
			AssertRuleSuccess<GoodStruct> ();
		}

		// interface members are not "declared" as public - but they force a type to do so!
		public interface IBadInterface {
			ICollection Collection { get; set; }
			ICollection<string> GenericCollection { get; set; }
		}

		public class TypeImplementingBadInterface : IBadInterface {
			public ICollection Collection { get; set; }
			public ICollection<string> GenericCollection { get; set; }
		}

		public struct BadStruct {
			public IDictionary Dictionary { private get; set; }
			public ArrayList List { get; set; }
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<IBadInterface> (2);
			AssertRuleFailure<TypeImplementingBadInterface> (2);
			AssertRuleFailure<BadStruct> (2);
		}

		public class SecurityPermissions {
			public PermissionSet Permissions { get; set; }
			public NamedPermissionSet NamedPermissions { get; set; }
		}

		public class Indexers {
			int [] array;

			public int this [int index] {
				get { return array [index]; }
				set { array [index] = value; }
			}

			public int this [int x, int y] {
				get { return array [x]; }
				set { array [y] = value; }
			}
		}

		public class Arrays {

			public Array Array { get; set; }
		}

		[Test]
		public void SpecialCases ()
		{
			AssertRuleSuccess<SecurityPermissions> ();
			AssertRuleSuccess<Indexers> ();
			AssertRuleFailure<Arrays> (1);
		}
	}
}
