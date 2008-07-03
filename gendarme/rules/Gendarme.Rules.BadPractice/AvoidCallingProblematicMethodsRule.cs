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
		Dictionary<string, ProblematicMethodInfo> problematicMethods = new Dictionary<string, ProblematicMethodInfo> (new StartWithEqualityComparer ()); 
		
		sealed class StartWithEqualityComparer : IEqualityComparer <string> {

			public bool Equals (string key, string source)
			{
				return source.StartsWith (key);
			}

			public int GetHashCode (string obj)
			{
				if (obj == null)
					return 0;
				//If two objects are equals, they return the
				//same hash code
				//If two objects are different, they don't have
				//to return different hash code -> and that will
				//be determined by the Equals call
				return GetHashCode ();
			}
		}

		private struct ProblematicMethodInfo {
			Severity severity;
			Predicate<Instruction> predicate;

			public ProblematicMethodInfo (Severity severity, Predicate<Instruction> predicate)
			{
				this.severity = severity;
				this.predicate = predicate;
			}
			
			public Severity Severity {
				get {
					return severity;
				}
			}

			public Predicate<Instruction> Predicate {
				get {
					return predicate;
				}
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				ProblematicMethodInfo target = (ProblematicMethodInfo) obj;
				return target.Severity == Severity && target.Predicate == Predicate;
			}

			public bool Equals (ProblematicMethodInfo target)
			{
				return target.Severity == Severity && target.Predicate == Predicate;
			}

			public override int GetHashCode ()
			{
				return Severity.GetHashCode () ^ Predicate.GetHashCode ();
			}

			public static bool operator == (ProblematicMethodInfo left, ProblematicMethodInfo right)
			{
				return left.Equals (right);
			}

			public static bool operator != (ProblematicMethodInfo left, ProblematicMethodInfo right)
			{
				return !left.Equals (right);
			}
		}

		public AvoidCallingProblematicMethodsRule ()
		{
			problematicMethods.Add ("System.Void System.GC::Collect(", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Void System.Threading.Thread::Suspend()", 
				new ProblematicMethodInfo (Severity.Medium, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Void System.Threading.Thread::Resume()", 
				new ProblematicMethodInfo (Severity.Medium, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.IntPtr System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFrom(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFile(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadWithPartialName(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Object System.Type::InvokeMember(", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return IsAccessingWithNonPublicModifiers (call);}));
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
			ProblematicMethodInfo info;
			if (problematicMethods.TryGetValue (operand, out info)) {
				if (info.Predicate (call))
					return info.Severity;
			}
			
			return null;
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl == FlowControl.Call) {
					string operand = instruction.Operand.ToString ();
					Severity? severity = IsProblematicCall (instruction, operand);
					if (severity.HasValue) 
						Runner.Report (method, instruction, severity.Value, Confidence.High, String.Format ("You are calling to {0}, which is a potentially problematic method", operand));
				}
			}	

			return Runner.CurrentRuleResult;
		}
	}
}
