include ../common.make
common_tests=../Test.Rules/Test.Rules.dll,System.Drawing

rules_sources =  \
	AvoidAlwaysNullFieldRule.cs \
	AvoidComplexMethodsRule.cs \
	AvoidDeepInheritanceTreeRule.cs \
	AvoidLackOfCohesionOfMethodsRule.cs \
	AvoidUnnecessarySpecializationRule.cs \
	ConsiderUsingStopwatchRule.cs \
	PreferStringIsNullOrEmptyRule.cs \
	RemoveDependenceOnObsoleteCodeRule.cs \
	RemoveOrphanResourcesRule.cs

EXTRA_TESTS_OPTIONS += -d:DEBUG -resource:Test/Resources.resources,Tests.Rules.Maintainability.Resource.resources

EXTRA_DIST += Test/Resource.resx

tests_resource_resx = Test/Resource.resx
tests_resource_prefix_resx = $(addprefix $(srcdir)/, $(tests_resource_resx))

Test/Resources.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx),Test/Resources.resources;

EXTRA_TESTS_DEPS = Test/Resources.resources

tests_sources = \
	AvoidAlwaysNullFieldTest.cs \
	AvoidComplexMethodsTest.cs \
	AvoidDeepInheritanceTreeTest.cs \
	AvoidLackOfCohesionOfMethodsTest.cs \
	AvoidUnnecessarySpecializationTest.cs \
	ConsiderUsingStopwatchTest.cs \
	PreferStringIsNullOrEmptyTest.cs \
	RemoveDependenceOnObsoleteCodeTest.cs \
	RemoveOrphanResourceTest.cs \
	Resource.Designer.cs
