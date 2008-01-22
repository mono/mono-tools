guicomparedir=$(prefix)/lib/gui-compare
guicompare_DATA = gui-compare.exe Mono.Cecil.dll

CLEANFILES = gui-compare.exe gui-compare.exe.mdb Mono.Cecil.dll 

bin_SCRIPTS = gui-compare

gui_compare_sources = \
	$(srcdir)/gtk-gui/generated.cs 			\
	$(srcdir)/MainWindow.cs 			\
	$(srcdir)/gtk-gui/MainWindow.cs 		\
	$(srcdir)/Main.cs 				\
	$(srcdir)/AssemblyInfo.cs 			\
	$(srcdir)/InfoManager.cs 			\
	$(srcdir)/CompareContext.cs 			\
	$(srcdir)/Comparison.cs 			\
	$(srcdir)/Metadata.cs 				\
	$(srcdir)/MasterMetadata.cs 			\
	$(srcdir)/Masterinfo.cs 			\
	$(srcdir)/CecilMetadata.cs 			\
	$(srcdir)/ProviderSelector.cs 			\
	$(srcdir)/gtk-gui/guicompare.ProviderSelector.cs\
	$(srcdir)/AssemblyResolver.cs 			\
	$(srcdir)/Config.cs 				\
	$(srcdir)/CustomCompare.cs 			\
	$(srcdir)/gtk-gui/GuiCompare.CustomCompare.cs

gui_compare_resources = 			\
	$(srcdir)/gtk-gui/gui.stetic		\
	$(srcdir)/cm/c.gif			\
	$(srcdir)/cm/d.gif			\
	$(srcdir)/cm/e.gif			\
	$(srcdir)/cm/en.gif			\
	$(srcdir)/cm/f.gif			\
	$(srcdir)/cm/i.gif			\
	$(srcdir)/cm/m.gif			\
	$(srcdir)/cm/n.gif			\
	$(srcdir)/cm/p.gif			\
	$(srcdir)/cm/r.gif			\
	$(srcdir)/cm/s.gif			\
	$(srcdir)/cm/sc.gif			\
	$(srcdir)/cm/se.gif			\
	$(srcdir)/cm/sm.gif			\
	$(srcdir)/cm/st.gif			\
	$(srcdir)/cm/sx.gif			\
	$(srcdir)/cm/tb.gif			\
	$(srcdir)/cm/tm.gif			\
	$(srcdir)/cm/tp.gif			\
	$(srcdir)/cm/w.gif			\
	$(srcdir)/cm/y.gif			\
	$(srcdir)/gtk-gui/objects.xml		\
	$(srcdir)/cm/mn.png

REWRITE = sed \
	-e 's,@''prefix@,$(prefix),g'	\
	-e 's,@''PACKAGE@,gui-compare,g'	\
	-e 's,@''exe_file@,$@.exe,g'				\
	-e 's,@''RUNTIME@,@RUNTIME@ --debug,g'

gui-compare: gui-compare.in

EXTRA_DIST = $(gui_compare_sources) $(gui_compare_resources) gui-compare.mds gui-compare.mdp  gui-compare.in

packages = -pkg:gtk-sharp-2.0 -pkg:glib-sharp-2.0 -pkg:glade-sharp-2.0 -r:Mono.Cecil.dll
assemblies = -r:System -r:Mono.Posix -r:System.Xml
ress= $(foreach res,$(gui_compare_resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

Mono.Cecil.dll: 
	cp `pkg-config --variable=Libraries cecil` .

gui-compare.exe: $(gui_compare_sources) $(gui_compare_resources) Mono.Cecil.dll
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:gui-compare.exe $(gui_compare_sources) $(ress) $(packages) $(assemblies)
