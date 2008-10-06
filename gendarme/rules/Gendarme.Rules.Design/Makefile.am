SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Design.dll
tests_dll = Test.Rules.Design.dll

design_rulesdir=$(prefix)/lib/gendarme
design_rules_SCRIPTS = $(rules_dll)
design_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Design.mdp Gendarme.Rules.Design.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Design.xml TestResult.xml

rules_sources = \
	AvoidMultidimensionalIndexerRule.cs \
	AvoidRefAndOutParametersRule.cs \
	ConsiderUsingStaticTypeRule.cs \
	DontDeclareProtectedFieldsInSealedClassRule.cs \
	DoNotDeclareVirtualMethodsInSealedTypeRule.cs \
	PreferIntegerOrStringForIndexersRule.cs \
	UsingCloneWithoutImplementingICloneableRule.cs \
	DefineAZeroValueRule.cs \
	EnumsShouldDefineAZeroValueRule.cs \
	FlagsShouldNotDefineAZeroValueRule.cs \
	AbstractTypesShouldNotHavePublicConstructorsRule.cs \
	AvoidEmptyInterfaceRule.cs \
	TypesShouldBeInsideNamespacesRule.cs \
	AvoidPropertiesWithoutGetAccessorRule.cs \
	MainShouldNotBePublicRule.cs \
	ConsiderConvertingMethodToPropertyRule.cs \
	AvoidPublicInstanceFieldsRule.cs \
	MissingAttributeUsageOnCustomAttributeRule.cs \
	AttributeArgumentsShouldHaveAccessorsRule.cs \
	TypesWithNativeFieldsShouldBeDisposableRule.cs \
	TypesWithDisposableFieldsShouldBeDisposableRule.cs \
	DisposableTypesShouldHaveFinalizerRule.cs \
	DisposableFieldsShouldBeDisposedRule.cs \
	FinalizersShouldBeProtectedRule.cs \
	FinalizersShouldCallBaseClassFinalizerRule.cs \
	EnsureSymmetryForOverloadedOperatorsRule.cs \
	OperatorEqualsShouldBeOverloadedRule.cs \
	OverrideEqualsMethodRule.cs \
	ProvideAlternativeNamesForOperatorOverloadsRule.cs \
	ImplementEqualsAndGetHashCodeInPairRule.cs \
	EnumsShouldUseInt32Rule.cs \
	ImplementIComparableCorreclyRule.cs \
	ConsiderConvertingFieldToNullableRule.cs \
	AvoidVisibleNestedTypesRule.cs \
	DeclareEventHandlersCorrectlyRule.cs

tests_sources = \
	AvoidMultidimensionalIndexerTest.cs \
	AvoidRefAndOutParametersTest.cs \
	ConsiderUsingStaticTypeTest.cs \
	DontDeclareProtectedFieldsInSealedClassTest.cs \
	DoNotDeclareVirtualMethodsInSealedTypeTest.cs \
	PreferIntegerOrStringForIndexersTest.cs \
	UsingCloneWithoutImplementingICloneableTest.cs \
	EnumsShouldDefineAZeroValueTest.cs \
	FlagsShouldNotDefineAZeroValueTest.cs \
	AbstractTypesShouldNotHavePublicConstructorsTest.cs \
	AvoidEmptyInterfaceTest.cs \
	TypesShouldBeInsideNamespacesTest.cs \
	AvoidPropertiesWithoutGetAccessorTest.cs \
	MainShouldNotBePublicTest.cs \
	ConsiderConvertingMethodToPropertyTest.cs \
	AvoidPublicInstanceFieldsTest.cs \
	MissingAttributeUsageOnCustomAttributeTest.cs \
	AttributeArgumentsShouldHaveAccessorsTest.cs \
	TypesWithNativeFieldsShouldBeDisposableTest.cs \
	TypesWithDisposableFieldsShouldBeDisposableTest.cs \
	DisposableTypesShouldHaveFinalizerTest.cs \
	DisposableFieldsShouldBeDisposedTest.cs \
	FinalizersShouldBeProtectedTest.cs \
	FinalizersShouldCallBaseClassFinalizerTest.cs \
	EnsureSymmetryForOverloadedOperatorsTest.cs \
	OperatorEqualsShouldBeOverloadedTest.cs \
	OverrideEqualsMethodTest.cs \
	ProvideAlternativeNamesForOperatorOverloadsTest.cs \
	ImplementEqualsAndGetHashCodeInPairTest.cs \
	EnumsShouldUseInt32Test.cs \
	ImplementIComparableCorreclyTest.cs \
	ConsiderConvertingFieldToNullableTest.cs \
	AvoidVisibleNestedTypesTest.cs \
	DeclareEventHandlersCorrectlyTest.cs
