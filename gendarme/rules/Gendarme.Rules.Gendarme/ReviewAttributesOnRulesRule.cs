// 
// Gendarme.Rules.Gendarme.ReviewAttributesOnRulesRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Text.RegularExpressions;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Gendarme {

	/// <summary>
	/// This rule checks if attribute usage match the following rules:	
	/// <list>
	/// <item>
	/// <term>[Problem] and [Solution] attributes</term>
	/// <description>should be used on rules only, every concrete rule must have both 
	/// attributes (or inherit them), and their arguments cannot be null or empty
	/// </description>
	/// </item>
	/// <item>
	/// <term>[FxCopCompatibility] attribute</term>
	/// <description>should be used on rules only, its arguments cannot be null or empty,
	/// and second argument should match this format: AB1234:FxCopRuleName</description>
	/// </item>
	/// <item>
	/// <term>[EngineDependency] attribute</term>
	/// <description>should be used on rules only, its argument cannot be null or empty,
	/// and its argument should inherit from Gendarme.Framework.Engine</description>
	/// </item>
	/// <item>
	/// <term>[DocumentationUri] attribute</term>
	/// <description>should be used on rules only</description>
	/// </item>
	/// <item>
	/// <term>[Description] and [DefaultValue] attributes</term>
	/// <description>should be used on rules' public properties only,
	/// Description attribute argument cannot be null or empty</description>
	/// </item>
	/// </list>
	/// </summary>

	[Problem ("Attributes should be correctly placed and have correct values provided in their arguments")]
	[Solution ("Change the code so that it satisfies attribute usage rules")]
	public class ReviewAttributesOnRulesRule : Rule, ITypeRule {

		Dictionary<string, Action<CustomAttribute, ICustomAttributeProvider>> attributes;

		Dictionary<string, bool> typeIsRule = new Dictionary<string, bool> ();

		public ReviewAttributesOnRulesRule ()
		{
			attributes = new Dictionary<string, Action<CustomAttribute, ICustomAttributeProvider>>
			{
				{"Gendarme.Framework.ProblemAttribute", CheckProblemAndSolutionAttributes},
				{"Gendarme.Framework.SolutionAttribute", CheckProblemAndSolutionAttributes},
				{"Gendarme.Framework.FxCopCompatibilityAttribute", CheckFxCopCompatibilityAttribute},
				{"Gendarme.Framework.EngineDependencyAttribute", CheckEngineDependencyAttribute},
				{"Gendarme.Framework.DocumentationUriAttribute", CheckIfAttributeUsedOnRule},
				{"System.ComponentModel.DescriptionAttribute", CheckDescriptionAttribute},
				{"System.ComponentModel.DefaultValueAttribute", CheckIfAttributeUsedOnRulesProperty},
			};
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			CheckAttributes (type);

			if (type.HasMethods)
				foreach (MethodDefinition method in type.Methods)
					CheckAttributes (method);

			if (type.HasProperties)
				foreach (PropertyDefinition property in type.Properties)
					CheckAttributes (property);
			if (type.HasFields)
				foreach (FieldDefinition field in type.Fields)
					CheckAttributes (field);

			// finally check if this is a rule and either has all required attributes
			// or inherits from a type that has all required attributes
			CheckIfRuleHasAllRequiredAttributes (type);

			return Runner.CurrentRuleResult;
		}

		private void CheckIfRuleHasAllRequiredAttributes (TypeDefinition type)
		{
			if (!type.IsAbstract && IsRule (type)) {
				bool foundSolution = false;
				bool foundProblem = false;
				TypeDefinition td = type;
				while (!foundSolution || !foundProblem) {
					if (td.HasCustomAttributes)
						foreach (CustomAttribute attribute in td.CustomAttributes) {
							var attributeTypeName = attribute.AttributeType.FullName;
							if (attributeTypeName == "Gendarme.Framework.SolutionAttribute")
								foundSolution = true;
							if (attributeTypeName == "Gendarme.Framework.ProblemAttribute")
								foundProblem = true;
						}

					TypeReference tr = td.BaseType;
					if (tr == null)
						break;
					TypeDefinition resolved = tr.Resolve ();
					if (resolved == null)
						break;
					td = resolved;
				}
				if (!foundProblem || !foundSolution)
					Runner.Report (type, Severity.High, Confidence.High,
						"Rules should have both Problem and Solution attributes");
			}
		}

		private void CheckAttributes (ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return;

			foreach (CustomAttribute attribute in provider.CustomAttributes) {
				var attributeTypeName = attribute.AttributeType.FullName;
				Action<CustomAttribute, ICustomAttributeProvider> f;
				if (attributes.TryGetValue(attributeTypeName, out f))
					f (attribute, provider);
			}
		}

		private bool IsRule (TypeReference type)
		{
			var typeName = type.FullName;
			bool result;
			if (!typeIsRule.TryGetValue (typeName, out result)) {
				result = type.Implements ("Gendarme.Framework.IRule");
				typeIsRule [typeName] = result;
			}
			return result;
		}

		private void CheckProblemAndSolutionAttributes (CustomAttribute attribute, ICustomAttributeProvider provider)
		{
			CheckIfAttributeUsedOnRule (attribute, provider);
			CheckIfStringArgumentsAreNotNullOrEmpty (attribute, provider);
		}

		private void CheckFxCopCompatibilityAttribute (CustomAttribute attribute, ICustomAttributeProvider provider)
		{
			CheckIfAttributeUsedOnRule (attribute, provider);
			if (!CheckIfStringArgumentsAreNotNullOrEmpty (attribute, provider))
				return;

			// check if second argument has correct format
			if (!attribute.HasConstructorArguments)
				return;
			var attributeTypeName = attribute.AttributeType.FullName;
			var argumentValue = attribute.ConstructorArguments [1].Value.ToString ();
			var length = argumentValue.Length;
			if (!((length == 6 || (length > 8 && argumentValue [6] == ':')) && 
				Char.IsLetter (argumentValue [0]) && Char.IsLetter(argumentValue[1]) && 
				Char.IsDigit(argumentValue[2]) && Char.IsDigit(argumentValue[3]) && 
				Char.IsDigit(argumentValue[4])  && Char.IsDigit(argumentValue[5]))) 
				Runner.Report (provider, Severity.Medium, Confidence.High,
					attributeTypeName + " second argument should match the followint format: XX9999:Name");
			else if (length == 6)
				Runner.Report (provider, Severity.Medium, Confidence.High,
					attributeTypeName + " second argument should contain both rule ID and name");
		}

		private void CheckEngineDependencyAttribute (CustomAttribute attribute, ICustomAttributeProvider provider)
		{
			TypeDefinition td = (provider as TypeDefinition);
			if (td == null || !(IsRule (td) || td.Implements ("Gendarme.Framework.IRunner")))
				Runner.Report (td, Severity.Medium, Confidence.High, "[EngineDependency] can only be used on rules and runners");

			CheckIfStringArgumentsAreNotNullOrEmpty (attribute, provider);

			if (!attribute.HasConstructorArguments)
				return;
			var argument = attribute.ConstructorArguments [0];

			// if possible, check if argument type implements IEngine
			if (argument.Type.FullName == "System.Type") {
				TypeReference tr = (argument.Value as TypeReference);
				if (tr == null || !tr.Inherits ("Gendarme.Framework.Engine")) // IEngine does not exist yet
					Runner.Report (provider, Severity.Medium, Confidence.High,
						"EngineDependency attribute argument should implement IEngine interface");

			}
		}

		private void CheckDescriptionAttribute (CustomAttribute attribute, ICustomAttributeProvider provider)
		{
			CheckIfStringArgumentsAreNotNullOrEmpty (attribute, provider);
			CheckIfAttributeUsedOnRulesProperty (attribute, provider);
		}

		private void CheckIfAttributeUsedOnRule (ICustomAttribute attribute, ICustomAttributeProvider provider)
		{
			TypeDefinition td = (provider as TypeDefinition);
			if (td == null || !IsRule (td))
				Runner.Report (td, Severity.Medium, Confidence.High,
					attribute.AttributeType.FullName + " can be used on rules only");
		}

		private void CheckIfAttributeUsedOnRulesProperty (ICustomAttribute attribute, ICustomAttributeProvider provider)
		{
			PropertyDefinition property = (provider as PropertyDefinition);
			if (property == null || !IsRule (property.DeclaringType) || 
				!property.GetMethod.IsPublic || !property.SetMethod.IsPublic)
				Runner.Report (provider, Severity.High, Confidence.High,
					attribute.AttributeType.FullName + " should be used only on rules' public properties");
		}

		// returns true when all arguments are fine, false otherwise
		private bool CheckIfStringArgumentsAreNotNullOrEmpty (CustomAttribute attribute, ICustomAttributeProvider provider)
		{
			if (!attribute.HasConstructorArguments)
				return true;
			foreach (CustomAttributeArgument argument in attribute.ConstructorArguments) {
				if (argument.Type.FullName != "System.String")
					continue;
				if (String.IsNullOrEmpty ((string) argument.Value)) {
					Runner.Report (provider, Severity.Medium, Confidence.High,
						attribute.AttributeType.FullName + " argument cannot be null or empty");
					return false;
				}
			}
			return true;
		}
	}
}
