SUBDIRS=Test 

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Concurrency.dll
tests_dll = Test.Rules.Concurrency.dll

rules_doc_source = doc/Gendarme.Rules.Concurrency.source 
rules_doc_tree = doc/Gendarme.Rules.Concurrency.tree
rules_doc_zip = doc/Gendarme.Rules.Concurrency.zip

concurrency_rulesdir=$(prefix)/lib/gendarme
concurrency_rules_SCRIPTS = $(rules_dll)
concurrency_rules_DATA = 

concurrency_documentationdir=$(prefix)/lib/monodoc/sources
concurrency_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Concurrency.mdp Gendarme.Rules.Concurrency.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Concurrency.xml TestResult.xml

rules_sources = \
	DoNotLockOnThisOrTypesRule.cs \
	DoNotLockOnWeakIdentityObjectsRule.cs \
	DontUseLockedRegionOutsideMethodRule.cs \
	DoubleCheckLockingRule.cs \
	LockAnalyzerRule.cs \
	NonConstantStaticFieldsShouldNotBeVisibleRule.cs \
	WriteStaticFieldFromInstanceMethodRule.cs

tests_sources = \
	DoNotLockOnThisOrTypesTest.cs \
	DoNotLockOnWeakIdentityObjectsTest.cs \
	DontUseLockedRegionOutsideMethodTest.cs \
	DoubleCheckLockingTest.cs \
	NonConstantStaticFieldsShouldNotBeVisibleTest.cs \
	WriteStaticFieldFromInstanceMethodTest.cs
