<%@ Page Language="C#" ClassName="Mono.Website.Index" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Assembly name="monodoc" %>

<html>
  <head>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" >
    <title><%=GetTitle ()%></title>
    <link rel="stylesheet" type="text/css" href="common.css" media="all" title="Default Style"/>
    <link rel="stylesheet" type="text/css" href="main.css" media="all" />
    <link type="text/css" rel="stylesheet" href="ptree/tree.css"/>
	<link type="text/css" rel="stylesheet" href="sidebar.css"/>
    <% = Global.IncludeExternalHeader (Global.ExternalResourceType.Css) %>
    <% = Global.IncludeExternalFooter (Global.ExternalResourceType.Css) %>
  </head>
  <body>
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

void Page_Load (object sender, EventArgs e)
{
	if (User.Identity.IsAuthenticated){
		login.NavigateUrl = "logout.aspx";
		login.Text = "Logged in as " + User.Identity.Name;
	} else {
		login.NavigateUrl = "javascript:parent.content.login (parent.content.window.location)";
		//login.Text = "Sign in / create account"; 
	}
}
    </script>
    <% = Global.IncludeExternalHeader (Global.ExternalResourceType.Html) %>
    <div id="banner" style="color: rgb(255, 255, 255);">
     <div id="rightSide">
       <label for="search">Search: </label>
       <input id="fsearch" type="search" placeholder="Enter search request" style="width:19em; margin-right: 10px"/>
	   <a href="#" onclick="document.getElementById ('content_frame').contentWindow.print ()"><img class="toolbar" src="images/print.png" width="24" height="24" alt="Print" title="Print this document"/></a>
       <a id="pageLink" href="/">
          <img class="toolbar" src="images/link.png" width="24" height="24" alt="Link to this document" title="Link to this document"/>
       </a>
     </div>

     <div id="header">
        <h1>Mono Documentation</h1>
     </div>
     <div id="dlogin">
       <asp:HyperLink id="login" runat="server"/>
     </div>

	 <div id="fsearch_companion"></div>
     <div id="fsearch_window"></div>
    </div>
    <div id="main_part">
     <div id="side">
	   <div id="contents" class="activeTab">
	     <div id="contentList"></div>
       </div>
     </div>
     <div><iframe id="content_frame" src="<% =getContentFrame() %>"></iframe></div>
	</div>
    <% = Global.IncludeExternalFooter (Global.ExternalResourceType.Html) %>

<script src="//ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
<script src="search.js"></script>
<script src="xtree/xmlextras.js"></script>
<script src="ptree/tree.js"></script>
<script src="sidebar.js"></script>
<script type="text/javascript">
var tree = new PTree ();
tree.strSrcBase = 'monodoc.ashx?tree=';
tree.strActionBase = '?link=';
tree.strImagesBase = 'xtree/images/msdn2/';
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
content_frame.load (update_tree);
</script>
<% = Global.IncludeExternalHeader (Global.ExternalResourceType.Javascript) %>
<% = Global.IncludeExternalFooter (Global.ExternalResourceType.Javascript) %>
</body>
</html>
