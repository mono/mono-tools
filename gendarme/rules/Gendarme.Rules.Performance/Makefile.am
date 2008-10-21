include ../common.make

rules_sources = \
	AvoidLargeNumberOfLocalVariablesRule.cs \
	AvoidUnneededFieldInitializationRule.cs \
	EmptyDestructorRule.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeRule.cs \
	UseStringEmptyRule.cs \
	UseTypeEmptyTypesRule.cs \
	AvoidUnusedParametersRule.cs \
	AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUncalledPrivateCodeRule.cs \
	AvoidUnneededCallsOnStringRule.cs \
	DontIgnoreMethodResultRule.cs \
	CompareWithStringEmptyEfficientlyRule.cs \
	AvoidReturningArraysOnPropertiesRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs \
	UseIsOperatorRule.cs \
	AvoidUnsealedUninheritedInternalClassesRule.cs \
	OverrideValueTypeDefaultsRule.cs \
	ImplementEqualsTypeRule.cs \
	MathMinMaxCandidateRule.cs \
	AvoidLargeStructureRule.cs \
	RemoveUnusedLocalVariablesRule.cs \
	AvoidUnneededUnboxingRule.cs \
	ConsiderCustomAccessorsForNonVisibleEventsRule.cs \
	AvoidTypeGetTypeForConstantStringsRule.cs \
	AvoidRepetitiveCastsRule.cs \
	AvoidUnusedPrivateFieldsRule.cs \
	PreferLiteralOverInitOnlyFieldsRule.cs

# turn off optimization for tests since gmcs (unlike MS csc) now turns "" into String.Empty when optimizing is on
EXTRA_TESTS_OPTIONS = -unsafe -o-

tests_sources = \
	AvoidLargeNumberOfLocalVariablesTest.cs \
	AvoidUnneededFieldInitializationTest.cs \
	EmptyDestructorTest.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeTest.cs \
	UseStringEmptyTest.cs \
	UseTypeEmptyTypesTest.cs \
	AvoidUnusedParametersTest.cs \
	CompareWithStringEmptyEfficientlyTest.cs \
	AvoidUncalledPrivateCodeTest.cs \
	AvoidUnneededCallsOnStringTest.cs \
	AvoidUninstantiatedInternalClassesTest.cs \
	DontIgnoreMethodResultTest.cs \
	AvoidReturningArraysOnPropertiesTest.cs \
	AvoidUnsealedConcreteAttributesTest.cs \
	UseIsOperatorTest.cs \
	AvoidUnsealedUninheritedInternalClassesTest.cs \
	OverrideValueTypeDefaultsTest.cs \
	ImplementEqualsTypeTest.cs \
	MathMinMaxCandidateTest.cs \
	AvoidLargeStructureTest.cs \
	RemoveUnusedLocalVariablesTest.cs \
	AvoidUnneededUnboxingTest.cs \
	ConsiderCustomAccessorsForNonVisibleEventsTest.cs \
	AvoidTypeGetTypeForConstantStringsTest.cs \
	AvoidRepetitiveCastsTest.cs \
	AvoidUnusedPrivateFieldsTest.cs \
	PreferLiteralOverInitOnlyFieldsTest.cs
