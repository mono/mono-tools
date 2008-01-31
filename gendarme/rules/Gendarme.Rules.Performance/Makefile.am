SUBDIRS=Test

performance_rulesdir=$(pkglibdir)
performance_rules_SCRIPTS = ../../bin/Gendarme.Rules.Performance.dll
EXTRA_DIST = $(performance_rules_sources) $(performance_rules_sources_in) Gendarme.Rules.Performance.xml \
	Gendarme.Rules.Performance.mdp Gendarme.Rules.Performance.csproj
CLEANFILES = $(performance_rules_SCRIPTS) $(performance_rules_SCRIPTS).mdb Test.Rules.Performance.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Performance.xml TestResult.xml

performance_rules_sources_in = ../../AssemblyInfo.cs.in
performance_rules_generated_sources = $(performance_rules_sources_in:.in=)
performance_rules_sources = EmptyDestructorRule.cs IDisposableWithDestructorWithoutSuppressFinalizeRule.cs \
	UseStringEmptyRule.cs AvoidUnusedParametersRule.cs AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUncalledPrivateCodeRule.cs AvoidToStringOnStringsRule.cs DontIgnoreMethodResultRule.cs \
	UsingStringLengthInsteadOfCheckingEmptyStringRule.cs AvoidReturningArraysOnPropertiesRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs

performance_rules_build_sources = $(addprefix $(srcdir)/, $(performance_rules_sources))
performance_rules_build_sources += $(performance_rules_generated_sources)

../../bin/Gendarme.Rules.Performance.dll: $(performance_rules_build_sources)
	$(MCS) -debug -target:library -langversion:linq -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(performance_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Performance.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Performance.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

performance_test_sources = EmptyDestructorTest.cs IDisposableWithDestructorWithoutSuppressFinalizeTest.cs \
	UseStringEmptyTest.cs AvoidUnusedParametersTest.cs UsingStringLengthInsteadOfCheckingEmptyStringTest.cs \
	AvoidUncalledPrivateCodeTest.cs AvoidToStringOnStringsTest.cs AvoidUninstantiatedInternalClassesTest.cs \
	DontIgnoreMethodResultTest.cs AvoidReturningArraysOnPropertiesTest.cs AvoidUnsealedConcreteAttributesTest.cs

performance_test_build_sources = $(addprefix $(srcdir)/Test/, $(performance_test_sources))

Test.Rules.Performance.dll: $(performance_rules_build_sources) $(performance_rules_SCRIPTS)
	$(MCS) -target:library -debug -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(performance_rules_SCRIPTS) -out:$@ $(performance_test_build_sources)

test: Test.Rules.Performance.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Performance.dll

self-test: $(performance_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(performance_rules_SCRIPTS)

