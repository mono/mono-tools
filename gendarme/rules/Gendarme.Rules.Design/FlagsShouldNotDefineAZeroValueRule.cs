// 
// Gendarme.Rules.Design.FlagsShouldNotDefineAZeroValueRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule ensures that enumerations decorated with the [System.Flags]  
	/// attribute do not contain a 0 value. This value would not be usable  
	/// with bitwise operators.
	/// </summary>
	/// <example>
	/// Bad example (using 0 for a normal value):
	/// <code>
	/// [Flags]
	/// [Serializable]
	/// enum Access {
	/// 	Read = 0,
	/// 	Write = 1
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (using None):
	/// <code>
	/// [Flags]
	/// [Serializable]
	/// enum Access {
	///	// this is less severe since the name of the 0 value helps
	/// 	None = 0,
	/// 	Read = 1,
	/// 	Write = 2
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Flags]
	/// [Serializable]
	/// enum Access {
	///	Read = 1,
	///	Write = 2
	/// }
	/// </code>
	/// </example>

	[Problem ("This enumeration flag defines a value of 0, which cannot be used in boolean operations.")]
	[Solution ("Remove the 0 value(s) from the flag.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	public class FlagsShouldNotDefineAZeroValueRule : DefineAZeroValueRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on [Flags] (this takes care of checking for enums)
			if (!type.IsFlags ())
				return RuleResult.DoesNotApply;

			// rule applies!

			FieldDefinition field = GetZeroValueField (type);
			if (field == null)
				return RuleResult.Success;

			// it's less likely an error if the field is named "None"
			Severity s = field.Name == "None" ? Severity.Medium : Severity.High;
			Runner.Report (field, s, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
