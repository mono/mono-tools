//
// Gendarme.Rules.Naming.UsePreferredTermsRule class
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// 	(C) 2007 Daniel Abramov
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensures that identifiers such as assemblies, namespaces, types, and members, 
	/// use the terms suggested by the .NET framework guidelines so that they are consistent
	/// with other class libraries.
	/// <list>
	/// <item><description><c>Arent</c> should be replaced with <c>AreNot</c>;</description></item>
	/// <item><description><c>Cancelled</c> should be replaced with <c>Canceled</c>;</description></item>
	/// <item><description><c>Cant</c> should be replaced with <c>Cannot</c>;</description></item>
	/// <item><description><c>ComPlus</c> should be replaced with <c>EnterpriseServices</c>;</description></item>
	/// <item><description><c>Couldnt</c> should be replaced with <c>CouldNot</c>;</description></item>
	/// <item><description><c>Didnt</c> should be replaced with <c>DidNot</c>;</description></item>
	/// <item><description><c>Doesnt</c> should be replaced with <c>DoesNot</c>;</description></item>
	/// <item><description><c>Dont</c> should be replaced with <c>DoNot</c>;</description></item>
	/// <item><description><c>Hadnt</c> should be replaced with <c>HadNot</c>;</description></item>
	/// <item><description><c>Hasnt</c> should be replaced with <c>HasNot</c>;</description></item>
	/// <item><description><c>Havent</c> should be replaced with <c>HaveNot</c>;</description></item>
	/// <item><description><c>Indices</c> should be replaced with <c>Indexes</c>;</description></item>
	/// <item><description><c>Isnt</c> should be replaced with <c>IsNot</c>;</description></item>
	/// <item><description><c>LogIn</c> should be replaced with <c>LogOn</c>;</description></item>
	/// <item><description><c>LogOut</c> should be replaced with <c>LogOff</c>;</description></item>
	/// <item><description><c>Shouldnt</c> should be replaced with <c>ShouldNot</c>;</description></item>
	/// <item><description><c>SignOn</c> should be replaced with <c>SignIn</c>;</description></item>
	/// <item><description><c>SignOff</c> should be replaced with <c>SignOut</c>;</description></item>
	/// <item><description><c>Wasnt</c> should be replaced with <c>WasNot</c>;</description></item>
	/// <item><description><c>Werent</c> should be replaced with <c>WereNot</c>;</description></item>
	/// <item><description><c>Wont</c> should be replaced with <c>WillNot</c>;</description></item>
	/// <item><description><c>Wouldnt</c> should be replaced with <c>WouldNot</c>;</description></item>
	/// <item><description><c>Writeable</c> should be replaced with <c>Writable</c>;</description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// abstract public class ComPlusSecurity {
	///	abstract public void LogIn ();
	///	abstract public void LogOut ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// abstract public class EnterpriseServicesSecurity {
	///	abstract public void LogOn ();
	///	abstract public void LogOff ();
	/// }
	/// </code>
	/// </example>

	[Problem ("The identifier contains non-recommended term(s).")]
	[Solution ("For consistency replace any non-recommended term with the preferred one.")]
	[EngineDependency (typeof (NamespaceEngine))]
	[FxCopCompatibility ("Microsoft.Naming", "CA1726:UsePreferredTerms")]
	public class UsePreferredTermsRule : Rule, IAssemblyRule, ITypeRule, IMethodRule {

		private const string Message = "Term '{0}' should be replaced with '{1}'.";

		// keys are obsolete terms, values are preferred ones
		// list is based on the FxCop naming rule (as the whole rule is inspired by it)
		// http://msdn.microsoft.com/en-us/library/ms182258.aspx
		private static Dictionary<string, string> preferredTerms =
			new Dictionary<string, string> () {
				{ "Arent", "AreNot" },
				{ "Cancelled", "Canceled" },
				{ "Cant", "Cannot" },
				{ "ComPlus", "EnterpriseServices" },
				{ "Couldnt", "CouldNot" },
				{ "Didnt", "DidNot" },
				{ "Doesnt", "DoesNot" },
				{ "Dont", "DoNot" },
				{ "Hadnt", "HadNot" },
				{ "Hasnt", "HasNot" },
				{ "Havent", "HaveNot" },
				{ "Indices", "Indexes" },
				{ "Isnt", "IsNot" },
				{ "LogIn", "LogOn" },
				{ "LogOut", "LogOff" },
				{ "Shouldnt", "ShouldNot" },
				{ "SignOn", "SignIn" },
				{ "SignOff", "SignOut" },
				{ "Wasnt", "WasNot" },
				{ "Werent", "WereNot" },
				{ "Wont", "WillNot" },
				{ "Wouldnt", "WouldNot" },
				{ "Writeable", "Writable" }
			};
		
		// Reports a defect when 'name' contains a non-recommended term.
		// * identifier: Metadata token for 'name' or null if it is a namespace.
		// * name: Name to check.
		// * severity: Severity for the defect to be reported (if any).
		private void CheckIdentifier (IMetadataTokenProvider identifier, string name, Severity severity)
		{
			// scan for any obsolete terms
			foreach (KeyValuePair<string, string> pair in preferredTerms) {
				int index = name.IndexOf (pair.Key, StringComparison.OrdinalIgnoreCase);
				if (index == -1)
					continue;
				if (index > 0 && char.IsLetter (name, index - 1) && char.IsLower (name, index))
					continue; //term is part of a larger word
				string s = String.Format (CultureInfo.InvariantCulture, Message, pair.Key, pair.Value);
				Runner.Report (identifier, severity, Confidence.High, s);
			}
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			// assembly names are very visible, severity == high
			CheckIdentifier (assembly, assembly.Name.Name, Severity.High);

			// check every namespaces inside the assembly using the NamespaceEngine
			// note: we don't reuse CheckIdentifier because we want to avoid 
			// creating Namespace instance unless necessary
			foreach (string ns in NamespaceEngine.NamespacesInside (assembly))
				CheckIdentifier (NamespaceDefinition.GetDefinition (ns), ns, Severity.Medium);

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			CheckIdentifier (type, type.Name, type.IsVisible () ? Severity.Medium : Severity.Low);

			if (type.HasFields) {
				// fields should not be visible (in most case) so we always report low
				foreach (FieldDefinition field in type.Fields)
					CheckIdentifier (field, field.Name, Severity.Low);
			}

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			CheckIdentifier (method, method.Name, method.IsVisible () ? Severity.Medium : Severity.Low);
			// we're not checking parameters
			return Runner.CurrentRuleResult;
		}
	}
}
