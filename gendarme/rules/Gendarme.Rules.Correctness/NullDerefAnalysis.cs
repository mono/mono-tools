/*
 * NullDerefAnalysis.cs: dataflow analysis details for null-pointer
 * dereference detection.
 *
 * Authors:
 *   Aaron Tomb <atomb@soe.ucsc.edu>
 *
 * Copyright (c) 2005 Aaron Tomb and the contributors listed
 * in the ChangeLog.
 *
 * This is free software, distributed under the MIT/X11 license.
 * See the included LICENSE.MIT file for details.
 **********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Correctness {

	public class NullDerefAnalysis : IDataflowAnalysis {

		int stackDepth;
		int locals;
		int args;
		[NonNull] private MethodDefinition method;
		[NonNull] private NonNullAttributeCollector nnaCollector;
		[NonNull] private IRunner runner;

		public NullDerefAnalysis([NonNull] MethodDefinition method,
				[NonNull] NonNullAttributeCollector nnaCollector,
				[NonNull] IRunner runner)
		{
			this.stackDepth = method.Body.MaxStack;
			this.locals = method.Body.Variables.Count;
			if(method.HasThis)
				this.args = method.Parameters.Count + 1;
			else
				this.args = method.Parameters.Count;
			this.method = method;
			this.nnaCollector = nnaCollector;
			this.runner = runner;
		}
		
		public bool Verbose { get; set; }

		[NonNull]
		public object NewTop()
		{
			return new NullDerefFrame(stackDepth, locals, args, false, runner);
		}

		[NonNull]
		public object NewEntry()
		{
			NullDerefFrame result =
				new NullDerefFrame(stackDepth, locals, args, true, runner);
			if(method.HasThis)
				result.SetArgNullity(0, Nullity.NonNull);
			foreach(ParameterDefinition param in method.Parameters)
				if(nnaCollector.HasNonNullAttribute(method, param))
					result.SetArgNullity(param.GetSequence () - 1, Nullity.NonNull);
			return result;
		}

		[NonNull]
		public object NewCatch()
		{
			NullDerefFrame result =
				new NullDerefFrame(stackDepth, locals, args, true, runner);
			if(method.HasThis)
				result.SetArgNullity(0, Nullity.NonNull);
			foreach(ParameterDefinition param in method.Parameters)
				if(nnaCollector.HasNonNullAttribute(method, param))
					result.SetArgNullity(param.GetSequence () - 1, Nullity.NonNull);
			/* The exception being caught is pushed onto the stack. */
			result.PushStack(Nullity.NonNull);
			return result;
		}

		/* Changes originalFact. */
		public void MeetInto([NonNull] object originalFact,
				[NonNull] object newFact, bool warn)
		{
			NullDerefFrame original = (NullDerefFrame)originalFact;
			NullDerefFrame incoming = (NullDerefFrame)newFact;
			original.MergeWith(incoming);
		}

		private static bool IsVoid([NonNull] TypeReference type)
		{
			if(type.FullName.Equals("System.Void"))
				return true;
			return false;
		}

		// FIXME: could probably rewrite this to be more reliable using
		// OpCode.StackBehaviourPop and StackBehaviourPush
		//
		// FIXME: This code is simply too naive to work well in the real world.
		// For example, code that compares a local to null will not work
		// correctly (we want the local to be null along one branch and non-
		// null along the other). But this is tricky to do with the textbook
		// algorithm (note that meet must be commutative). One fix is to 
		// splice in synthetic blocks and fix the code so that it can handle
		// zero length blocks.
		public void Transfer([NonNull] Node node, [NonNull] object inFact,
				[NonNull] object outFact, bool warn)
		{
			BasicBlock bb = (BasicBlock)node;

			/* Exit and exception nodes don't cover any real instructions. */
			if(bb.isExit || bb.isException)
				return;

			//NullDerefFrame inFrame = (NullDerefFrame)inFact;
			NullDerefFrame outFrame = (NullDerefFrame)outFact;
			VariableDefinitionCollection vars = method.Body.Variables;

			if (Verbose) {
				Trace.WriteLine (string.Empty);
				Trace.WriteLine (string.Format("Basic block {0}", bb.ToString ()));
				Trace.WriteLine ("Input frame:");
				Trace.Write (outFrame.ToString ());
			}

			for(int i = bb.first; i <= bb.last; i++) {
				Instruction insn = bb.Instructions[i];
				OpCode opcode = insn.OpCode;

			if (Verbose) {
				Trace.Write (string.Format ("   {0}", opcode.Name));
					if (insn.Operand != null && !(insn.Operand is Instruction)) {
						Trace.WriteLine (string.Format (" {0}", insn.Operand.ToString ()));
					} else if (insn.Operand is Instruction) {
						Trace.WriteLine (string.Format (" {0}",
								((Instruction)insn.Operand).Offset.ToString ("X4")));
					} else {
						Trace.WriteLine (string.Empty);
					}
				}

				switch (opcode.Code) {
					/* Load argument */
					/* Stored nullities are set to declared values on method
					 * entry. Starg and kin can change this over time. */
					case Code.Ldarg_0:
						outFrame.PushStack(outFrame.GetArgNullity(0));
						break;
			case Code.Ldarg_1:
						outFrame.PushStack(outFrame.GetArgNullity(1));
						break;
			case Code.Ldarg_2:
						outFrame.PushStack(outFrame.GetArgNullity(2));
						break;
			case Code.Ldarg_3:
						outFrame.PushStack(outFrame.GetArgNullity(3));
						break;
			case Code.Ldarg:
				if (insn.Operand is int) {
				outFrame.PushStack (outFrame.GetArgNullity ((int) insn.Operand));
				} else if (insn.Operand is ParameterDefinition) {
				ParameterDefinition pd = (insn.Operand as ParameterDefinition);
				outFrame.PushStack ((pd.HasConstant && pd.Constant == null) ? 
					Nullity.Null : Nullity.NonNull);
				} else {
				outFrame.PushStack(Nullity.NonNull);
				}
						break;
			case Code.Ldarg_S: {
						ParameterDefinition param =
							(ParameterDefinition)insn.Operand;
						outFrame.PushStack(
								outFrame.GetArgNullity(param.GetSequence () - 1));
						break;
					}
			case Code.Ldarga:
			case Code.Ldarga_S:
						outFrame.PushStack(Nullity.NonNull);
						break;
					/* Store argument */
			case Code.Starg:
						outFrame.SetArgNullity((int)insn.Operand,
								outFrame.PopStack());
						break;
			case Code.Starg_S: {
						ParameterDefinition param =
							(ParameterDefinition)insn.Operand;
						outFrame.SetArgNullity(param.GetSequence () - 1,
								outFrame.PopStack());
						break;
					}

					/* Load local */
			case Code.Ldloc_0:
						outFrame.PushStack(outFrame.GetLocNullity(0));
						break;
			case Code.Ldloc_1:
						outFrame.PushStack(outFrame.GetLocNullity(1));
						break;
			case Code.Ldloc_2:
						outFrame.PushStack(outFrame.GetLocNullity(2));
						break;
			case Code.Ldloc_3:
						outFrame.PushStack(outFrame.GetLocNullity(3));
						break;
			case Code.Ldloc:
			case Code.Ldloc_S:
						outFrame.PushStack(outFrame.GetLocNullity(
							vars.IndexOf((VariableDefinition)insn.Operand)));
						break;
			case Code.Ldloca:
			case Code.Ldloca_S:
						outFrame.SetLocNullity(
							vars.IndexOf((VariableDefinition)insn.Operand),
							Nullity.Unknown);
						outFrame.PushStack(Nullity.NonNull);
						break;

					/* Store local */
			case Code.Stloc_0:
						outFrame.SetLocNullity(0, outFrame.PopStack());
						break;
			case Code.Stloc_1:
						outFrame.SetLocNullity(1, outFrame.PopStack());
						break;
			case Code.Stloc_2:
						outFrame.SetLocNullity(2, outFrame.PopStack());
						break;
			case Code.Stloc_3:
						outFrame.SetLocNullity(3, outFrame.PopStack());
						break;
			case Code.Stloc:
			case Code.Stloc_S:
						outFrame.SetLocNullity(
							vars.IndexOf((VariableDefinition)insn.Operand),
							outFrame.PopStack());
						break;

					/* Load other things */
			case Code.Ldftn:
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Ldvirtftn:
						outFrame.PopStack();
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Ldstr:
			case Code.Ldnull:
						outFrame.PushStack(Nullity.Null);
						break;
			case Code.Ldlen:
						outFrame.PopStack();
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Ldtoken:
						outFrame.PushStack(Nullity.NonNull);
						break;

					 /* Object operations */
			case Code.Cpobj:
				outFrame.PopStack (2);
				break;
			case Code.Newobj:
						outFrame.PopStack(
							((MethodReference)insn.Operand).Parameters.Count);
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Ldobj:
						outFrame.PopStack();
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Stobj:
				outFrame.PopStack (2);
				break;
			case Code.Initobj:
				outFrame.PopStack ();
				break;

					 /* Load field */
			case Code.Ldfld: {
						Check(insn, warn, outFrame.PopStack(), "field");
						FieldReference field = (FieldReference)insn.Operand;
						if(nnaCollector.HasNonNullAttribute(field))
							outFrame.PushStack(Nullity.NonNull);
						else
							outFrame.PushStack(Nullity.Unknown);
						break;
					}
			case Code.Ldflda:
						Check(insn, warn, outFrame.PopStack(), "field");
						outFrame.PushStack(Nullity.NonNull);
						break;
			case Code.Ldsfld: {
						FieldReference field = (FieldReference)insn.Operand;
						if(nnaCollector.HasNonNullAttribute(field))
							outFrame.PushStack(Nullity.NonNull);
						else
							outFrame.PushStack(Nullity.Unknown);
						break;
					}
			case Code.Ldsflda:
				outFrame.PushStack (Nullity.NonNull);
				break;

					/* Store field */
			case Code.Stfld: {
				/* FIXME: warn if writing null to non-null field */
				Nullity n = outFrame.PopStack ();
				Check (insn, warn, outFrame.PopStack(), "field");
				FieldReference field = (FieldReference)insn.Operand;
				if (warn && nnaCollector.HasNonNullAttribute (field)) {
					if (Verbose)
						Trace.WriteLine (string.Format ("FAILURE1: null deref at {0:X2}", insn.Offset));
					if (n == Nullity.Unknown)
						runner.Report (method, insn, Severity.High, Confidence.Low, "storing possibly null value in field declared non-null");
					else if (n == Nullity.Null)
						runner.Report (method, insn, Severity.High, Confidence.Low, "storing null value in field declared non-null");
				}
				break;
			}
			case Code.Stsfld: {
				Nullity n = outFrame.PopStack ();
				FieldReference field = (FieldReference)insn.Operand;
				if (warn && nnaCollector.HasNonNullAttribute (field)) {
					if (Verbose)
						Trace.WriteLine (string.Format ("FAILURE2: null deref at {0:X2}", insn.Offset));
					if (n == Nullity.Unknown)
						runner.Report (method, insn, Severity.High, Confidence.Low, "storing possibly null value in field declared non-null");
					else if (n == Nullity.Null)
						runner.Report (method, insn, Severity.High, Confidence.Low, "storing null value in field declared non-null");
				}
				break;
			}

			/* Stack operations */
			case Code.Dup:
				outFrame.PushStack (outFrame.PeekStack ());
				break;
			case Code.Pop:
				outFrame.PopStack ();
				break;

			/* Method call and return */
			case Code.Calli:
				ProcessCall (insn, warn, true, outFrame);
				break;
			case Code.Call:
			case Code.Callvirt:
				ProcessCall (insn, warn, false, outFrame);
				break;
			case Code.Ret:
				if(!IsVoid(method.ReturnType)) {
					Nullity n = outFrame.PopStack();
					if(nnaCollector.HasNonNullAttribute(method) && warn) {
						if (Verbose)
							Trace.WriteLine (string.Format ("FAILURE3: null deref at {0:X2}", insn.Offset));
						if(n == Nullity.Null)
							runner.Report (method, insn, Severity.High, Confidence.Low, "returning null value from method declared non-null");
						else
							runner.Report (method, insn, Severity.High, Confidence.Low, "returning possibly null value from method declared non-null");
					}
				}
				break;

			/* Indirect load */
			case Code.Ldind_I1:
			case Code.Ldind_U1:
			case Code.Ldind_I2:
			case Code.Ldind_U2:
			case Code.Ldind_I4:
			case Code.Ldind_U4:
			case Code.Ldind_I8:
			case Code.Ldind_I:
			case Code.Ldind_R4:
			case Code.Ldind_R8:
			case Code.Ldind_Ref:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.Unknown);
				break;

			/* Indirect store */
			case Code.Stind_Ref:
			case Code.Stind_I:
			case Code.Stind_I1:
			case Code.Stind_I2:
			case Code.Stind_I4:
			case Code.Stind_I8:
			case Code.Stind_R4:
			case Code.Stind_R8:
				outFrame.PopStack (2);
				break;

			/* Class-related operations */
			case Code.Box:
			case Code.Unbox:
			case Code.Unbox_Any:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.NonNull);
				break;
			case Code.Castclass:
			case Code.Isinst:
				break;

			/* Exception handling */	
			case Code.Rethrow:
			case Code.Endfinally:
				break;
			case Code.Throw:
			case Code.Endfilter:
				outFrame.PopStack ();
				break;

			case Code.Leave:
			case Code.Leave_S:
				outFrame.EmptyStack ();
				break;

			/* Array operations */
			case Code.Newarr:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.NonNull);
				break;

			/* Load element */
			case Code.Ldelema:
			case Code.Ldelem_I1:
			case Code.Ldelem_U1:
			case Code.Ldelem_I2:
			case Code.Ldelem_U2:
			case Code.Ldelem_I4:
			case Code.Ldelem_U4:
			case Code.Ldelem_I8:
			case Code.Ldelem_I:
			case Code.Ldelem_R4:
			case Code.Ldelem_R8:
				outFrame.PopStack(2);
				outFrame.PushStack(Nullity.NonNull);
				break;
			case Code.Ldelem_Ref:
			case Code.Ldelem_Any: /* This may or may not be a reference. */
				outFrame.PopStack(2);
				outFrame.PushStack(Nullity.Unknown);
				break;
			/* Store element */
			/* Pop 3 */
			case Code.Stelem_I:
			case Code.Stelem_I1:
			case Code.Stelem_I2:
			case Code.Stelem_I4:
			case Code.Stelem_I8:
			case Code.Stelem_R4:
			case Code.Stelem_R8:
			case Code.Stelem_Ref:
			case Code.Stelem_Any:
				outFrame.PopStack (3);
				break;

			case Code.Arglist:
			case Code.Sizeof:
				outFrame.PushStack(Nullity.NonNull);
				break;
			case Code.Mkrefany:
			case Code.Refanyval:
			case Code.Refanytype:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.NonNull);
				break;

			/* Prefixes */
			case Code.Unaligned:
			case Code.Volatile:
			case Code.Tail:
				break;

			/* Effect-free instructions */
			case Code.Nop:
			case Code.Break:
				break;

			/* Load constant */
			/* Push non-ref. */
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
			case Code.Ldc_I8:
			case Code.Ldc_R4:
			case Code.Ldc_R8:
				outFrame.PushStack (Nullity.NonNull);
				break;

			/* Unconditional control flow */
			/* Do nothing */
			case Code.Br:
			case Code.Br_S:
				break;

			/* Conditional branches */
			/* Pop 1 */
			case Code.Brfalse:
			case Code.Brtrue:
			case Code.Brfalse_S:
			case Code.Brtrue_S:
				outFrame.PopStack ();
				break;

			/* Comparison branches */
			/* Pop 2. */
			case Code.Beq:
			case Code.Bge:
			case Code.Bgt:
			case Code.Ble:
			case Code.Blt:
			case Code.Bne_Un:
			case Code.Bge_Un:
			case Code.Bgt_Un:
			case Code.Ble_Un:
			case Code.Blt_Un:
			case Code.Beq_S:
			case Code.Bge_S:
			case Code.Bgt_S:
			case Code.Ble_S:
			case Code.Blt_S:
			case Code.Bne_Un_S:
			case Code.Bge_Un_S:
			case Code.Bgt_Un_S:
			case Code.Ble_Un_S:
			case Code.Blt_Un_S:
				outFrame.PopStack (2);
				break;

			case Code.Switch:
				outFrame.PopStack();
				break;

			/* Comparisons */
			/* Pop 2, push non-ref */
			case Code.Ceq:
			case Code.Cgt:
			case Code.Cgt_Un:
			case Code.Clt:
			case Code.Clt_Un:
				outFrame.PopStack(2);
				outFrame.PushStack(Nullity.NonNull);
				break;

			/* Arithmetic and logical binary operators */
			/* Pop 2, push non-ref */
			case Code.Add:
			case Code.Sub:
			case Code.Mul:
			case Code.Div:
			case Code.Div_Un:
			case Code.Rem:
			case Code.Rem_Un:
			case Code.And:
			case Code.Or:
			case Code.Xor:
			case Code.Shl:
			case Code.Shr:
			case Code.Shr_Un:
			case Code.Add_Ovf:
			case Code.Add_Ovf_Un:
			case Code.Mul_Ovf:
			case Code.Mul_Ovf_Un:
			case Code.Sub_Ovf:
			case Code.Sub_Ovf_Un:
				outFrame.PopStack(2);
				outFrame.PushStack(Nullity.NonNull);
				break;

			/* Arithmetic and logical unary operators */
			/* Pop 1, push non-ref */
			case Code.Neg:
			case Code.Not:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.NonNull);
				break;

			/* Conversions. */
			/* Do nothing. */
			case Code.Conv_I1:
			case Code.Conv_I2:
			case Code.Conv_I4:
			case Code.Conv_I8:
			case Code.Conv_R4:
			case Code.Conv_R8:
			case Code.Conv_U4:
			case Code.Conv_U8:
			case Code.Conv_U:
			case Code.Conv_R_Un:
			case Code.Conv_Ovf_I1_Un:
			case Code.Conv_Ovf_I2_Un:
			case Code.Conv_Ovf_I4_Un:
			case Code.Conv_Ovf_I8_Un:
			case Code.Conv_Ovf_U1_Un:
			case Code.Conv_Ovf_U2_Un:
			case Code.Conv_Ovf_U4_Un:
			case Code.Conv_Ovf_U8_Un:
			case Code.Conv_Ovf_I_Un:
			case Code.Conv_Ovf_U_Un:
			case Code.Conv_Ovf_I1:
			case Code.Conv_Ovf_U1:
			case Code.Conv_Ovf_I2:
			case Code.Conv_Ovf_U2:
			case Code.Conv_Ovf_I4:
			case Code.Conv_Ovf_U4:
			case Code.Conv_Ovf_I8:
			case Code.Conv_Ovf_U8:
			case Code.Conv_U2:
			case Code.Conv_U1:
			case Code.Conv_I:
			case Code.Conv_Ovf_I:
			case Code.Conv_Ovf_U:
			case Code.Ckfinite:
				break;

			/* Unverifiable instructions. */
			case Code.Jmp:
				break;
			case Code.Cpblk:
				outFrame.PopStack (3);
				break;
			case Code.Initblk:
				outFrame.PopStack (3);
				break;
			case Code.Localloc:
				outFrame.PopStack();
				outFrame.PushStack(Nullity.NonNull);
				break;

			default:
				Trace.WriteLine (string.Format ("Unknown instruction: {0} {1}",
						opcode.Name, opcode.Value.ToString("X4")));
				break;
				} /* switch */
			} /* for */

			if (Verbose) {
				Trace.WriteLine ("Output frame:");
				Trace.Write (outFrame.ToString ());
			}
		} /* Transfer */

		private void Check([NonNull]Instruction insn, bool warn, Nullity n,
				[NonNull] string type)
		{
			if (!warn)
				return;

			string name = insn.Operand.ToString();
			int nameOffset = name.LastIndexOf("::");
			if(nameOffset != -1)
				name = name.Substring(nameOffset + 2);
			if(type.Equals("method")) {
				string prefix = name.Substring(0, 4);
				if(prefix.Equals("get_") || prefix.Equals("set_")) {
					name = name.Substring(4);
					type = "property";
				}
			}
			if(n == Nullity.Null) {
				if (Verbose)
					Trace.WriteLine (string.Format ("FAILURE5: null deref at {0:X2}", insn.Offset));
				string s = String.Format ("accessing {0} {1} from null object", type, name);
				runner.Report (method, insn, Severity.High, Confidence.Low, s);
			}
		}

		private void ProcessCall ([NonNull] Instruction insn, bool warn, bool indirect, [NonNull] NullDerefFrame frame)
		{
			IMethodSignature csig = (IMethodSignature)insn.Operand;
			if(indirect)
				frame.PopStack(); /* Function pointer */
			foreach(ParameterDefinition param in csig.Parameters) {
				Nullity n = frame.PopStack();
				if(warn && nnaCollector.HasNonNullAttribute(method, param)) {
					if (Verbose)
						Trace.WriteLine (string.Format ("FAILURE6: null deref at {0:X2}", insn.Offset));
					if(n == Nullity.Null) 
						runner.Report (method, insn, Severity.High, Confidence.Low, "passing null value as argument declared non-null");
					else if(n == Nullity.Unknown)
						runner.Report (method, insn, Severity.High, Confidence.Low, "passing possibly null value as argument declared non-null");
				}
			}
			if(csig.HasThis && !Ignoring(csig)) /* Add 'this' parameter. */
				Check(insn, warn, frame.PopStack(), "method");
			if(!IsVoid(csig.ReturnType)) {
				if(csig.ReturnType.IsValueType)
					frame.PushStack(Nullity.NonNull);
				else if(nnaCollector.HasNonNullAttribute(csig))
					frame.PushStack(Nullity.NonNull);
				else
					frame.PushStack(Nullity.Unknown);
			}
		}

		private static bool Ignoring([NonNull] IMethodSignature msig)
		{
			/* FIXME: Ignoring is a temporary hack! */
			/* Right now, it always returns false, as it should. */
			return false;
		}
	}
}
