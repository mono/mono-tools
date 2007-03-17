monodocdir=$(prefix)/lib/monodoc
noinst_DATA = admin.exe


if ENABLE_GECKO
GECKO_TARGET=GeckoHtmlRender.dll
endif
if ENABLE_GTKHTML
GECKO_PRINTING_DEF="-d:USE_GTKHTML_PRINT"

GTKHTML_TARGET=GtkHtmlHtmlRender.dll
endif

CLEANFILES = browser.exe browser.exe.mdb admin.exe admin.exe.mdb $(GECKO_TARGET) $(GECKO_TARGET).mdb $(GTKHTML_TARGET) $(GTKHTML_TARGET).mdb monodoc.desktop
monodoc_DATA = browser.exe $(GECKO_TARGET) $(GTKHTML_TARGET)

DISTCLEANFILES = AssemblyInfo.cs monodoc.desktop monodoc

bin_SCRIPTS = monodoc

browser_sources   = \
	$(srcdir)/AssemblyInfo.cs	\
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/IHtmlRender.cs	\
	$(srcdir)/BookmarkManager.cs	\
	$(srcdir)/ProgressPanel.cs


geckorender_sources = \
	$(srcdir)/GeckoHtmlRender.cs

gtkhtmlrender_sources = \
	$(srcdir)/GtkHtmlHtmlRender.cs

admin_sources = \
	$(srcdir)/admin.cs		\
	$(srcdir)/Contributions.cs

browser_assemblies = $(GTK_SHARP_LIBS) $(MONODOC_LIBS) -r:System.Web.Services
# we insert gtkhtml libs if we have them for printing 
geckorender_assemblies = $(GTK_SHARP_LIBS) $(GTKHTML_SHARP_LIBS) $(GECKO_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe
gtkhtmlrender_assemblies = $(GTK_SHARP_LIBS) $(GTKHTML_SHARP_LIBS) $(MONODOC_LIBS) -r:browser.exe

EXTRA_DIST = \
	$(browser_sources) 		\
	$(geckorender_sources)		\
	$(gtkhtmlrender_sources)	\
	monodoc.desktop.in		\
	admin.cs			\
	admin.glade 			\
	browser.glade 			\
	monodoc.png			\
	AssemblyInfo.cs			\
	README.ADMIN

# admin hardcodes GTKHTML for now.
if ENABLE_GTKHTML
admin.exe: $(admin_sources) $(srcdir)/admin.glade
	$(MCS) -debug -out:admin.exe $(admin_sources) -resource:$(srcdir)/admin.glade,admin.glade $(browser_assemblies) $(GTKHTML_SHARP_LIBS) -r:System.Drawing
endif

browser.exe: $(browser_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(MCS) -debug -out:browser.exe $(browser_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) 

GeckoHtmlRender.dll : $(geckorender_sources) browser.exe
	$(MCS) -debug -target:library -out:$@ $(geckorender_sources) $(geckorender_assemblies)  $(GECKO_PRINTING_DEF)

GtkHtmlHtmlRender.dll : $(gtkhtmlrender_sources) browser.exe
	$(MCS) -debug -target:library -out:$@ $(gtkhtmlrender_sources) $(gtkhtmlrender_assemblies)

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
