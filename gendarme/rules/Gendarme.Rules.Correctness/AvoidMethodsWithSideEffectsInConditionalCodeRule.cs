//
// Gendarme.Rules.Correctness.AvoidMethodsWithSideEffectsInConditionalCodeRule
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
	/// Methods which have no visible side effects are termed pure methods. 
	/// Certain System methods and delegates (such as System.String and 
	/// System.Predicate&lt;T&gt;) are assumed to be pure. If you want to 
	/// use a method which is not known to be pure in a conditionally compiled 
	/// method call then you'll need to decorate it with PureAttribute (see the
	/// examples below).
	///
	/// Note that it is OK to disable this rule for defects where the define will
	/// always be set (now and in the future), if the program's state really
	/// should be affected by the define (e.g. a LINUX define), or the impure
	/// method should be decorated with PureAttribute but it is not under
	/// your control.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// using System.Collections.Generic;
	/// using System.Linq;
	/// using System.Runtime.Diagnostics;
	///
	/// internal sealed class Worker {
	/// 	private Dictionary&lt;string, Job&gt; jobs;
	///
	/// 	private bool IsValid (Job job)
	/// 	{
	/// 		return job != null &amp;&amp; job.PartId &gt; 0;
	/// 	}
	///
	/// 	private void Process (string name, Job job)
	/// 	{
	/// 		// This is OK because IsValid has no side effects (although the rule
	/// 		// won&apos;t know that unless we decorate IsValid with PureAttribute).
	/// 		Trace.Assert (IsValid (job), job + &quot; is not valid&quot;);
	///
	/// 		// This is potentially very bad: if release builds now (or in the future)
	/// 		// don&apos;t define TRACE the job won&apos;t be removed from our list in release.
	/// 		Trace.Assert (jobs.Remove (job), &quot;couldn&apos;t find job &quot; + name);
	///
	/// 		job.Execute ();
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// using System.Collections.Generic;
	/// using System.Linq;
	/// using System.Runtime.Diagnostics;
	///
	/// // note: in FX4 (and later) this attribute is defined in System.Runtime.Diagnostics.Contracts
	/// [Serializable]
	/// [AttributeUsage (AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
	/// public sealed class PureAttribute : Attribute {
	/// }
	/// 	
	/// internal sealed class Worker {
	/// 	private Dictionary&lt;string, Job&gt; jobs;
	///
	/// 	[Pure]
	/// 	private bool IsValid (Job job)
	/// 	{
	/// 		return job != null &amp;&amp; job.PartId &gt; 0;
	/// 	}
	///
	/// 	private void Process (string name, Job job)
	/// 	{
	/// 		// IsValid is decorated with PureAttribute so the rule won't complain
	/// 		// when we use it within Trace.Assert.
	/// 		Trace.Assert (IsValid (job), job + &quot; is not valid&quot;);
	///
	/// 		bool removed = jobs.Remove (job);
	/// 		Trace.Assert (removed, &quot;couldn't find job &quot; + name);
	///
	/// 		job.Execute ();
	/// 	}
	/// }
	/// </code>
	/// </example>
	
	[Problem ("A conditionally compiled method is being called, but uses the result of a non-pure method as an argument.")]
	[Solution ("If the non-pure method has no visible side effects then decorate it with PureAttribute. If it does have side effects then rewrite the code so that the method is not called.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidMethodsWithSideEffectsInConditionalCodeRule : Rule, IMethodRule {
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.Success;
			
			Log.WriteLine (this);
			Log.WriteLine (this, "---------------------------------------");
			Log.WriteLine (this, method);
			
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference target = ins.Operand as MethodReference;
					string define = ConditionalOn (target);
					if (define != null) {
						Log.WriteLine (this, "call to {0} method at {1:X4}", define, ins.Offset);
						
						MethodReference impure = FindImpurity (method, ins);
						if (impure != null) {
							string mesg = String.Format (CultureInfo.InvariantCulture, 
								"{0}::{1} is conditionally compiled on {2} but uses the impure {3}::{4}",
								target.DeclaringType.Name, target.Name, define, impure.DeclaringType.Name, impure.Name);
							Log.WriteLine (this, mesg);
							
							Confidence confidence = GetConfidence (define);
							Runner.Report (method, ins, Severity.High, confidence, mesg);
						}
					}
					break;
				}
			}
			
			return Runner.CurrentRuleResult;
		}
		
		internal static Confidence GetConfidence (string define)
		{
			Confidence confidence;
			
			switch (define) {
			case "DEBUG":
			case "CONTRACTS_FULL":
				confidence = Confidence.High;
				break;
				
			case "CONTRACTS_PRECONDITIONS":
				confidence = Confidence.Normal;
				break;
				
			default:
				confidence = Confidence.Low;
				break;
			}
			
			return confidence;
		}
		
		internal static string ConditionalOn (MethodReference mr)
		{
			MethodDefinition method = mr.Resolve ();
			if ((method == null) || !method.HasCustomAttributes)
				return null;

			foreach (CustomAttribute attr in method.CustomAttributes) {
				// ConditionalAttribute has a single ctor taking a string value
				// http://msdn.microsoft.com/en-us/library/system.diagnostics.conditionalattribute.conditionalattribute.aspx
				// any attribute without arguments can be skipped
				if (!attr.HasConstructorArguments)
					continue;
				if (StringConstructor.Matches (attr.Constructor)) {
					if (attr.AttributeType.IsNamed ("System.Diagnostics", "ConditionalAttribute")) {
						return (string) attr.ConstructorArguments [0].Value;
					}
				}
			}
			
			return null;
		}
		
		// This is like the TraceBack rock except that it continues traversing backwards
		// until it finds an instruction which does not pop any values off the stack. This
		// allows us to check all of the instructions used to compute the method's
		// arguments.
		internal static Instruction FullTraceBack (IMethodSignature method, Instruction end)
		{
			Instruction first = end.TraceBack (method);
			
			while (first != null && first.GetPopCount (method) > 0) {
				first = first.TraceBack (method);
			}
			
			return first;
		}
		
		#region Private Methods
		private MethodReference FindImpurity (IMethodSignature method, Instruction end)
		{
			MethodReference impure = null;
			
			Instruction ins = FullTraceBack (method, end);
			if (ins != null) {
				Log.WriteLine (this, "checking args for call at {0:X4} starting at {1:X4}", end.Offset, ins.Offset);
				while (ins.Offset < end.Offset && impure == null) {
					if (ins.OpCode.Code == Code.Call || ins.OpCode.Code == Code.Callvirt) {
						MethodReference candidate = ins.Operand as MethodReference;
						if (!IsPure (candidate))
							return candidate;
					}
					ins = ins.Next;
				}
			}
			
			return impure;
		}
		
		private bool IsPure (MethodReference mr)
		{
			MethodDefinition method = mr.Resolve ();
			
			if (method != null) {
				TypeDefinition type = method.DeclaringType;
				string type_name = type.GetFullName ();
				string method_name = method.Name;

				// getters
				if (method.IsGetter)
					return true;
				
				// System.String, System.Type, etc methods
				if (types_considered_pure.Contains (type_name))
					return true;
				
				// Equals, GetHashCode, Contains, etc
				if (methods_considered_pure.Contains (method_name))
					return true;
				
				// operators
				if (method_name.StartsWith ("op_", StringComparison.Ordinal) && method_name != "op_Implicit" && method_name != "op_Explicit")
					return true;
					
				// Contract methods (skip namespace)
				if (type_name == "System.Diagnostics.Contracts.Contract")
					return true;
					
				// System.Predicate<T> and System.Comparison<T>
				if (type_name.StartsWith ("System.Predicate`1", StringComparison.Ordinal))
					return true;
					
				if (type_name.StartsWith ("System.Comparison`1", StringComparison.Ordinal))
					return true;
					
				// delegate invocation
				if (MethodSignatures.Invoke.Matches (method)) {
					if (type.HasCustomAttributes) {
						if (HasPureAttribute (type.CustomAttributes)) {
							return true;
						}
					}
				}
				
				// PureAttribute
				if (method.HasCustomAttributes) {
					if (HasPureAttribute (method.CustomAttributes)) {
						return true;
					}
				}
				
				return false;
			}
			
			// If we can't resolve the method we have to assume it's OK to call.
			Log.WriteLine (this, "couldn't resolve {0} call: assuming it is pure", mr);
			return true;
		}
		
		// Note that we don't want to use ContainsType because we need to ignore
		// the namespace (at least until it lands in System.Diagnostics).
		static bool HasPureAttribute (IList<CustomAttribute> attrs)
		{
			foreach (CustomAttribute attr in attrs) {
				if (attr.AttributeType.Name == "PureAttribute") {
					return true;
				}
			}
			
			return false;
		}
		#endregion
		
		#region Fields
		private static readonly MethodSignature StringConstructor = new MethodSignature (".ctor", "System.Void", new string [] {"System.String"});

		private static HashSet<string> types_considered_pure = new HashSet<string> {
			"System.Math",
			"System.Object",
			"System.String",
			"System.Type",
			"System.IO.Path",
			"System.Linq.Enumerable",
		};

		private static HashSet<string> methods_considered_pure = new HashSet<string> {
			"AsReadOnly",
			"BinarySearch",
			"Clone",
			"Contains",
			"ContainsKey",
			"ContainsValue",
			"ConvertAll",
			"Equals",
			"Exists",
			"Find",
			"FindAll",
			"FindIndex",
			"FindLast",
			"FindLastIndex",
			"GetByIndex",
			"GetHashCode",
			"IndexOfKey",
			"IndexOfValue",
			"IsProperSubsetOf",
			"IsProperSupersetOf",
			"IsSubsetOf",
			"IsSupersetOf",
			"IndexOf",
			"LastIndexOf",
			"Overlaps",
			"Peek",
			"ToArray",
			"ToDictionary",
			"ToList",
			"ToString",
			"TryGetValue",
			"TrueForAll",
		};
		#endregion
	}
}
