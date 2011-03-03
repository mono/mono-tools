//
// Gendarme.Rules.Performance.ImplementEqualsTypeRule
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
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for types that override <c>Object.Equals(object)</c> but do not
	/// provide a <c>Equals(x)</c> overload using the type. Such an overload removes the
	/// need to cast the object to the correct type. For value types this also removes the
	/// costly boxing operations. Assemblies targeting .NET 2.0 (and later) should 
	/// also implement <c>System.IEquatable&lt;T&gt;</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	public override bool Equals (object obj)
	///	{
	///		return base.Equals (obj);
	///	}
	///	
	///	public override int GetHashCode ()
	///	{
	///		return base.GetHashCode ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// // IEquatable&lt;T&gt; is only available since
	/// // version 2.0 of the .NET framework
	/// public class Good : IEquatable&lt;Good&gt; {
	///	public override bool Equals (object obj)
	///	{
	///		return (obj as Good);
	///	}
	///	
	/// 	public bool Equals (Good other)
	///	{
	///		return (other != null);
	///	}
	///	
	///	public override int GetHashCode ()
	///	{
	///		return base.GetHashCode ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("Since this type overrides Equals(object) it is also a good candidate to provide a Equals method for it's own type.")]
	[Solution ("Implement the suggested method or interface (2.0) to avoid casting and, for ValueType, boxing penalities.")]
	public class ImplementEqualsTypeRule : Rule, ITypeRule {

		private string [] parameters = new string [1];

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums, delegates and to generated code
			if (type.IsEnum || type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// rule applies only if the type overrides Equals(object)
			if (!type.HasMethod (MethodSignatures.Equals))
				return RuleResult.DoesNotApply;

			// if so then the type should also implement Equals(type) since this avoid a cast 
			// operation (for reference types) and also boxing (for value types).

			// we suggest to implement IEquatable<T> if
			// * the assembly targets the 2.0 (or later) runtime
			// * and it does not already implement it
			if (type.Module.Runtime >= TargetRuntime.Net_2_0) {
				if (!type.Implements ("System", "IEquatable`1")) {
					Runner.Report (type, Severity.Medium, Confidence.Total, "Implement System.IEquatable<T>");
				}
				return Runner.CurrentRuleResult;
			}

			parameters [0] = type.GetFullName ();
			if (type.GetMethod (MethodAttributes.Public, "Equals", "System.Boolean", parameters) != null)
				return RuleResult.Success;

			// we consider this a step more severe for value types since it will need 
			// boxing/unboxing with Equals(object)
			Severity severity = type.IsValueType ? Severity.Medium : Severity.Low;
			string msg = String.Format (CultureInfo.InvariantCulture, "Implement 'bool Equals({0})'", type.Name);
			Runner.Report (type, severity, Confidence.High, msg);
			return RuleResult.Failure;
		}
	}
}
