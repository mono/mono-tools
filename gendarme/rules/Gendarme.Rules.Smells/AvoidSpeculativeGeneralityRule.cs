//
// Gendarme.Rules.Smells.AvoidSpeculativeGeneralityRule class
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Performance;

namespace Gendarme.Rules.Smells {
	public class AvoidSpeculativeGeneralityRule : ITypeRule {

		private MessageCollection messageCollection;

		private void CheckAbstractClassWithoutResponsability (TypeDefinition type)
		{
			if (type.IsAbstract) {
				ICollection inheritedClasses = Utilities.GetInheritedClassesFrom (type);
				if (inheritedClasses.Count == 1)
					AddMessage (type, "This abstract class only has one class inheritting from.  The abstract classes without responsability are a sign for the Speculative Generality smell.");
			}
		}

		private void AddMessage (TypeDefinition type, string summary)
		{
			Location location = new Location (type);
			Message message = new Message (summary, location, MessageType.Error);
			if (messageCollection == null)
				messageCollection = new MessageCollection ();
			messageCollection.Add (message);
		}
		
		//return true if the method only contains only a single call.
		private static bool OnlyDelegatesCall (MethodDefinition method)
		{
			if (!method.HasBody)
				return false;
			bool onlyOneCallInstruction = false;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Calli || instruction.OpCode.Code == Code.Callvirt)
					if (onlyOneCallInstruction)
						return false;
					else
						onlyOneCallInstruction = true;
			}

			return onlyOneCallInstruction;
		}

		private static bool InheritsOnlyFromObject (TypeDefinition type)
		{
			return type.BaseType.FullName == "System.Object" && type.Interfaces.Count == 0;
		}

		private static bool MostlyMethodsDelegatesCall (TypeDefinition type)
		{
			int delegationCounter = 0;
			foreach (MethodDefinition method in type.Methods) {
				if (OnlyDelegatesCall (method))
					delegationCounter++;
			}
			
			return type.Methods.Count / 2 + 1 <= delegationCounter;
		}

		private void CheckUnnecesaryDelegation (TypeDefinition type)
		{
			if (MostlyMethodsDelegatesCall (type) && InheritsOnlyFromObject (type))
				AddMessage (type, "This class contains a lot of methods that only delgates the call to other.  This kind of Delegation could be a sign for Speculative Generality");
		}

		private static bool AvoidUnusedParametersRuleScheduled (Runner runner)
		{
			foreach (IMethodRule rule in runner.Rules.Method) {
				if (rule is AvoidUnusedParametersRule)
					return true;
			}
			return false;
		}

		private void AddExistingMessages (MessageCollection existingMessages) {
			if (existingMessages == null)
				return;

			foreach (Message violation in existingMessages) {
				Message message = new Message ("This method contains unused parameters.  This is a sign for the Speculative Generality smell.",violation.Location, MessageType.Error);
				if (messageCollection == null)
					messageCollection = new MessageCollection ();
				messageCollection.Add (message);
			}
		}

		private void CheckMethods (IMethodRule rule, ICollection methods, Runner runner)
		{
			foreach (MethodDefinition method in methods) {
				AddExistingMessages (rule.CheckMethod (method, runner));
			}
		}

		private void CheckUnusedParameters (TypeDefinition type, Runner runner)
		{
			IMethodRule avoidUnusedParameters = new AvoidUnusedParametersRule ();
			CheckMethods (avoidUnusedParameters, type.Methods, runner);
			CheckMethods (avoidUnusedParameters, type.Constructors, runner);
		}

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			if (type.IsGeneratedCode ())
				return runner.RuleSuccess;

			messageCollection = null;

			CheckAbstractClassWithoutResponsability (type);
			if (!AvoidUnusedParametersRuleScheduled (runner))
				CheckUnusedParameters (type, runner);

			CheckUnnecesaryDelegation (type);
			if (messageCollection == null || messageCollection.Count == 0)
				return runner.RuleSuccess;
			return messageCollection;
		}
	}
}
