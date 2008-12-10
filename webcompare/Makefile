base = ../gui-compare

SOURCES =  \
	$(base)/AssemblyResolver.cs	\
	$(base)/CompareContext.cs	\
	$(base)/Comparison.cs		\
	$(base)/CecilMetadata.cs	\
	$(base)/MasterMetadata.cs	\
	$(base)/Metadata.cs		\
	$(base)/Masterinfo.cs

all: bin/Mono.API.Compare.dll 

bin/Mono.API.Compare.dll: $(SOURCES) Makefile
	-mkdir bin
	gmcs -debug -target:library -out:bin/Mono.API.Compare.dll $(SOURCES) -r:`pkg-config --variable Libraries cecil`

get: masterinfos-3.5.tar.gz
	tar xzvf masterinfos-3.5.tar.gz 

masterinfos-3.5.tar.gz:
	wget http://mono.ximian.com/masterinfos/2.4/masterinfos-3.5.tar.gz
	

run: all
	xsp2
