//
// Gendarme.Rules.Performance.ReviewLinqMethodRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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
	/// Linq extension methods operate on sequences of values so they generally
	/// have linear time complexity. However you may be able to achieve better
	/// than linear time performance if you use a less general method or take
	/// advantage of a method provided by an <c>Sytem.Collections.Generic.IEnumerable&lt;T&gt;</c> 
	/// subclass.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public string FirstOrMissing (IEnumerable&lt;string&gt; sequence, string missing)
	/// {
	/// 	// Count () is O(n)
	/// 	if (sequence.Count () &gt; 0) {
	/// 		return sequence.First ();
	/// 	}
	/// 	return missing;
	/// }
	/// 
	/// public void Append (List&lt;string&gt; lines, string line)
	/// {
	/// 	// Last () is O(n)
	/// 	if (lines.Count == 0 || lines.Last () != line) {
	/// 		lines.Add (line);
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public string FirstOrMissing (IEnumerable&lt;string&gt; sequence, string missing)
	/// {
	/// 	// We don't need an exact count so we can use the O(1) Any () method.
	/// 	if (sequence.Any ()) {
	/// 		return sequence.First ();
	/// 	}
	/// 	return missing;
	/// }
	/// 
	/// public void Append (List&lt;string&gt; lines, string line)
	/// {
	/// 	// Lines is a List so we can use the O(1) subscript operator instead of
	/// 	// the Last () method.
	/// 	if (lines.Count == 0 || lines [lines.Count - 1] != line) {
	/// 		lines.Add (line);
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("A linq extension method with linear time complexity is used, but a more efficient method is available.")]
	[Solution ("Use the more efficient method.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class ReviewLinqMethodRule : Rule, IMethodRule {

		private readonly OpCodeBitmask Comparisons = new OpCodeBitmask (0x2801400000000000, 0x0, 0x0, 0xB);
		private readonly OpCodeBitmask Conditions = new OpCodeBitmask (0x300180000000000, 0x0, 0x0, 0x0);

		public readonly MethodSignature CountProperty = new MethodSignature ("get_Count", null, new string [0]);
		public readonly MethodSignature LengthProperty = new MethodSignature ("get_Length", null, new string [0]);
		public readonly MethodSignature Subscript = new MethodSignature ("get_Item", null, new string [] {"System.Int32"});
		public readonly MethodSignature Sort = new MethodSignature ("Sort", null, new string [1]);

		private bool HasMethod (TypeReference type, MethodSignature method)
		{
			if (type.HasMethod (method))
				return true;
			
			TypeDefinition td = type.Resolve ();
			return td != null && td.BaseType != null && HasMethod (td.BaseType, method);
		}
		
		private void CheckForCountProperty (TypeReference type, MethodDefinition method, Instruction ins)
		{
			if (HasMethod (type, CountProperty)) {
				string message = "Use the Count property instead of the Count () method.";
				Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
				Runner.Report (method, ins, Severity.Medium, Confidence.High, message);

			} else if (type.IsArray || HasMethod (type, LengthProperty)) {
				// note: arrays [] always have a Length property but resolving arrays return the element type
				string message = "Use the Length property instead of the Count () method.";
				Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
				Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
			}
		}

		private void CheckForAny (MethodDefinition method, Instruction ins)
		{			
			// call System.Int32 System.Linq.Enumerable::Count<System.String>(System.Collections.Generic.IEnumerable`1<!!0>)
			// ldc.i4.0
			// cgt, clt, ceq, ble, ble.s, bge or bge.s
			Instruction n1 = ins.Next;
			Instruction n2 = n1 != null ? n1.Next : null;
			if (n1 != null && n2 != null) {
				object rhs = n1.GetOperand (method);
				if (rhs != null && rhs.Equals (0)) {
					if (Comparisons.Get (n2.OpCode.Code)) {
						string message = "Use Any () instead of Count ().";
						Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
						Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
					}
				}
			}

			// ldc.i4.0
			// ldarg.1
			// call System.Int32 System.Linq.Enumerable::Count<System.String>(System.Collections.Generic.IEnumerable`1<!!0>)
			// cgt, clt, ceq, ble, ble.s, bge or bge.s
			Instruction p1 = ins.Previous;
			Instruction p2 = p1 != null ? p1.Previous : null;
			if (p1 != null && p2 != null && n1 != null) {
				object rhs = p2.GetOperand (method);
				if (rhs != null && rhs.Equals (0)) {
					if (Comparisons.Get (n1.OpCode.Code)) {
						string message = "Use Any () instead of Count ().";
						Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
						Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
					}
				}
			}

			// call System.Int32 System.Linq.Enumerable::Count<System.String>(System.Collections.Generic.IEnumerable`1<!!0>)
			// brtrue, brtrue.s, brfalse or brfalse.s
			if (n1 != null) {
				if (Conditions.Get(n1.OpCode.Code)) {
					string message = "Use Any () instead of Count ().";
					Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
					Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
				}
			}
		}
		
		private void CheckForSubscript (TypeReference type, MethodDefinition method, Instruction ins, string name)
		{
			if (type.IsArray) {
				string message = String.Format (CultureInfo.InvariantCulture, 
					"Use operator [] instead of the {0} method.", name);
				Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
				Runner.Report (method, ins, Severity.Medium, Confidence.High, message);

			} else {
				TypeDefinition td = type.Resolve ();						// resolve of an array returns the element type...
				if (td != null && HasMethod (td, Subscript)) {
					string message = String.Format (CultureInfo.InvariantCulture,
						"Use operator [] instead of the {0} method.", name);
					Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
					Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
				}
			}
		}
		
		private void CheckForSort (TypeReference type, MethodDefinition method, Instruction ins, string name)
		{
			if (type.IsArray) {
				string message = String.Format (CultureInfo.InvariantCulture,
					"Use Array.Sort instead of the {0} method.", name);
				Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
				Runner.Report (method, ins, Severity.Medium, Confidence.High, message);

			} else {
				TypeDefinition td = type.Resolve ();						// resolve of an array returns the element type...
				if (td != null && HasMethod (td, Sort)) {
					string message = String.Format (CultureInfo.InvariantCulture,
						"Use Sort instead of the {0} method.", name);
					Log.WriteLine (this, "{0:X4} {1}", ins.Offset, message);
					Runner.Report (method, ins, Severity.Medium, Confidence.High, message);
				}
			}
		}
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// The IEnumerable<T> extension methods were introduced in .NET 3.5 (but runtime 2 !)
			// so disable the rule if the assembly can't have Linq
			Runner.AnalyzeAssembly += (object o, RunnerEventArgs e) => {
				Active = CanHasLinq (e.CurrentAssembly);
			};
		}

		private static bool CanHasLinq (AssemblyDefinition assembly)
		{
			if (assembly.MainModule.Runtime < TargetRuntime.Net_2_0)
				return false;

			return assembly.References ("System.Core");
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			OpCodeBitmask calls = OpCodeBitmask.Calls;
			if (!calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;
								
			Log.WriteLine (this, "--------------------------------------");
			Log.WriteLine (this, method);
			
			// Loop through each instruction,
			foreach (Instruction ins in method.Body.Instructions) {
				// if we're calling a method,
				if (!calls.Get (ins.OpCode.Code))
					continue;
				
				// and the method is a System.Linq.Enumerable method then,
				var target = ins.Operand as MethodReference;
				if (!target.DeclaringType.IsNamed ("System.Linq", "Enumerable"))
					continue;

				string tname = target.Name;
				int tcount = target.HasParameters ? target.Parameters.Count : 0;
				// see if we can use a more efficient method.
				if (tname == "Count" && tcount == 1) {
					TypeReference tr = ins.Previous.GetOperandType (method);
					if (tr != null) {
						CheckForCountProperty (tr, method, ins);
						CheckForAny (method, ins);
					}
				} else if ((tname == "ElementAt" || tname == "ElementAtOrDefault") && tcount == 2) {
					Instruction arg = ins.TraceBack (method);
					TypeReference tr = arg.GetOperandType (method);
					if (tr != null)
						CheckForSubscript (tr, method, ins, tname);
				} else if ((tname == "Last" || tname == "LastOrDefault") && tcount == 1) {
					TypeReference tr = ins.Previous.GetOperandType (method);
					if (tr != null)
						CheckForSubscript (tr, method, ins, tname);
				} else if (tname == "OrderBy" || tname == "OrderByDescending") {
					Instruction arg = ins.TraceBack (method);
					TypeReference tr = arg.GetOperandType (method);
					if (tr != null)
						CheckForSort (tr, method, ins, tname);
				}
			}

			return Runner.CurrentRuleResult;
		}
		
#if false
		private static OpCodeBitmask ComparisonsBitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Cgt);
			mask.Set (Code.Ceq);
			mask.Set (Code.Clt);
			mask.Set (Code.Ble);
			mask.Set (Code.Ble_S);
			mask.Set (Code.Bge);
			mask.Set (Code.Bge_S);
			Console.WriteLine ("ComparisonsBitmask : " + mask);
			return mask;
		}

		private static OpCodeBitmask ConditionsBitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Brtrue);
			mask.Set (Code.Brtrue_S);
			mask.Set (Code.Brfalse);
			mask.Set (Code.Brfalse_S);
			Console.WriteLine ("ConditionsBitmask : " + mask);
			return mask;
		}
#endif
	}
}
