monodocdir=$(prefix)/lib/monodoc
monodoc_DATA = browser.exe 
noinst_DATA = admin.exe

CLEANFILES = browser.exe browser.exe.mdb admin.exe admin.exe.mdb
DISTCLEANFILES = AssemblyInfo.cs monodoc.desktop monodoc.in

bin_SCRIPTS = monodoc

browser_sources   = \
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/HtmlRender.cs	

admin_sources = \
	$(srcdir)/admin.cs		\
	$(srcdir)/Contributions.cs

browser_assemblies = $(GTK_SHARP_LIBS) $(GECKO_SHARP_LIBS) -pkg:monodoc -r:System.Web.Services 

EXTRA_DIST = \
	$(browser_sources) 	\
	admin.cs		\
	admin.glade 		\
	browser.glade 		\
	monodoc.png

admin.exe: $(admin_sources) $(srcdir)/admin.glade
	$(MCS) -debug -out:admin.exe $(admin_sources) -resource:$(srcdir)/admin.glade,admin.glade $(browser_assemblies) -r:System.Drawing

browser.exe: $(browser_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png
	$(MCS) -debug -out:browser.exe $(browser_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies)

b: browser.exe
	MONO_PATH=. $(RUNTIME) --debug browser.exe

c: admin.exe
	MONO_PATH=. $(RUNTIME) --debug admin.exe

desktopdir = $(datadir)/applications
desktop_DATA = monodoc.desktop

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = monodoc.png
