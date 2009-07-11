SUBDIRS=db
base = ../gui-compare

# Do NOT copy your local webcompare-db.exe.config to the server
DB_SYNCFILES= \
	db/Makefile \
	db/init-db.mysql \
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
	README \
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
	mkdir -p bin
	gmcs -debug -target:library -r:`pkg-config --variable=Libraries cecil` -out:bin/Mono.Api.CompareMinimal.dll $(SOURCES)

get: masterinfos-3.5.tar.gz
	tar xzvf masterinfos-3.5.tar.gz 

masterinfos-3.5.tar.gz:
	wget http://mono.ximian.com/masterinfos/2.4/masterinfos-3.5.tar.gz

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

