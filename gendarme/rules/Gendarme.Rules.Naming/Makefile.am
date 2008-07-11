SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Naming.dll
tests_dll = Test.Rules.Naming.dll

naming_rulesdir=$(prefix)/lib/gendarme
naming_rules_SCRIPTS = $(rules_dll)
naming_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Naming.mdp Gendarme.Rules.Naming.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Naming.xml TestResult.xml

rules_sources = \
	DetectNonAlphaNumericsInTypeNamesRule.cs \
	DoNotPrefixEventsWithAfterOrBeforeRule.cs \
	DoNotPrefixValuesWithEnumNameRule.cs \
	DoNotUseReservedInEnumValueNamesRule.cs \
	EnumNotEndsWithEnumOrFlagsSuffixRule.cs \
	ParameterNamesShouldMatchOverridenMethodRule.cs \
	UseCorrectCasingRule.cs \
	UseCorrectPrefixRule.cs \
	UseCorrectSuffixRule.cs \
	UsePluralNameInEnumFlagsRule.cs \
	UseSingularNameInEnumsUnlessAreFlagsRule.cs \
	UsePreferredTermsRule.cs 

tests_sources = \
	DetectNonAlphaNumericsInTypeNamesTest.cs \
	DoNotPrefixEventsWithAfterOrBeforeTest.cs \
	DoNotPrefixValuesWithEnumNameTest.cs \
	DoNotUseReservedInEnumValueNamesTest.cs \
	EnumNotEndsWithEnumOrFlagsSuffixTest.cs \
	ParameterNamesShouldMatchOverridenMethodTest.cs \
	UsePluralNameInEnumFlagsTest.cs \
	UseSingularNameInEnumsUnlessAreFlagsTest.cs \
	UseCorrectCasingTest.cs \
	UseCorrectPrefixTest.cs \
	UseCorrectSuffixTest.cs \
	UsePreferredTermsTest.cs
