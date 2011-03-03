//
// Gendarme.Rules.Interoperability.DoNotAssumeIntPtrSizeRule
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule checks for code which casts an <code>IntPtr</code> or <code>UIntPtr</code> into a
	/// 32-bit (or smaller) value. It will also check if memory read with the <c>Marshal.ReadInt32</c>
	/// and <c>Marshal.ReadInt64</c> methods is being cast into an <code>IntPtr</code> or 
	/// <code>UIntPtr</code>. <code>IntPtr</code> is generally used to reference a memory 
	/// location and downcasting them to 32-bits will make the code fail on 64-bit CPUs.
	/// </summary>
	/// <example>
	/// Bad example (cast):
	/// <code>
	/// int ptr = dest.ToInt32 ();
	/// for (int i = 0; i &lt; 16; i++) {
	///	Marshal.StructureToPtr (this, (IntPtr)ptr, false);
	///	ptr += 4;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (Marshal.Read*):
	/// <code>
	/// // that won't work on 64 bits platforms
	/// IntPtr p = (IntPtr) Marshal.ReadInt32 (p);
	/// </code>
	/// </example>
	/// <example>
	/// Good example (cast):
	/// <code>
	/// long ptr = dest.ToInt64 ();
	/// for (int i = 0; i &lt; 16; i++) {
	///	Marshal.StructureToPtr (this, (IntPtr) ptr, false);
	///	ptr += IntPtr.Size;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (Marshal.Read*):
	/// <code>
	/// IntPtr p = (IntPtr) Marshal.ReadIntPtr (p);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0 but was named DoNotCastIntPtrToInt32Rule before 2.2</remarks>

	[Problem ("This method casts a [U]IntPtr to a 32-bit value which won't work on 64-bit architectures.")]
	[Solution ("You should always use 64 bits integers, signed or unsigned, when doing pointer math.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DoNotAssumeIntPtrSizeRule : Rule, IMethodRule {

		private void Report (MethodDefinition method, Instruction ins, string typeName)
		{
			string msg = String.Format (CultureInfo.InvariantCulture, "Type cast to '{0}'.", typeName);
			Runner.Report (method, ins, Severity.High, Confidence.Normal, msg);
		}

		private static string Convert (Instruction ins)
		{
			if (ins == null)
				return String.Empty;

			switch (ins.OpCode.Code) {
			case Code.Conv_I1:
			case Code.Conv_Ovf_I1:
				return "System.SByte";
			case Code.Conv_U1:
			case Code.Conv_Ovf_U1:
				return "System.Byte";
			case Code.Conv_I2:
			case Code.Conv_Ovf_I2:
				return "System.Int16";
			case Code.Conv_U2:
			case Code.Conv_Ovf_U2:
				return "System.UInt16";
			case Code.Conv_I4:
			case Code.Conv_Ovf_I4:
				return "System.Int32";
			case Code.Conv_U4:
			case Code.Conv_Ovf_U4_Un:
				return "System.UInt32";
			default:
				return String.Empty;
			}
		}

		private void CheckCastOnIntPtr (MethodDefinition method, Instruction ins, MethodReference mr, bool intptr, bool uintptr)
		{
			// check for calls to IntPtr.ToInt32, UIntPtr.ToUInt32 or 
			// cast to anything other than [U]IntPtr, [U]Int64 or Void*
			string name = mr.Name;
			if (intptr && (name == "ToInt32")) {
				Runner.Report (method, ins, Severity.High, Confidence.Normal, "Call to 'IntPtr.ToInt32()'.");
			} else if (uintptr && (name == "ToUInt32")) {
				Runner.Report (method, ins, Severity.High, Confidence.Normal, "Call to 'UIntPtr.ToUInt32()'.");
			} else if (name == "op_Explicit") {
				string rtfullname = mr.ReturnType.GetFullName ();
				switch (rtfullname) {
				case "System.Int64":
				case "System.UInt64":
					// valid cases unless (like [g]mcs does) it's followed by a convertion
					string msg = Convert (ins.Next);
					if (msg.Length > 0)
						Report (method, ins, msg);
					break;
				case "System.IntPtr": // used to cast void*
				case "System.UIntPtr": // used to cast void*
				case "System.Void*": // used to cast void*
					// valid cases
					break;
				default:
					// problematic cases
					Report (method, ins, rtfullname);
					break;
				}
			}
		}

		private void CheckCastOnMarshal (MethodDefinition method, Instruction ins, MethodReference mr)
		{
			// if we're reading an int32 or an int64 then we should not be assigning it to an IntPtr
			string method_name = mr.Name;
			if ((method_name == "ReadInt32") || (method_name == "ReadInt64")) {
				Instruction next = ins.Next;
				if (next.OpCode.Code != Code.Call)
					return;

				MethodReference m = (next.Operand as MethodReference);
				if (m.Name != "op_Explicit")
					return;

				string msg = String.Format (CultureInfo.InvariantCulture,
					"A '{0}' value is casted into an '{1}' when reading marshalled memory.",
					mr.ReturnType.GetFullName (), m.Parameters [0].ParameterType.GetFullName ());
				Runner.Report (method, ins, Severity.High, Confidence.Normal, msg);
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
				MethodReference mr = ins.GetMethod ();
				if (mr == null)
					continue;

				// look for both IntPtr and the (less known) UIntPtr
				TypeReference type = mr.DeclaringType;
				if (type.Namespace == "System") {
					string name = type.Name;
					bool intptr = (name == "IntPtr");
					bool uintptr = (name == "UIntPtr");
					if (intptr || uintptr)
						CheckCastOnIntPtr (method, ins, mr, intptr, uintptr);
				} else if (type.IsNamed ("System.Runtime.InteropServices", "Marshal")) {
					CheckCastOnMarshal (method, ins, mr);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
