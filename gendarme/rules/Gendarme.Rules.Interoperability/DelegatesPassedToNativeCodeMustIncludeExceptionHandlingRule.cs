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

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// Every delegate which is passed to native code must include an exception 
	/// block which spans the entire method and has a catch all handler.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void NativeCallback ()
	/// {
	///	Console.WriteLine ("{0}", 1);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void NativeCallback ()
	/// {
	///	try {
	///		Console.WriteLine ("{0}", 1);
	///	}
	///	catch {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("Every delegate passed to native code must include an exception block which spans the entire method and has a catch all handler.")]
	[Solution ("Surround the entire method body with a try/catch block.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule : Rule, IMethodRule {
		// A list of methods which have been verified to be safe to call from native code. 
		Dictionary<MethodDefinition, bool> verified_methods;
		
		// A list of methods which have been reported to be unsafe (to not report the same method twice).
		List<MethodDefinition> reported_methods;
		
		// A list of all the fields which have been passed to pinvokes (as a delegate parameter).
		// We report an error if a ld[s]fld stores an unsafe function pointer into any of these fields.
		List<FieldDefinition> fields_loads;
		
		// A list of all the fields which have been assigned function pointers.
		// We report an error if a pinvoke loads any of these fields.
		Dictionary<FieldDefinition, List<MethodDefinition>> field_stores;
				
		public RuleResult CheckMethod (MethodDefinition method)
		{
			MethodDefinition called_method;
			List<MethodDefinition> pointers;

			// Rule does not apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			List<List<MethodDefinition>> locals = new List<List<MethodDefinition>> ();
			List<ILRange> stack = new List<ILRange> (); // A rude way of assigning code sequences to stack positions.
			
			// Console.WriteLine ("Checking method: {0} on type: {1}", method.Name, method.DeclaringType.FullName);
			
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
			foreach (Instruction ins in method.Body.Instructions) {
				int pop = ins.GetPopCount (method);
				if (pop == -1) {
					// leave or leave.s, they leave the stack empty.
					stack_count = 0;
					stack.Clear ();
					continue; // No need to do anything else here.
				}
				
				if ((stack_count == 0) && method.Body.HasExceptionHandlers) {
					foreach (ExceptionHandler eh in method.Body.ExceptionHandlers) {
						if (eh.HandlerStart != null && eh.HandlerStart.Offset == ins.Offset) {
							// upon entry to a catch handler there is an implicit object on the stack already (the thrown object)
							stack_count = 1;
							stack.Add (null);
							break;
						}
					}
				}

				int push = ins.GetPushCount ();
				// Console.WriteLine ("before {0} + push {1} - pop {2} = after {3} {4}", stack_count, push, pop, stack_count + push - pop, ToString (ins));
				// Console.WriteLine ("{4}", stack_count, push, pop, stack_count + push - pop, ToString (ins));
			
				if (stack_count == 1 && stack [stack_count - 1] == null) {
					// Don't do anything, this is the implicit exception object passed to a catch handler.
				} else if (ins.OpCode.Code == Code.Call) {
					called_method = (ins.Operand as MethodReference).Resolve ();
					
					if (called_method != null && called_method.IsPInvokeImpl && called_method.HasParameters) {
						// Console.WriteLine ("Reached a call instruction to a pinvoke method: {0}", called_method.Name);
						
						for (int i = 0; i < called_method.Parameters.Count; i++) {
							if (!called_method.Parameters [i].ParameterType.IsDelegate ())
								continue;
#if DEBUG
							Console.WriteLine (" Stack slot #{0}:", i);
							stack [i].DumpAll ("  ");
#endif
							// if we load a field, store the field so that any subsequent unsafe writes to that field can be reported.
							Instruction last = stack [i].Last;
							if (last.OpCode.Code == Code.Ldfld || last.OpCode.Code == Code.Ldsfld) {
								FieldDefinition field = (last.Operand as FieldReference).Resolve ();
								if (fields_loads == null)
									fields_loads = new List<FieldDefinition> ();
								fields_loads.AddIfNew (field);
							}
							
							// Get and check the pointers
							VerifyMethods (GetDelegatePointers (locals, stack [i]));
						}
					}
				} else if (ins.OpCode.Code == Code.Stfld || ins.OpCode.Code == Code.Stsfld) {
					FieldDefinition field = (ins.Operand as FieldReference).Resolve ();
					
					pointers = (stack_count <= 0) ? null : GetDelegatePointers (locals, stack [stack_count - 1]);
					
					// Console.WriteLine (" Reached a field variable store to the field {0}, there are {1} unsafe pointers here.", field.Name, pointers == null ? 0 : pointers.Count);
					
					if (pointers != null && pointers.Count > 0) {
						List<MethodDefinition> tmp;
						if (field_stores == null) {
							field_stores = new Dictionary<FieldDefinition, List<MethodDefinition>> ();
							tmp = new List<MethodDefinition> ();
							field_stores.Add (field, tmp);
						} else if (!field_stores.TryGetValue (field, out tmp)) {
							tmp = new List<MethodDefinition> ();
							field_stores.Add (field, tmp);
						}
						tmp.AddRange (pointers);
					}
	
					// If this field has been loaded into a pinvoke, 
					// check the list for unsafe pointers.
					if (fields_loads != null && fields_loads.Contains (field))
						VerifyMethods (pointers);
				} else if (ins.IsStoreLocal ()) {
					int index = ins.GetStoreIndex ();

					pointers = (stack_count <= 0) ? null : GetDelegatePointers (locals, stack [stack_count - 1]);
					
					// Console.WriteLine (" Reached a local variable store at index {0}, there are {1} pointers here.", index, pointers == null ? 0 : pointers.Count);
					
					if (pointers != null && pointers.Count > 0) {
						while (locals.Count <= index)
							locals.Add (null);
						if (locals [index] == null)
							locals [index] = new List<MethodDefinition> ();
						locals [index].AddRange (pointers);
					}
				} 
				
				stack_count += push - pop;
				if (stack_count < 0) {
#if DEBUG
					Console.WriteLine ("{0} got a stack_count < 0 ({1})", method, stack_count);
#endif
					continue;
				}
				
				while (stack_count > stack.Count)
					stack.Add (new ILRange (ins));
				while (stack_count < stack.Count)
					stack.RemoveAt (stack.Count - 1);

				if (stack_count > 0 && stack [stack_count - 1] != null)
					stack [stack_count - 1].Last = ins;
			}
			
			// Console.WriteLine ("Checking method: {0} [Done]", method.Name);
			
			return Runner.CurrentRuleResult;
		}
		
		// Verifies that the method is safe to call as a callback from native code.
		private bool VerifyCallbackSafety (MethodDefinition callback)
		{
			bool result;
			bool valid_ex_handler = false;
				
			if (callback == null)
				return true;

			if (!callback.HasBody)
				return true;
								
			if (verified_methods != null && verified_methods.TryGetValue (callback, out result))
				return result;
			
			if (callback.Body.HasExceptionHandlers) {
				foreach (ExceptionHandler eh in callback.Body.ExceptionHandlers) {
#if DEBUG
					Console.WriteLine ("HandlerStart: {0}, HandlerEnd: {1}, FilterStart: {2}, FilterEnd: {3}, TryStart: {4}, TryEnd: {5}, CatchType: {6}, Type: {7}", 
					                   eh.HandlerStart.GetOffset (), eh.HandlerEnd.GetOffset (), eh.FilterStart.GetOffset (), eh.FilterEnd.GetOffset (),
					                   eh.TryStart.GetOffset (), eh.TryEnd.GetOffset (), eh.CatchType, eh.Type);
#endif
					
					/*
					 * Here we could get a lot stricter, we accept the following code:
					 * 
					 *  void Method ()
					 *  {
					 *      // no code here
					 *        try {
					 * 		    // ... code ..
					 *    [ } catch (Exception ex } ]
					 *    [     // ... code ...     ]
					 *      } catch {
					 *          // ... code ...
					 *    [ } finally { ]
					 *    [     // ... code ...     ]
					 *      } 
					 *      // no code here
					 *  }
					 * 
					 * There are a few ways to break this
					 * - any of the catch clauses may throw an exception.
					 * - finally clause may throw an exception.
					 * 
					 * The stricter rules would ensure that:
					 * - all code in catch and finally clauses are also embedded in a simple try { <code> } catch { <no code> } block
					 * 
					 */
					
					// We only care about catch clauses.
					if (eh.Type != ExceptionHandlerType.Catch)
						continue;
					
					// no 'Catch ... When <condition>' clause. C# doesn't support it, VB does
					if (eh.FilterStart != null || eh.FilterEnd != null)
						continue;
					
					// exception clause has to start at the beginning of the method
					if (eh.TryStart == null || eh.TryStart.Offset != 0)
						continue;
					
					// and span the entire method
					if (eh.HandlerEnd == null || eh.HandlerEnd.Offset != callback.Body.CodeSize - 1)
						continue;
					
					// check for empty catch clause catching every single exception
					// we don't allow 'catch (Exception)' because in IL it's valid to
					// throw any object.
					if (eh.CatchType != null && eh.CatchType.FullName != "System.Object")
						continue;
					
					valid_ex_handler = true;
					break;
				}
			}
				
			if (verified_methods == null)
				verified_methods = new Dictionary<MethodDefinition, bool> ();
			
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
				Instruction instr = First;
				do {
					string pretty = (instr == null) ? "<nil>" : 
						String.Format ("IL_{0} {1} {2}", instr.Offset, instr.OpCode.Name, instr.Operand);

					Console.WriteLine ("dump", "{0}{1}", prefix, pretty);
					if (instr == Last)
						break;
					instr = instr.Next;
				} while (true);
			}
#endif
		}
		
		// Parses the ILRange and return all delegate pointers that could end up on the stack as a result of executing that code.
		private List<MethodDefinition> GetDelegatePointers (List<List<MethodDefinition>> locals, ILRange range)
		{
			List<MethodDefinition> result = null;
			Instruction current;
			
			// Check if the code does any ldftn.
			current = range.First;
			do {
				if (current.OpCode.Code == Code.Ldftn) {
					if (result == null)
						result = new List<MethodDefinition> ();
					result.Add ((current.Operand as MethodReference).Resolve ());
				}
				if (current == range.Last)
					break;
				current = current.Next;
			} while (true);
			
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
			if (field_stores != null && (range.Last.OpCode.Code == Code.Ldfld || range.Last.OpCode.Code == Code.Ldsfld)) {
				FieldDefinition field = (range.Last.Operand as FieldReference).Resolve ();
				List<MethodDefinition> pointers;
				if (field_stores.TryGetValue (field, out pointers)) {
					if (result == null)
						result = new List<MethodDefinition> ();
					result.AddRange (pointers);
				}
				
			}
			
			return result;
		}
		
		
		// Verifies that all methods in the list are safe to call from native code,
		// otherwise reports the correspoding result.
		private void VerifyMethods (List<MethodDefinition> pointers)
		{
			if (pointers == null)
				return;
			
			foreach (MethodDefinition pointer in pointers)
				ReportVerifiedMethod (pointer, VerifyCallbackSafety (pointer));
		}
		
		// Reports the result from verifying the method.
		private void ReportVerifiedMethod (MethodDefinition pointer, bool safe)
		{
			if (!safe) {
				if (reported_methods == null)
					reported_methods = new List<MethodDefinition> ();
				reported_methods.Add (pointer);
				Runner.Report (pointer, Severity.High, Confidence.High);
			}
		}
	}
	
	internal static class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRuleHelper {
#if DEBUG
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
			case Code.Ldloc: // Untested for ldloc
			case Code.Ldloc_S: return ((VariableDefinition) ins.Operand).Index;
			case Code.Ldloca: // we don't care about ldloca
			case Code.Ldloca_S: return int.MaxValue;

			default:
				throw new ArgumentException (string.Format ("Invalid opcode: {0}", ins.OpCode.Name));
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
				throw new ArgumentException (string.Format ("Invalid opcode: {0}", ins.OpCode.Name));
			}
		}
	}	
}
