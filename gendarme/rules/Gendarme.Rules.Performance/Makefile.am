SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Performance.dll
tests_dll = Test.Rules.Performance.dll

performance_rulesdir=$(prefix)/lib/gendarme
performance_rules_SCRIPTS = $(rules_dll)
performance_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Performance.mdp Gendarme.Rules.Performance.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Performance.xml TestResult.xml

rules_sources = \
	AvoidLargeNumberOfLocalVariablesRule.cs \
	EmptyDestructorRule.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeRule.cs \
	UseStringEmptyRule.cs \
	UseTypeEmptyTypesRule.cs \
	AvoidUnusedParametersRule.cs \
	AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUncalledPrivateCodeRule.cs \
	AvoidUnneededCallsOnStringRule.cs \
	DontIgnoreMethodResultRule.cs \
	CompareWithStringEmptyEfficientlyRule.cs \
	AvoidReturningArraysOnPropertiesRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs \
	UseIsOperatorRule.cs \
	AvoidUnsealedUninheritedInternalClassesRule.cs \
	OverrideValueTypeDefaultsRule.cs \
	ImplementEqualsTypeRule.cs \
	MathMinMaxCandidateRule.cs \
	AvoidLargeStructureRule.cs \
	RemoveUnusedLocalVariablesRule.cs \
	AvoidUnneededUnboxingRule.cs \
	ConsiderCustomAccessorsForNonVisibleEventsRule.cs \
	AvoidTypeGetTypeForConstantStringsRule.cs \
	AvoidRepetitiveCastsRule.cs \
	AvoidUnusedPrivateFieldsRule.cs

EXTRA_TESTS_OPTIONS = -unsafe

tests_sources = \
	AvoidLargeNumberOfLocalVariablesTest.cs \
	EmptyDestructorTest.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeTest.cs \
	UseStringEmptyTest.cs \
	UseTypeEmptyTypesTest.cs \
	AvoidUnusedParametersTest.cs \
	CompareWithStringEmptyEfficientlyTest.cs \
	AvoidUncalledPrivateCodeTest.cs \
	AvoidUnneededCallsOnStringTest.cs \
	AvoidUninstantiatedInternalClassesTest.cs \
	DontIgnoreMethodResultTest.cs \
	AvoidReturningArraysOnPropertiesTest.cs \
	AvoidUnsealedConcreteAttributesTest.cs \
	UseIsOperatorTest.cs \
	AvoidUnsealedUninheritedInternalClassesTest.cs \
	OverrideValueTypeDefaultsTest.cs \
	ImplementEqualsTypeTest.cs \
	MathMinMaxCandidateTest.cs \
	AvoidLargeStructureTest.cs \
	RemoveUnusedLocalVariablesTest.cs \
	AvoidUnneededUnboxingTest.cs \
	ConsiderCustomAccessorsForNonVisibleEventsTest.cs \
	AvoidTypeGetTypeForConstantStringsTest.cs \
	AvoidRepetitiveCastsTest.cs \
	AvoidUnusedPrivateFieldsTest.cs
