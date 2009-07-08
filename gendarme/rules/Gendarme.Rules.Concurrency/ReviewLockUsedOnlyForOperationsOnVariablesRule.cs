//
// Gendarme.Rules.Concurrency.ReviewLockUsedOnlyForOperationsOnVariablesRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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
using System.Collections.Generic;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule checks if a lock is used only to perform operations on locals
	/// or fields.
	/// If the only purpose of that critical section is to make sure the variables
	/// are modified atomatically then the methods provided by
	/// System.Threading.Interlocked class will be more efficient.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// lock (_lockObject) {
	/// 	_counter++;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// Interlocked.Increment(_counter);
	/// </code>
	/// </example>
	/// <example>
	/// Bad example:
	/// <code>
	/// lock (_lockObject) {
	/// 	_someSharedObject = anotherObject;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// Interlocked.Exchange(_someSharedObject, anotherObject);
	/// </code>
	/// </example>

	[Problem ("Using a lock to do only atomic operations on locals or fields is often overkill.")]
	[Solution ("If possible, use System.Threading.Interlocked class to improve throughput.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewLockUsedOnlyForOperationsOnVariablesRule : Rule, IMethodRule {

		private int lockDepth = -1;
		//64 nested locks in one method should be enough for anybody ;)
		//the rule would just not analyze further.
		private int [] lockDoesNotApply = new int [64];

		private static OpCodeBitmask interlockedFriendlyOpCodeBitmask = BuildInterlockedFriendlyOpCodeBitmask ();


		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// is this module using Monitor.Enter ? (lock in c#)
			// if not then this rule does not need to be executed for the module
			// note: mscorlib.dll is an exception since it defines, not refer, System.Threading.Monitor
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == Constants.Corlib) ||
					e.CurrentModule.TypeReferences.ContainsType ("System.Threading.Monitor");
			};
		}


		private bool HandleLockEnterExit (MethodReference m, MethodDefinition caller, Instruction ins)
		{
			if (lockDepth >= lockDoesNotApply.Length)
				return false;

			if (IsMonitorMethod (m, "Enter")) {
				lockDepth ++;
				lockDoesNotApply [lockDepth] = 0;
				return true;
			} else if (IsMonitorMethod (m, "Exit") && lockDepth > -1) {
				if (lockDoesNotApply [lockDepth] < 0)
					Runner.Report (caller, ins, Severity.Medium, Confidence.Normal);
				lockDepth --;
				//a nested lock invalidates parent lock (deferred case)
				//thus return false
			}
			return false;
		}

		private bool CurrentLockSectionDoesNotApply
		{
			get {
				if (lockDepth < 0 || lockDepth >= lockDoesNotApply.Length)
					return false;
				return lockDoesNotApply [lockDepth] > 0;
			}
			set {
				if (lockDepth < 0 || lockDepth >= lockDoesNotApply.Length)
					return;
				if (value)
					lockDoesNotApply [lockDepth] = 1;
				else if (lockDoesNotApply [lockDepth] == 0) //deferred
					lockDoesNotApply [lockDepth] = -1;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			lockDepth = -1;
			Array.Clear (lockDoesNotApply, 0, lockDoesNotApply.Length);

			foreach (Instruction ins in method.Body.Instructions) {

				if (ins.OpCode.FlowControl == FlowControl.Call) {
					//if this is a lock entry or exit then change state
					//if not, the the current lock section does not apply
					if (!HandleLockEnterExit (ins.Operand as MethodReference, method, ins))
						CurrentLockSectionDoesNotApply = true;
					continue;
				}
				if (lockDepth < 0) //do not care below until we enter a lock
					continue;
				if (CurrentLockSectionDoesNotApply) //do not care below until we
					continue;                       //exit the lock

				//TODO: handle CompareExchange scenario
				if (ins.OpCode.FlowControl == FlowControl.Cond_Branch)
					CurrentLockSectionDoesNotApply = true;

				CurrentLockSectionDoesNotApply = !interlockedFriendlyOpCodeBitmask.Get (ins.OpCode.Code);
			}

			return Runner.CurrentRuleResult;
		}

		//FIXME: copied from DoubleCheckLockingRule AND DoNotUseLockedRegion..., we need to share this
		private static bool IsMonitorMethod (MethodReference method, string methodName)
		{
			if (method.Name != methodName)
				return false;
			return (method.DeclaringType.FullName == "System.Threading.Monitor");
		}

		private static OpCodeBitmask BuildInterlockedFriendlyOpCodeBitmask ()
		{
#if true
			return new OpCodeBitmask (0x80063FFFFFFFFF, 0x3f00000001800000, 0x1F30000000000000, 0x5F80);
#else
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.UnionWith (OpCodeBitmask.FlowControlBranch);
			mask.UnionWith (OpCodeBitmask.FlowControlReturn);
			//locals
			mask.UnionWith (OpCodeBitmask.LoadLocal);
			mask.UnionWith (OpCodeBitmask.StoreLocal);
			//arguments
			mask.UnionWith (OpCodeBitmask.LoadArgument);
			mask.UnionWith (OpCodeBitmask.StoreArgument);
			//fields
			mask.Set (Code.Ldfld);
			mask.Set (Code.Ldflda);
			mask.Set (Code.Stfld);
			mask.Set (Code.Ldsfld);
			mask.Set (Code.Ldsflda);
			mask.Set (Code.Stsfld);
			//constants
			mask.Set (Code.Ldnull);
			mask.Set (Code.Ldc_I4_M1);
			mask.Set (Code.Ldc_I4_0);
			mask.Set (Code.Ldc_I4_1);
			mask.Set (Code.Ldc_I4_2);
			mask.Set (Code.Ldc_I4_3);
			mask.Set (Code.Ldc_I4_4);
			mask.Set (Code.Ldc_I4_5);
			mask.Set (Code.Ldc_I4_6);
			mask.Set (Code.Ldc_I4_7);
			mask.Set (Code.Ldc_I4_8);
			mask.Set (Code.Ldc_I4_S);
			mask.Set (Code.Ldc_I4);
			mask.Set (Code.Ldc_I8);
			mask.Set (Code.Ldc_R4);
			mask.Set (Code.Ldc_R8);
			//safe
			mask.Set (Code.Dup);
			mask.Set (Code.Pop);
			mask.Set (Code.Nop);
			mask.Set (Code.Break);
			//could be replace by interlocked call
			mask.Set (Code.Add);
			mask.Set (Code.Add_Ovf);
			mask.Set (Code.Add_Ovf_Un);
			mask.Set (Code.Sub);
			mask.Set (Code.Sub_Ovf);
			mask.Set (Code.Sub_Ovf_Un);
			return mask;
#endif
		}

	}

}

