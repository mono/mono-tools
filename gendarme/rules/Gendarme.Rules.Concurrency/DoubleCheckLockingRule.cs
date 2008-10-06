//
// Gendarme.Rules.Concurrency.DoubleCheckLockingRule.cs: 
//	looks for instances of double-check locking.
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) 2005 Aaron Tomb
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	// FIXME?: see http://groups.google.com/group/gendarme/browse_thread/thread/b46d1ddc3a2d8fb9#msg_9b9c2989cedb4c34
	
	// note: the rule only report a single double-lock per method

	/// <summary>
	/// This rule is used to check for the double-check pattern in multi-threaded 
	/// code and warns of its incorrect usage. For more information about the double
	/// checking problem, see the next article: 
	/// http://www.cs.umd.edu/~pugh/java/memoryModel/DoubleCheckedLocking.html
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public static Singleton Instance {
    	/// 	get {
	///		if (instance == null) {
	///			lock (syncRoot) {
	///				if (instance == null) 
	///					instance = new Singleton ();
	///			}
	///		}
	///		return instance;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public static Singleton Instance {
	///	get {
	///		lock (syncRoot) {
	///			if (instance == null) 
	///				instance = new Singleton ();
	///		}
	///		return instance;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method uses the unreliable double-check locking technique.")]
	[Solution ("Remove the lock check that occurs outside of the protected region.")]
	public class DoubleCheckLockingRule : Rule, IMethodRule {

		Stack<int> monitorOffsetList = new Stack<int> ();
		List<Instruction> comparisons = new List<Instruction> ();

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

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
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

							Runner.Report (method, insn, Severity.Medium, Confidence.High, String.Empty);
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
			return (method.DeclaringType.FullName == "System.Threading.Monitor");
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
