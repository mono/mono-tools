SUBDIRS=Test 

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Design.Linq.dll
tests_dll = Test.Rules.Design.Linq.dll

rules_doc_source = doc/Gendarme.Rules.Design.Linq.source 
rules_doc_tree = doc/Gendarme.Rules.Design.Linq.tree
rules_doc_zip = doc/Gendarme.Rules.Design.Linq.zip

design_linq_rulesdir=$(prefix)/lib/gendarme
design_linq_rules_SCRIPTS = $(rules_dll)
design_linq_rules_DATA = 

design_linq_documentationdir=$(prefix)/lib/monodoc/sources
design_linq_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) 
# Gendarme.Rules.Design.Linq.mdp Gendarme.Rules.Design.Linq.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AvoidExtensionMethodOnSystemObjectRule.cs

tests_sources = \
	AvoidExtensionMethodOnSystemObjectTest.cs
