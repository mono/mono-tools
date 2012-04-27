<%@ Page Language="C#" %>
<%@ OutputCache Duration="60" VaryByParam="none" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Collections.Specialized" %>
<%@ Import Namespace="GuiCompare" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
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
	GenerateList (reference, "masterinfos/", profile, "binary/", ".dll");
}

void GenerateList (string reference, string ref_dir, string profile, string prof_dir, string dll_extension)
{
	string mpath = Server.MapPath (ref_dir + reference);
	string bpath = Server.MapPath (prof_dir + profile);

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
		where p.EndsWith (dll_extension)
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

	<h2>.NET 4.0 vs .NET 4.5</h2>

		<p>This shows the API added between 4.0 and 4.5.

		<ul class="assemblies">
			<% GenerateList ("4.0", "masterinfos/", "4.5", "masterinfos/", ".xml"); %>
		</ul>
	</div>

	<div id="col2">
	<h2>Mono 4.5 vs .NET 4.5</h2>

		<p>This shows the work-in-progress of Mono towards completing
		the 4.5 APIs.

		<ul class="assemblies">
			<% GenerateList ("4.5", "4.5"); %>
		</ul>
	</div>

	<div id="col3">
	<h2>Mono 4.0 vs .NET 4.0</h2>

		<p>This shows the work-in-progress of Mono towards completing
		the 4.0 APIs.

		<ul class="assemblies">
			<% GenerateList ("4.0", "4.0"); %>
		</ul>
	</div>
</div>
<div id="footer">&nbsp;</div>

</body>
</html>
