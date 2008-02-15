// 
// Gendarme.Rules.Design.EnumsShouldUseInt32Rule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	[Problem ("Unless required for interoperability this enumeration should use Int32 as its underling storage type.")]
	[Solution ("Remove the extra type from the enumeration declaration (Int32 will be used as default).")]
	public class EnumsShouldUseInt32Rule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on enums
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			// rule applies!

			string value_type = null;

			foreach (FieldDefinition field in type.Fields) {
				// we looking for the special value__
				if (field.IsStatic)
					continue;

				value_type = field.FieldType.FullName;
				break;
			}

			Severity severity = Severity.Critical;
			switch (value_type) {
			case "System.Int32":
				return RuleResult.Success;
			// some are bad choice (when possible) but usable by all CLS compliant languages
			case "System.Byte":
			case "System.Int16":
			case "System.Int64":
				severity = Severity.High;
				break;
			// while others are not usable in non-CLS compliant languages
			case "System.SByte":
			case "System.UInt16":
			case "System.UInt32":
			case "System.UInt64":
				severity = Severity.Critical;
				break;
			default:
				throw new NotSupportedException (value_type + " unexpected as a Enum value type");
			}

			string text = String.Format ("Enums should use System.Int32 instead of '{0}'.", value_type);
			Runner.Report (type, severity, Confidence.Total, text);
			return RuleResult.Failure;
		}
	}
}
