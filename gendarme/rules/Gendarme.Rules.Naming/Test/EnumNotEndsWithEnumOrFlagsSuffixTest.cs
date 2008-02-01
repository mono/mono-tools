//
// Unit Test for EnumNotEndsWithEnumOrFlagsSuffix Rule.
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using System.Collections;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Naming;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Naming {
	public enum ReturnValue {
		Foo,
		Bar
	}		

	public enum ReturnValueEnum {
		Foo,
		Bar
	}

	[Flags]
	public enum ReturnValues {
		Foo,
		Bar
	}

	[Flags]
	public enum ReturnValuesFlags {
		Foo,
		Bar
	}

	public enum returnvalueenum {
		Foo,
		Bar
	}

	[Flags]
	public enum returnvaluesflags {
		Foo,
		Bar
	} 
	
	[TestFixture]
	public class EnumNotEndsWithEnumOrFlagsSuffixTest {
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private MessageCollection messageCollection;
	
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new EnumNotEndsWithEnumOrFlagsSuffixRule ();
			messageCollection = null;
		}
		
		private void CheckMessageType (MessageCollection messageCollection, MessageType messageType) 
		{
			IEnumerator enumerator = messageCollection.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				Message message = (Message) enumerator.Current;
				Assert.AreEqual (message.Type, messageType);
			}
		}
		
		[Test]
		public void TestCorrectEnumName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ReturnValue"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestIncorrectEnumName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ReturnValueEnum"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestCorrectFlagsName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ReturnValues"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestIncorrectFlagsName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ReturnValuesFlags"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestIncorrectEnumNameInLower () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.returnvalueenum"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestIncorrectFlagsNameInLower () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.returnvaluesflags"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
			CheckMessageType (messageCollection, MessageType.Error);
		}
	}
}