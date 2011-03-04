// 
// Gendarme.Rules.BadPractice.AvoidAssemblyVersionMismatchRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule checks that the <c>[AssemblyVersion]</c> matches the <c>[AssemblyFileVersion]</c> 
	/// when both are present inside an assembly. Having different version numbers in both
	/// attributes can be confusing once the application is deployed.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [assembly: AssemblyVersion ("2.2.0.0")]
	/// [assembly: AssemblyFileVersion ("1.0.0.0")]
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [assembly: AssemblyVersion ("2.2.0.0")]
	/// [assembly: AssemblyFileVersion ("2.2.0.0")]
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("The assembly version, from [AssemblyVersion], does not match the file version, from [AssemblyFileVersion].")]
	[Solution ("This situation can be confusing once deployed. Make sure both version are identical.")]
	public class AvoidAssemblyVersionMismatchRule : Rule, IAssemblyRule {

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			if (!assembly.HasCustomAttributes)
				return RuleResult.DoesNotApply;

			// once compiled [AssemblyVersion] is not part of the custom attributes
			Version assembly_version = assembly.Name.Version;

			// if only one version is specified then there's no mismatch
			if (assembly_version.IsEmpty ())
				return RuleResult.DoesNotApply;

			Version file_version = null;
			foreach (CustomAttribute ca in assembly.CustomAttributes) {
				// AssemblyFileVersionAttribute has a single ctor taking a string value
				// http://msdn.microsoft.com/en-us/library/system.reflection.assemblyfileversionattribute.assemblyfileversionattribute.aspx
				// any attribute without arguments can be skipped
				if (!ca.HasConstructorArguments)
					continue;
				if (!ca.AttributeType.IsNamed ("System.Reflection", "AssemblyFileVersionAttribute"))
					continue;

				Version.TryParse (ca.ConstructorArguments [0].Value as string, out file_version);
				break;
			}

			// if only one version is specified then there's no mismatch
			if (file_version.IsEmpty ())
				return RuleResult.DoesNotApply;

			// rule applies since both versions are present

			// adjust severity based on the difference between the versions
			// a revision/build difference is less likely to be confusing than
			// a difference between the major or minor numbers
			Severity s = Severity.Low;
			if (assembly_version.Major != file_version.Major)
				s = Severity.Critical;
			else if (assembly_version.Minor != file_version.Minor)
				s = Severity.High;
			else if (assembly_version.Build != file_version.Build)
				s = Severity.Medium;
			else if (assembly_version.Revision == file_version.Revision)
				return RuleResult.Success;

			string msg = String.Format (CultureInfo.InvariantCulture,
				"Assembly version is '{0}' while file version is '{1}'.", assembly_version, file_version);
			Runner.Report (assembly, s, Confidence.High, msg);
			return RuleResult.Failure;
		}
	}
}

