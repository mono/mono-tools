// 
// Unit tests for ReviewInconsistentIdentityRule
//
// Authors:
//	Jesse Jones  <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public sealed class ReviewInconsistentIdentityTest : TypeRuleTestFixture<ReviewInconsistentIdentityRule> {

		private sealed class NoFields {
		}

		private sealed class Good1 {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Good1 rhs = obj as Good1;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Good1 lhs, Good1 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Good1 lhs, Good1 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Good2 {
			private string name;
			private int weight;
			
			public override bool Equals (object obj)
			{
				Good2 rhs = obj as Good2;
				if ((object) rhs == null)
					return false;
					
				return name == rhs.name && weight == rhs.weight;
			}
			
			public static bool operator== (Good2 lhs, string name)	// rule skips == methods if an arg is not the declaring type
			{
				return lhs.name == name;
			}
			
			public static bool operator!= (Good2 lhs, string name)
			{
				return lhs.name != name;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + weight.GetHashCode ());
			}
			
			internal string IckyField = "icky";
		}

		private sealed class Good3 {
			private string name;
			private int weight;
			
			public override bool Equals (object obj)
			{
				Good3 rhs = obj as Good3;
				if ((object) rhs == null)
					return false;
					
				return name == rhs.name && weight == rhs.weight;
			}
			
			public static bool operator== (Good3 lhs, Good3 rhs)
			{
				Good2 ick = new Good2();
				
				if (lhs.name == ick.IckyField)	// ok to use fields from other types
					return rhs.name.Length == 0 && lhs.weight == rhs.weight;
				else
					return lhs.name == rhs.name && lhs.weight == rhs.weight;
			}
			
			public static bool operator!= (Good3 lhs, Good3 rhs)
			{
				return lhs.name != rhs.name || lhs.weight != rhs.weight;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + weight.GetHashCode ());
			}
		}

		private sealed class Good4 {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Good4 rhs = obj as Good4;
				if ((object) rhs == null)
					return false;
					
				return name == rhs.name && address == rhs.address;
			}
						
			public override int GetHashCode ()
			{
				return name.GetHashCode ();		// subset is ok here
			}
		}

		struct Good5 : IEquatable<Good5> {
			public readonly string Type;
			public readonly int Slot;
			
			public Good5 (string type, int slot)
			{
				this.Type = type;
				this.Slot = slot;
			}

			public static bool operator == (Good5 a, Good5 b)
			{
				return a.Slot == b.Slot && a.Type == b.Type;
			}

			public static bool operator != (Good5 a, Good5 b)
			{
				return a.Slot != b.Slot || a.Type != b.Type;
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				if (!(obj is Good5))
					return false;
				Good5 other = (Good5) obj;
				return this == other;
			}

			public bool Equals (Good5 rhs)
			{
				return this == rhs;
			}

			public override int GetHashCode ()
			{
				return Slot.GetHashCode () ^ Type.GetHashCode ();
			}
		}

		private struct Good6
		{
			private double m_value;
	
			public int CompareTo (object value)
			{
				if (value == null)
					return 1;
					
				double dv = (double)value;
	
				if (IsPositiveInfinity (m_value) && IsPositiveInfinity (dv))
					return 0;
	
				if (IsNegativeInfinity (m_value) && IsNegativeInfinity (dv))
					return 0;
	
				if (IsNaN (dv))
					if (IsNaN (m_value))
						return 0;
					else
						return 1;
	
				if (IsNaN (m_value))
					if (IsNaN (dv))
						return 0;
					else
						return -1;
	
				if (m_value > dv) 
					return 1;
				else if (m_value < dv) 
					return -1;
				else 
					return 0;
			}
	
			public override bool Equals (object obj)
			{
				if (!(obj is Good6))
					return false;
	
				double value = (double) obj;
	
				if (IsNaN (value))
					return IsNaN (m_value);
	
				return (value == m_value);
			}
		
			public override int GetHashCode ()
			{
				return (int) m_value;
			}
	
			public static bool IsInfinity (double d)
			{
				return double.IsInfinity (d);
			}
	
			public static bool IsNaN (double d)
			{
				return double.IsNaN (d);
			}
	
			public static bool IsNegativeInfinity (double d)
			{
				return double.IsNegativeInfinity (d);
			}
	
			public static bool IsPositiveInfinity (double d)
			{
				return double.IsPositiveInfinity (d);
			}
		}
	
		private sealed class Good7 {
			private string name;
			private string address;
			private int hash;
			
			public override bool Equals (object obj)
			{
				Good7 rhs = obj as Good7;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Good7 lhs, Good7 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Good7 lhs, Good7 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				if (hash == 0)
					hash = unchecked (name.GetHashCode () + address.GetHashCode ());
					
				return hash;
			}
		}

		private sealed class Good8 {
			private string name;
			private string address;
			private int Hash { get; set; }
			
			public override bool Equals (object obj)
			{
				Good8 rhs = obj as Good8;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Good8 lhs, Good8 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Good8 lhs, Good8 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				if (Hash == 0)
					Hash = unchecked (name.GetHashCode () + address.GetHashCode ());
					
				return Hash;
			}
		}

		private sealed class Good9 {
			private string name;
			private string address;
			
			private int Compare (Good9 rhs)
			{
				int result = name.CompareTo (rhs.name);
				
				if (result == 0)
					result = address.CompareTo (rhs.address);
										
				return result;
			}
			
			public override bool Equals (object obj)
			{
				Good9 rhs = obj as Good9;
				if ((object) rhs == null)
					return false;
					
				return Compare (rhs) == 0;
			}
			
			public static bool operator== (Good9 lhs, Good9 rhs)
			{
				return lhs.Compare (rhs) == 0;
			}
			
			public static bool operator!= (Good9 lhs, Good9 rhs)
			{
				return lhs.Compare (rhs) != 0;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Good10 : ICloneable {
			private string name;
			private string address;
			private int age;
			
			public object Clone ()
			{
				Good10 result = new Good10 ();
				result.name = name;
				result.address = address;
				result.age = age;			// using more state than equality is ok
				
				return result;
			}
			
			public override bool Equals (object obj)
			{
				Good10 rhs = obj as Good10;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Good10 lhs, Good10 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Good10 lhs, Good10 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Good11 {
			private int weight;
			
			public override bool Equals (object obj)
			{
				Good11 rhs = obj as Good11;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Good11 lhs, Good11 rhs)
			{
				return lhs.weight == rhs.weight;
			}
			
			public static bool operator!= (Good11 lhs, Good11 rhs)
			{
				return lhs.weight != rhs.weight;
			}
			
			public override int GetHashCode ()
			{
				return weight.GetHashCode ();
			}
		}

		private sealed class Bad1 {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Bad1 rhs = obj as Bad1;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Bad1 lhs, Bad1 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad1 lhs, Bad1 rhs)
			{
				return lhs.name != rhs.name;	// doesn't use address
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Bad2 : IEquatable<Bad2> {			
			public string Name { get; set; }
			public string Address { get; set; }
			
			public override bool Equals (object obj)
			{
				Bad2 rhs = obj as Bad2;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public bool Equals (Bad2 rhs)
			{
				return Name != rhs.Name;	// doesn't use Address
			}
			
			public static bool operator== (Bad2 lhs, Bad2 rhs)
			{
				return lhs.Name == rhs.Name && lhs.Address == rhs.Address;
			}
			
			public static bool operator!= (Bad2 lhs, Bad2 rhs)
			{
				return lhs.Name != rhs.Name || lhs.Address != rhs.Address;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (Name.GetHashCode () + Address.GetHashCode ());
			}
		}

		private sealed class Bad3 : IComparable<Bad3> {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Bad3 rhs = obj as Bad3;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public int CompareTo (Bad3 rhs)
			{
				return name.CompareTo (rhs.name);	// doesn't use address
			}
			
			public static bool operator== (Bad3 lhs, Bad3 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad3 lhs, Bad3 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;	
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Bad4 : IComparable<Bad4> {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Bad4 rhs = obj as Bad4;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public int CompareTo (Bad4 rhs)
			{
				int result = name.CompareTo (rhs.name);
				
				if (result == 0)
					result = address.CompareTo (rhs.address);
					
				return result;
			}
			
			public static bool operator== (Bad4 lhs, Bad4 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad4 lhs, Bad4 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;	
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
			
			public static bool operator< (Bad4 lhs, Bad4 rhs)
			{
				return lhs.name.CompareTo (rhs.name) < 0;	// no address
			}
			
			public static bool operator<= (Bad4 lhs, Bad4 rhs)
			{
				return lhs.name.CompareTo (rhs.name) <= 0;	// no address
			}
			
			public static bool operator> (Bad4 lhs, Bad4 rhs)
			{
				return lhs.CompareTo (rhs) > 0;
			}
			
			public static bool operator>= (Bad4 lhs, Bad4 rhs)
			{
				return lhs.CompareTo (rhs) >= 0;
			}
		}

		private sealed class Bad5 {
			private string name;
			private string address;
			private int age;
			
			public override bool Equals (object obj)
			{
				Bad5 rhs = obj as Bad5;
				if ((object) rhs == null)
					return false;
					
				return name == rhs.name && address == rhs.address;
			}
						
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode () + age);	// can't check more state than equality does
			}
		}

		private sealed class Bad6 {
			private string name;
			private string address;
			private int age;
			
			public override bool Equals (object obj)
			{
				Bad6 rhs = obj as Bad6;
				if ((object) rhs == null)
					return false;
					
				return name == rhs.name && address == rhs.address;
			}
						
			public override int GetHashCode ()
			{
				return 7*age;		// must check at least some state used by equality
			}
		}

		private sealed class Bad7 : ICloneable {
			private string name;
			private string address;
			
			public object Clone ()
			{
				Bad7 result = new Bad7 ();
				result.name = name;			// missing address
				
				return result;
			}
			
			public override bool Equals (object obj)
			{
				Bad7 rhs = obj as Bad7;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Bad7 lhs, Bad7 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad7 lhs, Bad7 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Bad8 {
			private string name;
			private string address;
			
			public Bad8 Clone ()
			{
				Bad8 result = new Bad8 ();
				result.address = address;	// missing name
				
				return result;
			}
			
			public override bool Equals (object obj)
			{
				Bad8 rhs = obj as Bad8;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Bad8 lhs, Bad8 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad8 lhs, Bad8 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Bad9 : IComparable {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Bad9 rhs = obj as Bad9;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public int CompareTo (object value)
			{
				Bad9 rhs = (Bad9) value;
				
				int result = name.CompareTo (rhs.name);	// no address
									
				return result;
			}
			
			public static bool operator== (Bad9 lhs, Bad9 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad9 lhs, Bad9 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;	
			}
			
			public override int GetHashCode ()
			{
				return unchecked (name.GetHashCode () + address.GetHashCode ());
			}
		}

		private sealed class Bad10 {
			private string name;
			private string address;
			
			public override bool Equals (object obj)
			{
				Bad10 rhs = obj as Bad10;
				if ((object) rhs == null)
					return false;
					
				return this == rhs;
			}
			
			public static bool operator== (Bad10 lhs, Bad10 rhs)
			{
				return lhs.name == rhs.name && lhs.address == rhs.address;
			}
			
			public static bool operator!= (Bad10 lhs, Bad10 rhs)
			{
				return lhs.name != rhs.name || lhs.address != rhs.address;
			}
			
			public override int GetHashCode ()
			{
				return base.GetHashCode ();	// very bad to do this for classes, also bad for structs if all fields don't participate in equality
			}
		}

		[Test]
		public void Check ()
		{
			AssertRuleDoesNotApply<NoFields> ();
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);

			AssertRuleSuccess<Good1> ();
			AssertRuleSuccess<Good2> ();
			AssertRuleSuccess<Good3> ();
			AssertRuleSuccess<Good4> ();
			AssertRuleSuccess<Good5> ();
			AssertRuleSuccess<Good6> ();
			AssertRuleSuccess<Good7> ();
			AssertRuleSuccess<Good8> ();
			AssertRuleSuccess<Good9> ();
			AssertRuleSuccess<Good10> ();
			AssertRuleSuccess<Good11> ();

			AssertRuleFailure<Bad1> (1);
			AssertRuleFailure<Bad2> (1);
			AssertRuleFailure<Bad3> (1);
			AssertRuleFailure<Bad4> (1);
			AssertRuleFailure<Bad5> (1);
			AssertRuleFailure<Bad6> (1);
			AssertRuleFailure<Bad7> (1);
			AssertRuleFailure<Bad8> (1);
			AssertRuleFailure<Bad9> (1);
			AssertRuleFailure<Bad10> (1);
		}

		public abstract class AbstractEqualsTest : IEquatable<AbstractEqualsTest> {

			private int field;

			public abstract bool Equals (AbstractEqualsTest other);

			public override bool Equals (object obj)
			{
				return Equals (obj as AbstractEqualsTest);
			}
		}

		[Test]
		public void Abstract ()
		{
			AssertRuleSuccess<AbstractEqualsTest> ();
		}
	}
}

