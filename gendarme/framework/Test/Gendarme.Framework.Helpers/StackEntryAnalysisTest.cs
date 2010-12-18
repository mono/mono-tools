//
// Unit tests for StackEntryAnalysis
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;

using NUnit.Framework;

namespace Test.Framework {

	[TestFixture]
	public class StackEntryAnalysisTest {

		private AssemblyDefinition assembly;
		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Framework.StackEntryAnalysisTest");
		}

		public MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		public Instruction GetFirstNewObj (MethodDefinition method)
		{
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code == Code.Newobj)
					return ins;
			}
			return null;
		}

		public object SimpleReturn ()
		{
			return new object ();
		}

		[Test]
		public void TestSimpleReturn ()
		{
			MethodDefinition m = GetTest ("SimpleReturn");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object SimpleLoc ()
		{
			object a = new object ();
			return a;
		}

		[Test]
		public void TestSimpleLoc ()
		{
			MethodDefinition m = GetTest ("SimpleLoc");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object Loc ()
		{
			object a = new object ();
			object b = a;
			return b;
		}

		[Test]
		public void TestLoc ()
		{
			MethodDefinition m = GetTest ("Loc");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object Loc2 ()
		{
			int a = 0;
			int b = 1;
			int c = 2;
			int d = 3;
			object e = new object ();
			switch (new Random ().Next ()) {
			case 0:
				return a;
			case 1:
				return b;
			case 2:
				return c;
			case 3:
				return d;
			default:
				return e;
			}
		}

		[Test]
		public void TestLoc2 ()
		{
			MethodDefinition m = GetTest ("Loc2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object Pop ()
		{
			new object ();
			return null;
		}

		[Test]
		public void TestPop ()
		{
			MethodDefinition m = GetTest ("Pop");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (0, result.Length);
		}

		public object Branch ()
		{
			object a = new object ();
			object b;
			if (new Random ().Next () == 0)
				return a;
			else
				b = a;
			return b.ToString ();
		}

		[Test]
		public void TestBranch ()
		{
			MethodDefinition m = GetTest ("Branch");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result[0]-Opcode-Ret");
			Assert.AreEqual (OpCodes.Callvirt, result [1].Instruction.OpCode, "result[1]-Opcode-Callvirt");
		}

		public void Branch2 ()
		{
			Exception a = new Exception ();
			bool cond = new Random ().Next () == 0;
			a.Source = cond ? "a" : "b"; //tricks the compiler to load a onto the stack before doing a branch (to get more coverage)
		}

		[Test]
		public void TestBranch2 ()
		{
			MethodDefinition m = GetTest ("Branch2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result-Opcode-Callvirt");
		}

		public object TryFinally ()
		{
			object a = new object ();
			object b = null;
			object c;
			try {
				b = a;
			}
			finally {
				c = b;
				b = null;
			}
			if (new Random ().Next () == 0)
				return c.ToString ();
			else
				return b;
		}

		[Test]
		public void TestTryFinally ()
		{
			MethodDefinition m = GetTest ("TryFinally");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result-Opcode-Callvirt");
		}

		public object NestedTryFinally ()
		{
			object a = new object ();
			object b = null;
			object c = null;
			try {
				b = a;
				try {
					c = new object ();
				}
				finally {
					c = a;
					a = null;
				}
			}
			finally {
				b = null;
			}
			if (new Random ().Next () == 0)
				return a;
			else if (new Random ().Next () == 0)
				return b;
			else
				return c.ToString ();
		}

		[Test]
		public void TestNestedTryFinally ()
		{
			MethodDefinition m = GetTest ("NestedTryFinally");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result-Opcode-Callvirt");
		}

		public object NestedTryFinally2 ()
		{
			object a = new object ();
			object b = null;
			object c = null;
			try {
				b = a;
			}
			finally {
				try {
					c = new object ();
				}
				finally {
					c = a;
					a = null;
				}
				b = null;
			}
			if (new Random ().Next () == 0)
				return a;
			else if (new Random ().Next () == 0)
				return b;
			else
				return c.ToString ();
		}

		[Test]
		public void TestNestedTryFinally2 ()
		{
			MethodDefinition m = GetTest ("NestedTryFinally2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result-Opcode-Callvirt");
		}

		public object TryCatch ()
		{
			object a = new object ();
			object b = null;
			object c = null;
			try {
				if (new Random ().Next () == 0) {
					a = null;
					return a;
				}
			}
			catch {
				b = a;
				if (new Random ().Next () == 0)
					return a.ToString ();
			}
			return b.GetHashCode ();
		}

		[Test]
		public void TestTryCatch ()
		{
			MethodDefinition m = GetTest ("TryCatch");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2"); //no "return a";
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result[0]-Opcode-Callvirt"); //return a.ToString ();
			Assert.AreEqual (OpCodes.Callvirt, result [1].Instruction.OpCode, "result[1]-Opcode-Callvirt"); //return b.GetHashCode ();
		}

		public object TryCatchFinally ()
		{
			object a = new object ();
			object b = null;
			object c = null;
			try {
				if (new Random ().Next () == 0) {
					a = null;
					return a;
				}
			}
			catch {
				b = a;
				if (new Random ().Next () == 0)
					return a.ToString ();
				try {
					a = null;
				}
				finally {
					c = b;
					b = a;
				}
			}
			finally {
			}
			if (new Random ().Next () == 0)
				return b;
			else
				return c.GetHashCode ();
		}

		[Test]
		public void TestTryCatchFinally ()
		{
			MethodDefinition m = GetTest ("TryCatchFinally");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2"); //no "return a";
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result[0]-Opcode-Callvirt"); //return a.ToString ();
			Assert.AreEqual (OpCodes.Callvirt, result [1].Instruction.OpCode, "result[1]-Opcode-Callvirt"); //return c.GetHashCode ();
		}

		public object MultipleCatch ()
		{
			object a = new object ();
			object b = null;
			object c = null;
			try {
				if (new Random ().Next () == 0) {
					a = null;
					return a;
				}
			}
			catch (InvalidOperationException) {
				b = a;
			}
			catch (InvalidCastException) {
				c = a;
			}
			finally {
				a = null;
			}
			if (new Random ().Next () == 0)
				return a;
			else if (new Random ().Next () == 0)
				return b.GetHashCode ();
			else
				return c.ToString ();
		}

		[Test]
		public void TestMultipleCatch ()
		{
			MethodDefinition m = GetTest ("MultipleCatch");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2"); //no "return a";
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result[0]-Opcode-Callvirt"); //return b.ToString ();
			Assert.AreEqual (OpCodes.Callvirt, result [1].Instruction.OpCode, "result[1]-Opcode-Callvirt"); //return c.GetHashCode ();
		}

		public object Starg (object b)
		{
			object a = new object ();
			b = a;
			return b;
		}

		[Test]
		public void TestStarg ()
		{
			MethodDefinition m = GetTest ("Starg");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object Starg2 (object a, object b, object c, object d) //force ldarg.s (no macro)
		{
			d = new object ();
			return d;
		}

		[Test]
		public void TestStarg2 ()
		{
			MethodDefinition m = GetTest ("Starg2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public static object StargStatic (object a, object b, object c, object d, object e) //force ldarg.s (no macro) (static (no this))
		{
			e = new object ();
			return e;
		}

		[Test]
		public void TestStargStatic ()
		{
			MethodDefinition m = GetTest ("StargStatic");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result-Opcode-Ret");
		}

		public object OutArg (out object b)
		{
			object a = new object ();
			b = a;
			return b; //this is not guaranteed to work in complex situations. The lookup for stind_ref simply checks all previous instructions for ldargs.
		}

		[Test]
		public void TestOutArg ()
		{
			MethodDefinition m = GetTest ("OutArg");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (OpCodes.Stind_Ref, result [0].Instruction.OpCode, "result[0]-Opcode-Stind_Ref");
			Assert.AreEqual (OpCodes.Ret, result [1].Instruction.OpCode, "result[1]-Opcode-Ret");
		}

		public object OutArg2 (object a, object b, object c, out object d) //force non macro version
		{
			d = new object ();
			return d;
		}

		[Test]
		public void TestOutArg2 ()
		{
			MethodDefinition m = GetTest ("OutArg2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (OpCodes.Stind_Ref, result [0].Instruction.OpCode, "result[0]-Opcode-Stind_Ref");
			Assert.AreEqual (OpCodes.Ret, result [1].Instruction.OpCode, "result[1]-Opcode-Ret");
		}

		public object Switch ()
		{
			object a = new object ();
			object b = null;

			switch (new Random ().Next ()) {
			case 0:
				return a.ToString ();
			case 1:
				b = a;
				goto case 3;
			case 2:
				a = null;
				return a;
			case 3:
				return b.ToString ();

			}
			return null;
		}

		[Test]
		public void TestSwitch ()
		{
			MethodDefinition m = GetTest ("Switch");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (OpCodes.Callvirt, result [0].Instruction.OpCode, "result[0]-Opcode-Callvirt");
			Assert.AreEqual (OpCodes.Callvirt, result [1].Instruction.OpCode, "result[1]-Opcode-Callvirt");
		}

		[Test]
		public void TestSwitch2 ()
		{
			//I could not find c# code that loads a reference onto the stack before executing switch
			//newobj System.Object.ctor <- reference is on the stack
			//ldc.i4.0
			//switch +2,+3
			//ret <- default
			//ret <- switch1
			//ret <- switch2

			MethodDefinition m = new MethodDefinition ("Switch2", Mono.Cecil.MethodAttributes.Public, this.type);

			ILProcessor il = m.Body.GetILProcessor ();

			Instruction switch1 = il.Create (OpCodes.Ret);
			Instruction switch2 = il.Create (OpCodes.Ret);

			il.Emit (OpCodes.Newobj, (MethodReference) GetFirstNewObj (GetTest ("Switch")).Operand); //get object.ctor()
			il.Emit (OpCodes.Ldc_I4_0);
			il.Emit (OpCodes.Switch, new Instruction [] { switch1, switch2 });

			//default	
			il.Emit (OpCodes.Ret);

			//switch 1 and 2
			il.Append (switch1);
			il.Append (switch2);

			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (3, result.Length, "result-Length-3");
			Assert.AreEqual (OpCodes.Ret, result [0].Instruction.OpCode, "result[0]-Opcode-Ret");
			Assert.AreEqual (OpCodes.Ret, result [1].Instruction.OpCode, "result[1]-Opcode-Ret");
			Assert.AreEqual (OpCodes.Ret, result [2].Instruction.OpCode, "result[2]-Opcode-Ret");
		}

		public void Castclass ()
		{
			Exception a = (Exception) new object ();
			throw a;
		}

		[Test]
		public void TestCastclass ()
		{
			MethodDefinition m = GetTest ("Castclass");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (OpCodes.Throw, result [0].Instruction.OpCode, "result-Opcode-Throw");
		}

		public void StackOffset ()
		{
			string a = (string) new object ();
			a.ToString ();
			a.CompareTo (a);
			Console.WriteLine (a);
		}

		[Test]
		public void TestStackOffset ()
		{
			MethodDefinition m = GetTest ("StackOffset");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (4, result.Length, "result-Length-4");
			Assert.AreEqual (0, result [0].StackOffset, "result[0]-StackOffset-0");
			Assert.AreEqual (1, result [1].StackOffset, "result[1]-StackOffset-1");
			Assert.AreEqual (0, result [2].StackOffset, "result[2]-StackOffset-0");
			Assert.AreEqual (0, result [3].StackOffset, "result[3]-StackOffset-0");
		}

		object field;
		public void Field ()
		{
			field = new object ();
			field.ToString ();
		}

		[Test]
		public void TestField ()
		{
			MethodDefinition m = GetTest ("Field");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (Code.Stfld, result [0].Instruction.OpCode.Code, "result[0]-Opcode-Stfld");
			Assert.AreEqual (Code.Callvirt, result [1].Instruction.OpCode.Code, "result[1]-Opcode-Callvirt");
		}

		public void Field2 ()
		{
			var t = new StackEntryAnalysisTest ();
			t.field = new object (); //do not follow assigns to other objects!
			field.ToString ();
			t.field.ToString ();
		}

		[Test]
		[Ignore ("StackEntryAnalysis does currently not check which instance the field belongs to. This will need a reverse StackEntry test and AssemblyResolver support (for protected fields and/or static fields).")]
		public void TestField2 ()
		{
			MethodDefinition m = GetTest ("Field2");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (1, result.Length, "result-Length-1");
			Assert.AreEqual (Code.Stfld, result [0].Instruction.OpCode.Code, "result-Opcode-Stfld");
		}

		static object staticField;
		public void StaticField ()
		{
			staticField = new object ();
			staticField.ToString ();
		}

		[Test]
		public void TestStaticField ()
		{
			MethodDefinition m = GetTest ("StaticField");
			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (GetFirstNewObj (m));

			Assert.AreEqual (2, result.Length, "result-Length-2");
			Assert.AreEqual (Code.Stsfld, result [0].Instruction.OpCode.Code, "result[0]-Opcode-Stfld");
			Assert.AreEqual (Code.Callvirt, result [1].Instruction.OpCode.Code, "result[1]-Opcode-Callvirt");
		}

/*	Cecil doesn't support Calli instructions atm.
		[Test] 
		public void TestCalli ()
		{
			//ldftn Calli
			//calli void ()
			//ret

			MethodDefinition m = new MethodDefinition ("Calli", Mono.Cecil.MethodAttributes.Public, this.type);

			m.Body.CilWorker.Emit (OpCodes.Ldftn, m);
		
			m.Body.CilWorker.Create (OpCodes.Calli, new CallSite (false, false, MethodCallingConvention.Default, GetTest ("TestCalli").ReturnType));
			m.Body.CilWorker.Emit (OpCodes.Ret);

			StackEntryAnalysis sea = new StackEntryAnalysis (m);
			StackEntryUsageResult [] result = sea.GetStackEntryUsage (m.Body.Instructions[0]);

			Assert.AreEqual (1, result.Length);
			Assert.AreEqual (OpCodes.Calli, result [0].Instruction.OpCode);
		}*/
	}
}
