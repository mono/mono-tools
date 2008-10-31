include ../common.make

EXTRA_RULES_OPTIONS += -r:../../bin/Gendarme.Rules.Performance.dll

rules_sources =  \
	AvoidCodeDuplicatedInSameClassRule.cs \
	AvoidCodeDuplicatedInSiblingClassesRule.cs \
	AvoidLargeClassesRule.cs \
	AvoidLongMethodsRule.cs \
	AvoidLongParameterListsRule.cs \
	AvoidMessageChainsRule.cs \
	AvoidSpeculativeGeneralityRule.cs \
	CodeDuplicatedLocator.cs \
	ExpressionCollection.cs \
	ExpressionFillerVisitor.cs \
	Utilities.cs

EXTRA_TESTS_OPTIONS += -r:System.Windows.Forms.dll

tests_sources =  \
	AvoidCodeDuplicatedInSameClassTest.cs \
	AvoidCodeDuplicatedInSiblingClassesTest.cs \
	AvoidLargeClassesTest.cs \
	AvoidLongMethodsTest.cs \
	AvoidLongParameterListsTest.cs \
	AvoidMessageChainsTest.cs \
	AvoidSpeculativeGeneralityTest.cs
