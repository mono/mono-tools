//
// Gendarme.Rules.Correctness.CheckParametersNullityInVisibleMethodsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008,2010 Novell, Inc (http://www.novell.com)
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

	/// <summary>
	/// This rule checks if all nullable parameters of visible methods are compared 
	/// with '''null''' before they get used. This reduce the likelyhood of the runtime
	/// throwing a <c>NullReferenceException</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [DllImport ("message")]
	/// internal static extern byte [] Parse (string s, int length);
	///
	/// public bool TryParse (string s, out Message m)
	/// {
	///	// is 's' is null then 's.Length' will throw a NullReferenceException
	///	// which a TryParse method should never do
	///	byte [] data = Parse (s, s.Length);
	///	if (data == null) {
	///		m = null;
	///		return false;
	///	}
	///	m = new Message (data);
	///	return true;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [DllImport ("message")]
	/// internal static extern byte [] Parse (string s, int length);
	///
	/// public bool TryParse (string s, out Message m)
	/// {
	///	if (s == null) {
	///		m = null;
	///		return false;
	///	}
	///	byte [] data = Parse (s, s.Length);
	///	if (data == null) {
	///		m = null;
	///		return false;
	///	}
	///	m = new Message (data);
	///	return true;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("A visible method does not check its parameter(s) for null values.")]
	[Solution ("Since the caller is unknown you should always verify all of your parameters to protect yourself.")]
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

			// out parameters can't be null
			if (parameter.IsOut)
				return;

			int sequence = parameter.GetSequence ();
			// ldarg this - 'this' cannot be null
			if (sequence == 0)
				return;

			// was there a null check done before ?	
			if (has_null_check.Get (sequence))
				return;

			// make sure we don't report it more than once
			has_null_check.Set (sequence);

			// ignore a value type parameter (as long as its not an array of value types)
			TypeReference ptype = parameter.ParameterType;
			// take care of references (ref)
			ByReferenceType rt = (ptype as ByReferenceType);
			if (rt != null)
				ptype = rt.ElementType;
			if (ptype.IsValueType && !ptype.IsArray)
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

		private void CheckArgument (MethodDefinition method, Instruction ins)
		{
			ParameterDefinition parameter = ins.GetParameter (method);
			if (parameter == null)
				return;

			// avoid checking parameters where a null check was already found
			if (has_null_check.Get (parameter.GetSequence ()))
				return;

			Instruction next = ins.Next;
			Code nc = next.OpCode.Code;
			switch (nc) {
			case Code.Box:		// generics
			case Code.Ldind_Ref:	// load indirect reference
				next = next.Next;
				nc = next.OpCode.Code;
				break;
			}

			if (null_compare.Get (nc)) {
				has_null_check.Set (parameter.GetSequence ());
			} else {
				// compare with null (next or previous to current instruction)
				// followed by a CEQ instruction
				if (nc == Code.Ldnull) {
					if (next.Next.OpCode.Code == Code.Ceq)
						has_null_check.Set (parameter.GetSequence ());
				} else if (nc == Code.Ceq) {
					if (ins.Previous.OpCode.Code == Code.Ldnull)
						has_null_check.Set (parameter.GetSequence ());
				}
			}
		}

		private void CheckCall (MethodDefinition method, Instruction ins)
		{
			MethodDefinition md = (ins.Operand as MethodReference).Resolve ();
			if (md == null)
				return;

			// no dereference possible by calling a static method
			if (!md.IsStatic) {
				Instruction instance = ins.TraceBack (method);
				CheckParameter (instance.GetParameter (method));
			}

			// if was pass one of our parameter to another call then
			// this new call "takes" responsability of the null check
			// note: not perfect but greatly reduce false positives
			if (!md.HasParameters)
				return;
			int base_index = md.IsStatic ? 0 : -1;
			for (int i = 0; i < md.Parameters.Count; i++) {
				Instruction pi = ins.TraceBack (method, base_index - i);
				if (pi == null)
					continue;
				// generic types will be be boxed, skip that
				if (pi.OpCode.Code == Code.Box)
					pi = pi.Previous;
				ParameterDefinition p = pi.GetParameter (method);
				if (p != null)
					has_null_check.Set (p.GetSequence ());
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// p/invoke, abstract methods and method without parameters
			if (!method.HasBody || !method.HasParameters || !method.IsVisible ())
				return RuleResult.DoesNotApply;

			has_null_check.ClearAll ();
			int parameters = method.Parameters.Count;

			// check
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.IsLoadArgument ()) {
					CheckArgument (method, ins);
				} else if (OpCodeBitmask.Calls.Get (ins.OpCode.Code)) {
					CheckCall (method, ins);
				} else if (check.Get (ins.OpCode.Code)) {
					Instruction owner = ins;
					// fields (no need to check static fields), ldind, ldelem, ldlen
					while ((owner != null) && check.Get (owner.OpCode.Code)) {
						owner = owner.TraceBack (method);
					}
					CheckParameter (owner.GetParameter (method));
				}

				// stop processing instructions once all parameters are validated
				if (has_null_check.Count () == parameters)
					break;
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
