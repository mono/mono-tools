//
// Gendarme.Rules.Design.ProvideAlternativeNamesForOperatorOverloadsRule
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
using System.Collections.Generic;
using System.Globalization;
using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	// TODO: Shouldn't this only fire for externally visible operators?

	/// <summary>
	/// The rule ensure that all overloaded operators are also accessible using named 
	/// alternatives because some languages, like VB.NET, cannot use overloaded operators. 
	/// For those languages named methods should be implemented that provide the same
	/// functionality. This rule verifies that a named alternative exists for each overloaded operator.
	/// <list type="bullet">
	/// <item><term>op_UnaryPlus</term><description>Plus</description></item>
	/// <item><term>op_UnaryNegation</term><description>Negate</description></item>
	/// <item><term>op_LogicalNot</term><description>LogicalNot</description></item>
	/// <item><term>op_OnesComplement</term><description>OnesComplement</description></item>
	/// </list>
	/// <list type="bullet">
	/// <item><term>op_Increment</term><description>Increment</description></item>
	/// <item><term>op_Decrement</term><description>Decrement</description></item>
	/// <item><term>op_True</term><description>IsTrue</description></item>
	/// <item><term>op_False</term><description>IsFalse</description></item>
	/// </list>
	/// <list type="bullet">
	/// <item><term>op_Addition</term><description>Add</description></item>
	/// <item><term>op_Subtraction</term><description>Subtract</description></item>
	/// <item><term>op_Multiply</term><description>Multiply</description></item>
	/// <item><term>op_Division</term><description>Divide</description></item>
	/// <item><term>op_Modulus</term><description>Modulus</description></item>
	/// </list>
	/// <list type="bullet">
	/// <item><term>op_BitwiseAnd</term><description>BitwiseAnd</description></item>
	/// <item><term>op_BitwiseOr</term><description>BitwiseOr</description></item>
	/// <item><term>op_ExclusiveOr</term><description>ExclusiveOr</description></item>
	/// </list>
	/// <list type="bullet">
	/// <item><term>op_LeftShift</term><description>LeftShift</description></item>
	/// <item><term>op_RightShift</term><description>RightShift</description></item>
	/// </list>
	/// <list type="bullet">
	/// <item><term>op_Equality</term><description>Equals</description></item>
	/// <item><term>op_Inequality</term><description>(not) Equals</description></item>
	/// <item><term>op_GreaterThan</term><description>Compare</description></item>
	/// <item><term>op_LessThan</term><description>Compare</description></item>
	/// <item><term>op_GreaterThanOrEqual</term><description>Compare</description></item>
	/// <item><term>op_LessThanOrEqual</term><description>Compare</description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class DoesNotImplementAlternative {
	///	public static int operator + (DoesNotOverloadOperatorEquals a, DoesNotOverloadOperatorEquals b)
	///	{
	///		return 0;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class DoesImplementAdd {
	///	public static int operator + (DoesImplementAdd a, DoesImplementAdd b)
	///	{
	///		return 0;
	///	}
	///	
	///	public int Add (DoesImplementAdd a)
	///	{
	///		return this + a;
	///	}
	///}
	/// </code>
	/// </example>

	[Problem ("This type contains overloaded operators but doesn't provide named alternatives.")]
	[Solution ("Add named methods equivalent to the operators for language that do not support them (e.g. VB.NET).")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
	public class ProvideAlternativeNamesForOperatorOverloadsRule : Rule, ITypeRule {

		static string [] NoParameter = new string [] { };
		static string [] OneParameter = new string [] { null }; //new string [1] = one parameter of any type


		static MethodSignature Compare = new MethodSignature ("Compare", null, OneParameter);

		static KeyValuePair<MethodSignature, MethodSignature> [] AlternativeMethodNames = new KeyValuePair<MethodSignature, MethodSignature> [] { 
			//unary
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_UnaryPlus, new MethodSignature ("Plus", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_UnaryNegation, new MethodSignature ("Negate", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LogicalNot, new MethodSignature ("LogicalNot", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_OnesComplement, new MethodSignature ("OnesComplement", null, NoParameter)),
			
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Increment, new MethodSignature ("Increment", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Decrement, new MethodSignature ("Decrement", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_True, new MethodSignature ("IsTrue", null, NoParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_False, new MethodSignature ("IsFalse", null, NoParameter)),

			//binary
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Addition, new MethodSignature ("Add", null, OneParameter)), 
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Subtraction, new MethodSignature ("Subtract", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Multiply, new MethodSignature ("Multiply", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Division, new MethodSignature ("Divide", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Modulus, new MethodSignature ("Modulus", null, OneParameter)),

			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_BitwiseAnd, new MethodSignature ("BitwiseAnd", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_BitwiseOr, new MethodSignature ("BitwiseOr", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_ExclusiveOr, new MethodSignature ("ExclusiveOr", null, OneParameter)),
			
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LeftShift, new MethodSignature ("LeftShift", null, OneParameter)),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_RightShift, new MethodSignature ("RightShift", null, OneParameter)),
		
			// new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Equality, MethodSignatures.Equals), //handled by OverrideEqualsMethodRule
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Inequality, MethodSignatures.Equals),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThan, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LessThan, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThanOrEqual, Compare),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_LessThanOrEqual, Compare),
		};

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (var kv in AlternativeMethodNames) {
				MethodDefinition op = type.GetMethod (kv.Key);
				if (op == null)
					continue;
				bool alternativeDefined = false;
				foreach (MethodDefinition alternative in type.Methods) {
					if (kv.Value.Matches (alternative)) {
						alternativeDefined = true;
						break;
					}
				}

				if (!alternativeDefined) {
					string s = String.Format (CultureInfo.InvariantCulture,
						"This type implements the '{0}' operator. Some languages do not support overloaded operators so an alternative '{1}' method should be provided.",
						kv.Key.Name, kv.Value.Name);
					Runner.Report (op, Severity.Medium, Confidence.High, s);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
