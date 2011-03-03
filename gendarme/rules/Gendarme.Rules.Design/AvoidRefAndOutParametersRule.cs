//
// Gendarme.Rules.Design.AvoidRefAndOutParametersRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if a method uses <c>ref</c> or <c>out</c> parameters. 
	/// These are advanced features that can easily be misunderstood and misused 
	/// (by the consumer). This can result in an API that is difficult to use so
	/// avoid them whenever possible. 
	/// An alternative is to use one of the new generic <c>Tuple</c> types (FX v4 and later)
	/// as the return value.
	/// No defect will be reported if the method:
	/// <list>
	/// <item>follows the <c>bool Try*(X out)</c> pattern; or</item>
	/// <item>implement an interface requiring the use of <c>ref</c> or <c>out</c> parameters.
	/// In the later case defects will be reported against the interface itself (if analyzed
	/// by Gendarme).</item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool NextJob (ref int id, out string display)
	/// {
	///	if (id &lt; 0)
	///		return false;
	///	display = String.Format ("Job #{0}", id++);
	///	return true;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// private int id = 0;
	/// 
	/// private int GetNextId ()
	/// {
	///	int id = this.id++;
	///	return id;
	/// }
	/// 
	/// public string NextJob ()
	/// {
	///	return String.Format ("Job #{0}", Id);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method use ref and/or out parameters in a visible API which can confuse many developers.")]
	[Solution ("The most common reason to do this is to return multiple values from a method which can be rewritten so that it returns a custom type instead.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1021:AvoidOutParameters")]
	[FxCopCompatibility ("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
	public class AvoidRefAndOutParametersRule : Rule, IMethodRule {

		static bool IsSignatureDictatedByInterface (MethodDefinition method, TypeReference type)
		{
			if (type == null)
				return false;

			TypeDefinition td = type.Resolve ();
			if (td == null)
				return false;

			foreach (TypeReference intf_ref in td.Interfaces) {
				TypeDefinition intr = intf_ref.Resolve ();
				if (intr == null)
					continue;

				if (intr.HasMethods) {
					foreach (MethodDefinition md in intr.Methods) {
						if (method.CompareSignature (md))
							return true;
					}
				}
				// check if the interface inherits from another interface with this signature
				if (IsSignatureDictatedByInterface (method, intr.BaseType))
					return true;
			}
			// check if a base type is implementing an interface with this signature
			return IsSignatureDictatedByInterface (method, td.BaseType);
		}

		static bool IsSignatureDictatedByInterface (MethodDefinition method)
		{
			// quick-out: if the method implements an interface then it's virtual
			if (!method.IsVirtual)
				return false;

			// check if the interface itself has the defect (always reported)
			TypeDefinition type = method.DeclaringType;
			if (type.IsInterface)
				return false;

			return IsSignatureDictatedByInterface (method, type);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to visible methods with parameters
			// we also exclude all p/invokes since we have a rule for them not to be visible
			if (method.IsPInvokeImpl || !method.HasParameters || !method.IsVisible ())
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				string how = null;
				if (parameter.IsOut) {
					// out is permitted for the "bool Try* (...)" pattern
					if (method.ReturnType.IsNamed ("System", "Boolean") && 
						method.Name.StartsWith ("Try", StringComparison.Ordinal)) {
						continue;
					}

					how = "out";
				} else if (parameter.ParameterType.IsByReference) {
					how = "ref";
				}

				// report the method is it is not dictated by an interface
				if ((how != null) && !IsSignatureDictatedByInterface (method)) {
					// goal is to keep the API as simple as possible so this is more severe for public than protected methods
					Severity severity = method.IsPublic ? Severity.Medium : Severity.Low;
					string msg = String.Format (CultureInfo.InvariantCulture,
						"Parameter '{0}' passed by reference ({1}).", parameter.Name, how);
					Runner.Report (parameter, severity, Confidence.Total, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
