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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule checks types that implement the <c>System.ISerializable</c> interface
	/// and fires if either the serialization constructor or the <c>GetObjectData</c>
	/// method does not call it's <c>base</c> type, potentially breaking the serialization
	/// process.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Serializable]
	/// public class Base : ISerializable {
	///	// ...
	/// }
	/// 
	/// [Serializable]
	/// public class Bad : Base {
	///	int value;
	///	
	///	protected BadDerived (SerializationInfo info, StreamingContext context)
	///	{
	///		value = info.GetInt32 ("value");
	///	}
	///	
	///	public override void GetObjectData (SerializationInfo info, StreamingContext context)
	///	{
	///		info.AddValue ("value", value);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Serializable]
	/// public class Base : ISerializable {
	///	// ...
	/// }
	/// 
	/// [Serializable]
	/// public class Good : Base {
	///	int value;
	///	
	///	protected BadDerived (SerializationInfo info, StreamingContext context) : base (info, context)
	///	{
	///		value = info.GetInt32 ("value");
	///	}
	///	
	///	public override void GetObjectData (SerializationInfo info, StreamingContext context)
	///	{
	///		info.AddValue ("value", value);
	///		base.GetObjectData (info, context);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("You are overriding the GetObjectData method or serialization constructor but you aren't calling the base methods, and may not be serializing/deserializing the fields of the base type.")]
	[Solution ("Call the base method or constructor from your own code.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2236:CallBaseClassMethodsOnISerializableTypes")]
	public class CallBaseMethodsOnISerializableTypesRule : Rule, ITypeRule {

		private static bool InheritsFromISerializableImplementation (TypeDefinition type)
		{
			TypeDefinition current = type.BaseType != null ? type.BaseType.Resolve () : null;
			if (current == null || current.IsNamed ("System", "Object"))
				return false;
			if (current.IsSerializable && current.Implements ("System.Runtime.Serialization", "ISerializable"))
				return true;

			return InheritsFromISerializableImplementation (current);
		}

		private void CheckCallingBaseMethod (TypeReference type, MethodSignature methodSignature)
		{
			MethodDefinition method = type.GetMethod (methodSignature);
			if (method == null)
				return; // Perhaps should report that doesn't exist the method (only with ctor).

			// is there any Call or Callvirt instructions in the method
			if (OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method))) {
				// in this case we check further
				foreach (Instruction instruction in method.Body.Instructions) {
					if (instruction.OpCode.FlowControl != FlowControl.Call)
						continue;

					MethodReference operand = (MethodReference) instruction.Operand;
					TypeReference tr = operand.DeclaringType;
					if (methodSignature.Matches (operand) && type.Inherits (tr.Namespace, tr.Name))
						return;
				}
			}

			Runner.Report (method, Severity.High, Confidence.High);
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
