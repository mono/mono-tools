webdir=$(prefix)/lib/monodoc/web

web_DATA = \
	Global.asax \
	robots.txt \
	index.aspx \
	monodoc.ashx \
	monodoc.css \
	web.config \
	api.master \
	plugins.def \
	README.md 

DISTCLEANFILES = 

.PHONY :  \
	dist-hook \
	web \
	up
 
EXTRA_DIST = \
	$(wildcard App_Code/Plugins/*.cs) \
	$(wildcard views/*.css) \
	$(wildcard views/*.js) \
	$(wildcard views/*.html) \
	$(wildcard views/images/*.png) \
	$(wildcard views/images/*.gif) \
	$(wildcard skins/mono-external/header*) \
	$(wildcard skins/mono-external/footer*) \
	$(wildcard skins/mono-external/*.css) \
	$(wildcard skins/mono-external/*.js) \
	$(wildcard skins/mono-external/images/*.png) \
	$(wildcard skins/mono-external/images/*.gif) \
	$(wildcard plugins/iframe-plugin/*.js) \
        $(wildcard plugins/iframe-plugin/*.css) \
        $(wildcard plugins/iframe-plugin/*.html) \
	$(wildcard plugins/tooltip-plugin/*.js) \
	$(wildcard plugins/sidebar-plugin/*.js) \
        $(wildcard plugins/sidebar-plugin/*.css) \
	$(wildcard plugins/sidebar-plugin/dependencies/ptree/*.css) \
        $(wildcard plugins/sidebar-plugin/dependencies/ptree/*.js) \
        $(wildcard plugins/sidebar-plugin/dependencies/ptree/*.gif) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/*.css) \
        $(wildcard plugins/sidebar-plugin/dependencies/xtree/*.js) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/images/*.png) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/images/clean/*.gif) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/images/msdn/*.gif) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/images/msdn2/*.gif) \
	$(wildcard plugins/sidebar-plugin/dependencies/xtree/images/xp/*.png) \
        $(wildcard plugins/search-plugin/images/*.gif) \
        $(wildcard plugins/search-plugin/fullsearch/*.css) \
        $(wildcard plugins/search-plugin/fullsearch/*.js) \
        $(wildcard plugins/search-plugin/fullsearch/*.html) \
	$(wildcard plugins/search-plugin/fastsearch/*.css) \
        $(wildcard plugins/search-plugin/fastsearch/*.js)	

dist-hook:
	mkdir -p $(distdir)/App_Code/Plugins
	mkdir -p $(distdir)/views/images
	mkdir -p $(distdir)/skins/mono-external/images
	mkdir -p $(distdir)/plugins/iframe-plugin
	mkdir -p $(distdir)/plugins/tooltip-plugin
	mkdir -p $(distdir)/plugins/sidebar-plugin/dependencies/ptree
	mkdir -p $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn
	mkdir -p $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2
	mkdir -p $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp
	mkdir -p $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean
	mkdir -p $(distdir)/plugins/search-plugin/images
	mkdir -p $(distdir)/plugins/search-plugin/fastsearch
	mkdir -p $(distdir)/plugins/search-plugin/fullsearch
	for f in $(web_DATA) ; do cp $(srcdir)/$$f $(distdir) ; done
	cp $(srcdir)/App_Code/Plugins/*cs          				  $(distdir)/App_Code/Plugins
	cp $(srcdir)/views/*css                                                   $(distdir)/views
	cp $(srcdir)/views/*js                                                    $(distdir)/views
	cp $(srcdir)/views/images/*png                                            $(distdir)/views/images	
	cp $(srcdir)/views/images/*ico                                            $(distdir)/views/images
	cp $(srcdir)/skins/mono-external/header*                                  $(distdir)/skins/mono-external
	cp $(srcdir)/skins/mono-external/footer*                                  $(distdir)/skins/mono-external
	cp $(srcdir)/skins/mono-external/*css                                     $(distdir)/skins/mono-external
	cp $(srcdir)/skins/mono-external/images/*png                              $(distdir)/skins/mono-external/images
	cp $(srcdir)/skins/mono-external/images/*gif                              $(distdir)/skins/mono-external/images	
	cp $(srcdir)/plugins/iframe-plugin/*js                                    $(distdir)/plugins/iframe-plugin
	cp $(srcdir)/plugins/tooltip-plugin/*js                                    $(distdir)/plugins/tooltip-plugin
	cp $(srcdir)/plugins/sidebar-plugin/*css                                  $(distdir)/plugins/sidebar-plugin
	cp $(srcdir)/plugins/sidebar-plugin/*js                                   $(distdir)/plugins/sidebar-plugin	
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*css               $(distdir)/plugins/sidebar-plugin/dependencies/ptree	
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*js                $(distdir)/plugins/sidebar-plugin/dependencies/ptree
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*gif               $(distdir)/plugins/sidebar-plugin/dependencies/ptree	
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/*css               $(distdir)/plugins/sidebar-plugin/dependencies/xtree	
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/*js   	          $(distdir)/plugins/sidebar-plugin/dependencies/xtree 
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/*png        $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean/*gif  $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn/*gif   $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2/*gif  $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2
	cp $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp/*png     $(distdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp
	cp $(srcdir)/plugins/search-plugin/images/*gif                            $(distdir)/plugins/search-plugin/images
	cp $(srcdir)/plugins/search-plugin/fullsearch/*css                        $(distdir)/plugins/search-plugin/fullsearch
	cp $(srcdir)/plugins/search-plugin/fullsearch/*js                         $(distdir)/plugins/search-plugin/fullsearch
	cp $(srcdir)/plugins/search-plugin/fullsearch/*html                       $(distdir)/plugins/search-plugin/fullsearch
	cp $(srcdir)/plugins/search-plugin/fastsearch/*css                        $(distdir)/plugins/search-plugin/fastsearch
	cp $(srcdir)/plugins/search-plugin/fastsearch/*js                         $(distdir)/plugins/search-plugin/fastsearch

install-data-hook:
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/App_Code/Plugins
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/views/images
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/skins/mono-external/images
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/iframe-plugin
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/tooltip-plugin
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/ptree
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/search-plugin/images
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/search-plugin/fastsearch
	-$(mkinstalldirs) $(DESTDIR)$(webdir)/plugins/search-plugin/fullsearch
	 $(INSTALL_DATA) $(srcdir)/App_Code/Plugins/*cs                                         $(DESTDIR)$(webdir)/App_Code/Plugins
	 $(INSTALL_DATA) $(srcdir)/views/*css                                                   $(DESTDIR)$(webdir)/views
	 $(INSTALL_DATA) $(srcdir)/views/*js                                                    $(DESTDIR)$(webdir)/views
	 $(INSTALL_DATA) $(srcdir)/views/*html                                                  $(DESTDIR)$(webdir)/views
	 $(INSTALL_DATA) $(srcdir)/views/images/*png                                            $(DESTDIR)$(webdir)/views/images
	 $(INSTALL_DATA) $(srcdir)/views/images/*gif                                            $(DESTDIR)$(webdir)/views/images
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/header*                                  $(DESTDIR)$(webdir)/skins/mono-external
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/footer*                                  $(DESTDIR)$(webdir)/skins/mono-external
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/*css                                     $(DESTDIR)$(webdir)/skins/mono-external
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/*js                                      $(DESTDIR)$(webdir)/skins/mono-external
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/images/*png                              $(DESTDIR)$(webdir)/skins/mono-external/images
	 $(INSTALL_DATA) $(srcdir)/skins/mono-external/images/*gif                              $(DESTDIR)$(webdir)/skins/mono-external/images
	 $(INSTALL_DATA) $(srcdir)/plugins/iframe-plugins/*css                                  $(DESTDIR)$(webdir)/plugins/iframe-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/iframe-plugin/*js                                    $(DESTDIR)$(webdir)/plugins/iframe-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/iframe-plugin/*html                                  $(DESTDIR)$(webdir)/plugins/iframe-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/tooltip-plugin/*js                                    $(DESTDIR)$(webdir)/plugins/tooltip-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/*css                                  $(DESTDIR)$(webdir)/plugins/sidebar-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/*js                                   $(DESTDIR)$(webdir)/plugins/sidebar-plugin
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*css               $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/ptree
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*js                $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/ptree
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/ptree/*gif               $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/ptree
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/*css               $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/*js                $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/*png        $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean/*gif  $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/clean
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn/*gif   $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2/*gif  $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/msdn2
	 $(INSTALL_DATA) $(srcdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp/*png     $(DESTDIR)$(webdir)/plugins/sidebar-plugin/dependencies/xtree/images/xp
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/images/*gif                            $(DESTDIR)$(webdir)/plugins/search-plugin/images
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/fullsearch/*css                        $(DESTDIR)$(webdir)/plugins/search-plugin/fullsearch
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/fullsearch/*js                         $(DESTDIR)$(webdir)/plugins/search-plugin/fullsearch
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/fullsearch/*html                       $(DESTDIR)$(webdir)/plugins/search-plugin/fullsearch
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/fastsearch/*css                        $(DESTDIR)$(webdir)/plugins/search-plugin/fastsearch
	 $(INSTALL_DATA) $(srcdir)/plugins/search-plugin/fastsearch/*js                         $(DESTDIR)$(webdir)/plugins/search-plugin/fastsearch
	
Monodoc.Contributions.dll: server.cs Makefile.am
	mcs -debug+ $< -target:library -out:$@ -r:System.Web -r:System.Web.Services -pkg:monodoc -r:System.Data -r:ByteFX.Data

web: Monodoc.Contributions.dll
	xsp

up:
	scp monodoc.dll root@www.go-mono.com:

