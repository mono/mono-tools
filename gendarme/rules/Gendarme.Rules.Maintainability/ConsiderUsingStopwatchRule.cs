//
// Gendarme.Rules.Maintainability.ConsiderUsingStopwatchRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks methods for cases where a <c>System.Diagnostics.Stopwatch</c> could be
	/// used instead of using <c>System.DateTime</c> to compute the time required for an action.
	/// Stopwatch is preferred because it better expresses the intent of the code and because (on
	/// some platforms at least) StopWatch is accurate to roughly the microsecond whereas 
	/// DateTime.Now is only accurate to 16 milliseconds or so. This rule only applies to assemblies
	/// compiled with the .NET framework version 2.0 (or later).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public TimeSpan DoLongOperation ()
	/// {
	///	DateTime start = DateTime.Now;
	///	DownloadNewOpenSuseDvdIso ();
	///	return DateTime.Now - start;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public TimeSpan DoLongOperation ()
	/// {
	///	Stopwatch watch = Stopwatch.StartNew ();
	///	DownloadNewOpenSuseDvdIso ();
	///	return watch.Elapsed;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method uses the difference between two DateTime.Now calls to retrieve processing time. The developer's intent may not be very clear.")]
	[Solution ("Use the System.Diagnostics.Stopwatch type to improve code readability.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ConsiderUsingStopwatchRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference (sealed) System.DateTime
			// then no code inside the module will use it.
			// also we do not want to run this on <2.0 assemblies since Stopwatch
			// did not exist back then.
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0
					&& (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System", "DateTime");
					}
				)));
			};
		}

		private static bool AreMirrorInstructions (Instruction ld, Instruction st, MethodDefinition method)
		{
			return (ld.GetVariable (method).Index == st.GetVariable (method).Index);
		}

		private static bool IsGetNow (Instruction ins)
		{
			if (ins.OpCode.Code != Code.Call)
				return false;

			MethodReference calledMethod = (MethodReference) ins.Operand;
			return calledMethod.IsNamed ("System", "DateTime", "get_Now");
		}
		
		private static bool CheckParameters (MethodDefinition method, Instruction ins)
		{
			Instruction prev;
			if (ins.IsLoadLocal ()) {
				prev = ins.Previous;
				while (null != prev) {
					// look for a STLOC* instruction and compare the variable indexes
					if (prev.IsStoreLocal () && AreMirrorInstructions (ins, prev, method))
						return IsGetNow (prev.Previous);
					prev = prev.Previous;
				}
			} else if (ins.OpCode.Code == Code.Ldobj) {
				prev = ins.TraceBack (method);
				ParameterDefinition p = prev.GetParameter (method);
				if (p == null)
					return false;
				int arg = p.Index;
				prev = prev.Previous;
				while (null != prev) {
					// look for a STOBJ instruction and compare the objects
					if (prev.OpCode.Code == Code.Stobj) {
						prev = prev.TraceBack (method);
						p = prev.GetParameter (method);
						return (p == null) ? false : (arg == p.Index);
					}
					prev = prev.Previous;
				}
			} else {
				return IsGetNow (ins);
			}
			return false;
		}

		private static bool CheckUsage (MethodDefinition method, Instruction ins)
		{
			// track the two parameters given to DateTime.op_Substraction
			Instruction param1 = ins.TraceBack (method, -1);
			Instruction param2 = ins.TraceBack (method);
			return CheckParameters (method, param1) && CheckParameters (method, param2);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method
			OpCodeBitmask calls = OpCodeBitmask.Calls;
			if (!calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference calledMethod = ins.GetMethod ();
				if (calledMethod == null)
					continue;
				if (!calledMethod.DeclaringType.IsNamed ("System", "DateTime"))
					continue;
				if (!MethodSignatures.op_Subtraction.Matches (calledMethod))
					continue;

				if (CheckUsage (method, ins))
					Runner.Report (method, ins, Severity.Low, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
