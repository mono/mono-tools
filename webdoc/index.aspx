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

<div id="content_frame_wrapper"><iframe id="content_frame" src="<% =getContentFrame() %>"></iframe></div>        
</asp:Content>

<asp:Content ID="fsearch" ContentPlaceHolderID="FastSearch" Runat="Server">
<div id="fsearch_companion"></div>
<div id="fsearch_window"></div>
</asp:Content>
    
<asp:Content ID="CustomTree" ContentPlaceHolderID="CustomTreeGenerator" Runat="Server">
<script type="text/javascript">
		//create a container for the sidebar to sit in
        	var container = $("#sidebar_container");
        	container.append("<div id=\"side\"><div id=\"contents\" class=\"activeTab\"><div id=\"contentList\"></div></div></div>");

        	//populate the sidebar with our data
        	var tree = new PTree ();
        	tree.strSrcBase = 'monodoc.ashx?tree=';
        	tree.strActionBase = '?link=';
        	tree.strImagesBase = 'plugins/sidebar-plugin/dependencies/xtree/images/msdn2/';
        	tree.strImageExt = '.gif';
        	tree.onClickCallback = function (url) { change_page (url); };
        	var content = document.getElementById ('contentList');
        	var root = tree.CreateItem (null, '', 'root:', '', true);
        	content.appendChild (root);
		<% = Global.CreateTreeBootFragment () %>
</script>
</asp:Content>

