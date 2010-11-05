//
// Unit tests for PInvokeShouldNotBeVisibleRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Andreas Noever
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Gendarme.Rules.Interoperability;

using NUnit.Framework;
using Test.Rules.Fixtures;
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

		// special cases - we'll be modified (static removed) before their use

		[DllImport ("User32.dll")]
		internal static extern Boolean InstanceInternal (UInt32 beepType); //bad1
		
		[DllImport ("User32.dll")]
		public static extern Boolean InstancePublic (UInt32 beepType); //bad2
	}
	
	[TestFixture]
	public class PInvokeShouldNotBeVisibleTest : MethodRuleTestFixture<PInvokeShouldNotBeVisibleRule> {

		[TestFixtureSetUp]
		public void SetUp ()
		{
			// since C# compiler won't compile an instance p/invoke we'll need to hack our own
			AssemblyDefinition assembly = DefinitionLoader.GetAssemblyDefinition<PInvokePublic> ();
			foreach (MethodDefinition method in assembly.MainModule.GetType ("Test.Rules.Interoperability.PInvokePublic").Methods) {
				if (method.Name.StartsWith ("Instance"))
					method.IsStatic = false;
			}
		}

		[Test]
		public void NonVisibleType_NonVisibleMethod ()
		{
			AssertRuleSuccess<PInvokeInternal> ("MessageBeepInternal");
		}

		[Test]
		public void NonVisibleType_VisibleMethod ()
		{
			AssertRuleSuccess<PInvokeInternal> ("MessageBeepPublic");
		}		
		
		[Test]
		public void NonVisibleNestedType_VisibleMethod ()
		{
			AssertRuleSuccess<PInvokeInternal.Public> ("MessageBeepPublic");
			AssertRuleSuccess<PInvokePublic.Internal> ("MessageBeepPublic");
		}
		
		[Test]
		public void VisibleType_NonVisibleMethod ()
		{
			AssertRuleSuccess<PInvokePublic> ("MessageBeepInternal");
		}

		[Test]
		public void VisibleType_VisibleMethod ()
		{
			AssertRuleFailure<PInvokePublic> ("MessageBeepPublic", 1);
		}		
		
		[Test]
		public void VisibleNestedType_VisibleMethod ()
		{
			AssertRuleFailure<PInvokePublic.Public> ("MessageBeepPublic", 1);
		}

		[Test]
		public void Instance ()
		{
			AssertRuleFailure<PInvokePublic> ("InstanceInternal", 1);
			AssertRuleFailure<PInvokePublic> ("InstancePublic", 2);
		}
	}
}
