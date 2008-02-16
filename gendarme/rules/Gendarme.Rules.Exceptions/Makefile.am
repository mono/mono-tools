SUBDIRS=Test

exceptions_rulesdir=$(prefix)/lib/gendarme
exceptions_rules_SCRIPTS = ../../bin/Gendarme.Rules.Exceptions.dll
exceptions_rules_DATA = 
EXTRA_DIST = $(exceptions_rules_sources) $(exceptions_rules_sources_in) \
	Gendarme.Rules.Exceptions.mdp Gendarme.Rules.Exceptions.csproj
CLEANFILES = $(exceptions_rules_SCRIPTS) $(exceptions_rules_SCRIPTS).mdb Test.Rules.Exceptions.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Exceptions.xml TestResult.xml

exceptions_rules_sources_in = ../../AssemblyInfo.cs.in
exceptions_rules_generated_sources = $(exceptions_rules_sources_in:.in=)
exceptions_rules_sources = DoNotDestroyStackTraceRule.cs ISEHCatchBlock.cs \
	ISEHGuardedBlock.cs ISEHHandlerBlock.cs \
	SEHHandlerType.cs Impl/ExceptionBlockParser.cs Impl/ExecutionBlock.cs \
	Impl/ExecutionPath.cs Impl/ExecutionPathFactory.cs Impl/SEHCatchBlock.cs \
	Impl/SEHGuardedBlock.cs Impl/SEHHandlerBlock.cs Impl/SEHHandlerBlockCollection.cs \
	DontSwallowErrorsCatchingNonspecificExceptionsRule.cs

exceptions_rules_build_sources = $(addprefix $(srcdir)/, $(exceptions_rules_sources))
exceptions_rules_build_sources += $(exceptions_rules_generated_sources)

../../bin/Gendarme.Rules.Exceptions.dll: $(exceptions_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(exceptions_rules_build_sources)

exceptions_test_sources = DontDestroyStackTraceTest.cs DontSwallowErrorsCatchingNonspecificExceptionsTest.cs 
exceptions_test_build_sources = $(addprefix $(srcdir)/Test/, $(exceptions_test_sources))

Test.Rules.Exceptions.dll: $(exceptions_test_build_sources) $(exceptions_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(exceptions_rules_SCRIPTS) -out:$@ $(exceptions_test_build_sources)

test: Test.Rules.Exceptions.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Exceptions.dll

self-test: $(exceptions_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(exceptions_rules_SCRIPTS)

