SUBDIRS=Test

concurrency_rulesdir=$(pkglibdir)
concurrency_rules_SCRIPTS = ../../bin/Gendarme.Rules.Concurrency.dll
EXTRA_DIST = $(concurrency_rules_sources) $(concurrency_rules_sources_in) Gendarme.Rules.Concurrency.xml \
	Gendarme.Rules.Concurrency.mdp Gendarme.Rules.Concurrency.csproj
CLEANFILES = $(concurrency_rules_SCRIPTS) $(concurrency_rules_SCRIPTS).mdb Test.Rules.Concurrency.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Concurrency.xml TestResult.xml

concurrency_rules_sources_in = ../../AssemblyInfo.cs.in
concurrency_rules_generated_sources = $(concurrency_rules_sources_in:.in=)
concurrency_rules_sources = DoubleCheckLockingRule.cs WriteStaticFieldFromInstanceMethodRule.cs \
	NonConstantStaticFieldsShouldNotBeVisibleRule.cs

concurrency_rules_build_sources = $(addprefix $(srcdir)/, $(concurrency_rules_sources))
concurrency_rules_build_sources += $(concurrency_rules_generated_sources)

../../bin/Gendarme.Rules.Concurrency.dll: $(concurrency_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(concurrency_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Concurrency.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Concurrency.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

concurrency_test_sources = DoubleCheckLockingTest.cs WriteStaticFieldFromInstanceMethodTest.cs \
	NonConstantStaticFieldsShouldNotBeVisibleTest.cs

concurrency_test_build_sources = $(addprefix $(srcdir)/Test/, $(concurrency_test_sources))

Test.Rules.Concurrency.dll: $(concurrency_rules_build_sources) $(concurrency_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(concurrency_rules_SCRIPTS) -out:$@ $(concurrency_test_build_sources)

test: Test.Rules.Concurrency.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Concurrency.dll

self-test: $(concurrency_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(concurrency_rules_SCRIPTS)

