<%@ Page Language="C#" %>
<%@ OutputCache Duration="60" VaryByParam="none" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="GuiCompare" %>
<html>
<head>
<title>Mono - Class status pages</title>
<link href="main.css" media="screen" type="text/css" rel="stylesheet">
<style type="text/css">
#col1 {
     width: 24%;
     float: left;
     margin-right: 1em;
     border-right:1px dotted #AAAAAA;
}

#col2 {
     width: 23%;
     float: left;
     margin-right: 1em;
     border-right:1px dotted #AAAAAA;
}

#col3 {
     width: 24%;
     float: left;
     margin-right: 1em;
     border-right:1px dotted #AAAAAA;
}

#col4 {
     width: 23%;
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
		where p.EndsWith (".dll")
	    	select System.IO.Path.GetFileNameWithoutExtension (p);

	foreach (var assembly in (from p in infos.Intersect (dlls) orderby p select p))
		Response.Write (String.Format ("<li><a href=\"status.aspx?reference={0}&profile={1}&assembly={2}\">{2}</a>\n", reference, profile, assembly));
}

</script>
</head>
<body>
    <div id="header">
    	<h1>Mono Class Status Pages</h1>
    </div>
    <div id="content">
<div id="col1">
<h2>Mono 3.5 vs .NET 3.5</h2>

	<p>This shows the work-in-progress of Mono towards completing
	the 3.5 SP1 APIs.

	<ul class="assemblies">
		<% GenerateList ("3.5", "2.0"); %>
	</ul>
</div>

<div id="col2">
<h2>Moonlight vs Silverlight 2.0</h2>

	<p>This is used to compare Mono + Moonlight assemblies against
	the published API of Silverlight 2.0.

	<ul class="assemblies">
		<% GenerateList ("SL2", "2.1"); %>
	</ul>
</div>

<div id="col3">
<h2>Mono 3.5 vs .NET 2.0</h2>

	<p>This is comparing Mono's latest API which is typically
	installed in the lib/mono/2.0/ directory, but contains the 3.5 API.   

	<p>This list is only useful to determine if there are some
	major missing features, but not for detecting if there are
	extra APIs (we will have them, as we are now tracking 3.5)

	<ul class="assemblies">
		<% GenerateList ("2.0", "2.0"); %>
	</ul>
</div>

<div id="col4">
<h2>Mono 1.1 vs .NET 1.1</h2>

	<p>The Mono 1.1 API is typically used for embedded scenarios,
	so we are tracking here any major differences in the area of
	extra APIs that should not be exposed while building the
	NET_1_1 profile.

	<ul class="assemblies">
		<% GenerateList ("1.1", "1.0"); %>
	</ul>
</div>
</div>
<div id="footer">&nbsp;</div>
</body>
</html>
