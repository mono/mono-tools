//
// Gendarme.Rules.Naming.UseCorrectSuffixRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//      Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Néstor Salceda
//  (C) 2007 Daniel Abramov
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	[Problem ("This type does not end with the correct suffix. That usually happens when you define a custom attribute or exception and forget appending suffixes like 'Attribute' or 'Exception' to the type name.")]
	[Solution ("Rename the type and append the correct suffix.")]
	public class UseCorrectSuffixRule : Rule, ITypeRule {

		// keys are base class names, values are arrays of possible suffixes
		private static Dictionary<string, string []> definedSuffixes =
			new Dictionary<string, string []> () {
				{ "System.Attribute", new string [] { "Attribute" } },
				{ "System.EventArgs", new string [] { "EventArgs" } },
				{ "System.Exception", new string [] { "Exception" } },
				{ "System.Collections.Queue", new string [] { "Collection", "Queue" } },
				{ "System.Collections.Stack", new string [] { "Collection", "Stack" } },
				{ "System.Data.DataSet", new string [] { "DataSet" } },
				{ "System.Data.DataTable", new string [] { "DataTable", "Collection" } },
				{ "System.IO.Stream", new string [] { "Stream" } },
				{ "System.Security.IPermission", new string [] { "Permission" } },
				{ "System.Security.Policy.IMembershipCondition", new string [] { "Condition" } },
				{ "System.Collections.IDictionary", new string [] { "Dictionary" } },
				{ "System.Collections.Generic.IDictionary", new string [] { "Dictionary" } },
				{ "System.Collections.ICollection", new string [] { "Collection" } },
				{ "System.Collections.Generic.ICollection", new string [] { "Collection", "Set" } },
				{ "System.Collections.IEnumerable", new string [] { "Collection" } }
			};

		private static string [] GetSuffixes (string baseTypeName)
		{
			if (definedSuffixes.ContainsKey (baseTypeName)) {
				return definedSuffixes [baseTypeName];
			} else {
				return new string [] { };
			}
		}

		// handle types using generics
		private static string GetFullName (TypeReference type)
		{
			string name = type.FullName;
			// handle types using generics
			if ((type is GenericInstanceType) || (type.GenericParameters.Count > 0)) {
				int pos = name.IndexOf ('`');
				name = name.Substring (0, pos);
			}
			return name;
		}

		// checks if type name ends with an approriate suffix
		// returns array of proposed suffixes via out suffixes parameter or empty list (if none)
		private static bool HasRequiredSuffix (TypeDefinition type, List<string> suffixes)
		{
			TypeDefinition current = type;

			while (current != null && current.BaseType != null) {
				string base_name = GetFullName (current.BaseType);

				// if we have any suffixes defined by base type, we select them
				if (definedSuffixes.ContainsKey (base_name)) {
					suffixes.AddRangeIfNew (GetSuffixes (base_name));
				} else {
					// if no suffix for base type is found, we start looking through interfaces
					foreach (TypeReference iface in current.Interfaces) {
						string interface_name = GetFullName (iface);
						if (definedSuffixes.ContainsKey (interface_name))
							suffixes.AddRangeIfNew (GetSuffixes (interface_name));
					}
				}
				if (suffixes.Count > 0) {
					// if any suffixes found
					// check whether type name ends with any of these suffixes
					return suffixes.Exists (delegate (string suffix) {
						return GetFullName (type).EndsWith (suffix);
					});
				} else {
					// inspect base type
					current = current.BaseType.Resolve ();
				}
			}
			// by default, return true
			return true;
		}

		private List<string> proposedSuffixes = new List<string> ();

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to generated code (outside developer's control)
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// ok, rule applies

			proposedSuffixes.Clear ();
			if (HasRequiredSuffix (type, proposedSuffixes))
				return RuleResult.Success;

			// there must be some suffixes defined, but type name doesn't end with any of them
			string messageText;
			if (proposedSuffixes.Count > 0) {
				string joinedSuffixes = proposedSuffixes [0];
				if (proposedSuffixes.Count == 1) {
					messageText = string.Format ("The class name does not end with '{0}' suffix. Append it to the type name.", proposedSuffixes [0]);
				} else {
					foreach (string suffix in proposedSuffixes)
						joinedSuffixes += ", " + suffix;
					messageText = string.Format ("The class name does not end with one of the following suffixes: {0}. Append any of them to the type name.", joinedSuffixes);
				}
			} else {
				messageText = "The class name does not end with the correct suffix. However Gendarme could not determine what suffix should it end with. Contact the author of the rule to fix this bug.";
			}
			Runner.Report (type, Severity.Medium, Confidence.High, messageText);
			return RuleResult.Failure;
		}
	}
}
