wizarddir=$(prefix)/lib/gendarme
wizard_SCRIPTS = ../bin/gendarme-wizard.exe

desktopdir = $(datadir)/applications
desktop_DATA = gendarme-wizard.desktop

$(desktop_DATA): $(desktop_DATA).in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@

EXTRA_DIST = $(wizard_sources) $(wizard_sources_in) swf-wizard-runner.csproj Wizard.resx Properties/Resources.resx \
	Resources/gendarme.png $(wizard_icon) $(desktop_DATA).in
CLEANFILES = $(wizard_SCRIPTS) $(wizard_SCRIPTS).mdb Wizard.resources Resources.resources Properties/Resources.resources \
	$(desktop_DATA)
DISTCLEANFILES = Makefile.in

wizard_sources_in = ../AssemblyInfo.cs.in
wizard_generated_sources = $(wizard_sources_in:.in=)
wizard_sources =  \
	../AssemblyStaticInfo.cs \
	../console/Helpers.cs \
	../console/HtmlResultWriter.cs \
	../console/ResultWriter.cs \
	../console/TextResultWriter.cs \
	../console/XmlResultWriter.cs \
	Gendarme.cs \
	GuiRunner.cs \
	Page.cs \
	Properties/Resources.Designer.cs \
	Properties/Settings.Designer.cs \
	Wizard.cs \
	Wizard.Designer.cs

wizard_build_sources = $(addprefix $(srcdir)/, $(wizard_sources))
wizard_build_sources += $(wizard_generated_sources)

wizard_resources = Wizard.resx 
wizard_build_resources = $(addprefix $(srcdir)/, $(wizard_resources))

Wizard.resources: $(wizard_build_resources)
	resgen2 /compile $(wizard_build_resources),Wizard.resources

wizard_properties_resx = Properties/Resources.resx
wizard_properties_prefix_resx = $(addprefix $(srcdir)/, $(wizard_properties_resx))
wizard_properties_resource = Properties
wizard_properties_build_resource = $(addprefix $(srcdir)/, $(wizard_properties_resource))

Properties/Resources.resources: $(wizard_properties_prefix_resx)
	mkdir -p Properties; \
	resgen2 -usesourcepath -compile $(wizard_properties_prefix_resx),Properties/Resources.resources;

wizard_xsl = ../console/gendarme.xsl
wizard_build_xsl = $(addprefix $(srcdir)/, $(wizard_xsl))

wizard_icon = Resources/gendarme.ico
wizard_build_icon = $(addprefix $(srcdir)/, $(wizard_icon))

../bin/gendarme-wizard.exe: $(wizard_build_sources) $(wizard_resources) Wizard.resources Properties/Resources.resources $(wizard_build_xsl) $(wizard_build_icon)
	$(GMCS) -debug -target:winexe -r:$(CECIL_ASM) -r:../bin/Gendarme.Framework.dll \
		-r:System.Core.dll -r:System.Xml.Linq.dll -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Xml.dll \
		-resource:$(wizard_build_xsl) -resource:Wizard.resources,Gendarme.Wizard.resources \
		-resource:Properties/Resources.resources,Gendarme.Properties.Resources.resources \
		-win32icon:$(wizard_build_icon) -out:$@ $(wizard_build_sources)

self-test: $(wizard_SCRIPTS)
	mono --debug ../bin/gendarme.exe $(wizard_SCRIPTS)
