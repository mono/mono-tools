//
// Gendarme.Rules.Interoperability.PInvokeShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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

	[Problem ("P/Invoke declarations should not be visible outside of the assembly.")]
	[Solution ("Wrap the p/invoke call into a managed class/method and include parameters, and result, validation(s).")]
	public class PInvokeShouldNotBeVisibleRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to non-p/invoke
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;
			
			// rule applies
			
			// ok if method is not visible (this include it's declaring type too)
			if (!method.IsVisible ())
				return RuleResult.Success;

			// code will work (low) but it's bad design (non-fx-like validations) and makes
			// it easier to expose security vulnerabilities
			Runner.Report (method, Severity.Low, Confidence.Total, String.Empty);
			return RuleResult.Failure;
		}
	}
}
