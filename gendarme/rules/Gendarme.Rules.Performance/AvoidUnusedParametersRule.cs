//
// Gendarme.Rules.Performance.AvoidUnusedParameters class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
//  (C) 2007 Néstor Salceda
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

namespace Gendarme.Rules.Performance {

	public class AvoidUnusedParametersRule : IMethodRule {

		private static bool UseParameter (MethodDefinition method, ParameterDefinition parameter)
		{
			if (!method.HasBody)
				return false;
			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ldarg_0:
					if (method.Parameters.IndexOf (parameter) == 0)
						return true;
					break;
				case Code.Ldarg_1:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 1 : 0))
						return true;
					break;
				case Code.Ldarg_2:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 2 : 1))
						return true;
					break;
				case Code.Ldarg_3:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 3 : 2))
						return true;
					break;
				case Code.Ldarg_S:
				case Code.Ldarga:
				case Code.Ldarga_S:
					if (instruction.Operand.Equals (parameter))
						return true;
					break;
				default:
					break;
				}
			}
			return false;
		}

		private static bool ContainsReferenceDelegateInstructionFor (MethodDefinition method, MethodDefinition delegateMethod)
		{
			if (!method.HasBody)
				return false;
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Ldftn)
					return instruction.Operand.Equals (delegateMethod);
			}
			return false;
		}

		private static bool IsReferencedByDelegate (MethodDefinition delegateMethod)
		{
			TypeDefinition declaringType = delegateMethod.DeclaringType as TypeDefinition;
			if (declaringType != null) {
				foreach (MethodDefinition method in declaringType.Methods) {
					if (ContainsReferenceDelegateInstructionFor (method, delegateMethod))
						return true;
				}

				foreach (MethodDefinition method in declaringType.Constructors) {
					if (ContainsReferenceDelegateInstructionFor (method, delegateMethod))
						return true;
				}
			}
			return false;
		}

		private static ICollection GetUnusedParameters (MethodDefinition method)
		{
			ArrayList unusedParameters = new ArrayList ();
			foreach (ParameterDefinition parameter in method.Parameters) {
				if (!UseParameter (method, parameter))
					unusedParameters.Add (parameter);
			}
			return unusedParameters;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// catch abstract, pinvoke and icalls - where rule does not apply
			if (!method.HasBody)
				return runner.RuleSuccess;

			// rule doesn't apply to virtual, overrides or generated code
			if (method.IsVirtual || method.Overrides.Count != 0 || method.IsGeneratedCode ())
				return runner.RuleSuccess;

			// doesn't apply to code referenced by delegates (note: more complex check moved last)
			if (IsReferencedByDelegate (method))
				return runner.RuleSuccess;

			// rule applies

			MessageCollection mc = null;
			foreach (ParameterDefinition parameter in GetUnusedParameters (method)) {
				Location location = new Location (method);
				Message message = new Message (String.Format ("The parameter {0} is never used.", parameter.Name), location, MessageType.Error);
				if (mc == null)
					mc = new MessageCollection (message);
				else
					mc.Add (message);
			}
			return mc;
		}
	}
}
