SUBDIRS=Test

naming_rulesdir=$(pkglibdir)
naming_rules_SCRIPTS = ../../bin/Gendarme.Rules.Naming.dll
EXTRA_DIST = $(naming_rules_sources) $(naming_rules_sources_in) Gendarme.Rules.Naming.xml \
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
	$(MCS) -debug -target:library -langversion:linq -pkg:mono-cecil -r:../../bin/Gendarme.Framework.dll -out:$@ $(naming_rules_build_sources)
	cp Gendarme.Rules.*.xml ../../bin/

# Install Unstable Mono Libraries (see configure.ac)

install-data-hook:
	$(INSTALL) -c -m 0644 Gendarme.Rules.Naming.xml $(DESTDIR)$(pkglibdir);
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	rm -f $(DESTDIR)$(pkglibdir)/`basename Gendarme.Rules.Naming.xml`;
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
	done;

naming_test_sources = EnumNotEndsWithEnumOrFlagsSuffixTest.cs \
	UsePluralNameInEnumFlagsTest.cs UseSingularNameInEnumsUnlessAreFlagsTest.cs \
	DetectNonAlphaNumericsInTypeNamesTest.cs UseCorrectCasingTest.cs UseCorrectPrefixTest.cs UseCorrectSuffixTest.cs \
	UsePreferredTermsTest.cs DoNotPrefixValuesWithEnumNameTest.cs DoNotUseReservedInEnumValueNamesTest.cs \
	ParameterNamesShouldMatchOverridenMethodTest.cs

naming_test_build_sources = $(addprefix $(srcdir)/Test/, $(naming_test_sources))

Test.Rules.Naming.dll: $(naming_rules_build_sources) $(naming_rules_SCRIPTS)
	$(MCS) -target:library -debug -pkg:mono-cecil -pkg:mono-nunit -r:../../bin/Gendarme.Framework.dll \
		-r:$(naming_rules_SCRIPTS) -out:$@ $(naming_test_build_sources)

test: Test.Rules.Naming.dll

run-test: test
	MONO_PATH=../../bin/:$(CECIL_PATH):$(MONO_PATH) nunit-console2 Test.Rules.Naming.dll

self-test: $(naming_rules_SCRIPTS)
	mono --debug ../../bin/gendarme.exe $(naming_rules_SCRIPTS)

