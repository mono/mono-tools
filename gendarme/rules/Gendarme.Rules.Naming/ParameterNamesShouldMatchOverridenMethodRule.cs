//
// Gendarme.Rules.Naming.ParameterNamesShouldMatchOverriddenMethodRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

namespace Gendarme.Rules.Naming {

	[Problem ("This method overrides (or implement) an existing method but does not use the same parameter names as the original.")]
	[Solution ("Keep parameter names consistent when overriding a class or implementing an interface.")]
	public class ParameterNamesShouldMatchOverriddenMethodRule : Rule, IMethodRule {

		private static bool SignatureMatches (MethodDefinition method, MethodDefinition baseMethod, bool explicitInterfaceCheck)
		{
			if (method.Name != baseMethod.Name) {
				if (!explicitInterfaceCheck)
					return false;
				if (method.Name != baseMethod.DeclaringType.FullName + "." + baseMethod.Name)
					return false;
			}
			if (method.ReturnType.ToString () != baseMethod.ReturnType.ToString ())
				return false;
			if (method.Parameters.Count != baseMethod.Parameters.Count)
				return false;
			bool paramtersMatch = true;
			for (int i = 0; i < method.Parameters.Count; i++) {
				if (method.Parameters [i].ParameterType.ToString () != baseMethod.Parameters [i].ParameterType.ToString ()) {
					paramtersMatch = false;
					break;
				}
			}
			return paramtersMatch;
		}

		private static MethodDefinition GetBaseMethod (MethodDefinition method)
		{
			TypeDefinition baseType = method.DeclaringType.Resolve ();
			if (baseType == null)
				return null;

			while ((baseType.BaseType != null) && (baseType != baseType.BaseType)) {
				baseType = baseType.BaseType.Resolve ();
				if (baseType == null)
					return null;		// could not resolve

				foreach (MethodDefinition baseMethodCandidate in baseType.Methods) {
					if (SignatureMatches (method, baseMethodCandidate, false))
						return baseMethodCandidate;
				}
			}
			return null;
		}

		private static MethodDefinition GetInterfaceMethod (MethodDefinition method)
		{
			TypeDefinition type = (TypeDefinition) method.DeclaringType;
			foreach (TypeReference interfaceReference in type.Interfaces) {
				TypeDefinition interfaceCandidate = interfaceReference.Resolve ();
				if (interfaceCandidate == null)
					continue;
				foreach (MethodDefinition interfaceMethodCandidate in interfaceCandidate.Methods) {
					if (SignatureMatches (method, interfaceMethodCandidate, true))
						return interfaceMethodCandidate;
				}
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsVirtual || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition baseMethod = null;
			if (!method.IsNewSlot)
				baseMethod = GetBaseMethod (method);
			if (baseMethod == null)
				baseMethod = GetInterfaceMethod (method);
			if (baseMethod == null)
				return RuleResult.Success;

			for (int i = 0; i < method.Parameters.Count; i++) {
				if (method.Parameters [i].Name != baseMethod.Parameters [i].Name) {
					string s = string.Format (CultureInfo.InstalledUICulture,
						"The name of parameter #{0} ({1}) does not match the name of the parameter in the overriden method ({2}).", 
						i + 1, method.Parameters [i].Name, baseMethod.Parameters [i].Name);
					Runner.Report (method, Severity.Medium, Confidence.High, s);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
