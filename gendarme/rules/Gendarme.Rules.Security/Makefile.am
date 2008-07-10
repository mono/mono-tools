SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Security.dll
tests_dll = Test.Rules.Security.dll

security_rulesdir=$(prefix)/lib/gendarme
security_rules_SCRIPTS = $(rules_dll)
security_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Security.mdp Gendarme.Rules.Security.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Security.xml TestResult.xml

rules_sources = \
	ArrayFieldsShouldNotBeReadOnlyRule.cs \
	MethodCallWithSubsetLinkDemandRule.cs \
	NativeFieldsShouldNotBeVisibleRule.cs \
	NonVirtualMethodWithInheritanceDemandRule.cs \
	SealedTypeWithInheritanceDemandRule.cs \
	SecureGetObjectDataOverridesRule.cs \
	StaticConstructorsShouldBePrivateRule.cs \
	TypeExposeFieldsRule.cs \
	TypeIsNotSubsetOfMethodSecurityRule.cs \
	TypeLinkDemandRule.cs

tests_sources = \
	ArrayFieldsShouldNotBeReadOnlyTest.cs \
	MethodCallWithSubsetLinkDemandTest.cs \
	NativeFieldsShouldNotBeVisibleTest.cs \
	NonVirtualMethodWithInheritanceDemandTest.cs \
	SealedTypeWithInheritanceDemandTest.cs \
	SecureGetObjectDataOverridesTest.cs \
	StaticConstructorsShouldBePrivateTest.cs 
	TypeExposeFieldsTest.cs \
	TypeIsNotSubsetOfMethodSecurityTest.cs \
	TypeLinkDemandTest.cs
