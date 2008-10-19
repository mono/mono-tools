SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.BadPractice.dll
tests_dll = Test.Rules.BadPractice.dll

rules_doc_source = doc/Gendarme.Rules.BadPractice.source 
rules_doc_tree = doc/Gendarme.Rules.BadPractice.tree
rules_doc_zip = doc/Gendarme.Rules.BadPractice.zip

badpractice_rulesdir=$(prefix)/lib/gendarme
badpractice_rules_SCRIPTS = $(rules_dll)
badpractice_rules_DATA = 

badpractice_documentationdir=$(prefix)/lib/monodoc/sources
badpractice_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.BadPractice.mdp Gendarme.Rules.BadPractice.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.BadPractice.xml TestResult.xml

rules_sources = \
	AvoidAssemblyVersionMismatchRule.cs \
	AvoidCallingProblematicMethodsRule.cs \
	AvoidVisibleConstantFieldRule.cs \
	CheckNewExceptionWithoutThrowingRule.cs \
	CheckNewThreadWithoutStartRule.cs \
	CloneMethodShouldNotReturnNullRule.cs \
	ConstructorShouldNotCallVirtualMethodsRule.cs \
	DisableDebuggingCodeRule.cs \
	DoNotForgetNotImplementedMethodsRule.cs \
	DoNotUseGetInterfaceToCheckAssignabilityRule.cs \
	EqualShouldHandleNullArgRule.cs \
	GetEntryAssemblyMayReturnNullRule.cs \
	ObsoleteMessagesShouldNotBeEmptyRule.cs \
	ReplaceIncompleteOddnessCheckRule.cs \
	ReturnNullRule.cs \
	ToStringReturnsNullRule.cs 

EXTRA_TESTS_OPTIONS = -d:DEBUG -d:TRACE

tests_sources = \
	AvoidAssemblyVersionMismatchTest.cs \
	AvoidVisibleConstantFieldTest.cs \
	AvoidCallingProblematicMethodsTest.cs \
	CheckNewExceptionWithoutThrowingTest.cs \
	CheckNewThreadWithoutStartTest.cs \
	CloneMethodShouldNotReturnNullTest.cs \
	ConstructorShouldNotCallVirtualMethodsTest.cs \
	DoNotForgetNotImplementedMethodsTest.cs \
	DoNotUseGetInterfaceToCheckAssignabilityTest.cs \
	DisableDebuggingCodeTest.cs \
	EqualShouldHandleNullArgTest.cs \
	GetEntryAssemblyMayReturnNullTest.cs \
	ObsoleteMessagesShouldNotBeEmptyTest.cs \
	ReplaceIncompleteOddnessCheckTest.cs \
	ToStringReturnsNullTest.cs 
