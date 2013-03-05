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
	
	//Google analytics if we want em
	ua = WebConfigurationManager.AppSettings["GoogleAnalytics"];

	SettingsHandler.Settings.EnableEditing = false;
}

/*----------------TREE BUILDING----------------*/
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

/*------------SEARCH------------*/
public static SearchableIndex GetSearchIndex ()
{
	if (search_index != null)
		return search_index;
	return (search_index = help_tree.GetSearchIndex ());
}

</script>
