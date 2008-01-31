SUBDIRS=Test

security_rulesdir=$(pkglibdir)
security_rules_SCRIPTS = ../../bin/Gendarme.Rules.Security.dll
EXTRA_DIST = $(security_rules_sources) $(security_rules_sources_in) Gendarme.Rules.Security.xml \
	Gendarme.Rules.Security.mdp Gendarme.Rules.Security.csproj
CLEANFILES = $(security_rules_SCRIPTS) $(security_rules_SCRIPTS).mdb Test.Rules.Security.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Security.xml TestResult.xml

security_rules_sources_in = ../../AssemblyInfo.cs.in
security_rules_generated_sources = $(security_rules_sources_in:.in=)
security_rules_sources = MethodCallWithSubsetLinkDemandRule.cs NonVirtualMethodWithInheritanceDemandRule.cs \
	SealedTypeWithInheritanceDemandRule.cs SecureGetObjectDataOverridesRule.cs \
	TypeExposeFieldsRule.cs TypeIsNotSubsetOfMethodSecurityRule.cs TypeLinkDemandRule.cs \
	StaticConstructorsShouldBePrivateRule.cs ArrayFieldsShouldNotBeReadOnlyRule.cs NativeFieldsShouldNotBeVisibleRule.cs

security_rules_build_sources = $(addprefix $(srcdir)/, $(security_rules_sources))
security_rules_build_sources += $(security_rules_generated_sources)

../../bin/Gendarme.Rules.Security.dll: $(security_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(security_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Security.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	rm -f $(DESTDIR)$(pkglibdir)/`basename Gendarme.Rules.Security.xml`;
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

security_test_sources = MethodCallWithSubsetLinkDemandTest.cs NonVirtualMethodWithInheritanceDemandTest.cs \
	SealedTypeWithInheritanceDemandTest.cs SecureGetObjectDataOverridesTest.cs \
	TypeExposeFieldsTest.cs TypeIsNotSubsetOfMethodSecurityTest.cs TypeLinkDemandTest.cs \
	StaticConstructorsShouldBePrivateTest.cs ArrayFieldsShouldNotBeReadOnlyTest.cs NativeFieldsShouldNotBeVisibleTest.cs

security_test_build_sources = $(addprefix $(srcdir)/Test/, $(security_test_sources))

Test.Rules.Security.dll: $(security_rules_build_sources) $(security_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(security_rules_SCRIPTS) -out:$@ $(security_test_build_sources)

test: Test.Rules.Security.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Security.dll

self-test: $(security_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(security_rules_SCRIPTS)

