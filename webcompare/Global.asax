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
		InfoDir  = nvc ["reference"] ?? "3.5";
		string bdir = nvc ["profile"] ?? "2.0";
		Validate (bdir);

		BinDir = "binary/" + bdir;
	}

	public CompareParameters (string assembly, string infodir, string bindir)
	{
		Assembly = assembly;
		InfoDir = infodir;
		BinDir = bindir;
	}

	static void Validate (string s)
	{
		if (s.IndexOf ("..") != -1 || s.IndexOf ('/') != -1 || s.IndexOf ('%') != -1 || s.IndexOf (' ') != -1)
			throw new Exception (String.Format ("Invalid parameter: {0}", s));
	}

	string assembly;
	public string Assembly { 
		get { return assembly; }
		private set { 
			Validate (value);
			assembly = value;
		}
	}

	string infodir;
	public string InfoDir { 
		get { return infodir; }
		private set { 
			Validate (value);
			infodir = value;
		}
	}

	public string BinDir {  get; private set; } 

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
		string info_file = Path.Combine (HttpRuntime.AppDomainAppPath, Path.Combine (Path.Combine ("masterinfos", InfoDir), Assembly) + ".xml");
		string dll_file = Path.Combine (HttpRuntime.AppDomainAppPath, Path.Combine (BinDir, Assembly) + ".dll");

		using (var fs = File.OpenWrite ("/tmp/mylog")){
		      var sw = new StreamWriter (fs);
		      sw.WriteLine ("Comparing {0} and {1}", info_file, dll_file);
		      sw.Flush ();
		}
		Console.WriteLine ("Comparing {0} and {1}", info_file, dll_file);
		if (!File.Exists (info_file))
			throw new Exception (String.Format ("File {0} does not exist", info_file));
		if (!File.Exists (dll_file))
			throw new Exception (String.Format ("File {0} does not exist", dll_file));

		CompareContext cc = new CompareContext (
			() => new MasterAssembly (info_file),
		     	() => new CecilAssembly (dll_file));
		cc.ProgressChanged += delegate (object sender, CompareProgressChangedEventArgs a){
			Console.Error.WriteLine (a.Message);
		};
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

void Application_Start ()
{
}

</script>