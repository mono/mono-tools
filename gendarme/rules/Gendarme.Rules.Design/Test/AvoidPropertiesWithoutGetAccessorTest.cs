// 
// Unit tests for AvoidPropertiesWithoutGetAccessorRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;

namespace Test.Rules.Design {

	public abstract class PublicAbstract {
		public abstract int Value { get; set; }
	}

	public abstract class PublicAbstractGetOnly {
		public abstract int Value { get; }
	}

	public abstract class PublicAbstractSetOnly {
		public abstract int Value { set; }
	}

	public interface IPublic {
		int Value { get; set; }
	}

	public interface IPublicGetOnly {
		int Value { get; }
	}

	public interface IPublicSetOnly {
		int Value { set; }
	}

	public class PublicClassInterface : IPublic {
		public int Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicClassExplicitInterface : IPublic {
		int IPublic.Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicClass {
		public int Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicSetOnlyInheritClass : PublicAbstractSetOnly {
		public override int Value {
			set { throw new NotImplementedException (); }
		}
	}

	public class PublicSetOnlyImplementClass : IPublicSetOnly {
		public int Value {
			set { throw new NotImplementedException (); }
		}
	}

#if false
	// this cannot be compiled with CSC - error CS0082
	public class PublicGetIsNotAGetterClass : IPublicSetOnly {

		// try to confuse the rule
		public int get_Value ()
		{
			return 42;
		}

		public int Value {
			set { throw new NotImplementedException (); }
		}
	}
#endif
	public class PublicSetIsNotASetterClass {

		public void set_Value ()
		{
		}
	}

	[TestFixture]
	public class AvoidPropertiesWithoutGetAccessorTest {


		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidPropertiesWithoutGetAccessorRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetType (string name)
		{
			string fullname = "Test.Rules.Design." + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void WithNoProperties ()
		{
			TypeDefinition type = GetType ("AvoidPropertiesWithoutGetAccessorTest");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.Name);
			}
		}

		[Test]
		public void WithBothGetAndSet ()
		{
			TypeDefinition type = GetType ("PublicAbstract");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("IPublic");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicClass");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicClassExplicitInterface");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicClassInterface");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}
		}

		[Test]
		public void WithOnlyGet ()
		{
			TypeDefinition type = GetType ("PublicAbstractGetOnly");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("IPublicGetOnly");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}
		}

		[Test]
		public void WithOnlySet ()
		{
			TypeDefinition type = GetType ("PublicAbstractSetOnly");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNotNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("IPublicSetOnly");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNotNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicSetOnlyInheritClass");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNotNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicSetOnlyImplementClass");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNotNull (rule.CheckMethod (method, runner), method.ToString ());
			}

			type = GetType ("PublicSetIsNotASetterClass");
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, runner), method.ToString ());
			}
		}
	}
}
