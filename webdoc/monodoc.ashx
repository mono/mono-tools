<%@ WebHandler Language="c#" class="Mono.Website.Handlers.MonodocHandler" %>
<%@ Assembly name="monodoc" %>

#define MONODOC_PTREE

//
// Mono.Web.Handlers.MonodocHandler.  
//
// Authors:
//     Ben Maurer (bmaurer@users.sourceforge.net)
//     Miguel de Icaza (miguel@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2006 Novell, Inc.
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Xsl;
using Monodoc;
using System.Text.RegularExpressions;

namespace Mono.Website.Handlers
{
	public class MonodocHandler : IHttpHandler
	{
		static DateTime monodoc_timestamp, handler_timestamp;

		static MonodocHandler ()
		{
			monodoc_timestamp = File.GetCreationTimeUtc (typeof (Node).Assembly.Location);
			handler_timestamp = File.GetCreationTimeUtc (typeof (MonodocHandler).Assembly.Location);

			DumpEmbeddedImages ();
		}

		// Dumps the embedded images from monodoc.dll
		static void DumpEmbeddedImages ()
		{
			try {
				Directory.CreateDirectory ("mdocimages");
			} catch {}

			var mass = typeof (Node).Assembly;
		      	var buffer = new byte [4096];
			foreach (string image in mass.GetManifestResourceNames ()){
				if (!(image.EndsWith ("png") || image.EndsWith ("jpg")))
					continue;
				var target = Path.Combine ("mdocimages", image);
				if (File.Exists (target))
					continue;

				using (var output = File.Create (target)){
					var input = mass.GetManifestResourceStream (image);
					int n;
					while ((n = input.Read (buffer, 0, buffer.Length)) > 0){
						output.Write (buffer, 0, n);
					}
				}
			}
		}

		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			string s;

			s = (string) context.Request.Params["link"];
			if (s != null){
				HandleMonodocUrl (context, s);
				return;
			}

			s = (string) context.Request.Params["tree"];
			Console.WriteLine ("tree request:  '{0}'", s);
			if (s != null){
				if (s == "boot")
					HandleBoot (context);
				else {
					HandleTree (context, s);
				}
				return;
			}
			context.Response.Write ("<html><body>Unknown request</body></html>");
			context.Response.ContentType = "text/html";
		}
		
		void HandleTree (HttpContext context, string tree)
		{
		    context.Response.ContentType = "text/xml";
		    //Console.WriteLine ("Tree request: " + tree);
		    try {
			//
			// Walk the url, found what we are supposed to render.
			//
			string [] nodes = tree.Split (new char [] {'@'});
			Node current_node = Global.help_tree;
			for (int i = 0; i < nodes.Length; i++){
				try {
				current_node = (Node)current_node.Nodes [int.Parse (nodes [i])];
				} catch (Exception e){
					Console.WriteLine ("Failure with: {0} {i}", tree, i);
				}
			}

			XmlTextWriter w = new XmlTextWriter (context.Response.Output);

			w.WriteStartElement ("tree");

			for (int i = 0; i < current_node.Nodes.Count; i++) {
				Node n = (Node)current_node.Nodes [i];

				w.WriteStartElement ("tree");
				w.WriteAttributeString ("text", n.Caption);

				if (n.tree != null && n.tree.HelpSource != null)
					w.WriteAttributeString ("action", HttpUtility.UrlEncode (n.PublicUrl));

				if (n.Nodes != null){
					w.WriteAttributeString ("src", tree + "@" + i);
				}
				w.WriteEndElement ();
			}
			w.WriteEndElement ();
	           } catch (Exception e) {
			Console.WriteLine (e);
		   }
		   //Console.WriteLine ("Tree request satisfied");
		}
		
		void CheckLastModified (HttpContext context)
		{
			string strHeader = context.Request.Headers ["If-Modified-Since"];
			DateTime lastHelpSourceTime = Global.help_tree.LastHelpSourceTime;
			try {
				if (strHeader != null && lastHelpSourceTime != DateTime.MinValue) {
				   	// Console.WriteLine ("Got this: {0}", strHeader);
					DateTime dtIfModifiedSince = DateTime.ParseExact (strHeader, "r", null).ToUniversalTime ();
					DateTime ftime = lastHelpSourceTime.ToUniversalTime ();
					//Console.WriteLine ("Times:");
					//Console.WriteLine ("   ftime: {0}", ftime);
					//Console.WriteLine ("   monod: {0}", monodoc_timestamp);
					//Console.WriteLine ("   handl: {0}", handler_timestamp);
					//Console.WriteLine ("    dtIf: {0}", dtIfModifiedSince);
					if (dtIfModifiedSince < DateTime.UtcNow &&
					    ftime <= dtIfModifiedSince && 
					    monodoc_timestamp <= dtIfModifiedSince && 
					    handler_timestamp <= dtIfModifiedSince) {
						context.Response.StatusCode = 304;
						return;
					}
				}
			} catch { } 

			long ticks = System.Math.Max (monodoc_timestamp.Ticks, handler_timestamp.Ticks);
			if (lastHelpSourceTime != DateTime.MinValue) {
				ticks = System.Math.Max (ticks, lastHelpSourceTime.Ticks);
				DateTime lastWT = new DateTime (ticks).ToUniversalTime ();
				context.Response.AddHeader ("Last-Modified", lastWT.ToString ("r"));
			}

		}

