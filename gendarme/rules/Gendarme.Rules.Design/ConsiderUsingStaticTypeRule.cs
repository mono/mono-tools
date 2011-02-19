//
// Gendarme.Rules.Design.ConsiderUsingStaticTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for types that contain only static members and, if the assembly
	/// targets the CLR version 2.0 or later, suggests that the type be made <c>static</c>
	/// or, for earlier versions, that the type be made <c>sealed</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Class {
	///	public static void Method ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (targetting CLR 2.0 and later):
	/// <code>
	/// public static class Class {
	///	public static void Method ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (targetting CLR 1.x):
	/// <code>
	/// public sealed class Class {
	///	public static void Method ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains only static fields and methods and should be static.")]
	[Solution ("Change this type into a static (or sealed for 1.x) type gain clarity and better error reporting.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
	public class ConsiderUsingStaticTypeRule : Rule, ITypeRule {

		static bool IsAllStatic (TypeDefinition type)
		{
			if (type.HasMethods) {
				foreach (MethodDefinition method in type.Methods) {
					if (method.IsConstructor) {
						// let's the default ctor pass (since it's always here for 1.x code)
						if (!method.IsStatic && method.HasParameters)
							return false;
					} else {
						if (!method.IsStatic)
							return false;
					}
				}
			}
			if (type.HasFields) {
				foreach (FieldDefinition field in type.Fields) {
					if (!field.IsStatic)
						return false;
				}
			}
			return true;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only if the type isn't: an enum, an interface, a struct, a delegate or compiler generated
			if (type.IsEnum || type.IsInterface || type.IsValueType || type.IsDelegate () || type.IsGeneratedCode () 
				|| type.BaseType != null && !type.BaseType.IsNamed ("System", "Object"))
				return RuleResult.DoesNotApply;
			
			// success if the type is already static or, before 2.0, is it's sealed
			if ((type.Module.Runtime >= TargetRuntime.Net_2_0) ? type.IsStatic () : type.IsSealed)
				return RuleResult.Success;
			
			if (IsAllStatic (type)) {
				// no technical reason not to use a static type exists
				Runner.Report (type, Severity.Medium, Confidence.High);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
