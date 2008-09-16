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

	/// <summary>
	/// This rule is used for ensure that methods do not swallow the catched exceptions. 
	/// If you decide catch a non-specific exception, you should take care, because you 
	/// wont know exactly what went wrong. You should catch exceptions when you know why
	/// an exception can be thrown, and you can take a decision based on the failure.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// try {
	///	File.Open ("foo.txt", FileMode.Open); 
	/// }
	/// catch (Exception) {
	///	//Ooops  what's failed ??? UnauthorizedException, FileNotFoundException ??? 
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (catch a specific exception):
	/// <code>
	/// try {
	///	File.Open ("foo.txt", FileMode.Open);
	/// }
	/// catch (FileNotFoundException exception) {
	///	//I know that the system can't find the file.
	/// } 
	/// </code>
	/// </example>
	/// <example>
	/// Good example (catch all and rethrow):
	/// <code>
	/// try {
	///	File.Open ("foo.txt", FileMode.Open);
	/// }
	/// catch {
	///	Console.WriteLine ("An error has happened.");
	///	throw;  // You don't swallow the error, because you rethrow the original exception.
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.0 this rule was named DontSwallowErrorsCatchingNonspecificExceptionsRule.</remarks>

	[Problem ("The method catch a non-specific exception. This will likely hide the original problem to the callers.")]
	[Solution ("You can rethrow the original exception, to avoid destroying the stacktrace, or you can handle more specific exceptions.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public class DoNotSwallowErrorsCatchingNonSpecificExceptionsRule : Rule, IMethodRule {

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

			// and if the method has, at least one, exception handler(s)
			ExceptionHandlerCollection exceptionHandlerCollection = method.Body.ExceptionHandlers;
			if (exceptionHandlerCollection.Count == 0)
				return RuleResult.DoesNotApply;

			foreach (ExceptionHandler exceptionHandler in exceptionHandlerCollection) {
				if (exceptionHandler.Type == ExceptionHandlerType.Catch) {
					string catchTypeName = exceptionHandler.CatchType.FullName;
					if (IsForbiddenTypeInCatches (catchTypeName)) {
						Instruction throw_instruction = ThrowsGeneralException (exceptionHandler);
						if (throw_instruction != null) {
							Runner.Report (method, throw_instruction, Severity.Medium, Confidence.High);
						}
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
