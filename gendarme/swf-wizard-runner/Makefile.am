wizarddir=$(prefix)/lib/gendarme
wizard_SCRIPTS = ../bin/gendarme-wizard.exe
EXTRA_DIST = $(wizard_sources) $(wizard_sources_in) swf-wizard-runner.csproj Wizard.resx Properties/Resources.resx \
	Resources/gendarme.png
CLEANFILES = $(wizard_SCRIPTS) $(wizard_SCRIPTS).mdb Wizard.resources Properties/Resources.resources
DISTCLEANFILES = Makefile.in

wizard_sources_in = ../AssemblyInfo.cs.in
wizard_generated_sources = $(gendarme_sources_in:.in=)
wizard_sources = \
	../AssemblyStaticInfo.cs \
	Gendarme.cs \
	GuiRunner.cs \
	Wizard.cs \
	Wizard.Designer.cs \
	Properties/Resources.Designer.cs \
	Properties/Settings.Designer.cs \
	../console/ResultWriter.cs \
	../console/HtmlResultWriter.cs \
	../console/TextResultWriter.cs \
	../console/XmlResultWriter.cs

wizard_build_sources = $(addprefix $(srcdir)/, $(wizard_sources))
wizard_build_sources += $(wizard_generated_sources)

Wizard.resources:
	resgen2 /compile Wizard.resx 

Properties/Resources.resources:
	cd Properties; \
	resgen2 /compile Resources.resx; \
	cd ..

../bin/gendarme-wizard.exe: $(wizard_build_sources) $(wizard_resources) Wizard.resources Properties/Resources.resources
	$(GMCS) -debug -target:winexe -r:$(CECIL_ASM) -r:../bin/Gendarme.Framework.dll \
		-r:System.Core.dll -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Xml.dll \
		-resource:../console/gendarme.xsl -resource:Wizard.resources,Gendarme.Wizard.resources \
		-resource:Properties/Resources.resources,Gendarme.Properties.Resources.resources \
		-out:$@ $(wizard_build_sources)

self-test: $(wizard_SCRIPTS)
	mono --debug ../bin/gendarme.exe $(wizard_SCRIPTS)
