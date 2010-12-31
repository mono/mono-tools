//
// Gendarme.Rules.Smells.AvoidLongParameterListsRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	// TODO: What does "Other time, it's quite hard determine a long parameter list" mean?
	// Can the default be changed? This rule should may need to be reviewed if C# adds
	// default arguments. The solution is really confusing. What is "preserve whole object"?
	// The introduction of a helper class to bundle state together? What is "parameter object"?
	// The params keyword?
	
	/// <summary>
	/// This rule allows developers to measure the parameter list size in a method.
	/// If you have methods with a lot of parameters, perhaps you have a Long
	/// Parameter List smell.
	/// 
	/// This rule counts the method's parameters, and compares it against a maximum value. 
	/// If you have an overloaded method, then the rule will get the shortest overload 
	/// and compare the shortest overload against the maximum value.
	///
	/// Other time, it's quite hard determine a long parameter list. By default, 
	/// a method with 6 or more arguments will be flagged as a defect. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f,
	///                                         float z, double u, short s, int v, string[] array)
	/// {
   	/// 	// Method body ... 
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void MethodWithoutLongParameterList (int x, object obj)
	/// {
	/// 	// Method body.... 
	/// }
	/// </code>
	/// </example>

	//SUGGESTION: Setting all required properties in a constructor isn't
	//uncommon.
	//SUGGESTION: Different value for public / private / protected methods *may*
	//be useful.
	[Problem ("Generally, long parameter lists are hard to understand because they become hard to use and inconsistent.  And you will be forever changing them if you need more data.")]
	[Solution ("You should apply the Replace parameter with method refactoring, or preserve whole object or introduce parameter object")]
	public class AvoidLongParameterListsRule : Rule, ITypeRule {
		private int maxParameters = 6;

		public int MaxParameters {
			get {
				return maxParameters;
			}
			set {
				maxParameters = value;
			}
		}

		private static MethodDefinition GetSmallestConstructorFrom (TypeDefinition type)
		{
			MethodDefinition smallest = null;
			int scount = 0;
			foreach (MethodDefinition constructor in type.Methods) {
				if (!constructor.IsConstructor)
					continue;

				// skip the static ctor since it will always be the smallest one
				if (constructor.IsStatic)
					continue;

				if (smallest == null) {
					smallest = constructor;
					scount = smallest.HasParameters ? smallest.Parameters.Count : 0;
				} else {
					int ccount = constructor.HasParameters ? constructor.Parameters.Count : 0;
					if (scount > ccount) {
						smallest = constructor;
						scount = ccount;
					}
				}
			}
			return smallest;
		}

		private bool HasMoreParametersThanAllowed (IMethodSignature method)
		{
			return (method.HasParameters ? method.Parameters.Count : 0) >= MaxParameters;
		}

		private void CheckConstructor (MethodDefinition constructor)
		{
			//Skip enums, interfaces, <Module>, static classes ...
			//All stuff that doesn't contain a constructor
			if (constructor == null) 
				return;
			if (HasMoreParametersThanAllowed (constructor)) 
				Runner.Report (constructor, Severity.Medium, Confidence.Normal, "This constructor contains a long parameter list.");
		}

		private void CheckMethod (MethodDefinition method)
		{
			if (HasMoreParametersThanAllowed (method))
				Runner.Report (method, Severity.Medium, Confidence.Normal, "This method contains a long parameter list.");
		}

		//TODO: Perhaps we can perform this action with linq instead of
		//loop + hashtable
		private static IEnumerable<MethodDefinition> GetSmallestOverloaded (TypeDefinition type)
		{
			IDictionary<string, MethodDefinition> possibleOverloaded = new Dictionary<string, MethodDefinition> ();
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor || method.IsPInvokeImpl)
					continue;

				string name = method.Name;
				if (!possibleOverloaded.ContainsKey (name))
					possibleOverloaded.Add (name, method);
				else {
					MethodDefinition candidate = possibleOverloaded [name];
					int ccount = candidate.HasParameters ? candidate.Parameters.Count : 0;
					int mcount = method.HasParameters ? method.Parameters.Count : 0;
					if (ccount > mcount)
						possibleOverloaded [name] = method;
				}
			}
			return possibleOverloaded.Values;
		}

		private static bool OnlyContainsExternalMethods (TypeDefinition type)
		{
			bool has_methods = false;
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				has_methods = true;
				if (!method.IsPInvokeImpl)
					return false;
			}
			// all methods are p/invoke
			return has_methods;
		}

		private RuleResult CheckDelegate (TypeReference type)
		{
			MethodDefinition method = type.GetMethod ("Invoke");
			// MulticastDelegate inherits from Delegate without overriding Invoke
			if ((method != null) && HasMoreParametersThanAllowed (method))
				Runner.Report (type, Severity.Medium, Confidence.Normal, "This delegate contains a long parameter list.");
			return Runner.CurrentRuleResult;
		}
		
		public RuleResult CheckType (TypeDefinition type)
		{
			// we don't control, nor report, p/invoke declarations - sometimes the poor C 
			// guys don't have a choice to make long parameter lists ;-)
			if (OnlyContainsExternalMethods (type))
				return RuleResult.DoesNotApply;
			
			if (type.IsDelegate ())
				return CheckDelegate (type);

			if (type.HasMethods) {
				CheckConstructor (GetSmallestConstructorFrom (type));

				foreach (MethodDefinition method in GetSmallestOverloaded (type)) 
					CheckMethod (method);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
