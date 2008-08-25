SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Security.Cas.dll
tests_dll = Test.Rules.Security.Cas.dll

security_cas_rulesdir=$(prefix)/lib/gendarme
security_cas_rules_SCRIPTS = $(rules_dll)
security_cas_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in TestResult.xml

rules_sources = \
	AddMissingTypeInheritanceDemandRule.cs \
	DoNotExposeFieldsInSecuredTypeRule.cs \
	DoNotExposeMethodsProtectedByLinkDemandRule.cs \
	DoNotReduceTypeSecurityOnMethodsRule.cs \
	ReviewNonVirtualMethodWithInheritanceDemandRule.cs \
	ReviewSealedTypeWithInheritanceDemandRule.cs \
	ReviewSuppressUnmanagedCodeSecurityUsageRule.cs \
	SecureGetObjectDataOverridesRule.cs

tests_sources = \
	AddMissingTypeInheritanceDemandTest.cs \
	DoNotExposeFieldsInSecuredTypeTest.cs \
	DoNotExposeMethodsProtectedByLinkDemandTest.cs \
	DoNotReduceTypeSecurityOnMethodsTest.cs \
	ReviewNonVirtualMethodWithInheritanceDemandTest.cs \
	ReviewSealedTypeWithInheritanceDemandTest.cs \
	ReviewSuppressUnmanagedCodeSecurityUsageTest.cs \
	SecureGetObjectDataOverridesTest.cs
