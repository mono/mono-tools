include ../common.make

EXTRA_RULES_OPTIONS += -r:ICSharpCode.SharpZipLib.dll -pkg:dotnet

rules_sources =  \
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
