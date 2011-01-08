include ../common.make

rules_sources =  \
	AvoidUnusedInternalResourceRule.cs \
	SatelliteResourceMismatchRule.cs

EXTRA_TESTS_OPTIONS += -r:System.Drawing -d:DEBUG -resource:Test/Resource.resources -resource:Test/InternalResource.resources \
	-resource:Test/PublicResource.resources -resource:Test/Resources/XMLFileNotInResX.xml

EXTRA_DIST += Test/Resource.resx Test/Resource.fr.resx Test/MainResourceMissing.fr.resx Test/InternalResource.resx Test/PublicResource.resx

tests_resource_resx = Test/Resource.resx
tests_resource_prefix_resx = $(addprefix $(srcdir)/, $(tests_resource_resx))

Test/Resource.resources: $(tests_resource_resx)
	resgen2 -usesourcepath -compile $(tests_resource_resx),Test/Resource.resources;

tests_resource_resx_fr = Test/Resource.fr.resx
tests_resource_prefix_resx_fr = $(addprefix $(srcdir)/, $(tests_resource_resx_fr))

Test/Resource.fr.resources: $(tests_resource_resx_fr)
	resgen2 -usesourcepath -compile $(tests_resource_resx_fr),Test/Resource.fr.resources;

tests_resource_missing_resx_fr = Test/MainResourceMissing.fr.resx
tests_resource_missing_prefix_resx_fr = $(addprefix $(srcdir)/, $(tests_resource_missing_resx_fr))

Test/MainResourceMissing.fr.resources: $(tests_resource_missing_resx_fr)
	resgen2 -usesourcepath -compile $(tests_resource_missing_resx_fr),Test/MainResourceMissing.fr.resources;

tests_internalresource_resx = Test/InternalResource.resx
tests_internalresource_prefix_resx = $(addprefix $(srcdir)/, $(tests_internalresource_resx))

Test/InternalResource.resources: $(tests_internalresource_resx)
	resgen2 -usesourcepath -compile $(tests_internalresource_resx),Test/InternalResource.resources;

tests_publicresource_resx = Test/PublicResource.resx
tests_publicresource_prefix_resx = $(addprefix $(srcdir)/, $(tests_publicresource_resx))

Test/PublicResource.resources: $(tests_publicresource_resx)
	resgen2 -usesourcepath -compile $(tests_publicresource_resx),Test/PublicResource.resources;

tests_resource_xmlnotresx_fr = Test/Resources/XMLFileNotInResX.fr.xml
tests_resource_missing_xmlnotresx_fr = Test/Resources/XMLFileNotInResXOnlyInFrench.fr.xml

fr/Test.Rules.Globalization.resources.dll: Test/Resource.fr.resources Test/MainResourceMissing.fr.resources $(tests_resource_xmlnotresx_fr) $(tests_resource_missing_xmlnotresx_fr)
	mkdir -p fr
	al -culture:fr -embed:Test/MainResourceMissing.fr.resources -embed:Test/Resource.fr.resources -embed:$(tests_resource_xmlnotresx_fr) -embed:$(tests_resource_missing_xmlnotresx_fr) -out:fr/Test.Rules.Globalization.resources.dll

EXTRA_TESTS_DEPS = Test/Resource.resources Test/InternalResource.resources Test/PublicResource.resources Test/Resources/XMLFileNotInResX.xml fr/Test.Rules.Globalization.resources.dll

tests_sources = \
	AvoidUnusedInternalResourceTest.cs \
	SatelliteResourceMismatchTest.cs \
	Resource.Designer.cs \
	InternalResource.Designer.cs \
	PublicResource.Designer.cs
