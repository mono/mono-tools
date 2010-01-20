//
//
// GtkHtmlHtmlRender.cs: Implementation of IHtmlRender that uses Gtk.HTML
//
// Author: Mario Sopena
// Author:	Rafael Ferreira <raf@ophion.org>
//
using System;
using Gtk;
using System.IO;
using System.Reflection;

namespace Monodoc {
class GtkHtmlHtmlRender : IHtmlRender {
	
	HTML html_panel;
	public Widget HtmlPanel {
		get { return (Widget) html_panel; }
	}

	string url;
	public string Url {
		get { return url; }
	}

	RootTree help_tree;
	public event EventHandler OnUrl;
	public event EventHandler UrlClicked;

	
	public GtkHtmlHtmlRender (RootTree help_tree) 
	{
		this.help_tree = help_tree;
	}
	
	protected void LinkClicked (object o, LinkClickedArgs args)
	{
		url = FixUrl (args.Url);
		if (UrlClicked != null)
			UrlClicked (this, new EventArgs());
	}

	static string FixUrl (string url)
	{
		if (url == null)
			return url;
		return url.Replace ("&lt;", "<").Replace ("&gt;", ">");
	}
	
	protected void OnUrlMouseOver (object o, OnUrlArgs args)
	{
		url = FixUrl (args.Url);
		if (OnUrl != null)
			OnUrl (this, args);
	}

	public void JumpToAnchor (string anchor)
	{
		html_panel.JumpToAnchor(anchor);
	}

	public void Copy () 
	{
		html_panel.Copy();	
	}

	public void SelectAll () 
	{
		html_panel.SelectAll();	
	}

	public void Render (string html_code) 
	{
		Gtk.HTMLStream stream = html_panel.Begin ("text/html");
		stream.Write(html_code);
		html_panel.End (stream, HTMLStreamStatus.Ok);
	}

	static Stream GetBrowserResourceImage (string name)
	{
		Assembly assembly = typeof (RootTree).Assembly;
		System.IO.Stream s = assembly.GetManifestResourceStream (name);
		
		return s;
	}

	protected void UrlRequested (object sender, UrlRequestedArgs args)
	{
		Stream s = help_tree.GetImage (args.Url);
		
		if (s == null)
			s = GetBrowserResourceImage ("monodoc.png");
		byte [] buffer = new byte [8192];
		int n, m;
		m=0;
		while ((n = s.Read (buffer, 0, 8192)) != 0) {
			args.Handle.Write (buffer, n);
			m += n;
		}
		args.Handle.Close (HTMLStreamStatus.Ok);
	}
	
	public void Print (string Html) {
#if !MACOS
		if (Html == null) {
			Console.WriteLine ("empty print");
			return;
		}

		PrintManager.Print (Html);
#endif
	}

	public bool Initialize ()
	{
		try {
			html_panel = new HTML ();
		}
		catch (Exception ex) {
			Console.WriteLine (ex.Message);
			Console.WriteLine (ex.StackTrace);
			return false;
		}
		html_panel.Show ();
		html_panel.LinkClicked += new LinkClickedHandler (LinkClicked);
		html_panel.OnUrl += new OnUrlHandler (OnUrlMouseOver);
		html_panel.UrlRequested += new UrlRequestedHandler (UrlRequested);
		return true;

	}

	public Capabilities Capabilities
	{
		get { return Capabilities.None; }
	}

	public string Name
	{
		get { return "GtkHtml"; }
	}

}
}
