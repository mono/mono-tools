mperfmondir=$(prefix)/lib/mperfmon
bin_SCRIPTS = mperfmon
mperfmon_DATA = mperfmon.exe config
man_MANS=mperfmon.1

CLEANFILES = mperfmon.exe mperfmon.exe.mdb mperfmon

mperfmon_sources = \
	$(srcdir)/Main.cs 				\
	$(srcdir)/AssemblyInfo.cs 			\
	$(srcdir)/MainWindow.cs 			\
	$(srcdir)/NewCounter.cs 			\
	$(srcdir)/Preferences.cs 			\
	$(srcdir)/Config.cs 			\
	$(srcdir)/AddSet.cs 			\
	$(srcdir)/gtk-gui/generated.cs 			\
	$(srcdir)/gtk-gui/MainWindow.cs 		\
	$(srcdir)/gtk-gui/mperfmon.AddSet.cs	\
	$(srcdir)/gtk-gui/mperfmon.Preferences.cs	\
	$(srcdir)/gtk-gui/mperfmon.NewCounter.cs

mperfmon_resources = 			\
	$(srcdir)/gtk-gui/gui.stetic

EXTRA_DIST = $(mperfmon_sources) $(mperfmon_resources) mperfmon.in config $(man_MANS)

packages = -pkg:gtk-sharp-2.0
assemblies = -r:System -r:System.Xml -r:Mono.Cairo -r:Mono.Posix
ress= $(foreach res,$(mperfmon_resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

mperfmon.exe: $(mperfmon_sources) $(mperfmon_resources)
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:mperfmon.exe $(mperfmon_sources) $(ress) $(packages) $(assemblies)

