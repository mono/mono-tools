SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Portability.dll
tests_dll = Test.Rules.Portability.dll

portability_rulesdir=$(prefix)/lib/gendarme
portability_rules_SCRIPTS = $(rules_dll)
portability_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Portability.mdp Gendarme.Rules.Portability.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Portability.xml TestResult.xml

EXTRA_RULES_OPTIONS = -r:ICSharpCode.SharpZipLib.dll -pkg:dotnet

rules_sources = \
	NewLineLiteralRule.cs \
	ExitCodeIsLimitedOnUnixRule.cs \
	FeatureRequiresRootPrivilegeOnUnixRule.cs \
	MoMAWebService.cs \
	MonoCompatibilityReviewRule.cs 

tests_sources = \
	NewLineLiteralTest.cs \
	ExitCodeIsLimitedOnUnixTest.cs \
	FeatureRequiresRootPrivilegeOnUnixTest.cs \
	MonoCompatibilityReviewTest.cs 
