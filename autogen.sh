glib-gettextize --force --copy ||
  { echo "**Error**: glib-gettextize failed."; exit 1; }

aclocal
automake -a
autoconf
./configure --enable-maintainer-mode $*
