//
// Unit tests for ProtectCallToEventDelegatesRule
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

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class ProtectCallToEventDelegatesTest : MethodRuleTestFixture<ProtectCallToEventDelegatesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no call[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			// generated code - we assume the context is known not to be racy (or null)
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		public event EventHandler Loading;
		public event EventHandler Loaded;

		public void NoCheckLoading (EventArgs e)
		{
			// bad, as this could be null, but its not what this rule looks for
			Loading (this, e);
		}

		public void NoCheckLoad (EventArgs e)
		{
			// bad, as this could be null, but its not what this rule looks for
			Loading (this, e);
			Console.WriteLine ("LOAD");
			Loaded (this, e);
		}

		public void OnBadLoading (EventArgs e)
		{
			// Loading could be non-null here
			if (Loading != null) {
				// but be null once we get here :(
				Loading (this, e);
			}
		}

		public void OnBadLoadingToo (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null
			if (handler != null) {
				// but we're not using handler to call
				Loading (this, e);
			}
		}

		public void WrongCheckLoading (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null
			if (handler != null) {
				// and won't change
				Loaded (this, e);
			}
		}

		public void NoNullCheckLoading (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);
		}

		public void NoNullCheckLoad (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);

			Console.WriteLine ("LOAD");

			handler = Loaded;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoCheckLoading", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoCheckLoad", 2);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("OnBadLoading", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("OnBadLoadingToo", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("WrongCheckLoading", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoNullCheckLoading", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoNullCheckLoad", 2);
		}

		public void OnGoodLoading (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null
			if (handler != null) {
				// and won't change (safe)
				handler (this, e);
			}
		}

		public void OnGoodLoad (EventArgs e)
		{
			EventHandler handler = Loading;
			if (handler != null) {
				handler (this, e);
			}

			Console.WriteLine ("LOAD");

			handler = Loaded;
			if (handler != null) {
				handler (this, e);
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodLoading");
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodLoad");
		}

		public void OnGoodLoadingInverted (EventArgs e)
		{
			EventHandler handler = Loading;
			// handler is either null or non-null
			if (null == handler) {
				// and won't change (safe)
				handler (this, e);
			}
		}

		public void OnGoodLoadInverted (EventArgs e)
		{
			EventHandler handler = Loading;
			if (null == handler) {
				handler (this, e);
			}

			Console.WriteLine ("LOAD");

			handler = Loaded;
			if (null == handler) {
				handler (this, e);
			}
		}

		[Test]
		public void GoodInverted ()
		{
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodLoadingInverted");
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodLoadInverted");
		}

		// same but using generic EventHandler<TEventArgs>

		public event EventHandler<EventArgs> Testing;
		public event EventHandler<EventArgs> Tested;

		public void NoCheckTesting (EventArgs e)
		{
			// bad, as this could be null, but its not what this rule looks for
			Testing (this, e);
		}

		public void NoCheckTest (EventArgs e)
		{
			// bad, as this could be null, but its not what this rule looks for
			Testing (this, e);
			Console.WriteLine ("TEST");
			Tested (this, e);
		}

		public void OnBadTesting (EventArgs e)
		{
			// Testing could be non-null here
			if (Testing != null) {
				// but be null once we get here :(
				Testing (this, e);
			}
		}

		public void OnBadTestingToo (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null
			if (handler != null) {
				// but we're not using handler to call
				Testing (this, e);
			}
		}

		public void WrongCheckTesting (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null
			if (handler != null) {
				// and won't change
				Tested (this, e);
			}
		}

		public void NoNullCheckTesting (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);
		}

		public void NoNullCheckTest (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);

			Console.WriteLine ("TEST");

			handler = Tested;
			// handler is either null or non-null, but we call it without checking
			handler (this, e);
		}

		[Test]
		public void GenericBad ()
		{
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoCheckTesting", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoCheckTest", 2);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("OnBadTesting", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("OnBadLoadingToo", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("WrongCheckTesting", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoNullCheckTesting", 1);
			AssertRuleFailure<ProtectCallToEventDelegatesTest> ("NoNullCheckTest", 2);
		}

		public void OnGoodTesting (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null
			if (handler != null) {
				// and won't change (safe)
				handler (this, e);
			}
		}

		public void OnGoodTest (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			if (handler != null) {
				handler (this, e);
			}

			Console.WriteLine ("TEST");

			handler = Tested;
			if (handler != null) {
				handler (this, e);
			}
		}

		[Test]
		public void GenericGood ()
		{
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodTesting");
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodTest");
		}

		public void OnGoodTestingInverted (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			// handler is either null or non-null
			if (null != handler) {
				// and won't change (safe)
				handler (this, e);
			}
		}

		public void OnGoodTestInverted (EventArgs e)
		{
			EventHandler<EventArgs> handler = Testing;
			if (null != handler) {
				handler (this, e);
			}

			Console.WriteLine ("TEST");

			handler = Tested;
			if (null != handler) {
				handler (this, e);
			}
		}

		[Test]
		public void GenericGoodInverted ()
		{
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodTestingInverted");
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("OnGoodTestInverted");
		}

		// from Options.cs - generic, non-event, delegate
		public Converter<string, string> localizer;

		public void Throw ()
		{
			throw new Exception (localizer ("not an event"));
		}

		[Test]
		public void NonEventDelegates ()
		{
			AssertRuleSuccess<ProtectCallToEventDelegatesTest> ("Throw");
		}
	}
}
