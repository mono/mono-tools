SUBDIRS=Test

design_rulesdir=$(prefix)/lib/gendarme
design_rules_SCRIPTS = ../../bin/Gendarme.Rules.Design.dll
design_rules_DATA = Gendarme.Rules.Design.xml
EXTRA_DIST = $(design_rules_sources) $(design_rules_sources_in) Gendarme.Rules.Design.xml \
	Gendarme.Rules.Design.mdp 
CLEANFILES = $(design_rules_SCRIPTS) $(design_rules_SCRIPTS).mdb Test.Rules.Design.dll
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
	OverrideEqualsMethodRule.cs ProvideAlternativeNamesForOperatorOverloadsRule.cs ImplementEqualsAndGetHashCodeInPairRule.cs

design_rules_build_sources = $(addprefix $(srcdir)/, $(design_rules_sources))
design_rules_build_sources += $(design_rules_generated_sources)

../../bin/Gendarme.Rules.Design.dll: $(design_rules_build_sources)
	$(GMCS) -debug -target:library -langversion:linq -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:System.Core.dll -r:../../bin/Gendarme.Framework.dll -out:$@ $(design_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

design_test_sources = DontDeclareProtectedFieldsInSealedClassTest.cs UsingCloneWithoutImplementingICloneableTest.cs \
	EnumsShouldDefineAZeroValueTest.cs FlagsShouldNotDefineAZeroValueTest.cs \
	AbstractTypesShouldNotHavePublicConstructorsTest.cs AvoidEmptyInterfaceTest.cs TypesShouldBeInsideNamespacesTest.cs \
	AvoidPropertiesWithoutGetAccessorTest.cs MainShouldNotBePublicTest.cs ConsiderConvertingMethodToPropertyTest.cs \
	AvoidPublicInstanceFieldsTest.cs MissingAttributeUsageOnCustomAttributeTest.cs \
	AttributeArgumentsShouldHaveAccessorsTest.cs TypesWithNativeFieldsShouldBeDisposableTest.cs \
	TypesWithDisposableFieldsShouldBeDisposableTest.cs DisposableTypesShouldHaveFinalizerTest.cs \
	DisposableFieldsShouldBeDisposedTest.cs FinalizersShouldBeProtectedTest.cs FinalizersShouldCallBaseClassFinalizerTest.cs \
	EnsureSymmetryForOverloadedOperatorsTest.cs OperatorEqualsShouldBeOverloadedTest.cs OverrideEqualsMethodTest.cs \
	ProvideAlternativeNamesForOperatorOverloadsTest.cs ImplementEqualsAndGetHashCodeInPairTest.cs

design_test_build_sources = $(addprefix $(srcdir)/Test/, $(design_test_sources))

Test.Rules.Design.dll: $(design_rules_build_sources) $(design_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(design_rules_SCRIPTS) -out:$@ $(design_test_build_sources)

test: Test.Rules.Design.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Design.dll

self-test: $(design_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(design_rules_SCRIPTS)

