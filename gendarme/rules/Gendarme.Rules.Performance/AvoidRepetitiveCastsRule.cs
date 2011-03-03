//
// Gendarme.Rules.Performance.AvoidRepetitiveCastsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Jesse Jones <jesjones@mindspring.com>
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
	/// This rule fires if multiple casts are done on the same value, for the same type.
	/// Casts are expensive so reducing them, by changing the logic or caching the 
	/// result, can help performance.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// foreach (object o in list) {
	///	// first cast (is)
	///	if (o is ICollection) {
	///		// second cast (as) if item implements ICollection
	///		Process (o as ICollection);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// foreach (object o in list) {
	///	// a single cast (as) per item
	///	ICollection c = (o as ICollection);
	///	if (c != null) {
	///		SingleCast (c);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example:
	/// <code>
	///	// first cast (is):
	///	if (o is IDictionary) {
	///		// second cast if item implements IDictionary:
	///		Process ((IDictionary) o);
	///	// first cast (is):
	///	} else if (o is ICollection) {
	///		// second cast if item implements ICollection:
	///		Process ((ICollection) o);
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	///	// a single cast (as) per item
	///	IDictionary dict;
	///	ICollection col;
	///	if ((dict = o as IDictionary) != null) {
	///		SingleCast (dict);
	///	} else if ((col = o as ICollection) != null) {
	///		SingleCast (col);
	///	}
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The method seems to repeat the same cast operation multiple times.")]
	[Solution ("Change the logic to ensure the (somewhat expensive) cast is done once.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	public class AvoidRepetitiveCastsRule : Rule, IMethodRule {

		List<Instruction> casts = new List<Instruction> ();

		static Instruction GetOrigin (Instruction ins)
		{			
			Instruction previous = ins.Previous;
			Instruction origin = previous;

			if (previous.OpCode.FlowControl == FlowControl.Call) {
				Instruction next = ins.Next;
				if (next.IsStoreLocal ()) 
					origin = next;	// a duplicate cast which loads this local is an error
			}
				
//			Console.WriteLine("origin of instruction at {0:X4} is at {1:X4}", ins.Offset, origin.Offset);
			
			return origin;
		}
		
		static bool LocalsMatch (object operand1, object operand2)
		{
			VariableReference v1 = operand1 as VariableReference;
			VariableReference v2 = operand2 as VariableReference;
			
			if (v1 != null && v2 != null)
				return v1.Index == v2.Index;
			else if (operand1 != null)
				return operand1.Equals (operand2);			
			else
				return operand2 == null;
		}
		
		static bool IndexesMatch (MethodDefinition method, Instruction lhs, Instruction rhs)
		{
			bool match = false;
			
			if (lhs.OpCode.Code == rhs.OpCode.Code) {
				switch (lhs.OpCode.Code) {
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
				case Code.Ldc_I4_S:
				case Code.Ldc_I4:
					object operand1 = lhs.GetOperand (method);
					object operand2 = rhs.GetOperand (method);
					match = operand1.Equals (operand2);
					break;
				}
			}
			
			return match;
		}
		
		// stack[0]  == index
		// stack[-1] == array
		static bool LoadElementMatch (MethodDefinition method, Instruction lhs, Instruction rhs)
		{
			bool match = false;
			
			// We only handle simple cases like:
			//   ldarg.1
			//   ldc.i4.7
			//   ldelem.ref
			Instruction index1 = lhs.Previous;
			Instruction index2 = rhs.Previous;
			if (IndexesMatch (method, index1, index2)) {
				Instruction load1 = index1.Previous;
				Instruction load2 = index2.Previous;

				if (load1.IsLoadArgument () && load2.IsLoadArgument ())
					match = load1.GetOperand (method).Equals (load2.GetOperand (method));

				else if (load1.IsLoadLocal () && load2.IsLoadLocal ())
					match = LocalsMatch (load1.GetOperand (method), load2.GetOperand (method));
			}
			
			return match;
		}
		
		// stack[0] == addr
		static bool LoadIndirectMatch (MethodDefinition method, Instruction lhs, Instruction rhs)
		{
			bool match = false;
			
			// We only handle simple cases like:
			//    ldarg.1
			//    ldind.ref
			Instruction load1 = lhs.Previous;
			Instruction load2 = rhs.Previous;
			if (load1.IsLoadArgument () && load2.IsLoadArgument ())
				match = load1.GetOperand (method).Equals (load2.GetOperand (method));

			else if (load1.IsLoadLocal () && load2.IsLoadLocal ())
				match = LocalsMatch (load1.GetOperand (method), load2.GetOperand (method));
			
			return match;
		}
		
		static bool OriginsMatch (MethodDefinition method, Instruction lhs, Instruction rhs)
		{
			bool match = false;
			
			object operand1 = lhs.GetOperand (method);
			object operand2 = rhs.GetOperand (method);
			
			if (lhs.OpCode.Code == rhs.OpCode.Code) {
				if (lhs.IsLoadArgument ())
					match = operand1.Equals (operand2);

				else if (lhs.IsLoadElement ())
					match = LoadElementMatch (method, lhs, rhs);

				else if (lhs.IsLoadIndirect ())
					match = LoadIndirectMatch (method, lhs, rhs);

				else if (lhs.IsLoadLocal ())
					match = LocalsMatch (operand1, operand2);

			} else if (lhs.IsStoreLocal () && rhs.IsLoadLocal ())
				match = LocalsMatch (operand1, operand2);

			else if (lhs.IsLoadLocal () && rhs.IsStoreLocal ()) 
				match = LocalsMatch (operand1, operand2);
			
			return match;
		}

		private int FindDuplicates (MethodDefinition method, TypeReference type, Instruction origin)
		{
			// we already had our first cast if we got here
			int count = 1;

			// don't check 0 since it's the one we compare with
			for (int i = 1; i < casts.Count; i++) {
				Instruction ins = casts [i];
				if (!(ins.Operand as TypeReference).IsNamed (type.Namespace, type.Name))
					continue;
				if (!OriginsMatch(method, origin, GetOrigin (ins)))
					continue;
				// we're removing this so we need to adjust the counter
				// important since we don't want duplicate reports
				casts.RemoveAt (i);
				i--;
				count++;
			}
			return count;
		}

		static OpCodeBitmask Casts = new OpCodeBitmask (0x0, 0x18000000000000, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			// and was not generated by the compiler or a tool (outside of developer's control)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// is there any IsInst or Castclass instructions in the method ?
			if (!Casts.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;
			
//			Console.WriteLine ();
//			Console.WriteLine ("-----------------------------------------");
//			Console.WriteLine (new MethodPrinter(method));

			foreach (Instruction ins in method.Body.Instructions) {
				Code code = ins.OpCode.Code;
				// IsInst -> if (t is T) ...
				//        -> t = t as T; ...
				// Castclass -> t = (T) t; ...
				if ((code == Code.Isinst) || (code == Code.Castclass))
					casts.Add (ins);
			}

			// if there's only one then it can't be a duplicate cast
			while (casts.Count > 1) {
				Instruction ins = casts [0];
				TypeReference type = (ins.Operand as TypeReference);
				Instruction origin = GetOrigin (ins);

				int count = FindDuplicates (method, type, origin);
				if (count > 1) {
//					Console.WriteLine ("found {0} duplicates for {1:X4}", count, ins.Offset);

					// rare, but it's possible to cast a null value (ldnull)
					object name = origin.GetOperand (method) ?? "Null";
					string msg = String.Format (CultureInfo.InvariantCulture, 
						"'{0}' is casted {1} times for type '{2}'.", name, count, type.GetFullName ());
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
				}
				casts.RemoveAt (0);
			}
			casts.Clear ();

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask casts = new OpCodeBitmask ();
			casts.Set (Code.Castclass);
			casts.Set (Code.Isinst);
			Console.WriteLine (casts);
		}
#endif
	}
}
