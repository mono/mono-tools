//
// Gendarme.Rules.Concurrency.DoNotLockOnThisOrTypesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;

namespace Gendarme.Rules.Concurrency {

	[Problem ("This method use a lock(this) or lock(typeof(X)) construct which is often used incorrectly.")]
	[Solution ("To be safe from outside always lock on something that is totally private to your code.")]
	public class DoNotLockOnThisOrTypesRule : LockAnalyzerRule {

		private const string LockThis = "Monitor.Enter(this) or lock(this) in C#";
		private const string LockType = "Monitor.Enter(typeof({0})) or lock(typeof({0})) in C#";

		public override void Analyze (MethodDefinition method, Instruction ins)
		{
			// well original instruction since this is where we will report the defect
			Instruction call = ins;
			while (ins.Previous != null) {
				ins = ins.Previous;
				switch (ins.OpCode.Code) {
				case Code.Ldarg_0:
					Runner.Report (method, Severity.High, Confidence.High, LockThis);
					return;
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
					return;
				case Code.Ldarg:
				case Code.Ldarg_S:
					ParameterDefinition pd = (ins.Operand as ParameterDefinition);
					if (pd != null && pd.Sequence == 0)
						Runner.Report (method, call, Severity.High, Confidence.High, LockThis);
					return;
				case Code.Ldfld:
				case Code.Ldsfld:
					return;
				case Code.Call:
				case Code.Callvirt:
					MethodReference mr = (ins.Operand as MethodReference);
					if (mr.ReturnType.ReturnType.FullName != "System.Type")
						continue;
					
					string msg;
					if ((mr.Name == "GetTypeFromHandle") && (mr.DeclaringType.Name == "Type")) {
						// ldtoken
						msg = String.Format (LockType, (ins.Previous.Operand as TypeReference).Name);
					} else {
						msg = mr.ToString ();
					}
					Runner.Report (method, call, Severity.High, Confidence.High, msg);
					return;
				default:
					if (!IsLoadLoc (ins) || !IsStoreLoc (ins.Previous))
						continue;
					// [g]mcs commonly do a stloc.x ldloc.x just before a ldarg.0
					if (GetVariable (method, ins) != GetVariable (method, ins.Previous))
						continue;
					if (ins.Previous.Previous == null)
						continue;
					if (ins.Previous.Previous.OpCode.Code == Code.Ldarg_0) {
						Runner.Report (method, call, Severity.High, Confidence.High, LockThis);
						return;
					}
					break;
				}
			}
		}

		public override RuleResult CheckMethod (MethodDefinition method)
		{
			// can't lock on 'this' inside a static method
			if (method.IsStatic)
				return RuleResult.DoesNotApply;

			return base.CheckMethod (method);
		}
	}
}
