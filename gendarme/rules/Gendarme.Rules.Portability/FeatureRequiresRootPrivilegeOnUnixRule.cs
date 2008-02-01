//
// Gendarme.Rules.Portability.FeatureRequiresRootPrivilegeOnUnixRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return runner.RuleSuccess;

			// do not apply the rule to the Ping class itself (i.e. when running Gendarme on System.dll 2.0+)
			bool is_ping = (method.DeclaringType.FullName == Ping || 
				(method.DeclaringType.IsNested && method.DeclaringType.DeclaringType.FullName == Ping));

			MessageCollection results = null;

			foreach (Instruction ins in method.Body.Instructions) {

				//Check for usage of System.Diagnostics.Process.set_PriorityClass
				switch (ins.OpCode.Name) {
				case "call":
				case "calli":
				case "callvirt":
					MethodReference calledMethod = (MethodReference) ins.Operand;
					if (calledMethod.Name != "set_PriorityClass")
						break;
					if (calledMethod.DeclaringType.FullName != "System.Diagnostics.Process")
						break;

					Instruction prev = ins.Previous; //check stack
					if (prev.OpCode == OpCodes.Ldc_I4_S)
						if ((ProcessPriorityClass) (sbyte) prev.Operand == ProcessPriorityClass.Normal)
							break;

					if (results == null)
						results = new MessageCollection ();

					Location loc = new Location (method, ins.Offset);
					Message msg = new Message ("Setting Process.PriorityClass to something else than ProcessPriorityClass.Normal requires root privileges.", loc, MessageType.Warning);
					results.Add (msg);
					break;
				default:
					break;
				}

				// short-circuit
				if (is_ping)
					continue;

				switch (ins.OpCode.Name) {
				case "newobj": //new Ping ()
				case "call": //MyPing () : base () ! (automatic parent constructor call)
				case "calli":
				case "callvirt":
					MethodReference calledMethod = (MethodReference) ins.Operand;
					if (calledMethod.DeclaringType.FullName != "System.Net.NetworkInformation.Ping")
						break;

					if (results == null)
						results = new MessageCollection ();

					Location loc = new Location (method, ins.Offset);
					string s = String.Format ("Usage of {0} requires root privileges.", Ping);
					Message msg = new Message (s, loc, MessageType.Warning);
					results.Add (msg);
					break;
				}
			}

			return results;
		}
	}
}
