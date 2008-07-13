SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Maintainability.dll
tests_dll = Test.Rules.Maintainability.dll

maintainability_rulesdir=$(prefix)/lib/gendarme
maintainability_rules_SCRIPTS = $(rules_dll)
maintainability_rules_DATA = 
EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Maintainability.csproj Gendarme.Rules.Maintainability.mdp
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AvoidComplexMethodsRule.cs \
	AvoidDeepInheritanceTreeRule.cs \
	AvoidLackOfCohesionOfMethodsRule.cs \
	AvoidUnnecessarySpecializationRule.cs \
	PreferStringIsNullOrEmptyRule.cs

tests_sources = \
	AvoidComplexMethodsTest.cs \
	AvoidDeepInheritanceTreeTest.cs \
	AvoidLackOfCohesionOfMethodsTest.cs \
	AvoidUnnecessarySpecializationTest.cs \
	PreferStringIsNullOrEmptyTest.cs
