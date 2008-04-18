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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {
	
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

		private MethodDefinition GetSmallerConstructorFrom (TypeDefinition type)
		{
			if (type.Constructors.Count == 1)
				return type.Constructors[0];

			MethodDefinition bigger = null;
			foreach (MethodDefinition constructor in type.Constructors) {
				if (bigger != null) {
					if (bigger.Parameters.Count > constructor.Parameters.Count)
						bigger = constructor;
				}
				else 
					bigger = constructor;
			}
			return bigger;
		}

		private bool HasMoreParametersThanAllowed (MethodDefinition method)
		{
			return method.Parameters.Count >= MaxParameters;
		}

		private void CheckConstructor (MethodDefinition constructor)
		{
			//Skip enums, interfaces, <Module>, static classes ...
			//All stuff that doesn't contain a constructor
			if (constructor == null) 
				return;
			//Skip static constructors
			if ((constructor.Parameters.Count == 0) && !constructor.IsStatic && constructor.IsVisible ())
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
		private IEnumerable<MethodDefinition> GetSmallerOverloaded (TypeDefinition type)
		{
			IDictionary<string, MethodDefinition> possibleOverloaded = new Dictionary<string, MethodDefinition> ();
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsPInvokeImpl)
					continue;
				if (!possibleOverloaded.ContainsKey (method.Name))
					possibleOverloaded.Add (method.Name, method);
				else {
					if (possibleOverloaded[method.Name].Parameters.Count > method.Parameters.Count)
						possibleOverloaded[method.Name] = method;
				}
			}
			return possibleOverloaded.Values;
		}

		private bool OnlyContainsExternalMethods (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods)
				if (!method.IsPInvokeImpl)
					return false;
			return type.Methods.Count != 0;
		}

		private RuleResult CheckDelegate (TypeDefinition type)
		{
			if (HasMoreParametersThanAllowed (type.GetMethod ("Invoke")))
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

			CheckConstructor (GetSmallerConstructorFrom (type));
			
			foreach (MethodDefinition method in GetSmallerOverloaded (type)) 
				CheckMethod (method);

			return Runner.CurrentRuleResult;
		}
	}
}
