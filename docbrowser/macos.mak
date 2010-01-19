GECKO_PRINTING_DEF="-d:USE_GTKHTML_PRINT"
GTKHTML_TARGET=GtkHtmlHtmlRender.dll

srcdir=.
browser_sources   = \
	$(srcdir)/browser.cs		\
	$(srcdir)/list.cs 		\
	$(srcdir)/elabel.cs 		\
	$(srcdir)/history.cs 		\
	$(srcdir)/Contributions.cs	\
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/IHtmlRender.cs	\
	$(srcdir)/BookmarkManager.cs	\
	$(srcdir)/ProgressPanel.cs	\
	$(srcdir)/GtkHtmlHtmlRender.cs

browser_built_sources = Options.cs

browser_assemblies = -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0 -r:gtkhtml-sharp -r:System.Web.Services -r:../monodoc.dll

GMCS=gmcs
browser.exe: $(browser_sources) $(browser_built_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(GMCS) -debug -out:browser.exe $(browser_sources) $(browser_built_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) 

Options.cs:
	cp `pkg-config --variable=Sources mono-options` .

