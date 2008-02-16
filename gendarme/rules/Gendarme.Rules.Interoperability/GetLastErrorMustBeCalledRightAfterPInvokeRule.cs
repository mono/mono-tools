//
// Gendarme.Rules.Interoperability.GetLastErrorMustBeCalledRightAfterPInvokeRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007-2008 Andreas Noever
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	[Problem ("GetLastError() should be called immediately after this the P/Invoke call.")]
	[Solution ("Move the call to GetLastError just after the P/Invoke call.")]
	public class GetLastErrorMustBeCalledRightAfterPInvokeRule : Rule, IMethodRule {

		struct Branch {
			public readonly Instruction Instruction;
			public readonly bool DirtyMethodCalled;

			public Branch (Instruction ins, bool dirty)
			{
				this.Instruction = ins;
				this.DirtyMethodCalled = dirty;
			}
		}

		private const string Message = "GetLastError() should be called immediately after this the PInvoke call.";

		private const string GetLastError = "System.Int32 System.Runtime.InteropServices.Marshal::GetLastWin32Error()";
		private List<string> AllowedCalls;

		public GetLastErrorMustBeCalledRightAfterPInvokeRule ()
		{
			AllowedCalls = new List<string> ();
			AllowedCalls.Add ("System.Boolean System.Runtime.InteropServices.SafeHandle::get_IsInvalid()");
			AllowedCalls.Add ("System.Boolean System.IntPtr::op_Inequality(System.IntPtr,System.IntPtr)");
			AllowedCalls.Add ("System.Boolean System.IntPtr::op_Equality(System.IntPtr,System.IntPtr)");
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

						MethodDefinition mDef = ins.Operand as MethodDefinition;
						if (mDef != null && mDef.IsPInvokeImpl) { //check if another pinvoke method is called, this counts as "GetLastError not called"
							break;
						}
						string calledMethod = ins.Operand.ToString ();

						if (calledMethod == GetLastError) {
							getLastErrorFound = true;
							break; //found
						}

						if (!AllowedCalls.Contains (calledMethod)) {
							dirty = true;
						}

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

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					// nothing do check if there's no more instructions
					if (ins.Next == null)
						break;

					MethodDefinition pinvoke = (ins.Operand as MethodReference).Resolve ();
					if (pinvoke == null) 
						break;

					if (!pinvoke.IsPInvokeImpl)
						break;

					// check if GetLastError is called near enough this pinvoke call
					if (CheckPInvoke (ins))
						break;

					// code might not work if an error occurs
					Runner.Report (method, ins, Severity.High, Confidence.High, String.Empty);
					break;
				default:
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
