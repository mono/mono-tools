//
// Gendarme.Rules.Exceptions.InstantiateArgumentExceptionCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2008 Néstor Salceda
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule check that any <c>System.ArgumentException</c>, 
	/// <c>System.ArgumentNullException</c>, <c>System.ArgumentOutOfRangeException</c> or
	/// <c>System.DuplicateWaitObjectException</c> exception created to ensure the order of
	/// their parameters, in particular the position of <c>parameterName</c>, is correct.
	/// This is a common mistake since the position is not consistent across all exceptions.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Show (string s)
	/// {
	///	if (s == null) {
	///		throw new ArgumentNullException ("string is null", "s");
	///	}
	///	if (s.Length == 0) {
	///		return new ArgumentException ("s", "string is empty");
	///	}
	///	Console.WriteLine (s);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void Show (string s)
	/// {
	///	if (s == null) {
	///		throw new ArgumentNullException ("s", "string is null");
	///	}
	///	if (s.Length == 0) {
	///		return new ArgumentException ("string is empty", "s");
	///	}
	///	Console.WriteLine (s);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This method throws ArgumentException (or derived) exceptions without specifying an existing parameter name. This can hide useful information to developers.")]
	[Solution ("Fix the exception parameters to use the correct parameter name (or make sure the parameters are in the right order).")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
	public class InstantiateArgumentExceptionCorrectlyRule : Rule, IMethodRule {

		static bool MatchesAnyParameter (MethodReference method, string operand)
		{
			if (operand == null)
				return false;

			if (method.IsProperty ()) {
				return String.Compare (method.Name.Substring (4), operand) == 0;
			} else {
				foreach (ParameterDefinition parameter in method.Parameters) {
					if (String.Compare (parameter.Name, operand) == 0)
						return true;
				}
			}
			return false;
		}

		private void CheckArgumentException (MethodReference ctor, Instruction ins, MethodDefinition method)
		{
			int parameters = ctor.Parameters.Count;
			// OK		public ArgumentException ()
			// OK		public ArgumentException (string message)
			if (parameters < 2)
				return;

			// OK		public ArgumentException (string message, Exception innerException)
			if (ctor.Parameters [1].ParameterType.FullName != "System.String")
				return;

			// CHECK	public ArgumentException (string message, string paramName)
			// CHECK	public ArgumentException (string message, string paramName, Exception innerException)
			Instruction call = ins.TraceBack (method, -1);
			if (MatchesAnyParameter (method, (call.Operand as string)))
				return;

			Runner.Report (method, ins, Severity.High, Confidence.Normal);
		}

		// ctors are identical for ArgumentNullException, ArgumentOutOfRangeException and DuplicateWaitObjectException
		private void CheckOtherExceptions (MethodReference constructor, Instruction ins, MethodDefinition method)
		{
			int parameters = constructor.Parameters.Count;
			// OK		public ArgumentNullException ()
			if (parameters < 1)
				return;

			// OK		protected ArgumentNullException (SerializationInfo info, StreamingContext context)
			// OK		public ArgumentNullException (string message, Exception innerException)
			if ((parameters == 2) && (constructor.Parameters [1].ParameterType.FullName != "System.String"))
				return;

			// CHECK	public ArgumentNullException (string paramName)
			// CHECK	public ArgumentNullException (string paramName, string message)
			Instruction call = ins.TraceBack (method, 0);
			if (MatchesAnyParameter (method, (call.Operand as string)))
				return;

			Runner.Report (method, ins, Severity.High, Confidence.Normal);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, the rule doesn't apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains a NewObj instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Newobj))
				return RuleResult.DoesNotApply;

			foreach (Instruction current in method.Body.Instructions) {
				if (current.OpCode.Code != Code.Newobj)
					continue;

				MethodReference ctor = (current.Operand as MethodReference);

				switch (ctor.DeclaringType.FullName) {
				case "System.ArgumentException":
					CheckArgumentException (ctor, current, method);
					break;
				case "System.ArgumentNullException":
				case "System.ArgumentOutOfRangeException":
				case "System.DuplicateWaitObjectException":
					CheckOtherExceptions (ctor, current, method);
					break;
				default:
					continue;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
