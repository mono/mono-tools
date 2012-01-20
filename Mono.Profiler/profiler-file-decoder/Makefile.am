BUILDDIR = $(top_builddir)/Mono.Profiler/lib
ASSEMBLY = $(BUILDDIR)/mprof-decoder.exe

if ENABLE_DEBUG
CSFLAGS = -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY_MDB = $(ASSEMBLY).mdb
else
CSFLAGS = -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY_MDB = 
endif

programfilesdir = $(pkglibdir)
programfiles_DATA = $(ASSEMBLY) $(ASSEMBLY_MDB)
bin_SCRIPTS = mprof-decoder
man_MANS = man/man1/mprof-decoder.1 

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_MDB)

FILES = Main.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES = 

REFERENCES = 

DLL_REFERENCES = 

PROJECT_REFERENCES = ../lib/mprof-decoder-library.dll

build_references = $(addprefix /r:, $(PROJECT_REFERENCES))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:exe $(build_sources) $(build_references)

EXTRA_DIST = profiler-file-decoder.csproj $(FILES) $(RESOURCES) $(man_MANS)

