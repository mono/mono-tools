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
using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// The rule ensure that all overloaded operators are also accessible used named 
	/// alternatives because some languages, like VB.NET, cannot use overloaded operators. 
	/// For those languages named methods should be implemented that provide the same
	/// functionality. This rule checks for a named alternative for each overloaded operator.
	/// <list type="bullet">
	/// <item>op_UnaryPlus</item><description>Plus</description>
	/// <item>op_UnaryNegation</item><description>Negate</description>
	/// <item>op_LogicalNot</item><description>LogicalNot</description>
	/// <item>op_OnesComplement</item><description>OnesComplement</description>
	/// </list>
	/// <list type="bullet">
	/// <item>op_Increment</item><description>Increment</description>
	/// <item>op_Decrement</item><description>Decrement</description>
	/// <item>op_True</item><description>IsTrue</description>
	/// <item>op_False</item><description>IsFalse</description>
	/// </list>
	/// <list type="bullet">
	/// <item>op_Addition</item><description>Add</description>
	/// <item>op_Subtraction</item><description>Subtract</description>
	/// <item>op_Multiply</item><description>Multiply</description>
	/// <item>op_Division</item><description>Divide</description>
	/// <item>op_Modulus</item><description>Modulus</description>
	/// </list>
	/// <list type="bullet">
	/// <item>op_BitwiseAnd</item><description>BitwiseAnd</description>
	/// <item>op_BitwiseOr</item><description>BitwiseOr</description>
	/// <item>op_ExclusiveOr</item><description>ExclusiveOr</description>
	/// </list>
	/// <list type="bullet">
	/// <item>op_LeftShift</item><description>LeftShift</description>
	/// <item>op_RightShift</item><description>RightShift</description>
	/// </list>
	/// <list type="bullet">
	/// <item>op_Equality</item><description>Equals</description>
	/// <item>op_Inequality</item><description>(not) Equals</description>
	/// <item>op_GreaterThan</item><description>Compare</description>
	/// <item>op_LessThan</item><description>Compare</description>
	/// <item>op_GreaterThanOrEqual</item><description>Compare</description>
	/// <item>op_LessThanOrEqual</item><description>Compare</description>
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

	[Problem ("This type contains overloads for some operators but doesn't provide named alternatives.")]
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
					string s = String.Format ("This type implements the '{0}' operator. Some languages do not support overloaded operators so an alternative '{1}' method should be provided.",
						kv.Key.Name, kv.Value.Name);
					Runner.Report (op, Severity.Medium, Confidence.High, s);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
