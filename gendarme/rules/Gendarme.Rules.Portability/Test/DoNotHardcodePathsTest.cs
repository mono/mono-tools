// 
// Unit tests for DoNotHardcodePathsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.IO;
using System.Xml;

using Gendarme.Framework;
using Gendarme.Rules.Portability;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;

using NUnit.Framework;

namespace Test.Rules.Portability {

#pragma warning disable 169, 219, 414

	[TestFixture]
	public class DoNotHardcodePathsTest : MethodRuleTestFixture<DoNotHardcodePathsRule> {

		string pagefile;	// as a field otherwise the name can lost

		void TotalConfidence1 () // more than 13 points
		{
			// C:\directory\file.sys

			// drive letter			5
			// no slashes			2
			// 3-char extension		4
			// var name contains 'file'	4
			// TOTAL: 			15 points
			pagefile = @"C:\directory\file.sys";
		}

		void TotalConfidence2 () // more than 13 points
		{
			// /home/ex/.aMule/Incoming/Blues_Brothers.avi

			// starts with a slash		2
			// starts with /home/ 		4
			// no backslashes		2
			// 4 slashes			3
			// 3-char extension		4
			// param name contains 'file'	4
			// TOTAL: 			19 points
			OpenFile (42, "/home/ex/.aMule/Incoming/Blues_Brothers.avi", "yarr!");
		}

		void OpenFile (int something, string fileName, string somethingElse) { }

		void TotalConfidence3 () // more than 13 points
		{
			// /opt/mono/bin/mono

			// starts with a slash		2
			// starts with /opt/		4
			// no backslashes		2
			// 3 slashes			2
			// 'file' in param		4
			// TOTAL:			14 points
			System.Diagnostics.Process.Start ("/opt/mono/bin/mono");
		}

		void HighConfidence1 () // 10 to 13 points
		{
			// parser/data/English.nbin

			// no backslashes		2
			// 4-char extension		3
			// 2 slashes			2
			// setter name contains 'path'	4
			// TOTAL:			11 points
			SomePath = "parser/data/English.nbin";
		}


		void HighConfidence2 () // 10 to 13 points
		{
			// \\SHARED\Music\Donovan\Mellow_Yellow.mp3

			// starts with \\ (UNC)		4
			// no slashes			2
			// 3 backslashes (except \\)	3
			// 3-char extension		4
			// TOTAL:			13 points
			string music = @"\\SHARED\Music\Donovan\Mellow_Yellow.mp3";
			throw new NotSupportedException (music);
		}

		string SomePath
		{
			set { }
		}

		Stream NormalConfidence1 () // 8 to 9 points
		{

			// bin\Debug\framework.dll

			// no slashes			2
			// two backslashes		3
			// 3-char extension		4
			// TOTAL:			9 points

			string output = @"bin\Debug\framework.dll";
			return File.OpenWrite (output);
		}

		void NormalConfidence2 () // 8 to 9 points
		{

			// gendarme/bin/

			// no backslashes		2
			// two slashes			2
			// field name contains 'dir'	4
			// TOTAL:			8 points

			gendarmeDirectory = @"gendarme/bin/";
		}

		private string gendarmeDirectory;


		string DontReportUris ()
		{
			string something = "http://somewhere.com/index.php";
			return something;
		}

		string DontReportSlashlessStrings ()
		{
			// we do not check strings that contain no (back)slashes
			// since they can cause no portability problems
			return "a elbereth gilthoniel.txt";
		}

		void DontReportRegistryKeys ()
		{
			// they look like paths but they aren't
			Microsoft.Win32.RegistryKey env = Microsoft.Win32.Registry.LocalMachine
				.OpenSubKey (@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true);
		}

		string DontReportXML ()
		{
			string someXml = "<a><b /><c></c><b /></a>";
			return someXml;
		}

		string DontReportShortStrings ()
		{
			string someFile = "/";
			return someFile;
		}

		string DontReportStringsWithManyDots ()
		{
			string mimeType = "application/vnd.oasis-open.relax-ng.rnc";
			return mimeType;
		}

		void DontReportXPath ()
		{
			System.Xml.XmlDocument doc = null;
			System.Xml.XmlNode node = doc.SelectSingleNode ("/root/element/anotherelement");
		}

		void DontReportRegexes ()
		{
			System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex (
					@"^\s*"
					+ @"(((?<ORIGIN>(((\d+>)?[a-zA-Z]?:[^:]*)|([^:]*))):)"
					+ "|())"
					+ "(?<SUBCATEGORY>(()|([^:]*? )))"
					+ "(?<CATEGORY>(error|warning)) "
					+ "(?<CODE>[^:]*):"
					+ "(?<TEXT>.*)$",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		}

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no LDSTR instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void FailureTotalConfidence ()
		{
			AssertRuleFailure<DoNotHardcodePathsTest> ("TotalConfidence1", 1);
			Assert.AreEqual (Confidence.Total, Runner.Defects [0].Confidence, "1");
			AssertRuleFailure<DoNotHardcodePathsTest> ("TotalConfidence2", 1);
			Assert.AreEqual (Confidence.Total, Runner.Defects [0].Confidence, "2");
			AssertRuleFailure<DoNotHardcodePathsTest> ("TotalConfidence3", 1);
			Assert.AreEqual (Confidence.Total, Runner.Defects [0].Confidence, "3");
		}

		[Test]
		public void FailureHighConfidence ()
		{
			AssertRuleFailure<DoNotHardcodePathsTest> ("HighConfidence1", 1);
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "1");
			AssertRuleFailure<DoNotHardcodePathsTest> ("HighConfidence2", 1);
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "2");
		}

		[Test]
		public void FailureNormalConfidence ()
		{
			AssertRuleFailure<DoNotHardcodePathsTest> ("NormalConfidence1", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");
			AssertRuleFailure<DoNotHardcodePathsTest> ("NormalConfidence2", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "2");
		}

		[Test]
		public void IgnoreSomeCases ()
		{
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportUris");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportSlashlessStrings");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportRegistryKeys");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportXML");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportShortStrings");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportStringsWithManyDots");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportXPath");
			AssertRuleSuccess<DoNotHardcodePathsTest> ("DontReportRegexes");
		}

		// test case provided by Richard Birkby
		internal sealed class FalsePositive5 {
			public void Run ()
			{
				GetType ();

				XmlDocument doc = new XmlDocument ();
				doc.LoadXml ("<a><b><c/></b></a>");

				AddVariable ("a/b/c", doc);
			}

			private static void AddVariable (string xpath, XmlNode node)
			{
				node.SelectSingleNode (xpath);
			}
		}

		internal sealed class Fixed5 {
			public void Run ()
			{
				GetType ();

				XmlDocument doc = new XmlDocument ();
				doc.LoadXml ("<a><b><c/></b></a>");

				doc.SelectSingleNode ("b/c/d");
			}
		}

		[Test]
		public void XPath ()
		{
			AssertRuleFailure<FalsePositive5> ("Run", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");
			AssertRuleSuccess<Fixed5> ("Run");
		}
	}
}
