//
// Gendarme.Rules.Concurrency.DontUseLockedRegionOutsideMethod.cs: 
//	looks for methods that enter an exclusive region but do not exit
//	(this can imply deadlocks, or just a bad practice).
//
// Authors:
//	Andres G. Aragoneses <aaragoneses@novell.com>
//
// Copyright (C) 2008 Andres G. Aragoneses
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
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Concurrency {

	// TODO: do a rule that checks if Monitor.Enter is used *before* Exit (dumb code, I know)
	// TODO: do a more complex rule that checks that you have used Thread.Monitor.Exit in a finally block
	[Problem ("This method uses Thread.Monitor.Enter() but doesn't use Thread.Monitor.Exit().")]
	[Solution ("Rather use the lock{} statement in case your language is C#, or Thread.Monitor.Exit() in other case.")]
	public class DoNotUseLockedRegionOutsideMethodRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// is this module using Monitor.Enter/Exit ? (lock in c#)
			// if not then this rule does not need to be executed for the module
			// note: mscorlib.dll is an exception since it defines, not refer, System.Threading.Monitor
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == Constants.Corlib) ||
					e.CurrentModule.TypeReferences.ContainsType ("System.Threading.Monitor");
			};
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			
			int enter = 0;
			int exit = 0;
			
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl == FlowControl.Call) {
					MethodReference m = (ins.Operand as MethodReference);
					if (IsMonitorMethod (m, "Enter")) {
						enter++;
					} else if (IsMonitorMethod (m, "Exit")) {
						exit++;
					}
				}
			}
			
			if (enter == exit)
				return RuleResult.Success;

			Runner.Report (method, Severity.High, Confidence.Normal, String.Empty);
			return RuleResult.Failure;
		}
		
		//FIXME: copied from DoubleCheckLockingRule, we need to share this
		private static bool IsMonitorMethod (MethodReference method, string methodName)
		{
			if (method.Name != methodName)
				return false;
			return (method.DeclaringType.FullName == "System.Threading.Monitor");
		}
	}
}
