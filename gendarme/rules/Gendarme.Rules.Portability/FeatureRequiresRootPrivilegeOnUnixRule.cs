//
// Gendarme.Rules.Portability.FeatureRequiresRootPrivilegeOnUnixRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Andreas Noever
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
using System.Collections;
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Portability {

	public class FeatureRequiresRootPrivilegeOnUnixRule : IMethodRule {

		private const string Ping = "System.Net.NetworkInformation.Ping";

		//Check for usage of System.Diagnostics.Process.set_PriorityClass
		private static bool CheckProcessSetPriorityClass (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Call:
			case Code.Calli:
			case Code.Callvirt:
				MethodReference method = (ins.Operand as MethodReference);
				if (method.Name != "set_PriorityClass")
					return false;
				if (method.DeclaringType.FullName != "System.Diagnostics.Process")
					return false;

				Instruction prev = ins.Previous; //check stack
				switch (prev.OpCode.Code) {
				case Code.Ldc_I4_S:
					return ((ProcessPriorityClass) (sbyte) prev.Operand != ProcessPriorityClass.Normal);
				case Code.Ldc_I4:
					return ((ProcessPriorityClass) prev.Operand != ProcessPriorityClass.Normal);
				case Code.Ldc_I4_M1:
				case Code.Ldc_I4_0:
				case Code.Ldc_I4_1:
				case Code.Ldc_I4_2:
				case Code.Ldc_I4_3:
				case Code.Ldc_I4_4:
				case Code.Ldc_I4_5:
				case Code.Ldc_I4_6:
				case Code.Ldc_I4_7:
				case Code.Ldc_I4_8:
					return false;
				}
				break;
			}
			return false;
		}

		private static bool CheckPing (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Newobj:	// new Ping ()
			case Code.Call:		// MyPing () : base () ! (automatic parent constructor call)
			case Code.Calli:
			case Code.Callvirt:
				MethodReference method = (ins.Operand as MethodReference);
				return (method.DeclaringType.FullName == "System.Net.NetworkInformation.Ping");
			}
			return false;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return runner.RuleSuccess;

			// do not apply the rule to the Ping class itself (i.e. when running Gendarme on System.dll 2.0+)
			bool is_ping = (method.DeclaringType.FullName == Ping ||
				(method.DeclaringType.IsNested && method.DeclaringType.DeclaringType.FullName == Ping));

			MessageCollection results = null;

			foreach (Instruction ins in method.Body.Instructions) {

				// Check for usage of System.Diagnostics.Process.set_PriorityClass
				if (CheckProcessSetPriorityClass (ins)) {
					if (results == null)
						results = new MessageCollection ();

					Location loc = new Location (method, ins.Offset);
					Message msg = new Message ("Setting Process.PriorityClass to something else than ProcessPriorityClass.Normal requires root privileges.", loc, MessageType.Warning);
					results.Add (msg);
				}

				// short-circuit
				if (!is_ping && CheckPing (ins)) {
					if (results == null)
						results = new MessageCollection ();

					Location loc = new Location (method, ins.Offset);
					string s = String.Format ("Usage of {0} requires root privileges.", Ping);
					Message msg = new Message (s, loc, MessageType.Warning);
					results.Add (msg);
				}
			}

			return results;
		}
	}
}
