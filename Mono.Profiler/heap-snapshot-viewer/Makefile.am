
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug "-define:DEBUG"

ASSEMBLY = bin/Debug/mprof-heap-viewer.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../heap-snapshot-explorer/bin/Debug/mprof-heap-snapshot-explorer.dll \
	../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
BUILD_DIR = bin/Debug

MPROF_HEAP_VIEWER_DESKTOP_SOURCE=app.desktop
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB_SOURCE=../heap-snapshot-explorer/bin/Debug/mprof-heap-snapshot-explorer.dll.mdb
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB=$(BUILD_DIR)/mprof-heap-snapshot-explorer.dll.mdb
PROFILER_DECODER_LIBRARY_DLL=
HEAP_SNAPSHOT_EXPLORER_DLL=
MPROF_DECODER_LIBRARY_DLL_MDB_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_MDB=$(BUILD_DIR)/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
MPROF_DECODER_LIBRARY_DLL=$(BUILD_DIR)/mprof-decoder-library.dll
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_SOURCE=../heap-snapshot-explorer/bin/Debug/mprof-heap-snapshot-explorer.dll
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL=$(BUILD_DIR)/mprof-heap-snapshot-explorer.dll
MPROF_HEAP_VIEWER_1_SOURCE=man/man1/mprof-heap-viewer.1
MPROF_HEAP_VIEWER_EXE_MDB_SOURCE=bin/Debug/mprof-heap-viewer.exe.mdb
MPROF_HEAP_VIEWER_EXE_MDB=$(BUILD_DIR)/mprof-heap-viewer.exe.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/mprof-heap-viewer.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../heap-snapshot-explorer/bin/Release/heap-snapshot-explorer.dll \
	../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
BUILD_DIR = bin/Release

MPROF_HEAP_VIEWER_DESKTOP_SOURCE=app.desktop
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB=
PROFILER_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
PROFILER_DECODER_LIBRARY_DLL=$(BUILD_DIR)/profiler-decoder-library.dll
HEAP_SNAPSHOT_EXPLORER_DLL_SOURCE=../heap-snapshot-explorer/bin/Release/heap-snapshot-explorer.dll
HEAP_SNAPSHOT_EXPLORER_DLL=$(BUILD_DIR)/heap-snapshot-explorer.dll
MPROF_DECODER_LIBRARY_DLL_MDB=
MPROF_DECODER_LIBRARY_DLL=
MPROF_HEAP_SNAPSHOT_EXPLORER_DLL=
MPROF_HEAP_VIEWER_1_SOURCE=man/man1/mprof-heap-viewer.1
MPROF_HEAP_VIEWER_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB) \
	$(PROFILER_DECODER_LIBRARY_DLL) \
	$(HEAP_SNAPSHOT_EXPLORER_DLL) \
	$(MPROF_DECODER_LIBRARY_DLL_MDB) \
	$(MPROF_DECODER_LIBRARY_DLL) \
	$(MPROF_HEAP_SNAPSHOT_EXPLORER_DLL) \
	$(MPROF_HEAP_VIEWER_EXE_MDB)  

LINUX_DESKTOPAPPLICATIONS = \
	$(MPROF_HEAP_VIEWER_DESKTOP)  

COMMONAPPLICATIONDATAROOT_MAN_MAN1 = \
	$(MPROF_HEAP_VIEWER_1)  

BINARIES = \
	$(MPROF_HEAP_VIEWER)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(COMMONAPPLICATIONDATAROOT_MAN_MAN1) $(BINARIES) 

FILES = \
	gtk-gui/generated.cs \
	MainWindow.cs \
	Main.cs \
	AssemblyInfo.cs \
	gtk-gui/Mono.Profiler.MainWindow.cs 

DATA_FILES = \
	app.desktop \
	man/man1/mprof-heap-viewer.1 

RESOURCES = \
	gtk-gui/gui.stetic \
	gtk-gui/objects.xml 

EXTRAS = \
	mprof-heap-viewer.in 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(GLADE_SHARP_20_LIBS) \
	System \
	Mono.Posix

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(COMMONAPPLICATIONDATAROOT_MAN_MAN1) $(BINARIES) 

include $(top_srcdir)/Mono.Profiler/Makefile.include

MPROF_HEAP_VIEWER = $(BUILD_DIR)/mprof-heap-viewer
MPROF_HEAP_VIEWER_DESKTOP = $(BUILD_DIR)/mprof-heap-viewer.desktop
MPROF_HEAP_VIEWER_1 = $(BUILD_DIR)/man/man1/mprof-heap-viewer.1

$(eval $(call emit-deploy-wrapper,MPROF_HEAP_VIEWER,mprof-heap-viewer,x))
$(eval $(call emit-deploy-target,MPROF_HEAP_VIEWER_DESKTOP))
$(eval $(call emit-deploy-target,MPROF_HEAP_SNAPSHOT_EXPLORER_DLL_MDB))
$(eval $(call emit-deploy-target,PROFILER_DECODER_LIBRARY_DLL))
$(eval $(call emit-deploy-target,HEAP_SNAPSHOT_EXPLORER_DLL))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL_MDB))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL))
$(eval $(call emit-deploy-target,MPROF_HEAP_SNAPSHOT_EXPLORER_DLL))
$(eval $(call emit-deploy-target,MPROF_HEAP_VIEWER_1))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
