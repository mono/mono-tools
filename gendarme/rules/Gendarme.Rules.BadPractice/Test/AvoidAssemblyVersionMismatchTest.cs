// 
// Unit tests for AvoidAssemblyVersionMismatchRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

[assembly: AssemblyVersion ("2.2.0.0")]
[assembly: AssemblyFileVersion ("2.2.0.0")]

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class AvoidAssemblyVersionMismatchTest : AssemblyRuleTestFixture<AvoidAssemblyVersionMismatchRule> {

		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		[Test]
		public void EmptyAssemblyVersion ()
		{
			Version v = assembly.Name.Version;
			try {
				assembly.Name.Version = null; // should not happen
				AssertRuleDoesNotApply (assembly);

				assembly.Name.Version = new Version (0, 0, 0, 0);
				AssertRuleDoesNotApply (assembly);
			}
			finally {
				assembly.Name.Version = v;
			}
		}

		[Test]
		public void EmptyCustomAttributes ()
		{
			IList<CustomAttribute> cac = new List<CustomAttribute> ();
			foreach (CustomAttribute ca in assembly.CustomAttributes)
				cac.Add (ca);

			try {
				assembly.CustomAttributes.Clear ();
				AssertRuleDoesNotApply (assembly);
			}
			finally {
				foreach (CustomAttribute ca in cac)
					assembly.CustomAttributes.Add (ca);
			}
		}

		[Test]
		public void AbsentAssemblyFileVersion ()
		{
			CustomAttribute afv = null;
			foreach (CustomAttribute ca in assembly.CustomAttributes) {
				if (ca.AttributeType.FullName != "System.Reflection.AssemblyFileVersionAttribute")
					continue;
				afv = ca;
				break;
			}
			assembly.CustomAttributes.Remove (afv);

			try {
				AssertRuleDoesNotApply (assembly);
			}
			finally {
				assembly.CustomAttributes.Add (afv);
			}
		}

		[Test]
		public void EmptyAssemblyFileVersion ()
		{
			CustomAttribute afv = null;
			foreach (CustomAttribute ca in assembly.CustomAttributes) {
				if (ca.Constructor.DeclaringType.FullName != "System.Reflection.AssemblyFileVersionAttribute")
					continue;
				afv = ca;
				break;
			}
			CustomAttributeArgument value = afv.ConstructorArguments [0];
			afv.ConstructorArguments [0] = new CustomAttributeArgument ();

			try {
				AssertRuleDoesNotApply (assembly);
			}
			finally {
				afv.ConstructorArguments [0] = value;
			}
		}

		[Test]
		public void VersionMatch ()
		{
			AssertRuleSuccess (assembly);
		}

		[Test]
		public void VersionMismatch ()
		{
			Version v = assembly.Name.Version;
			try {
				assembly.Name.Version = new Version (2, 2, 0, 9);
				AssertRuleFailure (assembly, 1);
				Assert.AreEqual (Severity.Low, Runner.Defects [0].Severity, "Low");

				assembly.Name.Version = new Version (2, 2, 9, 0);
				AssertRuleFailure (assembly, 1);
				Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Medium");

				assembly.Name.Version = new Version (2, 9, 0, 0);
				AssertRuleFailure (assembly, 1);
				Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "High");

				assembly.Name.Version = new Version (9, 2, 0, 0);
				AssertRuleFailure (assembly, 1);
				Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "Critical");
			}
			finally {
				assembly.Name.Version = v;
			}
		}
	}
}
