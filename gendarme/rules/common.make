console_runner=../../bin/gendarme.exe
framework=../../bin/Gendarme.Framework.dll
common_tests=../Test.Rules/Test.Rules.dll


rules_sources_in = ../../AssemblyInfo.cs.in
rules_generated_sources = $(rules_sources_in:.in=)
rules_build_sources = $(addprefix $(srcdir)/, $(rules_sources))
rules_build_sources += $(rules_generated_sources)

$(rules_dll): $(rules_build_sources) $(framework)
	$(GMCS) -debug -target:library $(EXTRA_RULES_OPTIONS) -r:$(CECIL_ASM) -r:$(framework) -out:$@ $(rules_build_sources)


tests_build_sources = $(addprefix $(srcdir)/Test/, $(tests_sources))

$(tests_dll): $(test_build_sources) $(rules_dll)
	$(GMCS) -debug -target:library $(EXTRA_TESTS_OPTIONS) -r:$(CECIL_ASM) -r:$(framework) \
		-r:$(rules_dll) -r:$(common_tests) -pkg:mono-nunit -out:$@ $(tests_build_sources)

test: $(tests_dll)

run-test: test
	MONO_PATH=../../bin/:../Test.Rules/:$(MONO_PATH) nunit-console2 $(tests_dll)


self-test: $(rules_dll)
	mono --debug $(console_runner) $(rules_dll)
