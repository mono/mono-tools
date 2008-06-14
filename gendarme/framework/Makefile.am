frameworkdir=$(prefix)/lib/gendarme
framework_SCRIPTS = ../bin/Gendarme.Framework.dll
EXTRA_DIST = $(framework_sources) $(framework_sources_in) framework.mdp Gendarme.Framework.csproj gendarme-framework.pc.in
CLEANFILES = $(framework_SCRIPTS) $(framework_SCRIPTS).mdb Test.Framework.dll Gendarme.Framework.doc.xml
DISTCLEANFILES = ../AssemblyInfo.cs Makefile.in gendarme-framework.pc

pkgconfig_in_files = gendarme-framework.pc.in

pkgconfigdir= $(prefix)/lib/pkgconfig
pkgconfig_DATA = $(pkgconfig_in_files:.pc.in=.pc)

framework_sources_in = ../AssemblyInfo.cs.in
framework_generated_sources = $(framework_sources_in:.in=)
framework_sources = \
	../AssemblyStaticInfo.cs \
	Gendarme.Framework/AssemblyResolver.cs \
	Gendarme.Framework/BasicIgnoreList.cs \
	Gendarme.Framework/Confidence.cs \
	Gendarme.Framework/Defect.cs \
	Gendarme.Framework/DocumentationUriAttribute.cs \
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
	Gendarme.Framework/Symbols.cs \
	Gendarme.Framework/SolutionAttribute.cs \
	Gendarme.Framework/ApplicabilityScope.cs \
	Gendarme.Framework.Helpers/MethodSignature.cs \
	Gendarme.Framework.Helpers/MethodSignatures.cs \
	Gendarme.Framework.Helpers/StackEntryAnalysis.cs \
	Gendarme.Framework.Helpers/StackEntryUsageResult.cs \
	Gendarme.Framework.Rocks/AssemblyRocks.cs \
	Gendarme.Framework.Rocks/CecilRocks.cs \
	Gendarme.Framework.Rocks/CommonRocks.cs \
	Gendarme.Framework.Rocks/CustomAttributeRocks.cs \
	Gendarme.Framework.Rocks/FieldRocks.cs \
	Gendarme.Framework.Rocks/InstructionRocks.cs \
	Gendarme.Framework.Rocks/MethodRocks.cs \
	Gendarme.Framework.Rocks/ModuleRocks.cs \
	Gendarme.Framework.Rocks/TypeRocks.cs

framework_build_sources = $(addprefix $(srcdir)/, $(framework_sources))
framework_build_sources += $(framework_generated_sources)

../bin/:
	mkdir ../bin

../bin/Gendarme.Framework.dll: ../bin/ $(framework_build_sources) $(CECIL_ASM)
	$(GMCS) -debug -target:library  -doc:Gendarme.Framework.doc.xml -r:$(CECIL_ASM) \
		-out:$@ $(framework_build_sources)

framework_test_sources = \
	Gendarme.Framework.Helpers/MethodSignatureTest.cs \
	Gendarme.Framework.Helpers/StackEntryAnalysisTest.cs \
	Gendarme.Framework.Rocks/RocksTest.cs \
	Gendarme.Framework.Rocks/AssemblyRocksTest.cs \
	Gendarme.Framework.Rocks/CustomAttributeRocksTest.cs \
	Gendarme.Framework.Rocks/InstructionRocksTest.cs \
	Gendarme.Framework.Rocks/MethodRocksTest.cs \
	Gendarme.Framework.Rocks/TypeRocksTest.cs \
	Gendarme.Framework.Rocks/FieldRocksTest.cs
framework_test_build_sources = $(addprefix $(srcdir)/Test/, $(framework_test_sources))

Test.Framework.dll: $(framework_test_build_sources) $(framework_rules_SCRIPTS) ../bin/Gendarme.Framework.dll $(CECIL_ASM)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:../bin/Gendarme.Framework.dll \
		-out:$@ $(framework_test_build_sources)

test: Test.Framework.dll

run-test: test
	MONO_PATH=../bin/:$(MONO_PATH) nunit-console2 Test.Framework.dll


self-test: $(framework_SCRIPTS)
	mono --debug ../bin/gendarme.exe $(framework_SCRIPTS)
