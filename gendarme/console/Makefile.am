gendarmedir=$(prefix)/lib/gendarme
gendarme_SCRIPTS = ../bin/gendarme.exe
EXTRA_DIST = $(gendarme_sources) $(gendarme_sources_in) console.mdp gendarme.xsl gendarme.csproj
CLEANFILES = $(gendarme_SCRIPTS) $(gendarme_SCRIPTS).mdb
DISTCLEANFILES = Makefile.in

gendarme_sources_in = ../AssemblyInfo.cs.in
gendarme_generated_sources = $(gendarme_sources_in:.in=)
gendarme_sources = ConsoleRunner.cs ResultWriter.cs TextResultWriter.cs XmlResultWriter.cs HtmlResultWriter.cs Options.cs
gendarme_resources = gendarme.xsl

gendarme_build_sources = $(addprefix $(srcdir)/, $(gendarme_sources))
gendarme_build_sources += $(gendarme_generated_sources)

../bin/gendarme.exe: $(gendarme_build_sources) $(gendarme_resources)
	$(GMCS) -debug -d:LINQ -r:$(top_builddir)/gendarme/bin/Mono.Cecil.dll -r:../bin/Gendarme.Framework.dll -r:System.Xml.dll \
		-out:$@ $(gendarme_build_sources) -resource:gendarme.xsl

self-test: ../bin/gendarme.exe
	mono --debug ../bin/gendarme.exe ../bin/gendarme.exe
