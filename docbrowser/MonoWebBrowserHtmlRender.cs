//Permission is hereby granted, free of charge, to any person obtaining
//a copy of this software and associated documentation files (the
//"Software"), to deal in the Software without restriction, including
//without limitation the rights to use, copy, modify, merge, publish,
//distribute, sublicense, and/or sell copies of the Software, and to
//permit persons to whom the Software is furnished to do so, subject to
//the following conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//Copyright (c) 2008 Novell, Inc.
//
//Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using Mono.WebBrowser;
using Gtk;

namespace Monodoc
{
	public class MonoWebBrowserHtmlRender : IHtmlRender
	{
		BrowserWidget html_panel;
		RootTree help_tree;
		
		public MonoWebBrowserHtmlRender (RootTree help_tree)
		{
			
			
			this.help_tree = help_tree;
			
		}
		
		public void OnRealized (object sender, EventArgs e)
		{
		
		}
		
		public void OnExposed (object sender, ExposeEventArgs e) 
		{
		}
		
		public event EventHandler OnUrl;
		public event EventHandler UrlClicked;
		
		// Jump to an anchor of the form <a name="tttt">
		public void JumpToAnchor (string anchor_name) 
		{
		}

		//Copy to the clipboard the selcted text
		public void Copy () 
		{
		}

		//Select all the text
		public void SelectAll () 
		{
		}

		//Render the HTML code given
		public void Render (string html_code) 
		{
			html_panel.browser.Render (html_code);
		}


		// Variable that handles the info encessary for the events
		// As every implementation of HtmlRender will have differents events
		// we try to homogenize them with the variabel
		public string Url { 
			get {return html_panel.browser.Document.Url; } 
		}

		public Widget HtmlPanel { 
			get { return (Widget)html_panel; } 
		}

		public void Print (string Html) 
		{
		}

		public bool Initialize ()
		{
			html_panel = new BrowserWidget ();
			return html_panel.browser.Initialized;
		}

		public Capabilities Capabilities
		{
			get { return Capabilities.Css | Capabilities.Fonts; }
		}

		public string Name
		{
			get { return "MonoWebBrowser"; }
		}

	}
}
