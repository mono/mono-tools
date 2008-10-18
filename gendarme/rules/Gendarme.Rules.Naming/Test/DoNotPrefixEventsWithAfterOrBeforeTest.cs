//
// Unit tests for DoNotPrefixEventsWithAfterOrBeforeTest
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

#pragma warning disable 67, 169

	[TestFixture]
	public class DoNotPrefixEventsWithAfterOrBeforeTest : TypeRuleTestFixture<DoNotPrefixEventsWithAfterOrBeforeRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		public class ClassicBad {
			public event EventHandler Before;
			public event EventHandler After;
		}

		public class CustomBad {
			public event AssemblyLoadEventHandler BeforeLoad;
			public event UnhandledExceptionEventHandler AfterException;
		}

		public class GenericsBad {
			public event EventHandler<RunnerEventArgs> BeforeGenerics;
			public event EventHandler<RunnerEventArgs> AfterGenerics;
		}

		public struct StructBad {
			public event EventHandler Before;
		}

		public interface IBad {
			event EventHandler After;
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ClassicBad> (2);
			AssertRuleFailure<CustomBad> (2);
			AssertRuleFailure<GenericsBad> (2);
			AssertRuleFailure<StructBad> (1);
			AssertRuleFailure<IBad> (1);
		}

		public class ClassicOk {
			// stetching a bit ;-)
			public event EventHandler JustBefore;
			public event EventHandler JustAfter;
		}

		public class CustomOk {
			public event ResolveEventHandler Resolving;
			public event ModuleResolveEventHandler Resolved;
		}

		public class GenericOk {
			public event EventHandler<RunnerEventArgs> Genericing;
			public event EventHandler<RunnerEventArgs> Genericed;
		}

		public struct StructOk {
			public event EventHandler Beforing;
		}

		[Test]
		public void Ok ()
		{
			AssertRuleSuccess<ClassicOk> ();
			AssertRuleSuccess<CustomOk> ();
			AssertRuleSuccess<GenericOk> ();
			AssertRuleSuccess<StructOk> ();
		}

		[Test]
		public void None ()
		{
			// no events
			AssertRuleSuccess (SimpleTypes.Class);
			AssertRuleSuccess (SimpleTypes.Interface);
			AssertRuleSuccess (SimpleTypes.Structure);
		}
	}
}
