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

EXTRA_TESTS_OPTIONS += -d:DEBUG -resource:Test/Resource.resources

EXTRA_DIST += Test/Resource.resx Test/Resource.fr.resx Test/MainResourceMissing.fr.resx

tests_resource_resx = Test/Resource.resx
tests_resource_prefix_resx = $(addprefix $(srcdir)/, $(tests_resource_resx))

Test/Resource.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx),Test/Resource.resources;

tests_resource_resx_fr = Test/Resource.fr.resx
tests_resource_prefix_resx_fr = $(addprefix $(srcdir)/, $(tests_resource_resx_fr))

Test/Resource.fr.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx_fr),Test/Resource.fr.resources;

tests_resource_missing_resx_fr = Test/MainResourceMissing.fr.resx
tests_resource_missing_prefix_resx_fr = $(addprefix $(srcdir)/, $(tests_resource_missing_resx_fr))

Test/MainResourceMissing.fr.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx_fr),Test/MainResourceMissing.fr.resources;

fr/Test.Rules.Maintainability.resources.dll: Test/Resource.fr.resources Test/MainResourceMissing.fr.resources
	mkdir -p fr
	al -culture:fr -embed:Test/MainResourceMissing.fr.resources -embed:Test/Resource.fr.resources -out:fr/Test.Rules.Maintainability.resources.dll

EXTRA_TESTS_DEPS = Test/Resource.resources fr/Test.Rules.Maintainability.resources.dll

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
