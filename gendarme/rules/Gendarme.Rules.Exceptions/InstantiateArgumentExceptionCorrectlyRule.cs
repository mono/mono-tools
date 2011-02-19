//
// Gendarme.Rules.Exceptions.InstantiateArgumentExceptionCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2008 Néstor Salceda
// Copyright (C) 2008,2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule will fire if the arguments to the <c>System.ArgumentException</c>, 
	/// <c>System.ArgumentNullException</c>, <c>System.ArgumentOutOfRangeException</c>,
	/// and <c>System.DuplicateWaitObjectException</c> constructors are used incorrectly.
	/// This is a common mistake because the position of the <c>parameterName</c> argument
	/// is not consistent across these types.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void Show (string s)
	/// {
	///	if (s == null) {
	///		// the first argument should be the parameter name
	///		throw new ArgumentNullException ("string is null", "s");
	///	}
	///	if (s.Length == 0) {
	///		// the second argument should be the parameter name
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

			// for most getter and setter the property name is used
			if (method.IsProperty ()) {
				if (String.Compare (method.Name, 4, operand, 0, operand.Length, StringComparison.Ordinal) == 0)
					return true;
				// but we continue looking for parameters (e.g. indexers) unless it's the generic 'value' string
				if (operand == "value")
					return false;
			}

			// note: we already know there are Parameters for this method is we got here
			foreach (ParameterDefinition parameter in method.Parameters) {
				if (parameter.Name == operand)
					return true;
			}
			return false;
		}

		private void Report (MethodDefinition method, Instruction ins, string name)
		{
			Severity severity = ((name == "value") && method.IsProperty () &&
				!method.Name.EndsWith ("value", StringComparison.InvariantCultureIgnoreCase)) ?
				Severity.Low : Severity.High;

			Runner.Report (method, ins, severity, Confidence.Normal);
		}

		private void CheckArgumentException (IMethodSignature ctor, Instruction ins, MethodDefinition method)
		{
			// OK		public ArgumentException ()
			if (!ctor.HasParameters)
				return;

			// OK		public ArgumentException (string message)
			IList<ParameterDefinition> pdc = ctor.Parameters;
			if (pdc.Count < 2)
				return;

			// OK		public ArgumentException (string message, Exception innerException)
			if (!pdc [1].ParameterType.IsNamed ("System", "String"))
				return;

			// CHECK	public ArgumentException (string message, string paramName)
			// CHECK	public ArgumentException (string message, string paramName, Exception innerException)
			Instruction call = ins.TraceBack (method, -1);
			string name = call.Operand as string;
			if (MatchesAnyParameter (method, name))
				return;

			Report (method, ins, name);
		}

		// ctors are identical for ArgumentNullException, ArgumentOutOfRangeException and DuplicateWaitObjectException
		private void CheckOtherExceptions (IMethodSignature constructor, Instruction ins, MethodDefinition method)
		{
			// OK		public ArgumentNullException ()
			if (!constructor.HasParameters)
				return;

			// OK		protected ArgumentNullException (SerializationInfo info, StreamingContext context)
			// OK		public ArgumentNullException (string message, Exception innerException)
			IList<ParameterDefinition> pdc = constructor.Parameters;
			if ((pdc.Count == 2) && !pdc [1].ParameterType.IsNamed ("System", "String"))
				return;

			// CHECK	public ArgumentNullException (string paramName)
			// CHECK	public ArgumentNullException (string paramName, string message)
			Instruction call = ins.TraceBack (method, 0);
			string name = call.Operand as string;
			if (MatchesAnyParameter (method, name))
				return;

			Report (method, ins, name);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, the rule doesn't apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// don't process methods without parameters unless it's a special method (e.g. a property)
			// this cover cases like "if (x == null) CallLocalizedThrow();" and the inner type compilers
			// generates for yield/iterator (a field is used)
			if (!method.IsSpecialName && !method.HasParameters)
				return RuleResult.DoesNotApply;

			// and when the IL contains a NewObj instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Newobj))
				return RuleResult.DoesNotApply;

			foreach (Instruction current in method.Body.Instructions) {
				if (current.OpCode.Code != Code.Newobj)
					continue;

				MethodReference ctor = (current.Operand as MethodReference);
				TypeReference type = ctor.DeclaringType;
				if (type.Namespace != "System")
					continue;

				switch (type.Name) {
				case "ArgumentException":
					CheckArgumentException (ctor, current, method);
					break;
				case "ArgumentNullException":
				case "ArgumentOutOfRangeException":
				case "DuplicateWaitObjectException":
					CheckOtherExceptions (ctor, current, method);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
