//
// Unit Test for UseCorrectSuffix Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//      Abramov Daniel <ex@vingrad.ru>
//
//  (C) 2007 Néstor Salceda
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

	public class CorrectAttribute : Attribute {
	}

	public class IncorrectAttr : Attribute {
	}

	public class OtherAttribute : CorrectAttribute {
	}
	
	public class OtherAttr : CorrectAttribute {
	}
	
	public class CorrectContextStaticAttribute : ContextStaticAttribute {
	}
	
	public class OtherClass {
	}
	
	public class YetAnotherClass : System.Random {
	}
	
	public class InterfaceImplementer : System.Collections.ICollection {
		
		public int Count {
			get {
				throw new NotImplementedException();
			}
		}

		public bool IsSynchronized {
			get {
				throw new NotImplementedException();
			}
		}

		public object SyncRoot {
			get {
				throw new NotImplementedException();
			}
		}

		
		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException();
		}

	}
	
	public class CorrectICollectionCollection : InterfaceImplementer {
	}
	
	public class IncorrectICollectionCol : InterfaceImplementer {
	}
	
	public class MultipleInterfaceImplementer : IEnumerable, System.Security.IPermission {		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void FromXml (System.Security.SecurityElement e)
		{
			throw new NotImplementedException();
		}

		public System.Security.SecurityElement ToXml ()
		{
		throw new NotImplementedException();
		}

		public System.Security.IPermission Copy ()
		{
			throw new NotImplementedException();
		}

		public void Demand ()
		{
			throw new NotImplementedException();
		}

		public System.Security.IPermission Intersect (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}

		public System.Security.IPermission Union (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}
	}
	
	public class CorrectMultipleInterfaceImplementerPermission : MultipleInterfaceImplementer {
	}

	public class CorrectMultipleInterfaceImplementerCollection : MultipleInterfaceImplementer {
	}
	
	public class IncorrectMultipleInterfaceImplementer : MultipleInterfaceImplementer {
	}
       
	public class DerivingClassImplementingInterfaces : System.EventArgs, IEnumerable, System.Security.IPermission {		 
		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void FromXml (System.Security.SecurityElement e)
		{
			throw new NotImplementedException();
		}

		public System.Security.SecurityElement ToXml ()
		{
			throw new NotImplementedException();
		}

		public System.Security.IPermission Copy ()
		{
			throw new NotImplementedException();
		}

		public void Demand ()
		{
			throw new NotImplementedException();
		}

		public System.Security.IPermission Intersect (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}

		public System.Security.IPermission Union (System.Security.IPermission target)
		{
			throw new NotImplementedException();
		}
	}
	
	public class CorrectDerivingClassImplementingInterfacesEventArgs : DerivingClassImplementingInterfaces {
	}
	
	public class IncorrectDerivingClassImplementingInterfacesCollection : DerivingClassImplementingInterfaces { 
	}
	
	public class IncorrectDerivingClassImplementingInterfaces : DerivingClassImplementingInterfaces { 
	}	
	
	[TestFixture]
	public class UseCorrectSuffixTest {
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private MessageCollection messageCollection;
	
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UseCorrectSuffixRule ();
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
		
		[Test]
		public void TestOneLevelInheritanceIncorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectAttr"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestOneLevelInheritanceCorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectAttribute"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestVariousLevelInheritanceCorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.OtherAttribute"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestVariousLevelInheritanceIncorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.OtherAttr"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}
		
		[Test]
		public void TestVariousLevelInheritanceExternalTypeUndetermined () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectContextStaticAttribute"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
			//The System.ContextStaticAttribute class inherits from System.Attribute.
			//But we can retrieve that info from a TypeReference, because now 
			//Gendarme doesn't support loading assemblies.
		}
		
		[Test]
		public void TestOneLevelInheritanceExternalTypeNoApplyed () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.OtherClass"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void TestVariousLevelInheritanceExternalTypeNoApplyed () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.YetAnotherClass"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
			// The System.Random class doesn't inherit from any defined classes.
			// But we can retrieve that info from a TypeReference, because now 
			// Gendarme doesn't support loading assemblies.
		}
		
		[Test]
		public void TestInterfaceImplementerCorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectICollectionCollection"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

 		[Test]
		public void TestInterfaceImplementerIncorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectICollectionCol"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}			       
		
		[Test]
		public void TestMultipleInterfaceImplementerCorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectMultipleInterfaceImplementerPermission"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}     

		[Test]
		public void TestMultipleInterfaceImplementerAnotherCorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectMultipleInterfaceImplementerCollection"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}				     
		
       		[Test]
		public void TestMultipleInterfaceImplementerIncorrectName () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectMultipleInterfaceImplementer"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}			       
		
		[Test]
		public void TestDerivingClassImplementingInterfacesCorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectDerivingClassImplementingInterfacesEventArgs"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}      
		
		[Test]
		public void TestDerivingClassImplementingInterfacesIncorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectDerivingClassImplementingInterfaces"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}      
		
		[Test]
		public void TestDerivingClassImplementingInterfacesAnotherIncorrectName ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectDerivingClassImplementingInterfacesCollection"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.AreEqual (1, messageCollection.Count);
			CheckMessageType (messageCollection, MessageType.Error);
		}      
	}
}
