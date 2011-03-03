//
// Gendarme.Rules.Performance.AvoidUnneededUnboxingRule
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
using System.Collections.Generic;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule checks methods which unbox the same value type multiple times (i.e. the
	/// value is copied from the heap into the stack). Because the copy is relatively expensive, 
	/// the code should be rewritten to minimize unboxes. For example, using a local variable 
	/// of the right value type should remove the need for more than one unbox instruction
	/// per variable.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public struct Message {
	///	private int msg;
	///	private IntPtr hwnd, lParam, wParam, IntPtr result;
	///	
	///	public override bool Equals (object o)
	///	{
	///		bool result = (this.msg == ((Message) o).msg);
	///		result &amp;= (this.hwnd == ((Message) o).hwnd);
	///		result &amp;= (this.lParam == ((Message) o).lParam);
	///		result &amp;= (this.wParam == ((Message) o).wParam);
	///		result &amp;= (this.result == ((Message) o).result);
	///		return result;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public struct Message {
	///	private int msg;
	///	private IntPtr hwnd, lParam, wParam, IntPtr result;
	///	
	///	public override bool Equals (object o)
	///	{
	///		Message msg = (Message) o;
	///		bool result = (this.msg == msg.msg);
	///		result &amp;= (this.hwnd == msg.hwnd);
	///		result &amp;= (this.lParam == msg.lParam);
	///		result &amp;= (this.wParam == msg.wParam);
	///		result &amp;= (this.result == msg.result);
	///		return result;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method unboxes (converts from object to a value type) the same value multiple times.")]
	[Solution ("Cast the variable, once, into a temporary variable and use the temporary.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidUnneededUnboxingRule : Rule, IMethodRule {

		private static string Previous (MethodDefinition method, Instruction ins)
		{
			string kind, name;

			ins = ins.Previous;
			Code previous_op_code = ins.OpCode.Code;

			switch (previous_op_code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				kind = "Parameter";
				int index = previous_op_code - Code.Ldarg_0;
				if (!method.IsStatic)
					index--;
				name = (index >= 0) ? method.Parameters [index].Name : String.Empty;
				break;
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				kind = "Parameter";
				name = (ins.Operand as ParameterDefinition).Name;
				break;
			case Code.Ldfld:
			case Code.Ldsfld:
				kind = "Field";
				name = (ins.Operand as FieldReference).Name;
				break;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				kind = "Variable";
				int vindex = previous_op_code - Code.Ldloc_0;
				name = method.Body.Variables [vindex].GetName ();
				break;
			case Code.Ldloc:
			case Code.Ldloc_S:
				kind = "Variable";
				name = (ins.Operand as VariableDefinition).GetName ();
				break;
			default:
				return String.Empty;
			}
			return String.Format (CultureInfo.InvariantCulture, "{0} '{1}' unboxed to type '{2}' {{0}} times.", 
				kind, name, (ins.Operand as TypeReference).GetFullName ());
		}

		// unboxing is never critical - but a high amount can be a sign of other problems too
		private static Severity GetSeverityFromCount (int count)
		{
			if (count < 4)
				return Severity.Low;
			if (count < 8)
				return Severity.Medium;
			// >= 8
			return Severity.High;
		}

		static OpCodeBitmask Unbox = new OpCodeBitmask (0x0, 0x40000000000000, 0x400000000, 0x0);

		private Dictionary<string, int> unboxed = new Dictionary<string, int> ();

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Unbox or Unbox_Any instructions in the method ?
			if (!Unbox.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Unbox:
				case Code.Unbox_Any:
					string previous = Previous (method, ins);
					if (previous.Length == 0)
						continue;

					int num;
					if (!unboxed.TryGetValue (previous, out num)) {
						unboxed.Add (previous, 1);
					} else {
						unboxed [previous] = ++num;
					}
					break;
				}
			}

			// report findings (one defect per variable/parameter/field)
			foreach (KeyValuePair<string,int> kvp in unboxed) {
				// we can't (always) avoid unboxing one time
				if (kvp.Value < 2)
					continue;
				string s = String.Format (CultureInfo.InvariantCulture, kvp.Key, kvp.Value);
				Runner.Report (method, GetSeverityFromCount (kvp.Value), Confidence.Normal, s);
			}

			unboxed.Clear ();
			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask unbox = new OpCodeBitmask ();
			unbox.Set (Code.Unbox);
			unbox.Set (Code.Unbox_Any);
			Console.WriteLine (unbox);
		}
#endif
	}
}
