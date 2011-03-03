//
// Gendarme.Rules.Design.ImplementEqualsAndGetHashCodeInPairRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for types that either override the <c>Equals(object)</c> method 
	/// without overriding <c>GetHashCode()</c> or override <c>GetHashCode</c> without
	/// overriding <c>Equals</c>. In order to work correctly types should always override
	/// these together.
	/// </summary>
	/// <example>
	/// Bad example (missing GetHashCode):
	/// <code>
	/// public class MissingGetHashCode {
	/// 	public override bool Equals (object obj)
	/// 	{
	/// 		return this == obj;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (missing Equals):
	/// <code>
	/// public class MissingEquals {
	/// 	public override int GetHashCode ()
	/// 	{
	/// 		return 42;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	/// 	public override bool Equals (object obj)
	/// 	{
	/// 		return this == obj;
	/// 	}
	/// 	
	/// 	public override int GetHashCode ()
	/// 	{
	/// 		return 42;
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type only implements one of Equals(Object) and GetHashCode().")]
	[Solution ("Implement the other method.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals")]
	public class ImplementEqualsAndGetHashCodeInPairRule : Rule, ITypeRule {

		private const string Message = "Type implements '{0}' but is missing '{1}'.";

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule doesn't apply to interfaces and enums
			if (type.IsInterface || type.IsEnum || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			bool equals = type.HasMethod (MethodSignatures.Equals);
			bool getHashCode = type.HasMethod (MethodSignatures.GetHashCode);

			// if we have Equals but no GetHashCode method
			if (equals && !getHashCode) {
				string text = String.Format (CultureInfo.InvariantCulture, Message, MethodSignatures.Equals, MethodSignatures.GetHashCode);
				Runner.Report (type, Severity.Critical, Confidence.High, text);
			}

			// if we have GetHashCode but no Equals method
			if (!equals && getHashCode) {
				string text = String.Format (CultureInfo.InvariantCulture, Message, MethodSignatures.GetHashCode, MethodSignatures.Equals);
				Runner.Report (type, Severity.Medium, Confidence.High, text);
			}

			// we either have both Equals and GetHashCode or none (both fine)
			return Runner.CurrentRuleResult;
		}
	}
}
