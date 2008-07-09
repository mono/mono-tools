SUBDIRS=Test

framework=../../bin/Gendarme.Framework.dll
security_rulesdir=$(prefix)/lib/gendarme
security_rules_SCRIPTS = ../../bin/Gendarme.Rules.Security.dll
security_rules_DATA = 
EXTRA_DIST = $(security_rules_sources) $(security_rules_sources_in) \
	Gendarme.Rules.Security.mdp Gendarme.Rules.Security.csproj
CLEANFILES = $(security_rules_SCRIPTS) $(security_rules_SCRIPTS).mdb Test.Rules.Security.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Security.xml TestResult.xml

security_rules_sources_in = ../../AssemblyInfo.cs.in
security_rules_generated_sources = $(security_rules_sources_in:.in=)
security_rules_sources = MethodCallWithSubsetLinkDemandRule.cs NonVirtualMethodWithInheritanceDemandRule.cs \
	SealedTypeWithInheritanceDemandRule.cs SecureGetObjectDataOverridesRule.cs \
	TypeExposeFieldsRule.cs TypeIsNotSubsetOfMethodSecurityRule.cs TypeLinkDemandRule.cs \
	StaticConstructorsShouldBePrivateRule.cs ArrayFieldsShouldNotBeReadOnlyRule.cs NativeFieldsShouldNotBeVisibleRule.cs

security_rules_build_sources = $(addprefix $(srcdir)/, $(security_rules_sources))
security_rules_build_sources += $(security_rules_generated_sources)

../../bin/Gendarme.Rules.Security.dll: $(security_rules_build_sources) $(framework)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:$(framework) \
		-out:$@ $(security_rules_build_sources)

security_test_sources = MethodCallWithSubsetLinkDemandTest.cs NonVirtualMethodWithInheritanceDemandTest.cs \
	SealedTypeWithInheritanceDemandTest.cs SecureGetObjectDataOverridesTest.cs \
	TypeExposeFieldsTest.cs TypeIsNotSubsetOfMethodSecurityTest.cs TypeLinkDemandTest.cs \
	StaticConstructorsShouldBePrivateTest.cs ArrayFieldsShouldNotBeReadOnlyTest.cs NativeFieldsShouldNotBeVisibleTest.cs

security_test_build_sources = $(addprefix $(srcdir)/Test/, $(security_test_sources))

Test.Rules.Security.dll: $(security_test_build_sources) $(security_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:$(framework) -r:../Test.Rules/Test.Rules.dll \
		-r:$(security_rules_SCRIPTS) -out:$@ $(security_test_build_sources)

test: Test.Rules.Security.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Security.dll

self-test: $(security_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(security_rules_SCRIPTS)

