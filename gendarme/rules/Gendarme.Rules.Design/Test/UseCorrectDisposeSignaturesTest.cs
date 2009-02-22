// 
// Unit tests for UseCorrectDisposeSignaturesRule
//
// Authors:
//	Jesse Jones  <jesjones@mindpring.com>
//
// Copyright (C) 2009 Jesse Jones
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

using Gendarme.Rules.Design;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Design {

	[TestFixture]
	public class UseCorrectDisposeSignaturesTest : TypeRuleTestFixture<UseCorrectDisposeSignaturesRule> {
		
		public sealed class Good1 : IDisposable
		{
			public void Dispose ()		// sealed class does not need Dispose (bool)
			{
			}
		}
		
		public sealed class Good2 : IDisposable
		{
			public void Dispose ()		// but a sealed class may have Dispose (bool)
			{
				Dispose (true);
			}
			
			private void Dispose (bool disposing)
			{
			}
		}
		
		public sealed class Good3
		{
			public void Dispose ()		// non-disposable types can have weird Dispose methods
			{										// (altho OnlyUseDisposeForIDisposableTypesRule will complain)
				Dispose (1);
			}
			
			private void Dispose (int disposing)
			{
			}
		}
		
		public class Good4 : IDisposable
		{
			public void Dispose ()		// unsealed class needs Dispose (bool)
			{
				Dispose (true);
			}
			
			protected virtual void Dispose (bool disposing)
			{
			}
		}
		
		public class Good5 : Good4
		{
			protected override void Dispose (bool disposing)	// can override Dispose
			{
			}
		}
		
		public class Good6 : Good4									// but don't have to
		{
		}
		
		public sealed class Good7 : Good4
		{
			protected override void Dispose (bool disposing)	// Dispose can be protected in a sealed class if a base implements IDisposable
			{
			}
		}
		
		// This is a bit of an icky case: CriticalHandle is the guy who
		// implements IDisposable so Good8 does not need to mention
		// IDisposable.
		public abstract class Good8 : CriticalHandle, IDisposable
		{
			protected Good8 (IntPtr invalidHandleValue) : base (invalidHandleValue)
			{
			}
		}
		
		public sealed class Bad1 : IDisposable
		{
			public void Dispose ()		// types cannot have weird Dispose methods
			{
				Dispose (1);
			}
			
			private void Dispose (int disposing)
			{
			}
		}
		
		public sealed class Bad2 : IDisposable
		{
			public void Dispose ()		// types cannot have weird Dispose methods
			{
				Dispose (true);
			}
			
			private int Dispose (bool disposing)
			{
				return 1;
			}
		}
		
		public sealed class Bad3 : IDisposable
		{
			public void Dispose ()		// Dispose (bool) must be private for sealed classes
			{
				Dispose (true);
			}
			
			public void Dispose (bool disposing)
			{
			}
		}
		
		public struct Bad4 : IDisposable
		{
			public void Dispose ()		// Dispose (bool) must be private for sealed classes
			{
				Dispose (true);
			}
			
			public void Dispose (bool disposing)
			{
			}
		}
		
		public class Bad5 : IDisposable
		{
			public void Dispose ()		// unsealed class needs Dispose (bool)
			{
			}
		}
		
		public class Bad6 : IDisposable
		{
			public void Dispose ()		// unsealed class needs protected Dispose (bool)
			{
				Dispose (true);
			}
			
			private void Dispose (bool disposing)
			{
			}
		}
		
		public class Bad7 : IDisposable
		{
			public void Dispose ()		// unsealed class needs virtual Dispose (bool)
			{
				Dispose (true);
			}
			
			protected void Dispose (bool disposing)
			{
			}
		}
		
		public class Bad8 : IDisposable
		{
			public virtual void Dispose ()	// Dispose () should not be virtual
			{
				Dispose (true);
			}
			
			protected virtual void Dispose (bool disposing)
			{
			}
		}
		
		public class Bad9 : Good4
		{
			public new void Dispose ()		// can't declare a new Dispose () method
			{
				Dispose (true);
			}
			
			protected override void Dispose (bool disposing)
			{
			}
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);	
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}
		
		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();
			AssertRuleSuccess<Good2> ();
			AssertRuleSuccess<Good3> ();
			AssertRuleSuccess<Good4> ();
			AssertRuleSuccess<Good5> ();
			AssertRuleSuccess<Good6> ();
			AssertRuleSuccess<Good7> ();
			AssertRuleSuccess<Good8> ();
			
			AssertRuleFailure<Bad1> ();
			AssertRuleFailure<Bad2> ();
			AssertRuleFailure<Bad3> ();
			AssertRuleFailure<Bad4> ();
			AssertRuleFailure<Bad5> ();
			AssertRuleFailure<Bad6> ();
			AssertRuleFailure<Bad7> ();
			AssertRuleFailure<Bad8> ();
			AssertRuleFailure<Bad9> ();
		}
	}
}
