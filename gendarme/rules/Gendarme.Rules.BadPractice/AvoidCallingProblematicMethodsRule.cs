//
// Gendarme.Rules.BadPractice.AvoidCallingProblematicMethodsRule
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
using System.Reflection;
using System.Collections.Generic;
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.BadPractice {
	[Problem ("There are potentially dangerous calls into your code.")]
	[Solution ("You should remove or replace the call to the dangerous method.")]
	public class AvoidCallingProblematicMethodsRule : Rule, IMethodRule {

		sealed class StartWithComparer : IComparer <string> {

			public int Compare (string x, string y)
			{
				if (x.StartsWith (y))
					return 0;
				return String.CompareOrdinal (x, y);
			}
		}

		SortedDictionary<string, Func<Instruction, Severity?>> problematicMethods = new SortedDictionary<string, Func<Instruction, Severity?>> (new StartWithComparer ()); 

		public AvoidCallingProblematicMethodsRule ()
		{
			problematicMethods.Add ("System.Void System.GC::Collect(", call => Severity.Critical);
			problematicMethods.Add ("System.Void System.Threading.Thread::Suspend()", call => Severity.Medium);
			problematicMethods.Add ("System.Void System.Threading.Thread::Resume()", call => Severity.Medium);
			problematicMethods.Add ("System.IntPtr System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()", call => Severity.Critical);
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFrom(", call => Severity.High);
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFile(", call => Severity.High);
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadWithPartialName(", call => Severity.High);
			problematicMethods.Add ("System.Object System.Type::InvokeMember(", call => IsAccessingWithNonPublicModifiers (call) ? Severity.Critical : (Severity?) null);
		}

		private static bool OperandIsNonPublic (BindingFlags operand)
		{
			return (operand & BindingFlags.NonPublic) == BindingFlags.NonPublic;
		}

		private static bool IsAccessingWithNonPublicModifiers (Instruction call)
		{
			Instruction current = call;
			while (current != null) {
				if (current.OpCode == OpCodes.Ldc_I4_S)
					return OperandIsNonPublic ((BindingFlags) (sbyte) current.Operand);
				//Some compilers can use the ldc.i4
				//instruction instead
				if (current.OpCode == OpCodes.Ldc_I4)
					return OperandIsNonPublic ((BindingFlags) current.Operand);
				current = current.Previous;
			}

			return false;
		}

		private Severity? IsProblematicCall (Instruction call, string operand)
		{
			Func<Instruction, Severity?> sev;
			if (problematicMethods.TryGetValue (operand, out sev))
				return sev (call);

			return null;
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;

				string operand = instruction.Operand.ToString ();
				Severity? severity = IsProblematicCall (instruction, operand);
				if (severity.HasValue) 
					Runner.Report (method, instruction, severity.Value, Confidence.High, String.Format ("You are calling to {0}, which is a potentially problematic method", operand));
			}	

			return Runner.CurrentRuleResult;
		}
	}
}
