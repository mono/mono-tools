//
// Gendarme.Rules.Exceptions.InstantiateArgumentExceptionCorrectlyRule
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
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	[Problem ("This method throws ArgumentException (or derived) exceptions without specifying an existing parameter name. This can hide useful information to developers.")]
	[Solution ("Fix the exception parameters to use the correct parameter name (or make sure the parameters are in the right order).")]
	public class InstantiateArgumentExceptionCorrectlyRule : Rule, IMethodRule {
		static string[] checkedExceptions = {
			"System.ArgumentException",
			"System.ArgumentNullException",
			"System.ArgumentOutOfRangeException",
			"System.DuplicateWaitObjectException"
			};

		public static TypeReference GetArgumentExceptionThrown (Instruction throwInstruction)
		{
			if (throwInstruction.Previous.OpCode == OpCodes.Newobj) {
				Instruction instantiation = throwInstruction.Previous;
				MethodReference method = (MethodReference) instantiation.Operand;
				if (Array.IndexOf (checkedExceptions, method.DeclaringType.FullName) != -1)
					return method.DeclaringType;
			}
			return null;
		}

		public static bool ParameterNameIsLastOperand (MethodDefinition method, Instruction throwInstruction, int exceptionParameters)
		{
			Instruction current = throwInstruction;
			while (current != null && exceptionParameters != 0) {
				if (current.OpCode.Code == OpCodes.Ldstr.Code) {
					string operand = (string) current.Operand;
					//The second operand, a parameter name
					if (exceptionParameters == 2) {
						if (!MatchesAnyParameter (method, operand))
							return false;
					}
					//The first operand, would be handy to
					//have a description
					else {
						//Where are you calling in order
						//to get the message
						if (current.Next != null && current.Next.OpCode.FlowControl == FlowControl.Call) {
							exceptionParameters--;
							continue;
						}
						if (!operand.Contains (" "))
							return false;
					}
					exceptionParameters--;
				}
				current = current.Previous;
			}
			return true;
		}

		static bool MatchesAnyParameter (MethodDefinition method, string operand)
		{
			if (method.IsSetter) {
				return String.Compare (method.Name.Substring (4), operand) == 0;
			}
			else {
				foreach (ParameterDefinition parameter in method.Parameters) {
					if (String.Compare (parameter.Name, operand) == 0)
						return true;
				}
			}
			return false;
		}

		public static bool ParameterIsDescription (MethodDefinition method, Instruction throwInstruction)
		{
			Instruction current = throwInstruction;

			while (current != null) {
				if (current.OpCode.Code == OpCodes.Ldstr.Code) 
					return !MatchesAnyParameter (method, (string) current.Operand);
				current = current.Previous;
			}
			return true;
		}

		private static bool IsArgumentException (TypeReference exceptionType)
		{
			return Array.IndexOf (checkedExceptions, exceptionType.FullName) == 0;
		}
		
		private static bool ContainsOnlyStringsAsParameters (MethodReference method)
		{
			foreach (ParameterDefinition parameter in method.Parameters) {
				if (parameter.ParameterType.FullName != "System.String")
					return false;
			}
			return true;
		}
	
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction current in method.Body.Instructions) {
				if (current.OpCode.Code != OpCodes.Throw.Code) 
					continue;
				
				TypeReference exceptionType = GetArgumentExceptionThrown (current);
				if (exceptionType == null)	
					continue;
				
				MethodReference constructor = (MethodReference) current.Previous.Operand;
				if (!ContainsOnlyStringsAsParameters (constructor))
					continue;
					
				int parameters = constructor.Parameters.Count;
				
				if (IsArgumentException (exceptionType)) {
					if (parameters == 1 && !ParameterIsDescription (method, current)) {
						Runner.Report (method, current, Severity.High, Confidence.Normal, "The parameter for this signature should be a description, not a parameter name.");
						continue;
					}
				
					if (parameters == 2 && !ParameterNameIsLastOperand (method, current, parameters))
						Runner.Report (method, current, Severity.High, Confidence.Normal, "The parameter order should be first the description and second the parameter name.");
				}
				else {
					if (parameters == 1 && ParameterIsDescription (method, current)) {
						Runner.Report (method, current, Severity.High, Confidence.Normal, "The parameter for this signature should be the parameter name.");
						continue;
					}
					if (parameters == 2 && ParameterNameIsLastOperand (method, current, parameters))
						Runner.Report (method, current, Severity.High, Confidence.Normal, "The parameter order should be first the parameter name and second the description.");
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
