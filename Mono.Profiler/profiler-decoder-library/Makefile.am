
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/mprof-decoder-library.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

MPROF_DECODER_LIBRARY_DLL_MDB_SOURCE=bin/Debug/mprof-decoder-library.dll.mdb
MPROF_DECODER_LIBRARY_DLL_MDB=$(BUILD_DIR)/mprof-decoder-library.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/profiler-decoder-library.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

MPROF_DECODER_LIBRARY_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(MPROF_DECODER_LIBRARY_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	AssemblyInfo.cs \
	BaseTypes.cs \
	Decoder.cs \
	EventProcessor.cs \
	NativeLibraryReader.cs \
	ObjectModel.cs \
	Reader.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Mono.Profiler/Makefile.include




$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
