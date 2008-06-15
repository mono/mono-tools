TARGET = ilcontrast.exe
CLEANFILES = $(TARGET) $(TARGET).mdb
DISTCLEANFILES = Global.cs ilcontrast ilcontrast.desktop
EXTRA_DIST = $(sources) ilcontrast.png mono-api.xsl deploy.tar Global.cs.in ilcontrast.in ilcontrast.desktop.in

assemblydir = $(libdir)/ilcontrast
desktopdir = $(datadir)/applications
pixmapdir = $(datadir)/pixmaps

if ENABLE_WEBKIT
assembly_DATA = $(TARGET)
bin_SCRIPTS = ilcontrast
desktop_DATA = ilcontrast.desktop
pixmap_DATA = ilcontrast.png
else
if ENABLE_GECKO
assembly_DATA = $(TARGET)
bin_SCRIPTS = ilcontrast
desktop_DATA = ilcontrast.desktop
pixmap_DATA = ilcontrast.png
else
all:
	echo "Install webkit-sharp or gecko-sharp to build ilContrast"
endif
endif

if ENABLE_WEBKIT
references = $(GTK_SHARP_LIBS) $(WEBKIT_SHARP_LIBS) -r:ICSharpCode.SharpZipLib.dll
else
references = $(GTK_SHARP_LIBS) $(GECKO_SHARP_LIBS) -d:USE_GECKO -r:ICSharpCode.SharpZipLib.dll
endif

resources = 							\
	/resource:$(srcdir)/ilcontrast.png,ilcontrast.png	\
	/resource:$(srcdir)/mono-api.xsl,mono-api.xsl		\
	/resource:$(srcdir)/deploy.tar,deploy.tar		\
	#

sources =				\
	Application.cs			\
	ComparisonInfo.cs		\
	Window.cs			\
	mono-api-diff.cs		\
	mono-api-info.cs		\
	#

build_sources = Global.cs $(addprefix $(srcdir)/, $(sources))

ilcontrast.exe: $(build_sources)
	$(GMCS) -out:ilcontrast.exe $(references) $(resources) $(build_sources)

Global.cs: Global.cs.in
	sed -e "s|\@VERSION\@|$(VERSION)|" $< > $@

ilcontrast.desktop: ilcontrast.desktop.in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@
