sources=driver.cs CompareWindow.cs CompareContext.cs Masterinfo.cs Comparison.cs Metadata.cs CecilMetadata.cs MasterMetadata.cs

guicompare.exe: $(sources)
	gmcs -debug -pkg:gtk-sharp-2.0 -pkg:mono-cecil -out:guicompare.exe $(sources)