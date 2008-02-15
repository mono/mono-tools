SUBDIRS=Test

performance_rulesdir=$(prefix)/lib/gendarme
performance_rules_SCRIPTS = ../../bin/Gendarme.Rules.Performance.dll
performance_rules_DATA = Gendarme.Rules.Performance.xml
EXTRA_DIST = $(performance_rules_sources) $(performance_rules_sources_in) Gendarme.Rules.Performance.xml \
	Gendarme.Rules.Performance.mdp Gendarme.Rules.Performance.csproj
CLEANFILES = $(performance_rules_SCRIPTS) $(performance_rules_SCRIPTS).mdb Test.Rules.Performance.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Performance.xml TestResult.xml

performance_rules_sources_in = ../../AssemblyInfo.cs.in
performance_rules_generated_sources = $(performance_rules_sources_in:.in=)
performance_rules_sources = \
	EmptyDestructorRule.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeRule.cs \
	UseStringEmptyRule.cs \
	AvoidUnusedParametersRule.cs \
	AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUncalledPrivateCodeRule.cs \
	AvoidToStringOnStringsRule.cs \
	DontIgnoreMethodResultRule.cs \
	UsingStringLengthInsteadOfCheckingEmptyStringRule.cs \
	AvoidReturningArraysOnPropertiesRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs \
	UseIsOperatorRule.cs \
	AvoidUnsealedUninheritedInternalClassesRule.cs

performance_rules_build_sources = $(addprefix $(srcdir)/, $(performance_rules_sources))
performance_rules_build_sources += $(performance_rules_generated_sources)

../../bin/Gendarme.Rules.Performance.dll: $(performance_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:../../bin/Gendarme.Framework.dll -out:$@ $(performance_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

performance_test_sources = \
	EmptyDestructorTest.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeTest.cs \
	UseStringEmptyTest.cs \
	AvoidUnusedParametersTest.cs \
	UsingStringLengthInsteadOfCheckingEmptyStringTest.cs \
	AvoidUncalledPrivateCodeTest.cs \
	AvoidToStringOnStringsTest.cs \
	AvoidUninstantiatedInternalClassesTest.cs \
	DontIgnoreMethodResultTest.cs \
	AvoidReturningArraysOnPropertiesTest.cs \
	AvoidUnsealedConcreteAttributesTest.cs \
	UseIsOperatorTest.cs \
	AvoidUnsealedUninheritedInternalClassesTest.cs

performance_test_build_sources = $(addprefix $(srcdir)/Test/, $(performance_test_sources))

Test.Rules.Performance.dll: $(performance_rules_build_sources) $(performance_rules_SCRIPTS)
	$(GMCS) -target:library -debug -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(performance_rules_SCRIPTS) -out:$@ $(performance_test_build_sources)

test: Test.Rules.Performance.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Performance.dll

self-test: $(performance_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(performance_rules_SCRIPTS)

