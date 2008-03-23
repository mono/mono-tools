SUBDIRS=Test

badpractice_rulesdir=$(prefix)/lib/gendarme
badpractice_rules_SCRIPTS = ../../bin/Gendarme.Rules.BadPractice.dll
badpractice_rules_DATA = 
EXTRA_DIST = $(badpractice_rules_sources) $(badpractice_rules_sources_in) \
	Gendarme.Rules.BadPractice.mdp 
CLEANFILES = $(badpractice_rules_SCRIPTS) $(badpractice_rules_SCRIPTS).mdb Test.Rules.BadPractice.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.BadPractice.xml TestResult.xml

badpractice_rules_sources_in = ../../AssemblyInfo.cs.in
badpractice_rules_generated_sources = $(badpractice_rules_sources_in:.in=)
badpractice_rules_sources = CloneMethodShouldNotReturnNullRule.cs EqualShouldHandleNullArgRule.cs \
	ToStringReturnsNullRule.cs GetEntryAssemblyMayReturnNullRule.cs ConstructorShouldNotCallVirtualMethodsRule.cs \
	CheckNewExceptionWithoutThrowingRule.cs CheckNewThreadWithoutStartRule.cs DisableDebuggingCodeRule.cs

badpractice_rules_build_sources = $(addprefix $(srcdir)/, $(badpractice_rules_sources))
badpractice_rules_build_sources += $(badpractice_rules_generated_sources)

../../bin/Gendarme.Rules.BadPractice.dll: $(badpractice_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(badpractice_rules_build_sources)

badpractice_test_sources = CloneMethodShouldNotReturnNullTest.cs EqualShouldHandleNullArgTest.cs \
	ToStringReturnsNullTest.cs GetEntryAssemblyMayReturnNullTest.cs ConstructorShouldNotCallVirtualMethodsTest.cs \
	CheckNewExceptionWithoutThrowingTest.cs CheckNewThreadWithoutStartTest.cs DisableDebuggingCodeTest.cs

badpractice_test_build_sources = $(addprefix $(srcdir)/Test/, $(badpractice_test_sources))

Test.Rules.BadPractice.dll: $(badpractice_test_build_sources) $(badpractice_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(badpractice_rules_SCRIPTS) -out:$@ $(badpractice_test_build_sources)

test: Test.Rules.BadPractice.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.BadPractice.dll

self-test: $(badpractice_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(badpractice_rules_SCRIPTS)

