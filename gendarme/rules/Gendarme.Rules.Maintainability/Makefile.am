SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Maintainability.dll
tests_dll = Test.Rules.Maintainability.dll

rules_doc_source = doc/Gendarme.Rules.Maintainability.source 
rules_doc_tree = doc/Gendarme.Rules.Maintainability.tree
rules_doc_zip = doc/Gendarme.Rules.Maintainability.zip

maintainability_rulesdir=$(prefix)/lib/gendarme
maintainability_rules_SCRIPTS = $(rules_dll)
maintainability_rules_DATA = 

maintainability_documentationdir=$(prefix)/lib/monodoc/sources
maintainability_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Maintainability.csproj Gendarme.Rules.Maintainability.mdp
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AvoidComplexMethodsRule.cs \
	AvoidDeepInheritanceTreeRule.cs \
	AvoidLackOfCohesionOfMethodsRule.cs \
	AvoidUnnecessarySpecializationRule.cs \
	ConsiderUsingStopwatchRule.cs \
	PreferStringIsNullOrEmptyRule.cs

tests_sources = \
	AvoidComplexMethodsTest.cs \
	AvoidDeepInheritanceTreeTest.cs \
	AvoidLackOfCohesionOfMethodsTest.cs \
	AvoidUnnecessarySpecializationTest.cs \
	ConsiderUsingStopwatchTest.cs \
	PreferStringIsNullOrEmptyTest.cs
