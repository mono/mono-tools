SUBDIRS=Test

framework=../../bin/Gendarme.Framework.dll
serialization_rulesdir=$(prefix)/lib/gendarme
serialization_rules_SCRIPTS = ../../bin/Gendarme.Rules.Serialization.dll
serialization_rules_DATA = 
EXTRA_DIST = $(serialization_rules_sources) $(serialization_rules_sources_in) \
	Gendarme.Rules.Serialization.mdp Gendarme.Rules.Serialization.csproj
CLEANFILES = $(serialization_rules_SCRIPTS) $(serialization_rules_SCRIPTS).mdb Test.Rules.Serialization.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Serialization.xml TestResult.xml

serialization_rules_sources_in = ../../AssemblyInfo.cs.in
serialization_rules_generated_sources = $(serialization_rules_sources_in:.in=)
serialization_rules_sources = DeserializeOptionalFieldRule.cs MissingSerializableAttributeOnISerializableTypeRule.cs \
	MissingSerializationConstructorRule.cs UseCorrectSignatureForSerializationMethodsRule.cs \
	MarkAllNonSerializableFieldsRule.cs

serialization_rules_build_sources = $(addprefix $(srcdir)/, $(serialization_rules_sources))
serialization_rules_build_sources += $(serialization_rules_generated_sources)

../../bin/Gendarme.Rules.Serialization.dll: $(serialization_rules_build_sources) $(framework)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:$(framework) \
		-out:$@ $(serialization_rules_build_sources)

serialization_test_sources = DeserializeOptionalFieldTest.cs MissingSerializableAttributeOnISerializableTypeTest.cs \
	MissingSerializationConstructorTest.cs UseCorrectSignatureForSerializationMethodsTest.cs \
	MarkAllNonSerializableFieldsTest.cs

serialization_test_build_sources = $(addprefix $(srcdir)/Test/, $(serialization_test_sources))

Test.Rules.Serialization.dll: $(serialization_test_build_sources) $(serialization_rules_SCRIPTS)
	$(GMCS) -target:library -r:$(CECIL_ASM) -pkg:mono-nunit -r:$(framework) \
		-r:$(serialization_rules_SCRIPTS) -r:../Test.Rules/Test.Rules.dll -out:$@ $(serialization_test_build_sources)

test: Test.Rules.Serialization.dll

run-test: test
	MONO_PATH=../../bin/:../Test.Rules/:$(MONO_PATH) nunit-console2 Test.Rules.Serialization.dll

self-test: $(serialization_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(serialization_rules_SCRIPTS)
