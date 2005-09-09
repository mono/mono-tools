monodocdir=$(prefix)/lib/monodoc
noinst_DATA = admin.exe


if ENABLE_GECKO
CLEANFILES = browser.exe browser.exe.mdb admin.exe admin.exe.mdb GeckoHtmlRender.dll 
monodoc_DATA = browser.exe GeckoHtmlRender.dll 
else
CLEANFILES = browser.exe browser.exe.mdb admin.exe admin.exe.mdb 
monodoc_DATA = browser.exe 
endif


DISTCLEANFILES = AssemblyInfo.cs monodoc.desktop monodoc.in

bin_SCRIPTS = monodoc

browser_sources   = \
	$(srcdir)/AssemblyInfo.cs	\
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/GtkHtmlHtmlRender.cs	\
	$(srcdir)/IHtmlRender.cs	


geckorender_sources = \
	$(srcdir)/GeckoHtmlRender.cs

admin_sources = \
	$(srcdir)/admin.cs		\
	$(srcdir)/Contributions.cs

browser_assemblies = $(GTK_SHARP_LIBS) -pkg:monodoc -r:System.Web.Services 
geckorender_assemblies = $(GTK_SHARP_LIBS) $(GECKO_SHARP_LIBS) -pkg:monodoc -r:browser.exe

EXTRA_DIST = \
	$(browser_sources) 	\
	$(geckorender_sources)	\
	admin.cs		\
	admin.glade 		\
	browser.glade 		\
	monodoc.png		\
	AssemblyInfo.cs

admin.exe: $(admin_sources) $(srcdir)/admin.glade
	$(MCS) -debug -out:admin.exe $(admin_sources) -resource:$(srcdir)/admin.glade,admin.glade $(browser_assemblies) -r:System.Drawing

browser.exe: $(browser_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(MCS) -debug -out:browser.exe $(browser_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) 

GeckoHtmlRender.dll : $(geckorender_sources) browser.exe
	$(MCS) -debug -target:library -out:GeckoHtmlRender.dll $(geckorender_sources) $(geckorender_assemblies)



b: browser.exe
	MONO_PATH=. $(RUNTIME) --debug browser.exe

c: admin.exe
	MONO_PATH=. $(RUNTIME) --debug admin.exe

desktopdir = $(datadir)/applications
desktop_DATA = monodoc.desktop

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = monodoc.png
