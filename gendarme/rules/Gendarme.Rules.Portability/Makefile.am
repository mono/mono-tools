include ../common.make

EXTRA_RULES_OPTIONS += -r:ICSharpCode.SharpZipLib.dll -r:System.Web.Services.dll

rules_sources =  \
	DoNotHardcodePathsRule.cs \
	ExitCodeIsLimitedOnUnixRule.cs \
	FeatureRequiresRootPrivilegeOnUnixRule.cs \
	MoMAWebService.cs \
	MonoCompatibilityReviewRule.cs \
	NewLineLiteralRule.cs

EXTRA_TESTS_OPTIONS += -o-

tests_sources = \
	DoNotHardcodePathsTest.cs \
	ExitCodeIsLimitedOnUnixTest.cs \
	FeatureRequiresRootPrivilegeOnUnixTest.cs \
	MonoCompatibilityReviewTest.cs \
	NewLineLiteralTest.cs
