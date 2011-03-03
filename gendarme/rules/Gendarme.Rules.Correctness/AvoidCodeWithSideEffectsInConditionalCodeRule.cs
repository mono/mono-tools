//
// Gendarme.Rules.Correctness.AvoidCodeWithSideEffectsInConditionalCodeRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// 	(C) 2009 Jesse Jones
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// A number of System methods are conditionally compiled on #defines. 
	/// For example, System.Diagnostics.Trace::Assert is a no-op if TRACE
	/// is not defined and System.Diagnostics.Debug::Write is a no-op if DEBUG
	/// is not defined.
	///
	/// When calling a conditionally compiled method care should be taken to
	/// avoid executing code which has visible side effects. The reason is that 
	/// the state of the program should generally not depend on the value of 
	/// a define. If it does it is all too easy to create code which, for example, 
	/// works in DEBUG but fails or behaves differently in release.
	///
	/// This rule flags expressions used to construct the arguments to a 
	/// conditional call if those expressions write to a local variable, method
	/// argument, or field. This includes pre/postfix increment and decrement
	/// expressions and assignment expressions.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// internal sealed class Helpers {
	/// 	public string StripHex (string text)
	/// 	{
	/// 		int i = 0;
	/// 		
	/// 		// This code will work in debug, but not in release.
	/// 		Debug.Assert (text [i++] == '0');
	/// 		Debug.Assert (text [i++] == 'x');
	/// 		
	/// 		return text.Substring (i);
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// internal sealed class Helpers {
	/// 	public string StripHex (string text)
	/// 	{
	/// 		Debug.Assert (text [0] == '0');
	/// 		Debug.Assert (text [1] == 'x');
	/// 		
	/// 		return text.Substring (2);
	/// 	}
	/// }
	/// </code>
	/// </example>
	
	[Problem ("A conditionally compiled method is being called, but one of the actual arguments mutates state.")]
	[Solution ("If the state must be changed then do it outside the method call.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidCodeWithSideEffectsInConditionalCodeRule : Rule, IMethodRule {
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			
			if (!mask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;
			
			Log.WriteLine (this);
			Log.WriteLine (this, "---------------------------------------");
			Log.WriteLine (this, method);
			
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference target = ins.Operand as MethodReference;
					string define = AvoidMethodsWithSideEffectsInConditionalCodeRule.ConditionalOn (target);
					if (define != null) {
						Log.WriteLine (this, "call to {0} method at {1:X4}", define, ins.Offset);
						
						string name = Mutates (method, ins);
						if (name != null) {
							string mesg = String.Format (CultureInfo.InvariantCulture, 
								"{0}::{1} is conditionally compiled on {2} but mutates {3}",
								target.DeclaringType.Name, target.Name, define, name);
							Log.WriteLine (this, mesg);
							
							Confidence confidence = AvoidMethodsWithSideEffectsInConditionalCodeRule.GetConfidence (define);
							Runner.Report (method, ins, Severity.High, confidence, mesg);
						}
					}
					break;
				}
			}
			
			return Runner.CurrentRuleResult;
		}
		
		private string Mutates (MethodDefinition method, Instruction end)
		{
			string name = null;
			
			Instruction ins = AvoidMethodsWithSideEffectsInConditionalCodeRule.FullTraceBack (method, end);
			if (ins != null) {
				Log.WriteLine (this, "checking args for call at {0:X4} starting at {1:X4}", end.Offset, ins.Offset);
				while (ins.Offset < end.Offset && name == null) {
					if (ins.IsStoreArgument ()) {
						ParameterDefinition pd = ins.Operand as ParameterDefinition;
						name = "argument " + pd.Name;
					
					} else if (ins.IsStoreLocal ()) {
						VariableDefinition vd = ins.GetVariable (method);
						if (!vd.IsGeneratedName ())
							name = "local " + vd.Name;
					
					} else if (ins.OpCode.Code == Code.Stfld || ins.OpCode.Code == Code.Stsfld) {
						FieldReference fr = ins.Operand as FieldReference;
						if (!fr.IsGeneratedCode ())
							name = "field " + fr.Name;
					}
					
					ins = ins.Next;
				}
			}
			
			return name;
		}
		
#if false
		private void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			
			mask.Set (Code.Starg);
			mask.Set (Code.Starg_S);
			mask.Set (Code.Stloc_0);
			mask.Set (Code.Stloc_1);
			mask.Set (Code.Stloc_2);
			mask.Set (Code.Stloc_3);
			mask.Set (Code.Stloc);
			mask.Set (Code.Stloc_S);
			mask.Set (Code.Stfld);
			mask.Set (Code.Stsfld);
			
			Console.WriteLine (mask);
		}
#endif

		static OpCodeBitmask mask = new OpCodeBitmask (0x93C00, 0x2400000000000000, 0x0, 0x1200);
	}
}
