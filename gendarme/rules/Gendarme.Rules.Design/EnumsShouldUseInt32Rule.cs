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
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	// TODO: It would be really nice to explain why this is a problem. Presumably it is for
	// compatibility with other .NET languages which may not support non-int enums...
	// It might also be a good idea if it only fired for publicly visible enums.

	/// <summary>
	/// Enumaration types should avoid specifying a non-default storage type for their values 
	/// unless it is required for interoperability (e.g. with native code). If you do use a non-default
	/// type for the enum, and the enum is externally visible, then prefer the CLS-compliant
	/// integral types: System.Byte, System.Int16, System.Int32, and System.Int64.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// public enum SmallEnum : byte {
	///	Zero,
	///	One
	/// }
	/// 
	/// [Flags]
	/// public enum SmallFlag : ushort {
	///	One = 1,
	///	// ...
	///	Sixteen = 1 &lt;&lt; 15
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public enum SmallEnum {
	///	Zero,
	///	One
	/// }
	/// 
	/// [Flags]
	/// public enum SmallFlag {
	///	One = 1,
	///	// ...
	///	Sixteen = 1 &lt;&lt; 15
	/// }
	/// </code>
	/// </example>

	[Problem ("Unless required for interoperability this enumeration should use Int32 as its underling storage type.")]
	[Solution ("Remove the base type from the enumeration declaration (it will then default to Int32).")]
	[FxCopCompatibility ("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
	public class EnumsShouldUseInt32Rule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on enums
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			// rule applies!

			TypeReference ftype = null;

			foreach (FieldDefinition field in type.Fields) {
				// we looking for the special value__
				if (!field.IsStatic) {
					ftype = field.FieldType;
					break;
				}
			}

			Severity severity = Severity.Critical;
			if ((ftype != null) && (ftype.Namespace == "System")) {
				switch (ftype.Name) {
				case "Int32":
					return RuleResult.Success;
				// some are bad choice (when possible) but usable by all CLS compliant languages
				case "Byte":
				case "Int16":
				case "Int64":
				severity = Severity.High;
					break;
				// while others are not usable in non-CLS compliant languages
				default: // System.SByte, System.UInt16, System.UInt32, System.UInt64
					severity = Severity.Critical;
					break;
				}
			}

			string text = String.Format (CultureInfo.InvariantCulture, 
				"Enums should use System.Int32 instead of '{0}'.", ftype.GetFullName ());
			Runner.Report (type, severity, Confidence.Total, text);
			return RuleResult.Failure;
		}
	}
}
