man_MANS = gendarme.1 gendarme.5 gd2i.1
EXTRA_DIST = gendarme.1.in gendarme.5.in gd2i.1.in gendarme.config gendarme.source
DISTCLEANFILES = Makefile.in gendarme.1 gendarme.5 gd2i.1
CLEANFILES = gendarme.tree gendarme.zip

rules_documentationdir = $(prefix)/lib/monodoc/sources
rules_documentation_DATA = gendarme.source gendarme.tree gendarme.zip

all: gendarme.tree gendarme.zip

gendarme.zip : gendarme.tree
gendarme.tree: gendarme.config Makefile
	mdoc assemble -o gendarme -f man $<

