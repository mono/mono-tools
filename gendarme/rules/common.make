SUBDIRS=Test

console_runner=../../bin/gendarme.exe
framework=../../bin/Gendarme.Framework.dll
common_tests=../Test.Rules/Test.Rules.dll

prefixed_rules_category = $(shell expr match "$(PWD)" '.*\(Gendarme.Rules.*\)')
rules_category = $(shell echo $(prefixed_rules_category) | cut -c 16-)

rules_dll = ../../bin/$(prefixed_rules_category).dll
tests_dll = Test.Rules.$(rules_category).dll
rules_doc_source = doc/$(prefixed_rules_category).source
rules_doc_tree = doc/$(prefixed_rules_category).tree
rules_doc_zip = doc/$(prefixed_rules_category).zip

rules_categorydir = $(prefix)/lib/gendarme
rules_category_SCRIPTS = $(rules_dll)

rules_documentationdir = $(prefix)/lib/monodoc/sources
rules_documentation_DATA = $(rules_doc)

rules_sources_in = ../../AssemblyInfo.cs.in
rules_generated_sources = $(rules_sources_in:.in=)
rules_build_sources = $(addprefix $(srcdir)/, $(rules_sources))
rules_build_sources += $(rules_generated_sources)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) $(prefixed_rules_category).mdp $(prefixed_rules_category).csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(tests_dll).mdb $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in $(prefixed_rules_category).xml TestResult.xml

rules_doc = $(rules_doc_zip) $(rules_doc_source) $(rules_doc_tree)
generated_doc = doc/generated/**/*.xml

$(rules_dll): $(rules_build_sources) $(framework)
	$(GMCS) -debug -target:library $(EXTRA_RULES_OPTIONS) -nowarn:1591 -doc:$(rules_dll).doc \
		-r:$(CECIL_ASM) -r:$(framework) -out:$@ $(rules_build_sources)

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
	test -d doc || mkdir doc
	mdoc update -i $(rules_dll).doc -o doc/generated $(rules_dll)

$(rules_doc_zip): $(generated_doc)
	mdoc assemble -f ecma -o doc/$(prefixed_rules_category) doc/generated 

$(rules_doc_tree): $(generated_doc)
	
$(rules_doc_source):
	echo -e "<?xml version='1.0'?>\n<monodoc>\n\t<source provider='ecma' basefile='$(prefixed_rules_category)' path='ruleslib-$(prefixed_rules_category)'/>\n</monodoc>" > $(rules_doc_source) 

clean-local:
	rm -fr doc
