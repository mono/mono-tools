SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Correctness.dll
tests_dll = Test.Rules.Correctness.dll

correctness_rulesdir=$(prefix)/lib/gendarme
correctness_rules_SCRIPTS = $(rules_dll)
correctness_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Correctness.mdp Gendarme.Rules.Correctness.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Correctness.xml TestResult.xml

rules_sources = \
	AvoidConstructorsInStaticTypesRule.cs \
	BadRecursiveInvocationRule.cs \
	CallingEqualsWithNullArgRule.cs \
	DontCompareWithNaNRule.cs \
	DoNotRoundIntegersRule.cs \
	FloatComparisonRule.cs \
	MethodCanBeMadeStaticRule.cs \
	ReviewDoubleAssignmentRule.cs \
	ReviewSelfAssignmentRule.cs \
	ReviewUselessControlFlowRule.cs \
	ReviewUseOfInt64BitsToDoubleRule.cs \
	ReviewUseOfModuloOneOnIntegersRule.cs \
	UseValueInPropertySetterRule.cs

tests_sources = \
	AvoidConstructorsInStaticTypesTest.cs 
	BadRecursiveInvocationTest.cs \
	CallingEqualsWithNullArgTest.cs \
	DontCompareWithNaNTest.cs \
	DoNotRoundIntegersTest.cs \
	FloatComparisonRuleTest.cs \
	MethodCanBeMadeStaticTest.cs \
	ReviewDoubleAssignmentTest.cs \
	ReviewSelfAssignmentTest.cs \
	ReviewUselessControlFlowTest.cs \
	ReviewUseOfInt64BitsToDoubleTest.cs \
	ReviewUseOfModuloOneOnIntegersTest.cs \
	UseValueInPropertySetterTest.cs
