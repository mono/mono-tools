// 
// Unit tests for FinalizersShouldCallBaseClassFinalizerRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;

namespace Test.Rules.Design {

	internal class NoFinalizerDefinedClass {
	}

	internal class FinalizerCallingBaseFinalizerClass {
		~FinalizerCallingBaseFinalizerClass ()
		{
			// do some stuff
			int x = 2 + 2;
			object o = x.ToString ();
			int y = o.GetHashCode ();
			x = y;

			// compiler generates here base.Finalize () call that looks like this:

			// call System.Void System.Object::Finalize()
			// endfinally 
			// ret 
		}
	}

	[TestFixture]
	public class FinalizersShouldCallBaseClassFinalizerTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new FinalizersShouldCallBaseClassFinalizerRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestNoFinalizerDefinedClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<NoFinalizerDefinedClass> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestFinalizerCallingBaseFinalizerClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<FinalizerCallingBaseFinalizerClass> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestFinalizerNotCallingBaseFinalizerClass ()
		{
			TypeDefinition typeDefinition = GetTest<FinalizerCallingBaseFinalizerClass> ().Clone () as TypeDefinition;
			MethodDefinition finalizer = typeDefinition.Methods.GetMethod ("Finalize") [0];
			// remove base call
			foreach (Instruction current in finalizer.Body.Instructions) {
				if (current.OpCode.Code == Code.Call) { // it's call
					MethodReference mr = (current.Operand as MethodReference);
					if (mr != null && mr.Name == "Finalize") { // it's Finalize () call
						finalizer.Body.CilWorker.Remove (current); // remove it for our test
						break;
					}
				}

			}
			MessageCollection messages = rule.CheckType (typeDefinition, runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}
	}
}
