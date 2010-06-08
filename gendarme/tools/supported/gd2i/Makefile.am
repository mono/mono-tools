EXTRA_DIST = $(gd2i_sources)

gd2i_sources =  \
	../../../AssemblyStaticInfo.cs	\
	../../../AssemblyInfo.cs	\
	gd2i.cs

gd2i.exe:  $(gd2i_sources)
	$(GMCS) -debug $^ -out:$@

update: gd2i.exe
	mono --debug gd2i.exe ../../../bin/self-test.xml ../../../self-test.ignore

self-update: gd2i.exe
	mono --debug gd2i.exe self-test.xml self-test.ignore

self-test: all
	mono --debug ../../../bin/gendarme.exe --config ../../../rules/rules.xml --set self-test --xml self-test.xml --html self-test.html \
		--ignore=self-test.ignore --severity=all --confidence=all gd2i.exe

clean:
	rm -f self-test.xml self-test.html
	rm -f gd2i.exe*

all: gd2i.exe

