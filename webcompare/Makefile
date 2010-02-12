SUBDIRS=db
base = ../gui-compare

# Do NOT copy your local webcompare-db.exe.config to the server
DB_SYNCFILES= \
	db/Makefile \
	db/createdb.sql \
	db/webcompare-db.cs \
	db/DataAccess.cs		\
	db/MySqlDataAccess.cs		\
	db/PostgresDataAccess.cs	\
	db/webcompare-db.exe.config-EDITME \
	db/web.config \
	db/*.dll \
	db/*.exe

BASE_FILES= \
	Web.config-EDITME \
	Makefile \
	ChangeLog \
	main.css \
	index.aspx \
	status.aspx

SOURCES =  \
	$(base)/Comparison.cs		\
	$(base)/Metadata.cs

all: bin/Mono.Api.CompareMinimal.dll
	make -C db

bin/Mono.Api.CompareMinimal.dll: $(SOURCES) Makefile
	@mkdir -p bin
	gmcs -debug -target:library -r:`pkg-config --variable=Libraries cecil` -out:bin/Mono.Api.CompareMinimal.dll $(SOURCES)

run: all
	xsp2

push: all
	rsync --rsh=ssh -pr ${BASE_FILES} bin images App_Code mono-web@www.go-mono.com:go-mono/status/
	rsync --rsh=ssh -pr ${DB_SYNCFILES} mono-web@www.go-mono.com:go-mono/status/db/

pushdb: all
	rsync --rsh=ssh -pr ${DB_SYNCFILES} mono-web@www.go-mono.com:go-mono/status/db/

clean:
	rm -f rm bin/Mono.Api.CompareMinimal.dll*
	make -C db clean

