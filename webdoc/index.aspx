<%@ Page Language="C#" ClassName="Mono.Website.Index" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Assembly name="monodoc" %>

<html>
  <head>
    <title runat="server"><%= ConfigurationManager.AppSettings["Page Title"] %></title>
    <link href="favicon.ico" type="image/png" rel="icon">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" >
    <link type="text/css" rel="stylesheet" href="plugins/sidebar/ptree/tree.css"/>
    <link type="text/css" rel="stylesheet" href="plugins/sidebar/sidebar.css"/>
    <link type="text/css" rel="stylesheet" href="reset.css"/>
    <% = Global.IncludeExternalHeader (Global.ExternalResourceType.Css) %>
    <% = Global.IncludeExternalFooter (Global.ExternalResourceType.Css) %>
  </head>
  <body>

    <!--Do our c# scripts here to generate iframe/monodoc magic-->
    <script language="c#" runat="server">
	public string GetTitle ()
	{
		return Global.help_tree.GetTitle (Request.QueryString ["link"]);
	}

	// Get the path to be shown in the content fram
	string getContentFrame()
	{
		// Docs get shown from monodoc.ashx
		string monodocUrl="monodoc.ashx";
		string defaultParams="?link=root:";
		NameValueCollection qStringParams=Request.QueryString;

		// If no querystring params, show root link
		if(!qStringParams.HasKeys())
			return(monodocUrl+defaultParams);
		// else, build query for the content frame
		string nQueryString=monodocUrl+"?";
		foreach(string key in qStringParams)
			nQueryString+=(HttpUtility.UrlEncode(key)+"="+HttpUtility.UrlEncode(qStringParams[key]));
		return nQueryString;
	}
/*
void Page_Load (object sender, EventArgs e)
{
	if (User.Identity.IsAuthenticated){
		login.NavigateUrl = "logout.aspx";
		login.Text = "Logged in as " + User.Identity.Name;
	} else {
		login.NavigateUrl = "javascript:parent.content.login (parent.content.window.location)";
		login.Text = "Sign in / create account"; 
	}
}*/
   </script>

<!--HTML goes here-->
    <% = Global.IncludeExternalHeader (Global.ExternalResourceType.Html) %>
   <!--  <div id="dlogin">
       <asp:HyperLink id="login" runat="server"/>
     </div>

	 <div id="fsearch_companion"></div>
     <div id="fsearch_window"></div>
    </div>
-->
    <div id="main_part">
     <div id="side">
     <a class="doc-sidebar-toggle shrink" href="#"></a>
     <a class="doc-sidebar-toggle expand" href="#"></a>	   
	<div id="contents" class="activeTab">
	     <div id="contentList"></div>
       </div>
     </div>
     <div id="content_frame_wrapper"><iframe id="content_frame" src="<% =getContentFrame() %>"></iframe></div>
	</div>
    <% = Global.IncludeExternalFooter (Global.ExternalResourceType.Html) %>




<!--javascript goes here-->
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
<script src="plugins/search-plugin/search.js"></script>
<script src="plugins/sidebar/xtree/xmlextras.js"></script>
<script src="plugins/sidebar/ptree/tree.js"></script>
<script src="plugins/sidebar/sidebar.js"></script>

<script type="text/javascript">
var content_frame = $('#content_frame');
var page_link = $('#pageLink');

change_page = function (pagename) {
    content_frame.attr ('src', 'monodoc.ashx?link=' + pagename);
    page_link.attr ('href', '?link=' + pagename);
    if (window.history && window.history.pushState)
        window.history.pushState (null, '', '/?link=' + pagename);
};

var tree = new PTree ();
tree.strSrcBase = 'monodoc.ashx?tree=';
tree.strActionBase = '?link=';
tree.strImagesBase = 'plugins/sidebar/xtree/images/msdn2/';
tree.strImageExt = '.gif';
tree.onClickCallback = function (url) { change_page (url); };
var content = document.getElementById ('contentList');
var root = tree.CreateItem (null, 'Documentation List', 'root:', '', true);
content.appendChild (root);
<% = Global.CreateTreeBootFragment () %>

update_tree = function () {
  var tree_path = $('#content_frame').contents ().find ('meta[name=TreePath]');
  if (tree_path.length > 0) {
     var path = tree_path.attr ('value');
     tree.ExpandFromPath (path);
  }
};
update_tree ();
add_native_browser_link = function () {
	var contentDiv = $('#content_frame').contents ().find ('div[class=Content]').first ();
	if (contentDiv.length > 0 && contentDiv.attr ('id')) {
		var id = contentDiv.attr ('id').replace (':Summary', '');
		var h2 = contentDiv.children ('h2').first ();
		if (h2.prev ().attr ('class') != 'native-browser')
		h2.before ('<p><a class="native-browser" href="mdoc://' + encodeURIComponent (id) + '"><span class="native-icon"><img src="images/native-browser-icon.png" /></span>Open in Native Browser</a></p>');
	}
};
add_native_browser_link ();

content_frame.load (update_tree);
content_frame.load (add_native_browser_link);
</script>

<!--include external javascript-->
<% = Global.IncludeExternalHeader (Global.ExternalResourceType.Javascript) %>
<% = Global.IncludeExternalFooter (Global.ExternalResourceType.Javascript) %>
</body>
</html>
