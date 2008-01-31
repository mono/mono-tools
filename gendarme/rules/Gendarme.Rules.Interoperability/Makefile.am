SUBDIRS=Test

interoperability_rulesdir=$(pkglibdir)
interoperability_rules_SCRIPTS = ../../bin/Gendarme.Rules.Interoperability.dll
EXTRA_DIST = $(interoperability_rules_sources) $(interoperability_rules_sources_in) Gendarme.Rules.Interoperability.xml \
	Gendarme.Rules.Interoperability.mdp 
CLEANFILES = $(interoperability_rules_SCRIPTS) $(interoperability_rules_SCRIPTS).mdb Test.Rules.Interoperability.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Interoperability.xml TestResult.xml

interoperability_rules_sources_in = ../../AssemblyInfo.cs.in
interoperability_rules_generated_sources = $(interoperability_rules_sources_in:.in=)
interoperability_rules_sources = PInvokeShouldNotBeVisibleRule.cs GetLastErrorMustBeCalledRightAfterPInvokeRule.cs \
	UseManagedAlternativesToPInvokeRule.cs MarshalStringsInPInvokeDeclarationsRule.cs

interoperability_rules_build_sources = $(addprefix $(srcdir)/, $(interoperability_rules_sources))
interoperability_rules_build_sources += $(interoperability_rules_generated_sources)

../../bin/Gendarme.Rules.Interoperability.dll: $(interoperability_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(interoperability_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Interoperability.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Interoperability.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

interoperability_test_sources = PInvokeShouldNotBeVisibleTest.cs GetLastErrorMustBeCalledRightAfterPInvokeTest.cs \
	UseManagedAlternativesToPInvokeTest.cs MarshalStringsInPInvokeDeclarationsTest.cs

interoperability_test_build_sources = $(addprefix $(srcdir)/Test/, $(interoperability_test_sources))

Test.Rules.Interoperability.dll: $(interoperability_rules_build_sources) $(interoperability_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(interoperability_rules_SCRIPTS) -out:$@ $(interoperability_test_build_sources)

test: Test.Rules.Interoperability.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Interoperability.dll

self-test: $(interoperability_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(interoperability_rules_SCRIPTS)

