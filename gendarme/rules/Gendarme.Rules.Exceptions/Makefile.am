SUBDIRS=Test

exceptions_rulesdir=$(pkglibdir)
exceptions_rules_SCRIPTS = ../../bin/Gendarme.Rules.Exceptions.dll
EXTRA_DIST = $(exceptions_rules_sources) $(exceptions_rules_sources_in) Gendarme.Rules.Exceptions.xml \
	Gendarme.Rules.Exceptions.mdp Gendarme.Rules.Exceptions.csproj
CLEANFILES = $(exceptions_rules_SCRIPTS) $(exceptions_rules_SCRIPTS).mdb Test.Rules.Exceptions.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Exceptions.xml TestResult.xml

exceptions_rules_sources_in = ../../AssemblyInfo.cs.in
exceptions_rules_generated_sources = $(exceptions_rules_sources_in:.in=)
exceptions_rules_sources = DontDestroyStackTrace.cs ISEHCatchBlock.cs \
	ISEHGuardedBlock.cs ISEHHandlerBlock.cs \
	SEHHandlerType.cs Impl/ExceptionBlockParser.cs Impl/ExecutionBlock.cs \
	Impl/ExecutionPath.cs Impl/ExecutionPathFactory.cs Impl/SEHCatchBlock.cs \
	Impl/SEHGuardedBlock.cs Impl/SEHHandlerBlock.cs Impl/SEHHandlerBlockCollection.cs \
	DontSwallowErrorsCatchingNonspecificExceptionsRule.cs

exceptions_rules_build_sources = $(addprefix $(srcdir)/, $(exceptions_rules_sources))
exceptions_rules_build_sources += $(exceptions_rules_generated_sources)

../../bin/Gendarme.Rules.Exceptions.dll: $(exceptions_rules_build_sources)
	$(MCS) -debug -target:library -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(exceptions_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Exceptions.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	rm -f $(DESTDIR)$(pkglibdir)/`basename Gendarme.Rules.Exceptions.xml`;
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

exceptions_test_sources = TestPatterns.cs DontSwallowErrorsCatchingNonspecificExceptionsTest.cs 
exceptions_test_build_sources = $(addprefix $(srcdir)/Test/, $(exceptions_test_sources))

Test.Rules.Exceptions.dll: $(exceptions_test_build_sources) $(exceptions_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(exceptions_rules_SCRIPTS) -out:$@ $(exceptions_test_build_sources)

test: Test.Rules.Exceptions.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Exceptions.dll

self-test: $(exceptions_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(exceptions_rules_SCRIPTS)

