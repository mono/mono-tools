SUBDIRS = man framework console rules swf-wizard-runner win32-setup
bin_SCRIPTS = gendarme gendarme-wizard
EXTRA_DIST = gendarme.in gendarme-wizard.in AssemblyInfo.cs.in MIT.X11 gendarme.mds gendarme.sln self-test.ignore

CLEANFILES = gendarme gendarme-wizard bin/rules.xml bin/Gendarme.Rules.*.xml

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
		gendarme.exe gendarme-wizard.exe Gendarme.*.dll; \
	cd ..;

# note: this does not bundle Mono.Cecil.Mdb and Mono.Cecil.Pdb (not built outside Windows)
# unless they are previously (and manually) copied into the bin/ directory
zip-bin: all
	mkdir gendarme.$(GENDARME_VERSION); \
	cp bin/*.exe gendarme.$(GENDARME_VERSION)/; \
	cp bin/*.dll gendarme.$(GENDARME_VERSION)/; \
	cp $(CECIL_ASM) gendarme.$(GENDARME_VERSION)/; \
	cp rules/rules.xml gendarme.$(GENDARME_VERSION)/; \
	cp man/gendarme.1 gendarme.$(GENDARME_VERSION)/; \
	cp AUTHORS README MIT.X11 NEWS gendarme.$(GENDARME_VERSION)/; \
	cd gendarme.$(GENDARME_VERSION); \
	zip gendarme-$(GENDARME_VERSION)-bin.zip *; \
	mv *.zip ..; \
	cd ..; \
	rm -rf gendarme.$(GENDARME_VERSION);

