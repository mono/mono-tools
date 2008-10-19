SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Portability.dll
tests_dll = Test.Rules.Portability.dll

rules_doc_source = doc/Gendarme.Rules.Portability.source 
rules_doc_tree = doc/Gendarme.Rules.Portability.tree
rules_doc_zip = doc/Gendarme.Rules.Portability.zip

portability_rulesdir=$(prefix)/lib/gendarme
portability_rules_SCRIPTS = $(rules_dll)
portability_rules_DATA = 

portability_documentationdir=$(prefix)/lib/monodoc/sources
portability_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Portability.mdp Gendarme.Rules.Portability.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Portability.xml TestResult.xml

EXTRA_RULES_OPTIONS = -r:ICSharpCode.SharpZipLib.dll -pkg:dotnet

rules_sources = \
	DoNotHardcodePathsRule.cs \
	ExitCodeIsLimitedOnUnixRule.cs \
	FeatureRequiresRootPrivilegeOnUnixRule.cs \
	MoMAWebService.cs \
	MonoCompatibilityReviewRule.cs \
	NewLineLiteralRule.cs

tests_sources = \
	DoNotHardcodePathsTest.cs \
	ExitCodeIsLimitedOnUnixTest.cs \
	FeatureRequiresRootPrivilegeOnUnixTest.cs \
	MonoCompatibilityReviewTest.cs \
	NewLineLiteralTest.cs
