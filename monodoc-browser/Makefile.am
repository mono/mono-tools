monodocdir=$(prefix)/lib/monodoc
monodoc_DATA = browser.exe 

CLEANFILES = browser.exe browser.exe.mdb
DISTCLEANFILES = AssemblyInfo.cs

browser_sources   = \
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	

browser_assemblies = $(GTK_SHARP_LIBS) -pkg:monodoc -r:System.Web.Services

EXTRA_DIST = \
	$(browser_sources) 	\
	browser.glade 		\
	monodoc.png

monodoc_FILES = browser.exe

browser.exe: $(browser_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png
	$(MCS) -debug -out:browser.exe $(browser_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies)

