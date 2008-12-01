// 
// Gendarme.Rules.BadPractice.AvoidVisibleConstantFieldRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule looks for constant fields that are visible outside the current assembly.
	/// Such fields, if used outside the assemblies, will have their value (not the field
	/// reference) copied into the other assembly. Changing the value will requires to 
	/// recompile all assemblies in order to have the desired effect. Declaring the field
	/// as <c>static readonly</c> will have the desired effect (i.e. insert a reference to
	/// the field in its original assembly).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // if this fields is used inside another assembly then
	/// // the integer 42, not the field, will be part of it
	/// public const int MagicNumber = 42;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// // if this fields is used inside another assembly then
	/// // the MagicNumber will be referenced
	/// static public readonly int MagicNumber = 42;
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This type contains visible field constants where the value will be embedded into the assemblies that use it.")]
	[Solution ("Use a 'static readonly' field (C# syntax) to make sure a reference to the field itself is kept and avoid recompiling all assemblies.")]
	public class AvoidVisibleConstantFieldRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// if the type is not visible or has no fields then the rule does not apply
			if (!type.HasFields || type.IsEnum || !type.IsVisible ())
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				// look for 'const' fields
				if (!field.IsLiteral)
					continue;

				// that are visible outside the current assembly
				if (!field.IsVisible ())
					continue;

				// we let null constant for all reference types (since they can't be changed to anything else)
				// except for strings (which can be modified later)
				string type_name = field.FieldType.FullName;
				if (!field.FieldType.IsValueType && (type_name != "System.String"))
					continue;

				string msg = string.Format ("'{0}' of type {1}.", field.Name, type_name);
				Runner.Report (field, Severity.High, Confidence.High, msg);

			}
			return Runner.CurrentRuleResult;
		}
	}
}
