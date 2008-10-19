console_runner=../../bin/gendarme.exe
framework=../../bin/Gendarme.Framework.dll
common_tests=../Test.Rules/Test.Rules.dll


rules_sources_in = ../../AssemblyInfo.cs.in
rules_generated_sources = $(rules_sources_in:.in=)
rules_build_sources = $(addprefix $(srcdir)/, $(rules_sources))
rules_build_sources += $(rules_generated_sources)

rules_doc = $(rules_doc_zip) $(rules_doc_source) $(rules_doc_tree)
generated_doc = doc/generated/**/*.xml

rules_category: $(rules_dll) $(rules_doc)

$(rules_dll): $(rules_build_sources) $(framework)
	$(GMCS) -debug -target:library $(EXTRA_RULES_OPTIONS) -doc:$(rules_dll).doc -r:$(CECIL_ASM) -r:$(framework) -out:$@ $(rules_build_sources)

tests_build_sources = $(addprefix $(srcdir)/Test/, $(tests_sources))

$(tests_dll): $(test_build_sources) $(rules_dll)
	$(GMCS) -debug -target:library $(EXTRA_TESTS_OPTIONS) -r:$(CECIL_ASM) -r:$(framework) \
		-r:$(rules_dll) -r:$(common_tests) -pkg:mono-nunit -out:$@ $(tests_build_sources)

test: $(tests_dll)

run-test: test
	MONO_PATH=../../bin/:../Test.Rules/:$(MONO_PATH) nunit-console2 $(tests_dll)

self-test: $(rules_dll)
	mono --debug $(console_runner) $(rules_dll)

$(generated_doc): $(rules_dll)
	mdoc update -i $(rules_dll).doc -o doc/generated $(rules_dll)

$(rules_doc_zip): $(generated_doc)
	mdoc assemble -f ecma -o doc/`expr match "$(PWD)" '.*\(Gendarme.Rules.*\)'` doc/generated 

$(rules_doc_tree): $(generated_doc)
	
$(rules_doc_source):
	echo -e "<?xml version='1.0'?>\n<monodoc>\n\t<source provider='ecma' basefile='`expr match "$(PWD)" '.*\(Gendarme.Rules.*\)'`' path='ruleslib-`expr match "$(PWD)" '.*\(Gendarme.Rules.*\)'`'/>\n</monodoc>" > doc/`expr match "$(PWD)" '.*\(Gendarme.Rules.*\)'`.source 

clean-local:
	rm -fr doc/generated
