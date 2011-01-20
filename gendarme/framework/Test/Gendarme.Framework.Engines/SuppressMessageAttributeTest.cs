// 
// Unit tests for SuppressMessageAttribute
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010, 2011 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Engines;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

[assembly: SuppressMessage ("Test.Framework", "SuppressAssemblyRule")]

// test on module using a FxCop-like rule syntax (which gets remapped to Gendarme)
[module: SuppressMessage ("Unit.Test", "CA9999:TestingMapping")]

namespace Test.Framework {

	public class SuppressAssemblyRule : Rule, IAssemblyRule {

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			return RuleResult.Success;
		}
	}

	[TestFixture]
	public class SuppressMessageAttribute_AssemblyTest : AssemblyRuleTestFixture<SuppressAssemblyRule> {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}

		[Test]
		// cover AttributeTargets.Assembly
		public void Assemblies ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);
			// see [assembly: SuppressMessage ...] for test case
			// since the rule is NOT executed, the target (assembly) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply (assembly);

			unit = typeof (Rule).Assembly.Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			AssertRuleSuccess (assembly);
		}
	}

	// we need to be able to distinguish between Assembly and Module
	[FxCopCompatibility ("Unit.Test", "CA9999:TestingMapping")]
	public class SuppressModuleRule : Rule, IAssemblyRule {

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			Runner.Report (assembly.MainModule, Severity.Audit, Confidence.Low);
			return Runner.CurrentRuleResult;
		}
	}

	[TestFixture]
	public class SuppressMessageAttribute_ModuleTest : AssemblyRuleTestFixture<SuppressModuleRule> {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}

		[Test]
		// cover AttributeTargets.Module
		public void Modules ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);
			// see [module: SuppressMessage ...] for test case
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess (assembly);

			unit = typeof (Rule).Assembly.Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			AssertRuleFailure (assembly);
		}
	}

	public class SuppressTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.HasEvents) {
				Runner.Report (type.Events [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			if (type.HasProperties) {
				Runner.Report (type.Properties [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			if (type.HasFields) {
				Runner.Report (type.Fields [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			if (type.HasGenericParameters) {
				Runner.Report (type.GenericParameters [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			return RuleResult.Success;
		}
	}

	[TestFixture]
	public class SuppressMessageAttribute_TypeTest : TypeRuleTestFixture<SuppressTypeRule> {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}

		[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
		class ClassDirectlySuppressed {
		}

		// see GlobalSuppressions.cs
		class ClassIndirectlySuppressed {
		}

		class ClassNotSuppressed {
		}

		[Test]
		// cover AttributeTargets.Class
		public void Classes ()
		{
			// since the rule is NOT executed, the target (class) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<ClassDirectlySuppressed> ();
			AssertRuleDoesNotApply<ClassIndirectlySuppressed> ();
			AssertRuleSuccess<ClassNotSuppressed> ();
		}

		[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
		delegate void DelegateDirectlySuppressed (int x);

		delegate void DelegateNotSuppressed ();

		[Test]
		// cover AttributeTargets.Delegate
		public void Delegates ()
		{
			// since the rule is NOT executed, the target (delegate) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<DelegateDirectlySuppressed> ();
			AssertRuleSuccess<DelegateNotSuppressed> ();
		}

		[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
		enum EnumDirectlySuppressed {
		}

		enum EnumNotSuppressed {
		}

		[Test]
		// cover AttributeTargets.Enum
		public void Enums ()
		{
			// since the rule is NOT executed, the target (enum) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<EnumDirectlySuppressed> ();
			AssertRuleFailure<EnumNotSuppressed> ();
		}

		class EventDirectlySuppressed {
			[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
			event EventHandler<EventArgs> Event;
		}

		class EventNotSuppressed {
			event EventHandler<EventArgs> Event;
		}

		[Test]
		// cover AttributeTargets.Event
		public void Events ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<EventDirectlySuppressed> ();
			AssertRuleFailure<EventNotSuppressed> ();
		}

		class FieldDirectlySuppressed {
			[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
			public int Field;
		}

		class FieldNotSuppressed {
			public int Field;
		}

		[Test]
		// cover AttributeTargets.Field
		public void Fields ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<FieldDirectlySuppressed> ();
			AssertRuleFailure<FieldNotSuppressed> ();
		}

		class GenericParameterDirectlySuppressed<[SuppressMessage ("Test.Framework", "SuppressTypeRule")] T> {
		}

		class GenericParameterNotSuppressed<T> {
		}

		[Test]
		// cover (part of) AttributeTargets.GenericParameter (see Method)
		public void GenericParameters ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<GenericParameterDirectlySuppressed<object>> ();
			AssertRuleFailure<GenericParameterNotSuppressed<object>> ();
		}

		[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
		interface InterfaceDirectlySuppressed {
		}

		interface InterfaceNotSuppressed {
		}

		[Test]
		// cover AttributeTargets.Interface
		public void Interfaces ()
		{
			// since the rule is NOT executed, the target (interface) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<InterfaceDirectlySuppressed> ();
			AssertRuleSuccess<InterfaceNotSuppressed> ();
		}

		class PropertyDirectlySuppressed {
			[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
			public int Property { get; set; }
		}

		class PropertyNotSuppressed {
			public int Property { get; set; }
		}

		[Test]
		// AttributeTargets.Property
		public void Properties ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<PropertyDirectlySuppressed> ();
			AssertRuleFailure<PropertyNotSuppressed> ();
		}

		[SuppressMessage ("Test.Framework", "SuppressTypeRule")]
		struct StructDirectlySuppressed {
		}

		struct StructNotSuppressed {
		}

		[Test]
		// AttributeTargets.Struct
		public void Structs ()
		{
			// since the rule is NOT executed, the target (struct) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<StructDirectlySuppressed> ();
			AssertRuleSuccess<StructNotSuppressed> ();
		}
	}

	public class SuppressMethodRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.HasGenericParameters) {
				Runner.Report (method.GenericParameters [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			if (method.HasParameters) {
				Runner.Report (method.Parameters [0], Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			if (method.ReturnType.FullName != "System.Void") {
				Runner.Report (method.MethodReturnType, Severity.Critical, Confidence.Total);
				return Runner.CurrentRuleResult;
			}
			return RuleResult.Failure;
		}
	}

	[TestFixture]
	public class SuppressMessageAttribute_MethodTest : MethodRuleTestFixture<SuppressMethodRule> {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}

		class TestCases {

			[SuppressMessage ("Test.Framework", "SuppressMethodRule")]
			static TestCases ()
			{
			}

			public TestCases (int i)
			{
			}

			[SuppressMessage ("Test.Framework", "SuppressMethodRule")]
			public void MethodDirectlySuppressed ()
			{
			}

			public void MethodNotSuppressed ()
			{
			}

			public void GenericParameterDirectlySuppressed<[SuppressMessage ("Test.Framework", "SuppressMethodRule")] T> ()
			{
			}

			public void GenericParameterNotSuppressed<T> ()
			{
			}

			public void ParameterDirectlySuppressed ([SuppressMessage ("Test.Framework", "SuppressMethodRule")] int x)
			{
			}

			public void ParameterNotSuppressed (int x)
			{
			}

			[return: SuppressMessage ("Test.Framework", "SuppressMethodRule")]
			public object ReturnValueDirectlySuppressed ()
			{
				return null;
			}

			public object ReturnValueNotSuppressed (int x)
			{
				return null;
			}

			[SuppressMessage ("Test.Framework", "SuppressMethodRule")]
			public int PropertySuppressed { get; set; }

			public int PropertyNotSuppressed { get; set; }
		}

		[Test]
		// cover AttributeTargets.Constructor
		public void Constructors ()
		{
			// since the rule is NOT executed, the target (struct) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<TestCases> (".cctor");
			AssertRuleFailure<TestCases> (".ctor");
		}

		[Test]
		// cover (part of) AttributeTargets.GenericParameter (see Type)
		public void GenericParameters ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<TestCases> ("GenericParameterDirectlySuppressed");
			AssertRuleFailure<TestCases> ("GenericParameterNotSuppressed");
		}

		[Test]
		// cover AttributeTargets.Parameter
		public void Parameters ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<TestCases> ("ParameterDirectlySuppressed");
			AssertRuleFailure<TestCases> ("ParameterNotSuppressed");
		}

		[Test]
		// cover AttributeTargets.Method
		public void Methods ()
		{
			// since the rule is NOT executed, the target (struct) being ignored, the result is DoesNotApply
			AssertRuleDoesNotApply<TestCases> ("MethodDirectlySuppressed");
			AssertRuleFailure<TestCases> ("MethodNotSuppressed");
		}

		[Test]
		// cover AttributeTargets.ReturnValue
		public void ReturnValues ()
		{
			// since the rule is executed, then the defect ignored, the result is Success
			AssertRuleSuccess<TestCases> ("ReturnValueDirectlySuppressed");
			AssertRuleFailure<TestCases> ("ReturnValueNotSuppressed");
		}

		[Test]
		// AttributeTargets.Property
		public void Properties ()
		{
			// suppressing on the properties also means ignoring getters and setters
			AssertRuleDoesNotApply<TestCases> ("get_PropertySuppressed");
			AssertRuleDoesNotApply<TestCases> ("set_PropertySuppressed");
			AssertRuleFailure<TestCases> ("get_PropertyNotSuppressed", 1);
			AssertRuleFailure<TestCases> ("set_PropertyNotSuppressed", 1);
		}
	}
}
