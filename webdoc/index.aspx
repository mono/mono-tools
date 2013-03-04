<%@ Page Language="C#" ClassName="Mono.Website.Index" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Assembly name="monodoc" %>

<html>
  <head>
    <title><% = WebConfigurationManager.AppSettings["Title"] %></title>
    <link href="favicon.ico" type="image/png" rel="icon">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" >
    <link type="text/css" rel="stylesheet" href="reset.css"/>
    <% = Global.Plugin (Global.PluginContent.Css) %>    
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
  </head>
  <body>
  <% = Global.Plugin (Global.PluginContent.Header) %>
    <!--Do our c# scripts here to generate iframe/monodoc magic-->
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
     <% = Global.Plugin (Global.PluginContent.Footer) %>
<!--include external javascript-->
<% = Global.CreateTreeBootFragment () %>
<% = Global.Plugin (Global.PluginContent.Javascript) %>
</body>
</html>
