if ENABLE_GECKO
TARGET = ilcontrast.exe
CLEANFILES = $(TARGET) $(TARGET).mdb
DISTCLEANFILES = Global.cs ilcontrast ilcontrast.desktop
EXTRA_DIST = $(sources) ilcontrast.png mono-api.xsl deploy.tar Global.cs.in ilcontrast.in ilcontrast.desktop.in
assemblydir = $(libdir)/ilcontrast
assembly_DATA = $(TARGET)
bin_SCRIPTS = ilcontrast

references = $(GTK_SHARP_LIBS) $(GECKO_SHARP_LIBS) -r:ICSharpCode.SharpZipLib.dll

resources = 							\
	/resource:$(srcdir)/ilcontrast.png,ilcontrast.png	\
	/resource:$(srcdir)/mono-api.xsl,mono-api.xsl		\
	/resource:$(srcdir)/deploy.tar,deploy.tar		\
	#

sources =				\
	Application.cs			\
	ComparisonInfo.cs		\
	Global.cs			\
	Window.cs			\
	mono-api-diff.cs		\
	mono-api-info.cs		\
	#


build_sources = $(addprefix $(srcdir)/, $(sources))

ilcontrast.exe: $(build_sources)
	$(GMCS) -out:ilcontrast.exe $(references) $(resources) $(build_sources)

desktopdir = $(datadir)/applications
desktop_DATA = ilcontrast.desktop

Global.cs: Global.cs.in
	sed -e "s|\@VERSION\@|$(VERSION)|" $< > $@

ilcontrast.desktop: ilcontrast.desktop.in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = ilcontrast.png
else
all:
	echo "Install gecko-sharp to build ilContrast"
endif
