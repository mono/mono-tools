// 
// Unit tests for ParameterRocksTest
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

using Gendarme.Framework.Rocks;

using Mono.Cecil;
using NUnit.Framework;

namespace Test.Framework.Rocks {

	[TestFixture]
	public class ParameterRocksTest {

		private AssemblyDefinition assembly;
		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Framework.Rocks.ParameterRocksTest");
		}

		public void UseParams (int a, params int [] args)
		{
		}

		[Test]
		public void IsParams ()
		{
			ParameterDefinition pd = null;
			Assert.IsFalse (pd.IsParams (), "null");

			MethodDefinition md = type.GetMethod ("UseParams");
			Assert.IsFalse (md.Parameters [0].IsParams (), "0");
			Assert.IsTrue (md.Parameters [1].IsParams (), "1");
		}

		public void UseRef (int a, ref int b)
		{
		}

		[Test]
		public void IsRef ()
		{
			ParameterReference pr = null;
			Assert.IsFalse (pr.IsRef (), "null");

			MethodDefinition md = type.GetMethod ("UseRef");
			Assert.IsFalse (md.Parameters [0].IsRef (), "0");
			Assert.IsTrue (md.Parameters [1].IsRef (), "1");
		}
	}
}

