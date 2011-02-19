// 
// Gendarme.Rules.Gendarme.UseCorrectSuffixRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Gendarme {

	/// <summary>
	/// Types implementing IRule should have the "Rule" suffix, while other types
	/// are not allowed to have this suffix.
	/// </summary>
	/// <example>
	/// Bad example (rule type does not have a suffix):
	/// <code>
	/// public class ReviewSomething : Rule, IMethodRule {
	///	// rule code
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (non-rule type has a suffix):
	/// <code>
	/// public class SomeRule {
	///	// class code
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class ReviewSomethingRule : Rule, IMethodRule {
	///	// rule code
	/// }
	/// </code>
	/// </example>

	[Problem ("Types implementing IRule should have the 'Rule' suffix. Other types should not have this suffix.")]
	[Solution ("Change type name to follow this rule.")]
	public class UseCorrectSuffixRule : GendarmeRule, ITypeRule {
		public RuleResult CheckType (TypeDefinition type)
		{
			bool endsWithRule = type.Name.EndsWith ("Rule", StringComparison.Ordinal);
			bool implementsIRule = type.Implements ("Gendarme.Framework", "IRule");

			if (implementsIRule && !endsWithRule)
				Runner.Report (type, Severity.Medium, Confidence.High, "Type implements IRule but does not end with the 'Rule'");
			else if (!implementsIRule && endsWithRule)
				Runner.Report (type, Severity.Medium, Confidence.High, "Type does not implement IRule but ends with the 'Rule'");
			
			return Runner.CurrentRuleResult;
		}
	}
}
