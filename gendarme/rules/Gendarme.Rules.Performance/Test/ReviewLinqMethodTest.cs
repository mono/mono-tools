//
// Unit tests for ReviewLinqMethodRule
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
using System.Collections.Generic;
using System.Linq;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public sealed class ReviewLinqMethodTest : MethodRuleTestFixture<ReviewLinqMethodRule> {

		private sealed class CanUseProperty {
			public void Good1 (List<string> sequence)
			{
				Console.WriteLine (sequence.Count);			// doesn't use the extension method
			}

			public void Good2 (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Count ());		// can't use a more efficient method
			}

			public void Bad1 (List<string> sequence)
			{
				Console.WriteLine (sequence.Count ());
			}

			public void Bad2 (string [] sequence)
			{
				Console.WriteLine (sequence.Count ());
			}
		}

		private sealed class CanUseAny {
			public void Good1 (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Count () > 10);	// can't use a more efficient method
			}

			public void Good2 (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Any ());				// the usual workaround
			}

			public void Good3 (IEnumerable<string> sequence)
			{
				if (sequence.Count () > 10)
					Console.WriteLine ();
			}

			public void Good4 (IEnumerable<string> sequence)
			{
				if (sequence.Any ())
					Console.WriteLine ();
			}

			public void Bad1a (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Count () > 0);
			}

			public void Bad2a (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Count () == 0);
			}

			public void Bad3a (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Count () != 0);
			}

			public void Bad1b (IEnumerable<string> sequence)
			{
				Console.WriteLine (0 < sequence.Count ());
			}

			public void Bad2b (IEnumerable<string> sequence)
			{
				Console.WriteLine (0 == sequence.Count ());
			}

			public void Bad3b (IEnumerable<string> sequence)
			{
				Console.WriteLine (0 != sequence.Count ());
			}

			public void Bad1c (IEnumerable<string> sequence)
			{
				if (sequence.Count () > 0)
					Console.WriteLine ();
			}

			public void Bad2c (IEnumerable<string> sequence)
			{
				if (sequence.Count () == 0)
					Console.WriteLine ();
			}

			public void Bad3c (IEnumerable<string> sequence)
			{
				if (sequence.Count () != 0)
					Console.WriteLine ();
			}

			public void Bad1d (IEnumerable<string> sequence)
			{
				if (0 < sequence.Count ())
					Console.WriteLine ();
			}

			public void Bad2d (IEnumerable<string> sequence)
			{
				if (0 == sequence.Count ())
					Console.WriteLine ();
			}

			public void Bad3d (IEnumerable<string> sequence)
			{
				if (0 != sequence.Count ())
					Console.WriteLine ();
			}
		}

		private sealed class CanUseSubscript1 {
			public void Good1 (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.ElementAt (10));	// can't use a more efficient method
			}

			public void Bad1 (List<string> sequence)
			{
				Console.WriteLine (sequence.ElementAt (10));
			}

			public void Bad2 (string [] sequence)
			{
				Console.WriteLine (sequence.ElementAt (10));
			}

			public void Bad3 (string [] sequence)
			{
				Console.WriteLine (sequence.ElementAtOrDefault (10));
			}
		}
		
		private sealed class CanUseSubscript2 {
			public void Good1 (IEnumerable<string> sequence)
			{
				Console.WriteLine (sequence.Last ());	// can't use a more efficient method
			}

			public void Bad1 (List<string> sequence)
			{
				Console.WriteLine (sequence.Last ());
			}

			public void Bad2 (string [] sequence)
			{
				Console.WriteLine (sequence.Last ());
			}

			public void Bad3 (string [] sequence)
			{
				Console.WriteLine (sequence.LastOrDefault ());
			}
		}
		
		private sealed class CanUseSort {
			public object Good1 (IEnumerable<string> sequence)
			{
				return sequence.OrderBy<string, string> ((string x) => x);	// can't use a more efficient method
			}

			public object Bad1 (List<string> sequence)
			{
				return sequence.OrderBy<string, string> ((string x) => x);
			}

			public object Bad2 (string [] sequence)
			{
				return sequence.OrderBy<string, string> ((string x) => x);
			}

			public object Bad3 (string [] sequence)
			{
				return sequence.OrderByDescending<string, string> ((string x) => x);
			}

			public object Bad4 (string [] sequence)
			{
				return sequence.OrderByDescending<string, string> ((string x) => x, null);
			}
		}

		public class Bug656790 {
			public virtual int CanExecute<T> (object data)
			{
				return ((IEnumerable<T>) data).Count ();
			}
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void Cases ()
		{			
			// CanUseProperty
			AssertRuleSuccess<CanUseProperty> ("Good1");
			AssertRuleSuccess<CanUseProperty> ("Good2");

			AssertRuleFailure<CanUseProperty> ("Bad1");
			AssertRuleFailure<CanUseProperty> ("Bad2");
			
			// CanUseSubscript1
			AssertRuleSuccess<CanUseSubscript1> ("Good1");

			AssertRuleFailure<CanUseSubscript1> ("Bad1");
			AssertRuleFailure<CanUseSubscript1> ("Bad2");
			AssertRuleFailure<CanUseSubscript1> ("Bad3");
			
			// CanUseSubscript2
			AssertRuleSuccess<CanUseSubscript2> ("Good1");

			AssertRuleFailure<CanUseSubscript2> ("Bad1");
			AssertRuleFailure<CanUseSubscript2> ("Bad2");
			AssertRuleFailure<CanUseSubscript2> ("Bad3");
			
			// CanUseAny
			AssertRuleSuccess<CanUseAny> ("Good1");
			AssertRuleSuccess<CanUseAny> ("Good2");
			AssertRuleSuccess<CanUseAny> ("Good3");

			AssertRuleFailure<CanUseAny> ("Bad1a");
			AssertRuleFailure<CanUseAny> ("Bad2a");
			AssertRuleFailure<CanUseAny> ("Bad3a");

			AssertRuleFailure<CanUseAny> ("Bad1b");
			AssertRuleFailure<CanUseAny> ("Bad2b");
			AssertRuleFailure<CanUseAny> ("Bad3b");
			
			AssertRuleFailure<CanUseAny> ("Bad1c");
			AssertRuleFailure<CanUseAny> ("Bad2c");
			AssertRuleFailure<CanUseAny> ("Bad3c");
			
			AssertRuleFailure<CanUseAny> ("Bad1d");
			AssertRuleFailure<CanUseAny> ("Bad2d");
			AssertRuleFailure<CanUseAny> ("Bad3d");
			
			// CanUseSort
			AssertRuleSuccess<CanUseSort> ("Good1");

			AssertRuleFailure<CanUseSort> ("Bad1");
			AssertRuleFailure<CanUseSort> ("Bad2");
			AssertRuleFailure<CanUseSort> ("Bad3");
			AssertRuleFailure<CanUseSort> ("Bad4");

			AssertRuleSuccess<Bug656790> ("CanExecute");
		}

		class Bug664556 {
			public int DoSomething (Bug664556 [] ic)
			{
				return ic.Count ();
			}
		}

		[Test]
		public void Array ()
		{
			AssertRuleFailure<Bug664556> ("DoSomething", 1);
		}
	}
}
