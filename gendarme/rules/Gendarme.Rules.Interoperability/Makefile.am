include ../common.make

rules_sources =  \
	DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule.cs \
	DoNotCastIntPtrToInt32Rule.cs \
	GetLastErrorMustBeCalledRightAfterPInvokeRule.cs \
	MarshalBooleansInPInvokeDeclarationsRule.cs \
	MarshalStringsInPInvokeDeclarationsRule.cs \
	PInvokeShouldNotBeVisibleRule.cs \
	UseManagedAlternativesToPInvokeRule.cs

EXTRA_TESTS_OPTIONS += -unsafe

tests_sources = \
	DoNotCastIntPtrToInt32Test.cs \
	GetLastErrorMustBeCalledRightAfterPInvokeTest.cs \
	MarshalBooleansInPInvokeDeclarationsTest.cs \
	MarshalStringsInPInvokeDeclarationsTest.cs \
	PInvokeShouldNotBeVisibleTest.cs \
	UseManagedAlternativesToPInvokeTest.cs	\
	DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.cs
