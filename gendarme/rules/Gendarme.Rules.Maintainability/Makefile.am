SUBDIRS=Test

maintainability_rulesdir=$(prefix)/lib/gendarme
maintainability_rules_SCRIPTS = ../../bin/Gendarme.Rules.Maintainability.dll
maintainability_rules_DATA = 
EXTRA_DIST = $(maintainability_rules_sources) $(maintainability_rules_sources_in) \
	Gendarme.Rules.Maintainability.mdp 
CLEANFILES = $(maintainability_rules_SCRIPTS)* Test.Rules.Maintainability.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Maintainability.xml TestResult.xml

maintainability_rules_sources_in = ../../AssemblyInfo.cs.in
maintainability_rules_generated_sources = $(maintainability_rules_sources_in:.in=)
maintainability_rules_sources = \
	AvoidComplexMethodsRule.cs 

maintainability_rules_build_sources = $(addprefix $(srcdir)/, $(maintainability_rules_sources))
maintainability_rules_build_sources += $(maintainability_rules_generated_sources)

../../bin/Gendarme.Rules.Maintainability.dll: $(maintainability_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(maintainability_rules_build_sources)

maintainability_test_sources = \
	AvoidComplexMethodsTest.cs 

maintainability_test_build_sources = $(addprefix $(srcdir)/Test/, $(maintainability_test_sources))

Test.Rules.Maintainability.dll: $(maintainability_test_build_sources) $(maintainability_rules_SCRIPTS)
	$(GMCS) -target:library -debug -r:$(CECIL_ASM) -pkg:mono-nunit \
		-r:../../bin/Gendarme.Framework.dll -r:$(maintainability_rules_SCRIPTS) \
		-r:../Test.Rules/Test.Rules.dll -out:$@ $(maintainability_test_build_sources)

test: Test.Rules.Maintainability.dll

run-test: test
	MONO_PATH=../../bin/:../Test.Rules/:$(MONO_PATH) nunit-console2 Test.Rules.Maintainability.dll

self-test: $(maintainability_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(maintainability_rules_SCRIPTS)

