SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.BadPractice.dll
tests_dll = Test.Rules.BadPractice.dll

badpractice_rulesdir=$(prefix)/lib/gendarme
badpractice_rules_SCRIPTS = $(rules_dll)
badpractice_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.BadPractice.mdp Gendarme.Rules.BadPractice.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.BadPractice.xml TestResult.xml

rules_sources = \
	AvoidCallingProblematicMethodsRule.cs \
	AvoidVisibleConstantFieldRule.cs \
	CheckNewExceptionWithoutThrowingRule.cs \
	CheckNewThreadWithoutStartRule.cs \
	CloneMethodShouldNotReturnNullRule.cs \
	ConstructorShouldNotCallVirtualMethodsRule.cs \
	DisableDebuggingCodeRule.cs \
	DoNotForgetNotImplementedMethodsRule.cs \
	EqualShouldHandleNullArgRule.cs \
	GetEntryAssemblyMayReturnNullRule.cs \
	ObsoleteMessagesShouldNotBeEmptyRule.cs \
	ReplaceIncompleteOddnessCheckRule.cs \
	ToStringReturnsNullRule.cs 

tests_sources = \
	AvoidVisibleConstantFieldTest.cs \
	AvoidCallingProblematicMethodsTest.cs \
	CheckNewExceptionWithoutThrowingTest.cs \
	CheckNewThreadWithoutStartTest.cs \
	CloneMethodShouldNotReturnNullTest.cs \
	ConstructorShouldNotCallVirtualMethodsTest.cs \
	DoNotForgetNotImplementedMethodsTest.cs \
	DisableDebuggingCodeTest.cs \
	EqualShouldHandleNullArgTest.cs \
	GetEntryAssemblyMayReturnNullTest.cs \
	ObsoleteMessagesShouldNotBeEmptyTest.cs \
	ReplaceIncompleteOddnessCheckTest.cs \
	ToStringReturnsNullTest.cs 
