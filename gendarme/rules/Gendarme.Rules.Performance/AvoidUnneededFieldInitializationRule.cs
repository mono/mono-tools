//
// Gendarme.Rules.Performance.AvoidUnneededFieldInitializationRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for constructors that assign fields to their default value
	/// (e.g. 0 for an integer, null for an object or a string). Since the CLR zero initializes
	/// all values there is no need, under most circumstances, to assign default values.
	/// Doing so only adds size to source code and in IL.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	int i;
	///	string s;
	///	
	///	public Bad ()
	///	{
	///		i = 0;
	///		s = null;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	int i;
	///	string s;
	///	
	///	public Good ()
	///	{
	///		// don't assign 'i' since it's already 0
	///		// but we might prefer to assign a string to String.Empty
	///		s = String.Empty;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This constructor needlessly initializes zero initializes some fields.")]
	[Solution ("Remove the unneeded initialization from the constructors.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
	public class AvoidUnneededFieldInitializationRule : Rule, IMethodRule {

		// note: it's tempting to use IType rule here, since it would avoid iterating
		// all non-constructors methods. However the reporting would be less precise
		// since we want to report which source line inside a ctor is problematic

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsConstructor || !method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			TypeReference type = method.DeclaringType;

			foreach (Instruction ins in method.Body.Instructions) {
				// check for assignation on instance or static fields
				Code code = ins.OpCode.Code;
				bool is_static = (code == Code.Stsfld);
				bool is_instance = (code == Code.Stfld);
				if (!is_static && !is_instance)
					continue;

				// special case: a struct ctor MUST assign every instance fields
				if (type.IsValueType && is_instance)
					continue;

				// make sure we assign to this type (and not another one)
				FieldReference fr = (ins.Operand as FieldReference);
				if (fr.DeclaringType != type)
					continue;

				bool unneeded = false;
				if (fr.FieldType.IsValueType) {
					unneeded = ins.Previous.IsOperandZero ();
				} else {
					unneeded = ins.Previous.OpCode.Code == Code.Ldnull;
				}

				if (unneeded) {
					// we're more confident about the unneeded initialization
					// on static ctor, since another (previous) ctor, can't set
					// the values differently
					Confidence c = method.IsStatic ? Confidence.High : Confidence.Normal;
					Runner.Report (method, ins, Severity.Medium, c, fr.Name);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
