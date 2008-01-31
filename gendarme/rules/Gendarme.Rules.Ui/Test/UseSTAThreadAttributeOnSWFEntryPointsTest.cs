// 
// Unit tests for UseSTAThreadAttributeOnSWFEntryPointsTest
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Ui;

using NUnit.Framework;

namespace Test.Rules.Ui {
	internal class CommonMainClass { }

	internal class NoAttributesMain : CommonMainClass {
		public static void Main ()
		{
		}
	}

	internal class STAThreadMain : CommonMainClass {
		[STAThread]
		public static void Main ()
		{
		}
	}

	internal class MTAThreadMain : CommonMainClass {
		[MTAThread]
		public static void Main ()
		{
		}
	}

	internal class BothSTAAndMTAThreadMain : CommonMainClass {
		[STAThread]
		[MTAThread]
		public static void Main ()
		{
		}
	}

	[TestFixture]
	public class UseSTAThreadAttributeOnSWFEntryPointsTest {

		private UseSTAThreadAttributeOnSWFEntryPointsRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			rule = new UseSTAThreadAttributeOnSWFEntryPointsRule ();
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			runner = new MinimalRunner ();
		}

		private AssemblyDefinition GetAssemblyAndInject<TInjectedType> (bool SWF)
			where TInjectedType : CommonMainClass
		{
			// return executable assembly with predefined entry point - Main () of TInjectedType
			string fullClassName = typeof (TInjectedType).FullName;
			AssemblyDefinition ass = AssemblyFactory.DefineAssembly (typeof (TInjectedType).Name + "Assembly", AssemblyKind.Console);
			if (SWF) {
				ass.Kind = AssemblyKind.Windows;
				AssemblyNameReference winFormsRef = new AssemblyNameReference ();
				winFormsRef.Name = "System.Windows.Forms";
				winFormsRef.PublicKeyToken = new byte [] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
				winFormsRef.Version = new Version (2, 0, 0, 0);
				ass.MainModule.AssemblyReferences.Add (winFormsRef);
			}
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

		[Test]
		public void TestNoEntryPoint ()
		{
			MessageCollection messages = rule.CheckAssembly (assembly, runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestNoTANonSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<NoAttributesMain> (false), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestNoTASWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<NoAttributesMain> (true), runner);
			Assert.IsNotNull (messages);
		}

		[Test]
		public void TestSTANonSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<STAThreadMain> (false), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestMTANonSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<MTAThreadMain> (false), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestSTAThreadSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<STAThreadMain> (true), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestMTAThreadSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<MTAThreadMain> (true), runner);
			Assert.IsNotNull (messages);
		}

		[Test]
		public void TestSTAAndMTAThreadSWFAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (GetAssemblyAndInject<BothSTAAndMTAThreadMain> (true), runner);
			Assert.IsNotNull (messages);
		}
	}
}
