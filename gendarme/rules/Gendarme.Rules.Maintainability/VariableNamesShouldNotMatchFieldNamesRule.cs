//
// Gendarme.Rules.Maintainability.VariableNamesShouldNotMatchFieldNamesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
// 
// Copyright (C) 2010 N Lum
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks for local variables or parameters whose names match (case sensitive) an instance field name.
	/// Note that variable names can only be verified when debugging symbols (pdb or mdb) are available.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	///	public class Bad {
	///		public int value;
	///
	///		public void DoSomething (int value)
	///		{
	///			// without 'this.' the field will never be set
	///			this.value = value;
	///		}
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	///	public class Good {
	///		public int value;
	///
	///		public void DoSomething (int integralValue)
	///		{
	///			value = integralValue;
	///		}
	///	}
	/// </code>
	/// </example>

	[Problem ("An instance method declares a parameter or a local variable whose name matches an instance field of the declaring type.")]
	[Solution ("Rename the variable/parameter or the field.")]
	[FxCopCompatibility ("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames")]
	public class VariableNamesShouldNotMatchFieldNamesRule : Rule, ITypeRule {

		// Storing all field names in a hashset provides quicker .Contains(), and saves time
		// in the long run.
		HashSet<string> fields;

		public VariableNamesShouldNotMatchFieldNamesRule ()
		{
			fields = new HashSet<string> ();
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// We only like types with fields AND methods.
			if (!type.HasFields || !type.HasMethods || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			fields.Clear ();
			foreach (FieldDefinition field in type.Fields)
				fields.Add (field.Name);

			// Iterate through all the methods. Check parameter names then method bodies.
			foreach (MethodDefinition method in type.Methods) {
				if (method.HasParameters) {
					foreach (ParameterDefinition param in method.Parameters) {
						if (fields.Contains (param.Name))
							Runner.Report (param, Severity.Medium, Confidence.Total);
					}
				}

				// Method bodies w/o variables don't interest me.
				if (!method.HasBody)
					continue;

				MethodBody body = method.Body;
				if (body.HasVariables) {
					// Iterate through all variables in the method body.
					foreach (VariableDefinition var in body.Variables) {
						// if the name is compiler generated or if we do not have debugging symbols...
						if (var.IsGeneratedName ())
							continue;
						if (fields.Contains (var.Name))
							Runner.Report (method, Severity.Medium, Confidence.Normal, var.Name);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}

	}
}
