base = ../gui-compare

SOURCES =  \
	$(base)/AssemblyResolver.cs	\
	$(base)/CompareContext.cs	\
	$(base)/Comparison.cs		\
	$(base)/CecilMetadata.cs	\
	$(base)/MasterMetadata.cs	\
	$(base)/Metadata.cs		\
	$(base)/Masterinfo.cs

all: bin/Mono.Api.Compare.dll bin/Mono.Cecil.dll

bin/Mono.Api.Compare.dll: $(SOURCES) Makefile  bin/Mono.Cecil.dll
	-mkdir bin
	gmcs -debug -target:library -out:bin/Mono.Api.Compare.dll $(SOURCES) -r:bin/Mono.Cecil.dll

bin/Mono.Cecil.dll:
	-mkdir bin/
	cp `pkg-config --variable Libraries cecil` bin/

get: masterinfos-3.5.tar.gz
	tar xzvf masterinfos-3.5.tar.gz 

masterinfos-3.5.tar.gz:
	wget http://mono.ximian.com/masterinfos/2.4/masterinfos-3.5.tar.gz


run: all
	xsp2

push: all
	rsync --rsh=ssh -pr bin Global.asax *.gif *.png Web.config index.aspx status.aspx mono-web@www.go-mono.com:go-mono/status/
