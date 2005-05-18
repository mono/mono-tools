#
# Makefile.am for gnunit.exe
#
# Authors:
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#   Martin Willemoes Hansen (mwh@sysrq.dk>
#
local_flags = -debug+ -debug:full -nologo $(NUNIT_LIBS) -nowarn:169

local_flags += $(MCSFLAGS) $(GTK_SHARP_LIBS)

gnunit_SCRIPTS = gnunit.exe
gnunit2_SCRIPTS = gnunit2.exe
gnunit2dir = $(prefix)/lib/mono/2.0
gnunitdir = $(prefix)/lib/mono/1.0

CLEANFILES = gnunit.exe gnunit.exe.mdb gnunit2.exe gnunit2.exe.mdb gnunit gnunit2
EXTRA_DIST = $(gnunit_sources_dist) nunit-gtk.glade script.in script2.in

resources = $(srcdir)/nunit-gtk.glade \
	   $(srcdir)/../art/nunit-gui.png \
	   $(srcdir)/../art/none.png \
	   $(srcdir)/../art/red.png \
	   $(srcdir)/../art/yellow.png \
	   $(srcdir)/../art/green.png

gnunit_sources = main.cs \
	 AssemblyStore.cs \
	 CircleRenderer.cs \
	 FileDialog.cs \
	 Settings.cs

gnunit_sources_in = AssemblyInfo.cs.in catalog.cs.in
gnunit_sources_dist = $(gnunit_sources) $(gnunit_sources_in)
gnunit_sources_build = $(addprefix $(srcdir)/, $(gnunit_sources))
gnunit_sources_build += $(gnunit_sources_in:.in=)

schema=nunit-gtk.schema
###
ress= $(foreach res,$(resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

gnunit.exe: $(gnunit_sources_build) $(resources)
	$(MCS) $(MCSFLAGS) $(local_flags) $(ress) -out:$@ $(gnunit_sources_build)

gnunit2.exe: gnunit.exe
	$(GMCS) $(MCSFLAGS) $(local_flags) $(ress) -out:$@ $(gnunit_sources_build)

installschema:
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule $(schema)

scriptdir = $(bindir)

script_SCRIPTS = \
	gnunit \
	gnunit2

REWRITE = sed \
	-e 's,@''mono_one_instdir@,$(gnunitdir),g'	\
	-e 's,@''mono_two_instdir@,$(gnunit2dir),g'	\
	-e 's,@''exe_file@,$@.exe,g'				\
	-e 's,@''RUNTIME@,@RUNTIME@ --debug,g'

gnunit :
	$(REWRITE) $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@

gnunit2 :
	$(REWRITE) $(srcdir)/script2.in > $@.tmp
	mv $@.tmp $@
