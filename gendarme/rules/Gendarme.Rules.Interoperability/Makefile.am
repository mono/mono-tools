SUBDIRS=Test

interoperability_rulesdir=$(prefix)/lib/gendarme
interoperability_rules_SCRIPTS = ../../bin/Gendarme.Rules.Interoperability.dll
interoperability_rules_DATA = 
EXTRA_DIST = $(interoperability_rules_sources) $(interoperability_rules_sources_in) \
	Gendarme.Rules.Interoperability.mdp 
CLEANFILES = $(interoperability_rules_SCRIPTS) $(interoperability_rules_SCRIPTS).mdb Test.Rules.Interoperability.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Interoperability.xml TestResult.xml

interoperability_rules_sources_in = ../../AssemblyInfo.cs.in
interoperability_rules_generated_sources = $(interoperability_rules_sources_in:.in=)
interoperability_rules_sources = PInvokeShouldNotBeVisibleRule.cs GetLastErrorMustBeCalledRightAfterPInvokeRule.cs \
	UseManagedAlternativesToPInvokeRule.cs MarshalStringsInPInvokeDeclarationsRule.cs

interoperability_rules_build_sources = $(addprefix $(srcdir)/, $(interoperability_rules_sources))
interoperability_rules_build_sources += $(interoperability_rules_generated_sources)

../../bin/Gendarme.Rules.Interoperability.dll: $(interoperability_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(interoperability_rules_build_sources)

interoperability_test_sources = PInvokeShouldNotBeVisibleTest.cs GetLastErrorMustBeCalledRightAfterPInvokeTest.cs \
	UseManagedAlternativesToPInvokeTest.cs MarshalStringsInPInvokeDeclarationsTest.cs

interoperability_test_build_sources = $(addprefix $(srcdir)/Test/, $(interoperability_test_sources))

Test.Rules.Interoperability.dll: $(interoperability_test_build_sources) $(interoperability_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(interoperability_rules_SCRIPTS) -out:$@ $(interoperability_test_build_sources)

test: Test.Rules.Interoperability.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Interoperability.dll

self-test: $(interoperability_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(interoperability_rules_SCRIPTS)

