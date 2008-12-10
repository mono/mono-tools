<%@ Import Namespace="GuiCompare" %>
<%@ Import Namespace="System.Threading" %>
<%@ Assembly name="Mono.API.Compare" %>

<script runat="server" language="c#" >
public static CompareContext CompareContext;

void Application_Start ()
{
	// Temporary, while we add a sync version of the API
	CompareContext = new CompareContext (() => new MasterAssembly ("masterinfos/mscorlib.xml"),
					     () => new CecilAssembly ("/mono/lib/mono/2.0/mscorlib.dll"));
	ManualResetEvent r = new ManualResetEvent (false);
	CompareContext.Finished += delegate { r.Set (); };
	CompareContext.Compare ();
	r.WaitOne ();
	CompareContext.Comparison.PropagateCounts ();

	Console.WriteLine ("Compare complete");
}

</script>