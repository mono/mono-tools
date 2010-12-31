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
using Gendarme.Framework;
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

		private readonly HashSet<string> primitiveTypeInterfaces = new HashSet<string> ();

		public PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule ()
		{
			primitiveTypeInterfaces.Add ("System.IComparable");
			primitiveTypeInterfaces.Add ("System.IComparable`1<T>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Boolean>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Byte>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Char>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Decimal>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Double>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Float>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Int32>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Int64>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.SByte>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.Short>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.UInt16>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.UInt32>");
			primitiveTypeInterfaces.Add ("System.IComparable`1<System.UInt64>");
			primitiveTypeInterfaces.Add ("System.IFormattable");
			primitiveTypeInterfaces.Add ("System.IConvertible");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<T>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Boolean>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Byte>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Char>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Decimal>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Double>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Float>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Int32>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Int64>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.SByte>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.Short>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.UInt16>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.UInt32>");
			primitiveTypeInterfaces.Add ("System.IEquatable`1<System.UInt64>");
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasParameters)
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				TypeReference type = parameter.ParameterType;
				if (type.Namespace != "System")
					continue;

				string tname = type.FullName;
				if (primitiveTypeInterfaces.Contains (tname)) {
					string msg = String.Format ("You are using {0} as parameter, which cause boxing with value type as argument",
						tname);
					Runner.Report (method, Severity.Low, Confidence.Total, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
