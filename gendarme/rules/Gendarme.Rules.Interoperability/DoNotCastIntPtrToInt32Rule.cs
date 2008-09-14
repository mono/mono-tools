//
// Gendarme.Rules.Interoperability.DoNotCastIntPtrToInt32Rule
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule checks for any cast of <code>IntPtr</code> or <code>UIntPtr</code> into a
	/// 32bits (or smaller) value. <code>IntPtr</code> are generally used to reference a 
	/// memory location and downcasting them to 32bits will make the code fail on 64bits CPU.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// int ptr = dest.ToInt32 ();
	/// for (int i = 0; i &lt; 16; i++) {
	///	Marshal.StructureToPtr (this, (IntPtr)ptr, false);
	///	ptr += 4;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// long ptr = dest.ToInt64 ();
	/// for (int i = 0; i &lt; 16; i++) {
	///	Marshal.StructureToPtr (this, (IntPtr) ptr, false);
	///	ptr += IntPtr.Size;
	/// }
	/// </code>
	/// </example>

	[Problem ("This method cast a [U]IntPtr to a 32 bits value and this won't work on 64 bits architectures.")]
	[Solution ("You should always use 64 bits integers, signed or unsigned, when computing pointers.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotCastIntPtrToInt32Rule : Rule, IMethodRule {

		private void Report (MethodDefinition method, Instruction ins, string typeName)
		{
			string msg = String.Format ("Type cast to '{0}'.", typeName);
			Runner.Report (method, ins, Severity.High, Confidence.Normal, msg);
		}

		private static string Convert (Instruction ins)
		{
			if (ins == null)
				return null;

			switch (ins.OpCode.Code) {
			case Code.Conv_I1:
				return Constants.SByte;
			case Code.Conv_U1:
				return Constants.Byte;
			case Code.Conv_I2:
				return Constants.Int16;
			case Code.Conv_U2:
				return Constants.UInt16;
			case Code.Conv_I4:
				return Constants.Int32;
			case Code.Conv_U4:
				return Constants.UInt32;
			default:
				return null;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// it's a safe bet that the downsizing of the [U]IntPtr is not a problem inside GetHashCode
			if (MethodSignatures.GetHashCode.Matches (method))
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = (ins.Operand as MethodReference);
				if (mr == null)
					continue;

				// look for both IntPtr and the (less known) UIntPtr
				string type = mr.DeclaringType.FullName;
				bool intptr = (type == "System.IntPtr");
				bool uintptr = (type == "System.UIntPtr");
				if (!intptr && !uintptr)
					continue;

				// check for calls to IntPtr.ToInt32, UIntPtr.ToUInt32 or 
				// cast to anything other than [U]IntPtr, [U]Int64 or Void*
				if (intptr && (mr.Name == "ToInt32")) {
					Runner.Report (method, ins, Severity.High, Confidence.Normal, "Call to 'IntPtr.ToInt32()'.");
				} else if (uintptr && (mr.Name == "ToUInt32")) {
					Runner.Report (method, ins, Severity.High, Confidence.Normal, "Call to 'UIntPtr.ToUInt32()'.");
				} else if (mr.Name == "op_Explicit") {
					switch (mr.ReturnType.ReturnType.FullName) {
					case "System.Int64":
					case "System.UInt64":
						// valid cases unless (like [g]mcs does) it's followed by a convertion
						string msg = Convert (ins.Next);
						if (msg != null)
							Report (method, ins, msg);
						break;
					case "System.IntPtr": // used to cast void*
					case "System.UIntPtr": // used to cast void*
					case "System.Void*": // used to cast void*
						// valid cases
						break;
					default:
						// problematic cases
						Report (method, ins, mr.ReturnType.ReturnType.FullName);
						break;
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
