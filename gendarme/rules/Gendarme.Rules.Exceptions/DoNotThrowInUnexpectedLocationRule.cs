//
// Gendarme.Rules.Exceptions.DoNotThrowInUnexpectedLocationRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Jesse Jones
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Text;

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
	/// <item>
	/// <description><c>TryParse</c> methods - should not throw. These methods
	/// are designed to be executed without having to catch multiple exceptions
	/// (unlike the <c>Parse</c> methods).</description>
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
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	// http://msdn.microsoft.com/en-us/library/bb386039.aspx
	[Problem ("A method throws an exception it should not.")]
	[Solution ("Change the code so that it does not throw, throws a legal exception, or traps exceptions.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class DoNotThrowInUnexpectedLocationRule : Rule, IMethodRule {

		private static readonly OpCodeBitmask Throwers = new OpCodeBitmask (0x0, 0x80C8000000000000, 0x3FC17FC000001FF, 0x0);
		private static readonly OpCodeBitmask OverflowThrowers = new OpCodeBitmask (0x0, 0x8000000000000000, 0x3FC07F8000001FF, 0x0);
		private static readonly OpCodeBitmask Casts = new OpCodeBitmask (0x0, 0x48000000000000, 0x400000000, 0x0);

		private static readonly string [][] GetterExceptions = new string [][] {
			new string [] { "System", "InvalidOperationException" },
			new string [] { "System", "NotSupportedException"}
		};

		private static readonly string [][] IndexerExceptions = new string [][] {
			new string [] { "System", "InvalidOperationException" }, 
			new string [] { "System", "NotSupportedException" }, 
			new string [] { "System", "ArgumentException" }, 
			new string [] { "System.Collections.Generic", "KeyNotFoundException" }
		};

		private static readonly string [][] EventExceptions = new string [][] {
			new string [] { "System", "InvalidOperationException" }, 
			new string [] { "System", "NotSupportedException" }, 
			new string [] { "System", "ArgumentException" }
		};

		private static readonly string [][] HashCodeExceptions = new string [][] {
			new string [] { "System", "ArgumentException" }
		};

		private static bool CheckAttributes (MethodReference method, MethodAttributes attrs)
		{
			MethodDefinition md = method.Resolve ();
			return ((md == null) || ((md.Attributes & attrs) == attrs));
		}
		
		private static MethodSignature EqualityComparer_Equals = new MethodSignature ("Equals", "System.Boolean", new string [2],
			(method) => (CheckAttributes (method, MethodAttributes.Public)));
		private static MethodSignature EqualityComparer_GetHashCode = new MethodSignature ("GetHashCode", "System.Int32", new string [1], 
			(method) => (CheckAttributes (method, MethodAttributes.Public)));
		private static readonly MethodSignature GetTypeSig = new MethodSignature ("GetType", "System.Type", new string [0],
			(method) => (CheckAttributes (method, MethodAttributes.Public)));
		
		private MethodSignature equals_signature;
		private MethodSignature hashcode_signature;
		private string [][] allowedExceptions;
		private Severity severity;
		private bool is_equals;

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeType += delegate (object sender, RunnerEventArgs e) {
				if (e.CurrentType.Implements ("System.Collections.Generic", "IEqualityComparer`1")) {
					equals_signature = EqualityComparer_Equals;
					hashcode_signature = EqualityComparer_GetHashCode;
				} else {
					equals_signature = null;
					hashcode_signature = null;
				}
			};
		}

		static bool HasCatchBlock (MethodBody body)	
		{
			if (!body.HasExceptionHandlers)
				return false;

			foreach (ExceptionHandler handler in body.ExceptionHandlers) {
				if (handler.HandlerType == ExceptionHandlerType.Catch)
					return true;
			}
			
			return false;
		}

		private string PreflightMethod (MethodDefinition method)
		{
			if (method.IsSpecialName) {
				return PreflightSpecialNameMethod (method);
			} else if (method.IsVirtual) {
				return PreflightVirtualMethod (method);
			} else if (method.HasParameters && (method.Name == "Dispose")) {
				IList<ParameterDefinition> pdc = method.Parameters;
				if ((pdc.Count == 1) && pdc [0].ParameterType.IsNamed ("System", "Boolean"))
					return "Dispose (bool)";
			} else if (MethodSignatures.TryParse.Matches (method)) {
				return "TryParse";
			}
			
			return String.Empty;
		}

		private string PreflightSpecialNameMethod (MethodDefinition method)
		{
			if (method.IsConstructor && method.IsStatic)
				return "Static constructors";

			string name = method.Name;
			if (name == "get_Item") {
				severity = Severity.Medium;
				allowedExceptions = IndexerExceptions;
				return "Indexed getters";
			} else if (method.IsGetter) {
				severity = Severity.Medium;
				allowedExceptions = GetterExceptions;
				return "Property getters";
			} else if (method.IsAddOn || method.IsRemoveOn) {
				severity = Severity.Medium;
				allowedExceptions = EventExceptions;
				return "Event accessors";
			} else if (MethodSignatures.op_Equality.Matches (method)) {
				return "operator==";
			} else if (MethodSignatures.op_Inequality.Matches (method)) {	
				return "operator!=";
			} else if (name == "op_Implicit") {
				return "Implicit cast operators";
			} 
			return String.Empty;
		}

		private string PreflightVirtualMethod (MethodDefinition method)
		{
			if (MethodSignatures.ToString.Matches (method)) {
				return "Object.ToString";	// these names should match those used within the rule description
			} else if (MethodSignatures.Equals.Matches (method)) {
				is_equals = true;
				return "Object.Equals";
			} else if (MethodSignatures.GetHashCode.Matches (method)) {
				return "Object.GetHashCode";
			} else if (MethodSignatures.Finalize.Matches (method)) {
				return "Finalizers";
			} else if (MethodSignatures.Dispose.Matches (method) || MethodSignatures.DisposeExplicit.Matches (method)) {
				if (method.DeclaringType.Implements ("System", "IDisposable"))
					return "IDisposable.Dispose";
			} else if (equals_signature != null && equals_signature.Matches (method)) {
				return "IEqualityComparer<T>.Equals";
			} else if (hashcode_signature != null && hashcode_signature.Matches (method)) {
				allowedExceptions = HashCodeExceptions;
				return "IEqualityComparer<T>.GetHashCode";
			}
			return String.Empty;
		}

		// It's not always apparent why the code throws so we'll try to explain
		// the reason here (for example foreach can generate castclass or unbox
		// instructions and assemblies compiled with checked arithmetic can
		// throw even if the code doesn't explicitly use an arithmetic operator). 
		static string ExplainThrow (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Castclass:
				return String.Format (CultureInfo.InvariantCulture, " (cast to {0})", 
					((TypeReference) ins.Operand).Name);

			case Code.Throw:					// this one is obvious
				return string.Empty;

			case Code.Unbox:
			case Code.Unbox_Any:
				return String.Format (CultureInfo.InvariantCulture, " (unbox from {0})", 
					((TypeReference) ins.Operand).Name);
				
			case Code.Ckfinite:
				return " (the expression will throw if the value is a NAN or an infinity)";
				
			default:
				Debug.Assert (ins.OpCode.Name.Contains (".ovf"), "expected an overflow opcode, not " + ins.OpCode.Name);
				return " (checked arithmetic is being used)";
			}
		}

		static bool AreCastsOk (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Isinst:
				return true;
			case Code.Call:
			case Code.Callvirt:
				return GetTypeSig.Matches (ins.Operand as MethodReference);
			default:
				return false;
			}
		}
				
		private void ProcessMethod (MethodDefinition method, string methodLabel)
		{
			bool casts_are_ok = false;
			
			foreach (Instruction ins in method.Body.Instructions) {
				if (is_equals && !casts_are_ok)
					casts_are_ok = AreCastsOk (ins);

				Code code = ins.OpCode.Code;
				if (Throwers.Get (code)) {

					// A few instructions are bad to the bone.
					if (code == Code.Ckfinite) {
						Report (method, ins, methodLabel);
				
					// If the instruction is castclass or unbox then we may have a 
					// problem, but only within Object.Equals (casts occur way too 
					// often to flag them everywhere, but it's common mistake to
					// cast the Equals argument without an is or GetType check). 
					} else if (Casts.Get (code)) {
						if (is_equals && !casts_are_ok)
							Report (method, ins, methodLabel);
			
					// If the instruction is a checked math instruction then we have 
					// a problem, but only in GetHashCode methods (they are 
					// potential problems elsewhere but the likelihood of an actual 
					// problem is much higher in hash code methods and there are 
					// too many defects if we flag them everywhere).
					} else if (OverflowThrowers.Get (code)) {
						if (method.Name == "GetHashCode")
							Report (method, ins, methodLabel);

					// If the instruction is a throw,
					} else if (code == Code.Throw) {
							
						// and is throwing NotImplementedException then it is OK (this 
						// is a fairly common case and we'll let DoNotForgetNotImplementedMethodsRule
						// handle it).
						if (ins.Previous.Is (Code.Newobj)) {
							MethodReference mr = (MethodReference) ins.Previous.Operand;
							TypeReference tr = mr.DeclaringType;
							if (tr.IsNamed ("System", "NotImplementedException") || tr.Inherits ("System", "NotImplementedException"))
								continue;
						}	
					
						// If the method doesn't allow any exceptions then we have a 
						// problem.
						if (allowedExceptions == null)
							Report (method, ins, methodLabel);
							
						// If the throw does not one of the enumerated exceptions  (or 
						// a subclass) then we have a problem.
						else if (ins.Previous.Is (Code.Newobj)) {
							TypeReference type = (ins.Previous.Operand as MethodReference ).DeclaringType;
							bool allowed = false;
							foreach (string[] entry in allowedExceptions) {
								if (type.IsNamed (entry [0], entry [1]))
									allowed = true;
							}
							if (!allowed) {
								foreach (string [] entry in allowedExceptions) {
									if (type.Inherits (entry [0], entry [1])) {
										allowed = true;
										break;
									}
								}
							}
							if (!allowed)
								Report (method, ins, methodLabel);
						}	
					}
				}
			}
		}

		private void Report (MethodDefinition method, Instruction ins, string methodLabel)
		{
			string mesg;
			if (allowedExceptions == null) {
				mesg = String.Format (CultureInfo.InvariantCulture,
					"{0} should not throw{1}.", methodLabel, ExplainThrow (ins));
			} else {
				StringBuilder sb = new StringBuilder ();
				sb.Append (methodLabel).Append (" should only throw ");
				for (int i = 0; i < allowedExceptions.Length; i++) {
					string [] entry = allowedExceptions [i];
					sb.Append (entry [0]).Append ('.').Append (entry [1]);
					if (i < allowedExceptions.Length - 1)
						sb.Append (", ");
				}
				sb.Append (" or a subclass").Append (ExplainThrow (ins)).Append ('.');
				mesg = sb.ToString ();
			}

			Log.WriteLine (this, "{0:X4}: {1}", ins.Offset, mesg);
			Runner.Report (method, ins, severity, Confidence.High, mesg);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
				
			if (!Throwers.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			if (!HasCatchBlock (method.Body)) {
				// default severity for (most) methods
				severity = Severity.High;
				// by default no exceptions are allowed
				allowedExceptions = null;
				// special case for Equals
				is_equals = false;

				string method_label = PreflightMethod (method);
				if (method_label.Length > 0) { 
					Log.WriteLine (this);
					Log.WriteLine (this, "-------------------------------------------");
					Log.WriteLine (this, method);

					ProcessMethod (method, method_label);
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

		private static readonly Code [] Casts = new Code []
		{
			Code.Castclass,	// throws InvalidCastException
			Code.Unbox,		// throws InvalidCastException or NullReferenceException
			Code.Unbox_Any,		// throws InvalidCastException or NullReferenceException
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
			OpCodeBitmask casts = new OpCodeBitmask ();
			
			foreach (Code code in AlwaysBad) {
				throwers.Set (code);
				alwaysBad.Set (code);
			}
			
			foreach (Code code in Casts) {
				casts.Set (code);
				throwers.Set (code);
			}
			throwers.Set (Code.Throw);
			
			foreach (Code code in Overflow) {
				throwers.Set (code);
				overflow.Set (code);
			}
			
			Console.WriteLine ("throwers: {0}", throwers);
			Console.WriteLine ("alwaysBad: {0}", alwaysBad);
			Console.WriteLine ("overflow: {0}", overflow);
			Console.WriteLine ("casts: {0}", casts);
		}
#endif
	}
}
