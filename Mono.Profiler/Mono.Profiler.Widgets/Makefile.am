BUILDDIR= $(top_builddir)/Mono.Profiler/lib
ASSEMBLY = $(BUILDDIR)/Mono.Profiler.Widgets.dll

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -define:DEBUG
ASSEMBLY_MDB = $(ASSEMBLY).mdb
else
CSFLAGS =  -noconfig -codepage:utf8 -warn:4
ASSEMBLY_MDB = 
endif

pkglib_DATA = $(ASSEMBLY) $(ASSEMBLY_MDB)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY_MDB)

FILES =  \
	AllocationsStore.cs \
	AllocationsView.cs \
	AssemblyInfo.cs \
	CallsStore.cs \
	CallsView.cs \
	Node.cs \
	ProfileSetupDialog.cs \
	ProfileStore.cs \
	ProfileView.cs

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES = 

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))
build_resources_args = $(addprefix -resource:, $(build_resources))

REFERENCES =  \
	-pkg:glib-sharp-2.0 \
	-pkg:gtk-sharp-2.0 \
	-r:System

PROJECT_REFERENCES = ../lib/mprof-decoder-library.dll

build_references = $(addprefix /r:, $(PROJECT_REFERENCES)) $(REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources)
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = Mono.Profiler.Widgets.csproj $(FILES) $(RESOURCES)

