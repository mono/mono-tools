//
// Gendarme.Rules.Design.AvoidPublicInstanceFieldsRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
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

	[Problem ("This type contains public instance fields.")]
	[Solution ("If possible change the public fields into properties.")]
	public class AvoidPublicInstanceFieldsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule doesn't apply on enums or to compiler/tools-generated code
			// e.g. CSC compiles anonymous methods as an inner type that expose public fields
			if (type.IsEnum || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition fd in type.Fields) {
				if (!fd.IsPublic || fd.IsSpecialName || fd.IsStatic || fd.HasConstant || fd.IsInitOnly)
					continue;

				if (fd.FieldType.IsArray ()) {
					string s = String.Format ("Consider converting the public instance field '{0}' into a private field and add a 'Set{1}' method.", 
						fd.Name, Char.ToUpper (fd.Name [0]) + fd.Name.Substring (1));
					Runner.Report (fd, Severity.Medium, Confidence.Total, s);
				} else {
					string s = String.Format ("Public instance field '{0}' should probably be a property.", fd.Name);
					Runner.Report (fd, Severity.Medium, Confidence.Total, s);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
