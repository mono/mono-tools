// 
// Gendarme.Rules.Design.AttributeArgumentsShouldHaveAccessorsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if a parameter to an <c>Attribute</c> constructor is not exposed
	/// using a properly cased property. This is a problem because it is generally not useful
	/// to set state within an attribute without providing a way to get at that state.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [AttributeUsage (AttributeTargets.All)]
	/// public sealed class AttributeWithRequiredProperties : Attribute {
	///	private int storedFoo;
	///	private string storedBar;
	///	
	///	// we have no corresponding property with the name 'Bar' so the rule will fail
	///	public AttributeWithRequiredProperties (int foo, string bar)
	///	{
	///		storedFoo = foo;
	///		storedBar = bar;
	///	}
	///	
	///	public int Foo {
	///		get {
	///			return storedFoo;
	///		}
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [AttributeUsage (AttributeTargets.All)]
	/// public sealed class AttributeWithRequiredProperties : Attribute {
	/// 	private int storedFoo;
	/// 	private string storedBar;
	/// 	
	/// 	public AttributeWithRequiredProperties (int foo, string bar)
	/// 	{
	/// 		storedFoo = foo;
	/// 		storedBar = bar;
	/// 	}
	/// 	
	/// 	public int Foo {
	/// 		get {
	///			return storedFoo; 
	/// 		}
	/// 	}
	/// 	
	/// 	public string Bar {
	/// 		get {
	/// 			return storedBar;
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("All parameter values passed to this type's constructors should be visible through read-only properties.")]
	[Solution ("Add the missing property getters to the type.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public class AttributeArgumentsShouldHaveAccessorsRule : Rule, ITypeRule {

		private List<string> allProperties = new List<string> ();

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to attributes
			if (!type.IsAttribute ())
				return RuleResult.DoesNotApply;

			// look through getters
			allProperties.Clear ();
			foreach (PropertyDefinition property in type.Properties) {
				if (property.GetMethod != null) {
					allProperties.Add (property.Name);
				}
			}

			// look through parameters
			foreach (MethodDefinition constructor in type.Methods) {
				if (!constructor.IsConstructor)
					continue;

				foreach (ParameterDefinition param in constructor.Parameters) {
					 // pascal case it
					string correspondingPropertyName = Char.ToUpper (param.Name [0], CultureInfo.InvariantCulture).ToString (CultureInfo.InvariantCulture) +
						param.Name.Substring (1);
					if (!allProperties.Contains (correspondingPropertyName)) {
						string s = String.Format (CultureInfo.InvariantCulture, 
							"Add '{0}' property to the attribute class.", correspondingPropertyName);
						Runner.Report (param, Severity.Medium, Confidence.High, s);
						allProperties.Add (correspondingPropertyName); // to avoid double catching same property (e.g. from different constructors)
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
