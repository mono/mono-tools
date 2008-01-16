

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=.

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/gui-compare.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

MONO_CECIL_DLL_SOURCE=Mono.Cecil.dll

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/gui-compare.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

MONO_CECIL_DLL_SOURCE=Mono.Cecil.dll

endif


PROGRAMFILES = \
	$(MONO_CECIL_DLL)  

BINARIES = \
	$(GUI_COMPARE)  



GUI_COMPARE = $(BUILD_DIR)/gui-compare
MONO_CECIL_DLL = $(BUILD_DIR)/Mono.Cecil.dll


FILES = \
	gtk-gui/generated.cs \
	MainWindow.cs \
	gtk-gui/MainWindow.cs \
	Main.cs \
	AssemblyInfo.cs \
	InfoManager.cs \
	CompareContext.cs \
	Comparison.cs \
	Metadata.cs \
	MasterMetadata.cs \
	Masterinfo.cs \
	CecilMetadata.cs \
	ProviderSelector.cs \
	gtk-gui/guicompare.ProviderSelector.cs \
	AssemblyResolver.cs \
	Config.cs \
	CustomCompare.cs \
	gtk-gui/GuiCompare.CustomCompare.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/gui.stetic \
	cm/c.gif \
	cm/d.gif \
	cm/e.gif \
	cm/en.gif \
	cm/f.gif \
	cm/i.gif \
	cm/m.gif \
	cm/n.gif \
	cm/p.gif \
	cm/r.gif \
	cm/s.gif \
	cm/sc.gif \
	cm/se.gif \
	cm/sm.gif \
	cm/st.gif \
	cm/sx.gif \
	cm/tb.gif \
	cm/tm.gif \
	cm/tp.gif \
	cm/w.gif \
	cm/y.gif \
	gtk-gui/objects.xml \
	cm/mn.png 

EXTRAS = \
	gui-compare.in 

REFERENCES =  \
	-pkg:gtk-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:glade-sharp-2.0 \
	System \
	Mono.Posix \
	System.Xml

DLL_REFERENCES =  \
	Mono.Cecil.dll

CLEANFILES += $(PROGRAMFILES) $(BINARIES) 

#Targets
all-local: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make

$(GUI_COMPARE): gui-compare
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'
	chmod u+x '$@'

$(MONO_CECIL_DLL): $(MONO_CECIL_DLL_SOURCE)
	mkdir -p $(BUILD_DIR)
	cp '$<' '$@'



gui-compare: gui-compare.in $(top_srcdir)/config.make
	sed -e "s,@prefix@,$(prefix)," -e "s,@PACKAGE@,$(PACKAGE)," < gui-compare.in > gui-compare


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'



$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(dir $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)


install-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(GUI_COMPARE) $(MONO_CECIL_DLL)
	make pre-install-local-hook prefix=$(prefix)
	mkdir -p $(DESTDIR)$(prefix)/lib/$(PACKAGE)
	cp $(ASSEMBLY) $(ASSEMBLY_MDB) $(DESTDIR)$(prefix)/lib/$(PACKAGE)
	mkdir -p $(DESTDIR)$(prefix)/bin
	test -z '$(GUI_COMPARE)' || cp $(GUI_COMPARE) $(DESTDIR)$(prefix)/bin
	test -z '$(MONO_CECIL_DLL)' || cp $(MONO_CECIL_DLL) $(DESTDIR)$(prefix)/lib/$(PACKAGE)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB) $(GUI_COMPARE) $(MONO_CECIL_DLL)
	make pre-uninstall-local-hook prefix=$(prefix)
	rm -f $(DESTDIR)$(prefix)/lib/$(PACKAGE)/$(notdir $(ASSEMBLY))
	test -z '$(ASSEMBLY_MDB)' || rm -f $(DESTDIR)$(prefix)/lib/$(PACKAGE)/$(notdir $(ASSEMBLY_MDB))
	test -z '$(GUI_COMPARE)' || rm -f $(DESTDIR)$(prefix)/bin/$(notdir $(GUI_COMPARE))
	test -z '$(MONO_CECIL_DLL)' || rm -f $(DESTDIR)$(prefix)/lib/$(PACKAGE)/$(notdir $(MONO_CECIL_DLL))
	make post-uninstall-local-hook prefix=$(prefix)
