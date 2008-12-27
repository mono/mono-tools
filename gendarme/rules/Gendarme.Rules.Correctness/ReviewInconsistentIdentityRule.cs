//
// Gendarme.Rules.Correctness.ReviewInconsistentIdentityRule
//
// Authors:
//	Jesse Jones  <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks to see if a type is managing its identity in a
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
	[Problem ("The type does not manage identity consistently in its Equals, relational operator, CompareTo, GetHashCode, and Clone methods.")]
	[Solution ("Equals, relational operator, CompareTo methods should use the same fields and get properties. GetHashCode should use the same fields/properties or a strict subset of them. Clone should use the same fields/properties or a superset of them.")]
	public sealed class ReviewInconsistentIdentityRule: Rule, ITypeRule {
	
		private const int MaxMethodCount = 10;
		private Dictionary<MethodDefinition, MethodInfo> equalityMethods = new Dictionary<MethodDefinition, MethodInfo> (MaxMethodCount);
		private MethodDefinition hashMethod;
		private MethodInfo hashInfo;
		private MethodDefinition cloneMethod;
		private MethodInfo cloneInfo;
		
		private sealed class MethodInfo {
			private HashSet<FieldDefinition> fields = new HashSet<FieldDefinition> ();
			private HashSet<MethodDefinition> getters = new HashSet<MethodDefinition> ();
			
			public bool Delegates { get; set; }	// i.e. calls a method (but not a property of the type)
			
			public HashSet<FieldDefinition> Fields {
				get { return fields; }
			}
			
			public HashSet<MethodDefinition> Getters {
				get { return getters; }
			}
		}
				
		private void AddMethod (MethodDefinition method)
		{
			if (method != null) {
				equalityMethods.Add (method, new MethodInfo ());
				Debug.Assert (equalityMethods.Count <= MaxMethodCount, string.Format ("equalityMethods has {0} methods", equalityMethods.Count));
			}
		}
		
		private string [] args1 = new string [1];
		private string [] args2 = new string [2];
		private static readonly MethodSignature CompareTo = new MethodSignature ("CompareTo", "System.Int32", new string [] { "System.Object" },  MethodAttributes.Public);
		
		private void GetMethods (TypeDefinition type)	
		{			
			args1 [0] = type.FullName;
			AddMethod (type.GetMethod (MethodSignatures.Equals));
			AddMethod (type.GetMethod ("Equals", "System.Boolean", args1));

			AddMethod (type.GetMethod ("CompareTo", "System.Int32", args1));	// generic version
			AddMethod (type.GetMethod (CompareTo));								// non-generic version

			// Note that we don't want to use MethodSignatures for these 
			// because we don't want any weird overloads.
			args2 [0] = type.FullName;
			args2 [1] = type.FullName;
			AddMethod (type.GetMethod ("op_Equality", "System.Boolean", args2));	
			AddMethod (type.GetMethod ("op_Inequality", "System.Boolean", args2));

			AddMethod (type.GetMethod ("op_LessThan", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_LessThanOrEqual", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_GreaterThan", "System.Boolean", args2));
			AddMethod (type.GetMethod ("op_GreaterThanOrEqual", "System.Boolean", args2));
			
			cloneMethod = type.GetMethod (MethodSignatures.Clone);
			if (cloneMethod != null) 
				cloneInfo = new MethodInfo ();

			hashMethod = type.GetMethod (MethodSignatures.GetHashCode);
			if (hashMethod != null) 
				hashInfo = new MethodInfo ();
		}
		
		private HashSet<FieldDefinition> setFields = new HashSet<FieldDefinition> ();
		private HashSet<MethodDefinition> setProps = new HashSet<MethodDefinition> ();

		private void ProcessMethod (TypeDefinition type, MethodDefinition method, MethodInfo info)
		{
			Log.WriteLine (this, method);

			setFields.Clear ();
			setProps.Clear ();

			// For each instruction in the method,
			foreach (Instruction ins in method.Body.Instructions) {	
			
				// If we're loading a field which belongs to our type then
				// we need to add it to our list of referenced fields.
				if (ins.OpCode.Code == Code.Ldfld) {
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
						setFields.Add (field);
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
							setProps.Add (callee);
							
						// anything else is assumed to be some sort of helper method which means
						// we don't know all of the state which this method may use.
						else
							info.Delegates = true;
					}
				}
			}
			
			info.Fields.ExceptWith (setFields);
			if (setProps.Count > 0) {
				foreach (PropertyDefinition prop in type.Properties) {
					if (prop.GetMethod != null && setProps.Contains (prop.SetMethod))
						info.Getters.Remove (prop.GetMethod);
				}
			}

#if DEBUG
			if (info.Fields.Count > 0)
				Log.WriteLine (this, "{0} uses {1}", method.Name, string.Join (", ", (from f in info.Fields select f.Name).ToArray ()));
				
			if (info.Getters.Count > 0)
				Log.WriteLine (this, "{0} uses {1}", method.Name, string.Join (", ", (from g in info.Getters select g.Name).ToArray ()));
			
			Log.WriteLine (this);
#endif
		}
		
		// It's a bit silly to stick these into fields, but it does save some 
		// allocations in a highly used code path...
		private HashSet<FieldDefinition> equalityFields = new HashSet<FieldDefinition> ();
		private HashSet<MethodDefinition> equalityGetters = new HashSet<MethodDefinition> ();
		private List<string> badNames = new List<string> ();
		
		private void CheckMethods ()
		{
			// Get the set of all fields/getters used by the equality methods.
			equalityFields.Clear ();
			equalityGetters.Clear ();
			foreach (var info in equalityMethods.Values) {
				equalityFields.UnionWith (info.Fields);
				equalityGetters.UnionWith (info.Getters);
			}
			
			// If an equality or comparison method uses a subset of the 
			// full set then we have a problem.
			badNames.Clear ();
			MethodDefinition first = null;
			
			foreach (var entry in equalityMethods) {
				if (!entry.Value.Delegates && (entry.Value.Fields.Count > 0 || entry.Value.Getters.Count > 0)) {
					if (entry.Value.Fields.Count < equalityFields.Count) {
						first = first ?? entry.Key;
						badNames.Add (entry.Key.ToString ());
					} else if (entry.Value.Getters.Count < equalityGetters.Count) {
						first = first ?? entry.Key;
						badNames.Add (entry.Key.ToString ());
					}
				}
			}
			
			if (badNames.Count > 0) {
				string mesg = string.Format ("Inconsistent: {0}", string.Join (", ", badNames.ToArray ()));
				Log.WriteLine (this, mesg);
				Runner.Report (first, Severity.High, Confidence.Normal, mesg);
			}
			
			// We also have a problem if GetHashCode does not check a 
			// subset of the equality state.
			if (hashMethod != null && !hashInfo.Delegates && (hashInfo.Fields.Count > 0 || hashInfo.Getters.Count > 0)) {
				// Note that if there are no fields or getters then the 
				// equality methods delegate all of their work so we don't
				// don't really know which state they are checking.
				if (equalityFields.Count > 0 || equalityGetters.Count > 0) {
					if (!hashInfo.Fields.IsSubsetOf (equalityFields) || !hashInfo.Getters.IsSubsetOf (equalityGetters)) {
						hashInfo.Fields.ExceptWith (equalityFields);
						hashInfo.Getters.ExceptWith (equalityGetters);
						
						var fnames = (from f in hashInfo.Fields select f.Name).ToArray ();
						var gnames = (from g in hashInfo.Getters select g.Name).ToArray ();
						string mesg = string.Format ("GetHashCode uses fields and/or properties not used by the equality methods: {0} {1}", 
							string.Join (" ", fnames), string.Join (" ", gnames));
						
						Log.WriteLine (this, mesg);
						Runner.Report (hashMethod, Severity.High, Confidence.Normal, mesg);
					}
				}
			}
			
			// We also have a problem if Clone does not use a 
			// superset of the equality state.
			if (cloneMethod != null && !cloneInfo.Delegates && (cloneInfo.Fields.Count > 0 || cloneInfo.Getters.Count > 0)) {
				if (equalityFields.Count > 0 || equalityGetters.Count > 0) {
					if (!cloneInfo.Fields.IsSupersetOf (equalityFields) || !cloneInfo.Getters.IsSupersetOf (equalityGetters)) {
						equalityFields.ExceptWith (cloneInfo.Fields);
						equalityGetters.ExceptWith (cloneInfo.Getters);
						
						var fnames = (from f in equalityFields select f.Name).ToArray ();
						var gnames = (from g in equalityGetters select g.Name).ToArray ();
						string mesg = string.Format ("Clone does not use fields and/or properties used by the equality methods: {0} {1}", 
							string.Join (" ", fnames), string.Join (" ", gnames));
						
						Log.WriteLine (this, mesg);
						Runner.Report (cloneMethod, Severity.High, Confidence.Normal, mesg);
					}
				}
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasMethods || !type.HasFields || type.IsEnum)
				return RuleResult.DoesNotApply;
			
			Log.WriteLine (this);
			Log.WriteLine (this, "------------------------------------");
			Log.WriteLine (this, type.FullName);
			
			GetMethods (type);
			if (equalityMethods.Count > 0) {
				foreach (var entry in equalityMethods)
					ProcessMethod (type, entry.Key, entry.Value);
				if (hashMethod != null)
					ProcessMethod (type, hashMethod, hashInfo);
				if (cloneMethod != null)
					ProcessMethod (type, cloneMethod, cloneInfo);
					
				CheckMethods ();
			
				equalityMethods.Clear ();
				hashMethod = null;
				hashInfo = null;
				cloneMethod = null;
				cloneInfo = null;
			}
			
			return Runner.CurrentRuleResult;
		}
	}
}
