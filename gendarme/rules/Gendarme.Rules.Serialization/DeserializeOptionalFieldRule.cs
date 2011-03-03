// 
// Gendarme.Rules.Serialization.DeserializeOptionalFieldRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule will fire if a type has fields marked with <c>[OptionalField]</c>, but does
	/// not have methods decorated with the <c>[OnDeserialized]</c> or <c>[OnDeserializing]</c> 
	/// attributes. This is a problem because the binary deserializer does not actually construct
	/// objects (it uses <c>System.Runtime.Serialization.FormatterServices.GetUninitializedObject</c>
	/// instead). So, if binary deserialization is used the optional field(s) will be zeroed instead 
	/// of properly initialized.
	/// This rule only applies to assemblies compiled with the .NET framework version 2.0 
	/// (or later).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Serializable]
	/// public class ClassWithOptionalField {
	///	[OptionalField]
	///	private int optional;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Serializable]
	/// public class ClassWithOptionalField {
	/// 	// Normally the (compiler generated) default constructor will
	/// 	// initialize this. The default constructor will be called by the
	/// 	// XML and Soap deserializers, but not the binary serializer.
	/// 	[OptionalField]
	/// 	private int optional = 1;
	/// 	
	/// 	// This will be called immediately after the object is
	/// 	// deserialized. 
	/// 	[OnDeserializing]
	/// 	private void OnDeserializing (StreamingContext context)
	/// 	{
	/// 		optional = 1;
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("Some fields are marked with [OptionalField] but the type does not provide special deserialization routines.")]
	[Solution ("Add a deserialization routine, marked with [OnDeserialized], and re-compute the correct value for the optional fields.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2239:ProvideDeserializationMethodsForOptionalFields")]
	public class DeserializeOptionalFieldRule : Rule, ITypeRule {

		private const string MessageOptional = "Optional fields '{0}' is not deserialized.";
		private const string MessageSerializable = "Optional fields '{0}' in non-serializable type.";

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += (object o, RunnerEventArgs e) => {
				Active = 
					// the [OptionalField] and deserialization attributes are only available 
					// since fx 2.0 so there's no point to execute it on every methods if the 
					// assembly target runtime is earlier than 2.0
					e.CurrentModule.Runtime >= TargetRuntime.Net_2_0 &&
					
					// if the module does not have a reference to System.Runtime.Serialization.OptionalFieldAttribute
					// then nothing will be reported by this rule
					(e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Runtime.Serialization", "OptionalFieldAttribute");
					})
				);
			};
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// [OptionalField] is usable only if the type has fields
			if (!type.HasFields)
				return RuleResult.DoesNotApply;

			// look in methods for a deserialization candidates
			bool deserialized_candidate = false;
			bool deserializing_candidate = false;
			if (type.HasMethods) {
				foreach (MethodDefinition method in type.Methods) {
					if (method.IsConstructor || !method.HasCustomAttributes)
						continue;

					if (method.HasAttribute ("System.Runtime.Serialization", "OnDeserializedAttribute"))
						deserialized_candidate = true;
					if (method.HasAttribute ("System.Runtime.Serialization", "OnDeserializingAttribute"))
						deserializing_candidate = true;

					if (deserialized_candidate && deserializing_candidate)
						break;
				}
			}

			// check if we found some optional fields, if none then it's all ok
			foreach (FieldDefinition field in type.Fields) {
				if (field.HasAttribute ("System.Runtime.Serialization", "OptionalFieldAttribute")) {
					if (type.IsSerializable) {
						// report if we didn't find a deserialization method
						if (!deserialized_candidate || !deserializing_candidate) {
							// Medium since it's possible that the optional fields don't need to be re-computed
							string s = String.Format (CultureInfo.InvariantCulture, MessageOptional, field.Name);
							Runner.Report (field, Severity.Medium, Confidence.High, s);
						}
					} else {
						// [OptionalField] without [Serializable] is a bigger problem
						string s = String.Format (CultureInfo.InvariantCulture, MessageSerializable, field.Name);
						Runner.Report (field, Severity.Critical, Confidence.High, s);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
