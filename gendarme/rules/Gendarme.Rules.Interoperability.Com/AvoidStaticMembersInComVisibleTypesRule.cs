// 
// Gendarme.Rules.Interoperability.Com.AvoidStaticMembersInComVisibleTypesRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// COM visible types should not contain static methods because they are
	/// not supported by COM
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public class Bad {
	///		public static void BadMethod ()
	///		{
	///			// do something
	///		}
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
	///	public class Good {
	///		[ComVisiblte (false)]
	///		public static void GoodMethod ()
	///		{
	///			// do something
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// This rule ignores methods marked with ComRegisterFunctionAttribute or ComUnregisterFunctionAttribute attributes. 
	/// Rule applies only when the containing assembly has ComVisible attribute explicitly set to false 
	/// and the type has ComVisible attribute explicitly set to true.</remarks>
	[Problem ("Static method is visible to COM, while COM does not support statics.")]
	[Solution ("Add [ComVisible (false)] attribute to the method, or make static method an instance method.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1407:AvoidStaticMembersInComVisibleTypes")]
	public class AvoidStaticMembersInComVisibleTypesRule : Rule, IMethodRule {
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!IsApplicableMethod (method))
				return RuleResult.DoesNotApply;

			// check if assembly has [ComVisible (false)]
			// and type has [ComVisible (true)]
			if (!method.DeclaringType.IsTypeComVisible ())
				return RuleResult.DoesNotApply;
			
			bool comVisibleValue = true;
			
			if (method.HasCustomAttributes) {
				foreach (CustomAttribute attribute in method.CustomAttributes) {
					TypeReference type = attribute.AttributeType;
					if (type.Namespace != "System.Runtime.InteropServices")
						continue;

					switch (type.Name) {
					case "ComUnregisterFunctionAttribute":
					case "ComRegisterFunctionAttribute":
						return RuleResult.DoesNotApply;
					case "ComVisibleAttribute":
						comVisibleValue = (bool)attribute.ConstructorArguments [0].Value;
						break;
					}
				}
			}

			if (comVisibleValue)
				Runner.Report (method, Severity.Medium, Confidence.High);

			return Runner.CurrentRuleResult;
		}
		
		private static bool IsApplicableMethod (MethodDefinition method)
		{
			return !(!method.IsStatic || !method.IsPublic || method.HasGenericParameters || 
				method.IsAddOn || method.IsRemoveOn || method.IsGetter || method.IsSetter ||
				((method.Attributes & MethodAttributes.SpecialName) != 0 && method.Name.StartsWith ("op_", StringComparison.Ordinal)) ||
				method.DeclaringType.HasGenericParameters || method.DeclaringType.IsEnum ||
				method.DeclaringType.IsInterface);
		}
	}
}
