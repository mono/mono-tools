//
// admin.cs: Mono collaborative documentation adminsitration tool.
//
// Author:
//   Miguel de Icaza
//
// (C) 2003 Novell, Inc.
//
using Gtk;
using GtkSharp;
using Glade;
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Monodoc {
class AdminDriver {
	static int Main (string [] args)
	{
		bool real_server = true;
		
		if (Environment.GetEnvironmentVariable ("MONODOCTESTING") != null)
			real_server = false;
		
		Settings.RunningGUI = true;
		Application.Init ();
		Admin admin = new Admin (real_server);
		Application.Run ();
		return 0;
	}
}



class Admin {
	Glade.XML ui;
	[Widget] Window main_window;
	[Widget] ScrolledWindow container, review_container;
	[Widget] Statusbar statusbar;
	[Widget] Notebook notebook;
	[Widget] TextView text_ondisk;
	[Widget] TextView text_diff;
	[Widget] TextView text_current;
	uint contextid;
	
	HTML html, html_review;
	
	ContributionsSoap d;
	string login = SettingsHandler.Settings.Email;
	string pass = SettingsHandler.Settings.Key;
	PendingChange [] changes;
	
	public Admin (bool real_server)
	{
		LoadProviders ();
		ui = new Glade.XML (null, "admin.glade", "main_window", null);
		ui.Autoconnect (this);
		contextid = statusbar.GetContextId ("");
		
		main_window.DeleteEvent += new DeleteEventHandler (OnDeleteEvent);
		d = new ContributionsSoap ();
		if (real_server)
			d.Url = "http://www.go-mono.com/docs/server.asmx";
		
		html = new HTML ();
                html.LinkClicked += new LinkClickedHandler (LinkClicked);
		html.Show ();
		html.SetSizeRequest (700, 500);
		container.Add (html);

		html_review = new HTML ();
		html_review.LinkClicked += new LinkClickedHandler (ReviewLinkClicked);
		html_review.Show ();
		review_container.Add (html_review);
	}

	bool Decouple (string prefix, string url, out int id, out int serial)
	{
		if (!url.StartsWith (prefix)){
			id = 0;
			serial = 0;
			return false;
		}

		string rest = url.Substring (prefix.Length);
		int p = rest.IndexOf ('$');
		string sid = rest.Substring (0, p);
		string sserial = rest.Substring (p+1);

		id = int.Parse (sid);
		serial = int.Parse (sserial);
		
		return true;
	}
	
        void LinkClicked (object o, LinkClickedArgs args)
	{
		string url = args.Url;
		int id, serial;

		Console.WriteLine ("Got: " + url);
		if (Decouple ("review:", url, out id, out serial)){
			RenderReview (id, serial);
			return;
		}

		Console.WriteLine ("Unhandled url: " + url);
	}

	class FileAction {
		public GlobalChangeset globalset;
		public DocSetChangeset docset;
		public FileChangeset fileset;
		
		public FileAction (GlobalChangeset gs, DocSetChangeset ds, FileChangeset fs)
		{
			globalset = gs;
			docset = ds;
			fileset = fs;
		}
	}

	class ItemAction {
		public GlobalChangeset globalset;
		public DocSetChangeset docset;
		public FileChangeset fileset;
		public Change change;
		
		public ItemAction (GlobalChangeset gs, DocSetChangeset ds, FileChangeset fs, Change c)
		{
			globalset = gs;
			docset = ds;
			fileset = fs;
			change = c;
		}
	}
	
	void ApplyFile (FileAction fa)
	{
		XmlDocument d = LoadDocument (fa.docset, fa.fileset.RealFile);

		foreach (Change c in fa.fileset.Changes){
			XmlNode old = d.SelectSingleNode (c.XPath);
			if (old != null)
				old.ParentNode.ReplaceChild (d.ImportNode (c.NewNode, true), old);
			
		}
		SaveDocument (d, fa.docset, fa.fileset);
	}

	void ApplyItem (ItemAction fa)
	{
		XmlDocument d = LoadDocument (fa.docset, fa.fileset.RealFile);

		XmlNode old = d.SelectSingleNode (fa.change.XPath);
		if (old != null)
			old.ParentNode.ReplaceChild (d.ImportNode (fa.change.NewNode, true), old);

		SaveDocument (d, fa.docset, fa.fileset);
	}

	static void WriteNode (string file, string str)
	{
		using (FileStream s = File.Create (file)){
			using (StreamWriter sw = new StreamWriter (s)){
				sw.Write (str);
			}
		}
	}
	
	void DiffChangeItem (ItemAction fa)
	{
		XmlDocument d = LoadDocument (fa.docset, fa.fileset.RealFile);

		XmlNode orig = d.SelectSingleNode (fa.change.XPath);
		XmlNode newn = fa.change.NewNode;

		text_ondisk.Buffer.Text = orig.InnerXml;
		text_current.Buffer.Text = newn.InnerXml;

		WriteNode ("/tmp/file-1", orig.InnerXml);
		WriteNode ("/tmp/file-2", newn.InnerXml);
		
		Process diffp = new Process ();
		diffp.StartInfo.FileName = "diff";
		diffp.StartInfo.Arguments = "-uw /tmp/file-1 /tmp/file-2";
		diffp.StartInfo.UseShellExecute = false;
		diffp.StartInfo.RedirectStandardOutput = true;
		diffp.Start ();
		
		text_diff.Buffer.Text = "=" + diffp.StandardOutput.ReadToEnd ();
		diffp.WaitForExit ();
	}
	
	void SaveDocument (XmlDocument d, DocSetChangeset docset, FileChangeset fileset)
	{
		string basedir = (string) providers [docset.DocSet];
		string file = basedir + "/" + fileset.RealFile;
		
		d.Save (file);
		RenderReview (current_id, current_serial);
	}
	
	void ReviewLinkClicked (object o, LinkClickedArgs args)
	{
		string url = args.Url;
		int id, serial;

		if (Decouple ("flag-done:", url, out id, out serial)){
			d.UpdateStatus (login, pass, id, serial, 1);
			notebook.Page = 0;
			return;
		}

		if (url.StartsWith ("apply-file:")){
			string rest = url.Substring (11);
			
			ApplyFile ((FileAction) action_map [Int32.Parse (rest)]);
			return;
		}

		if (url.StartsWith ("apply-change:")){
			string rest = url.Substring (13);
			
			ApplyItem ((ItemAction) action_map [Int32.Parse (rest)]);
			return;
		}

		if (url.StartsWith ("diff-change:")){
			string rest = url.Substring (12);
			DiffChangeItem ((ItemAction) action_map [Int32.Parse (rest)]);
			
			notebook.Page = 2;
		}
		
		Console.WriteLine ("Unhandled url: " + url);
	}
	
	Hashtable cache = new Hashtable ();
	
	GlobalChangeset LoadReview (int id, int serial)
	{
		string key = String.Format ("{0}:{1}", id, serial);
		if (cache [key] != null)
			return (GlobalChangeset) cache [key];

		//
		// Download contribution
		//
		XmlNode n = d.FetchContribution (login, pass, id, serial);

		//
		// Parse into GlobalChangeset
		//
		XmlDocument doc = new XmlDocument ();
		doc.AppendChild (doc.ImportNode (n, true));
		XmlNodeReader r = new XmlNodeReader (doc);
		GlobalChangeset s;
		try {
			s = (GlobalChangeset) GlobalChangeset.serializer.Deserialize (r);
		} catch (Exception e) {
			Console.WriteLine ("Error: " + e);
			Status = "Invalid contribution obtained from server: " + key;
			return null;
		}
		
		cache [key] = s;
		return s;
	}

	Hashtable action_map;
	int current_id, current_serial;

	//
	// Renders the id/serial representation for review by the administrator.
	//
	void RenderReview (int id, int serial)
	{
		current_id = id;
		current_serial = serial;
		
		notebook.Page = 1;

		GlobalChangeset globalset;
		globalset = LoadReview (id, serial);

		HTMLStream s = html_review.Begin ("text/html");
		s.Write ("<html><body>");
		if (globalset == null){
			s.Write ("No data found");
			html_review.End (s, HTMLStreamStatus.Ok);
			return;
		}

		int key = 0;
		action_map = new Hashtable ();
		
		//
		// First make sure we dont have sources that we dont know about,
		// so a contribution can not be flagged as done by accident
		//
		bool allow_flag_as_done = true;
		foreach (DocSetChangeset docset in globalset.DocSetChangesets){
			if (!providers.Contains (docset.DocSet)){
				s.Write (String.Format ("<font color='red'>Warning: Skipping {0}</font>", docset.DocSet));
				allow_flag_as_done = false;
				continue;
			}
		}

		if (allow_flag_as_done)
			s.Write (String.Format ("<h1>Changes: <a href=\"flag-done:{0}${1}\">[Flag as Done]</a></h1>", id, serial));

		foreach (DocSetChangeset docset in globalset.DocSetChangesets){
			if (!providers.Contains (docset.DocSet))
				continue;
			
			if (docset == null){
				s.Write ("Null?");
				continue;
			}
			string ds;

			ds = String.Format ("<table width='100%' bgcolor='#aabbaa'><tr><td>Docset: {0}</td></tr></table>", docset.DocSet);
			foreach (FileChangeset fileset in docset.FileChangesets){
				string fs, es = null;

				fs = String.Format ("<h3><a href=\"apply-file:{0}\">[Apply]</a> File: {1} <br><blockquote>",
						    key, fileset.RealFile);
				
				action_map [key++] = new FileAction (globalset, docset, fileset);

				if (fileset.RealFile == null){
					s.Write (String.Format ("Warning: invalid contribution, its missing filename"));
					continue;
				}
				XmlDocument d = LoadDocument (docset, fileset.RealFile);
				
				foreach (Change c in fileset.Changes){
					XmlNode orig = d.SelectSingleNode (c.XPath);
					XmlNode newn = c.NewNode;

					if (orig == null){
						s.Write (String.Format ("Warning, node {0} does not exist", c.XPath));
						continue;
					}

					if (ds != null) { s.Write (ds); ds = null; }
					if (fs != null) { s.Write (fs); fs = null; es = "</blockquote>"; }

					string original_text = orig.InnerXml;
					string new_text = c.NewNode.InnerXml;

					if (original_text == new_text){
						//s.Write ("<b>Applied</b><br>");
						continue;
					}
					
					int p = c.XPath.LastIndexOf ("/");
					s.Write (String.Format ("<a href=\"diff-change:{0}\">[Diff]</a>", key));
					s.Write (String.Format ("<a href=\"apply-change:{0}\">[Apply]</a>: {1} ", key, c.XPath));
					if (c.FromVersion != RootTree.MonodocVersion)
						s.Write ("<b>FROM OLD VERSION</b>");
					action_map [key++] = new ItemAction (globalset, docset, fileset, c);
					s.Write ("<table border=1 width=100%><tr bgcolor=grey><td width=50%>Current</td><td width=50%>New</td>");

					s.Write ("<tr>");
					s.Write (String.Format ("<td>{0}</td>", Htmlize (original_text)));
					
					s.Write ("<td>");
					s.Write (Htmlize (new_text));
					s.Write ("</td></tr>");
					s.Write ("</table>");
				}
				if (es != null) s.Write (es);
			}
		}
		s.Write ("</body></html>");
		html_review.End (s, HTMLStreamStatus.Ok);
	}

	//
	// Colorizes the ECMA XML documentation into some pretty HTML
	//
	string Htmlize (string s)
	{
		string r = s.Replace ("<", "&lt;").Replace (">", "&gt;</font>").Replace ("&lt;", "<font color='blue'>&lt;").Replace
			("/para&gt;</font>", "para&gt;</font><p>");
		return r;
	}

	//
	// Loads the `file' from a DocSetChangeset
	//
	XmlDocument LoadDocument (DocSetChangeset ds, string file)
	{
		XmlDocument d = new XmlDocument ();
		string basedir = (string) providers [ds.DocSet];
		d.Load (basedir + "/" + file);
		return d;
	}

	void OnDeleteEvent (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}

	void RenderChanges ()
	{
		notebook.Page = 0;
		HTMLStream s = html.Begin ("text/html");
		if (changes != null){
			s.Write ("<h1>Pending Changes</h1>");
			int i = 0;
			foreach (PendingChange ch in changes){
				s.Write (String.Format ("{3}: <a href=\"review:{0}${1}\">{2}</a><br>", ch.ID, ch.Serial, ch.Login, i++));
			}
		} else {
			s.Write ("<h1>No pending changes on the server</h1>");
		}
		html.End (s, HTMLStreamStatus.Ok);
	}
	
	void OnCheckUpdatesClicked (object o, EventArgs args)
	{
		Status = "Loading";
		try {
			changes = d.GetPendingChanges (login, pass);
			if (changes == null)
				Status = "No changes available";
			else
				Status = "Changes loaded: " + changes.Length + " contributions";
			RenderChanges ();
		} catch (Exception e){
			Status = "There was a failure trying to fetch the status from the server";
			Console.WriteLine (e);
		}
	}

	string Status {
		set {
			statusbar.Pop (contextid);
			statusbar.Push (contextid, value);
		}
	}

	Hashtable providers = new Hashtable ();
	public void LoadProviders ()
	{
		string config_dir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
		string monodoc_dir = System.IO.Path.Combine (config_dir, "monodoc");
		string settings_file = System.IO.Path.Combine (monodoc_dir, "providers.xml");

		XmlSerializer ser = new XmlSerializer (typeof (Providers));
		Providers p;
		if (File.Exists (settings_file))
			p = (Providers) ser.Deserialize (new XmlTextReader (settings_file));
		else {
			Console.WriteLine ("File {0} does not exist", settings_file);
			Console.WriteLine ("Format is:");
			Console.WriteLine (@"<Providers xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Location Name=""netdocs"" Path=""/cvs/monodoc/class""/>
</Providers>
");

			
			Environment.Exit (1);
			return;
		}

		for (int i = 0; i < p.Locations.Length; i++){
			providers [p.Locations [i].Name] = p.Locations [i].Path;
		}
	}
}

///
/// Configuration Loading
///
public class ProviderLocation {
	[XmlAttribute] public string Name;
	[XmlAttribute] public string Path;
}

public class Providers {
	[XmlElement ("Location", typeof (ProviderLocation))]
	public ProviderLocation [] Locations;
}

}
