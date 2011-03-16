include ../options.make

SUBDIRS = Test

frameworkdir=$(prefix)/lib/gendarme
framework_SCRIPTS = ../bin/Gendarme.Framework.dll
EXTRA_DIST = $(framework_sources) $(framework_sources_in) $(framework_test_build_sources) \
	Gendarme.Framework.csproj gendarme-framework.pc.in
CLEANFILES = $(framework_SCRIPTS) $(framework_SCRIPTS).mdb Test.Framework.dll $(framework_SCRIPTS).doc $(framework_doc)
DISTCLEANFILES = ../AssemblyInfo.cs Makefile.in gendarme-framework.pc

pkgconfig_in_files = gendarme-framework.pc.in

pkgconfigdir= $(prefix)/lib/pkgconfig
pkgconfig_DATA = $(pkgconfig_in_files:.pc.in=.pc)

framework_doc_source = doc/Gendarme.Framework.source
framework_doc_tree = doc/Gendarme.Framework.tree
framework_doc_zip = doc/Gendarme.Framework.zip

framework_documentationdir = $(prefix)/lib/monodoc/sources
framework_documentation_DATA = $(framework_doc)

framework_doc = $(framework_doc_zip) $(framework_doc_source) $(framework_doc_tree)
generated_doc = doc/generated/index.xml

framework_sources_in = ../AssemblyInfo.cs.in
framework_generated_sources = $(framework_sources_in:.in=)
framework_sources =  \
	../AssemblyStaticInfo.cs \
	Gendarme.Framework.Engines/NamespaceEngine.cs \
	Gendarme.Framework.Engines/OpCodeEngine.cs \
	Gendarme.Framework.Engines/SuppressMessageEngine.cs \
	Gendarme.Framework.Helpers/Log.cs \
	Gendarme.Framework.Helpers/MethodPrinter.cs \
	Gendarme.Framework.Helpers/MethodSignature.cs \
	Gendarme.Framework.Helpers/MethodSignatures.cs \
	Gendarme.Framework.Helpers/Namespace.cs \
	Gendarme.Framework.Helpers/OpCodeBitmask.cs \
	Gendarme.Framework.Helpers/PrimitiveReferences.cs \
	Gendarme.Framework.Helpers/StackEntryAnalysis.cs \
	Gendarme.Framework.Helpers/StackEntryUsageResult.cs \
	Gendarme.Framework.Helpers/StreamLineReader.cs \
	Gendarme.Framework.Rocks/AssemblyRocks.cs \
	Gendarme.Framework.Rocks/CecilRocks.cs \
	Gendarme.Framework.Rocks/CommonRocks.cs \
	Gendarme.Framework.Rocks/CustomAttributeRocks.cs \
	Gendarme.Framework.Rocks/FieldRocks.cs \
	Gendarme.Framework.Rocks/InstructionRocks.cs \
	Gendarme.Framework.Rocks/MethodRocks.cs \
	Gendarme.Framework.Rocks/ModuleRocks.cs \
	Gendarme.Framework.Rocks/ParameterRocks.cs \
	Gendarme.Framework.Rocks/PropertyRocks.cs \
	Gendarme.Framework.Rocks/TypeRocks.cs \
	Gendarme.Framework.Rocks/VariableDefinitionRocks.cs \
	Gendarme.Framework/ApplicabilityScope.cs \
	Gendarme.Framework/AssemblyResolver.cs \
	Gendarme.Framework/BasicIgnoreList.cs \
	Gendarme.Framework/Bitmask.cs \
	Gendarme.Framework/Confidence.cs \
	Gendarme.Framework/Defect.cs \
	Gendarme.Framework/DocumentationUriAttribute.cs \
	Gendarme.Framework/Engine.cs \
	Gendarme.Framework/EngineController.cs \
	Gendarme.Framework/EngineDependencyAttribute.cs \
	Gendarme.Framework/EngineEventArgs.cs \
	Gendarme.Framework/FxCopCompatibilityAttribute.cs \
	Gendarme.Framework/HierarchicalEventArgs.cs \
	Gendarme.Framework/IAssemblyRule.cs \
	Gendarme.Framework/IIgnoreList.cs \
	Gendarme.Framework/IMethodRule.cs \
	Gendarme.Framework/IRule.cs \
	Gendarme.Framework/IRunner.cs \
	Gendarme.Framework/ITypeRule.cs \
	Gendarme.Framework/ProblemAttribute.cs \
	Gendarme.Framework/Rule.cs \
	Gendarme.Framework/RuleResult.cs \
	Gendarme.Framework/Runner.cs \
	Gendarme.Framework/RunnerEventArgs.cs \
	Gendarme.Framework/Severity.cs \
	Gendarme.Framework/SolutionAttribute.cs \
	Gendarme.Framework/Symbols.cs \
	Gendarme.Framework/ThreadModelAttribute.cs

