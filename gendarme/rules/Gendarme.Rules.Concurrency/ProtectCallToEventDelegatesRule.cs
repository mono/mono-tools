//
// Gendarme.Rules.Concurrency.ProtectCallsToEventDelegatesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule checks that event invocations are safely implemented. In particular,
	/// the event must be copied into a local to avoid race conditions and it must be
	/// checked for null before it is used (events will normally be null until a delegate is added
	/// to them).
	/// </summary>
	/// <example>
	/// Bad example (no check):
	/// <code>
	/// public event EventHandler Loading;
	/// 
	/// protected void OnLoading (EventArgs e)
	/// {
	///	// Loading field could be null, throwing a NullReferenceException
	/// 	Loading (this, e);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (race condition):
	/// <code>
	/// public event EventHandler Loading;
	/// 
	/// protected void OnLoading (EventArgs e)
	/// {
	/// 	// Loading could be non-null here
	/// 	if (Loading != null) {
	/// 		// but be null once we get here :(
	/// 		Loading (this, e);
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public event EventHandler Loading;
	/// protected void OnLoading (EventArgs e)
	/// {
	/// 	EventHandler handler = Loading;
	/// 	// handler is either null or non-null
	/// 	if (handler != null) {
	/// 		// and won't change (i.e. safe from a NullReferenceException)
	/// 		handler (this, e);
	///		// however it is still possible, like the original code, that
	///		// the Loading method will be removed before, or during its
	///		// execution. Your code should be safe against such occurance.
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The use of the event does not seems protected properly against NullReferenceException and/or race conditions.")]
	[Solution ("Fix the event use to make sure it won't be null or susceptible to a race condition.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ProtectCallToEventDelegatesRule : Rule, IMethodRule {

		static private bool CheckNull (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			// csc does a ldloc.x + ldnull + ceq for: if (variable == null)
			case Code.Ldnull:
				return (ins.Next.OpCode.Code == Code.Ceq);
			// [g]mcs will do a Br[true|false][.s]
			case Code.Brfalse:
			case Code.Brfalse_S:
			case Code.Brtrue:
			case Code.Brtrue_S:
				return true;
			case Code.Ceq:
				// csc does a ldnull + ldloc.x + ceq for: if (null == variable)
				return (ins.Previous.Previous.OpCode.Code == Code.Ldnull);
			default:
				return false;
			}
		}

		static private bool CheckVariable (MethodDefinition method, Instruction ins, VariableReference load)
		{
			// walkback to find the previous use of the variable
			Instruction previous = ins.Previous;
			while (previous != null) {
				VariableDefinition variable = previous.GetVariable (method);
				// are we talking about the same variable ?
				if ((variable != null) && (load.Index == variable.Index)) {
					if (previous.IsStoreLocal ()) {
						return false;
					} else {
						// load, check for null check
						if (CheckNull (previous.Next))
							return true;
					}
				}
				previous = previous.Previous;
			}
			return true;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to method without IL (e.g. p/invoke) or generated code (e.g. compiler or tools)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if ((ins.OpCode.Code != Code.Callvirt) && (ins.OpCode.Code != Code.Call))
					continue;

				// look for calls to .Invoke
				MethodReference mr = (ins.Operand as MethodReference);
				if (!MethodSignatures.Invoke.Matches (mr))
					continue;

				// limit ourself to events
				if (!mr.IsEventCallback ())
					continue;

				// first check if we're looking from a variable or directly from 
				// the field (bad, it could be null)
				Instruction caller = ins.TraceBack (method);
				FieldDefinition field = caller.GetField ();
				if (field != null) {
					string msg = String.Format (CultureInfo.InvariantCulture, "Possible race condition since field '{0}' is accessed directly.", field.Name);
					Runner.Report (method, ins, Severity.High, Confidence.High, msg);
				} else {
					// look for the variable, if it's not then stop analysis
					VariableDefinition load = caller.GetVariable (method);
					if ((load != null) && !CheckVariable (method, caller, load)) {
						string msg = String.Format (CultureInfo.InvariantCulture, "Variable '{0}' does not seems to be checked against null.", load.Name);
						Runner.Report (method, ins, Severity.High, Confidence.Normal, msg);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
