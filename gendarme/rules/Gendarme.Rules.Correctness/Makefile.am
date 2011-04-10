include ../common.make

rules_sources =  \
	AttributeStringLiteralsShouldParseCorrectlyRule.cs \
	AvoidCodeWithSideEffectsInConditionalCodeRule.cs \
	AvoidConstructorsInStaticTypesRule.cs \
	AvoidFloatingPointEqualityRule.cs \
	AvoidMethodsWithSideEffectsInConditionalCodeRule.cs \
	DeclareEventsExplicitlyRule.cs \
	DoNotRecurseInEqualityRule.cs \
	BadRecursiveInvocationRule.cs \
	CallingEqualsWithNullArgRule.cs \
	CheckParametersNullityInVisibleMethodsRule.cs \
	DisposableFieldsShouldBeDisposedRule.cs \
	DoNotRoundIntegersRule.cs \
	DontCompareWithNaNRule.cs \
	EnsureLocalDisposalRule.cs \
	FinalizersShouldCallBaseClassFinalizerRule.cs \
	FloatComparisonRule.cs \
	ReviewInconsistentIdentityRule.cs \
	MethodCanBeMadeStaticRule.cs \
	ProvideCorrectArgumentsToFormattingMethodsRule.cs \
	ProvideCorrectRegexPatternRule.cs \
	ProvideValidXmlStringRule.cs \
	ProvideValidXPathExpressionRule.cs \
	ReviewCastOnIntegerDivisionRule.cs \
	ReviewCastOnIntegerMultiplicationRule.cs \
	ReviewDoubleAssignmentRule.cs \
	ReviewSelfAssignmentRule.cs \
	ReviewUselessControlFlowRule.cs \
	ReviewUseOfInt64BitsToDoubleRule.cs \
	ReviewUseOfModuloOneOnIntegersRule.cs \
	TypesShouldBeDisposableBaseRule.cs \
	TypesWithDisposableFieldsShouldBeDisposableRule.cs \
	TypesWithNativeFieldsShouldBeDisposableRule.cs \
	UseNoInliningWithGetCallingAssemblyRule.cs \
	UseValueInPropertySetterRule.cs

EXTRA_TESTS_OPTIONS += -r:System.Configuration -d:DEBUG -resource:Test/Resources.resources,Tests.Rules.Correctness.Resource.resources

EXTRA_DIST += Test/Resource.resx

tests_resource_resx = Test/Resource.resx
tests_resource_prefix_resx = $(addprefix $(srcdir)/, $(tests_resource_resx))

Test/Resources.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx),Test/Resources.resources;

EXTRA_TESTS_DEPS = Test/Resources.resources

tests_sources = \
	AttributeStringLiteralsShouldParseCorrectlyTest.cs \
	AvoidCodeWithSideEffectsInConditionalCodeTest.cs \
	AvoidConstructorsInStaticTypesTest.cs \
	AvoidFloatingPointEqualityTest.cs \
	AvoidMethodsWithSideEffectsInConditionalCodeTest.cs \
	DeclareEventsExplicitlyTest.cs \
	DoNotRecurseInEqualityTest.cs \
	BadRecursiveInvocationTest.cs \
	CallingEqualsWithNullArgTest.cs \
	CheckParametersNullityInVisibleMethodsTest.cs \
	DisposableFieldsShouldBeDisposedTest.cs \
	DontCompareWithNaNTest.cs \
	DoNotRoundIntegersTest.cs \
	EnsureLocalDisposalTest.cs \
	FinalizersShouldCallBaseClassFinalizerTest.cs \
	ReviewInconsistentIdentityTest.cs \
	MethodCanBeMadeStaticTest.cs \
	ProvideCorrectArgumentsToFormattingMethodsTest.cs \
	ProvideCorrectRegexPatternTest.cs \
	ProvideValidXmlStringTest.cs \
	ProvideValidXPathExpressionTest.cs \
	Resource.Designer.cs \
	ReviewCastOnIntegerDivisionTest.cs \
	ReviewCastOnIntegerMultiplicationTest.cs \
	ReviewDoubleAssignmentTest.cs \
	ReviewSelfAssignmentTest.cs \
	ReviewUselessControlFlowTest.cs \
	ReviewUseOfInt64BitsToDoubleTest.cs \
	ReviewUseOfModuloOneOnIntegersTest.cs \
	TypesWithDisposableFieldsShouldBeDisposableTest.cs \
	TypesWithNativeFieldsShouldBeDisposableTest.cs \
	UseNoInliningWithGetCallingAssemblyTest.cs \
	UseValueInPropertySetterTest.cs
