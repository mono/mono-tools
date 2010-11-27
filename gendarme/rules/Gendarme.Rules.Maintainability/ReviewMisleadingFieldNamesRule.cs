﻿//
// Gendarme.Rules.Maintainability.ReviewMisleadingFieldNamesRule
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


using Mono.Cecil;

using Gendarme.Framework;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks for fields which have misleading names, e.g. instance fields beginning with "s_"
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	///	public class Bad {
	///		public int s_Value;
	///		public static int m_OtherValue;
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	///	public class Good {
	///		public int m_Value;
	///		public static int s_OtherValue;
	///	}
	/// </code>
	/// </example>

	[Problem ("Fields have misleading names, e.g. instance fields beginning with \"s_\" or static fields beginning with \"m_\"")]
	[Solution ("Rename the fields so that their names follow convention.")]
	[FxCopCompatibility ("Microsoft.Maintainability", "CA1504:ReviewMisleadingFieldNames")]
	public class ReviewMisleadingFieldNamesRule : Rule, ITypeRule {

		public RuleResult CheckType(TypeDefinition type)
		{
			// We need fields to test.
			if (!type.HasFields)
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsStatic && field.Name.StartsWith ("s_"))
					Runner.Report (field, Severity.Low, Confidence.Total);
				else if (field.IsStatic && field.Name.StartsWith ("m_"))
					Runner.Report (field, Severity.Low, Confidence.Total);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
