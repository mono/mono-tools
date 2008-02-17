SUBDIRS = man framework console rules
bin_SCRIPTS = gendarme
EXTRA_DIST = gendarme.in AssemblyInfo.cs.in MIT.X11 gendarme.mds gendarme.sln self-test.ignore

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

clean-local:
	rm -fr doc

doc: all
	monodocer -assembly:bin/Gendarme.Framework.dll -importslashdoc:framework/Gendarme.Framework.doc.xml -path:doc

assemble-doc: doc
	mdassembler --ecma doc -o doc/gendarme; \
	echo -e "<?xml version='1.0'?>\n<monodoc>\n\t<source provider='ecma' basefile='gendarme' path='gendarme'/>\n</monodoc>" > doc/gendarme.source; 

self-test: all
	cd bin;	\
	mono --debug gendarme.exe --config ../rules/rules.xml --set self-test --html self-test.html --ignore=../self-test.ignore \
		gendarme.exe Gendarme.*.dll; \
	cd ..;
