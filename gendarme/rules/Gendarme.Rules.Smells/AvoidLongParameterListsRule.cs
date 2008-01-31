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
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Smells {
	//SUGGESTION: Setting all required properties in a constructor isn't
	//uncommon.
	//SUGGESTION: Different value for public / private / protected methods *may*
	//be useful.
	public class AvoidLongParameterListsRule : IMethodRule {
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
			if (method.DeclaringType is TypeDefinition) {
				TypeDefinition type = (TypeDefinition) method.DeclaringType;
				MethodReference shortestOverloaded = (MethodReference) method;
				foreach (MethodReference overloadedMethod in type.Methods.GetMethod (method.Name)) {
					if (overloadedMethod.Parameters.Count < shortestOverloaded.Parameters.Count)
						shortestOverloaded = overloadedMethod;
				}
				return shortestOverloaded;
			}
			return method;
		}

		private bool HasLongParameterList (MethodDefinition method)
		{
			if (IsOverloaded (method))
				return GetShortestOverloaded (method).Parameters.Count >= MaxParameters;
			else
				return method.Parameters.Count >= MaxParameters;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// we don't control, nor report, p/invoke declarations - someimes the poor C 
			// guys don't have a choice to make long parameter lists ;-)
			if (method.IsPInvokeImpl)
				return runner.RuleSuccess;

			if (HasLongParameterList (method)) {
				MessageCollection messageCollection = new MessageCollection ();
				Location location = new Location (method);
				Message message = new Message ("The method contains a long parameter list.",location, MessageType.Error);
				messageCollection.Add (message);
				return messageCollection;
			}

			return runner.RuleSuccess;
		}
	}
}
