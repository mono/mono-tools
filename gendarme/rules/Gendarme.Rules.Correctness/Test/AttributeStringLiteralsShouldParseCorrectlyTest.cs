//
// Unit tests for AttributeStringLiteralShouldParseCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;
using Test.Rules.Helpers;
using NUnit.Framework;

namespace Test.Rules.Correctness {
	[AttributeUsage (AttributeTargets.All)]
	public class ValidSince : Attribute {
		public ValidSince (string assemblyVersion)
		{
		}
	}

	[AttributeUsage (AttributeTargets.All)]
	public class Reference : Attribute {
		public Reference (string url)
		{
		}
	}

	[AttributeUsage (AttributeTargets.All)]
	public class Uses : Attribute {
		public Uses (string guid)
		{
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyMethodTest : MethodRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule> {
		[Test]
		public void SkipOnAttributelessMethodsTest ()
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
		}
		
		[ValidSince ("1.0.0.0")]
		[Reference ("http://www.mono-project.com/Gendarme")]
		[Uses ("00000101-0000-0000-c000-000000000046")]
		public void WellAttributedMethod ()
		{
		}

		[Test]
		public void SuccessOnWellAttributedMethodTest ()
		{
			AssertRuleSuccess<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("WellAttributedMethod");
		}

		[ValidSince ("foo")]
		[Reference ("bar")]
		[Uses ("0")]
		public void BadAttributedMethod ()
		{
		}

		[Test]
		public void FailOnBadAttributedMethodTest ()
		{
			AssertRuleFailure<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("BadAttributedMethod", 3);
		}

		public void WellParameterAttributedMethod (
		[ValidSince ("1.0.0.0")]
		[Reference ("http://www.mono-project.com/Gendarme")]
		[Uses ("00000101-0000-0000-c000-000000000046")]
		int x)
		{
		}

		[Test]
		public void SuccessOnWellParameterAttributedMethodTest ()
		{
			AssertRuleSuccess<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("WellParameterAttributedMethod");
		}

		public void BadParameterAttributedMethod (
		[ValidSince ("foo")]
		[Reference ("bar")]
		[Uses ("0")]
		int x)
		{
		}

		[Test]
		public void FailOnBadParameterAttributedMethodTest ()
		{
			AssertRuleFailure<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("BadParameterAttributedMethod", 3);
		}

		[return:ValidSince ("1.0.0.0")]
		[return:Reference ("http://www.mono-project.com/Gendarme")]
		[return:Uses ("00000101-0000-0000-c000-000000000046")]
		public void WellReturnParameterAttributedMethod ()
		{
		}

		[Test]
		public void SuccessOnWellReturnParameterAttributedMethodTest ()
		{
			AssertRuleSuccess<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("WellReturnParameterAttributedMethod");
		}

		[return:ValidSince ("foo")]
		[return:Reference ("bar")]
		[return:Uses ("0")]
		public void BadReturnParameterAttributedMethod ()
		{
		}

		[Test]
		public void FailOnBadReturnParameterAttributedMethodTest ()
		{
			AssertRuleFailure<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("BadReturnParameterAttributedMethod");
		}

		public void WellGenericParameterAttributedMethod<[ValidSince ("1.0.0.0"), Reference ("http://www.mono-project.com/Gendarme"), Uses ("00000101-0000-0000-c000-000000000046")] T> ()
		{
		}

		[Test]
		public void SuccessOnWellGenericParameterAttributedMethodTest ()
		{
			AssertRuleSuccess<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("WellGenericParameterAttributedMethod");
		}

		public void BadGenericParameterAttributedMethod<[ValidSince ("foo"), Reference ("bar"), Uses ("0")] T>  ()
		{
		}

		[Test]
		public void FailOnBadGenericParameterAttributedMethodTest ()
		{
			AssertRuleFailure<AttributeStringLiteralsShouldParseCorrectlyMethodTest> ("BadGenericParameterAttributedMethod", 3);
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyTypeTest : TypeRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule> {
		
		[Test]
		public void SkipOnAttributelessTypesTest ()
		{
			AssertRuleSuccess (SimpleTypes.Class);
		}
		
		[ValidSince ("1.0.0.0")]
		[Reference ("http://www.mono-project.com/Gendarme")]
		[Uses ("00000101-0000-0000-c000-000000000046")]
		class WellAttributedClass {
		}

		[Test]
		public void SuccessOnWellAttributedClassTest ()
		{
			AssertRuleSuccess<WellAttributedClass> ();
		}


		[ValidSince ("foo")]
		[Reference ("bar")]
		[Uses ("0")]	
		class BadAttributedClass {
		}

		[Test]
		public void FailOnBadAttributedClassTest ()
		{
			AssertRuleFailure<BadAttributedClass> (3);
		}

		class WellAttributedClassWithFields {
			[ValidSince ("1.0.0.0")]
			[Reference ("http://www.mono-project.com/Gendarme")]
			[Uses ("00000101-0000-0000-c000-000000000046")]
			object obj;

		}

		[Test]
		public void SuccessOnWellAttributedClassWithFieldsTest () {
			AssertRuleSuccess<WellAttributedClassWithFields> ();
		}

		class BadAttributedClassWithFields {
			[ValidSince ("foo")]
			[Reference ("bar")]
			[Uses ("0")]	
			int foo;
		}

		[Test]
		public void FailOnBadAttributedClassWithFieldsTest ()
		{
			AssertRuleFailure<BadAttributedClassWithFields> (3);
		}

		class ClassWithWellAttributedProperty {
			[ValidSince ("1.0.0.0")]
			[Reference ("http://www.mono-project.com/Gendarme")]
			[Uses ("00000101-0000-0000-c000-000000000046")]
			public int Property {
				get {
					return 0;
				}
			}
		}

		[Test]
		public void SuccessOnClassWithWellAttributedPropertyTest ()
		{
			AssertRuleSuccess<ClassWithWellAttributedProperty> ();	
		}

		class ClassWithBadAttributedProperty {
			[ValidSince ("foo")]
			[Reference ("bar")]
			[Uses ("0")]
			public int Property {
				get {
					return 0;
				}
			}
		}

		[Test]
		public void FailOnClassWithBadAttributedPropertyTest ()
		{
			AssertRuleFailure<ClassWithBadAttributedProperty> (3);
		}

		class ClassWithWellAttributedEvent {
			[ValidSince ("1.0.0.0")]
			[Reference ("http://www.mono-project.com/Gendarme")]
			[Uses ("00000101-0000-0000-c000-000000000046")]
			event EventHandler<EventArgs> customEvent;
		}

		[Test]
		public void SuccessOnClassWithWellAttributedEventTest ()
		{
			AssertRuleSuccess<ClassWithWellAttributedEvent> ();
		}

		class ClassWithBadAttributedEvent {
			[ValidSince ("foo")]
			[Reference ("bar")]
			[Uses ("0")]
			event EventHandler<EventArgs> customEvent;
		}

		[Test]
		public void FailOnClassWithBadAttributedEventTest ()
		{
			AssertRuleFailure<ClassWithBadAttributedEvent> (3);
		}

		class ClassWithWellAttributedGenericParameter<[ValidSince ("1.0.0.0"), Reference ("http://www.mono-project.com/Gendarme"), Uses ("00000101-0000-0000-c000-000000000046")] T> {
		}

		[Test]
		public void SuccessOnClassWithWellAttributedGenericParameterTest ()
		{
			AssertRuleSuccess<ClassWithWellAttributedGenericParameter<int>> ();
		}

		class ClassWithBadAttributedGenericParameter<[ValidSince ("foo"), Reference ("bar"), Uses ("0")] T> {
		}

		[Test]
		public void FailOnClassWithBadAttributedGenericParameterTest ()
		{
			AssertRuleFailure<ClassWithBadAttributedGenericParameter<int>> (3);
		}
	}

	[TestFixture]
	public class AttributeStringLiteralsShouldParseCorrectlyAssemblyTest : AssemblyRuleTestFixture<AttributeStringLiteralsShouldParseCorrectlyRule>{

		static void AddStringArgument (CustomAttribute attribute, AssemblyDefinition assembly, string str)
		{
			attribute.ConstructorArguments.Add (
				new CustomAttributeArgument (assembly.MainModule.TypeSystem.String, str));
		}

		private AssemblyDefinition GenerateFakeAssembly (string version, string url, string guid)
		{
			AssemblyDefinition definition = DefinitionLoader.GetAssemblyDefinition (this.GetType ());
			CustomAttribute attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<ValidSince> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, version);
			definition.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Reference> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, url);
			definition.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Uses> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, guid);
			definition.CustomAttributes.Add (attribute);

			return definition;
		}

