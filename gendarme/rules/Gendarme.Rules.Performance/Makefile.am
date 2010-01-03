include ../common.make

rules_sources =  \
	AvoidLargeNumberOfLocalVariablesRule.cs \
	AvoidLargeStructureRule.cs \
	AvoidLocalDataStoreSlotRule.cs \
	AvoidMethodWithLargeMaximumStackSize.cs \
	AvoidRepetitiveCastsRule.cs \
	AvoidReturningArraysOnPropertiesRule.cs \
	AvoidTypeGetTypeForConstantStringsRule.cs \
	AvoidUncalledPrivateCodeRule.cs \
	AvoidUninstantiatedInternalClassesRule.cs \
	AvoidUnneededCallsOnStringRule.cs \
	AvoidUnneededFieldInitializationRule.cs \
	AvoidUnneededUnboxingRule.cs \
	AvoidUnsealedConcreteAttributesRule.cs \
	AvoidUnsealedUninheritedInternalClassesRule.cs \
	AvoidUnusedParametersRule.cs \
	AvoidUnusedPrivateFieldsRule.cs \
	CompareWithStringEmptyEfficientlyRule.cs \
	ConsiderCustomAccessorsForNonVisibleEventsRule.cs \
	DontIgnoreMethodResultRule.cs \
	EmptyDestructorRule.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeRule.cs \
	ImplementEqualsTypeRule.cs \
	MathMinMaxCandidateRule.cs \
	OverrideValueTypeDefaultsRule.cs \
	PreferCharOverloadRule.cs \
	PreferLiteralOverInitOnlyFieldsRule.cs \
	RemoveUnusedLocalVariablesRule.cs \
	ReviewLinqMethodRule.cs \
	UseIsOperatorRule.cs \
	UseStringEmptyRule.cs \
	UseTypeEmptyTypesRule.cs

# turn off optimization for tests since gmcs (unlike MS csc) now turns "" into String.Empty when optimizing is on
EXTRA_TESTS_OPTIONS += -unsafe -o-

tests_sources = \
	AvoidLargeNumberOfLocalVariablesTest.cs \
	AvoidLocalDataStoreSlotTest.cs \
	AvoidUnneededFieldInitializationTest.cs \
	EmptyDestructorTest.cs \
	IDisposableWithDestructorWithoutSuppressFinalizeTest.cs \
	UseStringEmptyTest.cs \
	UseTypeEmptyTypesTest.cs \
	AvoidUnusedParametersTest.cs \
	CompareWithStringEmptyEfficientlyTest.cs \
	AvoidMethodWithLargeMaximumStackSizeTest.cs \
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
	PreferCharOverloadTest.cs \
	PreferLiteralOverInitOnlyFieldsTest.cs \
	ReviewLinqMethodTest.cs
