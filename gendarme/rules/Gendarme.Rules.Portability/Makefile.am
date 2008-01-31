SUBDIRS=Test

portability_rulesdir=$(pkglibdir)
portability_rules_SCRIPTS = ../../bin/Gendarme.Rules.Portability.dll
EXTRA_DIST = $(portability_rules_sources) $(portability_rules_sources_in) Gendarme.Rules.Portability.xml \
	Gendarme.Rules.Portability.mdp Gendarme.Rules.Portability.csproj
CLEANFILES = $(portability_rules_SCRIPTS) $(portability_rules_SCRIPTS).mdb Test.Rules.Portability.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Portability.xml TestResult.xml

portability_rules_sources_in = ../../AssemblyInfo.cs.in
portability_rules_generated_sources = $(portability_rules_sources_in:.in=)
portability_rules_sources = NewLineLiteralRule.cs FeatureRequiresRootPrivilegeOnUnixRule.cs \
	MonoCompatibilityReviewRule.cs MoMAWebService.cs ExitCodeIsLimitedOnUnixRule.cs

portability_rules_build_sources = $(addprefix $(srcdir)/, $(portability_rules_sources))
portability_rules_build_sources += $(portability_rules_generated_sources)

../../bin/Gendarme.Rules.Portability.dll: $(portability_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -pkg:dotnet -r:ICSharpCode.SharpZipLib.dll -r:../../bin/Gendarme.Framework.dll -out:$@ $(portability_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Portability.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Portability.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

portability_test_sources = NewLineLiteralTest.cs FeatureRequiresRootPrivilegeOnUnixTest.cs \
	MonoCompatibilityReviewTest.cs ExitCodeIsLimitedOnUnixTest.cs
portability_test_build_sources = $(addprefix $(srcdir)/Test/, $(portability_test_sources))

Test.Rules.Portability.dll: $(portability_rules_build_sources) $(portability_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(portability_rules_SCRIPTS) -out:$@ $(portability_test_build_sources)

test: Test.Rules.Portability.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Portability.dll

self-test: $(portability_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(portability_rules_SCRIPTS)

