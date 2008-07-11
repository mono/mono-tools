SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Smells.dll
tests_dll = Test.Rules.Smells.dll

smells_rulesdir=$(prefix)/lib/gendarme
smells_rules_SCRIPTS = $(rules_dll)
smells_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Smells.mdp Gendarme.Rules.Smells.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Smells.xml TestResult.xml

EXTRA_RULES_OPTIONS = -r:../../bin/Gendarme.Rules.Performance.dll

rules_sources =  \
	AvoidCodeDuplicatedInSameClassRule.cs \
	AvoidCodeDuplicatedInSiblingClassesRule.cs \
	AvoidLargeClassesRule.cs \
	AvoidLongMethodsRule.cs \
	AvoidLongParameterListsRule.cs \
	AvoidSpeculativeGeneralityRule.cs \
	CodeDuplicatedLocator.cs \
	ExpressionCollection.cs \
	ExpressionFillerVisitor.cs \
	Utilities.cs

EXTRA_TESTS_OPTIONS = -r:System.Windows.Forms.dll

tests_sources =  \
	AvoidCodeDuplicatedInSameClassTest.cs \
	AvoidCodeDuplicatedInSiblingClassesTest.cs \
	AvoidLargeClassesTest.cs \
	AvoidLongMethodsTest.cs \
	AvoidLongParameterListsTest.cs \
	AvoidSpeculativeGeneralityTest.cs
