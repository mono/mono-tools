// 
// Gendarme.Rules.Design.Generic.DoNotExposeGenericListsRule
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// A type has an externally visible member that is, returns or has a signature containing a 
	/// <c>System.Collections.Generic.List&lt;T&gt;</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BadClass {
	///	public List&lt;string&gt; member { get; set; };
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class GoodClass {
	///	public Collection&lt;string&gt; member { get; set; };
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("The type exposes System.Collections.Generic.List<T>.")]
	[Solution ("Use a type such as System.Collections.ObjectModel.Collection<T> instead.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
	public class DoNotExposeGenericListsRule : GenericsBaseRule, ITypeRule {
		private const string List = "List`1";

		private static bool IsList (TypeReference type)
		{			
			return type.Namespace == "System.Collections.Generic" &&
				type.Name.StartsWith (List, StringComparison.Ordinal);
		}

		private void CheckField (FieldReference field)
		{
			if (field.IsVisible () && IsList (field.FieldType))
				Runner.Report (field, Severity.Medium, Confidence.Total);
		}

		private void CheckProperty (PropertyDefinition property)
		{
			if (!IsList (property.PropertyType))
				return;

			if (property.GetMethod.IsVisible () || property.SetMethod.IsVisible ())
				Runner.Report (property, Severity.Medium, Confidence.Total);
		}

		private void CheckMethod (MethodDefinition method)
		{
			// Getters/setters handled by CheckProperty.
			if (method.IsGetter || method.IsSetter || !method.IsVisible ())
				return;
			if (IsList (method.ReturnType))
				Runner.Report (method, Severity.Medium, Confidence.Total);
			if (!method.HasParameters)
				return;
			foreach (var param in method.Parameters) {
				if (IsList (param.ParameterType))
					Runner.Report (param, Severity.Medium, Confidence.Total);
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			if (type.HasFields) {
				foreach (var field in type.Fields)
					CheckField (field);
			}
			if (type.HasProperties) {
				foreach (var property in type.Properties)
					CheckProperty (property);
			}
			if (type.HasMethods) {
				foreach (var method in type.Methods)
					CheckMethod (method);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
