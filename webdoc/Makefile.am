monodoc_DATA = assembler.exe normalize.exe monodoc.xml mod.exe validate.exe cs2ecma.exe
noinst_DATA = monodoc.dll monodoc.dll.config
CLEANFILES = monodoc.dll assembler.exe $(monodoc_unix_DATA) normalize.exe mod.exe validate.exe cs2ecma.exe
DISTCLEANFILES = \
	*.mdb \
	AssemblyInfo.cs \
	lucene_sources \
	mono.snk \
	monodoc.dll.config \
	Test/man-provider-test.exe*

if USE_CYGPATH
GACDIR=`cygpath -w $(prefix)/lib`
GACROOT=`cygpath -w $(DESTDIR)$(prefix)/lib`
else
GACDIR=$(prefix)/lib
GACROOT=$(DESTDIR)$(prefix)/lib
endif

.PHONY :  \
	check \
	check-man-provider \
	check-man-provider-update \
	check-validate-update \
	check-validate \
	cleandb \
	db \
	dist-hook \
	install-data-local \
	web \
	uninstall-local \
	up

monodoc_sources = \
	$(srcdir)/colorizer.cs		\
	$(srcdir)/man-provider.cs 	\
	$(srcdir)/addins-provider.cs 	\
	$(srcdir)/monohb-provider.cs 	\
	$(srcdir)/xhtml-provider.cs 	\
	$(srcdir)/ecma-provider.cs	\
	$(srcdir)/simple-provider.cs 	\
	$(srcdir)/html-helper.cs	\
	$(srcdir)/provider.cs 		\
	$(srcdir)/index.cs 		\
	$(srcdir)/error-provider.cs 	\
	$(srcdir)/ecmaspec-provider.cs 	\
	$(srcdir)/editing.cs 		\
	$(srcdir)/settings.cs		\
	$(srcdir)/commentservice.cs	\
	$(srcdir)/XmlDocUtils.cs \
	$(srcdir)/XmlNodeWriter.cs	\
	$(srcdir)/SearchableIndex.cs	\
	$(srcdir)/SearchableDocument.cs	\
	$(srcdir)/wiki2ecma.cs		\
	$(srcdir)/ecma2wiki.cs		\
	$(srcdir)/ManifestResourceResolver.cs \
	AssemblyInfo.cs	

monodoc_resources = \
	$(srcdir)/base.css \
	$(srcdir)/ecmaspec.css \
	$(srcdir)/ecmaspec-html.xsl \
	$(srcdir)/ecmaspec-html-css.xsl \
	$(srcdir)/home.html \
	$(srcdir)/mdoc-html-utils.xsl \
	$(srcdir)/mdoc-sections.xsl \
	$(srcdir)/mdoc-sections-css.xsl \
	$(srcdir)/mono-ecma.css \
	$(srcdir)/mono-ecma.xsl \
	$(srcdir)/mono-ecma-css.js \
	$(srcdir)/mono-ecma-css.xsl \
	$(srcdir)/mono-ecma-impl.xsl \
	$(srcdir)/helper.js \
	$(srcdir)/web/xtree/images/clean/Lplus.gif \
	$(srcdir)/web/xtree/images/clean/Lminus.gif

lucene_sources = \
	$(srcdir)/Lucene.Net.dll.sources 

assembler_sources = \
	$(srcdir)/assembler.cs 

dump_sources      = \
	$(srcdir)/dump.cs 

validate_sources = \
	$(srcdir)/validate.cs

cs2ecma_sources = \
	$(srcdir)/cs2ecma.cs

MAN_TEST_FILES = \
	$(srcdir)/Test/man-provider-test.cs \
	$(srcdir)/Test/man-test1.1 \
	$(srcdir)/Test/man-test1.html \
	$(srcdir)/Test/man-test2.1 \
	$(srcdir)/Test/man-test2.html \
	$(srcdir)/Test/man-test3.1 \
	$(srcdir)/Test/man-test3.html \
	$(srcdir)/Test/man-test4.1 \
	$(srcdir)/Test/man-test4.html

VAL_TEST_FILES = \
	$(srcdir)/Test/validate.check.monodocer \
	$(srcdir)/Test/validate.check.monodocer.importslashdoc \
	$(srcdir)/Test/validate.check.monodocer.since

