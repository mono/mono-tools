//
// WebKitHtmlRender.cs: Implementation of IHtmlRender that uses WebKit
//
// Author: Alp Toker <alp@nuanti.com>
//

using System;
using System.IO;
using Gtk;
using WebKit;

namespace Monodoc {
public class WebKitHtmlRender : IHtmlRender {

	WebView web_view;
	public Widget HtmlPanel {
		get { return (Widget) web_view; }
	}

	string url;
	public string Url {
		get { return url; }
	}

	RootTree help_tree;
	public event EventHandler OnUrl;
	public event EventHandler UrlClicked;

	public WebKitHtmlRender (RootTree help_tree) 
	{
		web_view = new WebView ();
		web_view.Show (); 
		web_view.NavigationRequested += delegate (object sender, NavigationRequestedArgs e) {
			if (e.Request.Uri == "about:blank")
				return;
			url = e.Request.Uri;
			if (UrlClicked != null)
				UrlClicked (this, new EventArgs());
			e.RetVal = NavigationResponse.Ignore;
		};
		web_view.HoveringOverLink += delegate (object sender, HoveringOverLinkArgs e) {
			url = e.Link;
			if (OnUrl != null)
			  OnUrl (this, new EventArgs ());
		};
		this.help_tree = help_tree;
	}

	public void JumpToAnchor (string anchor)
	{
		web_view.Open ("#" + anchor);
	}

	public void Copy () 
	{
		web_view.CopyClipboard ();
	}

	public void SelectAll () 
	{
		web_view.SelectAll ();	
	}

	public void Render (string html) 
	{
		web_view.LoadHtmlString (html, null);
	}

	public void Print (string html)
	{
		web_view.ExecuteScript ("print();");
	}

	public bool Initialize ()
	{
		return true;
	}

	public Capabilities Capabilities
	{
		get { return Capabilities.Css | Capabilities.Fonts; }
	}

	public string Name
	{
		get { return "WebKit"; }
	}


}
}
