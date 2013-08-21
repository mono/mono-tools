Mono Tools:
----------

Mono Tools is a collection of development and testing programs and 
utilities for use with Mono.


Building:
--------

Building from a source tarball should be as simple as:

    ./configure [typical configure flags, such as --prefix=PREFIX]
    make
    make install

Building from git is almost identical, except that you need to
execute autogen.sh to create the configure script (which will be
automatically executed):

    git clone https://github.com/mono/mono-tools.git
    ./autogen.sh [typical configure flags, such as --prefix=PREFIX]
    make
    make install

Building on OSX with homebrew:
------------------------------

Have the following packages installed:
autoconf pkg-config	readline automake gettext glib intltool libtool

Run autogen like this:
PKG_CONFIG_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig/ ./autogen.sh 

