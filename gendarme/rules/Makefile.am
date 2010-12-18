RULEDIRS= Gendarme.Rules.BadPractice \
	 Gendarme.Rules.Concurrency \
	 Gendarme.Rules.Correctness \
	 Gendarme.Rules.Design \
	 Gendarme.Rules.Design.Generic \
	 Gendarme.Rules.Design.Linq \
	 Gendarme.Rules.Exceptions \
	 Gendarme.Rules.Globalization \
	 Gendarme.Rules.Gendarme \
	 Gendarme.Rules.Interoperability \
	 Gendarme.Rules.Interoperability.Com \
	 Gendarme.Rules.Maintainability \
	 Gendarme.Rules.Naming \
	 Gendarme.Rules.NUnit \
	 Gendarme.Rules.Performance \
	 Gendarme.Rules.Portability \
	 Gendarme.Rules.Security \
	 Gendarme.Rules.Security.Cas \
	 Gendarme.Rules.Serialization \
	 Gendarme.Rules.Smells \
	 Gendarme.Rules.Ui
TESTDIRS = Test.Rules

SUBDIRS = $(TESTDIRS) $(RULEDIRS)

EXTRA_DIST = rules.xml common.make
DISTCLEANFILES = Makefile.in

install-data-hook:
	$(INSTALL) -c -m 0644 $(addprefix $(srcdir)/, rules.xml) $(DESTDIR)$(prefix)/lib/gendarme;

uninstall-hook:
	rm -f $(DESTDIR)$(prefix)/lib/gendarme/`basename rules.xml`;
 
rules:
	for ASM in $(RULEDIRS); do \
		cd $$ASM; \
		make rule; \
		cd ..;	 \
	done;

test:
	list='$(SUBDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) test) ; \
	done;

run-test:
	list='$(TESTDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) test) ; \
	done;
	list='$(RULEDIRS)'; for d in $$list ; do \
	    (cd $$d && $(MAKE) run-test) ; \
	done;

