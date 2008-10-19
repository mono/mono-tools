SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Interoperability.dll
tests_dll = Test.Rules.Interoperability.dll

rules_doc_source = doc/Gendarme.Rules.Interoperability.source 
rules_doc_tree = doc/Gendarme.Rules.Interoperability.tree
rules_doc_zip = doc/Gendarme.Rules.Interoperability.zip

interoperability_rulesdir=$(prefix)/lib/gendarme
interoperability_rules_SCRIPTS = $(rules_dll)
interoperability_rules_DATA = 

interoperability_documentationdir=$(prefix)/lib/monodoc/sources
interoperability_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Interoperability.mdp Gendarme.Rules.Interoperability.csproj 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Interoperability.xml TestResult.xml

rules_sources = \
	DoNotCastIntPtrToInt32Rule.cs \
	GetLastErrorMustBeCalledRightAfterPInvokeRule.cs \
	MarshalStringsInPInvokeDeclarationsRule.cs \
	PInvokeShouldNotBeVisibleRule.cs \
	UseManagedAlternativesToPInvokeRule.cs

EXTRA_TESTS_OPTIONS = -unsafe

tests_sources = \
	DoNotCastIntPtrToInt32Test.cs \
	GetLastErrorMustBeCalledRightAfterPInvokeTest.cs \
	MarshalStringsInPInvokeDeclarationsTest.cs \
	PInvokeShouldNotBeVisibleTest.cs \
	UseManagedAlternativesToPInvokeTest.cs
