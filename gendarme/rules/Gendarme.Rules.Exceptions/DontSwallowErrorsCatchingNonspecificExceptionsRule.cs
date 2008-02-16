//
// DontSwallowErrorsCatchingNonspecificExceptions class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Exceptions {

	[Problem ("The method catch a nonspecific exception.")]
	[Solution ("You can rethrow the original exception, to avoid destroying the stacktrace, or you can handle more specific exceptions.")]
	public class DontSwallowErrorsCatchingNonspecificExceptionsRule : Rule, IMethodRule {

		//Added System.Object because is the code behind the following block:
		//try {
		//	File.Open (foo, bar);
		//}
		//catch {
		//}
		private string[] forbiddenTypeInCatches = {"System.Exception", "System.SystemException", "System.Object"};

		private bool IsForbiddenTypeInCatches (string typeName)
		{
			foreach (String forbiddenTypeName in forbiddenTypeInCatches) {
				if (typeName.Equals (forbiddenTypeName)) {
					return true;
				}
			}
			return false;
		}

		private static Instruction ThrowsGeneralException (ExceptionHandler exceptionHandler)
		{
			for (Instruction currentInstruction = exceptionHandler.HandlerStart; currentInstruction != exceptionHandler.HandlerEnd; currentInstruction = currentInstruction.Next) {
				if (currentInstruction.OpCode.Code == Code.Rethrow)
					return null;
			}
			return exceptionHandler.HandlerStart;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			ExceptionHandlerCollection exceptionHandlerCollection = method.Body.ExceptionHandlers;
			foreach (ExceptionHandler exceptionHandler in exceptionHandlerCollection) {
				if (exceptionHandler.Type == ExceptionHandlerType.Catch) {
					string catchTypeName = exceptionHandler.CatchType.FullName;
					if (IsForbiddenTypeInCatches (catchTypeName)) {
						Instruction throw_instruction = ThrowsGeneralException (exceptionHandler);
						if (throw_instruction != null) {
							Runner.Report (method, throw_instruction, Severity.Medium, Confidence.High, String.Empty);
						}
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
