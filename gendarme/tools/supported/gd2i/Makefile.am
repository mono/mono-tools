EXTRA_DIST = $(gd2i_sources) gd2i.csproj

gd2idir=$(prefix)/lib/gendarme
gd2i_SCRIPTS = ../../../bin/gd2i.exe
CLEANFILES = $(gd2i_SCRIPTS) $(gd2i_SCRIPTS).mdb
DISTCLEANFILES = Makefile.in

gd2i_sources =  \
	../../../AssemblyStaticInfo.cs	\
	../../../AssemblyInfo.cs	\
	../../../console/Options.cs	\
	gd2i.cs

$(gd2i_SCRIPTS):  $(gd2i_sources)
	$(GMCS) -d:CODE_ANALYSIS -debug $^ -out:$@

update: $(gd2i_SCRIPTS)
	mono --debug $(gd2i_SCRIPTS) ../../../bin/self-test.xml ../../../self-test.ignore

self-test: all
	mono --debug ../../../bin/gendarme.exe --config ../../../rules/rules.xml --set self-test --xml self-test.xml --html self-test.html \
		--ignore=../../../mono-options.ignore --severity=all --confidence=all $(gd2i_SCRIPTS)

clean:
	rm -f $(CLEANFILES) self-test.xml self-test.html

all: $(gd2i_SCRIPTS)

