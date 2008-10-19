SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Security.Cas.dll
tests_dll = Test.Rules.Security.Cas.dll

rules_doc_source = doc/Gendarme.Rules.Security.Cas.source 
rules_doc_tree = doc/Gendarme.Rules.Security.Cas.tree
rules_doc_zip = doc/Gendarme.Rules.Security.Cas.zip

security_cas_rulesdir=$(prefix)/lib/gendarme
security_cas_rules_SCRIPTS = $(rules_dll)
security_cas_rules_DATA = 

security_cas_documentationdir=$(prefix)/lib/monodoc/sources
security_cas_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) 
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
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
