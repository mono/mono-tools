// 
// Unit tests for AvoidNonAlphanumericIdentifierRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

using Gendarme.Rules.Naming;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test_Rules_Naming {

	public class Type_With_Underscore {

		protected int Field_With_Underscore;

		public void Method_With_Underscore (string param_with_underscore)
		{
			Event_With_Underscore += delegate { 
				Console.WriteLine ("hello");
			};
		}

		public event EventHandler<EventArgs> Event_With_Underscore;

		public bool Property_With_Underscore {
			get { return false; }
		}
	}

	public class TypeWithoutUnderscore {

		protected int FieldWithoutUnderscore;
		private int Field_With_Underscore;	// non-visible

		public void MethodWithoutUnderscore (string paramWithoutUnderscore)
		{
			EventWithoutUnderscore += delegate {
				Console.WriteLine ("hello");
			};
		}

		protected event EventHandler<EventArgs> EventWithoutUnderscore;

		public bool PropertyWithoutUnderscore {
			get { return false; }
		}

		internal int Property_With_Underscore {
			get { return 0; }
		}
	}
}

namespace Test.Rules.Naming {

	internal class InternalClassName_WithUnderscore {
	}

	public class PublicClassName_WithUnderscore {
	}

	internal interface InternalInterface_WithUnderscore {
	}

	public interface PublicInterface_WithUnderscore {
	}

	// from Mono.Cecil.Pdb
	[Guid ("809c652e-7396-11d2-9771-00a0c9b4d50c")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible (true)]
	public interface IMetaDataDispenser {
		void DefineScope_Placeholder ();
	}

	public enum PublicEnum_WithUnderscore {
		Value_WithUnderscore,
		ValueWithoutUnderscore
	}

	[TestFixture]
	public class AvoidNonAlphanumericIdentifierAssemblyTest : AssemblyRuleTestFixture<AvoidNonAlphanumericIdentifierRule> {

		AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		[Test]
		public void AssemblyName ()
		{
			string name = assembly.Name.Name;
			try {
				assembly.Name.Name = "My_Assembly";
				// bad name and bad namespace
				AssertRuleFailure (assembly, 2);
			}
			finally {
				assembly.Name.Name = name;
			}
		}

		[Test]
		public void Namespace ()
		{
			// Type_With_Underscore
			AssertRuleFailure (assembly, 1);
		}
	}

	[TestFixture]
	public class AvoidNonAlphanumericIdentifierTypeTest : TypeRuleTestFixture<AvoidNonAlphanumericIdentifierRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
			// because they are NOT visible
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void Enum ()
		{
			// the enum itself and one of it's two values
			AssertRuleFailure<PublicEnum_WithUnderscore> (2);
		}

		[Test]
		public void Types ()
		{
			// namespace is not consider at type level, but the field is catched here
			AssertRuleFailure<Test_Rules_Naming.Type_With_Underscore> (2);
			AssertRuleSuccess<Test_Rules_Naming.TypeWithoutUnderscore> ();
		}

		[Test]
		public void InternalClassWithUnderscoreTest ()
		{
			AssertRuleDoesNotApply<InternalClassName_WithUnderscore> ();
		}

		[Test]
		public void PublicClassWithUnderscoreTest ()
		{
			AssertRuleFailure<PublicClassName_WithUnderscore> (1);
		}

		[Test]
		public void InternalInterfaceWithUnderscoreTest ()
		{
			AssertRuleDoesNotApply<InternalInterface_WithUnderscore> ();
		}

		[Test]
		public void PublicInterfaceWithUnderscoreTest ()
		{
			AssertRuleFailure<PublicInterface_WithUnderscore> (1);
		}

		[Test]
		public void ComInterop ()
		{
			AssertRuleDoesNotApply<IMetaDataDispenser> ();
		}
	}

	[TestFixture]
	public class AvoidNonAlphanumericIdentifierMethodTest : MethodRuleTestFixture<AvoidNonAlphanumericIdentifierRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		[Test]
		public void Enum ()
		{
			AssertRuleFailure<PublicEnum_WithUnderscore> (1);
		}

		[Test]
		public void MembersWithoutUnderscoreTest ()
		{
			AssertRuleSuccess<Test_Rules_Naming.TypeWithoutUnderscore> ("MethodWithoutUnderscore");
			AssertRuleSuccess<Test_Rules_Naming.TypeWithoutUnderscore> ("add_EventWithoutUnderscore");
			AssertRuleSuccess<Test_Rules_Naming.TypeWithoutUnderscore> ("remove_EventWithoutUnderscore");
			AssertRuleSuccess<Test_Rules_Naming.TypeWithoutUnderscore> ("get_PropertyWithoutUnderscore");
		}

		[Test]
		public void MembersWithUnderscoreTest ()
		{
			// method and its parameter name contains an underscore
			AssertRuleFailure<Test_Rules_Naming.Type_With_Underscore> ("Method_With_Underscore", 2);
			AssertRuleFailure<Test_Rules_Naming.Type_With_Underscore> ("add_Event_With_Underscore", 1);
			AssertRuleFailure<Test_Rules_Naming.Type_With_Underscore> ("remove_Event_With_Underscore", 1);
			AssertRuleFailure<Test_Rules_Naming.Type_With_Underscore> ("get_Property_With_Underscore", 1);
		}

		public class ClassWithAnonymousDelegate {

			public bool MethodDefiningDelegate ()
			{
				byte [] array = null;
				return !Array.Exists (array, delegate (byte value) { return value.Equals (this); });
			}
		}

		[Test]
		public void AnonymousDelegate ()
		{
			AssertRuleSuccess<ClassWithAnonymousDelegate> ("MethodDefiningDelegate");
		}

		[Test]
		public void ComInterop ()
		{
			AssertRuleDoesNotApply<IMetaDataDispenser> ("DefineScope_Placeholder");
		}
	}
}
