//
// Unit tests for ProvideCorrectRegexPatternRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using NUnit.Framework;

using Gendarme.Rules.Correctness;

using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

using System.Text.RegularExpressions;
using System.Configuration;


namespace Test.Rules.Correctness {

	class RegexCases {
		string foo = "foo";
		const string good1 = "good1";
		const string bad1 = "bad1\\";
		Regex re;

		string DoesNotApply1 () {
			return bad1;
		}

		void Success0 () {
			re = new Regex (@"^\\");
		}

		void Failure0 () {
			re = new Regex ("^\\");
		}

		void Success1 () {
			re = new Regex (good1);
		}

		void Failure1 () {
			re = new Regex (bad1);
		}

		void Success2 (string pattern) {
			var re = new Regex (pattern);
		}

		void Failure2 () {
			var re = new Regex (@"^([a-z)*");
		}

		bool Success2b (Regex re) {
			return re.IsMatch ("^([a-z])*");
		}

		void Success2c () {
			var re = new Regex (this.ToString ());
		}

		void Failure2d1 (bool good) {
			var re = new Regex (good ? "bad\\" : "good");
		}

		void Failure2d2 (bool good) {
			var re = new Regex (good ? "good" : "bad\\");
		}

		void Failure2d3 (bool good)
		{
			var re = new Regex (good ? null : "good");
		}

		void Failure2d4 (bool good)
		{
			var re = new Regex (good ? "good" : null);
		}

		void Failure2d5 (bool good)
		{
			var re = new Regex (good ? String.Empty : "good");
		}

		void Failure2d6 (bool good)
		{
			var re = new Regex (good ? "good" : String.Empty);
		}

		bool Success3 (string pattern) {
			return Regex.IsMatch (foo, pattern);
		}

		bool Failure3 (string text) {
			return Regex.IsMatch (text, @"\9");
		}

		bool Success4 (string pattern) {
			if (null != Regex.Match (good1, "good"))
				return true;
			return Regex.IsMatch (foo, pattern);
		}

		bool Failure4 (string text) {
			if (null != Regex.Match (good1, "good"))
				return true;
			return Regex.IsMatch (text, @"\9");
		}

		string Success5 (string source) {
			return Regex.Replace (source, "foo", "niaa");
		}

		string Failure5 (string source) {
			return Regex.Replace (source, "foo\\", "niaa");
		}

		string Success6 (string source) {
			var re = new Regex ("good");
			return re.Replace ("bad\\", "bad\\");
		}

		string Failure6 (string source) {
			return Regex.Replace (source, null, "niaa");
		}

		string Failure7 (string source) {
			return Regex.Replace (source, "", "niaa");
		}

		string Failure67 (string source) {
			source = Regex.Replace (source, null, "niaa");
			return Regex.Replace (source, "", "niaa");
		}
	}

	class ValidatorCases {
		void Success1 (string input) {
			(new RegexStringValidator ("^[a-z]+")).Validate (input);
		}

		void Failure1 (string input) {
			(new RegexStringValidator ("^[a-z+")).Validate (input);
		}
	}

	[TestFixture]
	public class ProvideCorrectRegexPatternTest : MethodRuleTestFixture<ProvideCorrectRegexPatternRule> {

		static bool raisedAnalyzeModuleEvent;

		[SetUp]
		public void RaiseAnalyzeModuleEvent ()
		{
			if (raisedAnalyzeModuleEvent)
				return;

			raisedAnalyzeModuleEvent = true;
			((TestRunner) Runner).OnModule (DefinitionLoader.GetTypeDefinition<RegexCases> ().Module);
		}

		[Test]
		public void DoesNotApply0 ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<RegexCases> ("DoesNotApply1");
		}

		[Test]
		public void Success0 ()
		{
			AssertRuleSuccess<RegexCases> ("Success0");
		}

		[Test]
		public void Success1 ()
		{
			AssertRuleSuccess<RegexCases> ("Success1");
		}

		[Test]
		public void Success2 ()
		{
			AssertRuleSuccess<RegexCases> ("Success2");
			AssertRuleSuccess<RegexCases> ("Success2b");
			AssertRuleSuccess<RegexCases> ("Success2c");
		}

		[Test]
		public void Success3 ()
		{
			AssertRuleSuccess<RegexCases> ("Success3");
		}

		[Test]
		public void Success4 ()
		{
			AssertRuleSuccess<RegexCases> ("Success4");
		}

		[Test]
		public void Success5 ()
		{
			AssertRuleSuccess<RegexCases> ("Success5");
		}

		[Test]
		public void Success6 ()
		{
			AssertRuleSuccess<RegexCases> ("Success6");
		}

		[Test]
		public void Failure0 ()
		{
			AssertRuleFailure<RegexCases> ("Failure0", 1);
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<RegexCases> ("Failure1", 1);
		}

		[Test]
		public void Failure2 ()
		{
			AssertRuleFailure<RegexCases> ("Failure2", 1);
		}

		[Test]
		public void Failure2d ()
		{
			AssertRuleFailure<RegexCases> ("Failure2d1", 1);
			AssertRuleFailure<RegexCases> ("Failure2d2", 1);
			AssertRuleFailure<RegexCases> ("Failure2d3", 1);
			AssertRuleFailure<RegexCases> ("Failure2d4", 1);
			AssertRuleFailure<RegexCases> ("Failure2d5", 1);
			AssertRuleFailure<RegexCases> ("Failure2d6", 1);
		}

		[Test]
		public void Failure3 ()
		{
			AssertRuleFailure<RegexCases> ("Failure3", 1);
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<RegexCases> ("Failure4", 1);
		}

		[Test]
		public void Failure5 ()
		{
			AssertRuleFailure<RegexCases> ("Failure5", 1);
		}

		[Test]
		public void Failure6 ()
		{
			AssertRuleFailure<RegexCases> ("Failure6", 1);
		}

		[Test]
		public void Failure7 ()
		{
			AssertRuleFailure<RegexCases> ("Failure7", 1);
		}

		[Test]
		public void Failure67 ()
		{
			AssertRuleFailure<RegexCases> ("Failure67", 2);
		}

		[Test]
		public void ValidatorClassSuccess1 ()
		{
			AssertRuleSuccess<ValidatorCases> ("Success1");
		}

		[Test]
		public void ValidatorClassFailure1 ()
		{
			AssertRuleFailure<ValidatorCases> ("Failure1", 1);
		}
	}
}
