// 
// Unit tests for DoNotDeclareVirtualMethodsInSealedTypeRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using Mono.Cecil;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	public sealed class SealedClassWithoutVirtualMethods {
		public int GetInt ()
		{
			return 42;
		}
	}

	[TestFixture]
	public class DoNotDeclareVirtualMethodsInSealedTypeTest : TypeRuleTestFixture<DoNotDeclareVirtualMethodsInSealedTypeRule> {

		private TypeDefinition sealed_class_with_virtual_method;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyFactory.GetAssembly (unit);

			sealed_class_with_virtual_method = assembly.MainModule.Types [typeof (SealedClassWithoutVirtualMethods).FullName].Clone ();
			sealed_class_with_virtual_method.Module = assembly.MainModule;
			MethodDefinition get_int = sealed_class_with_virtual_method.Methods [0];
			get_int.IsVirtual = true;
		}

		[Test]
		public void DoesNotApply ()
		{
			// delegates are always sealed - but the rule does not apply to them
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			// enums are always sealed - but the rule does not apply to them
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			// interfaces are not sealed - and the rule does not apply to them
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			// struct are always sealed - but we can't declare protected fields in them
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void SealedClassWithVirtualMethodTest ()
		{
			AssertRuleFailure (sealed_class_with_virtual_method, 1);
		}

		public class UnsealedClass {

			public virtual int GetInt ()
			{
				return 42;
			}
		}

		[Test]
		public void Unsealed ()
		{
			AssertRuleDoesNotApply<UnsealedClass> ();
		}

		public abstract class AbstractClass {
			public abstract string GetIt ();
		}

		public sealed class SealedClass : AbstractClass {

			public int GetInt ()
			{
				return 42;
			}

			public override string GetIt ()
			{
				return String.Empty;
			}

			public override string ToString ()
			{
				return base.ToString ();
			}
		}

		[Test]
		public void Override ()
		{
			AssertRuleDoesNotApply<AbstractClass> ();
			AssertRuleSuccess<SealedClass> ();
		}
	}
}
