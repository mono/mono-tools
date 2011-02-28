//
// Gendarme.Rules.Exceptions.DoNotThrowInNonCatchClausesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule detects exceptions that are throw in <c>fault</c>, <c>filter</c> or 
	/// <c>finally</c> clauses. Such exceptions will make it much harder to debug your 
	/// applications since it will hide the original exception.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// int err = 0;
	/// try {
	///	err = Initialize ();
	/// }
	/// finally {
	///	Cleanup ();
	///	if (err != 0)
	///		throw new NotSupportedException ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// try {
	///	if (Initialize () != 0)
	///		throw new NotSupportedException ();
	/// }
	/// finally {
	///	Cleanup ();
	/// }
	/// </code>
	/// </example>
	[Problem ("An exception is thrown in a fault, filter or finally clause.")]
	[Solution ("Remove the exception or move it inside the try or catch clause.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotThrowInNonCatchClausesRule : Rule, IMethodRule {

		void CheckBlock (MethodDefinition method, Instruction start, Instruction end)
		{
			Instruction ins = start;
			while (ins != end) {
				if (ins.Is (Code.Throw))
					Runner.Report (method, ins, Severity.High, Confidence.High);
				ins = ins.Next;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL...
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// ... and exceptions handlers
			MethodBody body = method.Body;
			if (!body.HasExceptionHandlers)
				return RuleResult.DoesNotApply;

			// and when the IL contains a Throw instruction (Rethrow is fine)
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Throw))
				return RuleResult.DoesNotApply;

			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				// throwing in catch handler is fine
				if (eh.HandlerType == ExceptionHandlerType.Catch)
					continue;

				CheckBlock (method, eh.HandlerStart, eh.HandlerEnd);
				if (eh.FilterStart != null)
					CheckBlock (method, eh.FilterStart, eh.HandlerStart);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
