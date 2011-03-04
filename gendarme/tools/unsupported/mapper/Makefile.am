EXTRA_DIST = $(mapper_sources) fxcop.missing

mapper_sources =  \
	../../../AssemblyStaticInfo.cs	\
	../../../AssemblyInfo.cs	\
	FxCopMapBuilder.cs		\
	FxCopRule.cs

mapper.exe: $(mapper_sources)
	$(MCS) -debug $^ -out:$@ -r:System.Xml.Linq.dll -r:$(CECIL_ASM)

clean:
	rm -f mapper.exe*

all: mapper.exe

