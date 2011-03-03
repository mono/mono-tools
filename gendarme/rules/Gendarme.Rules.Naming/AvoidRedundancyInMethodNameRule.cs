//
// Gendarme.Rules.Naming.AvoidRedundancyInMethodNameRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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

using System;
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule will fire if a method name embeds the type name of its first parameter.
	/// Usually, removing that type name makes the API less
	/// verbose, easier to learn, and more future-proof.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class PostOffice {
	/// 	public void SendLetter (Letter letter) {
	/// 	}
	/// 	public void SendPackage (Package package) {
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class PostOffice {
	/// 	public void Send (Letter letter) {
	/// 	}
	/// 	public void Send (Package package) {
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example:
	/// <code>
	/// class PostOffice {
	/// 	public static bool IsPackageValid (Package package) {
	/// 		return package.HasAddress &amp;&amp; package.HasStamp;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Package {
	/// 	public bool IsValid {
	/// 		get {
	/// 			return HasAddress &amp;&amp; HasStamp;
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method's name includes the type name of the first parameter. This usually makes an API more verbose and less future-proof than necessary.")]
	[Solution ("Remove the type from the method name, move the method into the parameter's type, or create an extension method (if using C#).")]
	[EngineDependency (typeof (NamespaceEngine))]
	public class AvoidRedundancyInMethodNameRule : Rule, IMethodRule {

		bool ignoreAlienNamespaces = true;

		// <summary>
		// Option to ignore parameter whose type's namespace is not defined in the set of analyzed assemblies.
		// Default is True (ignore parameters from alien namespaces).
		// </summary>
		// <param name="value"></param>
		// <returns>True to ignore parameter whose type's namespace is not defined in the set of analyzed assemblies, False otherwise</returns>
		public bool IgnoreAlienNamespaces {
			get { return ignoreAlienNamespaces; }
			set { ignoreAlienNamespaces = value; }
		}


		public RuleResult CheckMethod (MethodDefinition method)
		{
			//does not apply if method has no parameter, is a property, or a p/invoke
			if (!method.HasParameters || method.IsProperty () || method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			//if this is a constructor or override, the method name is dependent
			if (method.IsConstructor || method.IsOverride ())
				return RuleResult.DoesNotApply;

			ParameterDefinition p0 = method.Parameters [0];
			TypeReference p0type = p0.ParameterType;

			//param is out/ref, it is already not obvious (there is a rule for that)
			if (p0.IsOut || p0type.IsByReference)
				return RuleResult.DoesNotApply;

			string name = p0type.Name;
			string method_name = method.Name;
			if (name.Length == 1 || method_name.Length <= name.Length)
				return RuleResult.DoesNotApply;
			if ((method_name.Length - name.Length) < 4 && IsVaguePrefix (method_name)) //suggestion would be too vague anyway (Get/Set/Is)
				return RuleResult.DoesNotApply;
			if (!char.IsUpper (name [0])) //non-compliant naming, cannot go further (PascalWords needed)
				return RuleResult.DoesNotApply;

			//if the method return the parameter type it is most likely clearer to have it in the name
			if (method.ReturnType == p0.ParameterType)
				return RuleResult.Success;

			//if starting with name it is most likely on purpose
			if (method_name.StartsWith (name, StringComparison.Ordinal))
				return RuleResult.Success;

			int pos = method_name.LastIndexOf (name, StringComparison.Ordinal);
			if (-1 == pos)
				return RuleResult.Success;

			Confidence confidence = Confidence.Normal;
			if (pos >= method_name.Length - name.Length) //suffix, most common and most verbose case
				confidence = Confidence.High;
			else if (!char.IsUpper (method_name [pos + name.Length])) //not the end of a 'PascalWord'
				return RuleResult.Success;

			//if IgnoreAlienNamespaces is True, then check if parameter type is from one of the analyzed namespaces
			if (IgnoreAlienNamespaces && IsTypeFromAlienNamespace (p0.ParameterType))
				return RuleResult.Success; //ignored/out-of-reach, so this is a success

			//main goal is to keep the API as simple as possible so this is more severe for visible methods
			Severity severity = method.IsVisible () ? Severity.Medium : Severity.Low;

			string suggestion = GetSuggestionMethodName (method, name, pos);
			string msg;
			if (method.IsStatic) { //we already have a rule that checks if the method should be static
				string memberKind = GetSuggestionMemberKind (method);
				msg = String.Format (CultureInfo.InvariantCulture, 
					"Consider renaming method to '{2}', or extracting method to type '{0}' as {1} '{2}', or making an extension method of that type.", 
					p0.ParameterType, memberKind, suggestion);
			} else {
				msg = String.Format (CultureInfo.InvariantCulture, "Consider renaming method to '{0}'.", suggestion);
			}

			Runner.Report (method, severity, confidence, msg);
			return RuleResult.Failure;
		}

		private static string GetSuggestionMethodName (MemberReference method, string name, int posFound)
		{
			string method_name = method.Name;
			string suggestion = string.Concat (method_name.Substring (0, posFound), method_name.Substring (posFound + name.Length));
			if (suggestion.EndsWith ("In", StringComparison.Ordinal))
				return suggestion.Substring (0, suggestion.Length - 2);
			if (suggestion.EndsWith ("For", StringComparison.Ordinal))
				return suggestion.Substring (0, suggestion.Length - 3);
			if (suggestion.EndsWith ("From", StringComparison.Ordinal) || suggestion.EndsWith ("With", StringComparison.Ordinal))
				return suggestion.Substring (0, suggestion.Length - 4);
			return suggestion;
		}

		private static string GetSuggestionMemberKind (IMethodSignature method)
		{
			if (method.Parameters.Count == 1 && !method.ReturnType.IsNamed ("System", "Void"))
				return "property";
			return "method";
		}

		private static bool IsVaguePrefix (string name)
		{
			if (name.StartsWith ("Get", StringComparison.Ordinal))
				return true;
			if (name.StartsWith ("Set", StringComparison.Ordinal))
				return true;
			if (name.StartsWith ("Is", StringComparison.Ordinal))
				return true;
			return false;
		}

		static private bool IsTypeFromAlienNamespace (TypeReference type)
		{
			return !NamespaceEngine.Exists (type.Namespace);
		}
	}
}

