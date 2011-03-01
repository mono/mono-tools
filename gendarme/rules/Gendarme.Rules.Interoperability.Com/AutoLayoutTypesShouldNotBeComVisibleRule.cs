//
// AutoLayoutTypesShouldNotBeComVisibleRule.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// This rule checks for <c>[System.Runtime.InteropServices.ComVisible]</c> decorated value 
	/// types which have <c>[System.Runtime.InteropServices.StructLayout]</c> attribute set to 
	/// <c>System.Runtime.InteropServices.LayoutKind</c>.<c>Auto</c> because auto layout can 
	/// change between Mono and .NET or even between releases of the .NET/Mono frameworks.
	/// Note that this does not affect <c>System.Enum</c>-based types.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	[StructLayout (LayoutKind.Auto)]
	///	public struct Good {
	///		ushort a;
	///		ushort b;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	[StructLayout (LayoutKind.Sequential)]
	///	public struct Good {
	///		ushort a;
	///		ushort b;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// Rule applies only when the containing assembly has ComVisible attribute explicitly set to false 
	/// and the type has ComVisible attribute explicitly set to true.
	/// </remarks>
	[Problem ("Value types with StructLayout attribute set to LayoutKind.Auto should not be COM visible")]
	[Solution ("Change StructLayout attribute for this type or make it invisible for COM")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1403:AutoLayoutTypesShouldNotBeComVisible")]
	public class AutoLayoutTypesShouldNotBeComVisibleRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || !type.IsValueType || !type.HasCustomAttributes || 
				(!type.IsPublic && !type.IsNestedPublic) || type.HasGenericParameters)
				return RuleResult.DoesNotApply;

			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;
				
			if (type.IsAutoLayout) 
				Runner.Report (type, Severity.High, Confidence.High);

			return Runner.CurrentRuleResult;	
		}
	}
}
