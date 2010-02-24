theme = hicolor
themedir = $(datadir)/icons/$(theme)
theme_icons = \
	apps,GNOME/monodoc-16.png \
	apps,GNOME/monodoc-22.png \
	apps,GNOME/monodoc-24.png \
	apps,GNOME/monodoc-32.png \
	apps,GNOME/monodoc-48.png \
	apps,GNOME/monodoc-256.png

install_icon_exec = $(srcdir)/icon-theme-installer \
	-t "$(theme)" \
	-s "$(srcdir)" \
	-d "x$(DESTDIR)" \
	-b "$(themedir)" \
	-m "$(mkinstalldirs)" \
	-x "$(INSTALL_DATA)"

install-data-local:
	@-$(install_icon_exec) -i $(theme_icons)

uninstall-hook:
	@-$(install_icon_exec) -u $(theme_icons)

EXTRA_DIST = \
	$(wildcard GNOME/*.png) \
	$(wildcard Mac/*.icns) \
	icon-theme-installer

