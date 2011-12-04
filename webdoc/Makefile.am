webdir=$(prefix)/lib/monodoc/web

web_DATA = \
	common.css \
	edit.aspx \
	Global.asax \
	header.html \
	index.aspx \
	index.html \
	login.aspx \
	logout.aspx \
	mono_logo_g.gif \
	monodoc.ashx \
	monodoc.asmx \
	oldposts.xml \
	server.asmx \
	server.cs \
	sidebar.css \
	sidebar.js \
	tables.sql \
	web.config \
	wiki2ecmahelper.xsl

DISTCLEANFILES = 

.PHONY :  \
	cleandb \
	db \
	dist-hook \
	web \
	up
 
EXTRA_DIST = \
	$(wildcard images/*.gif) \
	$(wildcard images/*.png) \
	$(wildcard ptree/tree*) \
	$(wildcard xtree/*.js) \
	$(wildcard xtree/*.css) \
	$(wildcard xtree/images/*.png) \
	$(wildcard xtree/images/*.png) \
	$(wildcard xtree/images/clean/*.gif) \
	$(wildcard xtree/images/msdn/*.gif) \
	$(wildcard xtree/images/msdn2/*.gif)
	$(wildcard xtree/images/xp/*.png)

dist-hook:
	mkdir -p $(distdir)/images
	mkdir -p $(distdir)/xtree/images/msdn
	mkdir -p $(distdir)/xtree/images/msdn2
	mkdir -p $(distdir)/xtree/images/xp
	mkdir -p $(distdir)/ptree
	for f in $(web_DATA) ; do cp $(srcdir)/$$f $(distdir) ; done
	cp $(srcdir)/images/*gif              $(distdir)/images
	cp $(srcdir)/images/*png              $(distdir)/images
	cp $(srcdir)/xtree/images/msdn/*gif   $(distdir)/xtree/images/msdn
	cp $(srcdir)/xtree/images/msdn2/*gif  $(distdir)/xtree/images/msdn2
	cp $(srcdir)/xtree/images/xp/*png     $(distdir)/xtree/images/xp
	cp $(srcdir)/xtree/images/*png        $(distdir)/xtree/images
	cp $(srcdir)/xtree/*js                $(distdir)/xtree
	cp $(srcdir)/xtree/*css               $(distdir)/xtree
	cp $(srcdir)/ptree/*css               $(distdir)/ptree
	cp $(srcdir)/ptree/*js                $(distdir)/ptree

install-data-hook:
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/images
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/ptree
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/xtree
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/xtree/images
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/xtree/images/msdn
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/xtree/images/msdn2
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/xtree/images/xp
	$(INSTALL_DATA) $(srcdir)/images/*gif             $(DESTDIR)$(webdir)/images
	$(INSTALL_DATA) $(srcdir)/images/*png             $(DESTDIR)$(webdir)/images
	$(INSTALL_DATA) $(srcdir)/xtree/images/msdn/*gif  $(DESTDIR)$(webdir)/xtree/images/msdn
	$(INSTALL_DATA) $(srcdir)/xtree/images/msdn2/*gif $(DESTDIR)$(webdir)/xtree/images/msdn2
	$(INSTALL_DATA) $(srcdir)/xtree/images/xp/*png    $(DESTDIR)$(webdir)/xtree/images/xp
	$(INSTALL_DATA) $(srcdir)/xtree/images/*png       $(DESTDIR)$(webdir)/xtree/images/xp
	$(INSTALL_DATA) $(srcdir)/xtree/*js               $(DESTDIR)$(webdir)/xtree
	$(INSTALL_DATA) $(srcdir)/xtree/*css              $(DESTDIR)$(webdir)/xtree
	$(INSTALL_DATA) $(srcdir)/ptree/*css              $(DESTDIR)$(webdir)/ptree
	$(INSTALL_DATA) $(srcdir)/ptree/*js               $(DESTDIR)$(webdir)/ptree

Monodoc.Contributions.dll: server.cs Makefile.am
	mcs -debug+ $< -target:library -out:$@ -r:System.Web -r:System.Web.Services -pkg:monodoc -r:System.Data -r:ByteFX.Data

web: Monodoc.Contributions.dll
	xsp

db:
	mysql -u admin -p 

cleandb:
	mysql -u admin -p < tables.sql

up:
	scp tables.sql server.cs server.asmx monodoc.dll root@www.go-mono.com:

