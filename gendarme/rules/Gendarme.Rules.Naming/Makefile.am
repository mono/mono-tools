SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Naming.dll
tests_dll = Test.Rules.Naming.dll

rules_doc_source = doc/Gendarme.Rules.Naming.source 
rules_doc_tree = doc/Gendarme.Rules.Naming.tree
rules_doc_zip = doc/Gendarme.Rules.Naming.zip

naming_rulesdir=$(prefix)/lib/gendarme
naming_rules_SCRIPTS = $(rules_dll)
naming_rules_DATA = 

naming_documentationdir=$(prefix)/lib/monodoc/sources
naming_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Naming.mdp Gendarme.Rules.Naming.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Naming.xml TestResult.xml

rules_sources = \
	AvoidDeepNamespaceHierarchyRule.cs \
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
	AvoidDeepNamespaceHierarchyTest.cs \
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
