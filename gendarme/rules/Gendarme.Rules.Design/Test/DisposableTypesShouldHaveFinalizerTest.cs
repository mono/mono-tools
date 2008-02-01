//
// Unit tests for DisposableTypesShouldHaveFinalizerRule
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
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design {

	class HasFinalizer : IDisposable {
		IntPtr A;
		~HasFinalizer ()
		{
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NoFinalizer : IDisposable {
		IntPtr A;
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}
	
	class NotDisposable {
		IntPtr A;
	}

	class NoNativeField : IDisposable {
		object A;
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NativeFieldArray : IDisposable {
		IntPtr [] A;
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NotDisposableBecauseStatic {
		static IntPtr A;
	}

	[TestFixture]
	public class DisposableTypesShouldHaveFinalizerTest {

		private DisposableTypesShouldHaveFinalizerRule rule;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DisposableTypesShouldHaveFinalizerRule ();
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestHasFinalizer ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.HasFinalizer");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestNoFinalizer ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NoFinalizer");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestNotDisposable ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NotDisposable");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestNoNativeFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NoNativeField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestNativeFieldArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestNotDisposableBecauseStatic ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NotDisposableBecauseStatic");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
