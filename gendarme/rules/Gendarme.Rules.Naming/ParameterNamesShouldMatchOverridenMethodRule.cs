//
// Gendarme.Rules.Naming.ParameterNamesShouldMatchOverriddenMethodRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule warns if an overriden method's parameter names does not match those of the 
	/// base class or those of the implemented interface. This can be confusing because it may
	/// not always be clear that it is an override or implementation of an interface method. It
	/// also makes it more difficult to use the method with languages that support named
	/// parameters (like C# 4.0).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Base {
	///	public abstract void Write (string text);
	/// }
	/// 
	/// public class SubType : Base {
	///	public override void Write (string output)
	///	{
	///		//...
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Base {
	///	public abstract void Write (string text);
	/// }
	/// 
	/// class SubType : Base {
	///	public override void Write (string text)
	///	{
	///		//...
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method overrides (or implements) an existing method but does not use the same parameter names as the original.")]
	[Solution ("Keep parameter names consistent when overriding a class or implementing an interface.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
	public class ParameterNamesShouldMatchOverriddenMethodRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			//check if this is a Boo assembly using macros
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				IsBooAssemblyUsingMacro = (e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
					return tr.IsNamed ("Boo.Lang.Compiler.Ast", "MacroStatement");
				}));
			};
		}

		private static bool SignatureMatches (MethodReference method, MethodReference baseMethod, bool explicitInterfaceCheck)
		{
			string name = method.Name;
			string base_name = baseMethod.Name;

			if (name != base_name) {
				if (!explicitInterfaceCheck)
					return false;

				TypeReference btype = baseMethod.DeclaringType;
				string bnspace = btype.Namespace;
				if (!name.StartsWith (bnspace, StringComparison.Ordinal))
					return false;
				if (name [bnspace.Length] != '.')
					return false;

				string bname = btype.Name;
				if (String.CompareOrdinal (bname, 0, name, bnspace.Length + 1, bname.Length) != 0)
					return false;

				int dot = bnspace.Length + bname.Length + 1;
				if (name [dot] != '.')
					return false;

				if (name.LastIndexOf (base_name, StringComparison.Ordinal) != dot + 1)
					return false;
			}
			return method.CompareSignature (baseMethod);
		}

		private static MethodDefinition GetBaseMethod (MethodDefinition method)
		{
			TypeDefinition baseType = method.DeclaringType.Resolve ();
			if (baseType == null)
				return null;

			while ((baseType.BaseType != null) && (baseType != baseType.BaseType)) {
				baseType = baseType.BaseType.Resolve ();
				if ((baseType == null) || !baseType.HasMethods)
					return null;		// could not resolve

				foreach (MethodDefinition baseMethodCandidate in baseType.Methods) {
					if (SignatureMatches (method, baseMethodCandidate, false))
						return baseMethodCandidate;
				}
			}
			return null;
		}

		private static MethodDefinition GetInterfaceMethod (MethodDefinition method)
		{
			TypeDefinition type = (method.DeclaringType as TypeDefinition);
			if (!type.HasInterfaces)
				return null;

			foreach (TypeReference interfaceReference in type.Interfaces) {
				TypeDefinition interfaceCandidate = interfaceReference.Resolve ();
				if ((interfaceCandidate == null) || !interfaceCandidate.HasMethods)
					continue;

				foreach (MethodDefinition interfaceMethodCandidate in interfaceCandidate.Methods) {
					if (SignatureMatches (method, interfaceMethodCandidate, true))
						return interfaceMethodCandidate;
				}
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsVirtual || !method.HasParameters || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition baseMethod = null;
			if (!method.IsNewSlot)
				baseMethod = GetBaseMethod (method);
			if (baseMethod == null)
				baseMethod = GetInterfaceMethod (method);
			if (baseMethod == null)
				return RuleResult.Success;

			IList<ParameterDefinition> base_pdc = baseMethod.Parameters;
			//do not trigger false positives on Boo macros
			if (IsBooAssemblyUsingMacro && IsBooMacroParameter (base_pdc [0]))
				return RuleResult.Success;

			IList<ParameterDefinition> pdc = method.Parameters;
			for (int i = 0; i < pdc.Count; i++) {
				if (pdc [i].Name != base_pdc [i].Name) {
					string s = String.Format (CultureInfo.InvariantCulture,
						"The name of parameter #{0} ({1}) does not match the name of the parameter in the overriden method ({2}).", 
						i + 1, pdc [i].Name, base_pdc [i].Name);
					Runner.Report (method, Severity.Medium, Confidence.High, s);
				}
			}
			return Runner.CurrentRuleResult;
		}

		private bool IsBooAssemblyUsingMacro { get; set; }

		private static bool IsBooMacroParameter (ParameterReference p)
		{
			return p.Name == "macro" && p.ParameterType.IsNamed ("Boo.Lang.Compiler.Ast", "MacroStatement");
		}
	}
}
