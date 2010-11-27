//
// Gendarme.Rules.Design.TypesWithDisposableFieldsShouldBeDisposableRule
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
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule will fire if a type contains disposable fields but does not implement
	/// <c>System.IDisposable</c>.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// class DoesNotImplementIDisposable {
	///	IDisposable field;
	/// }
	/// 
	/// class AbstractDispose : IDisposable {
	///	IDisposable field;
	///	
	///	// the field should be disposed in the type that declares it
	///	public abstract void Dispose ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Dispose : IDisposable {
	///	IDisposable field;
	///	
	///	public void Dispose ()
	///	{
	///		field.Dispose ();
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains disposable field(s) but doesn't implement IDisposable.")]
	[Solution ("Implement IDisposable and free the disposable field(s) in the Dispose method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class TypesWithDisposableFieldsShouldBeDisposableRule : Rule, ITypeRule {

		private const string AbstractTypeMessage = "Field implement IDisposable. Type should implement a non-abstract Dispose() method";
		private const string TypeMessage = "Field implement IDisposable. Type should implement a Dispose() method";
		private const string AbstractDisposeMessage = "Some field(s) implement IDisposable. Making this method abstract shifts the reponsability of disposing those fields to the inheritors of this class.";

		static bool IsAbstract (MethodDefinition method)
		{
			return ((method != null) && (method.IsAbstract));
		}

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

				if (IsAbstract (implicitDisposeMethod) || IsAbstract (explicitDisposeMethod)) {
					abstractWarning = true;
				} else {
					return RuleResult.Success;
				}
			}

			foreach (FieldDefinition field in type.Fields) {
				// we can't dispose static fields in IDisposable
				if (field.IsStatic)
					continue;
				TypeDefinition fieldType = field.FieldType.GetElementType ().Resolve ();
				if (fieldType == null)
					continue;
				// enums and primitives don't implement IDisposable
				if (fieldType.IsEnum || fieldType.IsPrimitive ())
					continue;
				if (fieldType.Implements ("System.IDisposable")) {
					Runner.Report (field, Severity.High, Confidence.High,
						abstractWarning ? AbstractTypeMessage : TypeMessage);
				}
			}

			// Warn about possible confusion if the Dispose methods are abstract
			if (IsAbstract (implicitDisposeMethod))
				Runner.Report (implicitDisposeMethod, Severity.Medium, Confidence.High, AbstractDisposeMessage);

			return Runner.CurrentRuleResult;
		}
	}
}

