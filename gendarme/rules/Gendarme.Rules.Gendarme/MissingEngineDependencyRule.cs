// 
// Gendarme.Rules.Gendarme.MissingEngineDependencyRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Gendarme {

	/// <summary>
	/// Rules should not use engines' features without subscribing to them
	/// using EngineDependency attribute because it will not work unless
	/// another rule has subscribed to the same engine.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class BadRule : Rule, IMethodRule {
	///	public RuleResult CheckMethod (MethodDefinition method) 
	///	{
	///		if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
	///			return RuleResult.DoesNotApply;
	///		// rule code
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [EngineDependency (typeof (OpCodeEngine))]
	/// class BadRule : Rule, IMethodRule {
	///	public RuleResult CheckMethod (MethodDefinition method) 
	///	{
	///		if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
	///			return RuleResult.DoesNotApply;
	///		// rule code
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("Rules uses engines' features without subscribing to it, thus these features will not work correctly.")]
	[Solution ("Add EngineDependency attribute to the rule.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class MissingEngineDependencyRule : GendarmeRule, ITypeRule {

		HashSet<string> engines = new HashSet<string> {
			"Gendarme.Framework.Engines.OpCodeEngine",
			"Gendarme.Framework.Engines.NamespaceEngine"
		};

		private HashSet<string> declaredEngines = new HashSet<string> ();

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasMethods)
				return RuleResult.DoesNotApply;

			GetEngineDependencyValue (type);

			foreach (MethodDefinition method in type.Methods) {
				if (!method.HasBody || !OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
					continue;

				foreach (Instruction instruction in method.Body.Instructions) {
					MethodReference m = (instruction.Operand as MethodReference);
					if (m == null)
						continue;

					TypeReference dtype = m.DeclaringType;
					// short-cut to avoid FullName - will work as long as all Engines comes from the same namespace (otherwise remove it)
					if (dtype.Namespace != "Gendarme.Framework.Engines")
						continue;

					string declaringType = dtype.GetFullName ();
					if (!engines.Contains (declaringType) || declaredEngines.Contains (declaringType))
						continue;
					Runner.Report (method, instruction, Severity.High, Confidence.High, 
						"An engine " + declaringType + " is being used without type being subscribed to it with EngineDependency attribute.");
				}
				
			}

			return Runner.CurrentRuleResult;
		}

		private void GetEngineDependencyValue (TypeDefinition type)
		{
			declaredEngines.Clear ();
			TypeDefinition td = type;
			while (declaredEngines.Count < engines.Count) {
				if (td.HasCustomAttributes)
					foreach (CustomAttribute attribute in td.CustomAttributes) {
						if (!attribute.HasConstructorArguments ||
							!attribute.AttributeType.IsNamed ("Gendarme.Framework", "EngineDependencyAttribute"))
							continue;

						object value = attribute.ConstructorArguments [0].Value;
						MemberReference mr = (value as MemberReference);
						declaredEngines.Add (mr == null ? value.ToString () : mr.GetFullName ());
					}
				if (td.BaseType == null)
					break;
				TypeDefinition baseType = td.BaseType.Resolve ();
				if (baseType == null)
					break;
				td = baseType;
			}
		}
	}
}
