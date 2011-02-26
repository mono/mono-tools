// 
// Gendarme.Rules.BadPractice.ObsoleteMessagesShouldNotBeEmptyRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule warns if any type (including classes, structs, enums, interfaces and 
	/// delegates), field, property, events, method and constructor are decorated with
	/// an empty <c>[Obsolete]</c> attribute because the attribute is much more helpful
	/// if it includes advice on how to deal with the situation (e.g. the new recommended
	/// API to use).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Obsolete]
	/// public byte[] Key {
	///	get {
	///		return (byte[]) key.Clone ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Obsolete ("Use the new GetKey() method since properties should not return arrays.")]
	/// public byte[] Key {
	///	get {
	///		return (byte[]) key.Clone ();
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The [Obsolete] attribute was used but no help, alternative or description was provided.")]
	[Solution ("Provide advice to help developers abandon old features and migrate to newer ones.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1041:ProvideObsoleteAttributeMessage")]
	public class ObsoleteMessagesShouldNotBeEmptyRule : Rule, ITypeRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not have a reference to System.ObsoleteAttribute 
			// then nothing will be marked as obsolete inside it
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System", "ObsoleteAttribute");
					}));
			};
		}

		private void CheckAttributes (ICustomAttributeProvider cap)
		{
			if (!cap.HasCustomAttributes)
				return;

			foreach (CustomAttribute ca in cap.CustomAttributes) {
				// ObsoleteAttribute has a three (3) ctors, including a default (parameter-less) ctor
				// http://msdn.microsoft.com/en-us/library/68k270ch.aspx
				if (!ca.AttributeType.IsNamed ("System", "ObsoleteAttribute"))
					continue;

				// note: we don't have to check fields since they cannot be used
				// (as the Message property isn't read/write it cannot be a named argument)

				// no parameter == empty description
				// note: Message is the first parameter in both ctors (with params)
				if (!ca.HasConstructorArguments || String.IsNullOrEmpty ((string) ca.ConstructorArguments [0].Value))
					Runner.Report ((IMetadataTokenProvider) cap, Severity.Medium, Confidence.High);
			}
			// no System.ObsoleteAttribute found inside the collection
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// handles AttributeTargets.[Class | Struct | Enum | Interface | Delegate]
			CheckAttributes (type);

			// handles AttributeTargets.Property
			// properties can be obsoleted - but this is different
			// than the getter/setter that CheckMethod will report
			if (type.HasProperties) {
				foreach (PropertyDefinition property in type.Properties) {
					CheckAttributes (property);
				}
			}

			// handle AttributeTargets.Event
			if (type.HasEvents) {
				foreach (EventDefinition evnt in type.Events) {
					CheckAttributes (evnt);
				}
			}

			// handle AttributeTargets.Field
			if (type.HasFields) {
				foreach (FieldDefinition field in type.Fields) {
					CheckAttributes (field);
				}
			}

			// handles AttributeTargets.Method
			if (type.HasMethods) {
				foreach (MethodDefinition method in type.Methods) {
					CheckAttributes (method);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
