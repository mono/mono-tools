<%@ Application ClassName="Mono.Website.Global" %>
<%@ Import Namespace="Monodoc" %>
<%@ Assembly name="monodoc" %>

<script runat="server" language="c#" >
public static RootTree help_tree;

void Application_Start ()
{
	HelpSource.use_css = true;
	HelpSource.FullHtml = false;
	HelpSource.UseWebdocCache = true;
	help_tree = RootTree.LoadTree ();
	SettingsHandler.Settings.EnableEditing = false;
}

private static readonly	string root = "http://docs.go-mono.com/";
private static readonly string prefixes = "TNCFEMP";

public static bool should_redirect_to_kipunji (string link)
{
	return prefixes.IndexOf (link [0]) > -1;
}

public static void redirect_to_kipunji (HttpContext context, string link)
{
	StringBuilder res = new StringBuilder ();
        res.Append (root);

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

</script>
