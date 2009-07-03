// 
// Gendarme.Rules.Design.EnumsShouldDefineAZeroValueRule
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
	/// This rule ensures that every non-flags enumeration contains a <c>0</c> 
	/// value. This is important because if a field is not explicitly initialized .NET
	/// will zero-initialize it and, if the enum has no zero value, then it will be
	/// initialized to an invalid value.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// enum Position {
	///	First = 1,
	///	Second
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// enum Position {
	///	First,
	///	Second
	/// }
	/// </code>
	/// </example>

	[Problem ("This enumeration does not provide a member with a value of 0.")]
	[Solution ("Add a new member in the enum with a value of 0.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	public class EnumsShouldDefineAZeroValueRule : DefineAZeroValueRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on enums
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			// rule doesn't apply on [Flags]
			if (type.IsFlags ())
				return RuleResult.DoesNotApply;

			// rule applies!

			FieldDefinition field = GetZeroValueField (type);
			if (field != null)
				return RuleResult.Success;

			Runner.Report (type, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
