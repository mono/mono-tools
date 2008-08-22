
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/mprof-decoder.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
BUILD_DIR = bin/Debug

PROFILER_DECODER_LIBRARY_DLL=
MPROF_DECODER_LIBRARY_DLL_MDB_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_MDB=$(BUILD_DIR)/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Debug/mprof-decoder-library.dll
MPROF_DECODER_LIBRARY_DLL=$(BUILD_DIR)/mprof-decoder-library.dll
MPROF_DECODER_1_SOURCE=man/man1/mprof-decoder.1
MPROF_DECODER_EXE_MDB_SOURCE=bin/Debug/mprof-decoder.exe.mdb
MPROF_DECODER_EXE_MDB=$(BUILD_DIR)/mprof-decoder.exe.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/mprof-decoder.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
BUILD_DIR = bin/Release

PROFILER_DECODER_LIBRARY_DLL_SOURCE=../profiler-decoder-library/bin/Release/profiler-decoder-library.dll
PROFILER_DECODER_LIBRARY_DLL=$(BUILD_DIR)/profiler-decoder-library.dll
MPROF_DECODER_LIBRARY_DLL_MDB=
MPROF_DECODER_LIBRARY_DLL=
MPROF_DECODER_1_SOURCE=man/man1/mprof-decoder.1
MPROF_DECODER_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(PROFILER_DECODER_LIBRARY_DLL) \
	$(MPROF_DECODER_LIBRARY_DLL_MDB) \
	$(MPROF_DECODER_LIBRARY_DLL) \
	$(MPROF_DECODER_EXE_MDB)  

COMMONAPPLICATIONDATAROOT_MAN_MAN1 = \
	$(MPROF_DECODER_1)  

BINARIES = \
	$(MPROF_DECODER)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(COMMONAPPLICATIONDATAROOT_MAN_MAN1) $(BINARIES) 

FILES = \
	Main.cs 

DATA_FILES = \
	man/man1/mprof-decoder.1 

RESOURCES = 

EXTRAS = \
	mprof-decoder.in 

REFERENCES = 

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(COMMONAPPLICATIONDATAROOT_MAN_MAN1) $(BINARIES) 

include $(top_srcdir)/Mono.Profiler/Makefile.include

MPROF_DECODER_1 = $(BUILD_DIR)/man/man1/mprof-decoder.1
MPROF_DECODER = $(BUILD_DIR)/mprof-decoder

$(eval $(call emit-deploy-target,PROFILER_DECODER_LIBRARY_DLL))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL_MDB))
$(eval $(call emit-deploy-target,MPROF_DECODER_LIBRARY_DLL))
$(eval $(call emit-deploy-target,MPROF_DECODER_1))
$(eval $(call emit-deploy-wrapper,MPROF_DECODER,mprof-decoder,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
