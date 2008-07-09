//
// Gendarme.Rules.Serialization.CallBaseMethodsOnISerializableTypesRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {
	[Problem ("You are overriding the GetObjectData method or serialization constructor but you aren't calling to the base methods, and perhaps you aren't serializing / deserializing the fields of the base type.")]
	[Solution ("Call the base method.")]
	public class CallBaseMethodsOnISerializableTypesRule : Rule, ITypeRule {

		private static bool InheritsFromISerializableImplementation (TypeDefinition type)
		{
			TypeDefinition current = type.BaseType.Resolve ();
			if (current == null || current.FullName == "System.Object")
				return false;
			if (current.IsSerializable && current.Implements ("System.Runtime.Serialization.ISerializable"))
				return true;

			return InheritsFromISerializableImplementation (current);
		}

		private void CheckCallingBaseMethod (TypeDefinition type, MethodSignature methodSignature)
		{
			MethodDefinition method = type.GetMethod (methodSignature);
			if (method == null)
				return; // Perhaps should report that doesn't exist the method (only with ctor).

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl == FlowControl.Call) {
					MethodReference operand = (MethodReference) instruction.Operand;
					if (methodSignature.Matches (operand) && type.Inherits (operand.DeclaringType.ToString ()))
						return;
				}
			}

			Runner.Report (method, Severity.High, Confidence.High, String.Format ("The method {0} isn't calling its base method.", method));
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!InheritsFromISerializableImplementation (type))
				return RuleResult.DoesNotApply;
			
			CheckCallingBaseMethod (type, MethodSignatures.SerializationConstructor);
			CheckCallingBaseMethod (type, MethodSignatures.GetObjectData);

			return Runner.CurrentRuleResult;
		}
	}
}
