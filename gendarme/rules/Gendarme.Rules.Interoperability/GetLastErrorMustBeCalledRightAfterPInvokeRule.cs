//
// Gendarme.Rules.Interoperability.GetLastErrorMustBeCalledRightAfterPInvokeRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007-2008 Andreas Noever
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule will fire if <code>Marshal.GetLastWin32Error()</code> is called, but is
	/// not called immediately after a P/Invoke. This is a problem because other methods,
	/// even managed methods, may overwrite the error code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void DestroyError ()
	/// {
	///	MessageBeep (2);
	///	Console.WriteLine ("Beep");
	///	int error = Marshal.GetLastWin32Error ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void GetError ()
	/// {
	///	MessageBeep (2);
	///	int error = Marshal.GetLastWin32Error ();
	///	Console.WriteLine ("Beep");
	/// }
	/// 
	/// public void DontUseGetLastError ()
	/// {
	///	MessageBeep (2);
	///	Console.WriteLine ("Beep");
	/// }
	/// </code>
	/// </example>

	[Problem ("GetLastError() should be called immediately after the P/Invoke call.")]
	[Solution ("Move the call to GetLastError just after the P/Invoke call.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke")]
	public class GetLastErrorMustBeCalledRightAfterPInvokeRule : Rule, IMethodRule {

		struct Branch : IEquatable<Branch> {
			public readonly Instruction Instruction;
			public readonly bool DirtyMethodCalled;

			public Branch (Instruction ins, bool dirty)
			{
				this.Instruction = ins;
				this.DirtyMethodCalled = dirty;
			}

			public override bool Equals (object obj)
			{
				if (obj is Branch)
					return Equals ((Branch) obj);
				return false;
			}

			public bool Equals (Branch other)
			{
				return (Instruction == other.Instruction) && (DirtyMethodCalled == other.DirtyMethodCalled);
			}

			public override int GetHashCode ()
			{
				return Instruction.GetHashCode () ^ DirtyMethodCalled.GetHashCode ();
			}

			public static bool operator == (Branch left, Branch right)
			{
				return left.Equals (right);
			}

			public static bool operator != (Branch left, Branch right)
			{
				return !left.Equals (right);
			}
		}

		List<Branch> branches = new List<Branch> ();

		private bool CheckPInvoke (Instruction startInstruction)
		{
			branches.Clear ();
			branches.Add (new Branch (startInstruction.Next, false));

			for (int i = 0; i < branches.Count; i++) { //follow all branches
				Instruction ins = branches [i].Instruction;
				bool dirty = branches [i].DirtyMethodCalled;
				bool getLastErrorFound = false;

				while (true) { //follow the branch

					//check if a method is called
					if (ins.OpCode.FlowControl == FlowControl.Call) {

						MethodDefinition mDef = (ins.Operand as MethodReference).Resolve ();
						if (mDef != null && mDef.IsPInvokeImpl) { //check if another pinvoke method is called, this counts as "GetLastError not called"
							break;
						}

						string s = (mDef == null) ? String.Empty : mDef.DeclaringType.GetFullName ();
						switch (s) {
						case "System.Runtime.InteropServices.Marshal":
							getLastErrorFound = (mDef.Name == "GetLastWin32Error");
							break; //found
						case "System.Runtime.InteropServices.SafeHandle":
							dirty = (mDef.Name != "get_IsInvalid");
							break;
						case "System.IntPtr":
						case "System.UIntPtr":
							string name = mDef.Name;
							dirty = ((name != "op_Inequality") && (name != "op_Equality"));
							break;
						default:
							dirty = true;
							break;
						}

						if (getLastErrorFound)
							break;
					}

					//fetch the next instruction
					object alternatives;
					ins = StackEntryAnalysis.GetNextInstruction (ins, out alternatives);

					if (ins == null)
						break;

					if (alternatives != null) {
						Instruction alt_ins = (alternatives as Instruction);
						if (alt_ins != null) {
							branches.AddIfNew (new Branch (alt_ins, dirty));
						} else {
							Instruction [] alts = (Instruction []) alternatives;
							foreach (Instruction altIns in alts)
								branches.AddIfNew (new Branch (altIns, dirty));
						}
					}
					//avoid infinity loop
					if (ins.OpCode.FlowControl == FlowControl.Branch) {
						branches.AddIfNew (new Branch (ins, dirty));
						break;
					}
				}

				//report error
				if (getLastErrorFound && dirty)
					return false;
			}
			return true;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				// nothing do check if there's no more instructions
				if (ins.Next == null)
					break;

				MethodReference mr = ins.GetMethod ();
				if (mr == null)
					break;

				MethodDefinition pinvoke = mr.Resolve ();
				if ((pinvoke == null) || !pinvoke.IsPInvokeImpl)
					break;

				// check if GetLastError is called near enough this pinvoke call
				if (CheckPInvoke (ins))
					break;

				// code might not work if an error occurs
				Runner.Report (method, ins, Severity.High, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}

