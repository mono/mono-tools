base = ../../gui-compare

SOURCES =  \
	$(base)/AssemblyResolver.cs	\
	$(base)/CompareContext.cs	\
	$(base)/Comparison.cs		\
	$(base)/CecilMetadata.cs	\
	$(base)/MasterMetadata.cs	\
	$(base)/Metadata.cs		\
	$(base)/Masterinfo.cs

all: webcompare-db.exe

webcompare-db.exe: webcompare-db.cs Mono.Cecil.dll Mono.Api.CompareFull.dll
	gmcs -debug -r:System.Configuration.dll -r:System.Core.dll -r:MySql.Data.dll -r:System.Data.dll -r:Mono.Api.CompareFull.dll -r:Mono.Cecil.dll webcompare-db.cs

Mono.Api.CompareFull.dll: $(SOURCES) Mono.Cecil.dll Makefile
	gmcs -nowarn:169,414 -debug -target:library -r:Mono.Cecil.dll -out:Mono.Api.CompareFull.dll $(SOURCES)

Mono.Cecil.dll:
	cp `pkg-config --variable Libraries cecil` .

clean:
	rm -f webcompare-db.exe webcompare-db.exe.mdb Mono.Cecil.dll* Mono.Api.CompareFull.dll*

