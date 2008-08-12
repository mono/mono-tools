#!/bin/bash
sed -i -e 's/EXTRA_DIST =  expansions.m4//' Makefile.am
for F in ./*/mprof-*.in; do
sed -i -e 's/@expanded_libdir@/@prefix@\/lib/' $F
done
for F in ./*/Makefile.am; do
sed -i -e 's/include $(top_srcdir)\/Makefile.include/include $(top_srcdir)\/Mono.Profiler\/Makefile.include/' $F;
sed -i -e 's/GTK_SHARP_20_LIBS/GTK_SHARP_LIBS/' $F;
done
rm autogen.sh configure.ac expansions.m4;
