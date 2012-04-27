//
// A few routines that were copy-pasted from the docbrowser
//

using System;
using Monodoc;
using System.IO;

namespace WinDoc
{
	public class DocTools {
		public static string GetHtml (string url, HelpSource helpSource)
		{
			Node _;
			return GetHtml (url, helpSource, out _);
		}
		
		public static string GetHtml (string url, HelpSource helpSource, out Node match)
		{
			string htmlContent = null;
			match = null;
			
			if (helpSource != null)
				htmlContent = helpSource.GetText (url, out match);
			if (htmlContent == null){
				// the displayed url have a lower case type code (e.g. t: instead of T:) which confuse monodoc
				if (url.Length > 2 && url[1] == ':')
					url = char.ToUpperInvariant (url[0]) + url.Substring (1);
				// It may also be url encoded so decode it
				url = Uri.UnescapeDataString (url);
				htmlContent = Program.Root.RenderUrl (url, out match);
				if (htmlContent != null && match != null && match.tree != null){
					helpSource = match.tree.HelpSource;
				}
			}
			if (htmlContent == null)
				return null;
			
			var html = new StringWriter ();
   			html.Write ("<html>\n<head><title>{0}</title>", url);
			
			if (helpSource != null){
            	if (helpSource.InlineCss != null) 
                    html.Write (" <style type=\"text/css\">{0}</style>\n", helpSource.InlineCss);
				if (helpSource.InlineJavaScript != null)
                    html.Write ("<script type=\"text/JavaScript\">{0}</script>\n", helpSource.InlineJavaScript);
            }

            html.Write ("</head><body>");
            html.Write (htmlContent);
            html.Write ("</body></html>\n");
            return html.ToString ();
		}
	}
}

