//
// Unit test for UseValueInPropertySetterRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Reflection;
using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

using Test.Rules.Helpers;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class UseValueInPropertySetterTest {

		public class Item {

			public bool UsesValue {
				set { bool val = value; }
			}

			public bool DoesNotUseValue {
				set { bool val = true; }
			}

			public void set_NotAProperty(bool value)
			{
				bool val = true;
			}

			public int Throw1 {
				set { throw new NotSupportedException (); }
			}

			public int Throw2 {
				set { throw new NotSupportedException ("value isn't used here"); }
			}

			private string Translate (string s)
			{
				return "value n'est pas utilise ici";
			}

			public int Throw3 {
				set { throw new NotSupportedException (Translate ("value isn't used here")); }
			}

			private static int maxFields = 25;
			public static int MaxFields {
				get { return maxFields; }
				set { maxFields = value; }
			}
			public static int MinFields {
				get { return 0; }
				set { maxFields = 0; }
			}

			public long time;
			public DateTime Time {
				set { time = value.Ticks; }
			}

			public string Empty {
				set { }
			}
		}

		public class BitVector32 {
			int bits;

			public struct Section {
				private short mask;
				private short offset;
				
				internal Section (short mask, short offset)
				{
					this.mask = mask;
					this.offset = offset;
				}
				
				public short Mask {
					get { return mask; }
				}
				
				public short Offset {
					get { return offset; }
				}
			}

			public int this [BitVector32.Section section] {
				get {
					return ((bits >> section.Offset) & section.Mask);
				}
				set {
					if (value < 0)
						throw new ArgumentException ();
					if (value > section.Mask)
						throw new ArgumentException ();
					bits &= ~(section.Mask << section.Offset);
					bits |= (value << section.Offset);
				}
			}
		}


		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			string unit = Assembly.GetExecutingAssembly().Location;
			assembly = AssemblyFactory.GetAssembly(unit);
			module = assembly.MainModule;
			type = module.Types["Test.Rules.Correctness.UseValueInPropertySetterTest/Item"];
			rule = new UseValueInPropertySetterRule ();
			runner = new TestRunner (rule);
		}

		MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		[Test]
		public void TestUsesValue()
		{
			MethodDefinition method = GetTest ("set_UsesValue");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void TestDoesNotUseValue()
		{
			MethodDefinition method = GetTest("set_DoesNotUseValue");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNotAProperty()
		{
			MethodDefinition method = GetTest("set_NotAProperty");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ThrowException ()
		{
			MethodDefinition method = GetTest ("set_Throw1");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");
			method = GetTest ("set_Throw2");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
			method = GetTest ("set_Throw3");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult3");
			Assert.AreEqual (0, runner.Defects.Count, "Count3");
		}

		[Test]
		public void TestStaticUsesValue ()
		{
			MethodDefinition method = GetTest ("set_MaxFields");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestStaticDoesNotUseValue ()
		{
			MethodDefinition method = GetTest ("set_MinFields");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestDateValue ()
		{
			MethodDefinition method = GetTest ("set_Time");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestEmpty ()
		{
			MethodDefinition method = GetTest ("set_Empty");
			// too many false positive, it seems too common to have empty set to report them
			// at least for this specific rule
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestThisProperty ()
		{
			TypeDefinition type = module.Types ["Test.Rules.Correctness.UseValueInPropertySetterTest/BitVector32"];
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "set_Item") {
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (0, runner.Defects.Count, "Count");
				}
			}
		}
	}
}
