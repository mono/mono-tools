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
<script type="text/javascript">
	var prev = 5;
	function compare_click(idx) {
		var active_div = "col" + idx.toString();
		var active_cmp = "cmp" + idx.toString();
		var prev_div = "col" + prev.toString();
		var prev_cmp = "cmp" + prev.toString();
		prev = idx;
		var elem = document.getElementById (prev_div);
		elem.style.display = "none";
		elem = document.getElementById (active_div);
		elem.style.display = "inherit";
		elem = document.getElementById (prev_cmp);
		elem.style.fontWeight = "normal";
		var elem = document.getElementById (active_cmp);
		elem.style.fontWeight = "bold";
	}
</script>
<style type="text/css">
#content {
}
#compare_list {
}
#assemblies_list {
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
		Response.Write (String.Format ("<li><a href=\"status.aspx?reference={0}&profile={1}&assembly={2}\">{2}</a></li>\n", reference, profile, assembly));
}

</script>
</head>
<body>
    <div id="header">
    	<h1>Mono Class Status Pages</h1>
    </div>
    <div id="content">
    <p>Click on any of the profiles to get the list of assemblies. Then select an assembly to see the differences.</p>
<div id="compare_list">
<ul class="plain">
<li id="cmp1"><a href="javascript:compare_click(1);">Mono 4.0 vs .NET 4.0beta2</a></li>
<li id="cmp2"><a href="javascript:compare_click(2);">Mono 3.5 vs .NET 3.5</a></li>
<li id="cmp3"><a href="javascript:compare_click(3);">Moonlight vs Silverlight 3.0</a></li>
<li id="cmp4"><a href="javascript:compare_click(4);">Moonlight vs Silverlight 2.0</a></li>
<li id="cmp5" style="font-weight: bold;"><a href="javascript:compare_click(5);">Mono 3.5 vs .NET 2.0</a></li>
</ul>
</div>

<div id"assemblies_list">
<div id="col1" style="display: none;">
<h2>Mono 4.0 vs .NET 4.0beta2</h2>

	<p>Snapshot comparing the latest Mono and .NET 4.0beta2.
	<ul class="plain">
		<% GenerateList ("4.0", "4.0"); %>
	</ul>
</div>
<div id="col2" style="display: none;">
<h2>Mono 3.5 vs .NET 3.5</h2>

	<p>This shows the work-in-progress of Mono towards completing
	the 3.5 SP1 APIs.

	<ul class="plain">
		<% GenerateList ("3.5", "2.0"); %>
	</ul>
</div>

<div id="col3" style="display: none;">
<h2>Moonlight vs Silverlight 3.0</h2>

	<p>This is used to compare Mono + Moonlight assemblies against
	the published API of Silverlight 3.0.

	<ul class="plain">
		<% GenerateList ("SL3", "2.1"); %>
	</ul>
</div>

<div id="col4" style="display: none;">
<h2>Moonlight vs Silverlight 2.0</h2>

	<p>This is used to compare Mono + Moonlight assemblies against
	the published API of Silverlight 2.0.

	<ul class="plain">
		<% GenerateList ("SL2", "2.1"); %>
	</ul>
</div>

<div id="col5" style="display: inherit;">
<h2>Mono 3.5 vs .NET 2.0</h2>

	<p>This is comparing Mono's latest API which is typically
	installed in the lib/mono/2.0/ directory, but contains the 3.5 API.   

	<p>This list is only useful to determine if there are some
	major missing features, but not for detecting if there are
	extra APIs (we will have them, as we are now tracking 3.5)

	<ul class="plain">
		<% GenerateList ("2.0", "2.0"); %>
	</ul>
</div>
</div>

</div>
<div id="footer">&nbsp;</div>
</body>
</html>
