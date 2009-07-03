// 
// Gendarme.Rules.Design.PreferEventsOverMethodsRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;

using Gendarme.Framework;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for method names that suggest they are providing similar 
	/// functionality to .NET events. When possible the method(s) should be replaced
	/// with a real event. If the methods are not using or providing 
	/// event-like features then they should be renamed since such names can confuse consumers
	/// about what the method is really doing.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public delegate void MouseUpCallback (int x, int y, MouseButtons buttons);
	/// 
	/// public class MouseController {
	///	private MouseUpCallback mouse_up_callback;
	/// 
	///	public void RaiseMouseUp (Message msg)
	///	{
	///		if (mouse_up_callback != null) {
	///			mouse_up_callback (msg.X, msg.Y, msg.Buttons);
	///		}
	///	}
	///	
	///	public void ProcessMessage (Message msg)
	///	{
	///		switch (msg.Id) {
	///		case MessageId.MouseUp: {
	///			RaiseMouseUp (msg);
	///			break;
	///		}
	///		// ... more ...
	///		default:
	///			break;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class MouseController {
	///	public event EventHandler&lt;MessageEvent&gt; MouseUp;
	///	
	///	public void ProcessMessage (Message msg)
	///	{
	///		switch (msg.Id) {
	///		case MessageId.MouseUp: {
	///			EventHandler&lt;MessageEvent&gt; handler = MouseUp;
	///			if (handler != null) {
	///				handler (new MessageEvent (msg));
	///			}
	///			break;
	///		}
	///		// ... more ...
	///		default:
	///			break;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method's name suggests that it could be replaced by an event.")]
	[Solution ("Replace the method(s) by events or rename the method to something less confusing.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
	public class PreferEventsOverMethodsRule : Rule, IMethodRule {

		static string [] SuspectPrefixes = {
			"AddOn",
			"RemoveOn",
			"Fire",
			"Raise"
		};

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// does not apply to properties and events
			if (method.IsSpecialName)
				return RuleResult.DoesNotApply;

			// check if the method name starts with one of the usual suspects
			string name = method.Name;
			foreach (string suspect in SuspectPrefixes) {
				if (name.StartsWith (suspect, StringComparison.Ordinal)) {
					Runner.Report (method, Severity.Medium, Confidence.Normal);
					// won't happen more than once
					return RuleResult.Failure;
				}
			}

			return RuleResult.Success;
		}
	}
}
