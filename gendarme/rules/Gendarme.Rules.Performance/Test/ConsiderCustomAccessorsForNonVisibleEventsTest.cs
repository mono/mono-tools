//
// Unit tests for ConsiderCustomAccessorsForNonVisibleEventsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;

using Mono.Cecil;
using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

	[TestFixture]
	public class ConsiderCustomAccessorsForNonVisibleEventsTest : TypeRuleTestFixture<ConsiderCustomAccessorsForNonVisibleEventsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);

			AssertRuleDoesNotApply<ConsiderCustomAccessorsForNonVisibleEventsTest> ();
		}

		public class TestEventArgs : EventArgs {
		}

		public class PublicDefaultType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;
			// non-visible, warn as it would be better to have them without a lock
			internal event EventHandler Internal;
			private event EventHandler<TestEventArgs> Private;
		}

		public class PublicManualType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;

			// non-visible, ok they are lock-free
			EventHandlerList events = new EventHandlerList ();

			static object InternalEvent = new object ();
			internal event EventHandler Internal {
				add { events.AddHandler (InternalEvent, value); }
				remove { events.RemoveHandler (InternalEvent, value); }
			}

			static object PrivateEvent = new object ();
			private event EventHandler<TestEventArgs> Private {
				add { events.AddHandler (PrivateEvent, value); }
				remove { events.RemoveHandler (PrivateEvent, value); }
			}
		}

		protected class ProtectedDefaultType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;
			internal event EventHandler Internal;
			private event EventHandler<TestEventArgs> Private;
		}

		public class ProtectedManualType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;

			// non-visible, ok they are lock-free
			EventHandlerList events = new EventHandlerList ();

			static object InternalEvent = new object ();
			internal event EventHandler Internal {
				add { events.AddHandler (InternalEvent, value); }
				remove { events.RemoveHandler (InternalEvent, value); }
			}

			static object PrivateEvent = new object ();
			private event EventHandler<TestEventArgs> Private {
				add { events.AddHandler (PrivateEvent, value); }
				remove { events.RemoveHandler (PrivateEvent, value); }
			}
		}

		// type is private still it could be publicly visible (e.g. thru an interface)
		private class PrivateDefaultType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;
			// non-visible, warn as it would be better to have them without a lock
			internal event EventHandler Internal;
			private event EventHandler<TestEventArgs> Private;
		}

		// type is private still it could be publicly visible (e.g. thru an interface)
		private class PrivateManualType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;

			// non-visible, ok they are lock-free
			EventHandlerList events = new EventHandlerList ();

			static object InternalEvent = new object ();
			internal event EventHandler Internal {
				add { events.AddHandler (InternalEvent, value); }
				remove { events.RemoveHandler (InternalEvent, value); }
			}

			static object PrivateEvent = new object ();
			private event EventHandler<TestEventArgs> Private {
				add { events.AddHandler (PrivateEvent, value); }
				remove { events.RemoveHandler (PrivateEvent, value); }
			}
		}

		// type is internal still it could be publicly visible
		// (e.g. using an attribute to make it visible to other assemblies)
		internal class InternalDefaultType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;
			// non-visible, warn as it would be better to have them without a lock
			internal event EventHandler Internal;
			private event EventHandler<TestEventArgs> Private;
		}

		// type is internal still it could be publicly visible
		// (e.g. using an attribute to make it visible to other assemblies)
		internal class InternalManualType {
			// visible, ok to let the compiler make them synchronized
			public event EventHandler Public;
			protected event EventHandler<TestEventArgs> Protected;

			// non-visible, ok they are lock-free
			EventHandlerList events = new EventHandlerList ();

			static object InternalEvent = new object ();
			internal event EventHandler Internal {
				add { events.AddHandler (InternalEvent, value); }
				remove { events.RemoveHandler (InternalEvent, value); }
			}

			static object PrivateEvent = new object ();
			private event EventHandler<TestEventArgs> Private {
				add { events.AddHandler (PrivateEvent, value); }
				remove { events.RemoveHandler (PrivateEvent, value); }
			}
		}

		[Test]
		public void Visible ()
		{
			AssertRuleSuccess<PublicManualType> ();
			AssertRuleSuccess<ProtectedManualType> ();

			MethodDefinition md = DefinitionLoader.GetMethodDefinition<PublicDefaultType> ("add_Public");
			if (md.IsSynchronized) {
				// older CSC (and xMCS) compilers still generate it's own add/remove methods that set the
				// synchronize flags and we simply ignore them since they are out of the developer's control
				AssertRuleFailure<PublicDefaultType> (2);
				AssertRuleFailure<ProtectedDefaultType> (2);
			}
		}

		[Test]
		public void NonVisible ()
		{
			MethodDefinition md = DefinitionLoader.GetMethodDefinition<PrivateDefaultType> ("add_Public");
			if (!md.IsSynchronized)
				Assert.Ignore ("newer versions of CSC (e.g. 10.0) does not set the Synchronized");

			// older CSC (and xMCS) compilers still generate it's own add/remove methods that set the
			// synchronize flags and we simply ignore them since they are out of the developer's control
			AssertRuleFailure<PrivateDefaultType> (4);
			AssertRuleFailure<PrivateManualType> (2);

			AssertRuleFailure<InternalDefaultType> (4);
			AssertRuleFailure<InternalManualType> (2);
		}
	}
}
