// 
// Gendarme.Framework.IRule interface
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

namespace Gendarme.Framework {

	/// <summary>
	/// Most basic way to manipulate a rule. To create a new rule you should consider
	/// to inherit from Rule and implement IAssemblyRule, ITypeRule and/or IMethodRule.
	/// </summary>
	public interface IRule {

		/// <summary>
		/// Turn on or off the rule. The runner won't call a Check* method on rules
		/// where Active is false. This is useful when used with the runner's events
		/// to turn off a rule if we know it's useless (e.g. for an assembly).
		/// </summary>
		bool Active { get; set; }

		/// <summary>
		/// Return the instance of the current runner.
		/// </summary>
		IRunner Runner { get; }

		/// <summary>
		/// Short name for the rule.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Unique name for the rule.
		/// </summary>
		string FullName { get; }

		/// <summary>
		/// URI to the rule documentation.
		/// </summary>
		Uri Uri { get; }

		/// <summary>
		/// Short abstract of the problem that the rule is looking for.
		/// For a complete description end-users should read the documentation URI.
		/// </summary>
		string Problem { get; }

		/// <summary>
		/// Short abstract of the solution to this problem. For a complete solution
		/// with examples (both good and bad) end-users should read the documentation URI.
		/// </summary>
		string Solution { get; }

		/// <summary>
		/// The runner will initialize each rule before starting analyzing any assembly.
		/// </summary>
		/// <param name="runner">Runner that will execute the rule during analysis.</param>
		void Initialize (IRunner runner);

		/// <summary>
		/// The runner will call TearDown on every rule once the analysis is over. 
		/// This is the last chance to report defects to the runner and the best place to clean up
		/// any temporary data (that is not required for reporting).
		/// This will be called even if Initialize was not called on the rule (e.g. an unused rule).
		/// </summary>
		void TearDown ();

		/// <summary>
		/// Defines the how the rule are going to be applied to a
		/// target according its visibility modifier.
		/// </summary>
		ApplicabilityScope ApplicabilityScope {get; set;}
	}
}
