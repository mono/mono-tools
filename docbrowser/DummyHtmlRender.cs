//
// DummyHtmlRender.cs: Implementation of IHtmlRender that does nothing
//
// Author: Calvin Buckley <calvin@cmpct.info>
//

using System;
using System.IO;
using Gtk;

namespace Monodoc {
public class DummyHtmlRender : IHtmlRender {

	Label web_view;
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

	public DummyHtmlRender (RootTree help_tree) 
	{
		web_view = new Label();
	}

	public void JumpToAnchor (string anchor)
	{
	}

	public void Copy () 
	{
	}

	public void SelectAll () 
	{
	}

	public void Render (string html) 
	{
	}

	public void Print (string html)
	{
	}

	public bool Initialize ()
	{
		return true;
	}

	public Capabilities Capabilities
	{
		get { return Capabilities.None; }
	}

	public string Name
	{
		get { return "Dummy"; }
	}


}
}
