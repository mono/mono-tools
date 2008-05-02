//
// GeckoHtmlRender.cs: Implementation of IHtmlRender that uses Gecko
//
// Author: Mario Sopena
// Author:	Rafael Ferreira <raf@ophion.org>
//
using System;
using System.Text;
using System.IO;
using System.Collections;
using Gecko;
using Gtk;
#if USE_GTKHTML_PRINT
using Gnome;
#endif

namespace Monodoc {
public class GeckoHtmlRender : IHtmlRender {
	
	//Images are cached in a temporal directory
	Hashtable cache_imgs;
	string tmpPath;

	WebControl html_panel;
	Viewport panel;
	public Widget HtmlPanel {
		get { return (Widget) panel; }
	}

	string url;
	public string Url {
		get { return url; }
	}
	RootTree help_tree;

	public event EventHandler OnUrl;
	public event EventHandler UrlClicked;

	public GeckoHtmlRender (RootTree help_tree) 
	{
		this.help_tree = help_tree;
	}

	public bool Initialize ()
	{
		tmpPath = Path.Combine (Path.GetTempPath (), "monodoc");
		try {
			string mozHome = System.Environment.GetEnvironmentVariable ("MOZILLA_HOME");
			if (mozHome != null)
				WebControl.CompPath = mozHome;
			html_panel = new WebControl (tmpPath, "MonodocGecko");
		}
		catch (Exception ex) {
			Console.WriteLine (ex.Message);
			Console.WriteLine (ex.StackTrace);
			return false;
		}

		html_panel.Show(); //due to Gecko bug
		html_panel.OpenUri += OnOpenUri;
		html_panel.LinkMsg += OnLinkMsg;
		panel = new Viewport();
		panel.Add (html_panel);
		cache_imgs = new Hashtable();
		return true;
	}

	public Capabilities Capabilities
	{
		get { return Capabilities.Css | Capabilities.Fonts; }
	}

	public string Name
	{
		get { return "Gecko"; }
	}


	protected void OnOpenUri (object o, OpenUriArgs args)
	{
		url = CheckUrl (args.AURI);
		// if the file is cached on disk, return
		if (url.StartsWith ("file:///") || url.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase)) 
			return;
		
		if (UrlClicked != null)
			UrlClicked (this, new EventArgs());
		args.RetVal = true; //this prevents Gecko to continue processing
	}

	protected void OnLinkMsg (object o, EventArgs args)
	{
		url = CheckUrl (html_panel.LinkMessage);
		if (OnUrl != null)
			OnUrl (this, args);
	}
	
	// URL like T:System.Activator are lower cased by gecko to t.;System.Activator
	// so we revert that
	string CheckUrl (string u)
	{
		if (u.IndexOf (':') == 1)
			return Char.ToUpper (u[0]) + u.Substring (1);
		return u;
	}
		
	/* NOT ALREADY IMPLEMENTED */
	public void JumpToAnchor (string anchor_name) 
	{
	}

	/* NOT ALREADY IMPLEMENTED */
	public void Copy() {}

	/* NOT ALREADY IMPLEMENTED */
	public void SelectAll() {}

	static int tmp_file = 0;
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
			html_panel.LoadUrl (filepath);
		} else {
			html_panel.OpenStream ("file:///", "text/html");
			html_panel.AppendData (r);
			html_panel.CloseStream ();
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
					System.Console.WriteLine("Cache: {0}", path);
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

	public void Print (string Html) {
		
		if (Html == null) {
			Console.WriteLine ("empty print");
			return;
		}
		Console.WriteLine ("XXXX");
		
#if USE_GTKHTML_PRINT
		try {
			PrintManager.Print (Html);
		} catch {
			MessageDialog md = new MessageDialog (null, 
					DialogFlags.DestroyWithParent,
					MessageType.Error, 
					ButtonsType.Close, "Printing not supported without gtkhtml");
	     
			int result = md.Run ();
			md.Destroy();
		}
#else
		MessageDialog md = new MessageDialog (null, 
				DialogFlags.DestroyWithParent,
				MessageType.Error, 
				ButtonsType.Close, "Printing not supported without gtkhtml");
     
		int result = md.Run ();
		md.Destroy();
#endif
	}
}
}
