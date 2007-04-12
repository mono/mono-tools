
CNM_FILES = \
	create-native-map.cs \
	MapAttribute.cs 

TM_FILES = \
	MapAttribute.cs \
	TestMap.cs 

cnm_build_sources = $(addprefix $(srcdir)/, $(CNM_FILES))
tm_build_sources  = $(addprefix $(srcdir)/, $(TM_FILES))

EXE = ../lib/create-native-map/create-native-map.exe

REFS = -r:Mono.Posix.dll

all: $(EXE)

$(EXE) : $(cnm_build_sources)
	$(GMCS) $(MCS_FLAGS) -d:TRACE -debug+ -out:$@ -target:exe $(REFS) $(cnm_build_sources)

assemblydir = $(prefix)/lib/create-native-map
assembly_DATA = $(EXE) MapAttribute.cs

check: $(PROGRAM) TestMap.dll

TestMap.dll: $(tm_build_sources) $(PROGRAM)
	$(MCS) -debug+ -out:$@ -t:library $(tm_build_sources)

test_output = test.c test.h test.cs test.xml

$(test_output): TestMap.dll $(EXE)
	../bin/create-native-map                                      \
		--autoconf-member autoconf_me                         \
		--autoconf-header='<unistd.h>'                        \
		--public-header='<stdlib.h>'                          \
		--public-macro=DefineMe=42                            \
		--impl-header='<stdio.h>'                             \
		--impl-header='"test.h"'                              \
		--impl-macro='FOO="foo"'                              \
		--exclude-native-symbol=exclude_native_symbol         \
		--library=NativeLib                                   \
		--rename-member=st_atime=st_atime_                    \
		--rename-namespace=MakeMap.ToBeRenamed=MakeMap_Rename \
		$< test

check: $(test_output)
	for f in test.c test.h test.cs test.xml ; do \
		diff -rup $(srcdir)/$$f.ref $$f ; \
	done

check-update:
	for f in test.c test.h test.cs test.xml ; do \
		mv $$f $$f.ref ; \
	done

libtest.so: test.c
	gcc -shared -o $@ $^ -I.. `pkg-config --libs --cflags glib-2.0`

test-overflow: test-overflow.c test-overflow.h test.c
	gcc -o $@ $< `pkg-config --libs --cflags glib-2.0`

CLEANFILES = $(EXE) $(EXE).mdb $(test_output) TestMap.dll TestMap.dll.mdb libtest.so

EXTRA_DIST = $(CNM_FILES) TestMap.cs test.c.ref test.cs.ref test.h.ref test.xml.ref

