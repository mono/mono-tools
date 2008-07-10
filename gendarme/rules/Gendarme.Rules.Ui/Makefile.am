SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Smells.dll
tests_dll = Test.Rules.Smells.dll

ui_rulesdir=$(prefix)/lib/gendarme
ui_rules_SCRIPTS = $(rules_dll)
ui_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Ui.mdp Gendarme.Rules.Ui.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Ui.xml TestResult.xml

rules_sources = \
	ExecutableTargetRule.cs \
	GtkSharpExecutableTargetRule.cs \
	SystemWindowsFormsExecutableTargetRule.cs \
	UseSTAThreadAttributeOnSWFEntryPointsRule.cs

tests_sources = \
	ExecutableTargetTest.cs \
	GtkSharpExecutableTargetTest.cs \
	SystemWindowsFormsExecutableTargetTest.cs \
	UseSTAThreadAttributeOnSWFEntryPointsTest.cs
