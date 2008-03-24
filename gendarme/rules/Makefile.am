SUBDIRS= Test.Rules \
	 Gendarme.Rules.Performance Gendarme.Rules.Security \
	 Gendarme.Rules.Correctness Gendarme.Rules.Concurrency \
	 Gendarme.Rules.Portability Gendarme.Rules.Exceptions \
	 Gendarme.Rules.Ui Gendarme.Rules.Naming Gendarme.Rules.Design \
	 Gendarme.Rules.Smells Gendarme.Rules.BadPractice \
	 Gendarme.Rules.Interoperability Gendarme.Rules.Serialization

EXTRA_DIST = rules.xml rules.mds tests.mds
DISTCLEANFILES = Makefile.in

install-data-hook:
	$(INSTALL) -c -m 0644 rules.xml $(DESTDIR)$(prefix)/lib/gendarme;

uninstall-hook:
	rm -f $(DESTDIR)$(prefix)/lib/gendarme/`basename rules.xml`;

test:
	for ASM in $(SUBDIRS); do \
		cd $$ASM;	\
		make test;	\
		cd ..;		\
	done;

run-test:
	for ASM in $(SUBDIRS); do \
		cd $$ASM;	\
		make run-test;	\
		cd ..;		\
	done;

self-test: all
	cp rules.xml ../bin/
	for ASM in $(SUBDIRS); do \
		cd $$ASM;	\
		make self-test;	\
		cd ..;		\
	done;

