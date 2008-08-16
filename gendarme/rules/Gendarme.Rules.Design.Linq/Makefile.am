SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Design.Linq.dll
tests_dll = Test.Rules.Design.Linq.dll

design_linq_rulesdir=$(prefix)/lib/gendarme
design_linq_rules_SCRIPTS = $(rules_dll)
design_linq_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) 
# Gendarme.Rules.Design.Linq.mdp Gendarme.Rules.Design.Linq.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AvoidExtensionMethodOnSystemObjectRule.cs

tests_sources = \
	AvoidExtensionMethodOnSystemObjectTest.cs
