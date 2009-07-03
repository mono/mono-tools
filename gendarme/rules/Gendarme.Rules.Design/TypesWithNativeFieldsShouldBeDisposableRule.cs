//
// Gendarme.Rules.Design.TypesWithNativeFieldsShouldBeDisposableRule
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

	/// <summary>
	/// This rule will fire if a type contains <c>IntPtr</c>, <c>UIntPtr</c>, or 
	/// <c>HandleRef</c> fields but does not implement <c>System.IDisposable</c>.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// public class DoesNotImplementIDisposable {
	///	IntPtr field;
	/// }
	/// 
	/// abstract public class AbstractDispose : IDisposable {
	///	IntPtr field;
	///	
	///	// the field should be disposed in the type that declares it
	///	public abstract void Dispose ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Dispose : IDisposable {
	///	IDisposable field;
	///	
	///	public void Dispose ()
	///	{
	///		UnmanagedFree (field);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains native field(s) but doesn't implement IDisposable.")]
	[Solution ("Implement IDisposable and free the native field(s) in the Dispose method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
	public class TypesWithNativeFieldsShouldBeDisposableRule : Rule, ITypeRule {

		private const string AbstractTypeMessage = "Field is native. Type should implement a non-abstract Dispose() method";
		private const string TypeMessage = "Field is native. Type should implement a Dispose() method";
		private const string AbstractDisposeMessage = "Some fields are native pointers. Making this method abstract shifts the reponsability of disposing those fields to the inheritors of this class.";

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule doesn't apply to enums, interfaces, structs, delegates or generated code
			if (type.IsEnum || type.IsInterface || type.IsValueType || type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition explicitDisposeMethod = null;
			MethodDefinition implicitDisposeMethod = null;

			bool abstractWarning = false;

			if (type.Implements ("System.IDisposable")) {
				implicitDisposeMethod = type.GetMethod (MethodSignatures.Dispose);
				explicitDisposeMethod = type.GetMethod (MethodSignatures.DisposeExplicit);

				if (((implicitDisposeMethod != null) && implicitDisposeMethod.IsAbstract) ||
					((explicitDisposeMethod != null) && explicitDisposeMethod.IsAbstract)) {
					abstractWarning = true;
				} else {
					return RuleResult.Success;
				}
			}

			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				if (field.FieldType.GetOriginalType ().IsNative ()) {
					if (abstractWarning)
						Runner.Report (field, Severity.High, Confidence.High, AbstractTypeMessage);
					else
						Runner.Report (field, Severity.High, Confidence.High, TypeMessage);
				}
			}

			// Warn about possible confusion if the Dispose methods are abstract
			if (implicitDisposeMethod != null && implicitDisposeMethod.IsAbstract)
				Runner.Report (implicitDisposeMethod, Severity.Medium, Confidence.High, AbstractDisposeMessage);

			return Runner.CurrentRuleResult;
		}
	}
}
