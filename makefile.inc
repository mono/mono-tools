# top-level makefile.

DIRS=type-reflector debugger

all:
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f $(MAKEFILE) $@ || exit 1; \
	done

install: all
	if test x$$prefix = x; then \
		echo Usage is: make -f $(MAKEFILE) install prefix=YOURPREFIX; \
		exit 1; \
	fi;
	mkdir -p $(prefix)/bin/
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f $(MAKEFILE) $@ || exit 1; \
	done

clean:
	rm -f *~
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f $(MAKEFILE) $@ || exit 1; \
	done

