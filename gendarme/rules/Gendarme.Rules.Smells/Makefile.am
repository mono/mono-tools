SUBDIRS=Test

smells_rulesdir=$(prefix)/lib/gendarme
smells_rules_SCRIPTS = ../../bin/Gendarme.Rules.Smells.dll
smells_rules_DATA = Gendarme.Rules.Smells.xml
EXTRA_DIST = $(smells_rules_sources) $(smells_rules_sources_in) Gendarme.Rules.Smells.xml \
	Gendarme.Rules.Smells.mdp 
CLEANFILES = $(smells_rules_SCRIPTS) $(smells_rules_SCRIPTS).mdb Test.Rules.Smells.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Smells.xml TestResult.xml

smells_rules_sources_in = ../../AssemblyInfo.cs.in
smells_rules_generated_sources = $(smells_rules_sources_in:.in=)
smells_rules_sources = AvoidCodeDuplicatedInSameClassRule.cs AvoidCodeDuplicatedInSiblingClassesRule.cs \
AvoidLargeClassesRule.cs AvoidLongMethodsRule.cs AvoidLongParameterListsRule.cs \
AvoidSpeculativeGeneralityRule.cs CodeDuplicatedLocator.cs Expression.cs \
ExpressionFillerVisitor.cs Utilities.cs

smells_rules_build_sources = $(addprefix $(srcdir)/, $(smells_rules_sources))
smells_rules_build_sources += $(smells_rules_generated_sources)

../../bin/Gendarme.Rules.Smells.dll: $(smells_rules_build_sources)
	$(GMCS) -debug -target:library -langversion:linq -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:../../bin/Gendarme.Framework.dll \
	-r:../../bin/Gendarme.Rules.Performance.dll -out:$@ $(smells_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

smells_test_sources =  AvoidCodeDuplicatedInSameClassTest.cs AvoidCodeDuplicatedInSiblingClassesTest.cs \
AvoidLargeClassesTest.cs AvoidLongMethodsTest.cs AvoidLongParameterListsTest.cs \
AvoidSpeculativeGeneralityTest.cs

smells_test_build_sources = $(addprefix $(srcdir)/Test/, $(smells_test_sources))

Test.Rules.Smells.dll: $(smells_rules_build_sources) $(smells_rules_SCRIPTS)
	$(GMCS) -target:library -langversion:linq -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(smells_rules_SCRIPTS) -r:System.Windows.Forms.dll -out:$@ $(smells_test_build_sources)

test: Test.Rules.Smells.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Smells.dll

self-test: $(smells_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(smells_rules_SCRIPTS)

