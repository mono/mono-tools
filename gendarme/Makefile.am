SUBDIRS = man framework console rules swf-wizard-runner win32-setup
bin_SCRIPTS = gendarme gendarme-wizard
EXTRA_DIST = gendarme.in gendarme-wizard.in AssemblyInfo.cs.in MIT.X11 gendarme.mds gendarme.sln self-test.ignore gendarme.svg

CLEANFILES = gendarme gendarme-wizard bin/rules.xml

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
	
TEST1 ?= AvoidVisibleConstantFieldTest
test1_file := $(shell find rules -name "\.svn" -prune -o  -name "*$(TEST1)*" -print)
test1_name := $(strip $(shell echo "$(test1_file)" | grep -oE "Gendarme\.Rules\.[^\\/]+"))
test1_namespace := $(strip $(shell echo "$(test1_name)" | sed "s/Gendarme/Test/"))
wdir := $(shell pwd)

test1:
	cd framework && make test
	cd $(wdir)/rules/Test.Rules && make test
	cd $(wdir)/rules/$(test1_name) && make test
	cd $(wdir)/rules/$(test1_name) && MONO_PATH=$(wdir)/bin/:$(wdir)/rules/Test.Rules/:$(MONO_PATH) \
	   nunit-console2 -fixture=$(test1_namespace).$(TEST1) -nologo $(test1_namespace).dll

help:
	@echo "The targets are:"
	@echo "all        - build everything"
	@echo "test       - build the unit test dlls"
	@echo "run-test   - build and run the unit tests"
	@echo "self-test  - run gendarme on itself, bin/self-test.html will have the results"
	@echo "test1      - build and run a single unit test named TEST1"
	@echo " "
	@echo "Variables include:"
	@echo "DEBUG  - compile a debug version of the assemblies"
	@echo "TEST1  - used with test1 target, should be a unit test class name"

