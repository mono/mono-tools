//
// Gendarme.Rules.Serialization.ImplementISerializableCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// 	(C) 2008 Néstor Salceda
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

	/// <summary>
	/// This rule checks for types that implement <c>ISerializable</c>. Such types
	/// serialize their data by implementing <c>GetObjectData</c>. This
	/// rule verifies that every instance field, not decorated with the <c>[NonSerialized]</c>
	/// attribute is serialized by the <c>GetObjectData</c> method. This rule will also warn
	/// if the type is unsealed and the <c>GetObjectData</c> is not <c>virtual</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Serializable]
	/// public class Bad : ISerializable {
	///	int foo;
	///	string bar;
	///	
	///	protected Bad (SerializationInfo info, StreamingContext context)
	///	{
	///		foo = info.GetInt32 ("foo");
	///	}
	///	
	///	// extensibility is limited since GetObjectData is not virtual:
	///	// any type inheriting won't be able to serialized additional fields
	///	public void GetObjectData (SerializationInfo info, StreamingContext context)
	///	{
	///		info.AddValue ("foo", foo);
	///		// 'bar' is not serialized, if not needed then the field should
	///		// be decorated with [NotSerialized]
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (virtual and not serialized):
	/// <code>
	/// [Serializable]
	/// public class Good : ISerializable {
	///	int foo;
	///	[NotSerialized]
	///	string bar;
	///	
	///	protected Good (SerializationInfo info, StreamingContext context)
	///	{
	///		foo = info.GetInt32 ("foo");
	///	}
	///	
	///	public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
	///	{
	///		info.AddValue ("foo", foo);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (sealed type and serialized):
	/// <code>
	/// [Serializable]
	/// public sealed class Good : ISerializable {
	///	int foo;
	///	string bar;
	///	
	///	protected Good (SerializationInfo info, StreamingContext context)
	///	{
	///		foo = info.GetInt32 ("foo");
	///	}
	///	
	///	public void GetObjectData (SerializationInfo info, StreamingContext context)
	///	{
	///		info.AddValue ("foo", foo);
	///		info.AddValue ("bar", bar);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("Although you are implementing the ISerializable interface, there are some fields that aren't going to be serialized and aren't marked with the [NonSerialized] attribute.")]
	[Solution ("Either add the [NonSerialized] attribute to the field or serialize it. This will help developers better understand your code and make errors easier to find.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
	public class ImplementISerializableCorrectlyRule : Rule, ITypeRule {

		private HashSet<FieldDefinition> fields = new HashSet<FieldDefinition> ();

		static private FieldDefinition CheckProperty (MethodDefinition getter)
		{
			TypeReference return_type = getter.ReturnType;
			foreach (Instruction ins in getter.Body.Instructions) {
				if (ins.OpCode.OperandType != OperandType.InlineField)
					continue;
				FieldDefinition field = (ins.Operand as FieldDefinition);
				if ((field != null) && field.FieldType.IsNamed (return_type.Namespace, return_type.Name))
					return field;
			}
			return null;
		}

		private void CheckSerializedFields (MethodDefinition method)
		{
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference mr = ins.Operand as MethodReference;
					if (!mr.HasParameters || (mr.Name != "AddValue") || (mr.Parameters.Count < 2))
						continue;
					// type is sealed so this check is ok
					if (!mr.DeclaringType.IsNamed ("System.Runtime.Serialization", "SerializationInfo"))
						continue;

					// look at the second parameter, which should be (or return) the field
					Instruction i = ins.TraceBack (method, -2);
					// if we're boxing then find what's in that box
					if (i.OpCode.Code == Code.Box)
						i = i.TraceBack (method);

					FieldDefinition f = (i.Operand as FieldDefinition);
					if (f != null) {
						fields.Remove (f);
						continue;
					}
					MethodDefinition md = (i.Operand as MethodDefinition);
					if ((md != null) && md.IsGetter && md.HasBody) {
						f = CheckProperty (md);
						if (f != null)
							fields.Remove (f);
					}
					break;
				}
			}
		}

		private void CheckUnusedFieldsIn (TypeDefinition type, MethodDefinition getObjectData)
		{
			// build a list of the fields that needs to be serialized
			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsNotSerialized && !field.IsStatic)
					fields.Add (field);
			}

			// remove all fields that are serialized
			CheckSerializedFields (getObjectData);

			// report all fields that have not been serialized
			foreach (FieldDefinition field in fields) {
				Runner.Report (field, Severity.Medium, Confidence.Normal);
			}

			fields.Clear ();
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsSerializable || !type.Implements ("System.Runtime.Serialization", "ISerializable"))
				return RuleResult.DoesNotApply;

			MethodDefinition getObjectData = type.GetMethod (MethodSignatures.GetObjectData);
			if (getObjectData == null) {
				// no GetObjectData means that the type's ancestor does the job but 
				// are we introducing new instance fields that need to be serialized ?
				if (!type.HasFields)
					return RuleResult.Success;
				// there are some, but they could be static
				foreach (FieldDefinition field in type.Fields) {
					if (!field.IsStatic)
						Runner.Report (field, Severity.Medium, Confidence.High);
				}
			} else {
				if (type.HasFields)
					CheckUnusedFieldsIn (type, getObjectData);

				if (!type.IsSealed && getObjectData.IsFinal) {
					string msg = "Either seal this type or change GetObjectData method to be virtual";
					Runner.Report (getObjectData, Severity.High, Confidence.Total, msg);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
