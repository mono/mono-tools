//
// Gendarme.Rules.Correctness.ProvideCorrectArgumentsToFormattingMethodsRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks that the format string used with <c>String.Format</c> matches
	/// the other parameters used with the method.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	///	string s1 = String.Format ("There is nothing to format here!");
	///	// no argument to back {0}
	///	string s2 = String.Format ("Hello {0}!");
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	///	string s1 = "There is nothing to format here!";
	///	string s2 = String.Format ("Hello {0}!", name);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("You are calling a Format method without the correct arguments.  This could result in a FormatException being thrown.")]
	[Solution ("Pass the correct arguments to the formatting method.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ProvideCorrectArgumentsToFormattingMethodsRule : Rule, IMethodRule {
		static MethodSignature formatSignature = new MethodSignature ("Format", "System.String");
		static BitArray results = new BitArray (16);

		private static Instruction GetLoadStringFormatInstruction (Instruction call, MethodDefinition method, 
			int formatPosition)
		{
			Instruction loadString = call.TraceBack (method, -formatPosition);
			if (loadString == null)
				return null;

			// If we find a variable load, search the store
			while (loadString.IsLoadLocal ()) {
				Instruction storeIns = GetStoreLocal (loadString, method);
				if (storeIns == null)
					return null;
				loadString = storeIns.TraceBack (method);
				if (loadString == null)
					return null;
			}

			if (loadString.OpCode.Code != Code.Ldstr)
				return null;

			return loadString;
		}

		// Get the store instruction associated with the load instruction
		private static Instruction GetStoreLocal (Instruction loadIns, MethodDefinition method)
		{
			Instruction storeIns = loadIns.Previous;
			do {
				// look for a STLOC* instruction and compare the variable indexes
				if (storeIns.IsStoreLocal () && AreMirrorInstructions (loadIns, storeIns, method))
					return storeIns;
				storeIns = storeIns.Previous;
			} while (storeIns != null);
			return null;
		}

		// Return true if both ld and st are store and load associated instructions
		private static bool AreMirrorInstructions (Instruction ld, Instruction st, MethodDefinition method)
		{
			return (ld.GetVariable (method).Index == st.GetVariable (method).Index);
		}

		//TODO: It only works with 0 - 9 digits
		private static int GetExpectedParameters (string loaded)
		{
			results.SetAll (false);
			// if last character is { then there's no digit after it
			for (int index = 0; index < loaded.Length - 1; index++) {
				if (loaded [index] == '{') {
					char next = loaded [index + 1];
					if (Char.IsDigit (next))
						results.Set (next - '0', true);
					else if (next == '{')
						index++; // skip special {{
				}
			}

			int counter = 0;
			//TODO: Check the order of the values too, by example
			// String.Format ("{1} {2}", x, y); <-- with this impl
			// it would return 0
			foreach (bool value in results) {
				if (value)
					counter++;
			}

			return counter;
		}

		private bool TryComputeArraySize (Instruction call, MethodDefinition method, int lastParameterPosition, 
			out int elementsPushed)
		{
			elementsPushed = 0;
			Instruction loadArray = call.TraceBack (method, -lastParameterPosition);

			if (loadArray == null)
				return false;

			while (loadArray.OpCode != OpCodes.Newarr) {
				if (loadArray.OpCode == OpCodes.Dup)
					loadArray = loadArray.TraceBack (method);
				else if (loadArray.IsLoadLocal ()) {
					Instruction storeIns = GetStoreLocal (loadArray, method);
					if (storeIns == null)
						return false;
					loadArray = storeIns.TraceBack (method);
				} else
					return false;

				if (loadArray == null)
					return false;
			}

			if (loadArray.Previous == null)
				return false;

			// Previous operand should be a ldc.I4 instruction type
			object previousOperand = loadArray.Previous.GetOperand (method);
			if (!(previousOperand is int))
				return false;
			elementsPushed = (int) previousOperand;
			return true;
		}

		private void CheckCallToFormatter (Instruction call, MethodDefinition method)
		{
			MethodReference mr = (call.Operand as MethodReference);

			int formatPosition = 0;
			int nbParameters = mr.Parameters.Count;
			int elementsPushed = nbParameters - 1;

			// String.Format (string, object) -> elementsPushed = 1
			// String.Format (string, object, object) -> elementsPushed = 2
			// String.Format (string, object, object, object) -> elementsPushed = 3
			// String.Format (string, object[]) -> compute
			// String.Format (IFormatProvider, string, object[]) -> compute
			if (mr.Parameters [nbParameters - 1].ParameterType.FullName != "System.Object") {
				// If we cannot determine the array size, we succeed (well we don't fail/report)
				if (!TryComputeArraySize (call, method, nbParameters - 1, out elementsPushed))
					return;

				// String.Format (IFormatProvider, string, object[]) -> formatPosition = 1
				if (mr.Parameters [0].ParameterType.FullName != "System.String")
					formatPosition = 1;
			}

			// if we don't find the content we succeed (well we don't fail/report).
			Instruction loadString = GetLoadStringFormatInstruction (call, method, formatPosition);
			if (loadString == null)
				return;

			int expectedParameters = GetExpectedParameters ((string) loadString.Operand);
			
			//There aren't parameters, and isn't a string with {
			//characters
			if (elementsPushed == 0 && expectedParameters == 0) {
				Runner.Report (method, call, Severity.Low, Confidence.Normal, "You are calling String.Format without arguments, you can remove the call to String.Format");
				return;
			}

			if ((expectedParameters == 0) && (elementsPushed > 0)) {
				Runner.Report (method, call, Severity.Medium, Confidence.Normal, "Extra parameters");
				return;
			}

			//It's likely you are calling a method for getting the
			//formatting string.
			if (expectedParameters == 0)
				return;
			
			if (elementsPushed < expectedParameters)
				Runner.Report (method, call, Severity.Critical, Confidence.Normal, String.Format ("The String.Format method is expecting {0} parameters, but only {1} are found.", expectedParameters, elementsPushed));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, the rule doesn't apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains a Call[virt] instruction
			if (!OpCodeEngine.GetBitmask (method).Intersect (OpCodeBitmask.Calls))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;

				MethodReference mr = (instruction.Operand as MethodReference);
				if (formatSignature.Matches (mr) && (mr.DeclaringType.FullName == "System.String"))
					CheckCallToFormatter (instruction, method);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
