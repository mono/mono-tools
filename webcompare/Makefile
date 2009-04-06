SUBDIRS=db
base = ../gui-compare
SOURCES =  \
	$(base)/Comparison.cs		\
	$(base)/Metadata.cs

all: bin/Mono.Api.CompareMinimal.dll
	make -C db

bin/Mono.Api.CompareMinimal.dll: $(SOURCES) Makefile
	mkdir -p bin
	gmcs -debug -target:library -out:bin/Mono.Api.CompareMinimal.dll $(SOURCES)

get: masterinfos-3.5.tar.gz
	tar xzvf masterinfos-3.5.tar.gz 

masterinfos-3.5.tar.gz:
	wget http://mono.ximian.com/masterinfos/2.4/masterinfos-3.5.tar.gz

run: all
	xsp2

push: all
	rsync --rsh=ssh -pr db bin images App_Code Web.config index.aspx status.aspx mono-web@www.go-mono.com:go-mono/status/

clean:
	rm -f rm bin/Mono.Api.CompareMinimal.dll*
	make -C db clean
