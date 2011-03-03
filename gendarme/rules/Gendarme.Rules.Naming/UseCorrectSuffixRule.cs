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
using System.Globalization;
using System.Text;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensure that types that inherit from certain types or implement certain interfaces
	/// have a specific suffix. It also ensures that no other
	/// types are using those suffixes without inheriting/implementing the types/interfaces. E.g.
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

	[Problem ("This type does not end with the correct suffix. That usually happens when you define a custom attribute or exception and forget to append suffixes like 'Attribute' or 'Exception' to the type name.")]
	[Solution ("Rename the type and append the correct suffix.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class UseCorrectSuffixRule : Rule, ITypeRule {

		static Dictionary<string, HashSet<string>> definedSuffixes = new Dictionary<string, HashSet<string>> ();
		static Dictionary<string, string> namespaces = new Dictionary<string, string> ();
		static SortedDictionary<string, Func<TypeDefinition, string>> reservedSuffixes = new SortedDictionary<string, Func<TypeDefinition, string>> ();

		static UseCorrectSuffixRule ()
		{
			Add ("Attribute", "System", "Attribute", true);
			Add ("Collection", "System.Collections", "ICollection", false);
			Add ("Collection", "System.Collections", "IEnumerable", false);
			Add ("Collection", "System.Collections", "Queue", false);
			Add ("Collection", "System.Collections", "Stack", false);
			Add ("Collection", "System.Collections.Generic", "ICollection`1", false);
			Add ("Collection", "System.Data", "DataSet", false);
			Add ("Collection", "System.Data", "DataTable", false);
			Add ("Condition", "System.Security.Policy", "IMembershipCondition", true);
			Add ("DataSet", "System.Data", "DataSet", true);
			Add ("DataTable", "System.Data", "DataTable", true);
			Add ("Dictionary", "System.Collections", "IDictionary", false);
			Add ("Dictionary", "System.Collections", "IDictionary`2", false);
			Add ("EventArgs", "System", "EventArgs", true);
			Add ("Exception", "System", "Exception", true);
			Add ("Permission", "System.Security", "IPermission", true);
			Add ("Queue", "System.Collections", "Queue", true);
			Add ("Stack", "System.Collections", "Stack", true);
			Add ("Stream", "System.IO", "Stream", true);

			// special cases
			reservedSuffixes.Add ("Collection", message => CheckCollection (message));
			reservedSuffixes.Add ("Dictionary", message => CheckDictionary (message));
			reservedSuffixes.Add ("EventHandler", message => CheckEventHandler (message));

			reservedSuffixes.Add ("Delegate", message => "'Delegate' should never be used as a suffix.");
			reservedSuffixes.Add ("Enum", message => "'Enum' should never be used as a suffix.");
			reservedSuffixes.Add ("Flags", message => "'Flags' should never be used as a suffix.");
			reservedSuffixes.Add ("Ex", message => "'Ex' should not be used to create a newer version of an existing type.");
			reservedSuffixes.Add ("Impl", message => "Use the 'Core' prefix instead of 'Impl'.");
		}

		static void Add (string suffix, string nameSpace, string name, bool reserved)
		{
			if (reserved) {
				reservedSuffixes.Add (suffix, message => InheritsOrImplements (message, nameSpace, name));
			}

			HashSet<string> set;
			if (!definedSuffixes.TryGetValue (name, out set)) {
				set = new HashSet<string> ();
				definedSuffixes.Add (name, set);
				namespaces.Add (name, nameSpace);
			}
			set.Add (suffix);
		}

		static string InheritsOrImplements (TypeReference type, string nameSpace, string name)
		{
			if (type.Inherits (nameSpace, name) || type.Implements (nameSpace, name))
				return String.Empty;

			return String.Format (CultureInfo.InvariantCulture,
				"'{0}' should only be used for types that inherits or implements '{1}.{2}'.", 
				type.Name, nameSpace, name);
		}

		static string CheckCollection (TypeReference type)
		{
			if (type.Implements ("System.Collections", "ICollection") ||
				type.Implements ("System.Collections", "IEnumerable") ||
				type.Implements ("System.Collections.Generic", "ICollection`1"))
				return String.Empty;

			if (type.Inherits ("System.Collections", "Queue") || type.Inherits ("System.Collections", "Stack") || 
				type.Inherits ("System.Data", "DataSet") || type.Inherits ("System.Data", "DataTable"))
				return String.Empty;

			return "'Collection' should only be used for implementing ICollection or IEnumerable or inheriting from Queue, Stack, DataSet and DataTable.";
		}

		static string CheckDictionary (TypeReference type)
		{
			if (type.Implements ("System.Collections", "IDictionary") || type.Implements ("System.Collections.Generic", "IDictionary`2"))
				return String.Empty;
			return "'Dictionary' should only be used for types implementing IDictionary and IDictionary<TKey,TValue>.";
		}

		static string CheckEventHandler (TypeReference type)
		{
			if (type.IsDelegate ())
				return String.Empty;
			return "'EventHandler' should only be used for event handler delegates.";
		}

		static bool TryGetCandidates (TypeReference type, out HashSet<string> candidates)
		{
			string name = type.Name;
			if ((type is GenericInstanceType) || type.HasGenericParameters) {
				int pos = name.IndexOf ('`');
				if (pos != -1)
					name = name.Substring (0, pos);
			}
			
			if (definedSuffixes.TryGetValue (name, out candidates)) {
				string nspace;
				if (namespaces.TryGetValue (name, out nspace)) {
					if (nspace == type.Namespace)
						return true;
				}
			}
			candidates = null;
			return false;
		}

		// checks if type name ends with an approriate suffix
		// returns array of proposed suffixes via out suffixes parameter or empty list (if none)
		// `currentTypeSuffix' is true if `type' itself does not have an appropriate suffix
		private static bool HasRequiredSuffix (TypeDefinition type, List<string> suffixes, out bool currentTypeSuffix)
		{
			TypeDefinition current = type;
			currentTypeSuffix = false;

			while (current != null && current.BaseType != null) {
				HashSet<string> candidates;
				if (TryGetCandidates (current.BaseType, out candidates)) {
					suffixes.AddRangeIfNew (candidates);
					if (current == type)
						currentTypeSuffix = true;
				} else {
					// if no suffix for base type is found, we start looking through interfaces
					foreach (TypeReference iface in current.Interfaces) {
						if (TryGetCandidates (iface, out candidates)) {
							suffixes.AddRangeIfNew (candidates);
							if (current == type)
								currentTypeSuffix = true;
						}
					}
				}
				if (suffixes.Count > 0) {
					// if any suffixes found
					// check whether type name ends with any of these suffixes
					string tname = type.Name;
					return suffixes.Exists (delegate (string suffix) {
						return HasSuffix (tname, suffix);
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
			if (candidates.Count == 1) {
				return String.Format (CultureInfo.InvariantCulture, 
					"The type name does not end with '{0}' suffix. Append it to the type name.", 
					candidates [0]);
			}

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

		static bool HasSuffix (string typeName, string suffix)
		{
			if (suffix.Length > typeName.Length)
				return false;

			// generic aware
			int gpos = typeName.LastIndexOf ('`');
			if (gpos == -1)
				gpos = typeName.Length;
			else if (suffix.Length > gpos)
				return false;

			return (String.Compare (suffix, 0, typeName, gpos - suffix.Length, suffix.Length, StringComparison.OrdinalIgnoreCase) == 0);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to generated code (outside developer's control)
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// ok, rule applies

			string tname = type.Name;
			// first check if the current suffix is correct
			// e.g. MyAttribute where the type does not inherit from Attribute
			foreach (string suffix in reservedSuffixes.Keys) {
				if (HasSuffix (tname, suffix)) {
					Func<TypeDefinition, string> f;
					if (reservedSuffixes.TryGetValue (suffix, out f)) {
						string msg = f (type);
						// if this is a valid suffix then there's not need to check for invalid later
						if (String.IsNullOrEmpty (msg))
							return RuleResult.Success;

						Runner.Report (type, Severity.Medium, Confidence.High, msg);
					}
				}
			}

			// then check if the type should have a (or one of) specific suffixes
			// e.g. MyStuff where the type implements ICollection
			proposedSuffixes.Clear ();

			bool currentTypeSuffix;
			if (!HasRequiredSuffix (type, proposedSuffixes, out currentTypeSuffix)) {
				Confidence confidence = Confidence.High;

				// if base type itself does not have any of the proposed suffixes, lower the confidence
				if (!currentTypeSuffix) {
					TypeDefinition baseType = type.BaseType.Resolve ();
					if (null != baseType && !HasRequiredSuffix (baseType, proposedSuffixes, out currentTypeSuffix))
						confidence = Confidence.Low;
				}

				// there must be some suffixes defined, but type name doesn't end with any of them
				Runner.Report (type, Severity.Medium, confidence, ComposeMessage (proposedSuffixes));
			}

			return Runner.CurrentRuleResult;
		}
	}
}