		void HandleMonodocUrl (HttpContext context, string link)
		{
			if (link.StartsWith ("source-id:") &&
				(link.EndsWith (".gif") || link.EndsWith (".jpeg") ||
				 link.EndsWith (".jpg")  || link.EndsWith(".png"))){
				switch (link.Substring (link.LastIndexOf ('.') + 1))
				{
				case "gif":
					context.Response.ContentType = "image/gif";
					break;
				case "jpeg":
				case "jpg":
					context.Response.ContentType = "image/jpeg";
					break;
				case "png":
					context.Response.ContentType = "image/png";
					break;
				default:
					throw new Exception ("Internal error");
				}
				
				Stream s = Global.help_tree.GetImage (link);
				
				if (s == null)
					throw new HttpException (404, "File not found");
				
				CheckLastModified (context);
				if (context.Response.StatusCode == 304)
					return;

				Copy (s, context.Response.OutputStream);
				return;
			}

			if (Global.help_tree == null)
				return;
			Node n;
			//Console.WriteLine ("Considering {0}", link);
			string content = Global.help_tree.RenderUrl (link, out n);
			CheckLastModified (context);
			if (context.Response.StatusCode == 304){
	   			//Console.WriteLine ("Keeping", link);

				return;
			}

			PrintDocs (content, n, context, GetHelpSource (n));
		}

		HelpSource GetHelpSource (Node n)
		{
			if (n != null)
				return n.tree.HelpSource;
			return null;
		}
		
		void HandleTreeLink (HttpContext context, string link)
		{
			string [] lnk = link.Split (new char [] {'@'});
			
			if (lnk.Length == 1) {
				HandleMonodocUrl (context, link);
				return;
			}
				
			int hsId = int.Parse (lnk [0]);
			
			Node n;
			HelpSource hs = Global.help_tree.GetHelpSourceFromId (hsId);
			string content = hs.GetText (lnk [1], out n);
			if (content == null) {
				content = Global.help_tree.RenderUrl (lnk [1], out n);
				hs = GetHelpSource (n);
			}
			PrintDocs (content, n, context, hs);
		}

		void Copy (Stream input, Stream output)
		{
			const int BUFFER_SIZE=8192; // 8k buf
			byte [] buffer = new byte [BUFFER_SIZE];

			int len;
			while ( (len = input.Read (buffer, 0, BUFFER_SIZE)) > 0)
				output.Write (buffer, 0, len);

			output.Flush();
		}

