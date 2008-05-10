testdir=.
test_SCRIPTS = 
test_DATA = 
EXTRA_DIST = $(test_sources) Test.Rules.mdp
CLEANFILES = Test.Rules.dll Test.Rules.dll.mdb
DISTCLEANFILES = Makefile.in

test_sources = \
	Definitions/SimpleMethods.cs \
	Definitions/SimpleTypes.cs \
	Fixtures/MethodRuleTestFixture.cs \
	Fixtures/RuleTestFixture.cs \
	Fixtures/TypeRuleTestFixture.cs \
	Helpers/AssemblyCache.cs \
	Helpers/DefinitionLoader.cs \
	Helpers/TestRunner.cs

Test.Rules.dll: $(test_sources)
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:$(CECIL_ASM) \
		-r:$(top_builddir)/gendarme/bin/Gendarme.Framework.dll -out:$@ $(test_sources)

all: Test.Rules.dll

test: Test.Rules.dll

run-test: test

self-test: $(rules_SCRIPTS)
