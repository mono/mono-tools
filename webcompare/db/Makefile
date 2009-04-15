base = ../../gui-compare

LIB_SOURCES =  \
	$(base)/AssemblyResolver.cs	\
	$(base)/CompareContext.cs	\
	$(base)/Comparison.cs		\
	$(base)/CecilMetadata.cs	\
	$(base)/MasterMetadata.cs	\
	$(base)/Metadata.cs		\
	$(base)/Masterinfo.cs

SOURCES = \
	webcompare-db.cs	\
	DataAccess.cs		\
	MySqlDataAccess.cs	
#	PostgresDataAccess.cs

REFS = \
	-r:System.Configuration.dll	\
	-r:System.Core.dll 		\
	-r:MySql.Data.dll 		\
	-r:System.Data.dll 		
#	-r:Npgsql

all: webcompare-db.exe

webcompare-db.exe: $(SOURCES) Mono.Cecil.dll Mono.Api.CompareFull.dll
	gmcs -debug $(REFS) -r:Mono.Api.CompareFull.dll -r:Mono.Cecil.dll $(SOURCES)

Mono.Api.CompareFull.dll: $(LIB_SOURCES) Mono.Cecil.dll Makefile
	gmcs -nowarn:169,414 -debug -target:library -r:Mono.Cecil.dll -out:Mono.Api.CompareFull.dll $(LIB_SOURCES)

Mono.Cecil.dll:
	cp `pkg-config --variable Libraries cecil` .

clean:
	rm -f webcompare-db.exe webcompare-db.exe.mdb Mono.Cecil.dll* Mono.Api.CompareFull.dll*

