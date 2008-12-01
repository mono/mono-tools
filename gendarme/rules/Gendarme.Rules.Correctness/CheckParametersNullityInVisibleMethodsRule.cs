//
// Gendarme.Rules.Correctness.CheckParametersNullityInVisibleMethodsRule
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	[Problem ("A visible method does not check its parameter(s) for null values.")]
	[Solution ("Since the caller is unkown you should always verify all of your parameters to protect yourself.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
	public class CheckParametersNullityInVisibleMethodsRule : Rule, IMethodRule {

		static OpCodeBitmask null_compare = new OpCodeBitmask (0x300180000000000, 0x0, 0x0, 0x0);
		static OpCodeBitmask check = new OpCodeBitmask (0x0, 0x70000000000FFE0, 0x100FFF800, 0x0);

		private Bitmask<long> has_null_check = new Bitmask<long> ();

		private void CheckParameter (ParameterDefinition parameter)
		{
			// nothing to do if it could not be resolved 
			if (parameter == null)
				return;

			// was there a null check done before ?	
			if (has_null_check.Get (parameter.Sequence))
				return;

			// make sure we don't report it more than once
			has_null_check.Set (parameter.Sequence);

			// ignore a value type parameter (as long as its not an array of value types)
			TypeReference ptype = parameter.ParameterType;
			// take care of references (ref)
			ReferenceType rt = (ptype as ReferenceType);
			if (rt != null)
				ptype = rt.ElementType;
			if (ptype.IsValueType && !ptype.IsArray ())
				return;

			// last chance, if the type is a generic type constrained not to be nullable
			GenericParameter gp = (parameter.ParameterType as GenericParameter);
			if ((gp != null) && gp.HasNotNullableValueTypeConstraint)
				return;

			// nullable with no check
			
			// we assume a better control of family members so this is less severe
			Severity s = (parameter.Method as MethodDefinition).IsFamily ? Severity.Medium : Severity.High;
			Runner.Report (parameter, s, Confidence.Normal);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// p/invoke, abstract methods and method without parameters
			if (!method.HasBody || !method.HasParameters || !method.IsVisible ())
				return RuleResult.DoesNotApply;

			has_null_check.ClearAll ();

			// check
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.IsLoadArgument ()) {
					ParameterDefinition parameter = ins.GetParameter (method);
					if (parameter != null) {
						Instruction next = ins.Next;
						Code nc = next.OpCode.Code;
						// load indirect reference (ref)
						if (nc == Code.Ldind_Ref) {
							next = next.Next;
							nc = next.OpCode.Code;
						}

						if (null_compare.Get (nc)) {
							has_null_check.Set (parameter.Sequence);
						} else {
							// compare with null
							if ((nc == Code.Ldnull) && (next.Next.OpCode.Code == Code.Ceq))
								has_null_check.Set (parameter.Sequence);
						}
					}
				} else if (OpCodeBitmask.Calls.Get (ins.OpCode.Code)) {
					MethodDefinition md = (ins.Operand as MethodReference).Resolve ();
					if (md == null)
						continue;
					// no dereference possible by calling a static method
					if (!md.IsStatic) {
						Instruction instance = ins.TraceBack (method);
						CheckParameter (instance.GetParameter (method));
					}
					// if was pass one of our parameter to another call then
					// this new call "takes" responsability of the null check
					// note: not perfect but greatly reduce false positives
					for (int i = 0; i < md.Parameters.Count; i++ ) {
						Instruction pi = ins.TraceBack (method, -(md.IsStatic ? 0 : 1 + i));
						if (pi == null)
							continue;
						ParameterDefinition p = pi.GetParameter (method);
						if (p != null)
							has_null_check.Set (p.Sequence);
					}
				} else if (check.Get (ins.OpCode.Code)) {
					Instruction owner = ins;
					// fields (no need to check static fields), ldind, ldelem, ldlen
					while ((owner != null) && check.Get (owner.OpCode.Code)) {
						owner = owner.TraceBack (method);
					}
					CheckParameter (owner.GetParameter (method));
				}
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask null_compare = new OpCodeBitmask ();
			null_compare.Set (Code.Brfalse);
			null_compare.Set (Code.Brfalse_S);
			null_compare.Set (Code.Brtrue);
			null_compare.Set (Code.Brtrue_S);
			Console.WriteLine (null_compare);

			OpCodeBitmask check = new OpCodeBitmask ();
			check.UnionWith (OpCodeBitmask.LoadElement);
			check.UnionWith (OpCodeBitmask.LoadIndirect);
			check.Set (Code.Ldlen);
			check.Set (Code.Ldfld);
			check.Set (Code.Ldflda);
			check.Set (Code.Stfld);
			Console.WriteLine (check);
		}
#endif
	}
}
