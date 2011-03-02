//
// Gendarme.Rules.Design.Generic.UseGenericEventHandlerRule
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
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This rule fires if an assembly defines a delegate which can be
	/// replaced by <c>System.EventHandler&lt;TEventArgs&gt;</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public delegate void AuthenticityHandler (object sender, AuthenticityEventArgs e);
	/// 
	/// public event AuthenticityHandler CheckingAuthenticity;
	/// public event AuthenticityHandler CheckedAuthenticity;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public event EventHandler&lt;AuthenticityEventArgs&gt; CheckingAuthenticity;
	/// public event EventHandler&lt;AuthenticityEventArgs&gt; CheckedAuthenticity;
	/// </code>
	/// </example>
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("This delegate definition is not needed with .NET 2.0 and later runtimes.")]
	[Solution ("Replace the delegate with System.EventHandler<TEventArgs>.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public class UseGenericEventHandlerRule : GenericsBaseRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule only apply to (non generated) delegates
			if (!type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition invoke = type.GetMethod ("Invoke");
			// this happens for System.MulticastDelegate
			if (invoke == null)
				return RuleResult.DoesNotApply;

			if (!invoke.ReturnType.IsNamed ("System", "Void"))
				return RuleResult.Success;

			if (!invoke.HasParameters)
				return RuleResult.Success;

			IList<ParameterDefinition> pdc = invoke.Parameters;
			if (pdc.Count != 2)
				return RuleResult.Success;
			if (!pdc [0].ParameterType.IsNamed ("System", "Object"))
				return RuleResult.Success;
			if (!pdc [1].ParameterType.Inherits ("System", "EventArgs"))
				return RuleResult.Success;

			Runner.Report (type, Severity.Medium, Confidence.High);
			return RuleResult.Failure;
		}
	}
}