		string requestPath;
		void PrintDocs (string content, Node node, HttpContext ctx, HelpSource hs)
		{
			string title = (node == null || node.Caption == null) ? "Mono XDocumentation" : node.Caption;

			ctx.Response.Write (@"
<html>
<head>
		<link type='text/css' rel='stylesheet' href='common.css' media='all' title='Default style' />
<script>
<!--
function login (rurl)
{
	document.location.href = 'login.aspx?ReturnUrl=' + rurl;
}

function load ()
{
	// If topic loaded in a window by itself, load index.aspx with the same set of params.
	if (top.location == document.location)
	{
		top.location.href = 'index.aspx'+document.location.search;
	}

	parent.Header.document.getElementById ('pageLink').href = parent.content.window.location;
	objs = document.getElementsByTagName('img');
	for (i = 0; i < objs.length; i++)
	{
		e = objs [i];
		if (e.src == null) continue;
		
		objs[i].src = makeLink (objs[i].src);
	}
}

function makeLink (link)
{
	if (link == '') return '';
	if (link.charAt(0) == '#') return link;
	
	protocol = link.substring (0, link.indexOf (':'));

	switch (protocol)
		{
		case 'http':
		case 'ftp':
		case 'mailto':
		case 'javascript':
			return link;
			
		default:
			if(document.all) {
				return '");
			ctx.Response.Write (ctx.Request.Path);
			ctx.Response.Write (@"?link=' + link.replace(/\+/g, '%2B').replace(/file:\/\/\//, '');
			}
			return '");

			ctx.Response.Write (ctx.Request.Path);
			ctx.Response.Write (@"?link=' + link.replace(/\+/g, '%2B');
		}
}
-->");
			ctx.Response.Write ("</script><title>");
			ctx.Response.Write (title);
			ctx.Response.Write ("</title>\n");
	
			if (hs != null && hs.InlineCss != null) {
				ctx.Response.Write ("<style type=\"text/css\">\n");
				ctx.Response.Write (hs.InlineCss);
				ctx.Response.Write ("</style>\n");
			}
			if (hs != null && hs.InlineJavaScript != null) {
				ctx.Response.Write ("<script type=\"text/JavaScript\">\n");
				ctx.Response.Write (hs.InlineJavaScript);
				ctx.Response.Write ("</script>\n");
			}
			ctx.Response.Write (@"</head><body onLoad='load()'>");

			// Set up object variable, as it's required by the MakeLink delegate
			requestPath=ctx.Request.Path;
			string output;
	
			if (content == null)
				output = "No documentation available on this topic";
			else 
				output = MakeLinks(content);
			ctx.Response.Write (output);
			ctx.Response.Write (@"</body></html>");
		}


		string MakeLinks(string content)
		{
			MatchEvaluator linkUpdater=new MatchEvaluator(MakeLink);
			if(content.Trim().Length<1|| content==null)
				return content;
			try
			{
				string updatedContents=Regex.Replace(content,"(<a[^>]*href=['\"])([^'\"]+)(['\"][^>]*)(>)", linkUpdater);
				return(updatedContents);
			}
			catch(Exception e)
			{
				return "LADEDA" + content+"!<!--Exception:"+e.Message+"-->";
			}
		}
		
		// Delegate to be called from MakeLinks for fixing <a> tag
		string MakeLink (Match theMatch)
		{
			string updated_link = null;

			// Return the link without change if it of the form
			//	$protocol:... or #...
			string link = theMatch.Groups[2].ToString();
			if (Regex.Match(link, @"^\w+:\/\/").Success || Regex.Match(link, "^#").Success ||
					Regex.Match(link, @"^javascript:").Success)
				updated_link = theMatch.Groups[0].ToString();
			else if (link.StartsWith ("edit:")){
				link = link.Substring (5);
 				updated_link = String.Format("{0}/edit.aspx?link={2}{3} target=\"content\"{4}",
 					theMatch.Groups[1].ToString(),
 					requestPath, 
 					HttpUtility.UrlEncode (link.Replace ("file://","")),
 						theMatch.Groups[3].ToString(),
 						theMatch.Groups[4].ToString());
			
			} else {
				updated_link = String.Format ("{0}{1}?link={2}{3} target=\"content\"{4}",
					theMatch.Groups[1].ToString(),
                                        requestPath,
                                        HttpUtility.UrlEncode (link.Replace ("file://","")),
						theMatch.Groups[3].ToString(),
                                                theMatch.Groups[4].ToString());

			}
			return updated_link;
		}
		
		bool IHttpHandler.IsReusable
		{
			get {
				return true;
			}
		}

		void HandleBoot (HttpContext context)
		{
			context.Response.Write (@"
<html>
	<head>
		<link type='text/css' rel='stylesheet' href='ptree/tree.css'/>
		<link type='text/css' rel='stylesheet' href='sidebar.css'/>
		<script src='xtree/xmlextras.js'></script>
		<script src='ptree/tree.js'></script>
		<script src='sidebar.js'></script>
		<script>
		var tree = new PTree ();
		function onBodyLoad ()
		{
			tree.strTargetDefault = 'content';
			tree.strSrcBase = 'monodoc.ashx?tree=';
			tree.strActionBase = 'monodoc.ashx?link=';
			tree.strImagesBase = 'xtree/images/msdn2/';
			tree.strImageExt = '.gif';
			var content = document.getElementById ('contentList');
			var root = tree.CreateItem (null, 'Mono Documentation', 'intro.html', '', true);
			content.appendChild (root);
		");

		for (int i = 0; i < Global.help_tree.Nodes.Count; i++){
			Node n = (Node)Global.help_tree.Nodes [i];

			string url = n.PublicUrl;
			string target = "content";

			if (n.Caption == "Base Class Library" || n.Caption == "Mono Libraries") {
			       url = Global.kipunji_root_url + (n.Caption == "Base Class Library" ? "?display_all=true" : String.Empty);
			       target = "_top";
			}

			context.Response.Write (
				"tree.CreateItem (root, '" + n.Caption + "', '" + url + "', ");
	
			if (n.Nodes.Count != 0)
				context.Response.Write ("'" + i + "'");
			else	
				context.Response.Write ("null");
	
			if (i == Global.help_tree.Nodes.Count-1)
				context.Response.Write (", true");
			else
				context.Response.Write (", false");

			context.Response.Write (", '" + target + "'");
				
			context.Response.Write (@");
			");
		}
		context.Response.Write (@"
		}</script>
	</head>
	<body onLoad='javascript:onBodyLoad();' onkeydown='javascript:return tree.onKeyDown (event);'>
	  <div id='tabs'>
	    <ul>
	      <li id='contentsTab' class='selected'><a href='javascript:ShowContents();'>Contents</a></li>
	      <li id='indexTab' style='display:none;'><a href='javascript:ShowIndex();'>Index</a></li>
	    </ul>
	  </div>
	  <div id='contents' class='activeTab'>
	    <div id='contentList'>
	    </div>
	  </div>
	  <div id='index' class='tab'>
	    <p>
	    <label for='indexInput'>Lookup:</label> <input type='text' id='indexInput'/>
	    <img alt='Spinner-blue' id='search_spinner' src='images/searching.gif' style='display:none;' align='middle' />
	    <p id='errorText'></p>
	    <ul id='indexList'></ul>
	  </div>
	</body>
</html>
");
		}
	}
}
