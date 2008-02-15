//
// Gendarme.Rules.Design.DisposableTypesShouldHaveFinalizerRule
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	[Problem ("This type contains native fields but does not have a finalizer.")]
	[Solution ("Add a finalizer, calling Dispose(true), to release unmanaged resources.")]
	public class DisposableTypesShouldHaveFinalizerRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule onyly applies to type that implements IDisposable
			if (!type.Implements ("System.IDisposable"))
				return RuleResult.DoesNotApply;

			// no problem is a finalizer is found
			if (type.HasMethod (MethodSignatures.Finalize))
				return RuleResult.Success;

			// otherwise check for native types
			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				if (!field.FieldType.GetOriginalType ().IsNative ())
					continue;
				Runner.Report (field, Severity.High, Confidence.High, String.Empty);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
