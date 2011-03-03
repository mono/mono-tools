//
// Gendarme.Rules.Design.EnsureSymmetryForOverloadedOperatorsRule
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

	/// <summary>
	/// This rule checks for operators that are not overloaded in pairs. Some compilers, like
	/// the C# compilers, require you to implement some of the pairs, but other languages might
	/// not. The following pairs are checked:
	/// <list>
	/// <description>Addition and Substraction</description>
	/// <description>Multiplication and Division</description>
	/// <description>Division and Modulus</description>
	/// <description>Equality and Inequality</description>
	/// <description>True and False</description>
	/// <description>GreaterThan and LessThan</description>
	/// <description>GreaterThanOrEqual and LessThanOrEqual</description>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class DoesNotOverloadAdd {
	///	public static int operator - (DoesNotOverloadAdd left, DoesNotOverloadAdd right)
	///	{
	///		return 0;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Good {
	///	public static int operator + (Good right, Good left)
	///	{
	///		return 0;
	///	}
	///	
	///	public static int operator - (Good right, Good left)
	///	{
	///		return 0;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type should overload symetric operators together (e.g. == and !=, + and -).")]
	[Solution ("Add the missing operator.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2226:OperatorsShouldHaveSymmetricalOverloads")]
	public class EnsureSymmetryForOverloadedOperatorsRule : Rule, ITypeRule {

		private const string Message = "The '{0}' operator is present, for symmetry, the '{1}' operator should be added.";

		static KeyValuePair<MethodSignature, MethodSignature> [] SymmetricOperators_Warning = new KeyValuePair<MethodSignature, MethodSignature> [] { 
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Addition, MethodSignatures.op_Subtraction),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Multiply, MethodSignatures.op_Division),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_Division, MethodSignatures.op_Modulus),
		};
		static KeyValuePair<MethodSignature, MethodSignature> [] SymmetricOperators_Error = new KeyValuePair<MethodSignature, MethodSignature> [] { 
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThan, MethodSignatures.op_LessThan),
			new KeyValuePair<MethodSignature,MethodSignature> (MethodSignatures.op_GreaterThanOrEqual, MethodSignatures.op_LessThanOrEqual),
			new KeyValuePair<MethodSignature, MethodSignature> (MethodSignatures.op_Equality, MethodSignatures.op_Inequality),
			new KeyValuePair<MethodSignature, MethodSignature> (MethodSignatures.op_True, MethodSignatures.op_False),
		};

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsInterface || type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (var kv in SymmetricOperators_Warning)
				CheckOperatorPair (kv, type, Severity.Medium);
			foreach (var kv in SymmetricOperators_Error)
				CheckOperatorPair (kv, type, Severity.High);

			return Runner.CurrentRuleResult;
		}

		private void CheckOperatorPair (KeyValuePair<MethodSignature, MethodSignature> pair, 
			TypeReference type, Severity severity)
		{
			MethodDefinition op = type.GetMethod (pair.Key);
			if (op == null) { //first one not defined
				pair = new KeyValuePair<MethodSignature, MethodSignature> (pair.Value, pair.Key); //reverse
				op = type.GetMethod (pair.Key);
				if (op == null)
					return; //neither one is defined
			} else {
				if (type.HasMethod (pair.Value))
					return; //both are defined
			}

			string s = string.Format (CultureInfo.InvariantCulture, Message, pair.Key.Name, pair.Value.Name);
			Runner.Report (op, severity, Confidence.Total, s);
		}
	}
}
