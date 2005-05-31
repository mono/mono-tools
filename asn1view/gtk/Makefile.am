#
# Makefile.am for gasnview.exe
#
# Authors:
#   Gonzalo Paniagua Javier (gonzalo@ximian.com)
#   Martin Willemoes Hansen (mwh@sysrq.dk>
#
local_flags = -debug+ -debug:full -nologo $(NUNIT_LIBS)

local_flags += $(MCSFLAGS) $(GTK_SHARP_LIBS)

gasnview_SCRIPTS = gasnview.exe
gasnviewdir = $(prefix)/lib/mono/1.0

CLEANFILES = gasnview.exe gasnview.exe.mdb gasnview
EXTRA_DIST = $(gasnview_sources_dist) gui.glade script.in

resources = $(srcdir)/gui.glade \
	$(srcdir)/../art/export-16.png \
	$(srcdir)/../art/export.png \
	$(srcdir)/../art/text_hilight-16.png \
	$(srcdir)/../art/text_lolight-16.png

gasnview_sources = $(srcdir)/Main.cs \
	$(srcdir)/../common/ASN1Decoder.cs \
	$(srcdir)/../common/PrettyPrinter.cs \
	$(srcdir)/../common/OidCache.cs \
	$(srcdir)/../common/AssemblyInfo.cs

gasnview_sources_in = 
gasnview_sources_dist = $(gasnview_sources) $(gasnview_sources_in)
gasnview_sources_build = $(gasnview_sources) $(gasnview_sources_in:.in=)

###
ress= $(foreach res,$(resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

gasnview.exe: $(gasnview_sources_build) $(resources)
	$(MCS) $(MCSFLAGS) $(local_flags) $(ress) -out:$@ $(gasnview_sources_build)

scriptdir = $(bindir)

script_SCRIPTS = gasnview

REWRITE = sed \
	-e 's,@''bindir@,$(bindir),g'				\
	-e 's,@''mono_one_instdir@,$(gasnviewdir),g'	\
	-e 's,@''exe_file@,$@.exe,g'				\
	-e 's,@''RUNTIME@,@RUNTIME@,g'

gasnview: $(srcdir)/script.in Makefile
	$(REWRITE) $(srcdir)/script.in > $@.tmp
	mv $@.tmp $@
