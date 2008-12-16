<%@ Import Namespace="GuiCompare" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Assembly name="Mono.API.Compare" %>

<script runat="server" language="c#" >

public class CompareParameters {
        public CompareParameters (NameValueCollection nvc)
	{
		Assembly = nvc ["assembly"] ?? "mscorlib";
		InfoDir  = nvc ["reference"] ?? "";
		string bdir = nvc ["profile"] ?? "2.0";
		BinDir = "/mono/lib/mono/" + bdir;
	}

	public CompareParameters (string assembly, string infodir, string bindir)
	{
		Assembly = assembly;
		InfoDir = infodir;
		BinDir = bindir;
	}

	public string Assembly { get; private set; }
	public string InfoDir  { get; private set; }
	public string BinDir   { get; private set; }

	public override int GetHashCode ()
	{
		return Assembly.GetHashCode ();
	}

	public override bool Equals (object obj)
	{
		if (obj == null)
			return false;
		CompareParameters other = obj as CompareParameters;
		if (other == null)
			return false;

		return other.Assembly == Assembly && other.InfoDir == InfoDir && other.BinDir == BinDir;
		
	}

	public CompareContext MakeCompareContext ()
	{
		Console.WriteLine ("Comparing {0} on {1} with {2}", Assembly, InfoDir, BinDir);
		CompareContext cc = new CompareContext (
			() => new MasterAssembly (Path.Combine (Path.Combine (InfoDir, "masterinfos"), Assembly) + ".xml"),
		     	() => new CecilAssembly (Path.Combine (BinDir, Assembly) + ".dll"));
		ManualResetEvent r = new ManualResetEvent (false);
		cc.Finished += delegate { r.Set (); };
		Console.WriteLine ("Starting Compare");
		cc.Compare ();
		r.WaitOne ();
		cc.Comparison.PropagateCounts ();

		return cc;
	}
}

public static Dictionary<CompareParameters,CompareContext> CompareCache = new Dictionary<CompareParameters,CompareContext> ();

</script>