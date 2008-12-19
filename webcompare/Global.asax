<%@ Import Namespace="GuiCompare" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Assembly name="Mono.Api.Compare" %>

<script runat="server" language="c#" >

public class CompareParameters {
	static Dictionary<CompareParameters,CompareContext> compare_cache = new Dictionary<CompareParameters,CompareContext> ();
	static Dictionary<CompareParameters,DateTime> timestamp = new Dictionary<CompareParameters,DateTime> ();

	static public bool InCache (CompareParameters cp)
	{
		lock (compare_cache){
			return compare_cache.ContainsKey (cp);
		}
	}

	static public DateTime GetAssemblyTime (CompareParameters cp)
	{
		return timestamp [cp];
	}

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

	public CompareContext GetCompareContext ()
	{
		CompareContext cc;

		lock (compare_cache){
			DateTime stamp = new FileInfo (DllFile).LastWriteTimeUtc;

			if (!compare_cache.TryGetValue (this, out cc) || timestamp [this] != stamp){
				cc = MakeCompareContext ();
				compare_cache [this] = cc;
				timestamp [this] = stamp;
			}
		}
		return cc;
	}

	string DllFile {
	       	get {
	       		return Path.Combine (HttpRuntime.AppDomainAppPath, Path.Combine (BinDir, Assembly) + ".dll");
		}
	}

	CompareContext MakeCompareContext ()
	{
		string info_file = Path.Combine (HttpRuntime.AppDomainAppPath, Path.Combine (Path.Combine ("masterinfos", InfoDir), Assembly) + ".xml");
		string dll_file = DllFile;

		using (var sw = File.AppendText ("/tmp/mylog")){
			sw.WriteLine ("{2} Comparing {0} and {1}", info_file, dll_file, DateTime.Now);
			sw.Flush ();

			if (!File.Exists (info_file)) 
				throw new Exception (String.Format ("File {0} does not exist", info_file));
			if (!File.Exists (dll_file))
				throw new Exception (String.Format ("File {0} does not exist", dll_file));
	
			CompareContext cc = new CompareContext (
				() => new MasterAssembly (info_file),
			     	() => new CecilAssembly (dll_file));

			cc.ProgressChanged += delegate (object sender, CompareProgressChangedEventArgs a){
				sw.WriteLine (a.Message);
				sw.Flush ();
			};
			ManualResetEvent r = new ManualResetEvent (false);
			cc.Finished += delegate { r.Set (); };
			cc.Compare ();
			r.WaitOne ();
			cc.Comparison.PropagateCounts ();
	
			sw.Flush ();

			return cc;
		}
	}
}

void Application_Start ()
{
}

</script>