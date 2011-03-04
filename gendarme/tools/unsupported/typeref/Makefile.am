EXTRA_DIST = $(typeref_sources) ChangeLog typeref.csproj

typeref_sources =  \
	../../../AssemblyStaticInfo.cs	\
	../../../AssemblyInfo.cs	\
	typeref.cs

typeref.exe:  $(typeref_sources)
	$(MCS) -debug $^ -out:$@ -r:$(CECIL_ASM)

clean:
	rm -f typeref.exe*

all: typeref.exe

