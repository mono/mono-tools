gsharpdir = $(prefix)/lib/gsharp
bin_SCRIPTS = gsharp

gsharp_DATA = gsharp.exe

desktopdir = $(datadir)/applications
desktop_DATA = gsharp.desktop

packages = -pkg:gtk-sharp-2.0 -pkg:glib-sharp-2.0
assemblies = -r:System -r:Mono.CSharp -r:Mono.Management -r:Mono.Posix -r:System.Drawing

gsharp_sources =                        			\
        $(srcdir)/AssemblyInfo.cs       			\
        $(srcdir)/InteractiveGraphicsBase.cs			\
        $(srcdir)/Main.cs               			\
        $(srcdir)/MainWindow.cs         			\
        $(srcdir)/ProcessSelector.cs    			\
        $(srcdir)/Shell.cs              			\
        $(srcdir)/SysDrawing.cs					\
        $(srcdir)/gtk-gui/generated.cs  			\
        $(srcdir)/gtk-gui/Mono.CSharp.Gui.MainWindow.cs 	\
        $(srcdir)/gtk-gui/Mono.CSharp.Gui.ProcessSelector.cs

EXTRA_DIST = $(gsharp_sources) gsharp.in $(desktop_DATA).in
CLEANFILES = $(gsharp_DATA) $(gsharp_DATA).mdb $(desktop_DATA).in

gsharp.exe: $(gsharp_sources)
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:gsharp.exe $(gsharp_sources) $(packages) $(assemblies)

$(desktop_DATA): $(desktop_DATA).in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@
