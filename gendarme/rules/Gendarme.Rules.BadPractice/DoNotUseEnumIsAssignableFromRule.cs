//
// Gendarme.Rules.BadPractice.DoNotUseEnumIsAssignableFromRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks for calls to <c>typeof (Enum).IsAssignableFrom (type)</c> that
	/// can be simplified to <c>type.IsEnum</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// if (typeof (Enum).IsAssignableFrom (type))  {
	///	// then the type is an enum
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// if (type.IsEnum) {
	///	// then the type is an enum.
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("This method uses typeof (Enum).IsAssignableFrom to check if a type is an enum.")]
	[Solution ("Use the Type.IsEnum property instead.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotUseEnumIsAssignableFromRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if there's no IL code
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (!IsCallToTypeIsAssignableFrom (instruction))
					continue;

				// get the previous expression's last instruction
				var previous = instruction.TraceBack (method);

				if (!IsCallToTypeGetTypeFromHandle (previous))
					continue;

				if (!IsLoadEnumToken (previous.Previous))
					continue;

				Runner.Report (method, instruction, Severity.Medium, Confidence.Normal);
			}

			return Runner.CurrentRuleResult;
		}

		static bool IsLoadEnumToken (Instruction instruction)
		{
			if (instruction.OpCode.Code != Code.Ldtoken)
				return false;

			var type = instruction.Operand as TypeReference;
			if (type == null)
				return false;

			return type.IsNamed ("System", "Enum");
		}

		static bool IsCallToTypeIsAssignableFrom (Instruction instruction)
		{
			return IsCallToTypeMethod (instruction, "IsAssignableFrom");
		}

		static bool IsCallToTypeGetTypeFromHandle (Instruction instruction)
		{
			return IsCallToTypeMethod (instruction, "GetTypeFromHandle");
		}

		static bool IsCallToTypeMethod (Instruction instruction, string name)
		{
			if (!IsCall (instruction.OpCode))
				return false;

			var operand = instruction.Operand as MethodReference;
			if (operand == null)
				return false;

			if (operand.Name != name)
				return false;

			return operand.DeclaringType.IsNamed ("System", "Type");
		}

		static bool IsCall (OpCode opcode)
		{
			return opcode.Code == Code.Call || opcode.Code == Code.Callvirt;
		}
	}
}
