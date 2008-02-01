//
// Unit Test for UsePreferredTermsRule
//
// Authors:
//      Abramov Daniel <ex@vingrad.ru>
//
//  (C) 2007 Abramov Daniel
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
	
	public class SomeComPlusStuff { // one obsolete term ('ComPlus')
	}
	
	public class SomeComPlusAndIndicesStuff { // two obsolete terms ('ComPlus' and 'Indices')
	}
	
	public class TermsMethodsAndProperties {

		public void SignOn () { } // incorrect
		public void SignIn () { } // correct
		
		public bool Writeable { get { return true; } } // incorrect
		public bool Writable { get { return true; } } // correct
	}
	
	[TestFixture]
	public class UsePreferredTermsTest {
		private UsePreferredTermsRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private MessageCollection messageCollection;
	
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UsePreferredTermsRule ();
			messageCollection = null;
		}
		
		private void CheckMessageType (MessageCollection messageCollection, MessageType messageType) 
		{
			IEnumerator enumerator = messageCollection.GetEnumerator ();
			if (enumerator.MoveNext ()) {
				Message message = (Message) enumerator.Current;
				Assert.AreEqual (messageType, message.Type);
			}
		}
		
      		private MethodDefinition GetPropertyGetter (string name)
		{
			string get_name = "get_" + name;
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == get_name)
					return method;
			}
			return null;
		}
				
      		private MethodDefinition GetMethod (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}
		
		[Test]
		public void TestOneObsoleteTerm ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.SomeComPlusStuff"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestTwoObsoleteTerms ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.SomeComPlusAndIndicesStuff"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (2, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestCorrectMethodsAndProperties ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.TermsMethodsAndProperties"];
			messageCollection = rule.CheckMethod (GetMethod ("SignIn"), new MinimalRunner ());
			Assert.IsNull (messageCollection);
			messageCollection = rule.CheckMethod (GetPropertyGetter ("Writable"), new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestIncorrectMethodsAndProperties ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.TermsMethodsAndProperties"];
			messageCollection = rule.CheckMethod (GetMethod ("SignOn"), new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
			messageCollection = rule.CheckMethod (GetPropertyGetter ("Writeable"), new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}		
	}
}
