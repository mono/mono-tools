SUBDIRS = man framework console rules swf-wizard-runner win32-setup
bin_SCRIPTS = gendarme gendarme-wizard
EXTRA_DIST = gendarme.in gendarme-wizard.in AssemblyInfo.cs.in MIT.X11 gendarme.mds gendarme.sln self-test.ignore gendarme.svg

CLEANFILES = gendarme gendarme-wizard bin/rules.xml bin/Gendarme.Rules.*.xml

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = gendarme.svg

DISTCLEANFILES = \
	AssemblyInfo.cs \
	configure \
	config.guess \
	config.sub \
	Makefile.in \
	install-sh \
	aclocal.m4 \
	INSTALL \
	COPYING \
	missing

test: all
	cd framework;	\
	make test;	\
	cd ..;		\
	cd rules;	\
	make test;	\
	cd ..;

run-test: test
	cd framework;	\
	make run-test;	\
	cd ..;		\
	cd rules;	\
	make run-test;	\
	cd ..;

self-test: all
	cd bin;	\
	mono --debug gendarme.exe --config ../rules/rules.xml --set self-test --html self-test.html --ignore=../self-test.ignore \
		--severity=all --confidence=all gendarme.exe gendarme-wizard.exe Gendarme.*.dll; \
	cd ..;
