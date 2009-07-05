//
// Gendarme.Rules.Interoperability.PInvokeShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Andreas Noever
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

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule checks for PInvoke declaration methods that are visible outside their assembly.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [DllImport ("user32.dll")]
	/// public static extern bool MessageBeep (UInt32 beepType);
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [DllImport ("user32.dll")]
	/// internal static extern bool MessageBeep (UInt32 beepType);
	/// </code>
	/// </example>
	
	[Problem ("P/Invoke declarations should not be visible outside of their assembly.")]
	[Solution ("Reduce the visibility of the p/invoke method and make sure it is declared as static.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
	public class PInvokeShouldNotBeVisibleRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to non-p/invoke
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;
			
			// rule applies
			
			// code is very unlikely to work (because of the extra this parameter)
			// note: existing C# compilers won't compile instance p/invoke, e.g.  
			// error CS0601: The DllImport attribute must be specified on a method marked `static' and `extern'
			if (!method.IsStatic)
				Runner.Report (method, Severity.Critical, Confidence.Total);

			// code will work (low) but it's bad design (non-fx-like validations) and makes
			// it easier to expose security vulnerabilities
			if (method.IsVisible ())
				Runner.Report (method, Severity.Low, Confidence.Total);

			return Runner.CurrentRuleResult;
		}
	}
}
