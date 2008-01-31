SUBDIRS=Test

badpractice_rulesdir=$(pkglibdir)
badpractice_rules_SCRIPTS = ../../bin/Gendarme.Rules.BadPractice.dll
EXTRA_DIST = $(badpractice_rules_sources) $(badpractice_rules_sources_in) Gendarme.Rules.BadPractice.xml \
	Gendarme.Rules.BadPractice.mdp 
CLEANFILES = $(badpractice_rules_SCRIPTS) $(badpractice_rules_SCRIPTS).mdb Test.Rules.BadPractice.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.BadPractice.xml TestResult.xml

badpractice_rules_sources_in = ../../AssemblyInfo.cs.in
badpractice_rules_generated_sources = $(badpractice_rules_sources_in:.in=)
badpractice_rules_sources = CloneMethodShouldNotReturnNullRule.cs EqualShouldHandleNullArgRule.cs \
	ToStringReturnsNullRule.cs GetEntryAssemblyMayReturnNullRule.cs ConstructorShouldNotCallVirtualMethodsRule.cs \
	CheckNewExceptionWithoutThrowingRule.cs CheckNewThreadWithoutStartRule.cs

badpractice_rules_build_sources = $(addprefix $(srcdir)/, $(badpractice_rules_sources))
badpractice_rules_build_sources += $(badpractice_rules_generated_sources)

../../bin/Gendarme.Rules.BadPractice.dll: $(badpractice_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(badpractice_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.BadPractice.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	rm -f $(DESTDIR)$(pkglibdir)/`basename Gendarme.Rules.BadPractice.xml`;
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

badpractice_test_sources = CloneMethodShouldNotReturnNullTest.cs EqualShouldHandleNullArgTest.cs \
	ToStringReturnsNullTest.cs GetEntryAssemblyMayReturnNullTest.cs ConstructorShouldNotCallVirtualMethodsTest.cs \
	CheckNewExceptionWithoutThrowingTest.cs CheckNewThreadWithoutStartTest.cs

badpractice_test_build_sources = $(addprefix $(srcdir)/Test/, $(badpractice_test_sources))

Test.Rules.BadPractice.dll: $(badpractice_rules_build_sources) $(badpractice_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(badpractice_rules_SCRIPTS) -out:$@ $(badpractice_test_build_sources)

test: Test.Rules.BadPractice.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.BadPractice.dll

self-test: $(badpractice_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(badpractice_rules_SCRIPTS)

