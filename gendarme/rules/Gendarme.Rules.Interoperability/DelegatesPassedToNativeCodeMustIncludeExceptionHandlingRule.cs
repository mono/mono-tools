//
// Gendarme.Rules.Interoperability.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule
//
// Authors:
//	Rolf Bjarne Kvinge <RKvinge@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

//#define DEBUG
//#define LOG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule checks for delegates which are created for methods which don't
	/// have exception handling and then passed to native code.
	/// Every delegate which is passed to native code must include an exception
	/// block which spans the entire method and has a catch all block.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// delegate void Callback ();
	/// [DllImport ("mylibrary.dll")]
	/// static extern void RegisterCallback (Callback callback);
	/// public void RegisterManagedCallback ()
	/// {
	/// 	RegisterCallback (ManagedCallback);
	/// }
	/// public void ManagedCallback ()
	/// {
	/// 	// This will cause the process to crash if native code calls this method.
	/// 	throw new NotImplementedException ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// delegate void Callback ();
	/// [DllImport ("mylibrary.dll")]
	/// static extern void RegisterCallback (Callback callback);
	/// public void RegisterManagedCallback ()
	/// {
	/// 	RegisterCallback (ManagedCallback);
	/// }
	/// public void ManagedCallback ()
	/// {
	///	try {
	///		throw new NotImplementedException ();
	///	}
	///	catch {
	///		// This way the exception won't "leak" to native code
	///	}
	///	try {
	///		throw new NotImplementedException ();
	///	}
	///	catch (System.Exception ex) {
	///		// This is also safe - the runtime will process this catch clause,
	///		// even if it is technically possible (by writing assemblies in managed
	///		// C++ or IL) to throw an exception that doesn't inherit from
	///		// System.Exception.
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("Every delegate passed to native code must include an exception block which spans the entire method and has a catch all block.")]
	[Solution ("Surround the entire method body with a try/catch block.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule : Rule, IMethodRule {
		// A list of methods which have been verified to be safe to call from native code. 
		Dictionary<MethodDefinition, bool> verified_methods = new Dictionary<MethodDefinition, bool> ();
		
		// A list of methods which have been reported to be unsafe (to not report the same method twice).
		HashSet<MethodDefinition> reported_methods = new HashSet<MethodDefinition> ();
		
		// A list of all the fields which have been passed to pinvokes (as a delegate parameter).
		// We report an error if a ld[s]fld stores an unsafe function pointer into any of these fields.
		List<FieldReference> loaded_fields = new List<FieldReference> ();
		
		// A list of all the fields which have been assigned function pointers.
		// We report an error if a pinvoke loads any of these fields.
		Dictionary<FieldReference, List<MethodDefinition>> stored_fields = new Dictionary<FieldReference, List<MethodDefinition>> ();
				
		// A list of all the locals in a method. 
		// Have one class-level instance which is emptied before checking a method.
		// Avoids creating a new list instance for each method to check.
		List<List<MethodDefinition>> locals = new List<List<MethodDefinition>> ();
		
		// A list of ilranges a stack position corresponds to.
		// Avoids creating a new list instance for each method to check.
		List<ILRange> stack = new List<ILRange> ();
	
		// A list of one bool per instruction in a method indicating whether that instruction is safe in a method.
		// Avoids creating a new list instance for each method to check.
		List<bool> is_safe = new List<bool>();
		
		// A bitmask of safe instructions.
		static OpCodeBitmask safe_instructions = new OpCodeBitmask (0x22F7FFE7FFD, 0x48000000007F0000, 0x3C00000000000000, 0x45C80);
		
		// A bitmask of opcodes a method must have to be checked in CheckMethod.
		static OpCodeBitmask applicable_method_bitmask = new OpCodeBitmask (0x8000000000, 0x2400400000000000, 0x0, 0x0);

#if LOG
		static class Log 
		{
			public static bool IsEnabled (object o) { return true; }
			public static void WriteLine (object o, string msg, params object [] args)
			{
				Console.WriteLine (msg, args);
			}
			public static void WriteLine (object o, MethodDefinition m)
			{
				Console.WriteLine (new MethodPrinter (m).ToString ());
			}
		}
#endif

#if false
		static DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule ()
		{
			GenerateBitmasks ();
		}
		
		public static void GenerateBitmasks ()
		{
			OpCodeBitmask bitmask;
			
			bitmask = new OpCodeBitmask ();
			bitmask.Set (Code.Stfld);
			bitmask.Set (Code.Stsfld);
			bitmask.Set (Code.Call);
			bitmask.Set (Code.Callvirt);
			applicable_method_bitmask = bitmask;
			Console.WriteLine ("applicable_method_bitmask: {0}", bitmask);
			
			bitmask = new OpCodeBitmask ();
			// a list of safe instructions, which under no circumstances
			// (for verifiable code) can cause an exception to be raised.
			bitmask = new OpCodeBitmask ();
			bitmask.Set (Code.Nop);
			bitmask.Set (Code.Ret);
			bitmask.Set (Code.Ldloc);
			bitmask.Set (Code.Ldloc_0);
			bitmask.Set (Code.Ldloc_1);
			bitmask.Set (Code.Ldloc_2);
			bitmask.Set (Code.Ldloc_3);
			bitmask.Set (Code.Ldloc_S);
			bitmask.Set (Code.Ldloca);
			bitmask.Set (Code.Ldloca_S);
			// This one is needed to load static fields for return values 
			// a method which returns IntPtr should be able to ldsfld IntPtr.Zero
			bitmask.Set (Code.Ldsfld); 
			// bitmask.Set (Code.Ldsflda); // Not quite sure about this one, leaving it out for now.
			bitmask.Set (Code.Leave);
			bitmask.Set (Code.Leave_S);
			bitmask.Set (Code.Endfilter);
			bitmask.Set (Code.Endfinally);
			bitmask.Set (Code.Ldc_I4);
			bitmask.Set (Code.Ldc_I4_0);
			bitmask.Set (Code.Ldc_I4_1);
			bitmask.Set (Code.Ldc_I4_2);
			bitmask.Set (Code.Ldc_I4_3);
			bitmask.Set (Code.Ldc_I4_4);
			bitmask.Set (Code.Ldc_I4_5);
			bitmask.Set (Code.Ldc_I4_6);
			bitmask.Set (Code.Ldc_I4_7);
			bitmask.Set (Code.Ldc_I4_8);
			bitmask.Set (Code.Ldc_I4_M1);
			bitmask.Set (Code.Ldc_I8);
			bitmask.Set (Code.Ldc_R4);
			bitmask.Set (Code.Ldc_R8);
			bitmask.Set (Code.Ldarg);
			bitmask.Set (Code.Ldarg_0);
			bitmask.Set (Code.Ldarg_1);
			bitmask.Set (Code.Ldarg_2);
			bitmask.Set (Code.Ldarg_3);
			bitmask.Set (Code.Ldarg_S);
			bitmask.Set (Code.Stloc);
			bitmask.Set (Code.Stloc_0);
			bitmask.Set (Code.Stloc_1);
			bitmask.Set (Code.Stloc_2);
			bitmask.Set (Code.Stloc_3);
			bitmask.Set (Code.Stloc_S);
			bitmask.Set (Code.Stobj); /* Can throw TypeLoadException. This is required to properly initialize out parameters which are ValueTypes */
			bitmask.Set (Code.Ldnull);
			bitmask.Set (Code.Initobj);
			bitmask.Set (Code.Pop);
			// The stind* instructions can raise an exception:
			// "NullReferenceException is thrown if addr is not naturally aligned for the argument type implied by the instruction suffix."
			// Not sure how we can verify that the alignment is correct, and this instruction is emitted by the compiler for byref/out parameters.
			// Not marking this instructions as safe would mean that it would be impossible to fix a delegate that takes 
			// a byref/out parameter.
			bitmask.Set (Code.Stind_I);
			bitmask.Set (Code.Stind_I1);
			bitmask.Set (Code.Stind_I2);
			bitmask.Set (Code.Stind_I4);
			bitmask.Set (Code.Stind_I8);
			bitmask.Set (Code.Stind_R4);
			bitmask.Set (Code.Stind_R8);
			bitmask.Set (Code.Stind_Ref);
			safe_instructions = bitmask;
			Console.WriteLine ("safe_instructions: {0}", bitmask);			
		}
#endif

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// Rule does not apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			// 
			// We need to check all methods which has any of the following opcodes: call, stfld or stsfld.
			// 
			OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
			if (!applicable_method_bitmask.Intersect (bitmask))
				return RuleResult.DoesNotApply;

			/* Unfortunately it's possible to generate IL this rule will choke on, especially when 
			 * using non-standard compilers or obfuscators. */
			try {
				return CheckMethodUnsafe (method);
			} catch (Exception ex) {
				// FIXME: This problem should be reported some other way, it's not really a failure.
				// Pending implementation of "analysis warnings", as mentioned here (post #21):
				// http://groups.google.com/group/gendarme/browse_frm/thread/c37d157ae0c9682/57f89f3abf14f2fd?tvc=1&q=Gendarme+2.6+Preview+1+is+ready+for+download#57f89f3abf14f2fd
				Runner.Report (method, Severity.Low, Confidence.Low,
					String.Format (CultureInfo.CurrentCulture, "An exception occurred while verifying this method. " +
					"This failure can probably be ignored, it's most likely due to an " + 
					"uncommon code sequence in the method the rule didn't understand. {0}", ex.Message));
				return RuleResult.Failure;
			}
		}

		private RuleResult CheckMethodUnsafe (MethodDefinition method)
		{
			locals.Clear ();
			stack.Clear ();
			
			Log.WriteLine (this, "\n\nChecking method: {0} on type: {1}", method.Name, method.DeclaringType.GetFullName ());
			Log.WriteLine (this, method);

			MethodBody body = method.Body;
#if DEBUG
			foreach (ExceptionHandler e in body.ExceptionHandlers)
				Log.WriteLine (this, " HandlerType: {7}, TryStart: {4:X}, TryEnd: {5:X}, HandlerStart: {0:X}, HandlerEnd: {1:X}, FilterStart: {2:X}, FilterEnd: {3:X}, CatchType: {6}", 
				                   e.HandlerStart.GetOffset (), e.HandlerEnd.GetOffset (), e.FilterStart.GetOffset (), e.FilterEnd.GetOffset (), 
				                   e.TryStart.GetOffset (), e.TryEnd.GetOffset (), e.CatchType, e.HandlerType);
#endif
			
			//
			// We check the following code patterns:
			// - All delegate expressions created and passed directly to the pinvoke
			// - All delegate expressions created and stored in fields or local variables and then passed to the pinvoke.
			// 
			// To do the second item, we keep track of all reads from fields to pinvoke and stores to fields (with delegates)
			// and report an error if the delegate is unsafe in either case (i.e. we read from a field which has been 
			// assigned an unsafe delegate, or we store an unsafe delegate in a field which have been used in a pinvoke).
			// This way only one pass over the code is required.
			// 
			// No other static analysis is done, which means that for things like this we'll report false positives:
			//   delegate v = SafeMethod;
			//   PInvoke (v);
			//   v = UnsafeMethod;
			// 
			// 
			// There are several ways to get around this:
			// - Create a delegate, store it in a array/list and then pass it to a pinvoke.
			// - Create a delegate, return it from a function and use the return value directly in a call to a pinvoke.
			// - ...
			// 
			
			int stack_count = 0;
			foreach (Instruction ins in body.Instructions) {
				int pop = ins.GetPopCount (method);
				if (pop == -1) {
					// leave or leave.s, they leave the stack empty.
					stack_count = 0;
					stack.Clear ();
					continue; // No need to do anything else here.
				}
				
				if ((stack_count == 0) && body.HasExceptionHandlers) {
					foreach (ExceptionHandler eh in body.ExceptionHandlers) {
						if (eh.HandlerStart != null && eh.HandlerStart.Offset == ins.Offset) {
							// upon entry to a catch handler there is an implicit object on the stack already (the thrown object)
							stack_count = 1;
							stack.Add (null);
							break;
						}
					}
				}

				int push = ins.GetPushCount ();
				Log.WriteLine (this, " {0:X} {5} prev stack: {1}, pop: {2}, push: {3}, post stack: {4}", ins.Offset, stack_count, pop, push, stack_count + push - pop, ins.OpCode.Name);
			
				if (stack_count == 1 && stack [stack_count - 1] == null) {
					// Don't do anything, this is the implicit exception object passed to a catch handler.
				} else if (ins.OpCode.Code == Code.Call) {
					VerifyCallInstruction (ins);
				} else if (ins.OpCode.Code == Code.Stfld || ins.OpCode.Code == Code.Stsfld) {
					VerifyStoreFieldInstruction (ins, stack_count);
				} else if (ins.IsStoreLocal ()) {
					VerifyStoreLocalInstruction (ins, stack_count);
				} 
				
				stack_count += push - pop;
				
				while (stack_count > stack.Count)
					stack.Add (new ILRange (ins));
				while (stack_count < stack.Count)
					stack.RemoveAt (stack.Count - 1);

				if (stack_count > 0 && stack [stack_count - 1] != null)
					stack [stack_count - 1].Last = ins;
			}
			
			Log.WriteLine (this, "Checking method: {0} [Done], result: {1}", method.Name, Runner.CurrentRuleResult);
			
			return Runner.CurrentRuleResult;
		}
		
		private void VerifyStoreLocalInstruction (Instruction ins, int stack_count)
		{
			List<MethodDefinition> pointers;
			int index = ins.GetStoreIndex ();
			
			pointers = (stack_count <= 0) ? null : GetDelegatePointers (stack [stack_count - 1]);
			
			Log.WriteLine (this, " Reached a local variable store at offset {2:X}. index {0}, there are {1} pointers here.", index, pointers == null ? 0 : pointers.Count, ins.Offset);
			
			if (pointers != null && pointers.Count > 0) {
				while (locals.Count <= index)
					locals.Add (null);
				if (locals [index] == null)
					locals [index] = new List<MethodDefinition> ();
				locals [index].AddRange (pointers);
			}
		}
		
		private void VerifyStoreFieldInstruction (Instruction ins, int stack_count)
		{
			List<MethodDefinition> pointers;
			FieldReference field = (ins.Operand as FieldReference);
			
			pointers = (stack_count <= 0) ? null : GetDelegatePointers (stack [stack_count - 1]);
			
			Log.WriteLine (this, " Reached a field variable store to the field {0}, there are {1} unsafe pointers here.", field.Name, pointers == null ? 0 : pointers.Count);
#if DEBUG
			stack [stack_count - 1].DumpAll ("  ");
#endif
			
			if (pointers != null && pointers.Count > 0) {
				List<MethodDefinition> tmp;
				if (!stored_fields.TryGetValue (field, out tmp)) {
					tmp = new List<MethodDefinition> ();
					stored_fields.Add (field, tmp);
				}
				tmp.AddRange (pointers);
			}

			// If this field has been loaded into a pinvoke, 
			// check the list for unsafe pointers.
			if (loaded_fields.Contains (field))
				VerifyMethods (pointers);
		}
		
		private void VerifyCallInstruction (Instruction ins)
		{
			MethodDefinition called_method;
			IList<ParameterDefinition> parameters;

			called_method = (ins.Operand as MethodReference).Resolve ();
					
			if (called_method != null && called_method.IsPInvokeImpl && called_method.HasParameters) {
				Log.WriteLine (this, " Reached a call instruction to a pinvoke method: {0}", called_method.Name);

				parameters = called_method.Parameters;
				for (int i = 0; i < parameters.Count; i++) {
					if (stack [i] == null)
						continue;
					
					if (!parameters [i].ParameterType.IsDelegate ())
						continue;

					Log.WriteLine (this, " Parameter #{0} takes a delegate, stack expression:", i);
#if DEBUG
					stack [i].DumpAll ("  ");
#endif

					// if we load a field, store the field so that any subsequent unsafe writes to that field can be reported.
					Instruction last = stack [i].Last;
					if (last.OpCode.Code == Code.Ldfld || last.OpCode.Code == Code.Ldsfld) {
						FieldReference field = (last.Operand as FieldReference);
						loaded_fields.AddIfNew (field);
					}
					
					// Get and check the pointers
					VerifyMethods (GetDelegatePointers (stack [i]));
				}
			}
		}
		
		// Verifies that the method is safe to call as a callback from native code.
		private bool VerifyCallbackSafety (MethodDefinition callback)
		{
			bool result;
			bool valid_ex_handler;
			MethodBody body;
			IList<Instruction> instructions;

			if (callback == null)
				return true;

			Log.WriteLine (this, " Verifying: {0} with code size: {1} instruction count: {2}", callback.Name, callback.Body.CodeSize, callback.Body.Instructions.Count);

			if (!callback.HasBody)
				return true;
								
			if (verified_methods.TryGetValue (callback, out result))
				return result;
			
			body = callback.Body;
			instructions = body.Instructions;
			int icount = instructions.Count;
			is_safe.Clear ();
			is_safe.Capacity = icount;

			// 
			// We assume that the method is verifiable.
			// 
			
			// Mark all instructions corresponding to a safe opcode as safe, others are unsafe for now.
			for (int i = 0; i < icount; i++) {
				bool safe = safe_instructions.Get (instructions [i].OpCode.Code);
				Log.WriteLine (this, "  {0} {1}: {2}", i, instructions [i].OpCode.Code, safe);
				is_safe.Add (safe);
			}

			//
			// Mark code handled by a catch all block as safe.
			// 
			// Catch all block is any of the following:
			// a) A try block which does not specify an exception type
			// b) A try block which specifies System.Exception as the exception type.
			// 
			// The second case will break if both of the following happens:
			// 1) An exception is thrown which does not inherit from System.Exception (not valid in C# nor VB, but it is valid in C++/CIL and IL)
			// 2) The assembly where the exception handler resides has the System.Runtime.CompilerServices.RuntimeCompatibility attribute set with WrapNonExceptionThrows = true.
			// 
			// If 2) is not true, the runtime will wrap the exception in a RuntimeWrappedException object, which is handled by case b) above.
			// Given that this is the normal case (otherwise you'd have to put the attribute in the assembly), we accept 2) as safe too.
			// 
	
			if (body.HasExceptionHandlers) {
				foreach (ExceptionHandler eh in body.ExceptionHandlers) {
					// We only care about catch clauses.
					if (eh.HandlerType != ExceptionHandlerType.Catch)
						continue;
					
					// no 'Catch ... When <condition>' clause. C# doesn't support it, VB does
					if (eh.FilterStart != null || eh.FilterEnd != null)
						continue;
					
					// check for catch all clauses
					TypeReference ctype = eh.CatchType;
					if (!(ctype == null || ctype.IsNamed ("System", "Object") || ctype.IsNamed ("System", "Exception")))
						continue;
					
					// Mark the code this exception handler handles as safe.
					int start_index = instructions.IndexOf (eh.TryStart);
					int end_index = instructions.IndexOf (eh.TryEnd);
					Log.WriteLine (this, " Catch all block found, marking instruction at index {0} to index {1} (included) as safe.", start_index, end_index - 1);
					for (int j = start_index; j < end_index; j++)
						is_safe [j] = true;
				}
			}
			
			// Check that all instructions have been marked as safe, otherwise mark the method as unsafe.
			valid_ex_handler = !is_safe.Contains (false);

#if DEBUG
			Log.WriteLine (this, " Method {0} verified: {1}.", callback.Name, valid_ex_handler);
			for (int i = 0; i < is_safe.Count; i++) {
				// Console.ForegroundColor = safe [i] ? ConsoleColor.DarkGreen : ConsoleColor.Red;
				Log.WriteLine (this, " {1} {0}", instructions [i].ToPrettyString (), is_safe [i] ? "Y" : "N");
				// Console.ResetColor ();
			}
			foreach (ExceptionHandler e in body.ExceptionHandlers)
				Log.WriteLine (this, " HandlerType: {7}, TryStart: {4}, TryEnd: {5}, HandlerStart: {0}, HandlerEnd: {1}, FilterStart: {2}, FilterEnd: {3}, CatchType: {6}", 
				                   e.HandlerStart.GetOffset (), e.HandlerEnd.GetOffset (), e.FilterStart.GetOffset (), e.FilterEnd.GetOffset (), 
				                   e.TryStart.GetOffset (), e.TryEnd.GetOffset (), e.CatchType, e.HandlerType);
#endif
			
			verified_methods.Add (callback, valid_ex_handler);
				
			return valid_ex_handler;
		}
				
		private sealed class ILRange {
			public Instruction First;
			public Instruction Last;
			public ILRange (Instruction first)
			{
				First = first;
			}
#if DEBUG
			public void DumpAll (string prefix)
			{
				if (!Log.IsEnabled (this.GetType ().Name))
					return;
				
				Instruction instr = First;
				do {
					Log.WriteLine (this, "{0}{1}", prefix, instr.ToPrettyString ());
					if (instr == Last)
						break;
					instr = instr.Next;
				} while (true);
			}
#endif
		}
		
		// Parses the ILRange and return all delegate pointers that could end up on the stack as a result of executing that code.
		private List<MethodDefinition> GetDelegatePointers (ILRange range)
		{
			List<MethodDefinition> result = null;
			MethodReference ldftn;
			MethodDefinition ldftn_definition;
			
			// Check if the code does any ldftn.
			if (range.First != range.Last && range.Last.OpCode.Code == Code.Newobj && range.Last.Previous.OpCode.Code == Code.Ldftn) {
				ldftn = range.Last.Previous.Operand as MethodReference;
				if (ldftn != null) {
					ldftn_definition = ldftn.Resolve ();
					if (ldftn_definition != null) {
						if (result == null)
							result = new List<MethodDefinition> ();
						result.Add (ldftn_definition);
					}
				}
			}
			
			// Check if the code loads any local variables which can be delegates.
			if (locals != null && range.Last.IsLoadLocal ()) {
				int index = range.Last.GetLoadIndex ();
				if (locals.Count > index) {
					List<MethodDefinition> pointers = locals [index];
					if (pointers != null && pointers.Count != 0) {
						if (result == null)
							result = new List<MethodDefinition> ();
						result.AddRange (pointers);
					}
				}
			}
			
			// If the last opcode is a field load, check if any pointers have been stored in that field.
			if (stored_fields.Count > 0 && range.Last.OpCode.Code == Code.Ldfld || range.Last.OpCode.Code == Code.Ldsfld) {
				FieldReference field = range.Last.Operand as FieldReference;
				List<MethodDefinition> pointers;
				if (field != null) {
					if (stored_fields.TryGetValue (field, out pointers)) {
						if (result == null)
							result = new List<MethodDefinition> ();
						result.AddRange (pointers);
					}
				}
			}
			
			return result;
		}
		
		// Verifies that all methods in the list are safe to call from native code,
		// otherwise reports the corresponding result.
		private void VerifyMethods (List<MethodDefinition> pointers)
		{
			Log.WriteLine (this, " Verifying {0} method pointers.", pointers == null ? 0 : pointers.Count);
			
			if (pointers == null)
				return;
			
			for (int i = 0; i < pointers.Count; i++)
				ReportVerifiedMethod (pointers [i], VerifyCallbackSafety (pointers [i]));
		}
		
		// Reports the result from verifying the method.
		private void ReportVerifiedMethod (MethodDefinition pointer, bool safe)
		{
			if (!safe) {
				if (reported_methods.Contains (pointer))
					return;

				reported_methods.Add (pointer);
				
				Log.WriteLine (this, " Reporting: {0}", pointer.Name);
				Runner.Report (pointer, Severity.High, Confidence.High);
			} else {
				Log.WriteLine (this, " Safe: {0}", pointer.Name);
			}
		}
	}
	
	internal static class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRuleHelper {
#if DEBUG
		public static string ToPrettyString (this Instruction instr)
		{
			if (instr == null)
				return "<nil>";
			
			return string.Format ("IL_{0} {1} {2}", instr.Offset, instr.OpCode.Name, instr.Operand);
		}
		
		public static int GetOffset (this Instruction instr)
		{
			if (instr != null)
				return instr.Offset;
			return -1;
		}
#endif
		// Return the index of the load opcode.
		// This could probably go into InstructionRocks.
		public static int GetLoadIndex (this Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldloc_0: return 0;
			case Code.Ldloc_1: return 1;
			case Code.Ldloc_2: return 2;
			case Code.Ldloc_3: return 3;
			case Code.Ldloc:  // Untested 
			case Code.Ldloca: // Untested
			case Code.Ldloca_S:
			case Code.Ldloc_S: return ((VariableDefinition) ins.Operand).Index;
			default:
				string msg = String.Format (CultureInfo.InvariantCulture, "Invalid opcode: {0}", ins.OpCode.Name);
				throw new ArgumentException (msg);
			}
		}
		
		// Return the index of the store opcode.
		// This could probably go into InstructionRocks.
		public static int GetStoreIndex (this Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Stloc_0: return 0;
			case Code.Stloc_1: return 1;
			case Code.Stloc_2: return 2;
			case Code.Stloc_3: return 3;
			case Code.Stloc: // Untested for stloc
			case Code.Stloc_S: return ((VariableDefinition) ins.Operand).Index;
			default:
				string msg = String.Format (CultureInfo.InvariantCulture, "Invalid opcode: {0}", ins.OpCode.Name);
				throw new ArgumentException (msg);
			}
		}
	}
}
