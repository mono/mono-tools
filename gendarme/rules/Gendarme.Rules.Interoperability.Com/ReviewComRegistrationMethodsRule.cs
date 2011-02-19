// 
// Gendarme.Rules.Interoperability.Com.ReviewComRegistrationMethodsRule
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
	/// This rule checks the correctness of COM register and unregister methods,
	/// i.e. they should not be externally visible and they should be matched 
	/// (both or none of them should exist ).
	/// </summary>
	/// <example>
	/// Bad example (public methods):
	/// <code>
	/// [ComVisible (true)
	/// class Bad {
	///	[ComRegisterFunction]
	///	public void Register ()
	///	{
	///	}
	///	
	///	[ComUnregisterFunction]
	///	public void Unregister ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (only one of the methods exist)
	/// <code>
	/// [ComVisible (true)]
	/// class Bad {
	///	[ComRegisterFunction]
	///	public void Register ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [ComVisible (true)]
	/// class Good {
	///	[ComRegisterFunction]
	///	private void Register ()
	///	{
	///	}
	///	
	///	[ComUnregisterFunction]
	///	private void Unregister ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("COM registration methods should be matched (i.e. both or none of them should exist) and should not be externally visible.")]
	[Solution ("Add a missing method or change methods visibility to private or internal.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1410:ComRegistrationMethodsShouldBeMatched")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1411:ComRegistrationMethodsShouldNotBeVisible")]
	public class ReviewComRegistrationMethodsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.HasGenericParameters || !type.IsVisible () || !type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			bool foundRegister = false; // type level variables
			bool foundUnregister = false;

			foreach (MethodDefinition method in type.Methods) {
				if (!method.HasCustomAttributes)
					continue;

				bool foundRegisterUnregisterMethod = false; // method level variable
				foreach (CustomAttribute attribute in method.CustomAttributes) {
					TypeReference atype = attribute.AttributeType;
					if (!foundRegister && atype.IsNamed ("System.Runtime.InteropServices", "ComRegisterFunctionAttribute")) {
						foundRegister = true;
						foundRegisterUnregisterMethod = true;
					}
					if (!foundUnregister && atype.IsNamed ("System.Runtime.InteropServices", "ComUnregisterFunctionAttribute")) {
						foundUnregister = true;
						foundRegisterUnregisterMethod = true;
					}
				}
				if (foundRegisterUnregisterMethod && method.IsVisible ()) {
					Runner.Report (method, Severity.High, Confidence.High,
						"Method is marked with the ComRegisterFunctionAttribute or with the ComUnregisterFunctionAttribute and is externally visible");
				}
			}

			if (foundRegister ^ foundUnregister) { // only one of them is true
				if (foundRegister)
					Runner.Report (type, Severity.High, Confidence.High,
						"Type contains has a method with ComRegisterFunctionAttribute but it doesn't contain a method with ComUnregisterFunctionAttribute");
				if (foundUnregister)
					Runner.Report (type, Severity.High, Confidence.High,
						"Type contains has a method with ComUnregisterFunctionAttribute but it doesn't contain a method with ComRegisterFunctionAttribute");
			}

			return Runner.CurrentRuleResult;
		}
	}
}
