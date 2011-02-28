TESTDIRS = framework rules
SUBDIRS = $(TESTDIRS) console swf-wizard-runner man wix tools
bin_SCRIPTS = gendarme gendarme-wizard gd2i
EXTRA_DIST = gendarme.in gendarme-wizard.in AssemblyInfo.cs.in MIT.X11 gendarme.sln self-test.ignore mono-options.ignore gendarme.svg options.make

CLEANFILES = gendarme gendarme-wizard bin/rules.xml

pixmapdir = $(datadir)/pixmaps
pixmap_DATA = gendarme.svg

DISTCLEANFILES = \
	AssemblyInfo.cs \
	Makefile.in
	
wdir = $(shell pwd)

# Note that PHONY is required on os-x because a directory exists with
# that name.
.PHONY: console
console:
	cd framework && make ../bin/Gendarme.Framework.dll
	cd $(wdir)/console && make ../bin/gendarme.exe
	cd $(wdir)/rules && make rules

bin: console
	cd $(wdir)/swf-wizard-runner && make ../bin/gendarme-wizard.exe

test:
	list='$(TESTDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) test) ; \
	done;

run-test: bin/gendarme.exe.config
	list='$(TESTDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) run-test) ; \
	done;

self-test: all bin/gendarme.exe.config
	rm -f gendarme.log; \
	mono --debug bin/gendarme.exe --config rules/rules.xml --set self-test --xml bin/self-test.xml --html bin/self-test.html \
		--ignore=self-test.ignore --severity=all --confidence=all bin/gendarme.exe bin/gendarme-wizard.exe bin/Gendarme.*.dll

check-test: all bin/gendarme.exe.config test
	mono --debug bin/gendarme.exe --config rules/rules.xml --set unit-test --xml bin/unit-test.xml --html bin/unit-test.html \
		--ignore=unit-test.ignore --severity=all --confidence=all @unit-test.list

test-regress: all
	mono --debug bin/gendarme.exe --config rules/rules.xml --set self-test --log regress.log \
		--severity=all --confidence=all testcases/*.dll testcases/*.exe
	
TEST1 ?= AvoidVisibleConstantFieldTest
test1_file = $(shell find rules -name "\.svn" -prune -o  -name "*$(TEST1)*" -print)
test1_name = $(strip $(shell echo "$(test1_file)" | grep -oE "Gendarme\.Rules\.[^\\/]+"))
test1_namespace = $(strip $(shell echo "$(test1_name)" | sed "s/Gendarme/Test/"))

test1: bin/gendarme.exe.config
	cd framework && make test
	cd $(wdir)/rules/Test.Rules && make test
	cd $(wdir)/rules/$(test1_name) && make test
	rm -f $(wdir)/gendarme.log
	cp $(wdir)/bin/gendarme.exe.config $(wdir)/rules/$(test1_name)/$(test1_namespace).dll.config
	cd $(wdir)/rules/$(test1_name) && MONO_PATH=$(wdir)/bin/:$(wdir)/rules/Test.Rules/:$(MONO_PATH) \
	   nunit-console2 -fixture=$(test1_namespace).$(TEST1) -nologo $(test1_namespace).dll

bin/gendarme.exe.config:
	cp console/gendarme.exe.config bin/gendarme.exe.config

help:
	@echo "The targets are:"
	@echo "all        - build everything, including documentation"
	@echo "bin        - build all binaries needed to run gendarme"
	@echo "console    - build everything to execute the console runner"
	@echo "test       - build the unit test dlls"
	@echo "run-test   - build and run the unit tests"
	@echo "self-test  - run gendarme on itself, bin/self-test.html will have the results"
	@echo "test1      - build and run a single unit test named TEST1"
	@echo "zip-bin    - build then create a ZIP file of the binaries needed to run gendarme"
	@echo " "
	@echo "Variables include:"
	@echo "DEBUG  - compile a debug version of the assemblies"
	@echo "TEST1  - used with test1 target, should be a unit test class name"

# note: we take care to bundle the version of Mono.Cecil.dll that we compiled against
extra-bin:
	mkdir -p extra; \
	cp -u $(CECIL_ASM) extra;

# note: this does not bundle Mono.Cecil.Mdb.dll and Mono.Cecil.Pdb.dll (not built outside Windows)
# unless they are previously (and manually) copied into the extra/ directory
zip-bin: bin extra-bin
	mkdir gendarme.$(GENDARME_VERSION); \
	cd gendarme.$(GENDARME_VERSION); \
	cp ../bin/*.exe .; \
	cp ../bin/*.dll .; \
	cp ../rules/rules.xml .; \
	cp ../man/gendarme.1 .; \
	cp ../man/gendarme.5 .; \
	cp ../AUTHORS ../README ../MIT.X11 ../NEWS .; \
	cp ../extra/* .; \
	zip gendarme-$(GENDARME_VERSION)-bin.zip *; \
	mv *.zip ..; \
	cd ..; \
	rm -rf gendarme.$(GENDARME_VERSION);

