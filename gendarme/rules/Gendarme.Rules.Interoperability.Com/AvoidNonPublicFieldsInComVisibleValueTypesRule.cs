//
// Gendarme.Rules.Interoperability.Com.AvoidNonPublicFieldsInComVisibleValueTypesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
// 
// Copyright (C) 2010 N Lum
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

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// This rule checks for ComVisible value types which contain fields that are non-public.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public struct BadStruct {
	///		internal int SomeValue;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public struct BadStruct {
	///		public int SomeValue;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("ComVisible value types which contain non-public fields will have such fields exposed to COM clients.")]
	[Solution ("Review fields for sensitive information, and either make the type a reference type, or remove the ComVisibleAttribute from the type")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1413:AvoidNonpublicFieldsInComVisibleValueTypes")]
	public class AvoidNonPublicFieldsInComVisibleValueTypesRule : Rule, ITypeRule {

		private const string ComVisible = "System.Runtime.InteropServices.ComVisibleAttribute";

		public RuleResult CheckType (TypeDefinition type)
		{
			// Only check for value types and types with fields.
			// But also for types with attributes, since FXCop only issues a warning if ComVisible is explicitly defined.
			if (!type.IsValueType || !type.HasCustomAttributes || !type.HasFields)
				return RuleResult.DoesNotApply;

			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			// If we find any, low severity as the code works, but it's bad practice.
			foreach (FieldDefinition field in type.Fields)
				if (!field.IsPublic)
					Runner.Report (field, Severity.Low, Confidence.Total);

			return Runner.CurrentRuleResult;
		}
	}
}
