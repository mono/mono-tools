SUBDIRS=Test

smells_rulesdir=$(pkglibdir)
smells_rules_SCRIPTS = ../../bin/Gendarme.Rules.Smells.dll
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
	$(MCS) -debug -target:library -langversion:linq -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll \
	-r:../../bin/Gendarme.Rules.Performance.dll -out:$@ $(smells_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Smells.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	rm -f $(DESTDIR)$(pkglibdir)/`basename Gendarme.Rules.Smells.xml`;
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

smells_test_sources =  AvoidCodeDuplicatedInSameClassTest.cs AvoidCodeDuplicatedInSiblingClassesTest.cs \
AvoidLargeClassesTest.cs AvoidLongMethodsTest.cs AvoidLongParameterListsTest.cs \
AvoidSpeculativeGeneralityTest.cs

smells_test_build_sources = $(addprefix $(srcdir)/Test/, $(smells_test_sources))

Test.Rules.Smells.dll: $(smells_rules_build_sources) $(smells_rules_SCRIPTS)
	$(MCS) -target:library -langversion:linq -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(smells_rules_SCRIPTS) -r:System.Windows.Forms.dll -out:$@ $(smells_test_build_sources)

test: Test.Rules.Smells.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Smells.dll

self-test: $(smells_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(smells_rules_SCRIPTS)

