//
// Unit tests for AvoidVisibleNestedTypesRule
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

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class AvoidVisibleNestedTypesTest : TypeRuleTestFixture<AvoidVisibleNestedTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// note: SimpleTypes.* are nested
			AssertRuleDoesNotApply<Confidence> ();		// enum
			AssertRuleDoesNotApply<IRule> ();		// interface
			AssertRuleDoesNotApply<AvoidVisibleNestedTypesTest> ();
		}

		public enum PublicEnum {
		}

		public interface PublicInterface {
		}

		public class PublicClass {
		}

		public struct PublicStruct {
		}

		[Test]
		public void Public ()
		{
			AssertRuleFailure<PublicEnum> (1);
			AssertRuleFailure<PublicInterface> (1);
			AssertRuleFailure<PublicClass> (1);
			AssertRuleFailure<PublicStruct> (1);
		}

		protected enum ProtectedEnum {
		}

		protected interface ProtectedInterface {
		}

		protected class ProtectedClass {
		}

		protected struct ProtectedStruct {
		}

		[Test]
		public void Protected ()
		{
			AssertRuleFailure<ProtectedEnum> (1);
			AssertRuleFailure<ProtectedInterface> (1);
			AssertRuleFailure<ProtectedClass> (1);
			AssertRuleFailure<ProtectedStruct> (1);
		}

		private enum PrivateEnum {
		}

		private interface PrivateInterface {
		}

		private class PrivateClass {
		}

		private struct PrivateStruct {
		}

		[Test]
		public void Private ()
		{
			AssertRuleSuccess<PrivateEnum> ();
			AssertRuleSuccess<PrivateInterface> ();
			AssertRuleSuccess<PrivateClass> ();
			AssertRuleSuccess<PrivateStruct> ();
		}

		private enum InternalEnum {
		}

		private interface InternalInterface {
		}

		private class InternalClass {
		}

		private struct InternalStruct {
		}

		[Test]
		public void Internal ()
		{
			AssertRuleSuccess<InternalEnum> ();
			AssertRuleSuccess<InternalInterface> ();
			AssertRuleSuccess<InternalClass> ();
			AssertRuleSuccess<InternalStruct> ();
		}
	}
}
