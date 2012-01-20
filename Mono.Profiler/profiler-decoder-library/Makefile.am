BUILDDIR= $(top_builddir)/Mono.Profiler/lib
ASSEMBLY = $(BUILDDIR)/mprof-decoder-library.dll

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
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
	BaseTypes.cs \
	Decoder.cs \
	EventProcessor.cs \
	NativeLibraryReader.cs \
	ObjectModel.cs \
	Reader.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES = 

REFERENCES = System

build_references = $(addprefix /r:, $(REFERENCES))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) 
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_references)

EXTRA_DIST = profiler-decoder-library.csproj $(FILES) $(RESOURCES)

