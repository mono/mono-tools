SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Exceptions.dll
tests_dll = Test.Rules.Exceptions.dll

exceptions_rulesdir=$(prefix)/lib/gendarme
exceptions_rules_SCRIPTS = ../../bin/Gendarme.Rules.Exceptions.dll
exceptions_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Exceptions.mdp Gendarme.Rules.Exceptions.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
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
	SEHHandlerType.cs

tests_sources = \
	AvoidArgumentExceptionDefaultConstructorTest.cs \
	AvoidThrowingBasicExceptionsTest.cs \
	DontDestroyStackTraceTest.cs \
	DontSwallowErrorsCatchingNonspecificExceptionsTest.cs \
	DoNotThrowReservedExceptionTest.cs
	ExceptionShouldBeVisibleTest.cs \
	MissingExceptionConstructorsTest.cs
