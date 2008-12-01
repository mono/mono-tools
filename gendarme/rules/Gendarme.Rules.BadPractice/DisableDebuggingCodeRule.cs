// 
// Gendarme.Rules.BadPractice.DisableDebuggingCodeRule
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule check if non-console applications are using <c>Console.WriteLine</c> as
	/// a mean to help debugging issues. While useful the debugging code should, before its
	/// release be removed, moved inside methods decorated with <c>[Conditional("DEBUG")]</c> or 
	/// <c>[Conditional("TRACE")]</c> or changed to use the <c>Debug</c> or <c>Trace</c> types.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// private byte[] GenerateKey ()
	/// {
	///	byte[] key = new byte[16];
	///	rng.GetBytes (key);
	///	Console.WriteLine ("debug key = {0}", BitConverter.ToString (key));
	///	return key;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (removed):
	/// <code>
	/// private byte[] GenerateKey ()
	/// {
	///	byte[] key = new byte[16];
	///	rng.GetBytes (key);
	///	return key;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (changed):
	/// <code>
	/// private byte[] GenerateKey ()
	/// {
	///	byte[] key = new byte[16];
	///	rng.GetBytes (key);
	///	Debug.WriteLine ("debug key = {0}", BitConverter.ToString (key));
	///	return key;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method include calls to Console.WriteLine inside an assembly not compiled for console application (e.g. /target:exe).")]
	[Solution ("If this code is used for debugging purpose then either use the Debug or Trace types or disable the code manually (e.g. using a preprocessor).")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DisableDebuggingCodeRule : Rule, IMethodRule {

		private const string ConditionalAttribute = "System.Diagnostics.ConditionalAttribute";
		private const string Console = "System.Console";

		// note: there can be multiple [Conditional] attribute on a method
		private static bool HasConditionalAttributeForDebugging (CustomAttributeCollection cac)
		{
			foreach (CustomAttribute ca in cac) {
				if (ca.Constructor.DeclaringType.FullName == ConditionalAttribute) {
					// this should not happen since there's a single ctor accepting a string
					// bu we never know what the next framework version can throw at us...
					if (ca.ConstructorParameters.Count < 1)
						continue;
					switch (ca.ConstructorParameters [0] as string) {
					case "DEBUG":
					case "TRACE":
						return true;
					}
				}
			}
			return false;
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// using Console.Write* methods is ok if the application is compiled
			// with /target:exe - but not if it's compiled with /target:winexe or
			// /target:library
			Runner.AnalyzeAssembly += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Kind != AssemblyKind.Console);
			};

			// if the module does not reference System.Console then no
			// method inside it will be calling any Console.write* methods
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active &= (e.CurrentAssembly.Name.Name == Constants.Corlib) ||
					e.CurrentModule.TypeReferences.ContainsType (Console);
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if there's no IL code
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// it's ok if the code is conditionally compiled for DEBUG or TRACE purposes
			if (method.HasCustomAttributes) {
				if (HasConditionalAttributeForDebugging (method.CustomAttributes))
					return RuleResult.Success;
			}

			foreach (Instruction ins in method.Body.Instructions) {
				// look for a call...
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				// ... to System.Console ...
				MethodReference mr = (ins.Operand as MethodReference);
				if (mr.DeclaringType.FullName != Console)
					continue;

				// ... Write* methods
				if (mr.Name.StartsWith ("Write", StringComparison.Ordinal))
					Runner.Report (method, ins, Severity.Low, Confidence.Normal, mr.ToString ());
				// Confidence==Normal because we can't be sure if there's some logic to avoid displaying
				// on the console under normal (non debugging) circumstances
			}

			return Runner.CurrentRuleResult;
		}
	}
}
