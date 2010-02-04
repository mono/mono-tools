glib-gettextize --force --copy ||
  { echo "**Error**: glib-gettextize failed."; exit 1; }

aclocal $ACLOCAL_FLAGS
automake -a
autoconf
./configure --enable-maintainer-mode $*
