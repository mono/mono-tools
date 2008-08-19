//
// Unit test for UseValueInPropertySetterRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class UseValueInPropertySetterTest : MethodRuleTestFixture<UseValueInPropertySetterRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public class Item {

			// use a filed to avoid potential issue with optimization (resulting in empty setters)
			bool val;

			public bool UsesValue {
				set { val = value; }
			}

			public bool DoesNotUseValue {
				set { val = true; }
			}

			public void set_NotAProperty(bool value)
			{
				val = true;
			}

			public int Throw1 {
				set { throw new NotSupportedException (); }
			}

			public int Throw2 {
				set { throw new NotSupportedException ("value isn't used here"); }
			}

			private string Translate (string s)
			{
				return "value n'est pas utilise ici";
			}

			public int Throw3 {
				set { throw new NotSupportedException (Translate ("value isn't used here")); }
			}

			public int CouldThrow {
				set {
					if (val)
						throw new NotSupportedException ();
				}
			}

			private static int maxFields = 25;
			public static int MaxFields {
				get { return maxFields; }
				set { maxFields = value; }
			}
			public static int MinFields {
				get { return 0; }
				set { maxFields = 0; }
			}

			public long time;
			public DateTime Time {
				set { time = value.Ticks; }
			}

			public string Empty {
				set { }
			}

			public object Marshalled {
				[param:MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(object))]
				set {
					val = (bool) value;
				}
			}
		}

		public class BitVector32 {
			int bits;

			public struct Section {
				private short mask;
				private short offset;
				
				internal Section (short mask, short offset)
				{
					this.mask = mask;
					this.offset = offset;
				}
				
				public short Mask {
					get { return mask; }
				}
				
				public short Offset {
					get { return offset; }
				}
			}

			public int this [BitVector32.Section section] {
				get {
					return ((bits >> section.Offset) & section.Mask);
				}
				set {
					if (value < 0)
						throw new ArgumentException ();
					if (value > section.Mask)
						throw new ArgumentException ();
					bits &= ~(section.Mask << section.Offset);
					bits |= (value << section.Offset);
				}
			}
		}


		[Test]
		public void TestUsesValue()
		{
			AssertRuleSuccess<Item> ("set_UsesValue");
		}
		
		[Test]
		public void TestDoesNotUseValue()
		{
			AssertRuleFailure<Item> ("set_DoesNotUseValue", 1);
		}

		[Test]
		public void TestNotAProperty()
		{
			AssertRuleDoesNotApply<Item> ("set_NotAProperty");
		}

		[Test]
		public void ThrowException ()
		{
			AssertRuleSuccess<Item> ("set_Throw1");
			AssertRuleSuccess<Item> ("set_Throw2");
			AssertRuleSuccess<Item> ("set_Throw3");
			AssertRuleFailure<Item> ("set_CouldThrow", 1);
		}

		[Test]
		public void TestStaticUsesValue ()
		{
			AssertRuleSuccess<Item> ("set_MaxFields");
		}

		[Test]
		public void TestStaticDoesNotUseValue ()
		{
			AssertRuleFailure<Item> ("set_MinFields", 1);
		}

		[Test]
		public void TestDateValue ()
		{
			AssertRuleSuccess<Item> ("set_Time");
		}

		[Test]
		public void TestEmpty ()
		{
			// too many false positive, it seems too common to have empty set to report them
			// at least for this specific rule
			AssertRuleSuccess<Item> ("set_Empty");
		}

		[Test]
		public void TestThisProperty ()
		{
			AssertRuleSuccess<BitVector32> ("set_Item");
		}

		[Test]
		public void TestMarshalled ()
		{
			// note: with [g]mcs the parameter is not named 'value'
			AssertRuleSuccess<Item> ("set_Marshalled");
		}
	}
}
