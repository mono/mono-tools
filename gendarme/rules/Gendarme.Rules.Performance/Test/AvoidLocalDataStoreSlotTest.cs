//
// Unit tests for AvoidLocalDataStoreSlotRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting.Contexts;
using System.Threading;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidLocalDataStoreSlotTest : MethodRuleTestFixture<AvoidLocalDataStoreSlotRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls to other methods
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		class BadThreadLocalStorage {

			LocalDataStoreSlot lds;

			public BadThreadLocalStorage ()
			{
				lds = Thread.AllocateDataSlot ();
			}

			private byte [] Key {
				get {
					return (byte []) Thread.GetData (lds);
				}
				set {
					Thread.SetData (lds, value);
				}
			}

			private byte [] GetKeyCopy ()
			{
				return (byte []) (Thread.GetData (lds) as byte []).Clone ();
			}
		}

		class BadNamedThreadLocalStorage {

			public BadNamedThreadLocalStorage ()
			{
				Thread.AllocateNamedDataSlot ("bad");
			}

			~BadNamedThreadLocalStorage ()
			{
				Thread.FreeNamedDataSlot ("bad");
			}

			LocalDataStoreSlot GetLocalDataStoreSlot ()
			{
				return Thread.GetNamedDataSlot ("bad");
			}

			private byte [] Key {
				get {
					return (byte []) Thread.GetData (GetLocalDataStoreSlot ());
				}
				set {
					Thread.SetData (GetLocalDataStoreSlot (), value);
				}
			}

			private byte [] GetKeyCopy ()
			{
				return (byte []) (Thread.GetData (GetLocalDataStoreSlot ()) as byte []).Clone ();
			}
		}

		class GoodThreadLocalStorage {

			[ThreadStatic]
			static byte[] key;

			private byte [] Key {
				get { return key; }
				set { key = (byte[]) value.Clone (); }
			}

			private byte [] GetKeyCopy ()
			{
				return (byte[]) key.Clone ();
			}
		}

		[Test]
		public void ThreadLocalStorage ()
		{
			AssertRuleFailure<BadThreadLocalStorage> (".ctor");
			AssertRuleFailure<BadThreadLocalStorage> ("get_Key");
			AssertRuleFailure<BadThreadLocalStorage> ("set_Key");
			AssertRuleFailure<BadThreadLocalStorage> ("GetKeyCopy");

			AssertRuleFailure<BadNamedThreadLocalStorage> (".ctor");
			AssertRuleFailure<BadNamedThreadLocalStorage> ("Finalize");
			AssertRuleFailure<BadNamedThreadLocalStorage> ("GetLocalDataStoreSlot");
			AssertRuleFailure<BadNamedThreadLocalStorage> ("get_Key");
			AssertRuleFailure<BadNamedThreadLocalStorage> ("set_Key");
			AssertRuleFailure<BadNamedThreadLocalStorage> ("GetKeyCopy");

			// no call
			AssertRuleDoesNotApply<GoodThreadLocalStorage> ("get_Key");
			AssertRuleSuccess<GoodThreadLocalStorage> ("set_Key");
			AssertRuleSuccess<GoodThreadLocalStorage> ("GetKeyCopy");
		}

		class BadContextLocalStorage {

			LocalDataStoreSlot lds;

			public BadContextLocalStorage ()
			{
				lds = Context.AllocateDataSlot ();
			}

			private byte [] Key {
				get { 
					return (byte[]) Context.GetData (lds);
				}
				set {
					Context.SetData (lds, value);
				}
			}

			private byte [] GetKeyCopy ()
			{
				return (byte []) (Context.GetData (lds) as byte []).Clone ();
			}
		}

		class BadNamedContextLocalStorage {

			public BadNamedContextLocalStorage ()
			{
				Context.AllocateNamedDataSlot ("bad");
			}

			~BadNamedContextLocalStorage ()
			{
				Context.FreeNamedDataSlot ("bad");
			}

			LocalDataStoreSlot GetLocalDataStoreSlot ()
			{
				return Context.GetNamedDataSlot ("bad");
			}

			private byte [] Key {
				get { 
					return (byte[]) Context.GetData (GetLocalDataStoreSlot ());
				}
				set {
					Context.SetData (GetLocalDataStoreSlot (), value);
				}
			}

			private byte [] GetKeyCopy ()
			{
				return (byte []) (Context.GetData (GetLocalDataStoreSlot ()) as byte []).Clone ();
			}
		}

		class GoodContextLocalStorage {

			[ContextStatic]
			static byte [] key;

			private byte [] Key {
				get { return key; }
				set { key = (byte[]) value.Clone (); }
			}

			private byte [] GetKeyCopy ()
			{
				return (byte []) key.Clone ();
			}
		}

		[Test]
		public void ContextLocalStorage ()
		{
			AssertRuleFailure<BadContextLocalStorage> (".ctor");
			AssertRuleFailure<BadContextLocalStorage> ("get_Key");
			AssertRuleFailure<BadContextLocalStorage> ("set_Key");
			AssertRuleFailure<BadContextLocalStorage> ("GetKeyCopy");

			AssertRuleFailure<BadNamedContextLocalStorage> (".ctor");
			AssertRuleFailure<BadNamedContextLocalStorage> ("Finalize");
			AssertRuleFailure<BadNamedContextLocalStorage> ("GetLocalDataStoreSlot");
			AssertRuleFailure<BadNamedContextLocalStorage> ("get_Key");
			AssertRuleFailure<BadNamedContextLocalStorage> ("set_Key");
			AssertRuleFailure<BadNamedContextLocalStorage> ("GetKeyCopy");

			// no call
			AssertRuleDoesNotApply<GoodContextLocalStorage> ("get_Key");
			AssertRuleSuccess<GoodContextLocalStorage> ("set_Key");
			AssertRuleSuccess<GoodContextLocalStorage> ("GetKeyCopy");
		}

		void UsingThread ()
		{
			Console.WriteLine (Thread.GetDomainID ());
		}

		void UsingContext ()
		{
			Console.WriteLine (Context.DefaultContext.ContextID);
		}

		[Test]
		public void OtherUsage ()
		{
			AssertRuleSuccess<AvoidLocalDataStoreSlotTest> ("UsingThread");
			AssertRuleSuccess<AvoidLocalDataStoreSlotTest> ("UsingContext");
		}
	}
}

