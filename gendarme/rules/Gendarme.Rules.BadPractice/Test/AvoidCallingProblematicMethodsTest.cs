//
// Unit tests for AvoidCallingProblematicMethodsRule 
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Globalization;
using System.Configuration.Assemblies;
using System.Security.Policy;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Gendarme.Rules.BadPractice;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.BadPractice {
	[TestFixture]
	public class AvoidCallingProblematicMethodsTest : MethodRuleTestFixture<AvoidCallingProblematicMethodsRule> {

		[Test]
		public void DoesNotApply () 
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public void MethodWithGCCall ()
		{
			List<string> list = new List<string> ();
			list.Add ("foo");
			list.Add ("bar");
			list = null;
			GC.Collect ();
			GC.Collect (1);
		}

		[Test]
		public void MethodWithGCCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithGCCall", 2);
		}

		public void MethodWithThreadSuspendCall ()
		{
			Thread thread = new Thread (delegate () {
				Console.WriteLine ("Stupid code");
			});

			thread.Suspend ();
		}

		[Test]
		public void MethodWithThreadSuspendCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithThreadSuspendCall");
		}

		public void MethodWithThreadResumeCall ()
		{
			Thread thread = new Thread (delegate () {
				Console.WriteLine ("Useless code");
			});

			thread.Resume ();
		}

		[Test]
		public void MethodWithThreadResumeCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithThreadResumeCall");
		}

		public void MethodWithInvokeMemberWithPrivateFlagsCall ()
		{
			this.GetType ().InvokeMember ("Foo", BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, null, Type.EmptyTypes);
			this.GetType ().InvokeMember ("Foo", BindingFlags.NonPublic | BindingFlags.Static, null, null, Type.EmptyTypes, CultureInfo.CurrentCulture);
			this.GetType ().InvokeMember ("Foo", BindingFlags.NonPublic | BindingFlags.Instance, null, null, Type.EmptyTypes, null, CultureInfo.CurrentCulture, null);
		}

		[Test]
		public void MethodWithInvokeMemberWithPrivateFlagsCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithPrivateFlagsCall", 3);
		}

		public void MethodWithInvokeMemberWithoutPrivateFlagsCall ()
		{
			this.GetType ().InvokeMember ("Foo", BindingFlags.Public | BindingFlags.IgnoreCase, null, null, Type.EmptyTypes);
			this.GetType ().InvokeMember ("Foo", BindingFlags.Public | BindingFlags.Instance, null, null, Type.EmptyTypes, CultureInfo.CurrentCulture);
			this.GetType ().InvokeMember ("Foo", BindingFlags.Public | BindingFlags.DeclaredOnly, null, null, Type.EmptyTypes, null, CultureInfo.CurrentCulture, null);
		}

		public void MethodWithInvokeMemberWithUnkownFlagsCall (BindingFlags flags)
		{
			this.GetType ().InvokeMember ("Foo", flags, null, null, Type.EmptyTypes);
		}

		public void MethodWithInvokeMemberWithLargeValueCall ()
		{
			BindingFlags flags = (BindingFlags) Int32.MinValue;
			this.GetType ().InvokeMember ("Foo", flags, null, null, Type.EmptyTypes, null, CultureInfo.CurrentCulture, null);
		}

		[Test]
		public void MethodWithInvokeMemberWithoutPrivateFlagsCallTest ()
		{
			AssertRuleSuccess<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithoutPrivateFlagsCall");
			AssertRuleSuccess<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithUnkownFlagsCall");
			AssertRuleSuccess<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithLargeValueCall");
		}
		
		private class MySafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
			public MySafeHandle () : base (true)
			{
			}

			protected override bool ReleaseHandle () 
			{
				return true;
			}
		}

		public void MethodWithSafeHandleDangerousGetHandleCall ()
		{
			MySafeHandle myHandle = new MySafeHandle ();
			IntPtr handlePtr = myHandle.DangerousGetHandle ();
		}

		[Test]
		public void MethodWithSafeHandleDangerousGetHandleCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithSafeHandleDangerousGetHandleCall");
		}

		public void MethodWithAssemblyLoadFromCall ()
		{
			Assembly.LoadFrom ("myAssembly.dll");	
			Assembly.LoadFrom ("myAssembly.dll", new Evidence ());
			Assembly.LoadFrom ("myAssembly.dll", new Evidence (), null, AssemblyHashAlgorithm.None);

		}

		[Test]
		public void MethodWithAssemblyLoadFromCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadFromCall", 3);
		}

		public void MethodWithAssemblyLoadFileCall ()
		{
			Assembly.LoadFile ("myAssembly.dll");
			Assembly.LoadFile ("myAssembly.dll", new Evidence ());

		}

		[Test]
		public void MethodWithAssemblyLoadFileCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadFileCall", 2);
		}

		public void MethodWithAssemblyLoadWithPartialNameCall ()
		{
			Assembly.LoadWithPartialName ("MyAssembly");
			Assembly.LoadWithPartialName ("MyAssembly", new Evidence ());
		}

		[Test]
		public void MethodWithAssemblyLoadWithPartialNameCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadWithPartialNameCall", 2);
		}

		public void MethodWithouAnyDangerousCall ()
		{
			List<string> list = new List<string> ();
			list.Add ("Foo");
			list.Add ("Bar");
		}

		[Test]
		public void MethodWithouAnyDangerousCallTest ()
		{
			AssertRuleSuccess<AvoidCallingProblematicMethodsTest> ("MethodWithouAnyDangerousCallTest");
		}
	}
}
