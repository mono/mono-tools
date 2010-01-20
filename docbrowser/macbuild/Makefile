GECKO_PRINTING_DEF="-d:USE_GTKHTML_PRINT"
GTKHTML_TARGET=GtkHtmlHtmlRender.dll

srcdir=../
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

gtkhtml_dll=gtkhtml-binaries-for-osx-monodoc/gtkhtml-sharp.dll
gtkhtml_native=gtkhtml-binaries-for-osx-monodoc/libgtkhtml-3.14.dylib

browser_assemblies = -pkg:gtk-sharp-2.0,glade-sharp-2.0,monodoc -r:$(gtkhtml_dll) -r:System.Web.Services

GMCS=gmcs
browser.exe: $(browser_sources) $(browser_built_sources) $(srcdir)/browser.glade $(srcdir)/monodoc.png 
	$(GMCS) -define:MACOS -debug -out:browser.exe $(browser_sources) $(browser_built_sources) -resource:$(srcdir)/monodoc.png,monodoc.png -resource:$(srcdir)/browser.glade,browser.glade  $(browser_assemblies) 

Options.cs:
	cp `pkg-config --variable=Sources mono-options` .

MAC_APP_DIR=MonoDoc.app
APP_MACOS="$(MAC_APP_DIR)/Contents/MacOS/"

MonoDoc.app: browser.exe Info.plist monodoc monodoc-assemblies-for-osx/* monodoc.icns
	mkdir -p $(MAC_APP_DIR)/Contents/{MacOS,Resources}
	cp $(gtkhtml_dll) $(gtkhtml_dll).config $(gtkhtml_native) browser.exe $(MAC_APP_DIR)/Contents/MacOS/
	cp monodoc$(MAC_APP_DIR)/Contents/MacOS/monodoc
	chmod +x $(MAC_APP_DIR)/Contents/MacOS/monodoc
	cp Info.plist $(MAC_APP_DIR)/Contents/
	cp monodoc.icns $(MAC_APP_DIR)/Contents/Resources
	touch MonoDoc.app

MonoDoc.app.zip: MonoDoc.app
	zip -r9uq MonoDoc.app.zip MonoDoc.app
