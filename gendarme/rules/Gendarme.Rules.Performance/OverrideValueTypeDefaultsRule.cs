//
// Gendarme.Rules.Performance.OverrideValueTypeDefaultsRule
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Performance {

	[Problem ("This type does not override the default ValueType implementation of Equals(object) and GetHashCode() which lacks performance due to their generalized design.")]
	[Solution ("To avoid performance penalities of the default implementations you should override, or implement, the specified methods.")]
	public class OverrideValueTypeDefaultsRule : Rule, ITypeRule {

		private const string MissingImplementationMessage = "Missing type-specific implementation for '{0}'. {1}";
		private const string MissingOperatorsMessage = "If your langage supports overloading operators then you should implement the equality (==) and inequality (!=) operators.";

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to ValueType, except enums and generated code
			if (!type.IsValueType || type.IsEnum || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// note: no inheritance check required since we're dealing with structs

			bool equals = type.HasMethod (MethodSignatures.Equals);
			bool gethashcode = type.HasMethod (MethodSignatures.GetHashCode);
			bool operators = type.HasMethod (MethodSignatures.op_Equality) && 
				type.HasMethod (MethodSignatures.op_Inequality);

			// note: we want to avoid reporting 4 defects each (well most) of the time
			// the rule is triggered (it's too verbose to be useful)
			if (equals && gethashcode && operators)
				return RuleResult.Success;

			// drop severity one level if only operators are missing (since overloading them
			// is not available in every language)
			Severity severity = (equals || gethashcode) ? Severity.Medium : Severity.Low;
			string msg = String.Format (MissingImplementationMessage,
				!equals && !gethashcode ? "Equals(object)' and 'GetHashCode()" : equals ? "GetHashCode()" : "Equals(object)",
				operators ? String.Empty : MissingOperatorsMessage);

			Runner.Report (type, severity, Confidence.High, msg);
			return RuleResult.Failure;
		}
	}
}
