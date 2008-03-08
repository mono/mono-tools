monodocdir=$(prefix)/lib/monodoc
noinst_DATA = admin.exe


if ENABLE_GECKO
GECKO_TARGET=GeckoHtmlRender.dll
endif
if ENABLE_GTKHTML
GECKO_PRINTING_DEF="-d:USE_GTKHTML_PRINT"

GTKHTML_TARGET=GtkHtmlHtmlRender.dll
endif
if ENABLE_WEBKIT
WEBKIT_TARGET=WebKitHtmlRender.dll
endif
if ENABLE_MONOWEBBROWSER
MONOWEBBROWSER_TARGET=MonoWebBrowserHtmlRender.dll
endif

CLEANFILES = browser.exe browser.exe.mdb admin.exe admin.exe.mdb $(GECKO_TARGET) $(GECKO_TARGET).mdb $(GTKHTML_TARGET) $(GTKHTML_TARGET).mdb $(WEBKIT_TARGET) $(WEBKIT_TARGET).mdb $(MONOWEBBROWSER_TARGET) $(MONOWEBBROWSER_TARGET).mdb monodoc.desktop
monodoc_DATA = browser.exe $(GECKO_TARGET) $(GTKHTML_TARGET) $(WEBKIT_TARGET) $(MONOWEBBROWSER_TARGET)

DISTCLEANFILES = AssemblyInfo.cs monodoc.desktop monodoc

bin_SCRIPTS = monodoc

browser_sources   = \
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/IHtmlRender.cs	\
	$(srcdir)/BookmarkManager.cs	\
	$(srcdir)/ProgressPanel.cs

browser_built_sources = AssemblyInfo.cs

geckorender_sources = 			\
	$(srcdir)/PrintManager.cs	\
	$(srcdir)/GeckoHtmlRender.cs

gtkhtmlrender_sources = 		\
	$(srcdir)/PrintManager.cs	\
	$(srcdir)/GtkHtmlHtmlRender.cs

webkitrender_sources = 			\
	$(srcdir)/WebKitHtmlRender.cs

monowebbrowserrender_sources = 		\
	$(srcdir)/BrowserWidget.cs	\
	$(srcdir)/MonoWebBrowserHtmlRender.cs

admin_sources = \
	$(srcdir)/admin.cs		\
	$(srcdir)/Contributions.cs

browser_assemblies = $(GTK_SHARP_LIBS) $(MONODOC_LIBS) -r:System.Web.Services
# we insert gtkhtml libs if we have them for printing 
geckorender_assemblies = $(GTK_SHARP_LIBS) $(GTKHTML_SHARP_LIBS) $(GECKO_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe
gtkhtmlrender_assemblies = $(GTK_SHARP_LIBS) $(GTKHTML_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe

# TODO: use pkg-config for webkit-sharp
WEBKIT_SHARP_LIBS = -r:nwebkit.dll
webkitrender_assemblies = $(GTK_SHARP_LIBS) $(WEBKIT_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe

monowebbrowserrender_assemblies = $(GTK_SHARP_LIBS) $(MONOWEBBROWSER_LIBS) $(MONODOC_LIBS) -r:browser.exe -r:Mono.Mozilla.dll

EXTRA_DIST = \
	$(browser_sources) 		\
	$(geckorender_sources)		\
	$(gtkhtmlrender_sources)	\
	$(webkitrender_sources)		\
	monodoc.desktop.in		\
	admin.cs			\
	admin.glade 			\
	browser.glade 			\
	monodoc.png			\
	AssemblyInfo.cs.in		\
	README.ADMIN

# admin hardcodes GTKHTML for now.
if ENABLE_GTKHTML
admin.exe: $(admin_sources) $(srcdir)/admin.glade
	$(GMCS) -debug -out:admin.exe $(admin_sources) -resource:$(srcdir)/admin.glade,admin.glade $(browser_assemblies) $(GTKHTML_SHARP_LIBS) -r:System.Drawing
endif

browser.exe: $(browser_sources) $(browser_built_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(GMCS) -debug -out:browser.exe $(browser_sources) $(browser_built_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) 

GeckoHtmlRender.dll : $(geckorender_sources) browser.exe
	$(GMCS) -debug -target:library -out:$@ $(geckorender_sources) $(geckorender_assemblies)  $(GECKO_PRINTING_DEF) $(CSHARP_FLAGS)

GtkHtmlHtmlRender.dll : $(gtkhtmlrender_sources) browser.exe
	$(GMCS) -debug -target:library -out:$@ $(gtkhtmlrender_sources) $(gtkhtmlrender_assemblies) $(CSHARP_FLAGS)

WebKitHtmlRender.dll : $(webkitrender_sources) browser.exe
	$(GMCS) -debug -target:library -out:$@ $(webkitrender_sources) $(webkitrender_assemblies) $(CSHARP_FLAGS)

MonoWebBrowserHtmlRender.dll : $(monowebbrowserrender_sources) browser.exe
	$(GMCS) -debug -target:library -out:$@ $(monowebbrowserrender_sources) $(monowebbrowserrender_assemblies) $(CSHARP_FLAGS)

b: browser.exe
	MONO_PATH=. $(RUNTIME) --debug browser.exe

c: admin.exe
	MONO_PATH=. $(RUNTIME) --debug admin.exe

desktopdir = $(datadir)/applications
desktop_DATA = monodoc.desktop

monodoc.desktop: monodoc.desktop.in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = monodoc.png
