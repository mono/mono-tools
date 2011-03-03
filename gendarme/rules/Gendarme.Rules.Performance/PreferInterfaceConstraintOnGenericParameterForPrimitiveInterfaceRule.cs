//
// Gendarme.Rules.Performance.PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule
//
// Authors:
//	Julien Hoarau <madgnome@gmail.com>
//
// Copyright (C) 2010 Julien Hoarau
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
using System.Globalization;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Performance {
	/// <summary>
	/// This rule fires if a method use an interface of a primitive type as parameter.  
	/// (IComparable, IFormattable, IConvertible, IComparable&lt;T&gt;, IEquatable&lt;int&gt; ...)
	/// Using generic method with an interface constraint instead avoid boxing of value
	/// type when calling the method.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool GreaterThan (IComparable arg1, IComparable arg2)
	/// {
	///		return arg1.CompareTo (arg2) > 0
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool GreaterThan&lt;T&gt; (T arg1, T arg2) where T : IComparable
	/// {
	///		return arg1.CompareTo (arg2) > 0
	/// }
	/// </code>
	/// </example>

	[Problem("The method should use interface constraint on generic type parameter to avoid boxing of value type")]
	[Solution("Replace the interface parameter with interface constraint on generic type parameter.")]
	public class PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule : Rule, IMethodRule {

		static bool CheckGenericArgument (GenericInstanceType git)
		{
			if ((git == null) || !git.HasGenericArguments)
				return false; // should not happen with the '`1' but...

			TypeReference arg = git.GenericArguments [0];
			switch (arg.MetadataType) {
			case MetadataType.MVar:
				return (arg.IsGenericParameter && arg.IsNamed (String.Empty, "T"));
			case MetadataType.ValueType:
				return arg.IsNamed ("System", "Decimal");
			case MetadataType.Boolean:
			case MetadataType.Byte:
			case MetadataType.Char:
			case MetadataType.Double:
			case MetadataType.Single:
			case MetadataType.Int16:
			case MetadataType.Int32:
			case MetadataType.Int64:
			case MetadataType.SByte:
			case MetadataType.UInt16:
			case MetadataType.UInt32:
			case MetadataType.UInt64:
				return true;
			default:
				return false;
			}
		}

		static bool IsPrimitiveInterface (MemberReference type)
		{
			switch (type.Name) {
			case "IComparable":
			case "IFormattable":
			case "IConvertible":
				return true; // no doubt
			case "IComparable`1":
			case "IEquatable`1":
				// maybe, check generic argument type
				return CheckGenericArgument (type as GenericInstanceType);
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasParameters)
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				TypeReference type = parameter.ParameterType;
				if (type.Namespace != "System")
					continue;

				if (IsPrimitiveInterface (type)) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"You are using {0} as parameter, which cause boxing with value type as argument",
						type.GetFullName ());
					Runner.Report (method, Severity.Low, Confidence.Total, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
