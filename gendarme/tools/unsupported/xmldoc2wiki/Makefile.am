EXTRA_DIST = $(xmldoc2wiki_sources) ChangeLog xmldoc2wiki.csproj

xmldoc2wiki_sources =  \
	../../../AssemblyStaticInfo.cs	\
	../../../AssemblyInfo.cs	\
	xmldoc2wiki.cs

xmldoc2wiki.exe: $(xmldoc2wiki_sources)
	$(MCS) -debug $^ -out:$@ -r:System.Xml.Linq.dll

clean:
	rm -f xmldoc2wiki.exe*

all: xmldoc2wiki.exe

