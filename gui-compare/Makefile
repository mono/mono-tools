sources=driver.cs CompareWindow.cs CompareContext.cs Masterinfo.cs Comparison.cs Metadata.cs CecilMetadata.cs MasterMetadata.cs
resources= \
	-resource:cm/c.gif,c.gif	\
	-resource:cm/d.gif,d.gif	\
	-resource:cm/e.gif,e.gif	\
	-resource:cm/en.gif,en.gif	\
	-resource:cm/f.gif,f.gif	\
	-resource:cm/i.gif,i.gif	\
	-resource:cm/m.gif,m.gif	\
	-resource:cm/n.gif,n.gif	\
	-resource:cm/p.gif,p.gif	\
	-resource:cm/r.gif,r.gif	\
	-resource:cm/sc.gif,sc.gif	\
	-resource:cm/se.gif,se.gif	\
	-resource:cm/s.gif,s.gif	\
	-resource:cm/sm.gif,sm.gif	\
	-resource:cm/st.gif,st.gif	\
	-resource:cm/sx.gif,sx.gif	\
	-resource:cm/tb.gif,tb.gif	\
	-resource:cm/tm.gif,tm.gif	\
	-resource:cm/tp.gif,tp.gif	\
	-resource:cm/w.gif,w.gif	\
	-resource:cm/y.gif,y.gif

guicompare.exe: $(sources)
	gmcs -debug -pkg:gtk-sharp-2.0 -pkg:cecil -out:guicompare.exe $(sources) $(resources)
