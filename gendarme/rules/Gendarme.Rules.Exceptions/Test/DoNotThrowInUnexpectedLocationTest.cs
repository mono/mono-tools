//
// Unit test for DoNotThrowInUnexpectedLocationRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Jesse Jones
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public sealed class DoNotThrowInUnexpectedLocationTest : MethodRuleTestFixture<DoNotThrowInUnexpectedLocationRule> {

		public DoNotThrowInUnexpectedLocationTest ()
		{
			// make sure OnAssembly / OnType / OnMethod are fired
			FireEvents = true;
		}

		private sealed class Inapplicable {
			public override string ToString ()
			{
				return "Inapplicable";	// doesn't have an instruction which can throw
			}
		}

		private sealed class Good1 {
			public int State { get; set; }
			
			public override string ToString ()
			{
				string result;
				
				try
				{
					if (State < 0)
						throw new Exception ("state is negative");
					
					result = "Good1";
				} catch (Exception e) {		// has a catch block so the throw is OK
					result = e.Message;
				}
				
				return result;
			}
		}

		private sealed class Good2 : IEqualityComparer<Good1> {			
			public bool Equals (Good1 x, Good1 y)
			{
				return x.State == y.State;
			}
						
			public int GetHashCode (Good1 x)
			{
				if (x.State < 0)
					throw new ArgumentException ("state must be non-negative", "x");	// ok to throw ArgumentException here
					
				return x.State;
			}
		}

		private sealed class Good3 : IEqualityComparer<Good1> {			
			public bool Equals (Good1 x, Good1 y)
			{
				return x.State == y.State;
			}
						
			public int GetHashCode (Good1 x)
			{
				if (x.State < 0)
					throw new ArgumentOutOfRangeException ("x", "state must be non-negative");	// subclass of ArgumentException so OK
					
				return x.State;
			}
		}

		private sealed class Good4 {
			public int State { get; set; }
			
			public override bool Equals (object obj)
			{
				if (!(obj is Good4))
					return false;
					
				Good4 rhs = (Good4) obj;		// cast is OK when paired with isinst
				
				return State == rhs.State;
			}
			
			public override int GetHashCode ()
			{
				if (State < 0)
					throw new ArgumentException ("oops");	// this is only allowed in IEqualityComparer
					
				return State;
			}
		}

		private sealed class Good5 {
			public int State { get; set; }
			
			public override string ToString ()
			{			
				int value = checked (State + 1000);		// checked aithmetic is OK outside GetHashCode									
				return value >= 0 ? "Good5" : "empty";
			}
		}

		private sealed class Good6 {			
			public override string ToString ()
			{			
				throw new NotImplementedException ();	// NotImplementedException is OK everywhere
			}
		}

		private struct Good7 {        
			public Good7 (int x, int y)
			{
				this.x = x;
				this.y = y;
			}
			
			public override bool Equals (object rhsObj)
			{
				if (rhsObj == null)
					return false;
				
				if (GetType () != rhsObj.GetType ()) 
					return false;
				
				Good7 rhs = (Good7) rhsObj;		// OK because of the GetType call                  
				return x == rhs.x && y == rhs.y;
			}
			
			public override int GetHashCode ()
			{
				int hash;
				
				unchecked
				{
					hash = 3*x.GetHashCode () + 7*y.GetHashCode ();
				}
				
				return hash;
			}
			
			private int x, y;
		}

		private sealed class Good8 {			
			public bool this [int index] {
				get {
					if (index < 0)
						throw new KeyNotFoundException ();	// this is OK from indexers
					
					return index > 0;
				}
			}
		}

		private sealed class Bad1 {
			public int State { get; set; }
			
			public override string ToString ()
			{				
				if (State < 0)
					throw new Exception ("state is negative");
								
				return "Bad1";
			}
		}

		private sealed class Bad2 {
			public int State { get; set; }
			
			public override string ToString ()
			{
				string result;
				
				try
				{
					if (State < 0)
						throw new Exception ("state is negative");
					
					result = "Bad2";
				} finally {		// finally blocks allow the exception to escape so they are no good
					result = "oops";
				}
				
				return result;
			}
		}

		private sealed class Bad3 {
			public string State { get; set; }
			
			public override string ToString ()
			{			
				if (State == null)
					throw new Exception ("oops");	// throws
												
				return State.Length >= 0 ? "Bad3" : "empty";
			}
		}

		// no Bad4

		private sealed class Bad5 {
			public int State { get; set; }
			
			public override bool Equals (object obj)
			{
				Bad5 rhs = obj as Bad5;
				if (rhs == null)        
					return false;
				
				return State == rhs.State;
			}
			
			public override int GetHashCode ()
			{
				return checked (State + 1000);	// checked airthmetic is not OK within GetHashCode
			}
		}

		private sealed class Bad6 {
			public int State { get; set; }
			
			public override bool Equals (object obj)
			{
				if (obj == null)        
					return false;
				
				Bad6 rhs = (Bad6) obj;				// cast
				return State == rhs.State;
			}
			
			public override int GetHashCode ()
			{
				return checked (State + 1000);
			}
		}

		private sealed class Bad7 : IEqualityComparer<Good1> {			
			public bool Equals (Good1 x, Good1 y)
			{
				return x.State == y.State;
			}
						
			public int GetHashCode (Good1 x)
			{
				if (x.State < 0)
					throw new NotSupportedException ("state must be non-negative");		// only ArgumentException is OK
					
				return x.State;
			}
		}

		private sealed class Bad8 : IEqualityComparer<Good1> {			
			public bool Equals (Good1 x, Good1 y)
			{
				if (x.State < 0)
					throw new Exception ("oops");
					
				return x.State == y.State;
			}
						
			public int GetHashCode (Good1 x)
			{
				return x.State;
			}
		}

		private sealed class Bad9 {
			public int State { get; set; }
			
			public override bool Equals (object obj)
			{
				Bad9 rhs = obj as Bad9;
				if (rhs == null)        
					return false;
				
				return State == rhs.State;
			}
			
			public override int GetHashCode ()
			{
				if (State < 0)
					throw new ArgumentException ("oops");	// this is only allowed in IEqualityComparer
					
				return State;
			}
		}

		private sealed class Bad10 : IDisposable {
			public void Dispose ()
			{
				if (disposed)        
					throw new ObjectDisposedException (GetType ().Name);	// throws
            
				Dispose (true);
				GC.SuppressFinalize (this);
			}
			
			private void Dispose (bool disposing)
			{
				if (!disposing)
					throw new Exception ("how did we get here?");	// throws
					
				disposed = true;
			}
			
			private bool disposed;
		}

		private sealed class Bad11 {
			public int State { get; set; }
			
			public static implicit operator int (Bad11 value)
			{
				if (value.State < 0)
					throw new ArgumentException ("oops");	// throws
					
				return value.State;
			}
		}

		private sealed class Bad12 {			
			public bool this [int index] {
				get {
					if (index < 0)
						throw new ApplicationException ();	// this is not OK
					
					return index > 0;
				}
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply<Inapplicable> ("ToString");	
		}

		[Test]
		public void Check ()
		{
			AssertRuleSuccess<Good1> ("ToString");
			AssertRuleSuccess<Good2> ("GetHashCode");
			AssertRuleSuccess<Good3> ("GetHashCode");
			AssertRuleSuccess<Good4> ("Equals");
			AssertRuleSuccess<Good5> ("ToString");
			AssertRuleSuccess<Good6> ("ToString");
			AssertRuleSuccess<Good7> ("Equals");
			AssertRuleSuccess<Good8> ("get_Item");

			AssertRuleFailure<Bad1> ("ToString", 1);
			AssertRuleFailure<Bad2> ("ToString", 1);
			AssertRuleFailure<Bad3> ("ToString", 1);
			AssertRuleFailure<Bad5> ("GetHashCode", 1);
			AssertRuleFailure<Bad6> ("Equals", 1);
			AssertRuleFailure<Bad6> ("GetHashCode", 1);
			AssertRuleFailure<Bad7> ("GetHashCode", 1);
			AssertRuleFailure<Bad8> ("Equals", 1);
			AssertRuleFailure<Bad9> ("GetHashCode", 1);
			AssertRuleFailure<Bad10> ("Dispose", new Type [0], 1);
			AssertRuleFailure<Bad10> ("Dispose", new Type [] {typeof (bool)}, 1);
			AssertRuleFailure<Bad11> ("op_Implicit", 1);
			AssertRuleFailure<Bad12> ("get_Item", 1);
		}

		class Bad : IDisposable {
			static Bad ()
			{
				throw new NullReferenceException ();
			}

			~Bad ()
			{
				throw new ObjectDisposedException ("a bit too soon");
			}

			int Property {
				get { throw new NotFiniteNumberException (); }
			}

			event EventHandler Handler {
				add { throw new NotFiniteNumberException (); }
				remove { throw new NotFiniteNumberException (); }
			}

			static public bool operator == (Bad a, Bad b)
			{
				// not a very good test since it recurse
				if ((a == null) || (b == null))
					throw new ArgumentNullException ();
				return Object.ReferenceEquals (a, b);
			}

			static public bool operator != (Bad a, Bad b)
			{
				if ((a == null) || (b == null))
					throw new ArgumentNullException ();
				return !Object.ReferenceEquals (a, b);
			}

			void IDisposable.Dispose ()
			{
				throw new ObjectDisposedException ("quickie");
			}

			static bool TryParse (string s, out Bad value)
			{
				if (s == null)
					throw new ArgumentNullException ();
				value = new Bad ();
				return true;
			}
		}

		class Good : IDisposable {
			static Good ()
			{
				Console.WriteLine ();
			}

			~Good ()
			{
			}

			int Property {
				get { return 0; }
			}

			event EventHandler Handler {
				add { throw new NotSupportedException (); }
				remove { }
			}

			static public bool operator == (Good a, Good b)
			{
				return Object.ReferenceEquals (a, b);
			}

			static public bool operator != (Good a, Good b)
			{
				return (!(a == b));
			}

			void IDisposable.Dispose ()
			{
				throw new NotImplementedException ();
			}

			static bool TryParse (string s, out Good value)
			{
				value = new Good ();
				return (s != null);
			}
		}

		[Test]
		public void StaticCtor ()
		{
			AssertRuleFailure<Bad> (".cctor", 1);
			AssertRuleDoesNotApply<Good> (".cctor"); // no throw
		}

		[Test]
		public void Finalizers ()
		{
			AssertRuleFailure<Bad> ("Finalize", 1);
			AssertRuleDoesNotApply<Good> ("Finalize"); // no throw
		}

		[Test]
		public void Getter ()
		{
			AssertRuleFailure<Bad> ("get_Property", 1);
			AssertRuleDoesNotApply<Good> ("get_Property"); // no throw
		}

		[Test]
		public void Events ()
		{
			AssertRuleFailure<Bad> ("add_Handler", 1);
			AssertRuleFailure<Bad> ("remove_Handler", 1);
			AssertRuleSuccess<Good> ("add_Handler"); // NotSupportedException is allowed here
			AssertRuleDoesNotApply<Good> ("remove_Handler"); // no throw
		}

		[Test]
		public void EqualityOperator ()
		{
			AssertRuleFailure<Bad> ("op_Equality", 1);
			AssertRuleDoesNotApply<Good> ("op_Equality"); // no throw
		}

		[Test]
		public void InequalityOperator ()
		{
			AssertRuleFailure<Bad> ("op_Inequality", 1);
			AssertRuleDoesNotApply<Good> ("op_Inequality"); // no throw
		}

		[Test]
		public void ExplicitDispose ()
		{
			AssertRuleFailure<Bad> ("System.IDisposable.Dispose", 1);
			// NotImplementedException is allowed here
			AssertRuleSuccess<Good> ("System.IDisposable.Dispose"); 
		}

		[Test]
		public void TryParse ()
		{
			AssertRuleFailure<Bad> ("TryParse", 1);
			AssertRuleDoesNotApply<Good> ("TryParse"); // no throw
		}

		struct Unbox {
			int i;

			public override bool Equals (object obj)
			{
				Unbox ub = (Unbox) obj;
				return ub.i == i;
			}
		}

		[Test]
		public void EqualsStructUnboxing ()
		{
			AssertRuleFailure<Unbox> ("Equals", 1);
		}
	}
}

