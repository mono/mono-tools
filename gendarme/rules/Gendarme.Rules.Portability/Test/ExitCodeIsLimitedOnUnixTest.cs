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
using Test.Rules.Helpers;

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
		private TestRunner runner;
		private AssemblyDefinition assembly;

		private AssemblyDefinition voidMainAssembly;
		private TypeDefinition envSetExitCodeTester;
		private TypeDefinition envExitTester;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new ExitCodeIsLimitedOnUnixRule ();
			runner = new TestRunner (rule);

			// generate void Main () assembly
			voidMainAssembly = CreateAssembly ("GoodAssembly", ModuleKind.Console);
			TypeDefinition goodMainClass = new TypeDefinition ("", "MainClass", TypeAttributes.Class | TypeAttributes.Public, voidMainAssembly.MainModule.TypeSystem.Object);
			MethodDefinition goodMain = new MethodDefinition ("Main", MethodAttributes.Static | MethodAttributes.Private, voidMainAssembly.MainModule.TypeSystem.Void);
			goodMainClass.Methods.Add (goodMain);
			voidMainAssembly.MainModule.Types.Add (goodMainClass);
			voidMainAssembly.EntryPoint = goodMain;
			// inject there EnvExitCodeTester and EnvExitTester (their methods must be in executable in order to be checked)
			envSetExitCodeTester = Inject (assembly.MainModule.GetType ("Test.Rules.Portability.EnvSetExitCodeTester"), voidMainAssembly);
			Assert.IsNotNull (envSetExitCodeTester);
			envExitTester = Inject (assembly.MainModule.GetType ("Test.Rules.Portability.EnvExitTester"), voidMainAssembly);
			Assert.IsNotNull (envExitTester);
		}

		static AssemblyDefinition CreateAssembly (string name, ModuleKind kind)
		{
			return AssemblyDefinition.CreateAssembly (new AssemblyNameDefinition (name, new Version ()), name, kind);
		}

		// horrible hack, pretend to inject a fully loaded type in another assembly
		static TypeDefinition Inject (TypeDefinition type, AssemblyDefinition target)
		{
			var module = ModuleDefinition.ReadModule (
				type.Module.FullyQualifiedName,
				new ReaderParameters { ReadingMode = ReadingMode.Immediate });

			type = module.GetType (type.FullName);

			foreach (var method in type.Methods) {
				// load the method body
				var _ = method.Body;
			}

			module.Types.Remove (type);

			target.MainModule.Types.Add (type);

			return type;
		}

		private AssemblyDefinition GetAssemblyAndInject<TInjectedType> ()
			where TInjectedType : CommonMainClass
		{
			// return executable assembly with predefined entry point - Main () of TInjectedType
			string fullClassName = typeof (TInjectedType).FullName;
			AssemblyDefinition ass = CreateAssembly (typeof (TInjectedType).Name + "Assembly", ModuleKind.Console);
			TypeDefinition mainClass = Inject (assembly.MainModule.GetType (fullClassName), ass);

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
			Assert.AreEqual (RuleResult.DoesNotApply, rule.CheckAssembly (voidMainAssembly));
		}

		[Test]
		public void TestGoodIntMainAssembly ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckAssembly (GetAssemblyAndInject<GoodIntMainClass> ()));
		}

		[Test]
		public void TestUnsureIntMainAssembly ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckAssembly (GetAssemblyAndInject<UnsureIntMainClass> ()), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.Low, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestTooBigReturnedAssembly ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckAssembly (GetAssemblyAndInject<TooBigReturnedMainClass> ()), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestMinusOneReturnedAssembly ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckAssembly (GetAssemblyAndInject<MinusOneReturnedMainClass> ()), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestSmallNegativeReturnedAssembly ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckAssembly (GetAssemblyAndInject<SmallNegativeReturnedMainClass> ()), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvSetBadExitCodeFromNonExecutable ()
		{
			// get method from this assembly, not generated one
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethod (assembly.MainModule.GetType ("Test.Rules.Portability.EnvSetExitCodeTester"), "SetTooBigExitCode")));
		}

		[Test]
		public void TestNoEnvSetExitCode ()
		{
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("EmptyMethod")));
		}

		[Test]
		public void TestEnvSetGoodExitCode ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetGoodExitCode")));
		}

		[Test]
		public void TestEnvSetMinusOneExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetMinusOneExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvSetSmallNegativeExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetSmallNegativeExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvSetTooBigExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetTooBigExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvSetUnsureExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvSetExitCodeTest ("SetUnsureExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.Low, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvExitWithBadExitCodeFromNonExecutable ()
		{
			// get method from this assembly, not generated one
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethod (assembly.MainModule.GetType ("Test.Rules.Portability.EnvExitTester"), "ExitWithTooBigExitCode")));
		}

		[Test]
		public void TestNoEnvExit ()
		{
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (GetMethodForEnvExitTest ("EmptyMethod")));
		}

		[Test]
		public void TestEnvExitWithGoodExitCode ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetMethodForEnvExitTest ("ExitWithGoodExitCode")));
		}

		[Test]
		public void TestEnvExitWithMinusOneExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvExitTest ("ExitWithMinusOneExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvExitWithSmallNegativeExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvExitTest ("ExitWithSmallNegativeExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvExitWithTooBigExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvExitTest ("ExitWithTooBigExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.High, runner.Defects [0].Confidence, "Confidence");
		}

		[Test]
		public void TestEnvExitWithUnsureExitCode ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetMethodForEnvExitTest ("ExitWithUnsureExitCode")), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
			Assert.AreEqual (Confidence.Low, runner.Defects [0].Confidence, "Confidence");
		}
	}
}
