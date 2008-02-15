//
// Gendarme.Rules.Naming.UsePreferredTermsRule class
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// 	(C) 2007 Daniel Abramov
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
using System.Text;

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Naming {

	[Problem ("The identifier contains some obsolete terms.")]
	[Solution ("For consistency replace any obsolete terms with the preferred ones.")]
	public class UsePreferredTermsRule : Rule, ITypeRule, IMethodRule {

		private const string Message = "Obsolete term '{0}' should be replaced with '{1}'.";

		// keys are obsolete terms, values are preferred ones
		// list is based on the FxCop naming rule (as the whole rule is inspired by it)
		// http://www.gotdotnet.com/Team/FxCop/Docs/Rules/Naming/UsePreferredTerms.html
		private static Dictionary<string, string> preferredTerms =
			new Dictionary<string, string> () {
				{ "ComPlus", "EnterpriseServices" },
				{ "Cancelled", "Canceled" },
				{ "Indices", "Indexes" },
				{ "LogIn", "LogOn" },
				{ "LogOut", "LogOff" },
				{ "SignOn", "SignIn" },
				{ "SignOff", "SignOut" },
				{ "Writeable", "Writable" }
			};
		
		// common function checking any identifier
		private RuleResult CheckIdentifier (TypeDefinition type, MethodDefinition method, string identifier)
		{
			// scan for any obsolete terms
			foreach (KeyValuePair<string, string> pair in preferredTerms) {
				if (identifier.IndexOf (pair.Key, StringComparison.OrdinalIgnoreCase) != -1) {
					string s = String.Format (Message, pair.Key, pair.Value);
					if (type != null)
						Runner.Report (type, Severity.Low, Confidence.High, s);
					else
						Runner.Report (method, Severity.Low, Confidence.High, s);
				}
			}
			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			return CheckIdentifier (type, null, type.Name);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			return CheckIdentifier (null, method, method.Name);
		}
	}
}
