//
// Gendarme.Rules.Serialization.ImplementISerializableCorrectlyRule
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
using System.Collections.Generic;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {
	[Problem ("Although you are implementing the ISerializable interface, there are some fields that aren't going to be serialized and aren't marked with the [NonSerialized] attribute.")]
	[Solution ("Mark with the [NonSerialized] attribute the field. This helps developers to understand better your code, and perhaps to discover quickly some errors.")]
	public class ImplementISerializableCorrectlyRule : Rule, ITypeRule {
		static MethodSignature addValueSignature = new MethodSignature ("AddValue", "System.Void");
	
		private static bool IsCallingToSerializationInfoAddValue (Instruction instruction)
		{
			if (instruction == null) 
				return false; 
			
			//Advance towards the next call	
			//Matches the cases of overloaded or boxed instructions
			Instruction current = (instruction);
			while (current != null) {
				if (current.OpCode.FlowControl == FlowControl.Call){
					MethodReference method = (MethodReference) current.Operand;
					if (addValueSignature.Matches (method) && String.Compare (method.DeclaringType.FullName, "System.Runtime.Serialization.SerializationInfo") == 0)
						return true;
				}
						
				current = current.Next;
			}

			return false;
		}

		private static FieldReference GetReferenceThroughProperty (TypeReference current, Instruction instruction)
		{
			if (instruction.OpCode.FlowControl != FlowControl.Call)
				return null;
			
			MethodDefinition target = instruction.Operand as MethodDefinition;
			if (target == null)
				return null;

			if (target.DeclaringType != current || !target.IsGetter || !target.HasBody)
				return null;//Where are you calling dude?

			FieldReference reference = null;
			foreach (Instruction each in target.Body.Instructions) {
				if (each.OpCode == OpCodes.Ldfld)
					reference = (FieldReference) each.Operand;
			}
			//Return the nearest to the ret instruction.
			return reference;
		}

		private static FieldReference GetFieldReference (Instruction instruction, MethodDefinition method)
		{
			if (instruction.OpCode == OpCodes.Ldfld)
				return (FieldReference) instruction.Operand;
			return GetReferenceThroughProperty (method.DeclaringType, instruction);
		}

		private static IList<FieldReference> GetFieldsUsedIn (MethodDefinition method)
		{
			IList<FieldReference> result = new List<FieldReference> ();

			foreach (Instruction instruction in method.Body.Instructions) {
				FieldReference reference = GetFieldReference (instruction, method);
				if (reference != null && IsCallingToSerializationInfoAddValue (instruction.Next)) 
					result.Add (reference);
			}
			return result;
		}

		private void CheckUnusedFieldsIn (TypeDefinition type, MethodDefinition getObjectData)
		{
			IList<FieldReference> fieldsUsed = GetFieldsUsedIn (getObjectData);
			
			foreach (FieldDefinition field in type.Fields) {
				if (!fieldsUsed.Contains (field) && !field.IsNotSerialized && !field.IsStatic)
					Runner.Report (type, Severity.Medium, Confidence.High, String.Format ("The field {0} isn't going to be serialized, please use the [NonSerialized] attribute.", field.Name));
			}
		}

		private void CheckExtensibilityFor (TypeDefinition type, MethodDefinition getObjectData)
		{
			if (!type.IsSealed && getObjectData.IsFinal)
				Runner.Report (type, Severity.High, Confidence.Total, "If this class is going to be sealed, seal it; else you should make virtual the GetObjectData method.");
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsSerializable || !type.Implements ("System.Runtime.Serialization.ISerializable"))
				return RuleResult.DoesNotApply;
			
			MethodDefinition getObjectData = type.GetMethod (MethodSignatures.GetObjectData);
			if (getObjectData != null) {
				CheckUnusedFieldsIn (type, getObjectData);
				CheckExtensibilityFor (type, getObjectData);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
