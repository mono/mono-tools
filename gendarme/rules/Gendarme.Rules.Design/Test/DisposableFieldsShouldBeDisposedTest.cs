//
// Unit tests for DisposableFieldsShouldBeDisposedRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Design;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public class DisposableFieldsShouldBeDisposedTest {

		private DisposableFieldsShouldBeDisposedRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DisposableFieldsShouldBeDisposedRule ();
			runner = new TestRunner (rule);
		}

		public TypeDefinition GetTest (string name)
		{
			foreach (TypeDefinition type in assembly.MainModule.Types ["Test.Rules.Design.DisposableFieldsShouldBeDisposedTest"].NestedTypes) {
				if (type.Name == name)
					return type;
			}
			return null;
		}


		class FalsePositive : IDisposable {
			int A;
			object b;

			public void Dispose () //no warning
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void TestFalsePositive ()
		{
			TypeDefinition type = GetTest ("FalsePositive");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class Disposable : IDisposable {
			public Disposable A;
			public void Dispose () //no warning
			{
				A.Dispose ();
			}
		}

		[Test]
		public void TestDisposable ()
		{
			TypeDefinition type = GetTest ("Disposable");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class ExtendsDispose : Disposable, IDisposable {
			void Dispose () //warning: should call base
			{

			}
		}

		[Test]
		public void TestExtendsDispose ()
		{
			TypeDefinition type = GetTest ("ExtendsDispose");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		

		class ExtendsDisposeCallsBase : Disposable, IDisposable {
			void Dispose () //no warning
			{
				base.Dispose ();
			}
		}

		[Test]
		public void TestExtendsDisposeCallsBase ()
		{
			TypeDefinition type = GetTest ("ExtendsDisposeCallsBase");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class ExtendsDispose2 : Disposable, IDisposable {
			public Disposable B;
			void Dispose () //warn: should dispose B
			{
				base.Dispose ();
			}
		}

		[Test]
		public void TestExtendsDispose2 ()
		{
			TypeDefinition type = GetTest ("ExtendsDispose2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		

		class DisposeableFieldsCorrect : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //no warning
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsCorrect ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsCorrect");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class MultipleDisposeableFieldsCorrect : IDisposable {
			object A;
			Disposable B;
			Disposable C;

			public void Dispose () //no warning
			{
				B.Dispose ();
				C.Dispose ();
			}
		}

		[Test]
		public void TestMultipleDisposeableFieldsCorrect ()
		{
			TypeDefinition type = GetTest ("MultipleDisposeableFieldsCorrect");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		
		class DisposeableFieldsIncorrect : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //warn
			{
				A = B;
				int f = B.GetHashCode ();
			}
		}

		[Test]
		public void TestDisposeableFieldsIncorrect ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsIncorrect");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		

		class DisposeableFieldsDisposePattern : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //warn
			{
				A = B;
				Dispose (true);
			}

			private void Dispose (bool disposing)
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsDisposePattern ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsDisposePattern");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class DisposeableFieldsExplicit : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}

		}

		[Test]
		public void TestDisposeableFieldsExplicit ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsExplicit");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		class DisposeableFieldsTwoCorrect : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}

			void Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsTwoCorrect ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsTwoCorrect");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		class DisposeableFieldsTwoIncorrect : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{

			}

			void Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsTwoIncorrect ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsTwoIncorrect");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}


		class DisposeableFieldsWithStaticExplicit : IDisposable {
			object A;
			Disposable B;
			static Disposable C;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsWithStaticExplicit ()
		{
			TypeDefinition type = GetTest ("DisposeableFieldsWithStaticExplicit");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
