SUBDIRS=Test 

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Correctness.dll
tests_dll = Test.Rules.Correctness.dll

rules_doc_source = doc/Gendarme.Rules.Correctness.source 
rules_doc_tree = doc/Gendarme.Rules.Correctness.tree
rules_doc_zip = doc/Gendarme.Rules.Correctness.zip

correctness_rulesdir=$(prefix)/lib/gendarme
correctness_rules_SCRIPTS = $(rules_dll)
correctness_rules_DATA = 

correctness_documentationdir=$(prefix)/lib/monodoc/sources
correctness_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Correctness.mdp Gendarme.Rules.Correctness.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Correctness.xml TestResult.xml

rules_sources = \
	AvoidConstructorsInStaticTypesRule.cs \
	AvoidFloatingPointEqualityRule.cs \
	BadRecursiveInvocationRule.cs \
	CallingEqualsWithNullArgRule.cs \
	DontCompareWithNaNRule.cs \
	DoNotRoundIntegersRule.cs \
	FloatComparisonRule.cs \
	MethodCanBeMadeStaticRule.cs \
	ReviewCastOnIntegerDivisionRule.cs \
	ReviewCastOnIntegerMultiplicationRule.cs \
	ReviewDoubleAssignmentRule.cs \
	ReviewSelfAssignmentRule.cs \
	ReviewUselessControlFlowRule.cs \
	ReviewUseOfInt64BitsToDoubleRule.cs \
	ReviewUseOfModuloOneOnIntegersRule.cs \
	UseValueInPropertySetterRule.cs \
	ProvideCorrectArgumentsToFormattingMethodsRule.cs \
	AttributeStringLiteralsShouldParseCorrectlyRule.cs

tests_sources = \
	AvoidConstructorsInStaticTypesTest.cs \
	AvoidFloatingPointEqualityTest.cs \
	BadRecursiveInvocationTest.cs \
	CallingEqualsWithNullArgTest.cs \
	DontCompareWithNaNTest.cs \
	DoNotRoundIntegersTest.cs \
	MethodCanBeMadeStaticTest.cs \
	ReviewCastOnIntegerDivisionTest.cs \
	ReviewCastOnIntegerMultiplicationTest.cs \
	ReviewDoubleAssignmentTest.cs \
	ReviewSelfAssignmentTest.cs \
	ReviewUselessControlFlowTest.cs \
	ReviewUseOfInt64BitsToDoubleTest.cs \
	ReviewUseOfModuloOneOnIntegersTest.cs \
	UseValueInPropertySetterTest.cs \
	ProvideCorrectArgumentsToFormattingMethodsTest.cs \
	AttributeStringLiteralsShouldParseCorrectlyTest.cs	
