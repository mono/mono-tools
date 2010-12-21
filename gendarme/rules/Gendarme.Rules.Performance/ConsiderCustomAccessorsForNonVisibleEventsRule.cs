//
// Gendarme.Rules.Performance.ConsiderCustomAccessorsForNonVisibleEventsRule
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

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for non-visible events to see if their add/remove accessors are
	/// the default ones. The default, compiler generated, accessor is marked as synchronized
	/// which means that the runtime will bracket them between <c>Monitor.Enter</c> and 
	/// <c>Monitor.Exit</c> calls. This is the safest approach unless, for non-visible events,
	/// you have a performance bottleneck around the events. In this case you should review
	/// if your code needs the locks or if you can provide an alternative to them.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// private event EventHandler&lt;TestEventArgs&gt; TimeCritical;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// static object TimeCriticalEvent = new object ();
	/// EventHandlerList events = new EventHandlerList ();
	///
	/// private event EventHandler&lt;TestEventArgs&gt; TimeCritical {
	///	add {
	///		events.AddHandler (TimeCriticalEvent, value);
	///	}
	///	remove {
	///		events.AddHandler (TimeCriticalEvent, value);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The compiler created add/remove event accessors are, by default, synchronized, i.e. the runtime will wrap them inside a Monitor.Enter/Exit.")]
	[Solution ("For non-visible events see if your code could work without being synchronized by supplying your own accessor implementations.")]
	public class ConsiderCustomAccessorsForNonVisibleEventsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to classes that defines events
			if (type.IsEnum || type.IsInterface || type.IsValueType || !type.HasEvents)
				return RuleResult.DoesNotApply;

			// type can be non-visible (private or internal) but still reachable
			// with an interface or with an attribute (internals)
			bool type_visible = type.IsVisible ();

			foreach (EventDefinition evnt in type.Events) {
				MethodDefinition adder = evnt.AddMethod;
				// we assume that Add|Remove have the same visibility
				if (adder.IsVisible ())
					continue;

				// report if Add|Remove is synchronized
				if (adder.IsSynchronized) {
					Confidence confidence = type_visible ? Confidence.Normal : Confidence.Low;
					Runner.Report (evnt, Severity.Medium, confidence);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
