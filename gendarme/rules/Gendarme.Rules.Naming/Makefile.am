SUBDIRS=Test

naming_rulesdir=$(prefix)/lib/gendarme
naming_rules_SCRIPTS = ../../bin/Gendarme.Rules.Naming.dll
naming_rules_DATA = 
EXTRA_DIST = $(naming_rules_sources) $(naming_rules_sources_in) \
	Gendarme.Rules.Naming.mdp 
CLEANFILES = $(naming_rules_SCRIPTS)* Test.Rules.Naming.dll
DISTCLEANFILES = Makefile.in Gendarme.Rules.Naming.xml TestResult.xml

naming_rules_sources_in = ../../AssemblyInfo.cs.in
naming_rules_generated_sources = $(naming_rules_sources_in:.in=)
naming_rules_sources = EnumNotEndsWithEnumOrFlagsSuffixRule.cs \
	UsePluralNameInEnumFlagsRule.cs UseSingularNameInEnumsUnlessAreFlagsRule.cs \
	DetectNonAlphaNumericsInTypeNamesRule.cs UseCorrectCasingRule.cs UseCorrectPrefixRule.cs UseCorrectSuffixRule.cs \
	UsePreferredTermsRule.cs DoNotPrefixValuesWithEnumNameRule.cs DoNotUseReservedInEnumValueNamesRule.cs \
	ParameterNamesShouldMatchOverridenMethodRule.cs

naming_rules_build_sources = $(addprefix $(srcdir)/, $(naming_rules_sources))
naming_rules_build_sources += $(naming_rules_generated_sources)

../../bin/Gendarme.Rules.Naming.dll: $(naming_rules_build_sources)
	$(GMCS) -debug -target:library -r:$(CECIL_ASM) -r:../../bin/Gendarme.Framework.dll \
		-out:$@ $(naming_rules_build_sources)

naming_test_sources = EnumNotEndsWithEnumOrFlagsSuffixTest.cs \
	UsePluralNameInEnumFlagsTest.cs UseSingularNameInEnumsUnlessAreFlagsTest.cs \
	DetectNonAlphaNumericsInTypeNamesTest.cs UseCorrectCasingTest.cs UseCorrectPrefixTest.cs UseCorrectSuffixTest.cs \
	UsePreferredTermsTest.cs DoNotPrefixValuesWithEnumNameTest.cs DoNotUseReservedInEnumValueNamesTest.cs \
	ParameterNamesShouldMatchOverridenMethodTest.cs

naming_test_build_sources = $(addprefix $(srcdir)/Test/, $(naming_test_sources))

Test.Rules.Naming.dll: $(naming_test_build_sources) $(naming_rules_SCRIPTS)
	$(GMCS) -target:library -debug -r:$(CECIL_ASM) -pkg:mono-nunit \
		-r:../../bin/Gendarme.Framework.dll -r:$(naming_rules_SCRIPTS) -out:$@ $(naming_test_build_sources)

test: Test.Rules.Naming.dll

run-test: test
	MONO_PATH=../../bin/:$(MONO_PATH) nunit-console2 Test.Rules.Naming.dll

self-test: $(naming_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(naming_rules_SCRIPTS)

