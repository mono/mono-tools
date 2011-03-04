include ../../options.make

testdir=.
test_SCRIPTS = 
test_DATA = 

framework=../../bin/Gendarme.Framework.dll
common_tests=Test.Rules.dll

EXTRA_DIST = $(test_sources) Test.Rules.csproj
CLEANFILES = Test.Rules.dll Test.Rules.dll.mdb $(common_tests)
DISTCLEANFILES = Makefile.in

test_sources =  \
	Definitions/SimpleMethods.cs \
	Definitions/SimpleTypes.cs \
	Fixtures/AssemblyRuleTestFixture.cs \
	Fixtures/MethodRuleTestFixture.cs \
	Fixtures/RuleTestFixture.cs \
	Fixtures/TypeRuleTestFixture.cs \
	Helpers/AssemblyCache.cs \
	Helpers/DefinitionLoader.cs \
	Helpers/TestRunner.cs

test_build_sources = $(addprefix $(srcdir)/, $(test_sources))

$(common_tests): $(test_build_sources) $(framework)
	$(MCS) -target:library $(TESTS_OPTIONS) -pkg:mono-nunit -r:$(CECIL_ASM) -r:$(framework) -out:$@ $(test_build_sources)

all: $(common_tests)

test: $(common_tests)

run-test: test

self-test:
