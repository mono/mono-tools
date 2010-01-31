//
// Gendarme.Rules.Naming.DoNotPrefixEventsWithAfterOrBeforeRule
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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensures that event names are not prefixed with <c>After</c> or <c>Before</c>.
	/// The .NET naming conventions recommend using a verb in the present and in
	/// the past tense.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	public event ResolveEventHandler BeforeResolve;
	///	public event ResolveEventHandler AfterResolve;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	public event ResolveEventHandler Resolving;	// present
	///	public event ResolveEventHandler Resolved;	// past
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains event(s) whose names start with either After or Before.")]
	[Solution ("Rename the event(s) to have a correct prefix. E.g. replace After with the future tense, and Before with the past tense.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
	public class DoNotPrefixEventsWithAfterOrBeforeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enumerations and delegates
			if (type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// quick out if there are no events
			if (!type.HasEvents)
				return RuleResult.Success;

			foreach (EventDefinition evnt in type.Events) {
				string name = evnt.Name;
				if (name.StartsWith ("After", StringComparison.Ordinal) || 
					name.StartsWith ("Before", StringComparison.Ordinal)) {
					Runner.Report (evnt, Severity.Medium, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
