include ../options.make

gendarmedir=$(prefix)/lib/gendarme
gendarme_SCRIPTS = ../bin/gendarme.exe
EXTRA_DIST = $(gendarme_sources) $(gendarme_sources_in) $(gendarme_resources) gendarme.csproj
CLEANFILES = $(gendarme_SCRIPTS) $(gendarme_SCRIPTS).mdb
DISTCLEANFILES = Makefile.in

gendarme_sources_in = ../AssemblyInfo.cs.in
gendarme_generated_sources = $(gendarme_sources_in:.in=)
gendarme_sources =  \
	../AssemblyStaticInfo.cs \
	ConsoleRunner.cs \
	Helpers.cs \
	HtmlResultWriter.cs \
	IgnoreFileList.cs \
	Options.cs \
	ResultWriter.cs \
	Settings.cs \
	TextResultWriter.cs \
	XmlResultWriter.cs
gendarme_resources = \
	gendarme.xsl \
	gendarme.xsd

gendarme_build_sources = $(addprefix $(srcdir)/, $(gendarme_sources))
gendarme_build_sources += $(gendarme_generated_sources)

gendarme_prefix_resources = $(addprefix $(srcdir)/, $(gendarme_resources))
gendarme_build_resources = $(foreach res,$(gendarme_prefix_resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

../bin/gendarme.exe: $(gendarme_build_sources) $(gendarme_prefix_resources)
	$(MCS) $(GENDARME_OPTIONS) -r:$(CECIL_ASM) -r:System.Xml.Linq -r:../bin/Gendarme.Framework.dll \
		-out:$@ $(gendarme_build_sources) $(gendarme_build_resources)

self-test: ../bin/gendarme.exe
	mono --debug ../bin/gendarme.exe ../bin/gendarme.exe
