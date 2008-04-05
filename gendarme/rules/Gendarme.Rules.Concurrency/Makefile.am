SUBDIRS=Test

concurrency_rulesdir=$(prefix)/lib/gendarme
concurrency_rules_SCRIPTS = ../../bin/Gendarme.Rules.Concurrency.dll
concurrency_rules_DATA = 
EXTRA_DIST = $(concurrency_rules_sources) $(concurrency_rules_sources_in) \
	Gendarme.Rules.Concurrency.mdp Gendarme.Rules.Concurrency.csproj
CLEANFILES = $(concurrency_rules_SCRIPTS) $(concurrency_rules_SCRIPTS).mdb Test.Rules.Concurrency.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Concurrency.xml TestResult.xml

concurrency_rules_sources_in = ../../AssemblyInfo.cs.in
concurrency_rules_generated_sources = $(concurrency_rules_sources_in:.in=)
concurrency_rules_sources = DoubleCheckLockingRule.cs WriteStaticFieldFromInstanceMethodRule.cs \
	NonConstantStaticFieldsShouldNotBeVisibleRule.cs

concurrency_rules_build_sources = $(addprefix $(srcdir)/, $(concurrency_rules_sources))
concurrency_rules_build_sources += $(concurrency_rules_generated_sources)

../../bin/Gendarme.Rules.Concurrency.dll: $(concurrency_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(concurrency_rules_build_sources)

concurrency_test_sources = DoubleCheckLockingTest.cs WriteStaticFieldFromInstanceMethodTest.cs \
	NonConstantStaticFieldsShouldNotBeVisibleTest.cs

concurrency_test_build_sources = $(addprefix $(srcdir)/Test/, $(concurrency_test_sources))

Test.Rules.Concurrency.dll: $(concurrency_test_build_sources) $(concurrency_rules_SCRIPTS)
	cp ../Test.Rules/*.dll .
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:Test.Rules.dll -r:../../bin/Gendarme.Framework.dll \
		-r:$(concurrency_rules_SCRIPTS) -out:$@ $(concurrency_test_build_sources)

test: Test.Rules.Concurrency.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Concurrency.dll

self-test: $(concurrency_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(concurrency_rules_SCRIPTS)

