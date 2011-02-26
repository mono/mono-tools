// 
// Gendarme.Rules.BadPractice.PreferParamsArrayForVariableArgumentsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// The rule warns for any method that use the (semi-documented) <c>vararg</c> 
	/// calling convention (e.g. <c>__arglist</c> in C#) and that is not used for 
	/// interoperability (i.e. pinvoke to unmanaged code).
	/// Using <c>params</c> (C#) can to achieve the same objective while <c>vararg</c> 
	/// is not CLS compliant. The later will limit the usability of the method to CLS 
	/// compliant language (e.g. Visual Basic does not support <c>vararg</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void ShowItems_Bad (string header, __arglist)
	/// {
	///	Console.WriteLine (header);
	///	ArgIterator args = new ArgIterator (__arglist);
	///	for (int i = 0; i &lt; args.GetRemainingCount (); i++) {
	///		Console.WriteLine (__refvalue (args.GetNextArg (), string));
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void ShowItems (string header, params string [] items)
	/// {
	///	Console.WriteLine (header);
	///	for (int i = 0; i &lt; items.Length; i++) {
	///		Console.WriteLine (items [i]);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (interoperability):
	/// <code>
	/// [DllImport ("libc.dll")]
	/// static extern int printf (string format, __arglist);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("The varags (__arglist in C#) calling convention should only be used for unmanaged interoperability purpose.")]
	[Solution ("Use params (C#) to support a variable number of parameters for managed code.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2230:UseParamsForVariableArguments")]
	public class PreferParamsArrayForVariableArgumentsRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += (object o, RunnerEventArgs e) => {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System", "ArgIterator");
					}));
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// methods using vararg are easily identifiable
			if ((method.CallingConvention & MethodCallingConvention.VarArg) == 0)
				return RuleResult.Success;

			// __arglist is accepted for interoperability purpose
			if (method.IsPInvokeImpl)
				return RuleResult.Success;

			// all other case should be changed to use "params"
			// this is more severe for visible methods since vararg is not CLS compliant
			Severity severity = method.IsVisible () ? Severity.Critical : Severity.High;
			Runner.Report (method, severity, Confidence.High);
			return RuleResult.Failure;
		}
	}
}

