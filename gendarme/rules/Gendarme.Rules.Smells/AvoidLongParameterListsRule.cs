//
// Gendarme.Rules.Smells.AvoidLongParameterListsRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
	public class AvoidLongParameterListsRule : Rule,IMethodRule {
		private int maxParameters = 6;

		public int MaxParameters {
			get {
				return maxParameters;
			}
			set {
				maxParameters = value;
			}
		}

		private static bool IsOverloaded (MethodDefinition method)
		{
			if (method.DeclaringType is TypeDefinition) {
				TypeDefinition type = (TypeDefinition) method.DeclaringType;
				return type.Methods.GetMethod (method.Name).Length > 1;
			}
			return false;
		}

		private static MethodReference GetShortestOverloaded (MethodDefinition method)
		{
			TypeDefinition type = method.DeclaringType.Resolve ();
			MethodReference shortestOverloaded = (MethodReference) method;
			foreach (MethodReference overloadedMethod in type.Methods.GetMethod (method.Name)) {
				if (overloadedMethod.Parameters.Count < shortestOverloaded.Parameters.Count)
					shortestOverloaded = overloadedMethod;
			}
			return shortestOverloaded;
		}

		private bool HasLongParameterList (MethodDefinition method)
		{
			if (method.IsConstructor) {
				// no need to call IsOverloaded since all constructors share the same name
				foreach (MethodDefinition ctor in method.DeclaringType.Resolve ().Constructors) {
					// if we have a default, non-static, visible ctor then it's not too long
					if ((ctor.Parameters.Count == 0) && !ctor.IsStatic && ctor.IsVisible ())
						return false;
					// it's also ok if we find a ctor with less than the maximum number of parameters
					if (ctor.Parameters.Count <= MaxParameters)
						return false;
				}
				// no default (visible) ctor and no ctor with less than the maximum number of parameters
				return true;
			} else if (IsOverloaded (method)) {
				return GetShortestOverloaded (method).Parameters.Count >= MaxParameters;
			} else {
				return method.Parameters.Count >= MaxParameters;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// we don't control, nor report, p/invoke declarations - someimes the poor C 
			// guys don't have a choice to make long parameter lists ;-)
			if (method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			if (HasLongParameterList (method)) 
				Runner.Report (method, Severity.Medium, Confidence.Normal, "This method contains a long parameter list.");

			return Runner.CurrentRuleResult;
		}
	}
}
