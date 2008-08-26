SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Design.Generic.dll
tests_dll = Test.Rules.Design.Generic.dll

design_generic_rulesdir=$(prefix)/lib/gendarme
design_generic_rules_SCRIPTS = $(rules_dll)
design_generic_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in)
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	ImplementGenericCollectionInterfacesRule.cs

tests_sources = \
	ImplementGenericCollectionInterfacesTest.cs
