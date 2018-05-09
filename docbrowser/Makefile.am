monodocdir=$(prefix)/lib/monodoc

if DMCS_PRESENT
COMPILER=$(MCS)
else
COMPILER=$(MCS)
endif

SUBDIRS=theme-icons

if ENABLE_WEBKIT
WEBKIT_TARGET=WebKitHtmlRender.dll
endif

DUMMY_TARGET=DummyHtmlRender.dll

CLEANFILES = browser.exe browser.exe.mdb $(WEBKIT_TARGET) $(WEBKIT_TARGET).mdb $(DUMMY_TARGET) $(DUMMY_TARGET).mdb monodoc.desktop Options.cs
monodoc_DATA = browser.exe $(WEBKIT_TARGET) $(DUMMY_TARGET)

DISTCLEANFILES = AssemblyInfo.cs monodoc.desktop monodoc

bin_SCRIPTS = monodoc

browser_sources   = \
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/editing.cs            \
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/IHtmlRender.cs	\
	$(srcdir)/BookmarkManager.cs	\
	$(srcdir)/ProgressPanel.cs

browser_built_sources = AssemblyInfo.cs Options.cs

webkitrender_sources = 			\
	$(srcdir)/WebKitHtmlRender.cs

dummyrender_sources = 			\
	$(srcdir)/DummyHtmlRender.cs

browser_assemblies = $(GTK_SHARP_LIBS) $(MONODOC_LIBS) $(GNOME_SHARP_LIBS) -r:System.Web.Services -r:System.Web
webkitrender_assemblies = $(GTK_SHARP_LIBS) $(WEBKIT_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe
dummyrender_assemblies = $(GTK_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe

EXTRA_DIST = \
	$(browser_sources) 		\
	$(webkitrender_sources)		\
	monodoc.desktop.in		\
	browser.glade 			\
	monodoc.png			\
	AssemblyInfo.cs.in		\
	README.ADMIN

browser.exe: $(browser_sources) $(browser_built_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(COMPILER) -debug -out:browser.exe $(browser_sources) $(browser_built_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) $(CSHARP_FLAGS)

WebKitHtmlRender.dll : $(webkitrender_sources) browser.exe
	$(COMPILER) -debug -target:library -out:$@ $(webkitrender_sources) $(webkitrender_assemblies) $(CSHARP_FLAGS)

DummyHtmlRender.dll : $(dummyrender_sources) browser.exe
	$(COMPILER) -debug -target:library -out:$@ $(dummyrender_sources) $(dummyrender_assemblies) $(CSHARP_FLAGS)

Options.cs:
	cp `pkg-config --variable=Sources mono-options` .

b: browser.exe
	MONO_PATH=. $(RUNTIME) --debug browser.exe

desktopdir = $(datadir)/applications
desktop_DATA = monodoc.desktop

monodoc.desktop: monodoc.desktop.in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = monodoc.png