		private AssemblyDefinition GenerateFakeModuleAnnotatedAssembly (string version, string url, string guid)
		{
			AssemblyDefinition definition = DefinitionLoader.GetAssemblyDefinition (this.GetType ());
			ModuleDefinition module = ModuleDefinition.CreateModule ("test", ModuleKind.NetModule);
			definition.Modules.Add (module);
			CustomAttribute attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<ValidSince> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, version);
			module.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Reference> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, url);
			module.CustomAttributes.Add (attribute);

			attribute = new CustomAttribute (DefinitionLoader.GetMethodDefinition<Uses> (".ctor", new Type[] {typeof (string)}));
			AddStringArgument (attribute, definition, guid);
			module.CustomAttributes.Add (attribute);

			return definition;
		}

		[TearDown]
		public void TearDown ()
		{
			AssemblyDefinition definition = DefinitionLoader.GetAssemblyDefinition (this.GetType ());
			
			foreach (CustomAttribute attribute in new ArrayList (definition.CustomAttributes)) {
				//We only revert our changes on assembly.
				if (String.Compare (attribute.AttributeType.FullName, "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute") != 0)
					definition.CustomAttributes.Remove (attribute);
			}
			
			for (int index = 0; index < definition.Modules.Count; index++) {
				if (String.Compare (definition.Modules[index].Name, "test") == 0)
					definition.Modules.Remove (definition.Modules[index]);
			}
		}

		[Test]
		public void SuccessOnWellAttributedAssemblyTest ()
		{
			AssertRuleSuccess (GenerateFakeAssembly ("1.0.0.0", "http://www.mono-project.com/Gendarme", "00000101-0000-0000-c000-000000000046"));
		}

		[Test]
		public void FailOnBadAttributedAssemblyTest ()
		{
			AssertRuleFailure (GenerateFakeAssembly ("foo", "bar", "0"), 3);
		}

		[Test]
		public void SuccessOnWellAttributedModuleTest ()
		{
			AssertRuleSuccess (GenerateFakeModuleAnnotatedAssembly ("1.0.0.0", "http://www.mono-project.com/Gendarme", "00000101-0000-0000-c000-000000000046"));
		}

		[Test]
		public void FailOnBadAttributedModuleTest ()
		{
			AssertRuleFailure (GenerateFakeModuleAnnotatedAssembly ("foo", "bar", "0"), 3);
		}
	}
}
