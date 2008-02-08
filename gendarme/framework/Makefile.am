frameworkdir=$(prefix)/lib/gendarme
framework_SCRIPTS = ../bin/Gendarme.Framework.dll $(top_builddir)/gendarme/bin/Mono.Cecil.dll
EXTRA_DIST = $(framework_sources) $(framework_sources_in) framework.mdp Gendarme.Framework.csproj gendarme-framework.pc.in
CLEANFILES = $(framework_SCRIPTS) $(framework_SCRIPTS).mdb Test.Framework.dll Gendarme.Framework.doc.xml
DISTCLEANFILES = ../AssemblyInfo.cs Makefile.in gendarme-framework.pc

pkgconfig_in_files = gendarme-framework.pc.in

pkgconfigdir= $(prefix)/lib/pkgconfig
pkgconfig_DATA = $(pkgconfig_in_files:.pc.in=.pc)

framework_sources_in = ../AssemblyInfo.cs.in
framework_generated_sources = $(framework_sources_in:.in=)
framework_sources = AssemblyManager.cs IRule.cs RuleCollection.cs \
		    RuleInformation.cs  RuleInformationManager.cs \
		    Rules.cs Runner.cs Violation.cs Violations.cs \
		    Message.cs Location.cs MinimalRunner.cs \
		    MessageCollection.cs MessageType.cs \
		    Gendarme.Framework/MethodSignature.cs \
		    Gendarme.Framework/MethodSignatures.cs \
		    Gendarme.Framework/StackEntryAnalysis.cs \
		    Gendarme.Framework.Rocks/AssemblyRocks.cs \
		    Gendarme.Framework.Rocks/CommonRocks.cs \
		    Gendarme.Framework.Rocks/CustomAttributeRocks.cs \
		    Gendarme.Framework.Rocks/FieldRocks.cs \
		    Gendarme.Framework.Rocks/MethodRocks.cs \
		    Gendarme.Framework.Rocks/TypeRocks.cs

framework_build_sources = $(addprefix $(srcdir)/, $(framework_sources))
framework_build_sources += $(framework_generated_sources)

../bin/:
	mkdir ../bin

$(top_builddir)/gendarme/bin/Mono.Cecil.dll: ../bin/
	cp $(CECIL_ASM) $(top_builddir)/gendarme/bin/

../bin/Gendarme.Framework.dll: ../bin/ $(framework_build_sources) $(top_builddir)/gendarme/bin/Mono.Cecil.dll
	$(GMCS) -debug -target:library -doc:Gendarme.Framework.doc.xml -langversion:linq -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:System.Xml.dll -out:$@ $(framework_build_sources)

framework_test_sources = \
	Gendarme.Framework/MethodSignatureTest.cs \
	Gendarme.Framework/StackEntryAnalysisTest.cs \
	Gendarme.Framework.Rocks/RocksTest.cs \
	Gendarme.Framework.Rocks/AssemblyRocksTest.cs \
	Gendarme.Framework.Rocks/CustomAttributeRocksTest.cs \
	Gendarme.Framework.Rocks/MethodRocksTest.cs \
	Gendarme.Framework.Rocks/TypeRocksTest.cs \
	Gendarme.Framework.Rocks/FieldRocksTest.cs
framework_test_build_sources = $(addprefix $(srcdir)/Test/, $(framework_test_sources))

Test.Framework.dll: $(framework_test_build_sources) $(framework_rules_SCRIPTS) ../bin/Gendarme.Framework.dll $(top_builddir)/gendarme/bin/Mono.Cecil.dll
	$(GMCS) -debug -target:library -langversion:linq -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../bin/Gendarme.Framework.dll \
		-out:$@ $(framework_test_build_sources)

test: Test.Framework.dll

run-test: test
	MONO_PATH=../bin/:$(MONO_PATH) nunit-console2 Test.Framework.dll


self-test: $(framework_SCRIPTS)
	mono --debug ../bin/gendarme.exe $(framework_SCRIPTS)