EXTRA_DIST = \
	$(monodoc_sources) $(assembler_sources) \
	$(monodoc_resources)			\
	$(dump_sources) 			\
	$(lucene_sources)			\
	$(validate_sources) monodoc-ecma.xsd	\
	monodoc.xml \
	normalize.cs monodoc.dll.config.in	\
	mod.cs		\
	AssemblyInfo.cs.in $(cs2ecma_sources) 	\
	$(MAN_TEST_FILES) \
	$(VAL_TEST_FILES)

monodoc_FILES = assembler.exe normalize.exe validate.exe cs2ecma.exe

lucene_warnings = -nowarn:169,164,219,168,162

assembler.exe: $(assembler_sources) monodoc.dll
	$(CSC) -debug /out:assembler.exe $(assembler_sources) -r:ICSharpCode.SharpZipLib.dll -r:./monodoc.dll

dump.exe: $(dump_sources) monodoc.dll
	$(CSC) -debug -out:dump.exe $(dump_sources)  -r:ICSharpCode.SharpZipLib.dll -r:./monodoc.dll

normalize.exe: normalize.cs
	$(CSC) $(srcdir)/normalize.cs -out:normalize.exe

validate.exe: validate.cs $(srcdir)/monodoc-ecma.xsd
	$(CSC) $(srcdir)/validate.cs -out:validate.exe /resource:$(srcdir)/monodoc-ecma.xsd,monodoc-ecma.xsd

cs2ecma.exe: cs2ecma.cs
	$(CSC) $(srcdir)/cs2ecma.cs -out:cs2ecma.exe

mono.snk: $(top_srcdir)/mono.snk
	cp $(top_srcdir)/mono.snk .

monodoc.dll: lucene_sources $(monodoc_sources) $(monodoc_resources) mono.snk
	$(CSC) /debug /codepage:utf8 /nowarn:169,164,162,168,219 \
		-out:monodoc.dll -target:library \
		/resource:$(srcdir)/mono-ecma-impl.xsl,mono-ecma-impl.xsl \
		/resource:$(srcdir)/mdoc-sections-css.xsl,mdoc-sections-css.xsl \
		/resource:$(srcdir)/mdoc-sections.xsl,mdoc-sections.xsl \
		/resource:$(srcdir)/mdoc-html-utils.xsl,mdoc-html-utils.xsl \
		/resource:$(srcdir)/mono-ecma.xsl,mono-ecma.xsl \
		/resource:$(srcdir)/mono-ecma-css.xsl,mono-ecma-css.xsl \
		/resource:$(srcdir)/mono-ecma-css.js,mono-ecma-css.js \
		/resource:$(srcdir)/mono-ecma.css,mono-ecma.css \
		/resource:$(srcdir)/ecmaspec-html.xsl,ecmaspec-html.xsl \
		/resource:$(srcdir)/ecmaspec-html-css.xsl,ecmaspec-html-css.xsl \
		/resource:$(srcdir)/base.css,base.css \
		/resource:$(srcdir)/ecmaspec.css,ecmaspec.css \
		/resource:$(srcdir)/home.html,home.html \
		/resource:$(srcdir)/helper.js,helper.js \
		/resource:$(srcdir)/web/xtree/images/clean/Lplus.gif \
		/resource:$(srcdir)/web/xtree/images/clean/Lminus.gif \
		$(monodoc_sources)  @lucene_sources \
		-r:ICSharpCode.SharpZipLib.dll -r:Commons.Xml.Relaxng \
		-r:System.Web -r:System.Web.Services

lucene_sources: $(srcdir)/Lucene.Net.dll.sources
	sed "s,\@srcdir\@,$(srcdir)," < $(srcdir)/Lucene.Net.dll.sources > lucene_sources

monodoc.dll.config: $(srcdir)/monodoc.dll.config.in Makefile
	if sed 's,@''monodoc_refdir@,$(monodoc_refdir),' $(srcdir)/monodoc.dll.config.in > $@t; then mv $@t $@; else rm -f $@t ; exit 1; fi

mod.exe: mod.cs monodoc.dll
	$(CSC) $(srcdir)/mod.cs -r:./monodoc.dll -out:mod.exe

