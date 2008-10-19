SUBDIRS=Test

include ../common.make

rules_dll = ../../bin/Gendarme.Rules.Serialization.dll
tests_dll = Test.Rules.Serialization.dll

rules_doc_source = doc/Gendarme.Rules.Serialization.source 
rules_doc_tree = doc/Gendarme.Rules.Serialization.tree
rules_doc_zip = doc/Gendarme.Rules.Serialization.zip

serialization_rulesdir=$(prefix)/lib/gendarme
serialization_rules_SCRIPTS = $(rules_dll)
serialization_rules_DATA = 

serialization_documentationdir=$(prefix)/lib/monodoc/sources
serialization_documentation_DATA = $(rules_doc)

EXTRA_DIST = $(rules_sources) $(rules_sources_in) Gendarme.Rules.Serialization.mdp Gendarme.Rules.Serialization.csproj
CLEANFILES = $(rules_dll) $(rules_dll).mdb $(tests_dll) $(rules_doc) $(rules_dll).doc
DISTCLEANFILES = Makefile.in Gendarme.Rules.Serialization.xml TestResult.xml

rules_sources = \
	CallBaseMethodsOnISerializableTypesRule.cs \
	DeserializeOptionalFieldRule.cs \
	MarkAllNonSerializableFieldsRule.cs \
	MissingSerializableAttributeOnISerializableTypeRule.cs \
	MissingSerializationConstructorRule.cs \
	UseCorrectSignatureForSerializationMethodsRule.cs \
	ImplementISerializableCorrectlyRule.cs

tests_sources = \
	CallBaseMethodsOnISerializableTypesTest.cs \
	DeserializeOptionalFieldTest.cs \
	MarkAllNonSerializableFieldsTest.cs \
	MissingSerializableAttributeOnISerializableTypeTest.cs \
	MissingSerializationConstructorTest.cs \
	UseCorrectSignatureForSerializationMethodsTest.cs \
	ImplementISerializableCorrectlyTest.cs
