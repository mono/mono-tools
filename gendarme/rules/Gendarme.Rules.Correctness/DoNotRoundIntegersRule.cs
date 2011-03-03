//
// Gendarme.Rules.Correctness.DoNotRoundIntegersRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// ICAST: int value cast to double and then passed to Math.ceil (ICAST_INT_CAST_TO_DOUBLE_PASSED_TO_CEIL)
	// ICAST: int value cast to float and then passed to Math.round (ICAST_INT_CAST_TO_FLOAT_PASSED_TO_ROUND)

	/// <summary>
	/// This rule check for attempts to call <c>System.Math.Round</c>, <c>System.Math.Ceiling</c>, <c>System.Math.Floor</c> or
	/// <c>System.Math.Truncate</c> on an integral type. This often indicate a typo in the source code
	/// (e.g. wrong variable) or an unnecessary operation.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public decimal Compute (int x)
	/// {
	///	return Math.Truncate ((decimal) x);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public decimal Compute (int x)
	/// {
	///	return (decimal) x;
	/// }
	/// </code>
	/// </example>

	[Problem ("This method calls round/ceil/floor/truncate with an integer value.")]
	[Solution ("Verify the code logic. This could be a typo (wrong variable) or an unnecessary operation.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotRoundIntegersRule : Rule, IMethodRule {

		static TypeReference GetType (Instruction ins, MethodDefinition method)
		{
			object operand = ins.GetOperand (method);

			ParameterDefinition parameter = (operand as ParameterDefinition);
			if (parameter != null)
				return parameter.ParameterType;

			FieldDefinition field = (operand as FieldDefinition);
			if (field != null)
				return field.FieldType;

			VariableDefinition variable = (operand as VariableDefinition);
			if (variable != null)
				return variable.VariableType;

			return null; // unknown
		}

		static TypeReference GetArgumentType (Instruction ins, MethodDefinition method)
		{
			switch (ins.OpCode.Code) {
			case Code.Conv_R8:
				// a convertion to a double! what was the original type ?
				// check for unsigned convertion
				if (ins.Previous.OpCode.Code == Code.Conv_R_Un)
					ins = ins.Previous;
				break;
			case Code.Call:
			case Code.Callvirt:
				MethodReference mr = (ins.Operand as MethodReference);
				TypeReference rv = mr.ReturnType;
				// a call that return a decimal or floating point is ok
				if (rv.IsFloatingPoint ())
					return null;
				// but convertion into decimals are not...
				if (rv.IsNamed ("System", "Decimal")) {
					if (!mr.DeclaringType.IsNamed ("System", "Decimal"))
						return null;

					// ... unless it's a convertion from a FP value
					switch (mr.Name) {
					case "op_Implicit":
					case "op_Explicit":
						if (mr.Parameters [0].ParameterType.IsFloatingPoint ())
							return null;
						break;
					default:
						// this it's not likely to be an integral type or 
						// cannot be detected at analysis time (e.g. Parse)
						return null;
					}
				}
				return rv;
			default:
				return null;
			}

			// floating point values are ok (so we return null)
			TypeReference type = GetType (ins.Previous, method);
			return (type == null || type.IsFloatingPoint ()) ? null : type;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// exclude methods that don't have calls
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = ins.GetMethod ();
				if ((mr == null) || !mr.DeclaringType.IsNamed ("System", "Math"))
					continue;

				Instruction value = null;
				string name = mr.Name;
				switch (name) {
				case "Ceiling":
				case "Floor":
				case "Truncate":
					value = ins.Previous;
					break;
				case "Round":
					// variable number of arguments for different overloads
					int n = ins.GetPopCount (method);
					value = ins.Previous;
					while (value != null) {
						// stop before first parameter
						if (n == 1)
							break;
						n += value.GetPopCount (method) - value.GetPushCount ();
						value = value.Previous;
					}
					break;
				}

				if (value == null)
					continue;
				
				TypeReference type = GetArgumentType (value, method);
				if (type == null)
					continue;

				string msg = String.Format (CultureInfo.InvariantCulture, "Math.{0} called on a {1}.", 
					name, type.GetFullName ());
				Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
