using System;
using Gecko;
using Gtk;
using System.Text;
using System.IO;
using System.Collections;

namespace Monodoc {
interface IHtmlRender {
	// Jump to an anchor of the form <a name="tttt">
	void JumpToAnchor (string anchor_name);

	//Copy to the clipboard the selcted text
	void Copy ();

	//Select all the text
	void SelectAll ();

	//Render the HTML code given
	void Render (string HtmlCode);

	//Event fired when the use is over an Url
	event EventHandler OnUrl;

	//Event fired when the user clicks on a Link
	event EventHandler UrlClicked;

	// Variable that handles the info encessary for the events
	// As every implementation of HtmlRender will have differents events
	// we try to homogenize them with the variabel
	string Url { get; }

	Widget HtmlPanel { get; }
}


class GeckoHtmlRender : IHtmlRender {
	
	Hashtable cache_imgs;
	string tmpPath;
	WebControl _HtmlPanel;
	Viewport panel;
	public Widget HtmlPanel {
		get { return (Widget) panel; }
	}

	string _url;
	public string Url {
		get { return _url; }
	}
	Browser browser;

	public event EventHandler OnUrl;
	public event EventHandler UrlClicked;

	public GeckoHtmlRender (Browser browser) 
	{
		this.browser = browser;
		_HtmlPanel = new WebControl("/tmp/monodoc", "MonodocGecko"); //FIXME
		_HtmlPanel.Show(); //due to Gecko bug
		_HtmlPanel.OpenUri += OnOpenUri;
		_HtmlPanel.LinkMsg += OnLinkMsg;
		panel = new Viewport();
		panel.Add (_HtmlPanel);
		cache_imgs = new Hashtable();
		tmpPath = Path.Combine (Path.GetTempPath(), "monodoc");
	}
	protected void OnOpenUri (object o, OpenUriArgs args)
	{
		_url = args.AURI;
		if (UrlClicked != null)
			UrlClicked (this, new EventArgs());
		args.RetVal = true; //this prevents Gecko to continue processing
	}
	protected void OnLinkMsg (object o, EventArgs args)
	{
		_url = _HtmlPanel.LinkMessage;
		if (OnUrl != null)
			OnUrl (this, args);
	}
		
	/* NOT ALREADY IMPLEMENTED */
	public void JumpToAnchor (string anchor_name) 
	{
	}

	/* NOT ALREADY IMPLEMENTED */
	public void Copy() {}

	/* NOT ALREADY IMPLEMENTED */
	public void SelectAll() {}

	public void Render (string HtmlCode) 
	{
		string r = ProcessImages(HtmlCode);
		_HtmlPanel.OpenStream("file:///", "text/html");
		_HtmlPanel.AppendData(r);
		_HtmlPanel.CloseStream();
	}

	// Substitute the src of the images with the appropriate path
	string ProcessImages(string HtmlCode)
	{
		//If there are no Images return fast
		int pos = HtmlCode.IndexOf ("<img", 0, HtmlCode.Length);
		if (pos == -1)
			return HtmlCode;

		StringBuilder html = new StringBuilder ();
		html.Append (HtmlCode.Substring (0, pos)); 
		int srcIni, srcEnd;
		string Img;
		Stream s;
		string path, img_name;

		while (pos != -1) {

			//look for the src of the img
		 	srcIni = HtmlCode.IndexOf ("src=\"", pos);
		 	srcEnd = HtmlCode.IndexOf ("\"", srcIni+6);
			Img = HtmlCode.Substring (srcIni+5, srcEnd-srcIni-5);

			path = "NO_IMG";
			//is the img cached?
			if (cache_imgs.Contains(Img)) {
				path = (string) cache_imgs[Img];
			} else {
				//obtain the stream from the compressed sources
				s = browser.help_tree.GetImage (Img);
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
			html.Append (HtmlCode.Substring (pos, srcIni + 5 - pos));
			//Add the Image path
			html.Append (path);		
			//Look for the next image
			pos = HtmlCode.IndexOf ("<img", srcIni);

			if (pos == -1)  
				//Add the rest of the file
				html.Append (HtmlCode.Substring (srcEnd));
			else 
				//Add from " to the next <img
				html.Append (HtmlCode.Substring (srcEnd, pos - srcEnd)); //check this
		}
		return html.ToString();
	}

}



class GtkHtmlHtmlRender : IHtmlRender {
	
	HTML _HtmlPanel;
	public Widget HtmlPanel {
		get { 
			return (Widget) _HtmlPanel; }
	}

	string _url;
	public string Url {
		get { return _url; }
	}
	Browser browser;
	
	public event EventHandler OnUrl;
	public event EventHandler UrlClicked;

	
	public GtkHtmlHtmlRender (Browser browser) 
	{
		_HtmlPanel = new HTML();
		_HtmlPanel.Show(); 
		_HtmlPanel.LinkClicked += new LinkClickedHandler (LinkClicked);
		_HtmlPanel.OnUrl += new OnUrlHandler (OnUrlMouseOver);
		_HtmlPanel.UrlRequested += new UrlRequestedHandler (UrlRequested);
		this.browser = browser;
	}
	protected void LinkClicked (object o, LinkClickedArgs args)
	{
		_url = args.Url;
		if (UrlClicked != null)
			UrlClicked (this, new EventArgs());
	}
	protected void OnUrlMouseOver (object o, OnUrlArgs args)
	{
		_url = args.Url;
		if (OnUrl != null)
			OnUrl (this, args);
	}
	public void JumpToAnchor (string anchor)
	{
		_HtmlPanel.JumpToAnchor(anchor);
	}

	public void Copy () 
	{
		_HtmlPanel.Copy();	
	}

	public void SelectAll () 
	{
		_HtmlPanel.SelectAll();	
	}

	public void Render (string HtmlCode) 
	{

		Gtk.HTMLStream stream = _HtmlPanel.Begin ("text/html");
		stream.Write(HtmlCode);
		_HtmlPanel.End (stream, HTMLStreamStatus.Ok);
	}

	protected void UrlRequested (object sender, UrlRequestedArgs args)
	{
		Stream s = browser.help_tree.GetImage (args.Url);
		
		if (s == null)
			s = browser.GetResourceImage ("monodoc.png");
		byte [] buffer = new byte [8192];
		int n, m;
		m=0;
		while ((n = s.Read (buffer, 0, 8192)) != 0) {
			args.Handle.Write (buffer, n);
			m += n;
		}
		args.Handle.Close (HTMLStreamStatus.Ok);
	}
	
}
}
