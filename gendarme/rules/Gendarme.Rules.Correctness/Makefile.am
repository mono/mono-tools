SUBDIRS=Test

correctness_rulesdir=$(prefix)/lib/gendarme
correctness_rules_SCRIPTS = ../../bin/Gendarme.Rules.Correctness.dll
correctness_rules_DATA = 
EXTRA_DIST = $(correctness_rules_sources) $(correctness_rules_sources_in) \
	 Gendarme.Rules.Correctness.mdp  Gendarme.Rules.Correctness.csproj
CLEANFILES = $(correctness_rules_SCRIPTS) $(correctness_rules_SCRIPTS).mdb Test.Rules.Correctness.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Correctness.xml TestResult.xml

correctness_rules_sources_in = ../../AssemblyInfo.cs.in
correctness_rules_generated_sources = $(correctness_rules_sources_in:.in=)
correctness_rules_sources = BadRecursiveInvocationRule.cs MethodCanBeMadeStaticRule.cs \
	CallingEqualsWithNullArgRule.cs FloatComparisonRule.cs UseValueInPropertySetterRule.cs \
	AvoidConstructorsInStaticTypesRule.cs DontCompareWithNaNRule.cs \
	ReviewDoubleAssignmentRule.cs \
	ReviewUselessControlFlowRule.cs \
	ReviewUseOfModuloOneOnIntegersRule.cs

correctness_rules_build_sources = $(addprefix $(srcdir)/, $(correctness_rules_sources))
correctness_rules_build_sources += $(correctness_rules_generated_sources)

../../bin/Gendarme.Rules.Correctness.dll: $(correctness_rules_build_sources)
	$(GMCS) -debug -target:library  -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(correctness_rules_build_sources)

correctness_test_sources = BadRecursiveInvocationTest.cs MethodCanBeMadeStaticTest.cs UseValueInPropertySetterTest.cs \
	AvoidConstructorsInStaticTypesTest.cs CallingEqualsWithNullArgTest.cs FloatComparisonRuleTest.cs \
	DontCompareWithNaNTest.cs \
	ReviewDoubleAssignmentTest.cs \
	ReviewUselessControlFlowTest.cs \
	ReviewUseOfModuloOneOnIntegersTest.cs

correctness_test_build_sources = $(addprefix $(srcdir)/Test/, $(correctness_test_sources))

Test.Rules.Correctness.dll: $(correctness_test_build_sources) $(correctness_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(correctness_rules_SCRIPTS) -r:../Test.Rules/Test.Rules.dll -out:$@ $(correctness_test_build_sources)

test: Test.Rules.Correctness.dll

run-test: test
	MONO_PATH=../../bin/:../Test.Rules/:$(MONO_PATH) nunit-console2 Test.Rules.Correctness.dll

self-test: $(correctness_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(correctness_rules_SCRIPTS)

