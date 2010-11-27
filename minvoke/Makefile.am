minvokedir=$(prefix)/lib/minvoke
bin_SCRIPTS = minvoke
minvoke_DATA = minvoke.exe

CLEANFILES = minvoke.exe minvoke.exe.gdb minvoke MapAssembly.dll RetargetAssembly.exe


minvoke_sources = \
	$(srcdir)/minvoke.cs

mapassembly_dll_sources = \
	$(srcdir)/MapAssembly.cs \
	$(srcdir)/MapDllImportAttribute.cs

retargetassembly_exe_sources = \
	$(srcdir)/RetargetAssembly.cs

EXTRA_DIST = $(minvoke_sources) $(mapassembly_dll_souorces) $(retargetassembly_exe_sources) minvoke.in

MapAssembly.dll: $(mapassembly_dll_sources) 
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -target:library -out:MapAssembly.dll $(mapassembly_dll_sources)

RetargetAssembly.exe: $(retargetassembly_exe_sources)
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -target:exe -out:RetargetAssembly.exe $(retargetassembly_exe_sources)

minvoke.exe: $(minvoke_sources);
	$(GMCS) -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG -target:exe -out:minvoke.exe $(minvoke_sources) -r:System.Core -r:$(CECIL_ASM)


test: minvoke.exe MapAssembly.dll RetargetAssembly.exe
	mono minvoke.exe MapAssembly.dll RetargetAssembly.exe RetargetedAssembly.exe
