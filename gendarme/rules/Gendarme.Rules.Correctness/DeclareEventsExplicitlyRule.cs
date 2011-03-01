//
// Gendarme.Rules.Correctness.DeclareEventsExplicitlyRule
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

using Gendarme.Framework;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule detect is an event handler was declared without the <c>event</c>
	/// keyword, making the declaration a simple field in its type. Such occurances
	/// are likely a typo and should be fixed.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class EventLess {
	///	public static EventHandler&lt;EventArgs&gt; MyEvent;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Event {
	///	public static event EventHandler&lt;EventArgs&gt; MyEvent;
	/// }
	/// </code>
	/// </example>
	// suggested in https://bugzilla.novell.com/show_bug.cgi?id=669192
	[Problem ("An event handler was declared without the 'event' keyword")]
	[Solution ("Add the missing 'event' keyword to your event handler declaration")]
	public class DeclareEventsExplicitlyRule : Rule, ITypeRule {

		static bool LookForEvent (MemberReference field, TypeDefinition type)
		{
			string fname = field.Name;
			foreach (EventDefinition evnt in type.Events) {
				if (fname == evnt.Name)
					return true;
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasFields || type.IsEnum)
				return RuleResult.DoesNotApply;

			// allow to short-circuit LookForEvent if the type has no event
			bool has_events = type.HasEvents;
			foreach (FieldDefinition field in type.Fields) {
				TypeReference ftype = field.FieldType;
				if (ftype.Namespace != "System")
					continue;

				switch (ftype.Name) {
				case "EventHandler":
				case "EventHandler`1":
					// is there (any?) event matching the field name ?
					if (!has_events || !LookForEvent (field, type))
						Runner.Report (field, Severity.High, Confidence.High);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
