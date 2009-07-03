// 
// Gendarme.Rules.Design.AvoidPropertiesWithoutGetAccessorRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an externally visible type contains a property with a setter but
	/// not a getter. This is confusing to users and can make it difficult to use shared
	/// objects. Instead either add a getter or make the property a method.
	/// </summary>
	/// <example>
	/// Bad examples:
	/// <code>
	/// public double Seed {
	///	// no get since there's no use case for it
	///	set {
	///		seed = value;
	///	}
	/// }
	/// 
	/// public sting Password {
	///	// no get as we don't want to expose the password
	///	set {
	///		password = value;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	/// public double Seed {
	///	get {
	///		return seed;
	///	}
	///	set {
	///		seed = value;
	///	}
	/// }
	/// 
	/// public void SetPassword (string value)
	/// {
	///	password = value;
	/// }	
	/// </code>
	/// </example>

	[Problem ("This type contains properties which only have setters.")]
	[Solution ("Add a getter to the property or change the property into a method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
	public class AvoidPropertiesWithoutGetAccessorRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies to type with properties
			if (!type.HasProperties)
				return RuleResult.DoesNotApply;

			// rule applies

			foreach (PropertyDefinition property in type.Properties) {
				MethodDefinition setter = property.SetMethod;
				if (setter != null) {
					if (property.GetMethod == null) {
						Runner.Report (setter, Severity.Medium, Confidence.Total);
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
