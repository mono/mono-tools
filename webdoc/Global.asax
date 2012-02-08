<%@ Application ClassName="Mono.Website.Global" %>
<%@ Import Namespace="Monodoc" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Assembly name="monodoc" %>

<script runat="server" language="c#" >

public static RootTree help_tree;
[ThreadStatic]
static SearchableIndex search_index;
public static string ua = null;
// These are dictionary of path for external couple (html, css, js) files that should get included
static Dictionary<ExternalResourceType, string> externalHeader = null;
static Dictionary<ExternalResourceType, string> externalFooter = null;

void Application_Start ()
{
	HelpSource.use_css = true;
	HelpSource.FullHtml = false;
	HelpSource.UseWebdocCache = true;
	var rootDir = WebConfigurationManager.AppSettings["MonodocRootDir"];
	if (!string.IsNullOrEmpty (rootDir))
		help_tree = RootTree.LoadTree (rootDir);
	else
		help_tree = RootTree.LoadTree ();
	ua = WebConfigurationManager.AppSettings["GoogleAnalytics"];
	externalHeader = ParseExternalDefinition (WebConfigurationManager.AppSettings["ExternalHeader"]);
	externalFooter = ParseExternalDefinition (WebConfigurationManager.AppSettings["ExternalFooter"]);
	SettingsHandler.Settings.EnableEditing = false;
}

public static readonly string kipunji_root_url = "http://docs.go-mono.com/";
private static readonly string prefixes = "TNCFEMP";

public static bool should_redirect_to_kipunji (string link)
{
	return prefixes.IndexOf (link [0]) > -1;
}

public static void redirect_to_kipunji (HttpContext context, string link)
{
	StringBuilder res = new StringBuilder ();
        res.Append (kipunji_root_url);

	if (link.StartsWith ("T:")) {

	      int end = link.Length;
	      string post = String.Empty;
	      if (link.Length > 3 && link [link.Length - 2] == '/') {
                  end = link.Length - 2;
	          switch (link [link.Length - 1]) {
	      	     case '*':
		     	  post = "/Members";
			  break;
		     case 'M':
		           post = "/Members#methods";
			   break;
		     case 'P':
		     	   post = "/Members#properties";
			   break;
		     case 'C':
		     	   post = "/Members#ctors";
			   break;
	             case 'F':
		     	   post = "/Members#fields";
			   break;
		     case 'E':
		     	   post = "/Members#events";
			   break;
	          }
              }

	      res.Append (link.Substring (2, end - 2));
	      res.Append (post);
	          
	} else if (link.StartsWith ("C:")) {
	      // HACK: Instead of linking to the proper ctor just send them to all the ctors
	      res.AppendFormat ("{0}/Members#ctors", link.Substring (2, link.Length - 2));
	} else if (link.StartsWith ("N:") || link.StartsWith ("M:") || link.StartsWith ("P:") || link.StartsWith ("C:") || link.StartsWith ("E:"))
	      res.Append (link.Substring (2, link.Length - 2));

	context.Response.RedirectPermanent (res.ToString ());
}

public static string CreateTreeBootFragment ()
{
	var fragment = new System.Text.StringBuilder ();

	for (int i = 0; i < help_tree.Nodes.Count; i++){
		Node n = (Node)help_tree.Nodes [i];

		string url = n.PublicUrl;

		if (n.Caption == "Base Class Library" || n.Caption == "Mono Libraries")
			url = kipunji_root_url + (n.Caption == "Base Class Library" ? "?display_all=true" : String.Empty);

		fragment.Append ("tree.CreateItem (root, '" + n.Caption + "', '" + url + "', ");
	
		if (n.Nodes.Count != 0)
			fragment.Append ("'" + i + "'");
		else
			fragment.Append ("null");
	
		if (i == help_tree.Nodes.Count-1)
			fragment.Append (", true");
		else
			fragment.Append (", false");

		fragment.Append (@");
			");
	}

	return fragment.ToString ();
}

public static SearchableIndex GetSearchIndex ()
{
	if (search_index != null)
		return search_index;
	return (search_index = help_tree.GetSearchIndex ());
}

public enum ExternalResourceType {
	Unknown,
	Html,
	Css,
	Javascript
}

public static string IncludeExternalHeader (ExternalResourceType type)
{
	return IncludeExternalFile (type, externalHeader);
}

public static string IncludeExternalFooter (ExternalResourceType type)
{
	return IncludeExternalFile (type, externalFooter);
}

static string IncludeExternalFile (ExternalResourceType type, Dictionary<ExternalResourceType, string> paths)
{
	string path;
	if (paths == null || !paths.TryGetValue (type, out path) || !File.Exists (path))
		return string.Empty;
	if (type == ExternalResourceType.Javascript) {
		return string.Format ("{1}script type='text/javascript' src='{0}'{2}{1}/script{2}", path, '<', '>');
	} else if (type == ExternalResourceType.Css) {
		return string.Format ("{1}link type='text/css' rel='stylesheet' href='{0}' /{2}", path, '<', '>');
	} else {
		return File.ReadAllText (path);
	}
}

static Dictionary<ExternalResourceType, string> ParseExternalDefinition (string definitionPath)
{
	if (string.IsNullOrEmpty (definitionPath) || !File.Exists (definitionPath))
		return null;
	// A definition file is a simple file with a line for each resource type in a key value fashion
	var lines = File.ReadAllLines (definitionPath);
	var result = lines.Where (l => !string.IsNullOrEmpty (l) && l[0] != '#') // Take non-empty, non-comment lines
		.Select (l => l.Split ('='))
		.Where (a => a != null && a.Length == 2)
		.Select (a => { ExternalResourceType t; return Tuple.Create (Enum.TryParse (a[0].Trim (), true, out t) ? t : ExternalResourceType.Unknown, a[1].Trim ()); })
		.Where (t => t.Item1 != ExternalResourceType.Unknown)
		.ToDictionary (t => t.Item1, t => t.Item2);

	return result;
}

</script>
