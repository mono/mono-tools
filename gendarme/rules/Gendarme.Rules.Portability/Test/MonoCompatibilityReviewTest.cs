//
// Unit tests for MonoCompatibilityReviewRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Gendarme.Framework;
using Gendarme.Rules.Portability;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Portability {

	[TestFixture]
	public class MonoCompatibilityReviewTest {

		private MonoCompatibilityReviewRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;


		public void NotImplemented ()
		{
			new object ().GetHashCode ();
		}

		public void Missing ()
		{
			new object ().ToString ();
		}

		public void TODO ()
		{
			new object ().GetType ();
		}

		public void FalsePositive ()
		{
			new object ().Equals (null);
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Portability.MonoCompatibilityReviewTest"];
			rule = new MonoCompatibilityReviewRule ();
			runner = new TestRunner (rule);
		}

		[SetUp]
		public void SetUp ()
		{
			rule.NotImplemented.Clear ();
			rule.NotImplemented.Add ("System.Int32 System.Object::GetHashCode()", null);
			rule.Missing.Clear ();
			rule.Missing.Add ("System.String System.Object::ToString()", null);
			rule.ToDo.Clear ();
			rule.ToDo.Add ("System.Type System.Object::GetType()", "TODO");
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			Assert.Fail ("Could not find '{0}'.", name);
			return null;
		}

		[Test]
		public void TestNotImplemented ()
		{
			MethodDefinition method = GetTest ("NotImplemented");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestMissing ()
		{
			MethodDefinition method = GetTest ("Missing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestTODO ()
		{
			MethodDefinition method = GetTest ("TODO");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestFalsePositive ()
		{
			MethodDefinition method = GetTest ("FalsePositive");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestOneTimeWarning ()
		{
/*			//inject some values
			rule.DownloadException = new System.Net.WebException ("FAIL");

			MethodDefinition method = GetTest ("FalsePositive");
			MessageCollection result = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (result);
			Assert.AreEqual (1, result.Count);

			//should only warn once
			Assert.IsNull (rule.CheckMethod (method, new MinimalRunner ()));*/
		}

		private void DeleteDefinitionData ()
		{
			string localAppDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			string definitionsFolder = Path.Combine (localAppDataFolder, "Gendarme");
			string definitionsFile = Path.Combine (definitionsFolder, "definitions.zip");
			if (File.Exists (definitionsFile))
				File.Delete (definitionsFile);
		}

		[Test]
		[Ignore ("This test takes a few seconds.")]
		public void TestDefinitionDownload ()
		{
			DeleteDefinitionData ();

			MonoCompatibilityReviewRule rule = new MonoCompatibilityReviewRule ();
			rule.Initialize (new TestRunner (rule));
			Assert.IsNotNull (rule.Missing);
			Assert.IsNotNull (rule.NotImplemented);
			Assert.IsNotNull (rule.ToDo);
		}
	}
}
