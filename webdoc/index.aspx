<%@ Page Language="C#" ClassName="Mono.Website.Index" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<html>
  <head>
    <title><%=GetTitle ()%></title>
    <link rel="stylesheet" type="text/css" href="common.css" media="all" title="Default Style"/>
    <link rel="stylesheet" type="text/css" href="main.css" media="all" />
    <link type="text/css" rel="stylesheet" href="ptree/tree.css"/>
	<link type="text/css" rel="stylesheet" href="sidebar.css"/>
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
    <div style="color: rgb(255, 255, 255); background-color: #c0dda2;">
     <div id="header">
        <h1>Mono Documentation</h1>
     </div>
     <div id="dlogin">
       <asp:HyperLink id="login" runat="server"/>
     </div>

     <div id="rightSide">
       <label for="search">Search: </label>
       <input id="fsearch" type="search" placeholder="Enter search request" style="width:19em"/>
       <a id="pageLink" href="/">
          <img class="toolbar" src="images/link.png" width="24" height="24" alt="Link to this document" title="Link to this document"/>
       </a>
     </div>
     <div id="fsearch_window"></div>
    </div>
    <div>
     <div style="width:20%; height: 95%; float: left; border-right: 1px solid black; overflow-x: hidden; overflow-y: auto">
	   <div id='contents' class='activeTab'>
	    <div id='contentList'></div>
     </div>
    </div>
    <div><iframe id="content_frame" style="width:79.9%; height: 95%;" src="<% =getContentFrame() %>"></iframe></div>
   </div>

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
</body>
</html>
