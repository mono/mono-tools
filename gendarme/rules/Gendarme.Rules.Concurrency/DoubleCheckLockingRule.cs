//
// Gendarme.Rules.Concurrency.DoubleCheckLockingRule.cs: 
//	looks for instances of double-check locking.
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) 2005 Aaron Tomb
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	// note: the rule only reports a single double-lock per method

	/// <summary>
	/// This rule is used to check for the double-check pattern, often used when implementing 
	/// the singleton pattern (1), and warns of potential incorrect usage. 
	/// 
	/// The original CLR (1.x) could not guarantee that a double-check would work correctly 
	/// in multithreaded applications. However the technique does work on the x86 architecture, 
	/// the most common architecture, so the problem is seldom seen (e.g. IA64).
	/// 
	/// The CLR 2 and later introduce a strong memory model (2) where a double check for a
	/// <c>lock</c> is correct (as long as you assign to a <c>volatile</c> variable). This
	/// rule won't report a defect for assemblies targetting the 2.0 (and later) runtime.
	/// <list>
	/// <item><term>1. Implementing Singleton in C#</term><description>
	/// http://msdn.microsoft.com/en-us/library/ms998558.aspx</description></item>
	/// <item><term>2. Understand the Impact of Low-Lock Techniques in Multithreaded Apps</term>
	/// <description>http://msdn.microsoft.com/en-ca/magazine/cc163715.aspx#S5</description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Singleton {
	///	private static Singleton instance;
	///	private static object syncRoot = new object ();
	/// 
	///	public static Singleton Instance {
    	/// 		get {
	///			if (instance == null) {
	///				lock (syncRoot) {
	///					if (instance == null) {
	///						instance = new Singleton ();
	///					}
	///				}
	///			}
	///			return instance;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (for 1.x code avoid using double check):
	/// <code>
	/// public class Singleton {
	///	private static Singleton instance;
	///	private static object syncRoot = new object ();
	/// 
	///	public static Singleton Instance {
	/// 		get {
	/// 			// do not check instance before the lock
	/// 			// this will work on all CLRs but will affect 
	/// 			// performance since the lock is always acquired
	///			lock (syncRoot) {
	///				if (instance == null) {
	///					instance = new Singleton ();
	///				}
	///			}
	///			return instance;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (for 2.x and later):
	/// <code>
	/// public class Singleton {
	///	// by using 'volatile' the double check will work under CLR 2.x
	///	private static volatile Singleton instance;
	///	private static object syncRoot = new object ();
	/// 
	///	public static Singleton Instance {
	/// 		get {
	///			if (instance == null) {
	///				lock (syncRoot) {
	///					if (instance == null) {
	///						instance = new Singleton ();
	///					}
	///				}
	///			}
	///			return instance;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method uses the potentially unreliable double-check locking technique.")]
	[Solution ("Remove the check that occurs outside of the protected region.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoubleCheckLockingRule : Rule, IMethodRule {

		Stack<int> monitorOffsetList = new Stack<int> ();
		List<Instruction> comparisons = new List<Instruction> ();

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = 
					// we only want to run this on assemblies that use either the
					// 1.0 or 1.1 runtime - since the memory model, at that time,
					// was not entirely safe for double check locks
					e.CurrentModule.Runtime < TargetRuntime.Net_2_0 &&
					
					// is this module using Monitor.Enter ? (lock in c#)
					// if not then this rule does not need to be executed for the module
					// note: mscorlib.dll is an exception since it defines, not refer, System.Threading.Monitor
					(e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Threading", "Monitor");
					}));
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			comparisons.Clear ();
			monitorOffsetList.Clear ();

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.FlowControl) {
				case FlowControl.Cond_Branch:
					if ((ins.Previous == null) || (ins.Previous.Previous == null))
						continue;
					if (monitorOffsetList.Count > 0) {
						/* If there's a comparison in the list matching this
						* one, we have double-check locking. */
						foreach (Instruction insn in comparisons) {
							if (!EffectivelyEqual (insn, ins))
								continue;
							if (!EffectivelyEqual (insn.Previous, ins.Previous))
								continue;
							if (!EffectivelyEqual (insn.Previous.Previous, ins.Previous.Previous))
								continue;
							if (insn.Offset >= monitorOffsetList.Peek ())
								continue;

							Runner.Report (method, insn, Severity.Medium, Confidence.High);
							return RuleResult.Failure;
						}
					}
					comparisons.Add (ins);
					break;
				case FlowControl.Call:
					MethodReference m = (ins.Operand as MethodReference);
					if (IsMonitorMethod (m, "Enter"))
						monitorOffsetList.Push (ins.Offset);
					else if (IsMonitorMethod (m, "Exit")) {
						if (monitorOffsetList.Count > 0)
							monitorOffsetList.Pop ();
					}
					break;
				}
			}
			return RuleResult.Success;
		}

		private static bool IsMonitorMethod (MethodReference method, string methodName)
		{
			if (method.Name != methodName)
				return false;
			if (!method.DeclaringType.IsNamed ("System.Threading", "Monitor"))
				return false;
			// exclude Monitor.Enter(object, ref bool) since the comparison would be made
			// againt the 'lockTaken' parameter and would report failures for every cases.
			// not a big deal since this rule if active only on code compiled < FX 2.0
			return (method.Parameters.Count == 1);
		}

		private static bool EffectivelyEqual (Instruction insn1, Instruction insn2)
		{
			// return false if opcode are different
			if (insn1.OpCode != insn2.OpCode)
				return false;

			// If both are branch instructions, we don't care about their targets, only their opcodes.
			if (insn1.OpCode.FlowControl == FlowControl.Cond_Branch)
				return true;

			// For other instructions, their operands must also be equal.
			if (insn1.Operand == null)
				return (insn2.Operand == null);

			return insn1.Operand.Equals (insn2.Operand);
		}
	}
}
