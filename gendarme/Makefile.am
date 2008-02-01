SUBDIRS = man framework console rules
bin_SCRIPTS = gendarme
EXTRA_DIST = gendarme.in AssemblyInfo.cs.in MIT.X11 gendarme.mds gendarme.sln

CLEANFILES = gendarme bin/rules.xml bin/Gendarme.Rules.*.xml

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
	cp rules/rules.xml bin; \
	cd bin;	\
	mono --debug gendarme.exe --set self-test --html self-test.html gendarme.exe Gendarme.*.dll;	\
	cd ..;
