//
// Unit tests for ImplementGenericCollectionInterfacesRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Collections.Generic;
using Mono.Cecil;

using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class ImplementGenericCollectionInterfacesTest : TypeRuleTestFixture<ImplementGenericCollectionInterfacesRule> {
		
		public class PublicIDictionary : IDictionary {
			bool ICollection.IsSynchronized { get { throw new NotImplementedException (); } }
			object ICollection.SyncRoot { get { throw new NotImplementedException (); } }
			bool IDictionary.IsFixedSize { get { throw new NotImplementedException (); } }
			bool IDictionary.IsReadOnly { get { throw new NotImplementedException (); } }
			
			object IDictionary.this [object key] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}

			void IDictionary.Add (object key, object value) { throw new NotImplementedException (); }
			bool IDictionary.Contains (object key) { throw new NotImplementedException (); }
			
			ICollection IDictionary.Keys { get { throw new NotImplementedException (); } }
			ICollection IDictionary.Values { get { throw new NotImplementedException (); } }
			IDictionaryEnumerator IDictionary.GetEnumerator () { throw new NotImplementedException (); }
			
			void IDictionary.Remove (object key) { throw new NotImplementedException (); }
			void ICollection.CopyTo (Array dest, int index) { throw new NotImplementedException (); }
			IEnumerator IEnumerable.GetEnumerator () { throw new NotImplementedException (); }

			public void Clear () { throw new NotImplementedException (); }
			public int Count { get { throw new NotImplementedException (); } }
		
		}
		
		internal class InternalNoUseOfGenerics : IEnumerable {	
			public IEnumerator GetEnumerator () { throw new NotImplementedException (); }			
		}
		
		public class NoUseOfGenerics : IEnumerable {
			public IEnumerator GetEnumerator () { throw new NotImplementedException (); }			
		}
		
		public class GenericsAreUsed : IEnumerable<object> {
			public IEnumerator<object> GetEnumerator () { throw new NotImplementedException (); }
			IEnumerator IEnumerable.GetEnumerator () { throw new NotImplementedException (); }
		}
		
		[Test]
		public void NotApplicable ()
		{
			// the rule does not apply to:
			// * interfaces and enums
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			
			// TODO: add SimpleType for compiler-generated code and corresponding check?
			
			// * classes not implementing IEnumerable
			AssertRuleDoesNotApply<ImplementGenericCollectionInterfacesTest> ();
			// * classes implementing IDictionary
			AssertRuleDoesNotApply<PublicIDictionary> ();
			//  * non-public types
			AssertRuleDoesNotApply<InternalNoUseOfGenerics> ();
		}
		
		[Test]
		public void NotApplicableBefore2_0 ()
		{
			// ensure that the rule does not apply for types defined in 1.x assemblies
			TypeDefinition violator = DefinitionLoader.GetTypeDefinition<NoUseOfGenerics> ();
			TargetRuntime realRuntime = violator.Module.Runtime;
			try {

				// fake assembly runtime version and do the check
				violator.Module.Runtime = TargetRuntime.Net_1_1;
				Rule.Active = true;
				Rule.Initialize (Runner);
				Assert.IsFalse (Rule.Active, "Active");
			}
			catch {
				// rollback
				violator.Module.Runtime = realRuntime;
				Rule.Active = true;
			}
		}
		
		[Test]
		public void GenericInterfaceImplementedReturnsSuccess ()
		{
			AssertRuleSuccess<GenericsAreUsed> ();
		}
		
		[Test]
		public void GenericInterfaceNotImplementedReturnsFailure ()
		{
			AssertRuleFailure<NoUseOfGenerics> ();
		}
	}
}
