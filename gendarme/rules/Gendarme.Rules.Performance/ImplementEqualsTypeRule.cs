//
// Gendarme.Rules.Performance.ImplementEqualsTypeRule
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

	[Problem ("Since this type overrides Equals(object) it is also a good candidate to provide a Equals method for it's own type.")]
	[Solution ("Implement the suggested method to avoid casting and, for ValueType, boxing penalities for calling Equals.")]
	public class ImplementEqualsTypeRule : Rule, ITypeRule {

		private string [] parameters = new string [1];

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums and to generated code
			if (type.IsEnum || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// rule applies only if the type overrides Equals(object)
			if (!type.HasMethod (MethodSignatures.Equals))
				return RuleResult.DoesNotApply;

			// if so then the type should also implement Equals(type) since this avoid a cast 
			// operation (for reference types) and also boxing (for value types).
			parameters [0] = type.FullName;
			if (type.GetMethod (MethodAttributes.Public, "Equals", "System.Boolean", parameters) != null)
				return RuleResult.Success;

			// we consider this a step more severe for value types since it will need 
			// boxing/unboxing with Equals(object)
			Severity severity = type.IsValueType ? Severity.Medium : Severity.Low;
			Runner.Report (type, severity, Confidence.High, String.Empty);
			return RuleResult.Failure;
		}
	}
}
