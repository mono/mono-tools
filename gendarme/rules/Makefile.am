SUBDIRS= Test.Rules \
	 Gendarme.Rules.BadPractice \
	 Gendarme.Rules.Concurrency \
	 Gendarme.Rules.Correctness \
	 Gendarme.Rules.Design \
	 Gendarme.Rules.Design.Generic \
	 Gendarme.Rules.Design.Linq \
	 Gendarme.Rules.Exceptions \
	 Gendarme.Rules.Interoperability \
	 Gendarme.Rules.Maintainability \
	 Gendarme.Rules.Naming \
	 Gendarme.Rules.Performance \
	 Gendarme.Rules.Portability \
	 Gendarme.Rules.Security \
	 Gendarme.Rules.Security.Cas \
	 Gendarme.Rules.Serialization \
	 Gendarme.Rules.Smells \
	 Gendarme.Rules.Ui

EXTRA_DIST = rules.xml rules.mds tests.mds common.make
DISTCLEANFILES = Makefile.in

install-data-hook:
	$(INSTALL) -c -m 0644 $(addprefix $(srcdir)/, rules.xml) $(DESTDIR)$(prefix)/lib/gendarme;

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
