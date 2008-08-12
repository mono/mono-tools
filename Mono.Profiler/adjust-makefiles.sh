#!/bin/bash
for MF in ./*/Makefile.am; do
sed -i -e 's/include $(top_srcdir)\/Makefile.include/include $(top_srcdir)\/Mono.Profiler\/Makefile.include/' $MF;
sed -i -e 's/GTK_SHARP_20_LIBS/GTK_SHARP_LIBS/' $MF;
done
rm autogen.sh configure.ac;
