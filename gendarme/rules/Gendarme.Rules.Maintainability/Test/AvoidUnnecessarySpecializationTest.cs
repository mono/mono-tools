//
// Unit tests for AvoidUnnecessarySpecializationRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Maintainability {

	public class Base {
		public virtual void Foo()
		{
		}
	}

	public class Derived : Base {
		public override void Foo()
		{
		}

		public void Bar(int x, string y)
		{
		}
	}

	public class DerivedDerived : Derived {
		public DerivedDerived (Base parent)
		{
		}

		public override void Foo()
		{
		}

		public new void Bar(int x, string y)
		{
		}

		public void DerivedDerivedSpecific()
		{
		}
	}


	public class GeneralizedClass {

		public void FooCouldBeBase (Base foo)
		{
			foo.Foo ();
		}

		public void DerivedDerivedSpecificCouldNotBeGeneralized (DerivedDerived specific)
		{
			specific.Bar (999, "bar");
			specific.DerivedDerivedSpecific ();
			specific.Foo ();
		}

		public void DerivedBarCouldNotBeGeneralized (Derived bar)
		{
			bar.Bar (0, string.Empty);
		}

		public int BarAndFooCouldNotBeGeneralized (string foo, Derived bar)
		{
			bar.Bar (foo.Length, foo);
			return 0;
		}

		public void BarAsGeneralizedArgument (string hash, Derived dbar)
		{
			dbar.Foo (); //if bar wasn't used as a Derived argument below it would be eligible
			DerivedBarCouldNotBeGeneralized(dbar);
		}

		public void Interface (IDisposable stream, int number)
		{
			stream.Dispose ();
		}

		public bool CollectionInterface (IEnumerable list, int number)
		{
			foreach (int i in list)
				if (number == i)
					return true;
			return false;
		}

		public bool GenericInterface (IEnumerable<int> list, int number)
		{
			foreach (int i in list)
				if (number == i)
					return true;
			return false;
		}

		public int Property {
			get { return prop; }
			set { prop = value; }
		}
		int prop;

		public void ParameterLessMethod ()
		{
		}

		public void GenericAddBar (Base bar)
		{
			List<Base> l = new List<Base>();
			l.Add(bar);
		}

		public DerivedDerived Constructor (Base bar)
		{
			return new DerivedDerived (bar);
		}

		public Derived derived;
		public void Stfld (Derived bar)
		{
			derived = bar;
		}
		public void Stfld2 (DerivedDerived bar)
		{
			derived = bar;//no warn since we use specific below
			bar.DerivedDerivedSpecific ();
		}

		public static Derived sderived;
		public void Stsfld (Derived bar)
		{
			sderived = bar;
		}
		public void Stsfld2 (DerivedDerived bar)
		{
			sderived = bar;//no warn since we use specific below
			bar.DerivedDerivedSpecific ();
		}

		public void GenericMethod<T> (T x) where T : System.Exception
		{
			Console.WriteLine (x.Message);
		}

		public int GenericMethodArgument (Type type)
		{
			Type [] types = new Type [0];
			return Array.IndexOf<Type> (types, type);
		}

		public FieldInfo [] OverloadNotSupportedByInterface (Type type)
		{
			return type.GetFields ();
		}
	}

	public class SpecializedClass {

		public void FooCouldBeBase (Derived foo)
		{
			foo.Foo ();
		}

		public void DerivedDerivedFooCouldBeBase (int dummy, DerivedDerived foo)
		{
			foo.Foo ();
		}

		public void DerivedDerivedBarCouldBeDerived (DerivedDerived bar)
		{
			bar.Bar (0, null);
		}

		public void BarAndFooCouldBeGeneralized (Derived foo, DerivedDerived bar)
		{
			bar.Bar (42, "hash");
			foo.Foo ();
		}

		public int BarCouldBeGeneralizedButNotFoo (string foo, DerivedDerived bar)
		{
			bar.Bar (42, "hash");
			return foo.Length;
		}

		public int BarAsSpecializedArgument (string hash, DerivedDerived bar)
		{
			FooCouldBeBase(bar);
			return 0;
		}

		//`stream` could be an IDisposable
		public void Interface (System.IO.Stream stream, int number)
		{
			stream.Dispose ();
		}

		//`list` could be a IEnumerable, but we ignore the suggestion
		public bool CollectionInterface (ArrayList list, int number)
		{
			foreach (int i in list)
				if (number == i) return true;
			return false;
		}

		//`list` could be a IEnumerable<int> (or int* ;)
		public bool GenericInterface (List<int> list, int number)
		{
			foreach (int i in list)
				if (number == i) return true;
			return false;
		}

		//`bar` could be Base
		public void GenericAddBar (Derived bar)
		{
			List<Base> l = new List<Base>();
			l.Add(bar);
		}

		//`bar` could be Base
		public DerivedDerived Constructor (Derived bar)
		{
			return new DerivedDerived (bar);
		}

		public Derived derived;
		public void Stfld (DerivedDerived bar)
		{
			derived = bar;
		}

		public static Derived sderived;
		public void Stsfld (DerivedDerived bar)
		{
			sderived = bar;
		}

		public void GenericMethod<T> (T x) where T : System.ArgumentException
		{
			Console.WriteLine (x.Message);
		}

		//`type` could be MemberInfo
		public int GenericMethodArgument (Type type)
		{
			MemberInfo [] types = new MemberInfo [0];
			return Array.IndexOf<MemberInfo> (types, type);
		}

		public FieldInfo [] OverloadNotSupportedByInterface (Type type)
		{
			//IReflect support this GetFields overload
			return type.GetFields (BindingFlags.Public);
		}
	}

	// resolve won't work when running unit test from makefiles
	interface ITestTypeRule {
		RuleResult CheckType (TypeDefinition type);
	}

	interface ITestMethodRule {
		RuleResult CheckMethod (MethodDefinition method);
	}

	class TestRule : Rule, ITestTypeRule, ITestMethodRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			Console.WriteLine (type.Name);
			return RuleResult.DoesNotApply;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			Console.WriteLine (method.Name);
			return RuleResult.Success;
		}
	}

	[TestFixture]
	public class AvoidUnnecessarySpecializationTest : MethodRuleTestFixture<AvoidUnnecessarySpecializationRule> {

		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		[Test]
		public void FooCouldBeBase ()
		{
			AssertRuleFailure<SpecializedClass> ("FooCouldBeBase");
			AssertRuleSuccess<GeneralizedClass> ("FooCouldBeBase");
		}

		[Test]
		public void DerivedDerivedFooCouldBeBase ()
		{
			AssertRuleFailure<SpecializedClass> ("DerivedDerivedFooCouldBeBase");
			AssertRuleSuccess<GeneralizedClass> ("DerivedDerivedSpecificCouldNotBeGeneralized");
		}

		[Test]
		public void DerivedDerivedBarCouldBeDerived ()
		{
			AssertRuleFailure<SpecializedClass> ("DerivedDerivedBarCouldBeDerived");
			AssertRuleSuccess<GeneralizedClass> ("DerivedBarCouldNotBeGeneralized");
		}

		[Test]
		public void BarAndFooCouldBeGeneralized ()
		{
			AssertRuleFailure<SpecializedClass> ("BarAndFooCouldBeGeneralized", 2);
			AssertRuleFailure<SpecializedClass> ("BarCouldBeGeneralizedButNotFoo", 1);
			AssertRuleSuccess<GeneralizedClass> ("BarAndFooCouldNotBeGeneralized");
		}

		[Test]
		public void BarUsedAsArgument ()
		{
			AssertRuleFailure<SpecializedClass> ("BarAsSpecializedArgument");
			AssertRuleSuccess<GeneralizedClass> ("BarAsGeneralizedArgument");
		}

		[Test]
		public void Interface ()
		{
			AssertRuleSuccess<GeneralizedClass> ("Interface");
			AssertRuleFailure<SpecializedClass> ("Interface");
			Assert.IsTrue (Runner.Defects [0].Text.IndexOf ("'System.IDisposable'") > 0);
		}

		[Test]
		public void CollectionInterface ()
		{
			//we ignore non-generic collection interface suggestions since
			//we cannot know if the suggestion would work wrt casts etc...
			AssertRuleSuccess<SpecializedClass> ("CollectionInterface");
			AssertRuleSuccess<GeneralizedClass> ("CollectionInterface");
		}

		[Test]
		public void GenericInterface ()
		{
			AssertRuleSuccess<GeneralizedClass> ("GenericInterface");
			AssertRuleFailure<SpecializedClass> ("GenericInterface");
			Assert.IsTrue (Runner.Defects [0].Text.IndexOf ("'System.Collections.Generic.IEnumerable<T>'") > 0);
		}

		[Test]
		public void GenericAddBar ()
		{
			AssertRuleFailure<SpecializedClass> ("GenericAddBar");
			AssertRuleSuccess<GeneralizedClass> ("GenericAddBar");
		}

		[Test]
		public void Constructor ()
		{
			AssertRuleFailure<SpecializedClass> ("Constructor");
			AssertRuleSuccess<GeneralizedClass> ("Constructor");
		}

		[Test]
		public void Field ()
		{
			AssertRuleFailure<SpecializedClass> ("Stfld");
			AssertRuleSuccess<GeneralizedClass> ("Stfld");
			AssertRuleSuccess<GeneralizedClass> ("Stfld2");
		}

		[Test]
		public void StaticField ()
		{
			AssertRuleFailure<SpecializedClass> ("Stsfld");
			AssertRuleSuccess<GeneralizedClass> ("Stsfld");
			AssertRuleSuccess<GeneralizedClass> ("Stsfld2");
		}

		[Test]
		public void GenericMethod ()
		{
			AssertRuleSuccess<GeneralizedClass> ("GenericMethod");
			AssertRuleFailure<SpecializedClass> ("GenericMethod");
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<GeneralizedClass> ("ParameterLessMethod");
			AssertRuleDoesNotApply<GeneralizedClass> ("get_Property");
			AssertRuleDoesNotApply<GeneralizedClass> ("set_Property");
		}

		[Test]
		public void SatisfyInterface ()
		{
			AssertRuleDoesNotApply<TestRule> ("CheckType");
			AssertRuleDoesNotApply<TestRule> ("CheckMethod");
		}

		// based on System.Boolean Gendarme.Rules.Smells.AvoidLongMethodsRule::IsAutogeneratedByTools(Mono.Cecil.MethodDefinition)
		// original recommandation is IMethodSignature - which does not supply a DeclaringType property
		private static bool IsAutogeneratedByTools (MethodDefinition method)
		{
			if (method.Parameters.Count != 0)
				return false;

			return (method.DeclaringType.Resolve () == null);
		}

		[Test]
		public void FalsePositive ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("IsAutogeneratedByTools");
		}

		private static bool Compare (MethodDefinition method)
		{
			return (method == null);
		}

		private static bool Unused (MethodDefinition method)
		{
			return false;
		}

		[Test]
		public void NoInformation ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("Compare");
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("Unused");
		}

		private bool CheckValueType (byte b)
		{
			return (b == 0);
		}

		private bool CheckArray (object [] array)
		{
			foreach (object o in array) {
				if (o == null)
					return true;
			}
			return false;
		}

		[Test]
		public void Ignored ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("CheckValueType");
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("CheckArray");
		}

		[Test]
		public void GenericParameters ()
		{
			// rule suggest IConvertible which is (one of) the constraint on <T>
			AssertRuleSuccess<Bitmask<Confidence>> ("Get");
			AssertRuleSuccess<Bitmask<Confidence>> ("Set");
		}

		// from AvoidUncalledPrivateCodeRule
		static Dictionary<TypeDefinition, HashSet<uint>> cache;

		private static HashSet<uint> GetCache (TypeDefinition type)
		{
			HashSet<uint> methods;
			if (!cache.TryGetValue (type, out methods)) {
				methods = new HashSet<uint> ();
				cache.Add (type, methods);
				foreach (MethodDefinition md in type.Methods) {
					if (!md.HasBody)
						continue;
				}
			}
			return methods;
		}

		[Test]
		public void CollectionKey ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("GetCache");
		}

		// from gendarme/rules/Gendarme.Rules.Portability/MonoCompatibilityReviewRule.cs
		public event EventHandler<EngineEventArgs> BuildingCustomAttributes;
		private void BuildCustomAttributes (Mono.Cecil.ICustomAttributeProvider custom, EngineEventArgs e)
		{
			if ((BuildingCustomAttributes != null) && (custom.CustomAttributes.Count > 0)) {
				BuildingCustomAttributes (custom, e);
			}
		}

		[Test]
		public void GenericMethodArgument ()
		{
			AssertRuleSuccess<GeneralizedClass> ("GenericMethodArgument");
			AssertRuleFailure<SpecializedClass> ("GenericMethodArgument");
			Assert.IsTrue(Runner.Defects [0].Text.IndexOf ("'System.Reflection.MemberInfo'") > 0);
		}

		private bool HasMoreParametersThanAllowed (IMethodSignature method)
		{
			return (method.HasParameters ? method.Parameters.Count : 0) >= 1;
		}

		// extracted from AvoidLongParameterListsRule where IMethodSignature was suggested
		// but could not be cast (when compiled) into Mono.Cecil.IMetadataTokenProvider
		private void CheckConstructor (IMethodSignature constructor)
		{
			if (HasMoreParametersThanAllowed (constructor))
				Runner.Report (constructor, Severity.Medium, Confidence.Normal, "This constructor contains a long parameter list.");
		}

		[Test]
		public void UncompilableSuggestion ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("CheckConstructor");
		}

		// test case based on false positive found on:
		// System.Void Gendarme.Rules.Concurrency.DoNotLockOnWeakIdentityObjectsRule::Analyze(Mono.Cecil.MethodDefinition,Mono.Cecil.MethodReference,Mono.Cecil.Cil.Instruction)
		class Base {
			public virtual void Show (MethodDefinition method)
			{
				Console.WriteLine (method.Body.CodeSize);
			}
		}

		class Override : Base {
			// without checking the override the rule would suggest:
			// Parameter 'method' could be of type 'Mono.Cecil.MemberReference'.
			// but this would not compile
			public override void Show (MethodDefinition method)
			{
 				 Console.WriteLine (method.Name);
			}
		}

		[Test]
		public void OverrideTypeCannotBeChanged ()
		{
			AssertRuleDoesNotApply<Override> ("Show");
		}

		public sealed class DecorateThreadsRule : Rule, IMethodRule {

			public override void Initialize (IRunner runner)
			{
				base.Initialize (runner);
				runner.AnalyzeAssembly += this.OnAssembly;
			}

			// rule suggest HierarchicalEventArgs but we can't change the event definition
			public void OnAssembly (object sender, RunnerEventArgs e)
			{
				Console.WriteLine (e.CurrentAssembly);
			}

			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void EventsCannotBeChanged ()
		{
			AssertRuleDoesNotApply<DecorateThreadsRule> ("OnAssembly");
			AssertRuleDoesNotApply<AvoidUnnecessarySpecializationTest> ("BuildCustomAttributes");
		}

		// this will generate code like:
		// call instance void !!T[0...,0...]::Set(int32, int32, !!0)
		// which does not really exists (e.g. can't be resolve into a MethodDefinition)
		private void Fill<T> (T [,] source, T value)
		{
			for (int i = 0; i < source.GetLength (0); i++) {
				for (int j = 0; j < source.GetLength (1); j++)
					source [i, j] = value;
			}
		}

		[Test]
		public void MultiDimSet ()
		{
			AssertRuleSuccess<AvoidUnnecessarySpecializationTest> ("Fill");
		}

		[Test]
		public void OverloadNotSupportedByInterface ()
		{
			AssertRuleSuccess<GeneralizedClass> ("OverloadNotSupportedByInterface");
			AssertRuleFailure<SpecializedClass> ("OverloadNotSupportedByInterface", 1);
		}
	}
}
