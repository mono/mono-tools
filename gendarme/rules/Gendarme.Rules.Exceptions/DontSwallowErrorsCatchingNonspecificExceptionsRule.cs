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
using Gendarme.Framework.Engines;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule will fire if a catch block catches <c>System.Exception</c> or
	/// <c>System.SystemException</c> but does not rethrow the original
	/// exception. This is problematic because you don't know what went wrong
	/// so it's difficult to know that the error was handled correctly. It is better
	/// to catch a more specific set of exceptions so that you do know what went
	/// wrong and do know that it is handled correctly.
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

	[Problem ("This method catches a very general exception without rethrowing it. This is not safe to do in general and may mask problems that the caller should be made aware of.")]
	[Solution ("Rethrow the original exception (which will preserve the stacktrace of the original error) or catch a more specific exception type.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public class DoNotSwallowErrorsCatchingNonSpecificExceptionsRule : Rule, IMethodRule {

		//Added System.Object because is the code behind the following block:
		//try {
		//	File.Open (foo, bar);
		//}
		//catch {
		//}
		private static bool IsForbiddenTypeInCatches (TypeReference type)
		{
			if (type.Namespace != "System")
				return false;

			string name = type.Name;
			return ((name == "Exception") || (name == "SystemException") || (name == "Object"));
		}

		// will always return exceptionHandler.HandlerStart if there's no 'rethrow' inside the method
		private static Instruction ThrowsGeneralException (ExceptionHandler exceptionHandler)
		{
			for (Instruction currentInstruction = exceptionHandler.HandlerStart; currentInstruction != exceptionHandler.HandlerEnd && currentInstruction != null; currentInstruction = currentInstruction.Next) {
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
			MethodBody body = method.Body;
			if (!body.HasExceptionHandlers)
				return RuleResult.DoesNotApply;

			bool has_rethrow = OpCodeEngine.GetBitmask (method).Get (Code.Rethrow);
			foreach (ExceptionHandler exceptionHandler in body.ExceptionHandlers) {
				if (exceptionHandler.HandlerType == ExceptionHandlerType.Catch) {
					if (IsForbiddenTypeInCatches (exceptionHandler.CatchType)) {
						// quickly find 'throw_instruction' if there's no 'rethrow' used in this method
						Instruction throw_instruction = has_rethrow ?
							ThrowsGeneralException (exceptionHandler) :
							exceptionHandler.HandlerStart;

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
