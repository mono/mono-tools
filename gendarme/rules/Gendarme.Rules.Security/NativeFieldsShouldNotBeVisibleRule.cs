//
// Gendarme.Rules.Security.NativeFieldsShouldNotBeVisibleRule
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

	/// <summary>
	/// This rule checks if a class exposes native fields. Native fields should not
	/// be public because you lose control over their lifetime (other code could free
	/// the memory or use it after it has been freed).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class HasPublicNativeField {
	///	public IntPtr NativeField;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (hide):
	/// <code>
	/// class HasPrivateNativeField {
	///	private IntPtr NativeField;
	///	public void DoSomethingWithNativeField ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (read-only):
	/// <code>
	/// class HasReadOnlyNativeField {
	///	public readonly IntPtr NativeField;
	/// }
	/// </code>
	/// </example>

	[Problem ("This type exposes native fields that aren't read-only.")]
	[Solution ("Native fields are best hidden or, if required to be exposed, read-only.")]
	[FxCopCompatibility ("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
	public class NativeFieldsShouldNotBeVisibleRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interface, enumerations and delegates or to types without fields
			if (type.IsInterface || type.IsEnum || !type.HasFields || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsVisible ())
					continue;

				//not readonly native fields or arrays of native fields
				if ((field.FieldType.IsNative () && !field.IsInitOnly) || 
					(field.FieldType.IsArray && field.FieldType.GetElementType ().IsNative ())) {

					Runner.Report (field, Severity.Medium, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
