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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

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
	public class FinalizersShouldCallBaseClassFinalizerTest : TypeRuleTestFixture<FinalizersShouldCallBaseClassFinalizerRule> {

		TypeDefinition finalizer_not_calling_base_class; 

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);

			finalizer_not_calling_base_class = new TypeDefinition (
				"Test.Rules.Correctness",
				"FinalizerNotCallingBaseClass",
				TypeAttributes.NotPublic,
				assembly.MainModule.TypeSystem.Object);

			assembly.MainModule.Types.Add (finalizer_not_calling_base_class);

			var finalizer = new MethodDefinition (
				"Finalize",
				MethodAttributes.Public | MethodAttributes.Virtual,
				assembly.MainModule.TypeSystem.Void);

			finalizer_not_calling_base_class.Methods.Add (finalizer);

			var il = finalizer.Body.GetILProcessor ();
			il.Emit (OpCodes.Ret);
		}

		[Test]
		public void TestNoFinalizerDefinedClass ()
		{
			AssertRuleDoesNotApply<NoFinalizerDefinedClass> ();
		}

		[Test]
		public void TestFinalizerCallingBaseFinalizerClass ()
		{
			AssertRuleSuccess<FinalizerCallingBaseFinalizerClass> ();
		}

		[Test]
		public void TestFinalizerNotCallingBaseFinalizerClass ()
		{
			AssertRuleFailure (finalizer_not_calling_base_class, 1);
		}

		[Test]
		public void NoBaseType ()
		{
			AssertRuleDoesNotApply<object> ();
		}
	}
}
