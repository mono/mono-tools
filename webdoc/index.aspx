<%@ Page Language="C#" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<html>
  <head>
    <title>Mono Documentation</title>
  </head>
        <script language="c#" runat="server">
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
        </script>

<frameset rows="75,*" frameborder="0" border="1">
 <frame src="header.aspx" name="Header" id='header' scrolling="no" noresize="true" />
  <frameset cols="20%,80%" frameborder="1" border="1">
    <frame src="monodoc.ashx?tree=boot" name="Tree" />
    <frame src="<% =getContentFrame() %>" name="content" />
  </frameset>
</frameset>
</html>
