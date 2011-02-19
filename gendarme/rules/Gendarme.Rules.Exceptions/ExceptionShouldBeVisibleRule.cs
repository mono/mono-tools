// 
// Gendarme.Rules.Exceptions.ExceptionShouldBeVisibleRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	// TODO: This rule will not work very well for assemblies without externally visible types.
	// It might be worthwhile to record the problematic exceptions and report them during
	// teardown only if an externally visible type was found in the assembly.

	/// <summary>
	/// This rule checks for non-visible exceptions which derive directly from
	/// the most basic exceptions: <c>System.Exception</c>, <c>System.ApplicationException</c>
	/// or <c>System.SystemException</c>. Those basic exceptions, being visible, will be the 
	/// only information available to the API consumer - but do not contain enough data to be
	/// useful.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// internal class GeneralException : Exception {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (visibility):
	/// <code>
	/// public class GeneralException : Exception {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (base class):
	/// <code>
	/// internal class GeneralException : ArgumentException {
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This exception is not public and its base class does not provide enough information to be useful.")]
	[Solution ("Change the exception's visibility to public, inherit from another exception, or ignore the defect.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
	public class ExceptionShouldBeVisibleRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			TypeReference btype = type.BaseType;
			if (btype == null)
				return RuleResult.DoesNotApply;

			// rule apply only to type that inherits from the base exceptions
			if (btype.Namespace != "System")
				return RuleResult.DoesNotApply;
			string name = btype.Name;
			if ((name != "Exception") && (name != "SystemException") && (name != "ApplicationException"))
				return RuleResult.DoesNotApply;

			if (type.IsAbstract || type.IsVisible ())
				return RuleResult.Success;

			Runner.Report (type, Severity.High, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
