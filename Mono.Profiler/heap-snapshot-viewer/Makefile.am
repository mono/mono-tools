ASSEMBLY = $(top_builddir)/Mono.Profiler/lib/mprof-heap-viewer.exe

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"
ASSEMBLY_MDB = $(ASSEMBLY).mdb
else
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY_MDB = 
endif

programfilesdir = $(pkglibdir)
programfiles_DATA = $(ASSEMBLY) $(ASSEMBLY_MDB)
bin_SCRIPTS = mprof-heap-viewer
man_MANS=man/man1/mprof-heap-viewer.1

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_MDB)

FILES =  \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	gtk-gui/Mono.Profiler.MainWindow.cs \
	Main.cs \
	MainWindow.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES =  \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml 

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))
build_resources_args = $(addprefix -resource:, $(build_resources))

REFERENCES =  \
	$(GLADE_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(GTK_SHARP_LIBS) \
	-r:Mono.Posix \
	-r:System

DLL_REFERENCES = 

PROJECT_REFERENCES =  \
	../lib/mprof-decoder-library.dll \
	../lib/mprof-heap-snapshot-explorer.dll

build_references = $(addprefix -r:, $(PROJECT_REFERENCES)) $(REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:exe $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = heap-snapshot-viewer.csproj $(FILES) $(RESOURCES) $(man_MANS)

