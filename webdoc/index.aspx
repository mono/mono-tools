<%@ Page Language="C#" ClassName="Mono.Website.Index" MasterPageFile="api.master" %>
<%@ Assembly name="monodoc" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" Runat="Server">
<script language="c#" runat="server">
public string GetTitle ()
{
return Global.help_tree.GetTitle (Request.QueryString ["link"]);
}
// Get the path to be shown in the content frame
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
</script>

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
</asp:Content>

<asp:Content ID="Login" ContentPlaceHolderID="Login" Runat="Server">
<script language="C#" runat="server">
void Page_Load (object sender, EventArgs e)
{
	if (User.Identity.IsAuthenticated){
		login.NavigateUrl = "plugins/contributor-plugin/logout.aspx";
		login.Text = "Logged in as " + User.Identity.Name;
	} else {
		login.NavigateUrl = "javascript:parent.content.login (parent.content.window.location)";
		//login.Text = "Sign in / create account";
	}
}
</script>
<div id="dlogin">
       <asp:HyperLink id="login" runat="server"/>
</div>
</asp:Content>

<asp:Content ID="fsearch" ContentPlaceHolderID="FastSearch" Runat="Server">
<div id="fsearch_companion"></div>
<div id="fsearch_window"></div>
</asp:Content>
    
<asp:Content ID="Tree" ContentPlaceHolderID="TreeGenerator" Runat="Server">
<script type="text/javascript">
        var tree = new PTree ();
        tree.strSrcBase = 'monodoc.ashx?tree=';
        tree.strActionBase = '?link=';
        tree.strImagesBase = 'plugins/sidebar-plugin/dependencies/xtree/images/msdn2/';
        tree.strImageExt = '.gif';
        tree.onClickCallback = function (url) { change_page (url); };
        var content = document.getElementById ('contentList');
        var root = tree.CreateItem (null, 'Documentation List', 'root:', '', true);
        content.appendChild (root);
<% = Global.CreateTreeBootFragment () %>
</script>
</asp:Content>
