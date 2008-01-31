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

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Naming {

	public class UsePreferredTermsRule : ITypeRule, IMethodRule {

		// keys are obsolete terms, values are preferred ones
		private Dictionary<string, string> preferredTerms = 
			new Dictionary<string, string> ();
		
		public UsePreferredTermsRule ()
		{
			// list is based on the FxCop naming rule (as the whole rule is inspired by it)
			// http://www.gotdotnet.com/Team/FxCop/Docs/Rules/Naming/UsePreferredTerms.html
			preferredTerms.Add ("ComPlus", "EnterpriseServices");
			preferredTerms.Add ("Cancelled", "Canceled");
			preferredTerms.Add ("Indices", "Indexes");
			preferredTerms.Add ("LogIn", "LogOn");
			preferredTerms.Add ("LogOut", "LogOff");
			preferredTerms.Add ("SignOn", "SignIn");
			preferredTerms.Add ("SignOff", "SignOut");
			preferredTerms.Add ("Writeable", "Writable");			
		}
		
		// common function checking any identifier
		private MessageCollection CheckIdentifier (string identifier, Location location, Runner runner)
		{
			Dictionary<string, string> foundTerms = new Dictionary<string, string> ();
			// scan for any obsolete terms
			foreach (KeyValuePair<string, string> pair in preferredTerms) {
				if (identifier.IndexOf (pair.Key, StringComparison.InvariantCultureIgnoreCase) != -1) {
					foundTerms.Add (pair.Key, pair.Value);
				}
			}
			if (foundTerms.Count == 0)
				return runner.RuleSuccess;
			
			// form our messages
			MessageCollection messages = new MessageCollection ();
			foreach (KeyValuePair<string, string> pair in foundTerms) {
				string errorMessage = string.Format (
					"Obsolete term '{0}' is used in the identifier. Replace it with the preferred term '{1}'.",
					pair.Key, pair.Value);
				Message message = new Message (errorMessage, location, MessageType.Error);
				messages.Add (message);
			}
			return messages;
		}

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			Location location = new Location (typeDefinition);
			return CheckIdentifier (typeDefinition.Name, location, runner);
		}

		public MessageCollection CheckMethod (MethodDefinition methodDefinition, Runner runner)
		{
			Location location = new Location (methodDefinition);
			return CheckIdentifier (methodDefinition.Name, location, runner);
		}
	}
}
