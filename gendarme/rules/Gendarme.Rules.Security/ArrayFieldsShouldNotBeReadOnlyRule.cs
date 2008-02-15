//
// Gendarme.Rules.Security.ArrayFieldsShouldNotBeReadOnlyRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

namespace Gendarme.Rules.Security {

	[Problem ("This type contains read-only array(s), however values inside the array(s) are not read-only.")]
	[Solution ("Replace the array with a method returning a clone of the array or a read-only collection.")]
	public class ArrayFieldsShouldNotBeReadOnlyRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interface and enumerations ??? struct ???
			if (type.IsInterface || type.IsEnum)
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				//IsInitOnly == readonly
				if (field.IsInitOnly && field.IsVisible () && field.FieldType.IsArray ()) {
					// Medium = this will work as long as no code starts "playing" with the array values
					Runner.Report (field, Severity.Medium, Confidence.Total, String.Empty);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
