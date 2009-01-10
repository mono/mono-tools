//
// Gendarme.Rules.Exceptions.DoNotThrowInUnexpectedLocationRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// There are a number of methods which have constraints on the exceptions
	/// which they may throw. This rule checks the following methods:
	/// <list type="bullet"> 
	/// <item> 
	/// <description>Property getters - properties should work very much 
	/// like fields: they should execute very quickly and, in general, should 
	/// not throw exceptions. However they may throw System.InvalidOperationException, 
	/// System.NotSupportedException, or an exception derived from these. 
	/// Indexed getters may also throw System.ArgumentException or 
	/// System.Collections.Generic.KeyNotFoundException.</description>
	/// </item>
	/// <item> 
	/// <description>Event accessors - in general events should not throw 
	/// when adding or removing a handler. However they may throw 
	/// System.InvalidOperationException, System.NotSupportedException, 
	/// System.ArgumentException, or an exception derived from these.</description>
	/// </item>
	/// <item>
	/// <description>Object.Equals and IEqualityComparer&lt;T&gt;.Equals - should 
	/// not throw. In particular they should do something sensible when passed
	/// null arguments or unexpected types.</description>
	/// </item>
	/// <item>
	/// <description>Object.GetHashCode - should not throw or the object 
	/// will not work properly with dictionaries and hash sets.</description>
	/// </item>
	/// <item>
	/// <description>IEqualityComparer&lt;T&gt;.GetHashCode - may throw
	/// System.ArgumentException.</description>
	/// </item>
	/// <item>
	/// <description>Object.ToString - these are called by the debugger to display 
	/// objects and are also often used with printf style debugging so they should 
	/// not change the object's state and should not throw.</description>
	/// </item>
	/// <item>
	/// <description>static constructors - should very rarely throw. If they 
	/// do throw then the type will not be useable within that application 
	/// domain.</description>
	/// </item>
	/// <item>
	/// <description>finalizers - should not throw. If they do (as of .NET 2.0)
	/// the process will be torn down.</description>
	/// </item>
	/// <item>
	/// <description>IDisposable.Dispose - should not throw. If they do
	/// it's much harder to guarantee that objects clean up properly.</description>
	/// </item>
	/// <item>
	/// <description>Dispose (bool) - should not throw because that makes 
	/// it very difficult to clean up objects and because they are often
	/// called from a finalizer.</description>
	/// </item>
	/// <item>
	/// <description>operator== and operator!= - should not throw. In particular 
	/// they should do something sensible when passed null arguments or 
	/// unexpected types.</description>
	/// </item>
	/// <item>
	/// <description>implicit cast operators - should not throw. These methods
	/// are called implicitly so it tends to be quite surprising if they throw 
	/// exceptions.</description>
	/// </item>
	/// </list>
	/// Note that the rule does not complain if a method throws 
	/// System.NotImplementedException because 
	/// DoNotForgetNotImplementedMethodsRule will flag them. Also the rule
	/// may fire with anonymous types with gmcs versions prior to 2.2, see
	/// [https://bugzilla.novell.com/show_bug.cgi?id=462622] for more details.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public override bool Equals (object obj)
	/// {
	/// 	if (obj == null) {
	/// 		return false;
	/// 	}
	/// 	
	/// 	Customer rhs = (Customer) obj;	// throws if obj is not a Customer
	/// 	return name == rhs.name;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public override bool Equals (object obj)
	/// {
	/// 	Customer rhs = obj as Customer;
	/// 	if (rhs == null) {
	/// 		return false;
	/// 	}
	/// 	
	/// 	return name == rhs.name;
	/// }
	/// </code>
	/// </example>

	// http://msdn.microsoft.com/en-us/library/bb386039.aspx
	[Problem ("A method throws an exception it should not.")]
	[Solution ("Change the code so that it does not throw, throw a correct exception, or trap exceptions.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DoNotThrowInUnexpectedLocationRule : Rule, IMethodRule {

		private static readonly OpCodeBitmask Throwers = new OpCodeBitmask (0x0, 0x80C8000000000000, 0x3FC17F8000001FF, 0x0);
		private static readonly OpCodeBitmask AlwaysBadThrowers = new OpCodeBitmask (0x0, 0x0, 0x100000000000, 0x0);
		private static readonly OpCodeBitmask OverflowThrowers = new OpCodeBitmask (0x0, 0x8000000000000000, 0x3FC17F8000001FF, 0x0);

		private static readonly string [] GetterExceptions = new string [] {"System.InvalidOperationException", "System.NotSupportedException"};
		private static readonly string [] IndexerExceptions = new string [] {"System.InvalidOperationException", "System.NotSupportedException", "System.ArgumentException", "System.Collections.Generic.KeyNotFoundException"};
		private static readonly string [] EventExceptions = new string [] {"System.InvalidOperationException", "System.NotSupportedException", "System.ArgumentException"};
		private static readonly string [] HashCodeExceptions = new string [] {"System.ArgumentException"};
		
		private TypeReference type;
		private MethodSignature equalityComparerEquals;
		private MethodSignature equalityComparerHashCode;
		private string [] allowedExceptions;
		private string methodLabel;

		private bool HasCatchBlock (MethodDefinition method)	
		{
			foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
				if (handler.Type == ExceptionHandlerType.Catch)
					return true;
			}
			
			return false;
		}
		
		private static readonly string [] AnySingleArg = new string [1];
		private static readonly string [] AnyTwoArgs = new string [2];
		
		private void InitType (TypeReference type)	
		{
			if (type.Implements ("System.Collections.Generic.IEqualityComparer`1")) {
				equalityComparerEquals = new MethodSignature ("Equals", "System.Boolean", AnyTwoArgs, MethodAttributes.Public);
				equalityComparerHashCode = new MethodSignature ("GetHashCode", "System.Int32", AnySingleArg, MethodAttributes.Public);
			} else {
				equalityComparerEquals = null;
				equalityComparerHashCode = null;
			}
		}

		private bool PreflightMethod (MethodDefinition method)
		{
			allowedExceptions = null;
			bool valid = false;
			
			if (MethodSignatures.ToString.Matches (method)) {
				methodLabel = "Object.ToString";	// these names should match those used within the rule description
				valid = true;

			} else if (MethodSignatures.Equals.Matches (method)) {
				methodLabel = "Object.Equals";
				valid = true;

			} else if (MethodSignatures.GetHashCode.Matches (method)) {
				methodLabel = "Object.GetHashCode";
				valid = true;

			} else if (MethodSignatures.Finalize.Matches (method)) {
				methodLabel = "Finalizers";
				valid = true;

			} else if (MethodSignatures.Dispose.Matches (method) && method.DeclaringType.Implements ("System.IDisposable")) {
				methodLabel = "IDisposable.Dispose";
				valid = true;

			} else if (MethodSignatures.DisposeExplicit.Matches (method) && method.DeclaringType.Implements ("System.IDisposable")) {
				methodLabel = "IDisposable.Dispose";
				valid = true;

			} else if (MethodSignatures.op_Equality.Matches (method)) {
				methodLabel = "operator==";
				valid = true;

			} else if (MethodSignatures.op_Inequality.Matches (method)) {	
				methodLabel = "operator!=";
				valid = true;

			} else if (equalityComparerEquals != null && equalityComparerEquals.Matches (method)) {
				methodLabel = "IEqualityComparer<T>.Equals";
				valid = true;

			} else if (equalityComparerHashCode != null && equalityComparerHashCode.Matches (method)) {
				methodLabel = "IEqualityComparer<T>.GetHashCode";
				allowedExceptions = HashCodeExceptions;
				valid = true;

			} else if (method.Name == "Dispose" && method.Parameters.Count == 1 &&  method.Parameters [0].ParameterType.FullName == "System.Boolean") {
				methodLabel = "Dispose (bool)";
				valid = true;

			} else if (method.IsConstructor && method.IsStatic) {	
				methodLabel = "Static constructors";
				valid = true;

			} else if (method.Name == "op_Implicit") {
				methodLabel = "Implicit cast operators";
				valid = true;

			} else if (method.IsSpecialName && method.Name == "get_Item") {
				methodLabel = "Indexed getters";
				allowedExceptions = IndexerExceptions;
				valid = true;

			} else if (method.IsGetter) {
				methodLabel = "Property getters";
				allowedExceptions = GetterExceptions;
				valid = true;

			} else if (method.IsAddOn || method.IsRemoveOn) {
				methodLabel = "Event accessors";
				allowedExceptions = EventExceptions;
				valid = true;
			}		
			
			return valid;
		}
		
		// It's not always apparent why the code throws so we'll try to explain
		// the reason here (for example foreach can generate castclass or unbox
		// instructions and assemblies compiled with checked arithmetic can
		// throw even if the code doesn't explicitly use an arithmetic operator). 
		private string ExplainThrow (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Castclass:
				return string.Format (" (cast to {0})", ((TypeReference) ins.Operand).Name);

			case Code.Throw:					// this one is obvious
				return string.Empty;

			case Code.Unbox:
				return string.Format (" (unbox from {0})", ((TypeReference) ins.Operand).Name);
				
			case Code.Ckfinite:
				return " (the expression will throw if the value is a NAN or an infinity)";
				
			default:
				Debug.Assert (ins.OpCode.Name.Contains (".ovf"), "expected an overflow opcode, not " + ins.OpCode.Name);
				return " (checked arithmetic is being used)";
			}
		}
				
		private List<Instruction> instructions = new List<Instruction> ();
		private List<Instruction> bad = new List<Instruction> ();
		private List<Instruction> casts = new List<Instruction> ();
		public static readonly MethodSignature GetTypeSig = new MethodSignature ("GetType", "System.Type", new string [0], MethodAttributes.Public);

		private void ProcessMethod (MethodDefinition method)
		{
			bad.Clear ();
			casts.Clear ();
			bool casts_are_ok = false;
			bool is_equals = MethodSignatures.Equals.Matches (method);
			
			foreach (Instruction ins in method.Body.Instructions) {
				Code code = ins.OpCode.Code;
				
				if (is_equals && !casts_are_ok) {
					if (code == Code.Isinst)
						casts_are_ok = true;
	
					else if (code == Code.Call || code == Code.Callvirt) {
						MethodReference mr = (MethodReference) ins.Operand;
						if (GetTypeSig.Matches (mr))
							casts_are_ok = true;
					}	
				}
				
				if (Throwers.Get (code)) {

					// A few instructions are bad to the bone.
					if (AlwaysBadThrowers.Get (code)) {
						bad.Add (ins);
				
					// If the instruction is castclass or unbox then we may have a 
					// problem, but only within Object.Equals (casts occur way too 
					// often to flag them everywhere, but it's common mistake to
					// cast the Equals argument without an is or GetType check). 
					} else if (code == Code.Castclass || code == Code.Unbox) {
						if (is_equals && !casts_are_ok)
							casts.Add (ins);
			
					// If the instruction is a checked math instruction then we have 
					// a problem, but only in GetHashCode methods (they are 
					// potential problems elsewhere but the likelihood of an actual 
					// problem is much higher in hash code methods and there are 
					// too many defects if we flag them everywhere).
					} else if (OverflowThrowers.Get (code)) {
						if (method.Name == "GetHashCode")
							bad.Add (ins);

					// If the instruction is a throw,
					} else if (code == Code.Throw) {
							
						// and is throwing NotImplementedException then it is OK (this 
						// is a fairly common case and we'll let DoNotForgetNotImplementedMethodsRule
						// handle it).
						if (ins.Previous != null && ins.Previous.OpCode.Code == Code.Newobj) {
							MethodReference mr = (MethodReference) ins.Previous.Operand;
							string name = mr.DeclaringType.FullName;
							if (name == "System.NotImplementedException" || mr.DeclaringType.Inherits ("System.NotImplementedException"))
								continue;
						}	
					
						// If the method doesn't allow any exceptions then we have a 
						// problem.
						if (allowedExceptions == null)
							bad.Add (ins);
							
						// If the throw does not one of the enumerated exceptions  (or 
						// a subclass) then we have a problem.
						else if (ins.Previous != null && ins.Previous.OpCode.Code == Code.Newobj) {
							MethodReference mr = (MethodReference) ins.Previous.Operand;
							string name = mr.DeclaringType.FullName;
							if (Array.IndexOf (allowedExceptions, name) < 0) {
								if (!allowedExceptions.Any (e => mr.DeclaringType.Inherits (e))) {
									bad.Add (ins);
								}
							}
						}	
					}
				}
			}
			
			instructions.Clear ();	
			instructions.AddRange (bad);
			if (is_equals && !casts_are_ok)
				instructions.AddRange (casts);
		}
		
		private void ReportErrors (MethodDefinition method)
		{
			foreach (Instruction ins in instructions) {
				string mesg;
				if (allowedExceptions == null)
					mesg = string.Format ("{0} should not throw{1}.", methodLabel, ExplainThrow (ins));
				else
					mesg = string.Format ("{0} should only throw {1} or a subclass{2}.", methodLabel, string.Join (", ", allowedExceptions), ExplainThrow (ins));
				Log.WriteLine (this, "{0:X4}: {1}", ins.Offset, mesg);
				
				// We reduce the severity of getters and event accessors because 
				// it's not quite as bad for a method which allows some exceptions
				// to throw the wrong exception.
				Severity severity = method.IsGetter || method.IsAddOn || method.IsRemoveOn ? 
					Severity.Medium : Severity.High;
				Runner.Report (method, ins, severity, Confidence.High, mesg);
			}
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
				
			if (!Throwers.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			if (method.DeclaringType != type) {		// note that we cannot use the AnalyzeType event because it is not called with the unit tests
				InitType (method.DeclaringType);
				type = method.DeclaringType;
			}
											
			if (!HasCatchBlock (method)) {
				if (PreflightMethod (method)) { 
					Log.WriteLine (this);
					Log.WriteLine (this, "-------------------------------------------");
					Log.WriteLine (this, method);
						
					ProcessMethod (method); 
					if (instructions.Count > 0)
						ReportErrors (method);
				}
			}
			
			return Runner.CurrentRuleResult;
		}

#if false
		// Note that none of these instructions throw an exception that is 
		// ever allowed.
		private static readonly Code [] AlwaysBad = new Code []
		{
			Code.Ckfinite,		// throws ArithmeticException
		};

		private static readonly Code [] SometimesBad = new Code []
		{
			Code.Castclass,	// throws InvalidCastException
			Code.Throw,
			Code.Unbox,		// throws InvalidCastException or NullReferenceException
		};

		private static readonly Code [] Overflow = new Code []
		{
			Code.Add_Ovf,	// throws OverflowException
			Code.Mul_Ovf,
			Code.Sub_Ovf,
			Code.Add_Ovf_Un,
			Code.Mul_Ovf_Un,
			Code.Sub_Ovf_Un,
			Code.Conv_Ovf_I1_Un,
			Code.Conv_Ovf_I2_Un,
			Code.Conv_Ovf_I4_Un,
			Code.Conv_Ovf_I8_Un,
			Code.Conv_Ovf_U1_Un,
			Code.Conv_Ovf_U2_Un,
			Code.Conv_Ovf_U4_Un,
			Code.Conv_Ovf_U8_Un,
			Code.Conv_Ovf_I_Un,
			Code.Conv_Ovf_U_Un,
			Code.Conv_Ovf_I1,
			Code.Conv_Ovf_U1,
			Code.Conv_Ovf_I2,
			Code.Conv_Ovf_U2,
			Code.Conv_Ovf_I4,
			Code.Conv_Ovf_U4,
			Code.Conv_Ovf_I8,
			Code.Conv_Ovf_U8,
			Code.Conv_Ovf_I,
			Code.Conv_Ovf_U,
		};
		
		public void GenerateBitmask ()
		{
			OpCodeBitmask throwers = new OpCodeBitmask ();
			OpCodeBitmask alwaysBad = new OpCodeBitmask ();
			OpCodeBitmask overflow = new OpCodeBitmask ();
			
			foreach (Code code in AlwaysBad) {
				throwers.Set (code);
				alwaysBad.Set (code);
			}
			
			foreach (Code code in SometimesBad) {
				throwers.Set (code);
			}
			
			foreach (Code code in Overflow) {
				throwers.Set (code);
				overflow.Set (code);
			}
			
			Console.WriteLine ("throwers: {0}", throwers);
			Console.WriteLine ("alwaysBad: {0}", alwaysBad);
			Console.WriteLine ("overflow: {0}", overflow);
		}
#endif
	}
}
