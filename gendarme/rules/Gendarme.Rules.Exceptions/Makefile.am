SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Exceptions.dll
tests_dll = Test.Rules.Exceptions.dll

rules_doc_source = doc/Gendarme.Rules.Exceptions.source 
rules_doc_tree = doc/Gendarme.Rules.Exceptions.tree
rules_doc_zip = doc/Gendarme.Rules.Exceptions.zip

exceptions_rulesdir=$(prefix)/lib/gendarme
exceptions_rules_SCRIPTS = $(rules_dll)
exceptions_rules_DATA = 

exceptions_documentationdir=$(prefix)/lib/monodoc/sources
exceptions_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Exceptions.mdp Gendarme.Rules.Exceptions.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Exceptions.xml TestResult.xml

rules_sources = \
	AvoidArgumentExceptionDefaultConstructorRule.cs \
	AvoidThrowingBasicExceptionsRule.cs \
	DoNotDestroyStackTraceRule.cs \
	DontSwallowErrorsCatchingNonspecificExceptionsRule.cs \
	DoNotThrowReservedExceptionRule.cs \
	ExceptionShouldBeVisibleRule.cs \
	Impl/ExceptionBlockParser.cs \
	Impl/ExecutionBlock.cs \
	Impl/ExecutionPath.cs \
	Impl/ExecutionPathFactory.cs \
	Impl/SEHCatchBlock.cs \
	Impl/SEHGuardedBlock.cs \
	Impl/SEHHandlerBlock.cs \
	MissingExceptionConstructorsRule.cs \
	NewExceptionRule.cs \
	SEHHandlerType.cs \
	InstantiateArgumentExceptionCorrectlyRule.cs

tests_sources = \
	AvoidArgumentExceptionDefaultConstructorTest.cs \
	AvoidThrowingBasicExceptionsTest.cs \
	DontDestroyStackTraceTest.cs \
	DontSwallowErrorsCatchingNonspecificExceptionsTest.cs \
	DoNotThrowReservedExceptionTest.cs \
	ExceptionShouldBeVisibleTest.cs \
	MissingExceptionConstructorsTest.cs \
	InstantiateArgumentExceptionCorrectlyTest.cs
