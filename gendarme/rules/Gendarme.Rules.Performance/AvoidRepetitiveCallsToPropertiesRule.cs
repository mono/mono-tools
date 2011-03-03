//
// Gendarme.Rules.Performance.AvoidRepetitiveCallsToPropertiesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// The rule warn if virtual, or unlikely to be inline-able, property getters
	/// are called several times by a method. In most cases repetitive calls simply
	/// requires more time without any gains since the result will always be identical.
	/// You should ignore the reported defects if a different value is expected 
	/// each time the property is called (e.g. calling <c>DateTime.Now</c>).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// private int Report (IList list)
	/// {
	///	if (list.Count > 1) {
	///		DisplayList (list);
	///	}
	///	Console.WriteLine ("# items: {0}", list.Count);
	///	return list.Count;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// private int Report (IList list)
	/// {
	///	int count = list.Count;
	///	if (count > 1) {
	///		DisplayList (list);
	///	}
	///	Console.WriteLine ("# items: {0}", count);
	///	return count;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("This method calls several times into the same properties. This is expensive for virtual properties or when the property cannot be inlined.")]
	[Solution ("Unless a different value is expected from each call, refactor your code to avoid the multiple calls by caching the returned value.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidRepetitiveCallsToPropertiesRule : Rule, IMethodRule {

		private const int Default = 20;
		private int inline_limit = Default;

		/// <summary>
		/// Methods with a code size below InlineLimit (20 by default)
		/// are considered to be inline-able by the JIT.
		/// </summary>
		[DefaultValue (Default)]
		[Description ("Maximum IL size of a property getter to be considered inline-able.")]
		public int InlineLimit {
			get { return inline_limit; }
			set { inline_limit = value; }
		}

		// for Mono see INLINE_LENGTH_LIMIT in mini.c
		// MS JIT probably use another limit (or is based on something else)
		private bool IsInliningCandidate (MethodDefinition method)
		{
			return (!method.HasBody || (method.Body.CodeSize < InlineLimit));
		}

		Dictionary<string, KeyValuePair<MethodDefinition, int>> calls = new Dictionary<string, KeyValuePair<MethodDefinition, int>> ();

		static string GetKey (MethodDefinition caller, MethodDefinition callee, Instruction ins)
		{
			if (callee.IsStatic)
				return callee.GetFullName ();

			IMetadataTokenProvider chain = callee;
			Instruction instance = ins.TraceBack (caller);

			StringBuilder sb = new StringBuilder ();
			while (instance != null) {
				MemberReference mr = (chain as MemberReference);
				if (mr == null)
					sb.Append (chain.ToString ()); // ?? "null")
				else
					sb.Append (mr.GetFullName ());
				sb.Append ('.');
				chain = (instance.Operand as IMetadataTokenProvider);
				if (chain == null) {
					sb.Append (instance.GetOperand (caller));
					break;
				}
				instance = instance.TraceBack (caller);
			}
			if (chain != null)
				sb.Append (chain.ToString ());
			return sb.ToString ();
		}

		//		virtual		non-virtual
		// Low		2		2-5
		// Medium	3-5		6-11
		// High		6-9		12-19
		// Critical	10+		20+
		//
		// note: inlining strategy is complex and varies between runtimes
		static Severity GetSeverity (int count, bool virtualCall)
		{
			if (!virtualCall)
				count >>= 1;

			if (count < 3)
				return Severity.Low;
			else if (count < 6)
				return Severity.Medium;
			else if (count < 10)
				return Severity.High;

			return Severity.Critical;
		}

		static bool Filter (MethodDefinition method)
		{
			TypeReference type = method.DeclaringType;
			// Elapsed* and IsRunning
			if (type.IsNamed ("System.Diagnostics", "Stopwatch"))
				return true;
			// Now and UtcNow
			else if (type.IsNamed ("System", "DateTime"))
				return method.IsStatic;

			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// applies only to methods with IL that are not generated by the compiler or tools
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// avoid processing methods that do not call any methods
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			calls.Clear ();

			foreach (Instruction ins in method.Body.Instructions) {
				MethodReference mr = ins.GetMethod ();
				if ((mr == null) || mr.HasParameters)
					continue;

				MethodDefinition md = mr.Resolve ();
				// md can be null for things like: new byte[,];
				if ((md == null) || !md.IsGetter)
					continue;

				if ((!md.IsVirtual || md.IsFinal) && IsInliningCandidate (md))
					continue;

				// some properties are known, by design, to be called several time
				if (Filter (md))
					continue;

				string key = GetKey (method, md, ins);

				KeyValuePair<MethodDefinition,int> kvp;
				if (calls.TryGetValue (key, out kvp)) {
					kvp = new KeyValuePair<MethodDefinition, int> (md, kvp.Value + 1);
					calls [key] = kvp;
				} else {
					kvp = new KeyValuePair<MethodDefinition, int> (md, 1);
					calls.Add (key, kvp);
				}
			}

			return ReportResults (method);
		}

		private RuleResult ReportResults (IMetadataTokenProvider method)
		{
			foreach (KeyValuePair<string, KeyValuePair<MethodDefinition, int>> kvp in calls) {
				// look which getter we're calling more than once
				int count = kvp.Value.Value;
				if (count == 1)
					continue;

				MethodDefinition md = kvp.Value.Key;
				if (md.IsVirtual && !md.IsFinal) {
					// virtual calls are expensive, so the code better cache the value
					string msg = String.Format (CultureInfo.InvariantCulture, 
						"Multiple ({0}) calls to virtual property '{1}'.", count, md.ToString ());
					Runner.Report (method, GetSeverity (count, true), Confidence.Normal, msg);
				} else if (!IsInliningCandidate (md)) {
					// non-virtual calls might be inlined
					// otherwise caching the value is again a good idea
					int size = md.HasBody ? md.Body.CodeSize : 0;
					string msg = String.Format (CultureInfo.InvariantCulture,
						"Multiple ({0}) calls to non-virtual property '{1}', likely non-inlined due to size ({2} >= {3}).",
						count, md.ToString (), size, InlineLimit);
					Runner.Report (method, GetSeverity (count, false), Confidence.Normal, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