dist-hook: assembler.exe
	mkdir -p $(distdir)/web/images
	mkdir -p $(distdir)/web/xtree/images/msdn
	mkdir -p $(distdir)/web/xtree/images/msdn2
	mkdir -p $(distdir)/web/xtree/images/xp
	mkdir -p $(distdir)/web/ptree
	cp $(srcdir)/web/index.aspx     $(srcdir)/web/monodoc.ashx $(distdir)/web
	cp $(srcdir)/web/header.aspx    $(distdir)/web
	cp $(srcdir)/web/web.config     $(distdir)/web
	cp $(srcdir)/web/common.css     $(distdir)/web
	cp $(srcdir)/web/sidebar.css    $(distdir)/web
	cp $(srcdir)/web/sidebar.js     $(distdir)/web
	cp $(srcdir)/web/images/*gif  $(distdir)/web/images
	cp $(srcdir)/web/images/*png    $(distdir)/web/images
	cp $(srcdir)/web/xtree/images/msdn/*gif $(distdir)/web/xtree/images/msdn
	cp $(srcdir)/web/xtree/images/msdn2/*gif $(distdir)/web/xtree/images/msdn2
	cp $(srcdir)/web/xtree/images/xp/*png $(distdir)/web/xtree/images/xp
	cp $(srcdir)/web/xtree/images/*png $(distdir)/web/xtree/images
	cp $(srcdir)/web/xtree/*js $(distdir)/web/xtree
	cp $(srcdir)/web/xtree/*css $(distdir)/web/xtree
	cp $(srcdir)/web/ptree/*css $(distdir)/web/ptree
	cp $(srcdir)/web/ptree/*js $(distdir)/web/ptree
	cp -a $(srcdir)/Lucene.Net $(distdir)


bin/Monodoc.Contributions.dll: server.cs 
	-mkdir bin
	mcs -g server.cs -target:library -out:bin/Monodoc.Contributions.dll -r:System.Web -r:System.Web.Services -r:./monodoc.dll -r:System.Data -r:ByteFX.Data

web: bin/Monodoc.Contributions.dll
	(cd web && xsp)

db:
	mysql -u admin -p 

cleandb:
	mysql -u admin -p < tables.sql

up:
	scp tables.sql server.cs server.asmx monodoc.dll root@www.go-mono.com:

install-data-local:
	$(GACUTIL) /i monodoc.dll /f /package monodoc /gacdir $(GACDIR) /root $(GACROOT)

uninstall-local:
	-$(GACUTIL) /u monodoc /package monodoc /gacdir $(GACDIR) /root $(GACROOT)

Test/man-provider-test.exe: Test/man-provider-test.cs monodoc.dll
	-mkdir -p `dirname $@`
	$(CSC) -debug -out:$@ $< -r:monodoc.dll

check-man-provider: Test/man-provider-test.exe
	MONO_PATH=. mono $< $(srcdir)/Test/man-test1.1 | diff - $(srcdir)/Test/man-test1.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test2.1 | diff - $(srcdir)/Test/man-test2.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test3.1 | diff - $(srcdir)/Test/man-test3.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test4.1 | diff - $(srcdir)/Test/man-test4.html

check-man-provider-update: Test/man-provider-test.exe
	MONO_PATH=. mono $< $(srcdir)/Test/man-test1.1 > $(srcdir)/Test/man-test1.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test2.1 > $(srcdir)/Test/man-test2.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test3.1 > $(srcdir)/Test/man-test3.html
	MONO_PATH=. mono $< $(srcdir)/Test/man-test4.1 > $(srcdir)/Test/man-test4.html

my_abs_top_srcdir=$(shell cd $(abs_top_srcdir) && pwd)

check-validate-update: validate.exe
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		$(srcdir)/Test/validate.check.monodocer
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected.importslashdoc | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		$(srcdir)/Test/validate.check.monodocer.importslashdoc
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected.since | \
		sed 's#file://$(my_abs_top_srcdir)/##g' > \
		$(srcdir)/Test/validate.check.monodocer.since

check-validate: validate.exe
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff - $(srcdir)/Test/validate.check.monodocer
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected.importslashdoc | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff --brief - $(srcdir)/Test/validate.check.monodocer.importslashdoc
	mono validate.exe ecma $(srcdir)/../tools/DocTest/en.expected.since | \
		sed 's#file://$(my_abs_top_srcdir)/##g' | \
		diff --brief - $(srcdir)/Test/validate.check.monodocer.since

check: check-man-provider check-validate

