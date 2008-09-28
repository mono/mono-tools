gsharpdir = $(prefix)/lib/gsharp

bin_scripts = gsharp

gsharp_DATA = gsharp.exe

packages = -pkg:gtk-sharp-2.0 -pkg:glib-sharp-2.0
assemblies = -r:Mono.CSharp -r:Mono.Management

gsharp_sources = 			\
	$(srcdir)/driver.cs		\
	$(srcdir)/MainWindow.cs 

EXTRA_DIST = $(gsharp_sources)

gsharp.exe: $(gsharp_sources) 
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:gsharp.exe $(gsharp_sources) $(packages) $(assemblies)

