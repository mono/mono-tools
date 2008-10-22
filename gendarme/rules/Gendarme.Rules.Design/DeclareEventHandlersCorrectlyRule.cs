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

	/// <summary>
	/// The rule inspects all events inside every type and verify if they all have correct
	/// signatures.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // if miss the second parameter, inheriting from System.EventArgs
	/// delegate void MyDelegate (int sender);
	/// 
	/// class Bad {
	///	public event MyDelegate CustomEvent;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (delegate):
	/// <code>
	/// delegate void MyDelegate (int sender, EventArgs e);
	/// 
	/// class Good {
	///	public event MyDelegate CustomEvent;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (generics):
	/// <code>
	/// class Good {
	///	public event EventHandler&lt;EventArgs&gt; CustomEvent;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("The delegate which handles the event haven't the correct signature.")]
	[Solution ("You should correct the signature, return type, parameter types or parameter names.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public class DeclareEventHandlersCorrectlyRule : Rule, ITypeRule {
		static IList<TypeReference> valid_event_handler_types = new List<TypeReference> ();

		private bool CheckReturnVoid (TypeReference eventType, MethodReference invoke)
		{
			if (String.Compare (invoke.ReturnType.ReturnType.FullName, "System.Void") == 0)
				return true;

			Runner.Report (eventType, Severity.Medium, Confidence.High, String.Format ("The delegate should return void, not {0}", invoke.ReturnType.ReturnType.FullName));
			return false;
		}

		private bool CheckAmountOfParameters (TypeReference eventType, MethodReference invoke)
		{
			if (invoke.Parameters.Count == 2)
				return true;

			Runner.Report (eventType, Severity.Medium, Confidence.High, "The delegate should have 2 parameters");
			return false;
		}

		private bool CheckParameterTypes (TypeReference eventType, ParameterDefinitionCollection invokeParameters)
		{
			bool ok = true;
			if (invokeParameters.Count >= 1) {
				string type_name = invokeParameters [0].ParameterType.FullName;
				if (String.Compare (type_name, "System.Object") != 0) {
					Runner.Report (eventType, Severity.Medium, Confidence.High, String.Format ("The first parameter should have an object, not {0}", type_name));
					ok = false;
				}
			}
			if (invokeParameters.Count >= 2) {
				if (!invokeParameters [1].ParameterType.Inherits ("System.EventArgs")) {
					Runner.Report (eventType, Severity.Medium, Confidence.High, "The second parameter should be a subclass of System.EventArgs");
					ok = false;
				}
			}
			return ok;
		}

		private bool CheckParameterName (TypeReference eventType, ParameterReference invokeParameter, string expectedName)
		{
			if (String.Compare (invokeParameter.Name, expectedName) == 0)
				return true;

			Runner.Report (eventType, Severity.Low, Confidence.High, String.Format ("The expected name is {0}, not {1}", expectedName, invokeParameter.Name));
			return false;
		}
		
		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.Events.Count == 0)  
				return RuleResult.DoesNotApply;

			// Note: this is a bit more complex than it seems at first sight
			// The "real" defect is on the delegate type, which can be a type (reference) outside the
			// specified assembly set (i.e. where we have no business to report defects against) but
			// we still want to report the defect against the EventDefinition (which is inside one of
			// the specified assemblies)
			foreach (EventDefinition each in type.Events) {
				TypeReference tr = each.EventType;
				// don't check the same type over and over again
				if (valid_event_handler_types.Contains (tr))
					continue;

				TypeDefinition td = tr.Resolve ();
				if (td == null) 
					continue;
				
				//If we are using the Generic
				//EventHandler<TEventArgs>, the compiler forces
				//us to write the correct signature
				bool valid = (td.GenericParameters.Count == 0) ?
					CheckDelegate (td) : CheckGenericDelegate (td);

				// avoid re-processing the same *valid* type multiple times
				if (valid)
					valid_event_handler_types.Add (tr);
			}
			return Runner.CurrentRuleResult;
		}

		private bool CheckDelegate (TypeReference type)
		{
			MethodReference invoke = type.GetMethod (MethodSignatures.Invoke);
			if (invoke == null)
				return false;

			// we cannot short-circuit since we would miss reporting some defects
			bool valid = CheckReturnVoid (type, invoke);
			valid &= CheckAmountOfParameters (type, invoke);
			valid &= CheckParameterTypes (type, invoke.Parameters);
			if (invoke.Parameters.Count > 0)
				valid &= CheckParameterName (type, invoke.Parameters [0], "sender");
			if (invoke.Parameters.Count > 1)
				valid &= CheckParameterName (type, invoke.Parameters [1], "e");
			return valid;
		}

		private bool CheckGenericDelegate (TypeReference type)
		{
			if (type.FullName == "System.EventHandler`1")
				return true;

			Runner.Report (type, Severity.Medium, Confidence.High, "Generic delegates should use EventHandler<TEventArgs>");
			return false;
		}

		public override void TearDown (IRunner runner)
		{
			valid_event_handler_types.Clear ();
			base.TearDown (runner);
		}
	}
}
