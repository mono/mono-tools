//
// Unit tests for ParameterNamesShouldMatchOverridenMethodTest
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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
using Gendarme.Framework.Rocks;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Naming {

	interface ISomeInterface {
		bool InterfaceMethod (int im);
	}

	interface ISomeInterface2 {
		bool InterfaceMethod2 (int im);
	}

	abstract public class SuperBaseClass {
		protected virtual void VirtualSuperIncorrect (int vsi1, bool vsi2)
		{
		}
		protected virtual void VirtualSuperIncorrect (int vsi1, int vsi2_)
		{
		}
	}

	abstract public class BaseClass : SuperBaseClass {
		protected virtual void VirtualCorrect (int vc1, int vc2)
		{
		}

		protected virtual void VirtualIncorrect (int vi1, int vi2)
		{
		}

		protected abstract void AbstractCorrect (int ac1, int ac2);

		protected abstract void AbstractIncorrect (int ai1, int ai2);

		protected virtual void NoOverwrite (int a, int b)
		{
		}
	}

	[TestFixture]
	public class ParameterNamesShouldMatchOverridenMethodTest : BaseClass, ISomeInterface, ISomeInterface2 {

		private ParameterNamesShouldMatchOverriddenMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Naming.ParameterNamesShouldMatchOverridenMethodTest"];
			rule = new ParameterNamesShouldMatchOverriddenMethodRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		protected override void VirtualCorrect (int vc1, int vc2)
		{
		}

		[Test]
		public void TestVirtualCorrect ()
		{
			MethodDefinition method = GetTest ("VirtualCorrect");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		protected override void VirtualIncorrect (int vi1, int vi2a)
		{
		}

		[Test]
		public void TestVirtualIncorrect ()
		{
			MethodDefinition method = GetTest ("VirtualIncorrect");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		protected override void VirtualSuperIncorrect (int vsi1, bool vsi2_)
		{
		}

		[Test]
		public void TestVirtualSuperIncorrect ()
		{
			MethodDefinition method = GetTest ("VirtualSuperIncorrect");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		protected override void AbstractCorrect (int ac1, int ac2)
		{
			throw new NotImplementedException ();
		}

		[Test]
		public void TestAbstractCorrect ()
		{
			MethodDefinition method = GetTest ("AbstractCorrect");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		protected override void AbstractIncorrect (int ai1, int ai2_)
		{
			throw new NotImplementedException ();
		}

		[Test]
		public void TestAbstractIncorrect ()
		{
			MethodDefinition method = GetTest ("AbstractIncorrect");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		protected virtual void NoOverwrite (int a, int bb)
		{
		}

		[Test]
		public void TestNoOverwrite ()
		{
			MethodDefinition method = GetTest ("NoOverwrite");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public bool InterfaceMethod (int im_)
		{
			return false;
		}

		[Test]
		public void TestInterfaceMethod ()
		{
			MethodDefinition method = GetTest ("InterfaceMethod");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		bool ISomeInterface2.InterfaceMethod2 (int im_)
		{
			return false;
		}

		[Test]
		public void TestInterfaceMethod2 ()
		{
			MethodDefinition method = GetTest ("Test.Rules.Naming.ISomeInterface2.InterfaceMethod2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
