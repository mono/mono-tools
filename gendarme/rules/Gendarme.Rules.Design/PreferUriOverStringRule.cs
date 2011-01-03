// 
// Gendarme.Rules.Design.PreferUriOverStringRule
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

using Mono.Cecil;
using Mono.Collections.Generic;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using System;
using System.Linq;
using System.Collections.Generic;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// Checks methods and properties to ensure that System.Uri is used in place
	/// of or in addition to strings where appropriate.
	/// </summary>
	/// <example>
	/// Bad example 1:
	/// <code>
	/// string Uri { get; set; }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example 2:
	/// <code>
	/// string GetUri () { return "http://www.mono-project.com"; }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example 3:
	/// <code>
	/// void SendRequest (string url) {  
	///	...
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example 1:
	/// <code>
	/// Uri Uri { get; set; }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example 2:
	/// <code>
	/// Uri GetUri () { return new Uri ("http://www.mono-project.com"); }
	/// </code>
	/// </example>
	/// <example>
	/// Good example 3:
	/// <code>
	/// void SendRequest (string url) { 
	///	SendRequest (new Uri(url)); 
	/// }
	/// void SendRequest (Uri url) {
	///	...
	/// }
	/// </code>
	/// </example>

	[Problem ("A method, parameter, or property with uri, url, or urn in the name is or returns a System.String instead of a System.Uri.")]
	[Solution ("Use System.Uri in place of System.String, or add an additional overload that takes a System.Uri.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
	[FxCopCompatibility ("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
	[FxCopCompatibility ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	public class PreferUriOverStringRule : Rule, IMethodRule {
		private static readonly char [] url_enders = { 'i', 'n', 'l' };
		private Bitmask<long> methodBitmask = new Bitmask<long> ();

		private static int FindTokenStart (string memberName, string token, int start)
		{
			int index = start;
			// We assume the name is pascal- or camel- cased: to prevent false-positives (such as the urn in return),
			// the position is only returned if the character is the first in the string, or is an uppercase letter.
			while ((index = memberName.IndexOf (token, index, StringComparison.OrdinalIgnoreCase)) != -1) {
				if (index == 0 || char.IsUpper (memberName [index]))
					break;
				index += token.Length;
			}
			return index;
		}

		private static bool IsUri (string memberName)
		{
			int index = 0;
			while ((index = FindTokenStart (memberName, "ur", index)) != -1) {
				if (memberName.Length < index + 2)
					break;
				if (url_enders.Contains (char.ToLower (memberName [index + 2])))
					return true;
				index += 2;
			}
			return false;
		}

		private static bool IsOkay (TypeReference type, string memberName)
		{
			if (type == null)
				return true;
			return !(type.Namespace == "System" && type.Name == "String") || !IsUri (memberName);
		}

		private bool FindBadParameters (Collection<ParameterDefinition> parameters)
		{
			// Every uri parameter that is a string is recorded by the bitmask.
			// Note: we're assuming the number of parameters will fit into the bitmask.
			methodBitmask.ClearAll ();
			var defect = false;
			for (int i = 0; i < parameters.Count; i++) {
				var param = parameters [i];
				var type = param.ParameterType;
				if (IsOkay (type, param.Name))
					continue;
				defect = true;
				methodBitmask.Set (1 << i);
			}

			return defect;
		}

		private void ReportBadParameters (Collection<ParameterDefinition> parameters)
		{
			for (var i = 0; i < parameters.Count; i++) {
				long bit = 1 << i;
				if (!methodBitmask.Get (bit))
					continue;
				Runner.Report (parameters [i], Severity.Medium, Confidence.Normal);
			}
		}

		private void CheckParameters (MethodDefinition method)
		{
			// attributes are a special case where Uri cannot be used and has it's own
			// rule to cover this: Gendarme.Rules.Correctness.AttributeStringLiteralShouldParseCorrectlyRule
			if (method.IsConstructor && method.DeclaringType.Inherits ("System.Attribute"))
				return;

			var methodParams = method.Parameters;
			if (!FindBadParameters (methodParams))
				return;

			var ok = false;
			// Find each of the overloads for the method being checked.
			foreach (var overload in method.DeclaringType.Methods) {
				var overloadParams = overload.Parameters;
				var numOverloadParams = overloadParams.Count;
				if (overload == method || overload.Name != method.Name || 
					numOverloadParams != methodParams.Count)
					continue;

				ok = true;
				// Find each parameter that was a string in the original, but should be a Uri
				for (int i = 0; i < numOverloadParams; i++) {
					long bit = 1 >> i;
					if (!methodBitmask.Get (bit))
						continue;
					var paramType = overloadParams [i].ParameterType;
					ok = paramType.Namespace == "System" && paramType.Name == "Uri";
					// If this overload didn't replace the string with a uri, skip to the next one.
					if (!ok)
						break;
				}
				// End the method early if a valid overload was found.
				if (ok)
					return;
			}

			// If we've gotten this far, then there is a problem with at least one parameter and
			// there are no suitable replacement overloads. Report every bad parameter.
			ReportBadParameters (methodParams);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// Check property getters/setters. In order to prevent the property from
			// being reported twice, setters are only checked if the property has no getter.
			PropertyDefinition property = method.IsProperty () ? method.GetPropertyByAccessor () : null;
			if (property != null) {
				// however do not exclude automatic properties (getter/setter marked a generated code)
				if ((method.IsSetter && property.GetMethod != null) || property.IsGeneratedCode ())
					return RuleResult.DoesNotApply;
				if (!IsOkay (property.PropertyType, property.Name))
					Runner.Report (property, Severity.Medium, Confidence.Normal);
			} else {
				// exclude generated code like webservices
				if (method.IsGeneratedCode ())
					return RuleResult.DoesNotApply;

				// Check the method's parameters.
				if (method.HasParameters)
					CheckParameters (method);

				// Check the method's return type.
				if (!IsOkay (method.ReturnType, method.Name))
					Runner.Report (method, Severity.Medium, Confidence.Normal);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
