SUBDIRS=Test 

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Design.Generic.dll
tests_dll = Test.Rules.Design.Generic.dll

rules_doc_source = doc/Gendarme.Rules.Design.Generic.source 
rules_doc_tree = doc/Gendarme.Rules.Design.Generic.tree
rules_doc_zip = doc/Gendarme.Rules.Design.Generic.zip

design_generic_rulesdir=$(prefix)/lib/gendarme
design_generic_rules_SCRIPTS = $(rules_dll)
design_generic_rules_DATA = 

design_generic_documentationdir=$(prefix)/lib/monodoc/sources
design_generic_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in)
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AvoidMethodWithUnusedGenericTypeRule.cs \
	ImplementGenericCollectionInterfacesRule.cs \
	UseGenericEventHandlerRule.cs

tests_sources = \
	AvoidMethodWithUnusedGenericTypeTest.cs \
	ImplementGenericCollectionInterfacesTest.cs \
	UseGenericEventHandlerTest.cs
