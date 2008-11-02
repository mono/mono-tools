//
// Unit tests for CheckParametersNullityInVisibleMethodsRule.
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
using System.Drawing;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Tests.Rules.Correctness {

	[TestFixture]
	public class CheckParametersNullityInVisibleMethodsTest : MethodRuleTestFixture<CheckParametersNullityInVisibleMethodsRule> {

		public void UnusedInstance (object o)
		{
		}

		static public void UnusedStatic (object o)
		{
		}

		public int ValueTypeInstance (int i)
		{
			return i.GetHashCode ();
		}

		static public int ValueTypeStatic (int i)
		{
			return i.GetHashCode ();
		}

		public int BadInstance (object o)
		{
			return o.GetHashCode ();
		}

		static public int BadStatic (object o)
		{
			return o.GetHashCode ();
		}

		[Test]
		public void NoCheck ()
		{
			// parameter is unused so no need to check null
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("UnusedInstance");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("UnusedStatic");
			// parameter is used but is a value type, so no need to check null
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("ValueTypeInstance");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("ValueTypeStatic");
			// parameter is used, not a value type, but has no null check
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("BadInstance", 1);
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("BadStatic", 1);
		}

		// only check nullity, nothing else (no deref)
		public void NotNullInstance (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
		}

		static public void NotNullStatic (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
		}

		[Test]
		public void OnlyCheck ()
		{
			// parameter is only checked, never dereferenced
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("NotNullInstance");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("NotNullStatic");
		}

		public int GoodThrowInstance (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			return o.GetHashCode ();
		}

		static public int GoodThrowStatic (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			return o.GetHashCode ();
		}

		public int GoodAvoidInstance (object o)
		{
			return (o == null) ? 0 : o.GetHashCode ();
		}

		static public int GoodAvoidStatic (object o)
		{
			return (o == null) ? 0 : o.GetHashCode ();
		}

		[Test]
		public void AllCheck ()
		{
			// a null check is done and throws
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("GoodThrowInstance");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("GoodThrowStatic");

			// a null check is done and avoid the dereference
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("GoodAvoidInstance");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("GoodAvoidStatic");
		}

		public int BadAvoidInstance (object o)
		{
			return (o != null) ? 0 : o.GetHashCode ();
		}

		[Test]
		[Ignore ("Remainder: By design this rule reports *lack* if null checks not [in]valid null checks")]
		public void BadCheck ()
		{
			// a null check is done but does not prevent the dereference
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("BadAvoidInstance");
		}

		// ignore ? always or only if NotNull is public too ?
		public int IgnoreInstanceCheck (object o)
		{
			NotNullInstance (o);
			return o.GetHashCode ();
		}

		static public int IgnoreStaticCheck (object o)
		{
			NotNullStatic (o);
			return o.GetHashCode ();
		}

		[Test]
		public void PotentiallyDelegatedNullCheck ()
		{
			// since parameters is used in a method call before being dereferenced
			// we assume that:
			// (a) the method called does the null check - and we'll even check this if the method is visible
			// (b) it's a method specific for doing the null check test
			// and we wont report this as a defect (too many false positives)
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("IgnoreInstanceCheck");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("IgnoreStaticCheck");
		}

		public int CheckThruLocal (object o)
		{
			// null check using local
			object local = o;
			if (local == null)
				throw new ArgumentNullException ("o");
			// dereference argument
			return o.GetHashCode ();
		}

		// 'dtk' as an enum and i as a valuetype don't need to be checked
		public int CheckMixedParameters (DateTimeKind dtk, int i, string s, object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");

			// 's' is used without a null check
			if (i < s.Length)
				return dtk.GetHashCode ();
			else
				return o.GetHashCode ();
		}

		[Test]
		public void MissingCheck ()
		{
			// parameter is checked indirectly but dereferenced directly
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("CheckThruLocal", 1);
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("CheckMixedParameters", 1);
		}

		private object obj;
		private int code;

		public object ObjectGood {
			get { return obj; }
			set { 
				obj = value;
				// value is checked against null before being used
				code = (value == null) ? 0 : value.GetHashCode ();
			}
		}

		public object ObjectBad {
			get { return obj; }
			set {
				obj = value;
				// no null check *and* value is dereferenced inside the method
				code = value.GetHashCode ();
			}
		}

		public int HashCode {
			get { return obj == null ? code : obj.GetHashCode (); }
			set {
				// value type
				code = value.GetHashCode ();
				obj = null;
			}
		}

		[Test]
		public void InstanceProperties ()
		{
			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_ObjectGood");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("set_ObjectGood");

			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_ObjectBad");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("set_ObjectBad", 1);

			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_HashCode");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("set_HashCode");
		}

		static object obj_static;
		static int code_static;

		static public object StaticObjectGood {
			get { return obj_static; }
			set {
				obj_static = value;
				// value is checked against null before being used
				code_static = (value == null) ? 0 : value.GetHashCode ();
			}
		}

		static public object StaticObjectBad {
			get { return obj_static; }
			set {
				obj_static = value;
				// no null check *and* value is dereferenced inside the method
				code_static = value.GetHashCode ();
			}
		}

		static public int StaticHashCode {
			get { return obj_static == null ? code_static : obj_static.GetHashCode (); }
			set {
				// value type
				code_static = value.GetHashCode ();
				obj_static = null;
			}
		}

		[Test]
		public void StaticProperties ()
		{
			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_StaticObjectGood");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("set_StaticObjectGood");

			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_StaticObjectBad");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("set_StaticObjectBad", 1);

			// getter does not apply because it has no parameter
			AssertRuleDoesNotApply<CheckParametersNullityInVisibleMethodsTest> ("get_StaticHashCode");
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("set_StaticHashCode");
		}

		public class ClassContainer : IEquatable<ClassContainer> {
			public ClassContainer Container;
			public int ValueType;

			public ClassContainer (ClassContainer c)
			{
				// no check if 'c' is null
				Container = c.Container;
				ValueType = c.ValueType;
			}

			public bool Equals (ClassContainer other)
			{
				if (other == null)
					return false;
				return (Container.Equals (other.Container) && (ValueType == other.ValueType));
			}
		}

		public struct StructContainer {
			// a struct cannot contain itself
			public object Container;
			public int ValueType;

			public StructContainer (StructContainer c)
			{
				// no need to check for 'c' since it can't be null
				Container = c.Container;
				ValueType = c.ValueType;
			}

			public override bool Equals (object obj)
			{
				// problem if cast is invalid
				StructContainer other = (StructContainer) obj;
				return (Container.Equals (other.Container) && (ValueType == other.ValueType));
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		[Test]
		public void Fields ()
		{
			AssertRuleFailure<ClassContainer> (".ctor", 1);
			AssertRuleSuccess<ClassContainer> ("Equals");

			AssertRuleSuccess<StructContainer> (".ctor");
			AssertRuleSuccess<StructContainer> ("Equals");
		}

		// constrained to struct == valuetype
		public class Bitmask<T> where T : struct, IConvertible {

			ulong mask;

			// missing null check, since it's a Bitmap<T>, not T
			public bool IsSubsetOf (Bitmask<T> bitmask)
			{
				return ((mask & bitmask.mask) == mask);
			}

			// no need for null check (T is constrained to ValueType)
			public void SetUp (T bit)
			{
				Console.WriteLine (bit.ToInt32 (null));
			}
		}

		[Test]
		public void Generics ()
		{
			AssertRuleFailure<Bitmask<int>> ("IsSubsetOf", 1);
			AssertRuleSuccess<Bitmask<int>> ("SetUp");
		}

		public int AccessInstanceArrayBad (object[] array)
		{
			return array [0].GetHashCode ();
		}

		public int AccessInstanceArrayGood (object [] array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			return array [0].GetHashCode ();
		}

		static public int AccessStaticArrayBad (int [] array)
		{
			return array [0] + array [1];
		}

		static public int AccessStaticArrayGood (int [] array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			return array [0] + array [1];
		}

		[Test]
		public void Array ()
		{
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("AccessInstanceArrayGood");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("AccessInstanceArrayBad", 1);

			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("AccessStaticArrayGood");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("AccessStaticArrayBad", 1);
		}

		public int InstanceGoodRef (ref object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return obj.GetHashCode ();
		}

		public int InstanceBadRef (ref object obj)
		{
			return obj.GetHashCode ();
		}

		static public int StaticGoodRef (ref object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return obj.GetHashCode ();
		}

		static public int StaticBadRef (ref object obj)
		{
			return obj.GetHashCode ();
		}

		[Test]
		public void Ref ()
		{
			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("InstanceGoodRef");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("InstanceBadRef", 1);

			AssertRuleSuccess<CheckParametersNullityInVisibleMethodsTest> ("StaticGoodRef");
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("StaticBadRef", 1);
		}

		public void LateCheck (string s)
		{
			Console.WriteLine (s.Length);
			if (s != null)
				Console.WriteLine (s.GetHashCode ());	
		}

		[Test]
		public void Late ()
		{
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("LateCheck", 1);
		}

		// adapted from GraphicPathIterator
		public int Enumerate (ref double [] points, ref byte [] types)
		{
			int resultCount = 0;
			int count = points.Length;

			if (count != types.Length)
				throw new ArgumentException ("Invalid arguments passed. Both arrays should have the same length.");
			return resultCount;
		}

		[Test]
		public void Reference ()
		{
			AssertRuleFailure<CheckParametersNullityInVisibleMethodsTest> ("Enumerate", 2);
		}
	}
}
