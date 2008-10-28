DISTCLEANFILES = 

.PHONY :  \
	cleandb \
	db \
	dist-hook \
	web \
	up

EXTRA_DIST =

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


Monodoc.Contributions.dll: server.cs 
	mcs -g server.cs -target:library -out:$@ -r:System.Web -r:System.Web.Services -pkg:monodoc -r:System.Data -r:ByteFX.Data

web: Monodoc.Contributions.dll
	xsp

db:
	mysql -u admin -p 

cleandb:
	mysql -u admin -p < tables.sql

up:
	scp tables.sql server.cs server.asmx monodoc.dll root@www.go-mono.com:

