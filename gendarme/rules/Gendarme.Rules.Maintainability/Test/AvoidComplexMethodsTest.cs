//
// Unit Test for AvoidComplexMethods Rule.
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// 	(C) 2008 Cedric Vivier
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Maintainability;
using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Maintainability {

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ExpectedCCAttribute : Attribute
	{
		public ExpectedCCAttribute(int cc)
		{
			_cc = cc;
		}

		public int Value
		{
			get { return _cc; }
		}

		private int _cc = 0;
	}

	public class MethodsWithExpectedCC
	{
		[ExpectedCC (1)]
		[DllImport ("libc.so")]
		private static extern void strncpy (StringBuilder dest, string src, uint n);

		[ExpectedCC(1)]
		public void Test1()
		{
			Console.Write("1");
		}

		[ExpectedCC(1)]
		public void TestLong1(int x, int y)
		{
			Console.Write("{0} {1} {2} {3}", x, x+0, (x+0>y), y);
			Console.Write("{3} {2} {1} {0}", x, x+1, (x+1>y), y);
			Console.Write("{3} {2} {1} {0}", x, x+2, (x+2>y), y);
			Console.Write("{0} {2} {1} {3}", x, x+3, (x+3>y), y);
			Console.Write("{0} {1} {3} {2}", x, x+4, (x+4>y), y);
			Console.Write("{3} {1} {2} {0}", x, x+5, (x+5>y), y);
			Console.Write("{3} {1} {2} {0}", x*x, (x+5)*x, (((x+5)*x)>y), y*y);
		}

		[ExpectedCC(2)]
		public object Test2(object x)
		{
			return x ?? new object()/*2*/;
		}

		[ExpectedCC(3)]
		public int TestTernary3(int x)
		{
			return (x == 0) ? -1/*2*/ : 1/*3*/;
		}

		[ExpectedCC(5)]
		public void TestSwitch5(int x)
		{
			switch (x)
			{
				case 1:
				case 2:
				case 3:
					Console.Write("abc");
					break;
				case 4:
					Console.Write("d");
					break;
				case 5:
					Console.Write("e");
					break;
				default:
					Console.Write("default");
					break;
			}
			Console.Write("oob");
		}

		[ExpectedCC(6)]
		public void TestSwitch6(int x)
		{
			Console.Write("oob");
			switch (x)
			{
				case 1:
					Console.Write("a");
					break;
				case 2:
					Console.Write("b");
					break;
				case 3:
					Console.Write("c");
					break;
				case 4:
					Console.Write("d");
					break;
				case 5:
					Console.Write("e");
					break;
			}
			Console.Write("oob");
		}

		[ExpectedCC(7)]
		public void TestSwitch7(int x)
		{
			switch (x)
			{
				case 1:
					Console.Write("a");
					break;
				case 2:
					Console.Write("b");
					break;
				case 3:
					Console.Write("c");
					break;
				case 4:
					Console.Write("d");
					break;
				case 5:
					Console.Write("e");
					break;
				default:
					Console.Write("default");
					break;
			}
			Console.Write("oob");
		}

		[ExpectedCC(6)]
		public object Test6(object val)
		{
			if (val != null) {/*2*/
				return (val == this) ? 1/*3*/ : 2/*4*/;
			}
			else if (val.GetType() == typeof(string))/*5*/
			{
				string sRef = val.GetType().FullName;
				return sRef ?? "foo"/*6*/;
			}
			throw new InvalidOperationException();
		}

		[ExpectedCC(14)]
		public void Test14(int x)
		{
			switch (x)
			{
				case 1:
					Console.Write("a");
					break;
				case 2:
					Console.Write("b");
					break;
				case 3:
					Console.Write("c");
					break;
				case 4:
					Console.Write("d");
					break;
				case 5:
					Console.Write("e");
					break;
				default:/*7*/
					Console.Write("default");
					break;
			}
			int y = this.GetHashCode();
			if (Math.Min(x, y) > 0/*8*/ && Math.Min(x,y) < 42/*9*/)
			{
				y = (x == 32) ? x/*10*/ : y/*11*/;
			}
			else if (x == 3000/*12*/ || y > 3000/*13*/ || y == 500/*14*/)
			{
				y = 200;
			}
			Console.Write("oob");
		}

		[ExpectedCC(27)]
		public object Test27(object val)
		{
			if (val == null) {/*2*/
				return null;
			}
			else if (val.GetType() == typeof(string))/*3*/
			{
				string sRef = "eateat";
				return sRef;
			} else if (val.GetType () == typeof (int)/*4*/
				|| val.GetType () == typeof (uint)/*5*/
				|| val.GetType () == typeof (float)/*6*/
				|| val.GetType () == typeof (double)/*7*/
				|| val.GetType () == typeof (byte)/*8*/
				|| val.GetType () == typeof (sbyte)/*9*/
				|| val.GetType () == typeof (short)/*10*/
				|| val.GetType () == typeof (ushort)/*11*/
				|| val.GetType () == typeof (long)/*12*/
				|| val.GetType () == typeof (ulong)/*13*/
				|| val.GetType () == typeof (void)/*14*/
				|| val.GetType () == typeof (IntPtr)/*15*/
				|| val.GetType () == typeof (UIntPtr)/*16*/
				|| val.GetType () == typeof (char))/*17*/ {
				return 50;
			}
			else if (val.GetType() == typeof(bool))/*19*/
			{
				return (val.GetType().FullName == "foo") ? true/*19*/ : false/*20*/;
			}
			else if (val.GetType() == typeof(object))/*21*/
			{
				return val;
			}
			int i = val.GetHashCode();
			if (i > 0/*22*/ && i < 42/*23*/)
			{
				return null;
			}
			else if (i == 42/*24*/ || i == 69/*25*/)
			{
				return (i == 42) ? true/*26*/ : false/*27*/;
			}
			throw new InvalidOperationException();
		}

		[ExpectedCC (53)]
		public void TooManyIf (char c)
		{
			if ((c == 'a') || (c == 'b') || (c == 'c') || (c == 'd') || (c == 'e') ||
				(c == 'f') || (c == 'g') || (c == 'h') || (c == 'i') ||	(c == 'j') ||
				(c == 'k') || (c == 'l') || (c == 'm') || (c == 'n') || (c == 'o') ||
				(c == 'p') || (c == 'q') || (c == 'r') || (c == 's') ||	(c == 't') ||
				(c == 'u') || (c == 'v') || (c == 'w') || (c == 'x') ||	(c == 'y') ||
				(c == 'z') || (c == 'A') || (c == 'B') || (c == 'C') ||	(c == 'D') ||
				(c == 'E') || (c == 'F') || (c == 'G') || (c == 'H') || (c == 'I') ||
				(c == 'J') || (c == 'K') || (c == 'L') || (c == 'M') || (c == 'N') ||
				(c == 'O') || (c == 'P') || (c == 'Q') || (c == 'R') ||	(c == 'S') ||
				(c == 'T') || (c == 'U') || (c == 'V') || (c == 'W') ||	(c == 'X') ||
				(c == 'Y') || (c == 'Z'))
					Console.WriteLine ("52!");
		}

		[ExpectedCC(15)]
		[System.Runtime.CompilerServices.CompilerGenerated]
		public object Generated15(object val)
		{
			if (val == null) {/*2*/
				return null;
			} else if (val.GetType () == typeof (string))/*3*/ {
				string sRef = "eateat";
				return sRef;
			} else if (val.GetType () == typeof (int)/*4*/
				  || val.GetType () == typeof (uint)/*5*/
				  || val.GetType () == typeof (float)/*6*/
				  || val.GetType () == typeof (double)/*7*/
				  || val.GetType () == typeof (byte)/*8*/
				  || val.GetType () == typeof (long)/*9*/
				  || val.GetType () == typeof (ulong)/*10*/
				  || val.GetType () == typeof (char))/*11*/ {
				return 50;
			} else if (val.GetType () == typeof (bool))/*12*/ {
				return (val.GetType ().FullName == "foo") ? true/*13*/ : false/*14*/;
			} else if (val.GetType () == typeof (object))/*15*/ {
				return val;
			}
			throw new InvalidOperationException ();
		}

		// adapted from InstructionRocks.GetOperandType
		[ExpectedCC (8)]
		public static int GetOperandType (Instruction self, MethodDefinition method)
		{
			int i = 0;
			switch (self.OpCode.Code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
			case Code.Starg:
			case Code.Starg_S:
				i = 1;
				Console.WriteLine ("arguments");
				break;
			case Code.Conv_R4:
			case Code.Ldc_R4:
			case Code.Ldelem_R4:
			case Code.Ldind_R4:
			case Code.Stelem_R4:
			case Code.Stind_R4:
				i = 2;
				Console.WriteLine ("singles");
				break;
			case Code.Conv_R8:
			case Code.Ldc_R8:
			case Code.Ldelem_R8:
			case Code.Ldind_R8:
			case Code.Stelem_R8:
				i = 3;
				Console.WriteLine ("doubles");
				break;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloca:
			case Code.Ldloca_S:
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc:
			case Code.Stloc_S:
				i = 4;
				Console.WriteLine ("locals");
				break;
			case Code.Ldfld:
			case Code.Ldflda:
			case Code.Ldsfld:
			case Code.Ldsflda:
			case Code.Stfld:
			case Code.Stsfld:
				i = 5;
				Console.WriteLine ("fields");
				break;
			case Code.Call:
			case Code.Callvirt:
			case Code.Newobj:
				i = 6;
				Console.WriteLine ("calls");
				break;
			default:
				i = 7;
				Console.WriteLine ("default");
				break;
			}
			Console.WriteLine ("end");
			return i;
		}
	}


	[TestFixture]
	public class AvoidComplexMethodsTest : MethodRuleTestFixture<AvoidComplexMethodsRule>
	{

		[Test]
		public void CyclomaticComplexityMeasurementTest ()
		{
			TypeDefinition type = DefinitionLoader.GetTypeDefinition<MethodsWithExpectedCC> ();
			int actual_cc;
			int expected_cc;

			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;

				actual_cc = AvoidComplexMethodsRule.GetCyclomaticComplexity (method);
				expected_cc = GetExpectedComplexity (method);
				Assert.AreEqual (expected_cc, actual_cc,
					"CC for method '{0}' is {1} but should have been {2}.",
					method.Name, actual_cc, expected_cc);
			}
		}
		
		private int GetExpectedComplexity(MethodDefinition method)
		{
			foreach (CustomAttribute attr in method.CustomAttributes) {
				if (attr.AttributeType.Name == "ExpectedCCAttribute")
					return (int) attr.ConstructorArguments [0].Value;
			}
			
			throw new ArgumentException (method + " does not have ExpectedCCAttribute");
		}

		[Test]
		public void SimpleMethodsTest ()
		{
			AssertRuleSuccess<MethodsWithExpectedCC> ("Test1");
			AssertRuleSuccess<MethodsWithExpectedCC> ("TestLong1");
			AssertRuleSuccess<MethodsWithExpectedCC> ("Test2");
			AssertRuleSuccess<MethodsWithExpectedCC> ("TestTernary3");
			AssertRuleSuccess<MethodsWithExpectedCC> ("TestSwitch5");
			AssertRuleSuccess<MethodsWithExpectedCC> ("TestSwitch6");
			AssertRuleSuccess<MethodsWithExpectedCC> ("TestSwitch7");
			AssertRuleSuccess<MethodsWithExpectedCC> ("Test6");
			AssertRuleSuccess<MethodsWithExpectedCC> ("Test14");
		}

		[Test]
		public void ComplexMethodTest ()
		{
			AssertRuleFailure<MethodsWithExpectedCC> ("Test27");
		}

		[Test]
		public void MoreComplexMethodTest ()
		{
			AssertRuleFailure<MethodsWithExpectedCC> ("TooManyIf");
		}

		[Test]
		public void MethodDoesNotApplyTest ()
		{
			AssertRuleDoesNotApply<MethodsWithExpectedCC> ("Generated15");
		}

		[Test]
		public void Custom ()
		{
			try {
				Rule.SuccessThreshold = 10;
				Rule.LowThreshold = 15;
				Rule.MediumThreshold = 25;
				AssertRuleFailure<MethodsWithExpectedCC> ("Test27", 1);
				Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Test27-Severity");
				Rule.HighThreshold = 50;
				AssertRuleFailure<MethodsWithExpectedCC> ("TooManyIf", 1);
				Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "TooManyIf-Severity");
			}
			finally {
				Rule.SuccessThreshold = 25;
				Rule.LowThreshold = 50;
				Rule.MediumThreshold = 75;
				Rule.HighThreshold = 100;
			}
		}

		[Test]
		public void HighLevelSwitchBrokenIntoSeveralIlSwitch ()
		{
			AssertRuleSuccess<MethodsWithExpectedCC> ("GetOperandType");
		}
	}
}

