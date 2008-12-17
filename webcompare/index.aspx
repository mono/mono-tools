<%@ Page Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="GuiCompare" %>
<html>
<head>
<style type="text/css">
#col1 {
     width: 30%;
     float: left;
     margin-right: 4%;
     border-right:1px dotted #AAAAAA;
}

#col2 {
     width: 30%;
     float: left;
     margin-right: 4%;
     border-right:1px dotted #AAAAAA;
}

#col3 {
     width: 30%;
     float: left;
}
</style>
<script runat="server">

void GenerateList (string reference, string profile)
{
	string mpath = Server.MapPath ("masterinfos/" + reference);
	string bpath = Server.MapPath ("binary/" + profile);

	if (!Directory.Exists (mpath)){
		Response.Write ("Directory does not exist for masterinfo: " + reference);
		return;
	}

	if (!Directory.Exists (bpath)){
		Response.Write ("Directory does not exist for binary: " + profile);
		return;
	}
	
	var infos = from p in Directory.GetFiles (mpath)
		select System.IO.Path.GetFileNameWithoutExtension (p);

	var dlls  = from p in Directory.GetFiles (bpath)
	    	select System.IO.Path.GetFileNameWithoutExtension (p);

	foreach (var assembly in (from p in infos.Intersect (dlls) orderby p select p))
		Response.Write (String.Format ("<li><a href=\"status.aspx?reference={0}&profile={1}&assembly={2}\">{2}</a>\n", reference, profile, assembly));
}

</script>
</head>
<body>

<div id="col1">
<h1>Mono 3.5 vs .NET 3.5</h1>

	<ul>
		<% GenerateList ("3.5", "2.0"); %>
	</ul>
</div>

<div id="col2">
<h1>Mono 3.5 vs .NET 2.0</h1>

	<ul>
		<% GenerateList ("2.0", "2.0"); %>
	</ul>
</div>

<div id="col3">
<h1>Mono 1.1 vs .NET 1.1</h1>

	<ul>
		<% GenerateList ("1.1", "1.0"); %>
	</ul>
</div>
</body>
</html>