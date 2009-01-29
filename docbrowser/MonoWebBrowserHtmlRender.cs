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
using System.IO;
using System.Text;
using System.Collections;

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

		// Variable that handles the info encessary for the events
		// As every implementation of HtmlRender will have differents events
		// we try to homogenize them with the variabel
		string url;
		public string Url { 
			get {return url; }
		}

		public Widget HtmlPanel { 
			get { return (Widget)html_panel; } 
		}

		public void Print (string Html) 
		{
			html_panel.browser.ExecuteScript ("print();");
			return;
		}

		LoadFinishedEventHandler loadEvent;
		public bool Initialize ()
		{
			html_panel = new BrowserWidget ();
			html_panel.Realized += delegate (object sender, EventArgs e) {
				html_panel.browser.NavigationRequested += delegate (object sender1, NavigationRequestedEventArgs e1) {

					url = CheckUrl (e1.Uri);
					// if the file is cached on disk, return
					if (url.StartsWith ("file:///") || url.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase))
						return;

					if (UrlClicked != null)
						UrlClicked (this, new EventArgs());
					e1.Cancel = true;
				};
				html_panel.browser.StatusChanged += delegate (object sender1, StatusChangedEventArgs e1) {
					url = e1.Message;
					if (OnUrl != null)
						OnUrl (this, new EventArgs ());
				};
			};
			cache_imgs = new Hashtable();
			tmpPath = Path.Combine (Path.GetTempPath (), "monodoc");
			return html_panel.browser.Initialized;
		}

		// URL like T:System.Activator are lower cased by gecko to t.;System.Activator
		// so we revert that
		string CheckUrl (string u)
		{
			if (u.IndexOf (':') == 1)
				return Char.ToUpper (u[0]) + u.Substring (1);
			return u;
		}

		static int tmp_file = 0;
		string tmpPath;
		Hashtable cache_imgs;

		public void Render (string html_code)
		{
			string r = ProcessImages (html_code);
			// if the html code is too big, write it down to a tmp file
			if (((uint) r.Length) > 50000) {
				string filename = (tmp_file++) + ".html";
				string filepath = Path.Combine (tmpPath, filename);
				using (FileStream file = new FileStream (filepath, FileMode.Create)) {
					StreamWriter sw = new StreamWriter (file);
					sw.Write (r);
					sw.Close ();
				}
				html_panel.browser.Navigation.Go (filepath);
			} else {
				html_panel.browser.Render (r);
			}

		}

		// Substitute the src of the images with the appropriate path
		string ProcessImages (string html_code)
		{
			//If there are no Images return fast
			int pos = html_code.IndexOf ("<img", 0, html_code.Length);
			if (pos == -1)
				return html_code;

			StringBuilder html = new StringBuilder ();
			html.Append (html_code.Substring (0, pos));
			int srcIni, srcEnd;
			string Img;
			Stream s;
			string path, img_name;

			while (pos != -1) {

				//look for the src of the img
				srcIni = html_code.IndexOf ("src=\"", pos);
				srcEnd = html_code.IndexOf ("\"", srcIni+6);
				Img = html_code.Substring (srcIni+5, srcEnd-srcIni-5);

				path = "NO_IMG";
				//is the img cached?
				if (cache_imgs.Contains(Img)) {
					path = (string) cache_imgs[Img];
				} else {
					//obtain the stream from the compressed sources
					s = help_tree.GetImage (Img);
					if (s == null) {
						s = help_tree.GetImage (Img.Substring (Img.LastIndexOf ("/") + 1));
					}
					if (s != null) {
						//write the file to a tmp directory
						img_name = Img.Substring (Img.LastIndexOf (":")+1);
						path = Path.Combine (tmpPath, img_name);
						Directory.CreateDirectory (Path.GetDirectoryName (path));
						FileStream file = new FileStream (path, FileMode.Create);
						byte[] buffer = new byte [8192];
						int n;

						while ((n = s.Read (buffer, 0, 8192)) != 0)
							file.Write (buffer, 0, n);
						file.Flush();
						file.Close();
						//Add the image to the cache
						cache_imgs[Img] = path;
					}
				}
				//Add the html code from <img until src="
				html.Append (html_code.Substring (pos, srcIni + 5 - pos));
				//Add the Image path
				html.Append (path);
				//Look for the next image
				pos = html_code.IndexOf ("<img", srcIni);

				if (pos == -1)
					//Add the rest of the file
					html.Append (html_code.Substring (srcEnd));
				else
					//Add from " to the next <img
					html.Append (html_code.Substring (srcEnd, pos - srcEnd)); //check this
			}

			foreach (string cached in cache_imgs.Keys) {
				html.Replace ("\"" + cached + "\"", "\"" + (string)cache_imgs[cached] + "\"");
			}

			return html.ToString();
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
