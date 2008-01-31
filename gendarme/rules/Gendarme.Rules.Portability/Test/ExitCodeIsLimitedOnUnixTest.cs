// 
// Unit tests for ExitCodeIsLimitedOnUnixRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Portability;

using NUnit.Framework;

namespace Test.Rules.Portability {

	internal class CommonMainClass { }

	internal class GoodIntMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			if (arguments.Length > 0)
				return 0;
			else if (arguments.IsReadOnly)
				return 255;
			return 42;
		}
	}

	internal class UnsureIntMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			return arguments.Length;
		}
	}

	internal class TooBigReturnedMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			if (arguments.Length > 0)
				return 0;
			else if (arguments.IsReadOnly)
				return 256; // too big!
			return 42;
		}
	}

	internal class MinusOneReturnedMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			if (arguments.Length > 0)
				return -1; // bad
			else if (arguments.IsReadOnly)
				return 255;
			return 42;
		}
	}

	internal class SmallNegativeReturnedMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			if (arguments.Length > 0)
				return 42;
			else if (arguments.IsReadOnly)
				return -10; // bad
			return 77;
		}
	}

	internal class BigNegativeReturnedMainClass : CommonMainClass {
		public static int Main (string [] arguments)
		{
			if (arguments.Length > 0)
				return 42;
			else if (arguments.IsReadOnly)
				return -100000; // bad
			return 77;
		}
	}

	internal class EnvSetExitCodeTester {
		public void EmptyMethod () { }

		public void SetGoodExitCode ()
		{
			Environment.ExitCode = 0;
			Environment.ExitCode = 255;
		}

		public void SetMinusOneExitCode ()
		{
			Environment.ExitCode = 5;
			Environment.ExitCode = -1; // bad
			Environment.ExitCode = 42;
		}

		public void SetSmallNegativeExitCode ()
		{
			Environment.ExitCode = 10;
			Environment.ExitCode = -42; // bad
			Environment.ExitCode = 100;
		}

		public void SetTooBigExitCode ()
		{
			Environment.ExitCode = 7;
			Environment.ExitCode = 12345; // bad
			Environment.ExitCode = 3;
		}

		public void SetUnsureExitCode ()
		{
			Environment.ExitCode = 3;
			Environment.ExitCode = new Random ().Next (1024); // unsure			
			Environment.ExitCode = 96;
		}
	}

	internal class EnvExitTester {
		public void EmptyMethod () { }

		public void ExitWithGoodExitCode ()
		{
			Environment.Exit (0);
			Environment.Exit (255);
		}

		public void ExitWithMinusOneExitCode ()
		{
			Environment.Exit (5);
			Environment.Exit (-1); // bad
			Environment.Exit (42);
		}

		public void ExitWithSmallNegativeExitCode ()
		{
			Environment.Exit (10);
			Environment.Exit (-42); // bad
			Environment.Exit (100);
		}

		public void ExitWithTooBigExitCode ()
		{
			Environment.Exit (7);
			Environment.Exit (12345); // bad
			Environment.Exit (3);
		}

		public void ExitWithUnsureExitCode ()
		{
			Environment.Exit (3);
			Environment.Exit (new Random ().Next (1024)); // unsure			
			Environment.Exit (96);
		}
	}

	[TestFixture]
	public class ExitCodeIsLimitedOnUnixTest {

		private ExitCodeIsLimitedOnUnixRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;

		private AssemblyDefinition voidMainAssembly;
		private TypeDefinition envSetExitCodeTester;
		private TypeDefinition envExitTester;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			rule = new ExitCodeIsLimitedOnUnixRule ();
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			runner = new MinimalRunner ();

			// generate void Main () assembly
			voidMainAssembly = AssemblyFactory.DefineAssembly ("GoodAssembly", AssemblyKind.Console);
			TypeDefinition goodMainClass = new TypeDefinition ("MainClass", "", TypeAttributes.Class | TypeAttributes.Public, voidMainAssembly.MainModule.TypeReferences ["System.Object"]);
			MethodDefinition goodMain = new MethodDefinition ("Main", MethodAttributes.Static | MethodAttributes.Private, voidMainAssembly.MainModule.TypeReferences ["System.Void"]);
			goodMainClass.Methods.Add (goodMain);
			voidMainAssembly.MainModule.Types.Add (goodMainClass);
			voidMainAssembly.EntryPoint = goodMain;
			// inject there EnvExitCodeTester and EnvExitTester (their methods must be in executable in order to be checked)
			envSetExitCodeTester = voidMainAssembly.MainModule.Inject (assembly.MainModule.Types ["Test.Rules.Portability.EnvSetExitCodeTester"]);
			Assert.IsNotNull (envSetExitCodeTester);
			envExitTester = voidMainAssembly.MainModule.Inject (assembly.MainModule.Types ["Test.Rules.Portability.EnvExitTester"]);
			Assert.IsNotNull (envExitTester);
		}

		private void CheckMessageType (MessageCollection messageCollection, MessageType messageType)
		{
			IEnumerator enumerator = messageCollection.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				Message message = (Message) enumerator.Current;
				Assert.AreEqual (messageType, message.Type);
			}
		}

		private AssemblyDefinition GetAssemblyAndInject<TInjectedType> ()
			where TInjectedType : CommonMainClass
		{
			// return executable assembly with predefined entry point - Main () of TInjectedType
			string fullClassName = typeof (TInjectedType).FullName;
			AssemblyDefinition ass = AssemblyFactory.DefineAssembly (typeof (TInjectedType).Name + "Assembly", AssemblyKind.Console);
			TypeDefinition mainClass = ass.MainModule.Inject (assembly.MainModule.Types [fullClassName]);
			ass.EntryPoint = GetMethod (mainClass, "Main");
			return ass;
		}

		private MethodDefinition GetMethod (TypeDefinition type, string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		private MethodDefinition GetMethodForEnvSetExitCodeTest (string name)
		{
			return GetMethod (envSetExitCodeTester, name);
		}

		private MethodDefinition GetMethodForEnvExitTest (string name)
		{
			return GetMethod (envExitTester, name);
		}

		[Test]
		public void TestVoidMainAssembly ()
		{
			Assert.IsNull (rule.CheckAssembly (voidMainAssembly, runner));
		}

		[Test]
		public void TestGoodIntMainAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<GoodIntMainClass> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestUnsureIntMainAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<UnsureIntMainClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Warning);
		}

		[Test]
		public void TestTooBigReturnedAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<TooBigReturnedMainClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestMinusOneReturnedAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<MinusOneReturnedMainClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestSmallNegativeReturnedAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<SmallNegativeReturnedMainClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvSetBadExitCodeFromNonExecutable ()
		{
			// get method from this assembly, not generated one
			MessageCollection messages = rule.CheckMethod (GetMethod (assembly.MainModule.Types ["Test.Rules.Portability.EnvSetExitCodeTester"], "SetTooBigExitCode"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestNoEnvSetExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("EmptyMethod"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestEnvSetGoodExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetGoodExitCode"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestEnvSetMinusOneExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetMinusOneExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvSetSmallNegativeExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetSmallNegativeExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvSetTooBigExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetTooBigExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvSetUnsureExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetUnsureExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Warning);
		}

		[Test]
		public void TestEnvExitWithBadExitCodeFromNonExecutable ()
		{
			// get method from this assembly, not generated one
			MessageCollection messages = rule.CheckMethod (GetMethod (assembly.MainModule.Types ["Test.Rules.Portability.EnvExitTester"], "ExitWithTooBigExitCode"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestNoEnvExit ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("EmptyMethod"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestEnvExitWithGoodExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("ExitWithGoodExitCode"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestEnvExitWithMinusOneExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("ExitWithMinusOneExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvExitWithSmallNegativeExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("ExitWithSmallNegativeExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvExitWithTooBigExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("ExitWithTooBigExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Error);
		}

		[Test]
		public void TestEnvExitWithUnsureExitCode ()
		{
			MessageCollection messages = rule.CheckMethod (GetMethodForEnvExitTest ("ExitWithUnsureExitCode"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
			CheckMessageType (messages, MessageType.Warning);
		}
	}
}
