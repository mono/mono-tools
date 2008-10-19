SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Security.dll
tests_dll = Test.Rules.Security.dll

rules_doc_source = doc/Gendarme.Rules.Security.source 
rules_doc_tree = doc/Gendarme.Rules.Security.tree
rules_doc_zip = doc/Gendarme.Rules.Security.zip

security_rulesdir=$(prefix)/lib/gendarme
security_rules_SCRIPTS = $(rules_dll)
security_rules_DATA = 

security_documentationdir=$(prefix)/lib/monodoc/sources
security_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Security.mdp Gendarme.Rules.Security.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Security.xml TestResult.xml

rules_sources = \
	ArrayFieldsShouldNotBeReadOnlyRule.cs \
	NativeFieldsShouldNotBeVisibleRule.cs \
	StaticConstructorsShouldBePrivateRule.cs

tests_sources = \
	ArrayFieldsShouldNotBeReadOnlyTest.cs \
	NativeFieldsShouldNotBeVisibleTest.cs \
	StaticConstructorsShouldBePrivateTest.cs 
