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
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule warns about methods that calls into potentially dangerous API of the .NET
	/// framework. If possible try to avoid the API (there are generally safer ways to do the
	/// same) or at least make sure your code can be safely called from others.
	/// <list type="bullet">
	/// <item>
	/// <description><c>System.GC::Collect()</c></description>
	/// </item>
	/// <item>
	/// <description><c>System.Threading.Thread::Suspend()</c> and <c>Resume()</c></description>
	/// </item>
	/// <item>
	/// <description><c>System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()</c></description>
	/// </item>
	/// <item>
	/// <description><c>System.Reflection.Assembly::LoadFrom()</c>, <c>LoadFile()</c> and 
	/// <c>LoadWithPartialName()</c></description>
	/// </item>
	/// <item>
	/// <description><c>System.Type::InvokeMember()</c> when used with 
	/// <c>BindingFlags.NonPublic</c></description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Load (string filename)
	/// {
	///	Assembly a = Assembly.LoadFile (filename);
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void Load (string filename)
	/// {
	///	AssemblyName aname = AssemblyName.GetAssemblyName (filename);
	///	// ensure it's the assembly you expect (e.g. public key, version...)
	///	Assembly a = Assembly.Load (aname);
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("There are potentially dangerous calls into your code.")]
	[Solution ("You should remove or replace the call to the dangerous method.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
	public class AvoidCallingProblematicMethodsRule : Rule, IMethodRule {

		SortedDictionary<string, Func<MethodReference, Instruction, Severity?>> problematicMethods = 
			new SortedDictionary<string, Func<MethodReference, Instruction, Severity?>> (); 

		public AvoidCallingProblematicMethodsRule ()
		{
			problematicMethods.Add ("Collect", (m, i) => 
				m.DeclaringType.IsNamed ("System", "GC") ? Severity.Critical : (Severity?) null);
			problematicMethods.Add ("Suspend", (m, i) => 
				m.DeclaringType.IsNamed ("System.Threading", "Thread") ? Severity.Medium : (Severity?) null);
			problematicMethods.Add ("Resume", (m, i) => 
				m.DeclaringType.IsNamed ("System.Threading", "Thread") ? Severity.Medium : (Severity?) null);
			problematicMethods.Add ("DangerousGetHandle", (m, i) => 
				m.DeclaringType.IsNamed ("System.Runtime.InteropServices", "SafeHandle") ? Severity.Critical : (Severity?) null);
			problematicMethods.Add ("LoadFrom", (m, i) => 
				m.DeclaringType.IsNamed ("System.Reflection", "Assembly") ? Severity.High : (Severity?) null);
			problematicMethods.Add ("LoadFile", (m, i) => 
				m.DeclaringType.IsNamed ("System.Reflection", "Assembly") ? Severity.High : (Severity?) null);
			problematicMethods.Add ("LoadWithPartialName", (m, i) => 
				m.DeclaringType.IsNamed ("System.Reflection", "Assembly") ? Severity.High : (Severity?) null);
			problematicMethods.Add ("InvokeMember", (m, i) => 
				!m.DeclaringType.IsNamed ("System", "Type") ? (Severity?) null :
					IsAccessingWithNonPublicModifiers (i) ? Severity.Critical : (Severity?) null);
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

		private Severity? IsProblematicCall (Instruction call)
		{
			MethodReference method = call.GetMethod ();
			if (method != null) {
				Func<MethodReference, Instruction, Severity?> sev;
				if (problematicMethods.TryGetValue (method.Name, out sev))
					return sev (method, call);
			}
			return null;
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if there's no IL code
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;

				Severity? severity = IsProblematicCall (instruction);
				if (severity.HasValue) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"You are calling to {0}, which is a potentially problematic method", 
						instruction.Operand);
					Runner.Report (method, instruction, severity.Value, Confidence.High, msg);
				}
			}	

			return Runner.CurrentRuleResult;
		}
	}
}
