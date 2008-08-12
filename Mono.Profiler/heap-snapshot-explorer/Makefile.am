
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"

ASSEMBLY = bin/Debug/mprof-heap-snapshot-explorer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
BUILD_DIR = bin/Debug

MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB_SOURCE=bin/Debug/mprof-heap-snapshot-explorer.dll.mdb
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB=$(BUILD_DIR)/mprof-heap-snapshot-explorer.dll.mdb
PROFILER_DECODER_LIBRARY_DLL=
MPROF_DECODER_LIBRARY_DLL_MDB_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_MDB=$(BUILD_DIR)/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
MPROF_DECODER_LIBRARY_DLL=$(BUILD_DIR)/mprof-decoder-library.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/heap-snapshot-explorer.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
BUILD_DIR = bin/Release

MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB=
PROFILER_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
PROFILER_DECODER_LIBRARY_DLL=$(BUILD_DIR)/profiler-decoder-library.dll
MPROF_DECODER_LIBRARY_DLL_MDB=
MPROF_DECODER_LIBRARY_DLL=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB) \
	$(PROFILER_DECODER_LIBRARY_DLL) \
	$(MPROF_DECODER_LIBRARY_DLL_MDB) \
	$(MPROF_DECODER_LIBRARY_DLL)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	MyClass.cs \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	HeapSnapshotExplorer.cs \
	HeapExplorerActions.cs \
	gtk-gui/Mono.Profiler.HeapExplorerActions.cs \
	HeapExplorerTreeModel.cs \
	gtk-gui/Mono.Profiler.HeapSnapshotExplorer.cs \
	LoadedClassChooser.cs \
	gtk-gui/Mono.Profiler.LoadedClassChooser.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml 

EXTRAS = 

REFERENCES =  \
	System \
	$(GTK_SHARP_20_LIBS) \
	Mono.Posix

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Mono.Profiler/Makefile.include


$(eval $(call emit-deploy-target,PROFILER_DECODER_LIBRARY_DLL))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL_MDB))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
