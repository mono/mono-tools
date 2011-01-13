gsharpdir = $(prefix)/lib/gsharp

if DMCS_PRESENT
COMPILER=$(DMCS)
else
COMPILER=$(GMCS)
endif

bin_SCRIPTS = gsharp

gsharp_DATA = gsharp.exe gsharp.exe.config

desktopdir = $(datadir)/applications
desktop_DATA = gsharp.desktop

packages = -pkg:gtk-sharp-2.0 -pkg:glib-sharp-2.0
assemblies = -r:System -r:Mono.CSharp -r:Mono.Management -r:Mono.Posix -r:System.Drawing -r:System.Xml.Linq -r:System.Core -r:System.Xml

gsharp_sources =                        			\
        $(srcdir)/AssemblyInfo.cs       			\
	$(srcdir)/DescribeType.cs				\
        $(srcdir)/InteractiveGraphicsBase.cs			\
        $(srcdir)/Main.cs               			\
        $(srcdir)/MainWindow.cs         			\
	$(srcdir)/Options.cs	         			\
        $(srcdir)/ProcessSelector.cs    			\
        $(srcdir)/Shell.cs              			\
        $(srcdir)/SysDrawing.cs					\
	$(srcdir)/TypeView.cs					\
        $(srcdir)/gtk-gui/generated.cs  			\
        $(srcdir)/gtk-gui/Mono.CSharp.Gui.MainWindow.cs 	\
        $(srcdir)/gtk-gui/Mono.CSharp.Gui.ProcessSelector.cs	\
	$(srcdir)/gtk-gui/Mono.CSharp.Gui.DescribeType.cs	\
	$(srcdir)/outline.cs

EXTRA_DIST = $(gsharp_sources) gsharp.in $(desktop_DATA).in close.png gsharp.exe.config.in
CLEANFILES = $(gsharp_DATA) $(gsharp_DATA).mdb $(desktop_DATA)

gsharp.exe: $(gsharp_sources)
	$(COMPILER) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:gsharp.exe $(gsharp_sources) $(packages) $(assemblies) -resource:$(srcdir)/close.png,close.png

$(desktop_DATA): $(desktop_DATA).in
	sed -e "s|\@bindir\@|$(bindir)|" $< > $@
