//
// AvoidInt64ArgumentsInComVisibleMethodsRule.cs
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
	/// This rule checks that ComVisible methods do not take System.Int64 arguments
	/// because Visual Basic 6 clients do not support it.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public class Bad {
	///		public void DoBadThings (long a)
	///		{
	///			// doing bad things
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (type changed):
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public class Good {
	///		public void DoGoodThings (int a)
	///		{
	///			// doing good things
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (method is not visible from COM):
	/// <code>
	/// [assembly: ComVisible (false)]
	/// namespace InteropLibrary {
	///	[ComVisible (true)]
	///	public class Good {
	///		[ComVisible (false)]
	///		public void DoGoodThings (long a)
	///		{
	///			// doing good things
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// Rule applies only when the containing assembly has ComVisible attribute explicitly set to false 
	/// and the type has ComVisible attribute explicitly set to true.
	/// </remarks>
	[Problem ("ComVisible method takes Int64 argument which is not usable from VB6 clients")]
	[Solution ("Change argument type (e.g. to the System.Int32 or System.Decimal), or mark the method with [ComVisible (false)]")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1406:AvoidInt64ArgumentsForVB6Clients")]
	public class AvoidInt64ArgumentsInComVisibleMethodsRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasParameters)
				return RuleResult.DoesNotApply;
			
			// check that class is explicitly ComVisible, assembly is explicitly not ComVisible
			// and method does not have [ComVisible (false)]
			if (!method.DeclaringType.IsTypeComVisible () || !(method.IsComVisible () ?? true))
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				if (parameter.ParameterType.IsNamed ("System", "Int64"))
					Runner.Report (parameter, Severity.Medium, Confidence.Total);
			}

			return Runner.CurrentRuleResult;

		}
	}
}
