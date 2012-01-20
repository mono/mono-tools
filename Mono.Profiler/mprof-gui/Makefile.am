ASSEMBLY = $(top_builddir)/Mono.Profiler/lib/emveepee.exe

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -debug "-define:DEBUG"
ASSEMBLY_MDB = $(ASSEMBLY).mdb
else
CSFLAGS =  -noconfig -codepage:utf8 -warn:4
ASSEMBLY_MDB = 
endif

programfilesdir = $(pkglibdir)
programfiles_DATA = $(ASSEMBLY) $(ASSEMBLY_MDB)
bin_SCRIPTS = emveepee

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_MDB)

FILES =  \
	AssemblyInfo.cs \
	Main.cs \
	MainWindow.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES =

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))
build_resources_args = $(addprefix -resource:, $(build_resources))

REFERENCES =  \
	-pkg:gtk-sharp-2.0 \
	-r:Mono.Posix \
	-r:System

DLL_REFERENCES = 

PROJECT_REFERENCES =  \
	../lib/Mono.Profiler.Widgets.dll \
	../lib/mprof-decoder-library.dll

build_references = $(addprefix -r:, $(PROJECT_REFERENCES)) $(REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:exe $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = mprof-gui.csproj $(FILES) $(RESOURCES)

