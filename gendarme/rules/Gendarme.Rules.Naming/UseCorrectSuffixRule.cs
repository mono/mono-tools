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
using System.Text;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensure that types that inherit from certain types or implement some interfaces
	/// are named correctly by appending the right suffix to them. E.g.
	/// <list>
	/// <item><description><c>System.Attribute</c> should end with <c>Attribute</c></description></item>
	/// <item><description><c>System.EventArgs</c> should end with <c>EventArgs</c></description></item>
	/// <item><description><c>System.Exception</c> should end with <c>Exception</c></description></item>
	/// <item><description><c>System.Collections.Queue</c> should end with <c>Collection</c> or <c>Queue</c></description></item>
	/// <item><description><c>System.Collections.Stack</c> should end with <c>Collection</c> or <c>Stack</c></description></item>
	/// <item><description><c>System.Data.DataSet</c> should end with <c>DataSet</c></description></item>
	/// <item><description><c>System.Data.DataTable</c> should end with <c>DataTable</c> or <c>Collection</c></description></item>
	/// <item><description><c>System.IO.Stream</c> should end with <c>Stream</c></description></item>
	/// <item><description><c>System.Security.IPermission</c> should end with <c>Permission</c></description></item>
	/// <item><description><c>System.Security.Policy.IMembershipCondition</c> should end with <c>Condition</c></description></item>
	/// <item><description><c>System.Collections.IDictionary</c> or <c>System.Collections.Generic.IDictionary</c> should end with <c>Dictionary</c></description></item>
	/// <item><description><c>System.Collections.ICollection</c>, <c>System.Collections.Generic.ICollection</c> or <c>System.Collections.IEnumerable</c> should end with <c>Collection</c></description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public sealed class SpecialCode : Attribute {
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public sealed class SpecialCodeAttribute : Attribute {
	///	// ...
	/// }
	/// </code>
	/// </example>

	[Problem ("This type does not end with the correct suffix. That usually happens when you define a custom attribute or exception and forget appending suffixes like 'Attribute' or 'Exception' to the type name.")]
	[Solution ("Rename the type and append the correct suffix.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
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

				string[] candidates;
				if (definedSuffixes.TryGetValue (base_name, out candidates)) {
					suffixes.AddRangeIfNew (candidates);
				} else {
					// if no suffix for base type is found, we start looking through interfaces
					foreach (TypeReference iface in current.Interfaces) {
						string interface_name = GetFullName (iface);
						if (definedSuffixes.TryGetValue (interface_name, out candidates))
							suffixes.AddRangeIfNew (candidates);
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
			// found nothing
			return (suffixes.Count == 0);
		}

		private static string ComposeMessage (List<string> candidates)
		{
			if (candidates.Count == 1)
				return String.Format ("The type name does not end with '{0}' suffix. Append it to the type name.", candidates [0]);

			StringBuilder sb = new StringBuilder ("The type name does not end with one of the following suffixes: ");
			sb.Append (candidates [0]);
			for (int i = 1; i < candidates.Count; i++) {
				sb.Append (", ");
				sb.Append (candidates [i]);
			}
			sb.Append (". Append any of them to the type name.");
			return sb.ToString ();
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
			Runner.Report (type, Severity.Medium, Confidence.High, ComposeMessage (proposedSuffixes));
			return RuleResult.Failure;
		}
	}
}
