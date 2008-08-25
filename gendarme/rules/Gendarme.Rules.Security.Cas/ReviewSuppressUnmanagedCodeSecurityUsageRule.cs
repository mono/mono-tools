//
// Gendarme.Rules.Security.Cas.ReviewSuppressUnmanagedCodeSecurityUsageRule
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

namespace Gendarme.Rules.Security.Cas {

	[Problem ("This type or method is decorated with [SuppressUnmanagedCodeSecurity] which reduce the number of security checks done to call unmanaged code.")]
	[Solution ("Ensure the use of this attribute does not compromise the security of the application.")]
	[FxCopCompatibility ("Microsoft.Security", "CA1021:ReviewSuppressUnmanagedCodeSecurityUsage")]
	public class ReviewSuppressUnmanagedCodeSecurityUsageRule : Rule, ITypeRule, IMethodRule {

		private const string SUCS = "System.Security.SuppressUnmanagedCodeSecurityAttribute";

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference [SuppressUnmanagedCodeSecurityAttribute]
			// then it's not being used inside it
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active &= (e.CurrentAssembly.Name.Name == Constants.Corlib
					|| e.CurrentModule.TypeReferences.ContainsType (SUCS));
			};
		}

		// The [SuppressUnmanagedCodeSecurity] attribute applies to
		// Classes, Interfaces, Delegates (ITypeRule) and Methods (IMethodRule)

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum)
				return RuleResult.DoesNotApply;

			if (!type.HasAttribute (SUCS))
				return RuleResult.Success;

			Runner.Report (type, Severity.Audit, Confidence.Total);
			return RuleResult.Failure;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasAttribute (SUCS))
				return RuleResult.Success;

			Runner.Report (method, Severity.Audit, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
