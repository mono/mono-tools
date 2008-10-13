//
// Unit tests for RemoveUnneededFinalizerRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2006, 2008 Novell, Inc (http://www.novell.com)
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
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class RemoveUnneededFinalizerTest : TypeRuleTestFixture<RemoveUnneededFinalizerRule> {

		class NoDestructorClass {
		}

		[Test]
		public void NoDestructor ()
		{
			AssertRuleDoesNotApply<NoDestructorClass> ();
		}

		class EmptyDestructorClass {

			~EmptyDestructorClass ()
			{
			}
		}

		[Test]
		public void EmptyDestructor ()
		{
			AssertRuleFailure<EmptyDestructorClass> (1);
		}

		class DestructorClass {

			IntPtr ptr;

			public DestructorClass ()
			{
				ptr = (IntPtr) 1;
			}

			public IntPtr Handle {
				get { return ptr; }
			}

			~DestructorClass ()
			{
				ptr = IntPtr.Zero;
			}
		}

		[Test]
		public void Destructor ()
		{
			AssertRuleSuccess<DestructorClass> ();
		}

		class NullifyFieldClass : DestructorClass {

			object field;

			~NullifyFieldClass ()
			{
				field = null;
			}
		}

		[Test]
		public void NullifyField ()
		{
			AssertRuleFailure<NullifyFieldClass> (1);
		}

		class SettingFieldClass : DestructorClass {

			object field;

			~SettingFieldClass ()
			{
				field = this;
			}
		}

		class DelegatedCleanupClass : DestructorClass {

			object field;

			~DelegatedCleanupClass ()
			{
				Cleanup ();
			}

			void Cleanup ()
			{
				field = null;
			}
		}

		[Test]
		public void MoreCoverage ()
		{
			AssertRuleSuccess<SettingFieldClass> ();
			AssertRuleSuccess<DelegatedCleanupClass> ();
		}
	}
}
