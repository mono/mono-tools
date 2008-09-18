//
// Gendarme.Rules.Design.DeclareEventHandlersCorrectlyRule
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
using System.Collections.Generic;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;

namespace Gendarme.Rules.Design {
	[Problem ("The delegate which handles the event haven't the correct signature.")]
	[Solution ("You should correct the signature, return type, parameter types or parameter names.")]
	public class DeclareEventHandlersCorrectlyRule : Rule, ITypeRule {
		static IList<TypeReference> possibleTargets = new List<TypeReference> ();

		private void CheckReturnVoid (TypeReference eventType, MethodReference invoke)
		{
			if (String.Compare (invoke.ReturnType.ReturnType.FullName, "System.Void") != 0)
				Runner.Report (eventType, Severity.Medium, Confidence.High, String.Format ("The delegate should return void, not {0}", invoke.ReturnType.ReturnType.FullName));
		}

		private void CheckAmountOfParameters (TypeReference eventType, MethodReference invoke)
		{
			if (invoke.Parameters.Count != 2)
				Runner.Report (eventType, Severity.Medium, Confidence.High, String.Format ("The delegate should have 2 parameters"));
		}

		private void CheckParameterTypes (TypeReference eventType, MethodReference invoke)
		{
			if (invoke.Parameters.Count >= 1) {
				if (String.Compare (invoke.Parameters[0].ParameterType.FullName, "System.Object") != 0)
					Runner.Report (eventType, Severity.Medium, Confidence.High, String.Format ("The first parameter should have an object, not {0}", invoke.Parameters[0].ParameterType.FullName));
			}
			if (invoke.Parameters.Count >= 2) {
				if (!invoke.Parameters[1].ParameterType.Inherits ("System.EventArgs"))
					Runner.Report (eventType, Severity.Medium, Confidence.High, "The second parameter should be a subclass of System.EventArgs");
			}
		}

		private void CheckParameterName (TypeReference eventType, MethodReference invoke, int position, string expected)
		{
			if (invoke.Parameters.Count >= position + 1) {
				if (String.Compare (invoke.Parameters[position].Name, expected) != 0)
					Runner.Report (eventType, Severity.Low, Confidence.High, String.Format ("The expected name is {0}, not {1}", expected, invoke.Parameters[position].Name));
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.Events.Count == 0)
				return RuleResult.DoesNotApply;

			foreach (EventDefinition each in type.Events) {
				if (!possibleTargets.Contains (each.EventType))
					possibleTargets.Add (each.EventType);	
			}
			return Runner.CurrentRuleResult;
		}

		public override void TearDown (IRunner runner)
		{
			foreach (TypeReference type in possibleTargets) {
				MethodReference invoke = type.GetMethod (MethodSignatures.Invoke);
				if (invoke != null) {
					CheckReturnVoid (type, invoke);
					CheckAmountOfParameters (type, invoke);
					CheckParameterTypes (type, invoke);
					CheckParameterName (type, invoke, 0, "sender");
					CheckParameterName (type, invoke, 1, "e");
				}
			}
			possibleTargets.Clear ();
			base.TearDown (runner);
		}
	}
}
