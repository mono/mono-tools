using Gtk;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Xml;

using Mono.Options;

#if MACOS
using OSXIntegration.Framework;
#endif

namespace Monodoc {
//
// A Tab is a Notebok with two pages, one for editing and one for visualizing
//
public class Tab : Notebook {
	// Where we render the contents
	public IHtmlRender html;
	
	public History history;
	private Browser browser;
	private Label titleLabel;
	public HBox TabLabel;
	
	public string Title {
		get { return titleLabel.Text; }
		set { titleLabel.Text = value; }
	}
	
	public Node CurrentNode;
	
	void FocusOut (object sender, FocusOutEventArgs args)
	{	
	}


	private static IHtmlRender LoadRenderer (string dll, Browser browser) {
		if (!System.IO.File.Exists (dll))
			return null;
		
		try {
			Assembly ass = Assembly.LoadFile (dll);		
			System.Type type = ass.GetType ("Monodoc." + ass.GetName ().Name, false, false);
			if (type == null)
				return null;
			return (IHtmlRender) Activator.CreateInstance (type, new object[1] { browser.help_tree });
		} catch (Exception ex) {
			Console.Error.WriteLine (ex);
		}
		return null;
	}
	
	public static IHtmlRender GetRenderer (string engine, Browser browser)
	{
		IHtmlRender renderer = LoadRenderer (System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, engine + "HtmlRender.dll"), browser);
		if (renderer != null) {
			try {
				if (renderer.Initialize ()) {
					Console.WriteLine ("using " + renderer.Name);
					return renderer;
				}
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
			}
		}
		
		foreach (string backend in Driver.engines) {
			if (backend != engine) {
				renderer = LoadRenderer (System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, backend + "HtmlRender.dll"), browser);
				if (renderer != null) {
					try {
						if (renderer.Initialize ()) {
							Console.WriteLine ("using " + renderer.Name);
							return renderer;
						}
					} catch (Exception ex) {
						Console.Error.WriteLine (ex);
					}
				}			
			}
		}
		
		return null;		
	}

	public Tab(Browser br) 
	{
		browser = br;
		CurrentNode = br.help_tree;
		ShowTabs = false;
		ShowBorder = false;
		TabBorder = 0;
		TabHborder = 0;
		history = new History (browser.back_button, browser.forward_button);
		
		//
		// First Page
		//
		ScrolledWindow html_container = new ScrolledWindow();
		html_container.Show();
		
		//
		// Setup the HTML rendering and preview area
		//

		html = GetRenderer (browser.engine, browser);
		if (html == null)
			throw new Exception ("Couldn't find html renderer!");

		browser.capabilities = html.Capabilities;

		HelpSource.FullHtml = false;
		HelpSource.UseWebdocCache = true;
		if ((html.Capabilities & Capabilities.Css) != 0)
			HelpSource.use_css = true;

		//Prepare Font for css (TODO: use GConf?)
		if ((html.Capabilities & Capabilities.Fonts) != 0 && SettingsHandler.Settings.preferred_font_size == 0) { 
			Pango.FontDescription font_desc = Pango.FontDescription.FromString ("Sans 12");
			SettingsHandler.Settings.preferred_font_family = font_desc.Family;
			SettingsHandler.Settings.preferred_font_size = 100; //size: 100%
		}
		
		html_container.Add (html.HtmlPanel);
		html.UrlClicked += new EventHandler (browser.LinkClicked);
		html.OnUrl += new EventHandler (browser.OnUrlMouseOver);
		browser.context_id = browser.statusbar.GetContextId ("");
		
		AppendPage(html_container, new Label("Html"));
		
		//
		//Create the Label for the Tab
		//
		TabLabel = new HBox(false, 2);
		
		titleLabel = new Label("");
		
		//Close Tab button
		Button tabClose = new Button();
		Image img = new Image(Stock.Close, IconSize.SmallToolbar);
		tabClose.Add(img);
		tabClose.Relief = Gtk.ReliefStyle.None;
		tabClose.SetSizeRequest (18, 18);
		tabClose.Clicked += new EventHandler (browser.OnCloseTab);

		TabLabel.PackStart (titleLabel, true, true, 0);
		TabLabel.PackStart (tabClose, false, false, 2);
		
		// needed, otherwise even calling show_all on the notebook won't
		// make the hbox contents appear.
		TabLabel.ShowAll();
	
	}

	public static string GetNiceUrl (Node node) {
		if (node.Element.StartsWith("N:"))
			return node.Element;
		string name, full;
		int bk_pos = node.Caption.IndexOf (' ');
		// node from an overview
		if (bk_pos != -1) {
			name = node.Caption.Substring (0, bk_pos);
			full = node.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full;
		}
		// node that lists constructors, methods, fields, ...
		if ((node.Caption == "Constructors") || (node.Caption == "Fields") || (node.Caption == "Events") 
			|| (node.Caption == "Members") || (node.Caption == "Properties") || (node.Caption == "Methods")
			|| (node.Caption == "Operators")) {
			bk_pos = node.Parent.Caption.IndexOf (' ');
			name = node.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full + "/" + node.Element; 
		}
		int pr_pos = node.Caption.IndexOf ('(');
		// node from a constructor
		if (node.Parent.Element == "C") {
			name = node.Parent.Parent.Parent.Caption;
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + name + "." + node.Caption.Replace ('.', '+');
		// node from a method with one signature, field, property, operator
		} else if (pr_pos == -1) {
			bk_pos = node.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + full + "." + node.Caption;
		// node from a method with several signatures
		} else {
			bk_pos = node.Parent.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + full + "." + node.Caption;
		}
	}
}
}
