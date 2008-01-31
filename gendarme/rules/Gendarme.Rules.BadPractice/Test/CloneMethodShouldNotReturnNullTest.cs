// 
// Unit tests for CloneMethodShouldNotReturnNullRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//
// Copyright (c) <2007> Nidhi Rawal
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

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.BadPractice
{
	[TestFixture]
	public class CloneMethodShouldNotReturnNullTest {
		
		public class CloneMethodReturningNull: ICloneable
		{
			public object Clone ()
			{
				return null;
			}
		}
		
		public class CloneMethodNotReturningNull: ICloneable
		{
			public object Clone ()
			{
				return this.MemberwiseClone ();
			}
		}
		
		public class NotUsingICloneableClone
		{
			public object Clone ()
			{
				return null;
			}
		}
		
		public class CloneWithDifferentArgsReturningNull: ICloneable
		{
			public virtual object Clone ()
			{
				return this.MemberwiseClone ();
			}
			
			public object Clone (int j)
			{
				return null;
			}
		}
		
		public class CloneReturningNullInSomeConditions: ICloneable
		{
			public bool test (int j)
			{
				if (j > 10)
					return true;
				else
					return false;
			}
			
			public object Clone()
			{
				if (test (11))
					return MemberwiseClone();
				else
					return null;
			}
		}
			
	 	private ITypeRule typeRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		MessageCollection messageCollection;
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			typeRule = new CloneMethodShouldNotReturnNullRule ();
			messageCollection = null;
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.BadPractice.CloneMethodShouldNotReturnNullTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
			
		[Test]
		public void cloneMethodReturningNullTest ()
		{
			type = GetTest ("CloneMethodReturningNull");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void cloneMethodNotReturningNullTest ()
		{
			type = GetTest ("CloneMethodNotReturningNull");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void notUsingICloneableCloneTest ()
		{
			type = GetTest ("NotUsingICloneableClone");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void cloneWithDifferentArgsReturningNullTest ()
		{
			type = GetTest ("CloneWithDifferentArgsReturningNull");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void cloneReturningNullInSomeConditionsTest ()
		{
			type = GetTest ("CloneReturningNullInSomeConditions");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
	}
}