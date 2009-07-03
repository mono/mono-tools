//
// Gendarme.Rules.Design.OperatorEqualsShouldBeOverloadedRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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
using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if a type overloads operator add <c>+</c>, or overloads operator subtract <c>-</c>,
	/// or is a value type and overrides <c>Object.Equals</c>, but equals <c>==</c> is
	/// not overloaded.
	/// </summary>
	/// <example>
	/// Bad example (add/substract):
	/// <code>
	/// class DoesNotOverloadOperatorEquals {
	///	public static int operator + (DoesNotOverloadOperatorEquals a)
	///	{
	///		return 0;
	///	}
	///	
	///	public static int operator - (DoesNotOverloadOperatorEquals a)
	///	{
	///		return 0;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (value type):
	/// <code>
	/// struct OverridesEquals {
	///	public override bool Equals (object obj)
	///	{
	///		return base.Equals (obj);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// struct OverloadsOperatorEquals {
	///	public static int operator + (OverloadsOperatorEquals a)
	///	{
	///		return 0;
	///	}
	///	
	///	public static int operator - (OverloadsOperatorEquals a)
	///	{
	///		return 0;
	///	}
	///	
	///	public static bool operator == (OverloadsOperatorEquals a, OverloadsOperatorEquals b)
	///	{
	///		return a.Equals (b);
	///	}
	///	
	///	public override bool Equals (object obj)
	///	{
	///		return base.Equals (obj);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type is a value type and overrides the Equals method or it overloads + and - operators but does not overload the == operator.")]
	[Solution ("Overload the == operator to match the results of the Equals method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2231:OverloadOperatorEqualsOnOverridingValueTypeEquals")]
	public class OperatorEqualsShouldBeOverloadedRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			if (type.HasMethod (MethodSignatures.op_Addition) && type.HasMethod (MethodSignatures.op_Subtraction)) {
				if (!type.HasMethod (MethodSignatures.op_Equality)) {
					Runner.Report (type, Severity.Low, Confidence.High, "This type implements the addition (+) and subtraction (-) operators. It should also implement the equality (==) operator.");
				}
			}

			if (type.IsValueType) {
				if (type.HasMethod (MethodSignatures.Equals) && !type.HasMethod (MethodSignatures.op_Equality)) {
					Runner.Report (type, Severity.Medium, Confidence.High, "This type overrides Object.Equals. It should also implement the equality (==) operator.");
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
