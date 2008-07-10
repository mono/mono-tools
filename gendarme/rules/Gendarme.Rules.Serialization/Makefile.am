SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Serialization.dll
tests_dll = Test.Rules.Serialization.dll

serialization_rulesdir=$(prefix)/lib/gendarme
serialization_rules_SCRIPTS = $(rules_dll)
serialization_rules_DATA = 

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Serialization.mdp Gendarme.Rules.Serialization.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll)
DISTCLEANFILES = Makefile.in Gendarme.Rules.Serialization.xml TestResult.xml

rules_sources = \
	CallBaseMethodsOnISerializableTypesRule.cs \
	DeserializeOptionalFieldRule.cs \
	MarkAllNonSerializableFieldsRule.cs \
	MissingSerializableAttributeOnISerializableTypeRule.cs \
	MissingSerializationConstructorRule.cs \
	UseCorrectSignatureForSerializationMethodsRule.cs

tests_sources = \
	CallBaseMethodsOnISerializableTypesTest.cs \
	DeserializeOptionalFieldTest.cs \
	MarkAllNonSerializableFieldsTest.cs \
	MissingSerializableAttributeOnISerializableTypeTest.cs \
	MissingSerializationConstructorTest.cs \
	UseCorrectSignatureForSerializationMethodsTest.cs
