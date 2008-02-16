SUBDIRS=Test

portability_rulesdir=$(prefix)/lib/gendarme
portability_rules_SCRIPTS = ../../bin/Gendarme.Rules.Portability.dll
portability_rules_DATA = 
EXTRA_DIST = $(portability_rules_sources) $(portability_rules_sources_in) \
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
	$(GMCS) -debug -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:dotnet -r:ICSharpCode.SharpZipLib.dll -r:../../bin/Gendarme.Framework.dll -out:$@ $(portability_rules_build_sources)

portability_test_sources = NewLineLiteralTest.cs FeatureRequiresRootPrivilegeOnUnixTest.cs \
	MonoCompatibilityReviewTest.cs ExitCodeIsLimitedOnUnixTest.cs
portability_test_build_sources = $(addprefix $(srcdir)/Test/, $(portability_test_sources))

Test.Rules.Portability.dll: $(portability_test_build_sources) $(portability_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(portability_rules_SCRIPTS) -out:$@ $(portability_test_build_sources)

test: Test.Rules.Portability.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Portability.dll

self-test: $(portability_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(portability_rules_SCRIPTS)

