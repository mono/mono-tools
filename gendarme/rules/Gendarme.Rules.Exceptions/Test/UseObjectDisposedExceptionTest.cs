//
// Unit tests for UseObjectDisposedExceptionRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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
using System.Runtime.Serialization;

using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public sealed class UseObjectDisposedExceptionTest : MethodRuleTestFixture<UseObjectDisposedExceptionRule> {
		
		internal sealed class Good1 : IDisposable
		{
			// Has a throw.
			public void Write (string message)
			{
				if (disposed)
					throw new ObjectDisposedException (GetType ().Name);
					
				DoSomething ();
			}
			
			// Doesn't call any members or touch any fields.
			public void WriteA (string message)
			{
				Console.WriteLine ("fee fie fum");
			}
			
			// Simple forwarder.
			public void WriteB (string message)
			{
				Write (message + '\n');
			}
			
			// Does not need a throw.
			public void Dispose ()
			{
				DoSomething ();
			}
			
			public event EventHandler Closed;

			// These are not public.
			internal void Write2 (string message)
			{
				DoSomething ();
			}
			
			protected void Write3 (string message)
			{
				DoSomething ();
			}
			
			private void Write4 (string message)
			{
				DoSomething ();
			}
			
			internal bool disposed;
			
			private void DoSomething ()
			{
			}
		}
		
		internal sealed class Good2
		{
			// Not disposable.
			public void Write (string message)
			{
				DoSomething ();
			}
			
			private void DoSomething ()
			{
			}
		}
		
		// None of these methods should throw ObjectDisposedException.
		internal sealed class Good3 : IDisposable
		{
			// OK to just call a static method.
			public void Write (string message)
			{
				DoStaticSomething ();
			}
			
			// OK to call a non-this method.
			public void Write (Good3 other, string message)
			{
				other.Write (message);
			}
			
			// OK to access non-this field.
			public void Write (Good3 other)
			{
				Console.WriteLine (other.flag);
			}
			
			// Special case for methods which use a helper to throw
			// ObjectDisposedException.
			public void Write2 ()
			{
				CheckIfClosedThrowDisposed ();
				DoSomething ();
			}
			
			public void Close ()
			{
				DoSomething ();
			}
			
			public void Dispose ()
			{
			}
			
			internal bool disposed;
			
			public void Write3 (string message)
			{
				if (disposed)
					throw new ObjectDisposedException (GetType ().Name);
					
				DoSomething ();
			}
			
			private void CheckIfClosedThrowDisposed ()
			{
				if (disposed)
					throw new ObjectDisposedException (GetType ().Name);
			}
			
			private void DoSomething ()
			{
			}
			
			private static void DoStaticSomething ()
			{
			}
			
			internal bool flag;
		}
		
		internal sealed class Bad1 : IDisposable
		{
			// Missing throw.
			public void Write (string message)
			{
				DoSomething ();
			}
			
			// Missing throw.
			public bool Flag {
				set { flag = value; }
			}
			
			public void Dispose ()
			{
				DoSomething ();
			}
			
			internal bool disposed;
			
			private void DoSomething ()
			{
			}
			
			private bool flag;
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}
		
		[Test]
		public void Test ()
		{
			AssertRuleSuccess<Good1> ();
			AssertRuleSuccess<Good2> ();
			AssertRuleSuccess<Good3> ();
			
			AssertRuleFailure<Bad1> ("Write");
			AssertRuleFailure<Bad1> ("set_Flag");
		}
	}
}
