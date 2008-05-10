//
// Unit tests for PInvokeShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Interoperability {

	internal class PInvokeInternal {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeepInternal (UInt32 beepType); //ok
		
		[DllImport ("User32.dll")]
		public static extern Boolean MessageBeepPublic (UInt32 beepType); //ok
		
		public class Public {
			[DllImport ("User32.dll")]
			public static extern Boolean MessageBeepPublic (UInt32 beepType); //ok
		}
	}
	
	public class PInvokePublic {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeepInternal (UInt32 beepType); //ok
		
		[DllImport ("User32.dll")]
		public static extern Boolean MessageBeepPublic (UInt32 beepType); //warn
		
		public class Public {
			[DllImport ("User32.dll")]
			public static extern Boolean MessageBeepPublic (UInt32 beepType); //warn
		}
		
		internal class Internal {
			[DllImport ("User32.dll")]
			public static extern Boolean MessageBeepPublic (UInt32 beepType); //ok
		}
	}
	
	[TestFixture]
	public class PInvokeShouldNotBeVisibleRuleTest {

		private PInvokeShouldNotBeVisibleRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition internalType;
		private TypeDefinition publicType;
		private TypeDefinition internalType_PublicNested;
		private TypeDefinition publicType_PublicNested;
		private TypeDefinition publicType_InternalNested;
		

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			internalType = assembly.MainModule.Types ["Test.Rules.Interoperability.PInvokeInternal"];
			publicType = assembly.MainModule.Types ["Test.Rules.Interoperability.PInvokePublic"];
			internalType_PublicNested = assembly.MainModule.Types ["Test.Rules.Interoperability.PInvokeInternal"].NestedTypes [0];
			publicType_PublicNested = assembly.MainModule.Types ["Test.Rules.Interoperability.PInvokePublic"].NestedTypes [0];
			publicType_InternalNested = assembly.MainModule.Types ["Test.Rules.Interoperability.PInvokePublic"].NestedTypes [1];
			rule = new PInvokeShouldNotBeVisibleRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (TypeDefinition type, string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		[Test]
		public void TestInternal_Internal ()
		{
			MethodDefinition method = GetTest (internalType, "MessageBeepInternal");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestInternal_Public ()
		{
			MethodDefinition method = GetTest (internalType, "MessageBeepPublic");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}		
		
		[Test]
		public void TestInternal_PublicNested()
		{
			MethodDefinition method = GetTest (internalType_PublicNested, "MessageBeepPublic");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
		
		[Test]
		public void TestPublic_Internal ()
		{
			MethodDefinition method = GetTest (publicType, "MessageBeepInternal");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestPublic_Public ()
		{
			MethodDefinition method = GetTest (publicType, "MessageBeepPublic");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}		
		
		[Test]
		public void TestPublic_PublicNested ()
		{
			MethodDefinition method = GetTest (publicType_PublicNested, "MessageBeepPublic");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}
		
		[Test]
		public void TestPublic_InternalNested ()
		{
			MethodDefinition method = GetTest (publicType_InternalNested, "MessageBeepPublic");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
	}
}
