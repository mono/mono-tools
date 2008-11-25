//
// Gendarme.Rules.Naming.AvoidRedundancyInTypeNameRule
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
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule checks if a type is prefixed with the last component of its
	/// enclosing namespace, and that removing the prefix would not cause
	/// undesirable ambiguity with its base type or a type from a parent namespace.
	/// If possible, removing the prefix from the type name usually makes an API less
	/// verbose and more autocompletion-friendly.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// namespace Foo.Lang.Compiler {
	/// 	public class CompilerContext {
	/// 	}
	/// }
	/// </code>
	/// <code>
	/// using Foo.Lang;
	/// ...
	/// Compiler.CompilerContext context = new Compiler.CompilerContext ();
	/// </code>
	/// <code>
	/// using Foo.Lang.Compiler;
	/// ...
	/// CompilerContext context = new CompilerContext ();
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// namespace Foo.Lang.Compiler {
	/// 	public class Context {
	/// 	}
	/// }
	/// </code>
	/// <code>
	/// using Foo.Lang;
	/// ...
	/// Compiler.Context context = new Compiler.Context ();
	/// </code>
	/// <code>
	/// using Foo.Lang.Compiler;
	/// ...
	/// Context context = new Context ();
	/// </code>
	/// </example>
	/// <example>
	/// Another good example (more meaningful term in the context of the namespace):
	/// <code>
	/// namespace Foo.Lang.Compiler {
	/// 	public class CompilationContext {
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type name is prefixed with the last component of its enclosing namespace. This usually makes an API more verbose and less autocompletion-friendly than necessary.")]
	[Solution ("Rename type without the prefix or replace it with a more meaningful term in the context of the namespace.")]
	[EngineDependency (typeof (NamespaceEngine))]
	public class AvoidRedundancyInTypeNameRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsGeneratedCode () || string.IsNullOrEmpty (type.Namespace))
				return RuleResult.DoesNotApply;

			int ifaceOffset = type.IsInterface ? 1 : 0;
			int lastDot = type.Namespace.LastIndexOf ('.');

			//type name is smaller or equal to namespace it cannot be a defect
			if (type.Name.Length - ifaceOffset <= (type.Namespace.Length - lastDot))
				return RuleResult.Success;

			string name = type.IsInterface ? type.Name.Substring (1) : type.Name;
			string lastComponent = type.Namespace.Substring (lastDot + 1);
			if (!name.StartsWith (lastComponent, StringComparison.Ordinal))
				return RuleResult.Success;

			//if first char of suggestion does not start with a uppercase, can ignore it
			//ie. Foo.Bar.Barometer
			if (!char.IsUpper (name [lastComponent.Length]))
				return RuleResult.Success;

			string suggestion = name.Substring (lastComponent.Length);

			//if base type name is or ends with suggestion, likely not clearer if we rename it.
			//would bring ambiguity or make suggestion looks like base of basetype
			if (null != type.BaseType) {
				if (type.BaseType.Name.EndsWith (suggestion, StringComparison.Ordinal))
					return RuleResult.Success;

				//equally, if base type starts with the prefix, it is likely a wanted pattern
				if (DoesNameStartWithPascalWord (type.BaseType.Name, lastComponent))
					return RuleResult.Success;
			}

			if (type.IsInterface)
				suggestion = string.Concat ("I", suggestion);

			//we have a really interesting candidate now, let's check that removing prefix
			//would not cause undesirable ambiguity with a type from a parent namespace
			string ns = type.Namespace;
			while (0 != ns.Length) {
				foreach (TypeDefinition typ in NamespaceEngine.TypesInside (ns)) {
					if (null == typ)
						break;
					if (suggestion == typ.Name) //ambiguity
						return RuleResult.Success;
				}
				ns = ns.Substring (0, Math.Max (0, ns.LastIndexOf ('.')));
			}

			//main goal is to keep the API as simple as possible so this is more severe for visible types
			Severity severity = type.IsVisible () ? Severity.Medium : Severity.Low;

			string msg = String.Format ("Consider renaming type to '{0}'.", suggestion);
			Runner.Report (type, severity, Confidence.Normal, msg);
			return RuleResult.Failure;
		}

		//FIXME: share this?
		private static bool DoesNameStartWithPascalWord (string name, string word)
		{
			if (!name.StartsWith (word, StringComparison.Ordinal))
				return false;
			return (name.Length > word.Length && char.IsUpper (name [word.Length]));
		}

	}
}
