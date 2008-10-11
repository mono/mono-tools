//
// Unit tests for AvoidUnneededUnboxingRule
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidUnneededUnboxingTest : MethodRuleTestFixture<AvoidUnneededUnboxingRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		// from mcs/class/Managed.Windows.Forms/System.Windows.Forms/Message.cs
		public struct Message {
			private int msg;
			private IntPtr hwnd;
			private IntPtr lParam;
			private IntPtr wParam;
			private IntPtr result;

			public override bool Equals (object o)
			{
				if (!(o is Message)) {
					return false;
				}

				return ((this.msg == ((Message) o).msg) &&
					(this.hwnd == ((Message) o).hwnd) &&
					(this.lParam == ((Message) o).lParam) &&
					(this.wParam == ((Message) o).wParam) &&
					(this.result == ((Message) o).result));
			}

			// other test cases

			public bool EqualsOk (object o)
			{
				if (!(o is Message)) {
					return false;
				}

				Message msg = (Message) o;

				return ((this.msg == msg.msg) &&
					(this.hwnd == msg.hwnd) &&
					(this.lParam == msg.lParam) &&
					(this.wParam == msg.wParam) &&
					(this.result == msg.result));
			}

			public bool EqualsNone (Message msg)
			{
				return ((this.msg == msg.msg) &&
					(this.hwnd == msg.hwnd) &&
					(this.lParam == msg.lParam) &&
					(this.wParam == msg.wParam) &&
					(this.result == msg.result));
			}

			// test static and 2 parameters
			static public bool StaticEquals (object left, object right)
			{
				return ((((Message) left).msg == ((Message) right).msg) &&
					(((Message) left).hwnd == ((Message) right).hwnd) &&
					(((Message) left).lParam == ((Message) right).lParam) &&
					(((Message) left).wParam == ((Message) right).wParam) &&
					(((Message) left).result == ((Message) right).result));
			}

			// a lot of extra parameters to hit the ldarg instruction (not the macros)
			static public bool StaticEqualsOk (string message, string msgLeft, object left, string msgRight, object right)
			{
				Message m1 = (Message) left;
				Message m2 = (Message) right;

				return ((m1.msg == m1.msg) &&
					(m1.hwnd == m1.hwnd) &&
					(m1.lParam == m1.lParam) &&
					(m1.wParam == m1.wParam) &&
					(m1.result == m1.result));
			}
		}

		private object GetMessage ()
		{
			return new Message ();
		}

		private void CompareMessages (Message msg)
		{
			object o = GetMessage ();
			Console.WriteLine (msg.Equals ((Message) o) && ((Message) o).Equals (msg));
		}

		private void CompareMessagesNoVariable (Message msg)
		{
			Console.WriteLine (GetMessage ().Equals ((Message) GetMessage()));
		}

		private object field_one;
		private object field_two;

		private void CompareFields ()
		{
			Console.WriteLine (field_one.Equals ((Message) field_two) && ((Message) field_one).Equals (field_two));
		}

		private void CompareFieldsTwo ()
		{
			// field_two is unboxed two times while field_one is always treated as an object
			Console.WriteLine (field_one.Equals ((Message) field_two) && ((Message) field_two).Equals (field_one));
		}

		private void CompareVariables ()
		{
			object one = GetMessage ();
			bool two = (one != null);
			string three = "result {0}";

			object four = GetMessage ();
			if (two) {
				bool five = (four != null);
				Console.WriteLine (three, five && ((Message) four).Equals ((Message) one));
				Console.WriteLine (three, !five && ((Message) four).Equals ((Message) four));
				Console.WriteLine (three, five && ((Message) one).Equals ((Message) four));
				Console.WriteLine (three, !five && ((Message) one).Equals ((Message) one));
			} else {
				object six = GetMessage ();
				Console.WriteLine (three, ((Message) four).Equals ((Message) six) && ((Message) four).Equals ((Message) one));
				Console.WriteLine (three, ((Message) six).Equals ((Message) four) && ((Message) six).Equals ((Message) one));
				Console.WriteLine (three, ((Message) one).Equals ((Message) four) && ((Message) one).Equals ((Message) six));
			}
		}

		[Test]
		public void TooMuchUnboxing ()
		{
			// one (parameter) is unboxed 5 times
			AssertRuleFailure<Message> ("Equals", 1);
			// two parameters are unboxed 5 times
			AssertRuleFailure<Message> ("StaticEquals", 2);
			// one variable is unboxed 2 times
			AssertRuleFailure<AvoidUnneededUnboxingTest> ("CompareMessages", 1);
			// one field is unboxed 2 times
			AssertRuleFailure<AvoidUnneededUnboxingTest> ("CompareFieldsTwo", 1);
			// three variables are unboxed several times
			AssertRuleFailure<AvoidUnneededUnboxingTest> ("CompareVariables", 3);
		}

		[Test]
		public void SingleUnboxing ()
		{
			// one (parameter) is unboxed 1 time (ok)
			AssertRuleSuccess<Message> ("EqualsOk");
			// two parameters are unboxed 1 time (each)
			AssertRuleSuccess<Message> ("StaticEqualsOk");
			// two fields are unboxed 1 time (each)
			AssertRuleSuccess<AvoidUnneededUnboxingTest> ("CompareFields");
			// we can't be sure the same call returns the same value 
			// each time, i.e. the boxing may be required
			AssertRuleSuccess<AvoidUnneededUnboxingTest> ("CompareMessagesNoVariable");
		}

		[Test]
		public void NoUnboxing ()
		{
			AssertRuleDoesNotApply<Message> ("EqualsNone");
			AssertRuleDoesNotApply<AvoidUnneededUnboxingTest> ("GetMessage");
		}
	}
}
