SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Interoperability.dll
tests_dll = Test.Rules.Interoperability.dll

interoperability_rulesdir=$(prefix)/lib/gendarme
interoperability_rules_SCRIPTS = $(rules_dll)
interoperability_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Interoperability.mdp Gendarme.Rules.Interoperability.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Interoperability.xml TestResult.xml

rules_sources = \
	DoNotCastIntPtrToInt32Rule.cs \
	GetLastErrorMustBeCalledRightAfterPInvokeRule.cs \
	MarshalStringsInPInvokeDeclarationsRule.cs \
	PInvokeShouldNotBeVisibleRule.cs \
	UseManagedAlternativesToPInvokeRule.cs

EXTRA_TESTS_OPTIONS = -unsafe

tests_sources = \
	DoNotCastIntPtrToInt32Test.cs
	GetLastErrorMustBeCalledRightAfterPInvokeTest.cs \
	MarshalStringsInPInvokeDeclarationsTest.cs \
	PInvokeShouldNotBeVisibleTest.cs \
	UseManagedAlternativesToPInvokeTest.cs
