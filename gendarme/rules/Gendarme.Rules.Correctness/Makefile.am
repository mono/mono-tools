SUBDIRS=Test

correctness_rulesdir=$(pkglibdir)
correctness_rules_SCRIPTS = ../../bin/Gendarme.Rules.Correctness.dll
EXTRA_DIST = $(correctness_rules_sources) $(correctness_rules_sources_in) Gendarme.Rules.Correctness.xml \
	 Gendarme.Rules.Correctness.mdp  Gendarme.Rules.Correctness.csproj
CLEANFILES = $(correctness_rules_SCRIPTS) $(correctness_rules_SCRIPTS).mdb Test.Rules.Correctness.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Correctness.xml TestResult.xml

correctness_rules_sources_in = ../../AssemblyInfo.cs.in
correctness_rules_generated_sources = $(correctness_rules_sources_in:.in=)
correctness_rules_sources = BadRecursiveInvocationRule.cs MethodCanBeMadeStaticRule.cs \
	CallingEqualsWithNullArgRule.cs NonNullAttribute.cs NonNullAttributeCollector.cs NullDerefAnalysis.cs NullDerefRule.cs \
	NullDerefFrame.cs Nullity.cs graph/INode.cs graph/Node.cs graph/IEdge.cs graph/Edge.cs graph/IGraph.cs \
	graph/Graph.cs dataflow/BasicBlock.cs dataflow/CFG.cs dataflow/CFGEdge.cs dataflow/Dataflow.cs graph/DFS.cs \
	dataflow/IDataflowAnalysis.cs FloatComparisonRule.cs UseValueInPropertySetterRule.cs \
	AvoidConstructorsInStaticTypesRule.cs DontCompareWithNaNRule.cs

correctness_rules_build_sources = $(addprefix $(srcdir)/, $(correctness_rules_sources))
correctness_rules_build_sources += $(correctness_rules_generated_sources)

../../bin/Gendarme.Rules.Correctness.dll: $(correctness_rules_build_sources)
	$(MCS) -debug -target:library  -pkg:mono-cecil -langversion:linq -r:../../bin/Gendarme.Framework.dll -out:$@ $(correctness_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Correctness.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Correctness.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

correctness_test_sources = BadRecursiveInvocationTest.cs MethodCanBeMadeStaticTest.cs UseValueInPropertySetterTest.cs \
	AvoidConstructorsInStaticTypesTest.cs CallingEqualsWithNullArgTest.cs FloatComparisonRuleTest.cs \
	DontCompareWithNaNTest.cs

correctness_test_build_sources = $(addprefix $(srcdir)/Test/, $(correctness_test_sources))

Test.Rules.Correctness.dll: $(correctness_rules_build_sources) $(correctness_rules_SCRIPTS)
	$(MCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(correctness_rules_SCRIPTS) -out:$@ $(correctness_test_build_sources)

test: Test.Rules.Correctness.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Correctness.dll

self-test: $(correctness_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(correctness_rules_SCRIPTS)

