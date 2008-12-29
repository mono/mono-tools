include ../common.make

rules_sources =  \
	AvoidMethodWithUnusedGenericTypeRule.cs \
	DoNotExposeNestedGenericSignaturesRule.cs \
	ImplementGenericCollectionInterfacesRule.cs \
	PreferGenericsOverRefObjectRule.cs \
	UseGenericEventHandlerRule.cs

EXTRA_TESTS_OPTIONS += -r:System.Xml.dll -r:System.Xml.Linq.dll

tests_sources = \
	AvoidMethodWithUnusedGenericTypeTest.cs \
	DoNotExposeNestedGenericSignaturesTest.cs \
	ImplementGenericCollectionInterfacesTest.cs \
	PreferGenericsOverRefObjectTest.cs \
	UseGenericEventHandlerTest.cs
