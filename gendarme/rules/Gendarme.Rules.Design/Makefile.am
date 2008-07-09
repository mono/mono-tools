SUBDIRS=Test

framework=../../bin/Gendarme.Framework.dll
design_rulesdir=$(prefix)/lib/gendarme
design_rules_SCRIPTS = ../../bin/Gendarme.Rules.Design.dll
design_rules_DATA = 
EXTRA_DIST = $(design_rules_sources) $(design_rules_sources_in) \
	Gendarme.Rules.Design.mdp 
CLEANFILES = $(design_rules_SCRIPTS) $(design_rules_SCRIPTS).mdb Test.Rules.Design.dll* Test.Rules.*
DISTCLEANFILES = Makefile.in Gendarme.Rules.Design.xml TestResult.xml

design_rules_sources_in = ../../AssemblyInfo.cs.in
design_rules_generated_sources = $(design_rules_sources_in:.in=)
design_rules_sources = DontDeclareProtectedFieldsInSealedClassRule.cs UsingCloneWithoutImplementingICloneableRule.cs \
	DefineAZeroValueRule.cs EnumsShouldDefineAZeroValueRule.cs FlagsShouldNotDefineAZeroValueRule.cs \
	AbstractTypesShouldNotHavePublicConstructorsRule.cs AvoidEmptyInterfaceRule.cs TypesShouldBeInsideNamespacesRule.cs \
	AvoidPropertiesWithoutGetAccessorRule.cs MainShouldNotBePublicRule.cs ConsiderConvertingMethodToPropertyRule.cs \
	AvoidPublicInstanceFieldsRule.cs MissingAttributeUsageOnCustomAttributeRule.cs \
	AttributeArgumentsShouldHaveAccessorsRule.cs TypesWithNativeFieldsShouldBeDisposableRule.cs \
	TypesWithDisposableFieldsShouldBeDisposableRule.cs DisposableTypesShouldHaveFinalizerRule.cs \
	DisposableFieldsShouldBeDisposedRule.cs FinalizersShouldBeProtectedRule.cs FinalizersShouldCallBaseClassFinalizerRule.cs \
	EnsureSymmetryForOverloadedOperatorsRule.cs OperatorEqualsShouldBeOverloadedRule.cs \
	OverrideEqualsMethodRule.cs ProvideAlternativeNamesForOperatorOverloadsRule.cs ImplementEqualsAndGetHashCodeInPairRule.cs \
	EnumsShouldUseInt32Rule.cs ImplementIComparableCorreclyRule.cs ImplementGenericCollectionInterfacesRule.cs \
	ConsiderConvertingFieldToNullableRule.cs AvoidVisibleNestedTypesRule.cs

design_rules_build_sources = $(addprefix $(srcdir)/, $(design_rules_sources))
design_rules_build_sources += $(design_rules_generated_sources)

../../bin/Gendarme.Rules.Design.dll: $(design_rules_build_sources) $(framework)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:System.Core.dll \
		-r:$(framework) -out:$@ $(design_rules_build_sources)

design_test_sources = DontDeclareProtectedFieldsInSealedClassTest.cs UsingCloneWithoutImplementingICloneableTest.cs \
	EnumsShouldDefineAZeroValueTest.cs FlagsShouldNotDefineAZeroValueTest.cs \
	AbstractTypesShouldNotHavePublicConstructorsTest.cs AvoidEmptyInterfaceTest.cs TypesShouldBeInsideNamespacesTest.cs \
	AvoidPropertiesWithoutGetAccessorTest.cs MainShouldNotBePublicTest.cs ConsiderConvertingMethodToPropertyTest.cs \
	AvoidPublicInstanceFieldsTest.cs MissingAttributeUsageOnCustomAttributeTest.cs \
	AttributeArgumentsShouldHaveAccessorsTest.cs TypesWithNativeFieldsShouldBeDisposableTest.cs \
	TypesWithDisposableFieldsShouldBeDisposableTest.cs DisposableTypesShouldHaveFinalizerTest.cs \
	DisposableFieldsShouldBeDisposedTest.cs FinalizersShouldBeProtectedTest.cs FinalizersShouldCallBaseClassFinalizerTest.cs \
	EnsureSymmetryForOverloadedOperatorsTest.cs OperatorEqualsShouldBeOverloadedTest.cs OverrideEqualsMethodTest.cs \
	ProvideAlternativeNamesForOperatorOverloadsTest.cs ImplementEqualsAndGetHashCodeInPairTest.cs EnumsShouldUseInt32Test.cs \
	ImplementIComparableCorreclyTest.cs ImplementGenericCollectionInterfacesTest.cs \
	ConsiderConvertingFieldToNullableTest.cs AvoidVisibleNestedTypesTest.cs

design_test_build_sources = $(addprefix $(srcdir)/Test/, $(design_test_sources))

Test.Rules.Design.dll: $(design_test_build_sources) $(design_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit \
		-r:$(framework) -r:../Test.Rules/Test.Rules.dll \
		-r:$(design_rules_SCRIPTS) -out:$@ $(design_test_build_sources)

test: Test.Rules.Design.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Design.dll

self-test: $(design_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(design_rules_SCRIPTS)

