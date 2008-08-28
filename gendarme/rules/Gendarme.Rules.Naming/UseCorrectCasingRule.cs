//
// UseCorrectCasingRule class
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2007 Daniel Abramov
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
using System.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	[Problem ("This identifier (namespace, type or method) violates the .NET naming conventions.")]
	[Solution ("Make all namespace, type and method names pascal-cased (like MyClass), and all parameter names must be camel-cased (like myParameter).")]
	public class UseCorrectCasingRule : Rule, ITypeRule, IMethodRule {

		HashSet<string> namespaces = new HashSet<string> ();

		// check if name is PascalCased
		private static bool IsPascalCase (string name)
		{
			if (String.IsNullOrEmpty (name))
				return true;

			return Char.IsUpper (name [0]);
		}

		// convert name to PascalCase
		private static string PascalCase (string name)
		{
			if (String.IsNullOrEmpty (name))
				return String.Empty;

			if (name.Length == 1)
				return name.ToUpperInvariant ();

			int index = IndexOfFirstCorrectChar (name);
			return Char.ToUpperInvariant (name [index]) + name.Substring (index + 1);
		}

		// check if name is camelCased
		private static bool IsCamelCase (string name)
		{
			if (String.IsNullOrEmpty (name))
				return true;

			return Char.IsLower (name [0]);
		}

		// convert name to camelCase
		private static string CamelCase (string name)
		{
			if (String.IsNullOrEmpty (name))
				return String.Empty;

			if (name.Length == 1)
				return name.ToLowerInvariant ();

			int index = IndexOfFirstCorrectChar (name);
			return Char.ToLowerInvariant (name [index]) + name.Substring (index + 1);
		}

		private static int IndexOfFirstCorrectChar (string s)
		{
			int index = 0;
			while ((index < s.Length) && (s [index] == '_'))
				index++;
			// it's possible that we won't find one, e.g. something called "_"
			return (index == s.Length) ? 0 : index;
		}

		void ReportCasingError (TypeDefinition type, string message)
		{
			Runner.Report (type, Severity.Medium, Confidence.High, message);
		}

		void CheckNamespace (TypeDefinition type)
		{
			string nspace = type.Namespace;

			if (string.IsNullOrEmpty (nspace))
				return;

			if (namespaces.Contains (nspace))
				return;

			namespaces.Add (nspace);

			foreach (string ns in nspace.Split ('.')) {
				switch (ns.Length) {
				case 1:
					ReportCasingError (type, string.Format (
						"Use of single character namespace is discouraged. Rename namespace {0}", ns));

					break;
				case 2:
					// if the subnamespace is made of 2 chars, each letter have to be uppercase
					if (ns.Any (c => Char.IsLetter (c) && Char.IsLower (c)))
						ReportCasingError (type, string.Format (
							"In namespaces made of two characters, every character have to be upper cased. Rename namespace {0}", ns));

					break;
				default:
					// if the sub namespace is made of 3 or more chars, make sure they're not all uppercase
					if (!IsPascalCase (ns) || ns.All (c => Char.IsLetter (c) && Char.IsUpper (c)))
						ReportCasingError (type, string.Format (
							"Namespaces made of three characters have to be pascal cased. Rename namespace {0}", ns));

					break;
				}
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to generated code (outside developer's control)
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// ok, rule applies, check namespace first
			CheckNamespace (type);

			// types should all be PascalCased
			if (!IsPascalCase (type.Name))
				ReportCasingError (type, string.Format (
					"Type names should all be pascal-cased. Rename '{0}' type to '{1}'.", type.Name, PascalCase (type.Name)));

			return Runner.CurrentRuleResult;
		}

		static MethodSemanticsAttributes mask = MethodSemanticsAttributes.Getter | MethodSemanticsAttributes.Setter |
			MethodSemanticsAttributes.AddOn | MethodSemanticsAttributes.RemoveOn;

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// ignore constructors (.ctor or .cctor) and compiler/tool-generated code
			if (method.IsConstructor || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// don't consider private compiler generated add / remove on events
			if ((method.IsAddOn || method.IsRemoveOn) && method.IsSynchronized && method.IsPrivate)
				return RuleResult.DoesNotApply;

			string name = method.Name;
			MethodSemanticsAttributes attrs = method.SemanticsAttributes;
			if ((attrs & mask) != 0) {
				// it's something special
				int underscore = method.Name.IndexOf ('_');
				if (underscore != -1)
					name = name.Substring (underscore + 1);
			} else if (method.IsSpecialName) {
				return RuleResult.Success;
			}

			// like types, methods/props should all be PascalCased, too
			if (!IsPascalCase (name)) {
				string errorMessage = string.Format ("By existing naming conventions, all the method and property names should all be pascal-cased (e.g. MyOperation). Rename '{0}' to '{1}'.",
								     name, PascalCase (name));
				Runner.Report (method, Severity.Medium, Confidence.High, errorMessage);
			}
			// check parameters
			foreach (string param in (from ParameterDefinition p in method.Parameters select p.Name).Distinct ()) {
				// params should all be camelCased
				if (!IsCamelCase (param)) {
					string errorMessage = string.Format ("By existing naming conventions, the parameter names should all be camel-cased (e.g. myParameter). Rename '{0}' parameter to '{1}'.",
										param, CamelCase (param));
					Runner.Report (method, Severity.Medium, Confidence.High, errorMessage);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
