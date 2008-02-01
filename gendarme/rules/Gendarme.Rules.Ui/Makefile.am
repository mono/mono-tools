SUBDIRS=Test

ui_rulesdir=$(prefix)/lib/gendarme
ui_rules_SCRIPTS = ../../bin/Gendarme.Rules.Ui.dll
ui_rules_DATA = Gendarme.Rules.Ui.xml
EXTRA_DIST = $(ui_rules_sources) $(ui_rules_sources_in) Gendarme.Rules.Ui.xml \
	Gendarme.Rules.Ui.mdp Gendarme.Rules.Ui.csproj
CLEANFILES = $(ui_rules_SCRIPTS) $(ui_rules_SCRIPTS).mdb Test.Rules.Ui.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Ui.xml TestResult.xml

ui_rules_sources_in = ../../AssemblyInfo.cs.in
ui_rules_generated_sources = $(ui_rules_sources_in:.in=)
ui_rules_sources = ExecutableTargetRule.cs GtkSharpExecutableTargetRule.cs \
	SystemWindowsFormsExecutableTargetRule.cs UseSTAThreadAttributeOnSWFEntryPointsRule.cs

ui_rules_build_sources = $(addprefix $(srcdir)/, $(ui_rules_sources))
ui_rules_build_sources += $(ui_rules_generated_sources)

../../bin/Gendarme.Rules.Ui.dll: $(ui_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:../../bin/Gendarme.Framework.dll -out:$@ $(ui_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

ui_test_sources = ExecutableTargetTest.cs GtkSharpExecutableTargetTest.cs \
	SystemWindowsFormsExecutableTargetTest.cs UseSTAThreadAttributeOnSWFEntryPointsTest.cs

ui_test_build_sources = $(addprefix $(srcdir)/Test/, $(ui_test_sources))

Test.Rules.Ui.dll: $(ui_rules_build_sources) $(ui_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(ui_rules_SCRIPTS) -out:$@ $(ui_test_build_sources)

test: Test.Rules.Ui.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Ui.dll

self-test: $(ui_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(ui_rules_SCRIPTS)

