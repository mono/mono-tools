//
// Gendarme.Rules.Correctness.AttributeStringLiteralShouldParseCorrectlyRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

using Mono.Cecil;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// As attributes are used at compile time, only constants can
	/// be passed to constructors. This can lead to runtime errors for
	/// things like malformed URI strings.
	///
	/// This rule checks attributes with the following types, represented as
	/// a string, and validates the string value:
	/// <list type="bullet">
	/// <item>
	/// <description>Version</description>
	/// </item>
	/// <item>
	/// <description>Guid</description>
	/// </item>
	/// <item>
	/// <description>Uri</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: AssemblyFileVersion ("fooo")]
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [assembly: AssemblyFileVersion ("0.0.1.*")]
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("An url, version, or guid string seems to be malformed.")]
	[Solution ("Correctly format the reported parameters.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly")]
	public class AttributeStringLiteralsShouldParseCorrectlyRule : Rule, IMethodRule, ITypeRule, IAssemblyRule {

		static bool Contains (string original, string value)
		{
			return original.IndexOf (value, 0, StringComparison.OrdinalIgnoreCase) != -1;
		}
		
		void CheckParametersAndValues (IMetadataTokenProvider provider, IMethodSignature constructor, IList<CustomAttributeArgument> arguments)
		{
			for (int index = 0; index < arguments.Count; index++) {
				ParameterDefinition parameter = constructor.Parameters[index];
				if (parameter.ParameterType.IsNamed ("System", "String")) {
					string value = (string) arguments [index].Value;
					if (Contains (parameter.Name, "version")) {
						Version v = null;
						if (!Version.TryParse (value, out v)) {
							string msg = String.Format (CultureInfo.InvariantCulture, "The value passed: {0} can't be parsed to a valid Version.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
					if (Contains (parameter.Name, "url") ||
						Contains (parameter.Name, "uri") ||
						Contains (parameter.Name, "urn")) {
						Uri parsed = null;
						if (!Uri.TryCreate (value, UriKind.Absolute, out parsed)) {
							string msg = String.Format (CultureInfo.InvariantCulture, "The valued passed {0} can't be parsed to a valid Uri.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
					if (Contains (parameter.Name, "guid")) {
						Guid g;
						if (!Guid.TryParse (value, out g)) {
							string msg = String.Format (CultureInfo.InvariantCulture, "The valued passed {0} can't be parsed to a valid Guid.", value);
							Runner.Report (provider, Severity.High, Confidence.High, msg);
						}
						continue;
					}
				}
			}
		}
		
		void CheckAttributesIn (ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return;

			//There isn't a relationship between
			//IMetadataTokenProvider and ICustomAttributeProvider,
			//altough a method, or type, implements both interfaces.
			IMetadataTokenProvider metadataProvider = provider as IMetadataTokenProvider;
	
			foreach (CustomAttribute attribute in provider.CustomAttributes) {
				// if the attribute has no argument then there will be nothing to check later, skip it
				if (!attribute.HasConstructorArguments)
					continue;
				MethodReference ctor = attribute.Constructor;
				if (ctor.HasParameters)
					CheckParametersAndValues (metadataProvider, ctor, attribute.ConstructorArguments);
			}
		}

		void CheckAttributesIn (IEnumerable targets)
		{
			foreach (ICustomAttributeProvider provider in targets)
				CheckAttributesIn (provider);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			CheckAttributesIn (method);
			CheckAttributesIn (method.MethodReturnType);
			if (method.HasParameters)
				CheckAttributesIn (method.Parameters);
			if (method.HasGenericParameters)
				CheckAttributesIn (method.GenericParameters);

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			CheckAttributesIn (type);
			if (type.HasFields)
				CheckAttributesIn (type.Fields); 
			if (type.HasProperties)
				CheckAttributesIn (type.Properties);
			if (type.HasEvents)
				CheckAttributesIn (type.Events);
			if (type.HasGenericParameters)
				CheckAttributesIn (type.GenericParameters);

			return Runner.CurrentRuleResult;
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			CheckAttributesIn (assembly);
			CheckAttributesIn (assembly.Modules);
			
			return Runner.CurrentRuleResult;			
		}
	}
}

