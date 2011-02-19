//
// Gendarme.Rules.Design.ImplementIComparableCorrectlyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Text;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for types that implement <c>System.IComparable</c> and verifies
	/// that the type overrides the <c>Equals(object)</c> method and overloads the <c>==</c>,
	/// <c>!=</c>, <c>&lt;</c> and <c>&gt;</c> operators.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public struct Comparable : IComparable {
	/// 
	///	private int x;
	/// 
	///	public int CompareTo (object obj)
	///	{
	///		return x.CompareTo (((Comparable)obj).x);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public struct Comparable : IComparable {
	/// 
	///	public int CompareTo (object obj)
	///	{
	///		return x.CompareTo (((Comparable)obj).x);
	///	}
	///	
	///	public override bool Equals (object obj)
	///	{
	///		return x == ((Comparable) obj).x;
	///	}
	///	
	///	static public bool operator == (Comparable left, Comparable right)
	///	{
	///		return (left.x == right.x);
	///	}
	///	
	///	static public bool operator != (Comparable left, Comparable right)
	///	{
	///		return (left.x != right.x);
	///	}
	///	
	///	static public bool operator &gt; (Comparable left, Comparable right)
	///	{
	///		return (left.x &gt; right.x);
	///	}
	///	
	///	static public bool operator &lt; (Comparable left, Comparable right)
	///	{
	///		return (left.x &lt; right.x);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This type implements IComparable so it should override Equals(object) and overload the ==, !=, < and > operators.")]
	[Solution ("Implement the suggested method and/or operators.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public class ImplementIComparableCorrectlyRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums, interfaces and to generated code
			if (type.IsEnum || type.IsInterface || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// rule only applies if the type implements IComparable or IComparable<T>
			// Note: we do not use Implements rock because we do not want a recursive answer
			bool icomparable = false;
			foreach (TypeReference iface in type.Interfaces) {
				if (iface.Namespace != "System")
					continue;
				// catch both System.IComparable and System.IComparable`1<X>
				if (iface.Name.StartsWith ("IComparable", StringComparison.Ordinal)) {
					icomparable = true;
					break;
				}
			}
			if (!icomparable)
				return RuleResult.DoesNotApply;

			// type should override Equals(object)
			if (!type.HasMethod (MethodSignatures.Equals))
				Runner.Report (type, Severity.High, Confidence.High, "Missing Equals(object) override.");

			// type should implement overloads for ==, !=, < and > operators
			// note: report all missing operators as single defect
			bool equality = type.HasMethod (MethodSignatures.op_Equality);
			bool inequality = type.HasMethod (MethodSignatures.op_Inequality);
			bool less_than = type.HasMethod (MethodSignatures.op_LessThan);
			bool greater_than = type.HasMethod (MethodSignatures.op_GreaterThan);
			if (!equality || !inequality || !less_than || !greater_than) {
				StringBuilder sb = new StringBuilder ("Missing operators:");
				if (!equality)
					sb.Append (" op_Equality (==)");
				if (!inequality)
					sb.Append (" op_Inequality (!=)");
				if (!less_than)
					sb.Append (" op_LessThan (<)");
				if (!greater_than)
					sb.Append (" op_GreaterThan (>)");
				// not all languages support operator overloading so we lower the severity a bit
				Runner.Report (type, Severity.Medium, Confidence.High, sb.ToString ());
			}

			return Runner.CurrentRuleResult;
		}
	}
}

