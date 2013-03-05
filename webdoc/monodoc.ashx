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
using System.Linq;
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
			string callback;

                        s = (string) context.Request.Params["link"];
                        if (s != null){
                                HandleMonodocUrl (context, s);
                                return;
                        }

                        s = (string) context.Request.Params["tree"];
                        Console.WriteLine ("tree request:  '{0}'", s);
                        if (s != null){
                                HandleTree (context, s);
                                return;
                        }

                        s = (string) context.Request.Params["fsearch"];
						callback = (string) context.Request.Params["callback"];
						Console.WriteLine ("Fast search requested for query {0}", s);
                        if (s != null) {
                                HandleFastSearchRequest (context, s, callback);
                                return;
                        }

                        s = (string) context.Request.Params["search"];
                        	callback = (string) context.Request.Params["callback"];
				Console.WriteLine ("Full search requested for query {0}", s);
                        if (s != null) {
                                HandleFullSearchRequest (context, s, callback);
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
					Console.WriteLine ("Failure with: {0} {1}", tree, i);
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

				s.CopyTo (context.Response.OutputStream);
				return;
			} else if (link.Equals ("root:", StringComparison.Ordinal) && File.Exists ("home.html")) {
				context.Response.WriteFile ("home.html");
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

		void HandleFastSearchRequest (HttpContext context, string request, string callback)
                {
                        if (string.IsNullOrWhiteSpace (request) || request.Length < 3) {
                                // Unprocessable entity
                                context.Response.StatusCode = 422;
                                return;
                        }


                        var searchIndex = Global.GetSearchIndex ();
                        var result = searchIndex.FastSearch (request, 15);
                        // return Json corresponding to the results
                        var answer = result == null || result.Count == 0 ? "[]" : "[" + 
                                Enumerable.Range (0, result.Count)
                      .Select (i => string.Format ("{{ \"name\" : \"{0}\", \"url\" : \"{1}\", \"fulltitle\" : \"{2}\" }}",
                                                   result.GetTitle (i), result.GetUrl (i), result.GetFullTitle (i)))
                      .Aggregate ((e1, e2) => e1 + ", " + e2) + "]";

						if (!string.IsNullOrWhiteSpace (callback))
							answer = callback + "(" + answer + ")";

                        Console.WriteLine ("answer is {0}", answer);

                        context.Response.ContentType = "application/json";
                        context.Response.Write (answer);
                }

                void HandleFullSearchRequest (HttpContext context, string request, string callback)
                {
                        if (string.IsNullOrWhiteSpace (request)) {
                                // Unprocessable entity
                                context.Response.StatusCode = 422;
                                return;
                        }
                        int start = 0, count = 0;
                        var searchIndex = Global.GetSearchIndex ();
                        Result result = null;
                        if (int.TryParse (context.Request.Params["count"], out count)) {
                                if (int.TryParse (context.Request.Params["start"], out start))
                                        result = searchIndex.Search (request, count, start);
                                else
                                        result = searchIndex.Search (request, count);
                        } else {
                                count = 20;
                                result = searchIndex.Search (request, count);
                        }
                        // return Json corresponding to the results
                        var answer = result == null || result.Count == 0 ? "[]" : "[" +
                                Enumerable.Range (0, result.Count)
                      .Select (i => string.Format ("{{ \"name\" : \"{0}\", \"url\" : \"{1}\", \"fulltitle\" : \"{2}\" }}",
                                                   result.GetTitle (i), result.GetUrl (i), result.GetFullTitle (i)))
                      .Aggregate ((e1, e2) => e1 + ", " + e2) + "]";

			if(!string.IsNullOrWhiteSpace (callback)) {
                        	answer = string.Format ("{0}({{ \"count\": {1}, \"start\": {2}, \"result\": {3} }})", callback, count, start, answer);
			}
                        
			Console.WriteLine ("answer is {0}", answer);

                        context.Response.ContentType = "application/json";
                        context.Response.Write (answer);
                }
		
		HelpSource GetHelpSource (Node n)
		{
			if (n != null)
				return n.tree.HelpSource;
			return null;
		}

		string requestPath;
		void PrintDocs (string content, Node node, HttpContext ctx, HelpSource hs)
		{
			string tree_path = string.Empty;
			Node current = node;
			while (current != null && current.Parent != null) {
				int index = current.Parent.Nodes.IndexOf (current);
				tree_path = '@' + (index + tree_path);
				current = current.Parent;
			}
			tree_path = tree_path.Length > 0 ? tree_path.Substring (1) : tree_path;
			Console.WriteLine ("Tree path is:" + tree_path);

			string title = (node == null || node.Caption == null) ? "Mono XDocumentation" : node.Caption;

			ctx.Response.Write (@"
<html>
<head>
	<link type='text/css' rel='stylesheet' href='views/monodoc.css' media='all' title='Default style' />
	<meta name='TreePath' value='");
		ctx.Response.Write (tree_path);
		ctx.Response.Write (@"' />
	<style type='text/css'>
  		body, h1, h2, h3, h4, h5, h6, .named-header {
    			word-wrap: break-word !important;
			font-family: 'Myriad Pro', 'myriad pro', Helvetica, Verdana, Arial !important; 
  		}
  		p, li, span, table, pre, .Content {
   			font-family: Helvetica, Verdana, Arial !important;
  		}
  		.named-header { height: auto !important; padding: 8px 0 20px 10px !important; font-weight: 600 !important; font-size: 2.3em !important; margin: 0.3em 0 0.6em 0 !important; margin-top: 0 !important; font-size: 2.3em !important; }
  		h2 { padding-top: 1em !important; margin-top: 0 !important;  font-weight: 600 !important; font-size: 1.8em !important; color: #333 !important; }
  		p { margin: 0 0 1.3em !important; color: #555753 !important; line-height: 1.8 !important; }
  		body, table, pre { line-height: 1.8 !important; color: #55753 !important; } 
 		.breadcrumb { font-size: 12px !important; }
	</style>
	
	<script src='//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js'></script>

	<script type='text/javascript'>

	function printFrame() {
		window.print();
		return false;
	}
	
	//pass the function object to parent
	parent.printFrame = printFrame;

	function try_change_page (link, e)
	{
		if (!e)
			e = window.event;
		if (e.ctrlKey || e.shiftKey || e.altKey || e.metaKey || e.modifiers > 0)
			return;
		window.parent.change_page (link)
	}

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
		case 'https':
			return link;
			
		default:
			if(document.all) {
				return 'monodoc.ashx?link=' + link.replace(/\+/g, '%2B').replace(/file:\/\/\//, '');
			}
			return 'monodoc.ashx?link=' + link.replace(/\+/g, '%2B');
		}
}");
			if (!string.IsNullOrEmpty (Global.ua)) {
				ctx.Response.Write (@"var _gaq = _gaq || [];
 _gaq.push(['_setAccount', '");
				ctx.Response.Write (Global.ua);
				ctx.Response.Write (@"']);
 _gaq.push(['_trackPageview']);

 (function() {
   var ga = document.createElement('script'); ga.type =
'text/javascript'; ga.async = true;
   ga.src = ('https:' == document.location.protocol ? 'https://ssl' :
'http://www') + '.google-analytics.com/ga.js';
   var s = document.getElementsByTagName('script')[0];
s.parentNode.insertBefore(ga, s);
 })();");
			}
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
			ctx.Response.Write (@"</head><body onload='load()'>");
			ctx.Response.Write (@"<iframe id='helpframe' src='' height='0' width='0' frameborder='0'></iframe>");

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
				updated_link = String.Format ("{0}{1}?link={2}{3} onclick=\"try_change_page('{2}')\" {4}",
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
	}
}