framework_build_sources = $(addprefix $(srcdir)/, $(framework_sources))
framework_build_sources += $(framework_generated_sources)

../bin/Gendarme.Framework.dll: $(framework_build_sources) $(CECIL_ASM)
	test -d ../bin || mkdir ../bin
	$(MCS) $(GENDARME_OPTIONS) -target:library -doc:$(framework_SCRIPTS).doc -r:$(CECIL_ASM) \
		-out:$@ $(framework_build_sources)

framework_test_sources = \
	GlobalSuppressions.cs \
	Gendarme.Framework/BitmaskTest.cs \
	Gendarme.Framework/ThreadModelAttributeTest.cs \
	Gendarme.Framework.Engines/SuppressMessageAttributeTest.cs \
	Gendarme.Framework.Helpers/MethodSignatureTest.cs \
	Gendarme.Framework.Helpers/StackEntryAnalysisTest.cs \
	Gendarme.Framework.Rocks/RocksTest.cs \
	Gendarme.Framework.Rocks/AssemblyRocksTest.cs \
	Gendarme.Framework.Rocks/CustomAttributeRocksTest.cs \
	Gendarme.Framework.Rocks/InstructionRocksTest.cs \
	Gendarme.Framework.Rocks/MethodRocksTest.cs \
	Gendarme.Framework.Rocks/ParameterRocksTest.cs \
	Gendarme.Framework.Rocks/TypeRocksTest.cs \
	Gendarme.Framework.Rocks/VariableDefinitionRocksTest.cs \
	Gendarme.Framework.Rocks/FieldRocksTest.cs
framework_test_build_sources = $(addprefix $(srcdir)/Test/, $(framework_test_sources))

Test.Framework.dll: $(framework_test_build_sources) $(framework_rules_SCRIPTS) ../bin/Gendarme.Framework.dll $(CECIL_ASM)
	$(MCS) $(TESTS_OPTIONS) -target:library -d:CODE_ANALYSIS -pkg:mono-nunit \
		-r:$(CECIL_ASM) -r:../bin/Gendarme.Framework.dll -r:../rules/Test.Rules/Test.Rules.dll \
		-out:$@ $(framework_test_build_sources)

test: Test.Framework.dll

run-test: test
	MONO_PATH=../bin/:../rules/Test.Rules/:$(MONO_PATH) $(prefix)/bin/mono $(prefix)/lib/mono/4.0/nunit-console.exe Test.Framework.dll

self-test: $(framework_SCRIPTS)
	mono --debug ../bin/gendarme.exe $(framework_SCRIPTS)

$(generated_doc): $(framework_SCRIPTS)
	mkdir -p doc
	mdoc update -i $(framework_SCRIPTS).doc -o doc/generated $(framework_SCRIPTS)
	touch $(generated_doc)

$(framework_doc_zip): $(generated_doc)
	mdoc assemble -f ecma -o doc/Gendarme.Framework doc/generated 

$(framework_doc_tree): $(generated_doc)
	
$(framework_doc_source):
	echo -e "<?xml version='1.0'?>\n<monodoc>\n\t<node label='Gendarme Framework' name='classlib-Gendarme.Framework' parent='libraries' />\n\t<source provider='ecma' basefile='Gendarme.Framework' path='classlib-Gendarme.Framework'/>\n</monodoc>" > $(framework_doc_source) 

clean-local:
	rm -fr doc/
