BUILDDIR = $(top_builddir)/Mono.Profiler/lib
ASSEMBLY = $(BUILDDIR)/mprof-heap-snapshot-explorer.dll

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"
ASSEMBLY_MDB = $(ASSEMBLY).mdb
else
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY_MDB = 
endif

programfilesdir = $(pkglibdir)
programfiles_DATA = $(ASSEMBLY) $(ASSEMBLY_MDB)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_MDB)

FILES =  \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	gtk-gui/Mono.Profiler.HeapExplorerActions.cs \
	gtk-gui/Mono.Profiler.HeapSnapshotExplorer.cs \
	gtk-gui/Mono.Profiler.LoadedClassChooser.cs \
	HeapExplorerActions.cs \
	HeapExplorerTreeModel.cs \
	HeapSnapshotExplorer.cs \
	LoadedClassChooser.cs \
	MyClass.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES =  \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml 

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))

build_resources_args = $(addprefix -resource:, $(build_resources)) 

REFERENCES =

PACKAGE_REFERENCES =  \
	-pkg:gtk-sharp-2.0 \
	-r:Mono.Posix \
	-r:System

DLL_REFERENCES = 

PROJECT_REFERENCES = ../lib/mprof-decoder-library.dll

build_references = $(addprefix /r:,$(PROJECT_REFERENCES)) $(PACKAGE_REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(PROJECT_REFERENCES)
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = heap-snapshot-explorer.csproj $(FILES) $(RESOURCES)
