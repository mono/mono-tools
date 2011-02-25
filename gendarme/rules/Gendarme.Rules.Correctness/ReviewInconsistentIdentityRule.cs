//
// Gendarme.Rules.Correctness.ReviewInconsistentIdentityRule
//
// Authors:
//	Jesse Jones  <jesjones@mindspring.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Jesse Jones
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks to see if a type manages its identity in a
	/// consistent way. It checks:
	/// <list type = "bullet">
	///    <item>
	///        <description>Equals methods, relational operators and <c>CompareTo</c> 
	///        must either use the same set of fields and properties or call a
	///        helper method.</description>
	///    </item>
	///    <item>
	///        <description><c>GetHashCode</c> must use the same or a subset of 
	///        the fields used by the equality methods or call a helper method.</description>
	///    </item>
	///    <item>
	///        <description><c>Clone</c> must use the same or a superset of 
	///        the fields used by the equality methods or call a helper method.</description>
	///    </item>
	/// </list>
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("The type does not manage identity consistently in its Equals, relational operator, CompareTo, GetHashCode, and Clone methods.")]
	[Solution ("Equals, relational operator, CompareTo methods should use the same fields and getter properties. GetHashCode should use the same fields/properties or a strict subset of them. Clone should use the same fields/properties or a superset of them.")]
	public sealed class ReviewInconsistentIdentityRule: Rule, ITypeRule {
	
		private HashSet<MethodInfo> methods = new HashSet<MethodInfo> ();
		private MethodInfo hash = new MethodInfo ();
		private MethodInfo clone = new MethodInfo ();
		
		private sealed class MethodInfo {
			private HashSet<MemberReference> fields;
			private HashSet<MemberReference> getters;

			public bool Delegates { get; set; }	// i.e. calls a method (but not a property of the type)
			
			public HashSet<MemberReference> Fields {
				get {
					if (fields == null)
						fields = new HashSet<MemberReference> ();
					return fields;
				}
			}
			
			public HashSet<MemberReference> Getters {
				get {
					if (getters == null)
						getters = new HashSet<MemberReference> ();
					return getters;
				}
			}

			public bool HasFields {
				get { return ((fields != null) && (fields.Count > 0)); }
			}

			public bool HasGetters {
				get { return ((getters != null) && (getters.Count > 0)); }
			}

			public MethodDefinition Method { get; set; }

			public void Clear ()
			{
				Delegates = false;
				if (fields != null)
					fields.Clear ();
				if (getters != null)
					getters.Clear ();
				Method = null;
			}
		}

		private void AddMethod (MethodDefinition method)
		{
			if (method != null)
				methods.Add (new MethodInfo () { Method = method });
		}
		
		private static readonly string [] args1 = new string [1];
		private static readonly string [] args2 = new string [2];
		private static readonly MethodSignature CompareTo = new MethodSignature ("CompareTo", "System.Int32", new string [] { "System.Object" });
		
		private void GetMethods (TypeReference type)	
		{
			string full_name = type.GetFullName ();
			args1 [0] = full_name;
			AddMethod (type.GetMethod (MethodSignatures.Equals));
			AddMethod (type.GetMethod ("Equals", "System.Boolean", args1));

			AddMethod (type.GetMethod ("CompareTo", "System.Int32", args1));	// generic version
			AddMethod (type.GetMethod (CompareTo));								// non-generic version

			// Note that we don't want to use MethodSignatures for these 
			// because we don't want any weird overloads.
			args2 [0] = full_name;
			args2 [1] = full_name;
			AddMethod (type.GetMethod ("op_Equality", "System.Boolean", args2));	
			AddMethod (type.GetMethod ("op_Inequality", "System.Boolean", args2));

			AddMethod (type.GetMethod ("op_LessThan", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_LessThanOrEqual", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_GreaterThan", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_GreaterThanOrEqual", "System.Boolean", args2));
			
			clone.Method = type.GetMethod (MethodSignatures.Clone);
			hash.Method = type.GetMethod (MethodSignatures.GetHashCode);
		}
		
		private HashSet<MemberReference> stored_fields = new HashSet<MemberReference> ();
		private HashSet<MemberReference> property_setters = new HashSet<MemberReference> ();

		private void ProcessMethod (TypeDefinition type, MethodInfo info)
		{
			MethodDefinition method = info.Method;
			Log.WriteLine (this, method);

			// don't process abstract, pinvoke... methods
			if (!method.HasBody)
				return;

			stored_fields.Clear ();
			property_setters.Clear ();

			// For each instruction in the method,
			foreach (Instruction ins in method.Body.Instructions) {	
			
				// If we're loading a field which belongs to our type then
				// we need to add it to our list of referenced fields.
				if (ins.OpCode.Code == Code.Ldfld || ins.OpCode.Code == Code.Ldflda) {
					if (method.IsStatic) {
						if (ins.Previous.IsLoadArgument ()) {
							ParameterDefinition pd = ins.Previous.GetParameter (method);
							if (pd != null && pd.ParameterType == method.DeclaringType) {
								FieldDefinition field = ins.GetField ();	
								info.Fields.Add (field);
							}
						}
					} else {
						if (ins.Previous.OpCode.Code == Code.Ldarg_0) {
							FieldDefinition field = ins.GetField ();	
							info.Fields.Add (field);
						}
					}
				
				// We'll ignore any fields which we wind up storing into. (These 
				// will typically be something like a GetHashCode cache.)
				} else if (ins.OpCode.Code == Code.Stfld) {
					if (!MethodSignatures.Clone.Matches (method)) {
						FieldDefinition field = ins.GetField ();	
						stored_fields.Add (field);
					}
				
				// If we're calling a method which belongs to our type then,
				} else if (ins.OpCode.Code == Code.Call || ins.OpCode.Code == Code.Callvirt) {
					MethodDefinition callee = (ins.Operand as MethodReference).Resolve ();
					if (callee != null && callee.DeclaringType == method.DeclaringType) {
					
						// if it is a getter then save a reference to it,
						if (callee.IsGetter)
							info.Getters.Add (callee);
							
						// if it's a setter then we'll ignore the corresponding getter,
						else if (callee.IsSetter)
							property_setters.Add (callee);
							
						// anything else is assumed to be some sort of helper method which means
						// we don't know all of the state which this method may use.
						else
							info.Delegates = true;
					}
				}
			}
			
			info.Fields.ExceptWith (stored_fields);
			if (property_setters.Count > 0) {
				foreach (PropertyDefinition prop in type.Properties) {
					if (prop.GetMethod != null && property_setters.Contains (prop.SetMethod))
						info.Getters.Remove (prop.GetMethod);
				}
			}
#if DEBUG
			if (info.HasFields) {
				StringBuilder sb = new StringBuilder ();
				sb.Append (method.Name).Append (" uses fields ");
				AppendTo (sb, info.Fields);
				Log.WriteLine (this, sb.ToString ());
			}
			if (info.HasGetters) {
				StringBuilder sb = new StringBuilder ();
				sb.Append (method.Name).Append (" uses getters ");
				AppendTo (sb, info.Getters);
				Log.WriteLine (this, sb.ToString ());
			}
			Log.WriteLine (this);
#endif
		}
		
		// It's a bit silly to stick these into fields, but it does save some 
		// allocations in a highly used code path...
		private HashSet<MemberReference> fields = new HashSet<MemberReference> ();
		private HashSet<MemberReference> getters = new HashSet<MemberReference> ();
		private List<MemberReference> badNames = new List<MemberReference> ();
		
		private void CheckMethods ()
		{
			// Get the set of all fields/getters used by the equality methods.
			foreach (MethodInfo info in methods) {
				if (info.HasFields)
					fields.UnionWith (info.Fields);
				if (info.HasGetters)
					getters.UnionWith (info.Getters);
			}
		}

		private void CheckBadNames ()
		{
			// If an equality or comparison method uses a subset of the 
			// full set then we have a problem.
			MethodDefinition first = null;
			
			foreach (MethodInfo info in methods) {
				if (info.Delegates)
					continue;

				if (info.HasFields && (info.Fields.Count < fields.Count)) {
					first = first ?? info.Method;
					badNames.Add (info.Method);
				} else if (info.HasGetters && (info.Getters.Count < getters.Count)) {
					first = first ?? info.Method;
					badNames.Add (info.Method);
				}
			}
			
			if (badNames.Count > 0) {
				Report (first, "Inconsistent:", badNames, null);
				badNames.Clear ();
			}
		}

		private void CheckHashMethod ()
		{
			// We also have a problem if GetHashCode does not check a 
			// subset of the equality state.
			if (hash.Method != null && !hash.Delegates) {
				// Note that if there are no equality fields or getters then the 
				// equality methods delegate all of their work so we don't
				// don't know which state they are checking.
				if (fields.Count > 0 || getters.Count > 0) {
					if (!hash.HasFields && !hash.HasGetters) {
						Report (hash.Method, "GetHashCode does not use any of the fields and/or properties used by the equality methods.", 
							null, null);
					} else if (!hash.Fields.IsSubsetOf (fields) || !hash.Getters.IsSubsetOf (getters)) {
						hash.Fields.ExceptWith (fields);
						hash.Getters.ExceptWith (getters);

						Report (hash.Method, "GetHashCode uses fields and/or properties not used by the equality methods:",
							hash.Fields, hash.Getters);
					}
				}
			}
			hash.Clear ();
		}

		private void CheckCloneMethod ()
		{
			// We also have a problem if Clone does not use a 
			// superset of the equality state.
			if (clone.Method != null && !clone.Delegates && (clone.HasFields || clone.HasGetters)) {
				if (fields.Count > 0 || getters.Count > 0) {
					if (!clone.Fields.IsSupersetOf (fields) || !clone.Getters.IsSupersetOf (getters)) {
						fields.ExceptWith (clone.Fields);
						getters.ExceptWith (clone.Getters);

						Report (clone.Method, "Clone does not use fields and/or properties used by the equality methods:", 
							fields, getters);
					}
				}
			}
			clone.Clear ();
		}

		static void AppendTo (StringBuilder sb, IEnumerable<MemberReference> values)
		{
			foreach (MemberReference mr in values) {
				sb.Append (' ');
				sb.Append (mr.Name);
			}
		}

		private void Report (IMetadataTokenProvider method, string message, IEnumerable<MemberReference> first, IEnumerable<MemberReference> second)
		{
			StringBuilder sb = new StringBuilder (message);
			if (first != null)
				AppendTo (sb, first);
			if (second != null)
				AppendTo (sb, second);
			string mesg = sb.ToString ();
			Log.WriteLine (this, mesg);
			Runner.Report (method, Severity.High, Confidence.Normal, mesg);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasMethods || !type.HasFields || type.IsEnum)
				return RuleResult.DoesNotApply;
			
			Log.WriteLine (this);
			Log.WriteLine (this, "------------------------------------");
			Log.WriteLine (this, type);
			
			GetMethods (type);
			if (methods.Count > 0) {
				foreach (MethodInfo info in methods)
					ProcessMethod (type, info);
				if (hash.Method != null)
					ProcessMethod (type, hash);
				if (clone.Method != null)
					ProcessMethod (type, clone);
					
				CheckMethods ();
				CheckBadNames ();
				CheckHashMethod ();
				CheckCloneMethod ();

				methods.Clear ();
				fields.Clear ();
				getters.Clear ();
			}
			
			return Runner.CurrentRuleResult;
		}
	}
}
