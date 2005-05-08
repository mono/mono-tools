//
// GnomeMain.cs: Main GUI file for GNOME
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
//

// fx
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

// gtk#
using Glade;
using Gnome;
using Gtk;
using Gdk;
using Pango;

// project internal
using Mono.Security;
using Mono.Tools;

[assembly: AssemblyTitle ("ASNView")]
[assembly: AssemblyDescription ("ASN.1 Viewer for GNOME.")]

public class GASNViewerApp {

	private static readonly string cache;
	private static readonly string config;
//	private static Program program;
	private static AssemblyInfo _info;
	private static TextSearchFlags tsf = TextSearchFlags.TextOnly | TextSearchFlags.VisibleOnly;

	// Application
	[Widget] private App gasnview;
	[Widget] private AppBar appbar1;
	[Widget] private TextView textview1;

	// Menu / Toolbar support
	[Widget] private ImageMenuItem file_export;
	[Widget] private Gtk.Image fileexportimage;
	[Widget] private CheckMenuItem view_position;
	[Widget] private CheckMenuItem view_tag;
	[Widget] private CheckMenuItem view_dotted_indentation;
	[Widget] private CheckMenuItem view_length;
	[Widget] private RadioMenuItem view_oid_format_ietf;
	[Widget] private RadioMenuItem view_oid_format_itu;
	[Widget] private RadioMenuItem view_oid_format_urn;
	[Widget] private CheckMenuItem view_display_class;
	[Widget] private CheckMenuItem view_encapsulated;
	[Widget] private RadioMenuItem settings_oid_alvestrand;
	[Widget] private RadioMenuItem settings_oid_elibel;
	[Widget] private RadioMenuItem settings_oid_none;
	
	// Find bar support
	[Widget] private HBox findbar;
	[Widget] private Label findlabel;
	[Widget] private Gtk.Entry findentry;
	[Widget] private Button findnextbutton;
	[Widget] private Button findhighlightbutton;
	[Widget] private Button findpreviousbutton;
	[Widget] private Gtk.Image highlightimage;
	[Widget] private Gtk.Image findimage;
	private TextIter findbck;
	private int findstart;
	private int findend;
	private Gdk.Color finderrorbasecolor;
	private Gdk.Color finderrortextcolor;
	private TextIter findfwd;
	private Gdk.Color findnormalbasecolor;
	private Gdk.Color findnormaltextcolor;
	private bool findhighlight;
	private TextTag highlight;
	private Pixbuf highlight_off;
	private Pixbuf highlight_on;

	// Editor
	private FontDescription fontdesc;
	private PrettyPrinterOptions options;
	private PrettyPrinter pp;
	private TextTag encapsulated;

	// Application data
	private ASN1Element asn;
	private string currentfile;
	private string savename;

	static GASNViewerApp ()
	{
		string path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
			Path.Combine (".config", "asnview"));

		cache = Path.Combine (path, "oid.cache");
		config = Path.Combine (path, "config.xml");
		_info = new AssemblyInfo ();
	}

	public static void Main (string[] args)
	{
//		GASNViewerApp.program =
		new Program (_info.Title, _info.Version, Modules.UI, args);
		new GASNViewerApp (args);
	}

	public static AssemblyInfo AppInfo {
		get { return GASNViewerApp._info; }
	}

	public GASNViewerApp (string[] args)
	{
		Application.Init ();
		Glade.XML xml = new Glade.XML (null, "gui.glade", "gasnview", null);
		xml.Autoconnect (this);
		
		options = LoadConfig (config);
		UpdateOptions ();
		
		// load cache
		PrettyPrinter.Cache.Load (cache);
		
		// UI preparation
		fileexportimage.Pixbuf = new Pixbuf (null, "export.png");
		file_export.Image = new Gtk.Image (new Pixbuf (null, "export-16.png"));
		textview1.Editable = false;
		textview1.GrabFocus ();
		Font = options.FontName;
		findfwd = textview1.Buffer.StartIter;
		findbck = textview1.Buffer.EndIter;
		findstart = -1;
		findend = -1;
		highlight_on = new Pixbuf (null, "text_hilight-16.png");
		highlight_off = new Pixbuf (null, "text_lolight-16.png");
		findhighlight = false;
		findnormalbasecolor = findentry.Style.Base (StateType.Normal);
		try {
			// GTK# bug - entry point was missing in early 1.0.x versions
			findnormaltextcolor = findentry.Style.Text (StateType.Normal);
		}
		catch (EntryPointNotFoundException) {
			findnormaltextcolor = new Gdk.Color (0x00, 0x00, 0x00);
		}
		finderrorbasecolor = new Gdk.Color (0xff, 0x00, 0x00);
		finderrortextcolor = new Gdk.Color (0xff, 0xff, 0xff);

		highlight = new TextTag ("highlight");
		highlight.BackgroundGdk = new Gdk.Color (0xff, 0xff, 0x00);
		textview1.Buffer.TagTable.Add (highlight);
		encapsulated = new TextTag ("encapsulated");
		encapsulated.ForegroundGdk = new Gdk.Color (0x00, 0x00, 0xff);
		textview1.Buffer.TagTable.Add (encapsulated);
		Highlight (false);

		// load any specified file and execute application
		if (args.Length > 0) {
			FileLoad (args[0]);
		}
		Application.Run ();
	}
	
	// Application events
 
	public void OnWindowDeleteEvent (object o, DeleteEventArgs args)
	{
		OnFileQuit (null, null);
		args.RetVal = true;
	}

	public void OnCloseFindBarButtonClick (object sender, EventArgs args)
	{
		findbar.Visible = false;
	}

	// Data Management

	private byte[] Load (string filename)
	{
		byte[] buffer = null;
		using (FileStream fs = File.OpenRead (filename)) {
			buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			fs.Close ();
		}
		return buffer;
	}

	private ASN1Element Decode (byte[] data)
	{
		return new ASN1Element (data, 0);
	}

	private void FileExport (string filename)
	{
		try {
			filename = Path.GetFullPath(filename);
			if (filename == currentfile) {
				string msg = string.Format ("You are about to replace the original binary ASN.1 file with its textual representation.\nDo you want to overwrite original file {0} ?", filename);
				if (!AskUser (msg)) {
					return;
				}
			} else if (File.Exists(filename)) {
				string msg = string.Format("Do you want to overwrite file {0} ?", filename);
				if (!AskUser (msg)) {
					return;
				}
			}

			using (StreamWriter sw = new StreamWriter (filename, false, Encoding.UTF8)) {
				sw.Write (textview1.Buffer.Text);
				sw.Close ();
			}
			appbar1.ProgressPercentage = 1.0f;
			savename = filename;
		}
		catch (Exception e) {
			savename = null;
			appbar1.Default = "Error saving file " + filename;
			Console.Error.WriteLine (e);
		}
	}

	private void FileLoad (string filename)
	{
		try {
			byte[] buffer = Load (filename);
			appbar1.ProgressPercentage = 0.25f;
			asn = Decode (buffer);
			appbar1.ProgressPercentage = 0.75f;
			UpdateDisplay ();
			appbar1.ProgressPercentage = 1.0f;
			currentfile = Path.GetFullPath (filename);
			appbar1.Default = currentfile;
			findfwd = textview1.Buffer.StartIter;
			findbck = textview1.Buffer.EndIter;
		}
		catch (Exception e) {
			currentfile = null;
			asn = null;
			textview1.Buffer.Text = e.ToString ();
			appbar1.Default = "Error loading file " + filename;
			Console.Error.WriteLine (e);
		}
	}
	 
	// File Menu
	 
	public void OnFileNew (object sender, EventArgs args)
	{
		currentfile = null;
		savename = null;
		asn = null;
		pp = null;
		appbar1.Default = String.Empty;
		UpdateDisplay ();
		findfwd = textview1.Buffer.StartIter;
		findbck = textview1.Buffer.EndIter;
		findstart = -1;
		findend = -1;
		OnFindEntryChange (sender, args);
	}
	 
	public void OnFileOpen (object sender, EventArgs args)
	{
		appbar1.ProgressPercentage = 0.0f;
		FileSelection fs = new FileSelection ("Open");
		try {
			bool ok = (fs.Run () == (int)Gtk.ResponseType.Ok);
			fs.Hide ();
			if (ok) {
				OnFileNew (sender, args);
				FileLoad (fs.Filename);
				OnFindEntryChange (sender, args);
			}
		}
		finally {
			fs.Destroy ();
			appbar1.ProgressPercentage = 1.0f;
		}
	}

	public void OnFileExport (object sender, EventArgs args)
	{
		if (savename == null) {
			OnFileExportAs (sender, args);
		} else {
			FileExport (savename);
		}
	}

	public void OnFileExportAs(object sender, EventArgs args)
	{
		appbar1.ProgressPercentage = 0.0f;
		FileSelection fs = new FileSelection ("Export As");
		try {
			bool ok = (fs.Run () == (int)Gtk.ResponseType.Ok);
			fs.Hide ();
			if (ok) {
				FileExport (fs.Filename);
			}
		}
		finally {
			fs.Destroy ();
			appbar1.ProgressPercentage = 1.0f;
		}
	}

	public void OnFileRevert(object sender, EventArgs args)
	{
		if (currentfile != null) {
			FileLoad (currentfile);
		}
	}

	public void OnFileQuit (object sender, EventArgs args)
	{
		PrettyPrinter.Cache.Save (cache);
		SaveConfig (config, options);
		Application.Quit ();
	}

	// Edit Menu

	public void OnEditCopy (object sender, EventArgs args)
	{
		textview1.Buffer.CopyClipboard (Clipboard.Get (Gdk.Selection.Clipboard));
	}

	public void OnEditFind (object sender, EventArgs args)
	{
		findlabel.Text = String.Empty;
		OnFindEntryChange (sender, args);
		findbar.Visible = true;
		findentry.GrabFocus ();
	}
	 
	public void OnEditSelectAll (object sender, EventArgs args)
	{
		textview1.Buffer.MoveMark ("insert", textview1.Buffer.StartIter);
		textview1.Buffer.MoveMark ("selection_bound", textview1.Buffer.EndIter);
	}

	// View Menu

	public void OnViewPosition (object sender, EventArgs args)
	{
		options.ViewPosition = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}

	public void OnViewTag (object sender, EventArgs args)
	{
		options.ViewTag = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}
	 
	public void OnViewLength (object sender, EventArgs args)
	{
		options.ViewLength = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}

	public void OnViewDottedIndentation (object sender, EventArgs args)
	{
		options.DottedIndentation = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}
	 
	public void OnViewOidFormatItu (object sender, EventArgs args)
	{
		options.OidFormat = OidFormat.ITU;
		UpdateDisplay ();
	}

	public void OnViewOidFormatIetf (object sender, EventArgs args)
	{
		options.OidFormat = OidFormat.IETF;
		UpdateDisplay ();
	}

	public void OnViewOidFormatUrn (object sender, EventArgs args)
	{
		options.OidFormat = OidFormat.URN;
		UpdateDisplay ();
	}
	 
	public void OnViewDisplayClass (object sender, EventArgs args)
	{
		options.ShowTagClass = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}

	public void OnViewEncapsulated (object sender, EventArgs args)
	{
		options.IncludeEncapsulated = (sender as CheckMenuItem).Active;
		UpdateDisplay ();
	}

	// Settings Menu

	public void OnSettingsOidSourceNone (object sender, EventArgs args)
	{
		options.OidSource = OidSource.None;
	}
	 
	public void OnSettingsOidSourceAlvestrand (object sender, EventArgs args)
	{
		options.OidSource = OidSource.Alvestrand;
	}
	 
	public void OnSettingsOidSourceElibel (object sender, EventArgs args)
	{
		options.OidSource = OidSource.Elibel;
	}

	public void OnSettingsClearOidCache (object sender, EventArgs args)
	{
	      if (AskUser ("Are you sure you want to clear the OID cache ?\nN.b. They will be downloaded again as required from the selected site.")) {
	            PrettyPrinter.Cache.Clear ();
	      }
	}
 
	public void OnSettingsSelectFont (object sender, EventArgs args)
	{
		FontSelectionDialog fsd = new FontSelectionDialog ("Select Font");
		try {
			fsd.SetFontName (Font);
			if (fsd.Run () == (int)Gtk.ResponseType.Ok) {
				Font = fsd.FontName;
			}
		}
		finally {
			fsd.Destroy ();
		}
	}

	// Help Menu
	 
	public void OnHelpAbout(object sender, EventArgs args)
	{
		string[] authors = new string [1] { "Sebastien Pouliot  <sebastien@ximian.com>" } ;
		About about = new About (_info.Title, _info.Version, _info.Copyright, _info.Description, authors, new string [0], String.Empty, null);
		about.Run ();
	}

	// Find Bar
	 
	public void OnFindEntryChange (object sender, EventArgs args)
	{
		bool empty = (findentry.Text.Length == 0);
		bool found = (!empty && (textview1.Buffer.Text.IndexOf (findentry.Text) >= 0));
		if (!empty && !found) {
			findentry.ModifyBase (StateType.Normal, finderrorbasecolor);
			findentry.ModifyText (StateType.Normal, finderrortextcolor);
			findlabel.Text = "Text not found";
			findimage.Visible = true;
			findimage.Display.Beep ();
		} else {
			findentry.ModifyBase (StateType.Normal, findnormalbasecolor);
			findentry.ModifyText (StateType.Normal, findnormaltextcolor);
			findlabel.Text = String.Empty;
			findimage.Visible = false;
		}

		findnextbutton.Sensitive = found;
		findpreviousbutton.Sensitive = found;
		findhighlightbutton.Sensitive = found;

		if (found && findhighlight) {
			OnFindHighlightButtonClick (sender, args);
			OnFindHighlightButtonClick (sender, args);
		}
	}
	 
	public void OnFindHighlightButtonClick (object sender, EventArgs args)
	{
		appbar1.ClearStack ();
		findhighlight = !findhighlight;
		Highlight (findhighlight);
	}
	 
	public void OnFindNextButtonClick (object sender, EventArgs args)
	{
		if (findentry.Text.Length < 1) {
			return;
		}

		try {
			TextIter start;
			TextIter end;
			bool found = findfwd.ForwardSearch (findentry.Text, tsf, out start, out end, textview1.Buffer.EndIter);
			if (!found) {
				WarnWrapBuffer (true);
				findfwd = textview1.Buffer.StartIter;
				findfwd.ForwardSearch (findentry.Text, tsf, out start, out end, textview1.Buffer.EndIter);
			}
			UpdateSelection (found, start, end);
		}
		catch (Exception e) {
			// safety net
			Console.Error.WriteLine (e);
		}
	}
	 
	public void OnFindPreviousButtonClick (object sender, EventArgs args)
	{
		if (findentry.Text.Length < 1) {
			return;
		}

		try {
			TextIter start;
			TextIter end;
			bool found = findbck.BackwardSearch (findentry.Text, tsf, out start, out end, textview1.Buffer.StartIter);
			if (!found) {
				WarnWrapBuffer (false);
				findbck = textview1.Buffer.EndIter;
				findbck.BackwardSearch (findentry.Text, tsf, out start, out end, textview1.Buffer.StartIter);
			}
			UpdateSelection (found, start, end);
		}
		catch (Exception e) {
			// safety net
			Console.Error.WriteLine (e);
		}
	}

	private void Highlight (bool on)
	{
		if (on) {
			TextIter start;
			TextIter end;
			highlightimage.Pixbuf = highlight_on;
			TextIter current = textview1.Buffer.StartIter;
			while (current.ForwardSearch (findentry.Text, tsf, out start, out end, textview1.Buffer.EndIter)) {
				textview1.Buffer.ApplyTag (highlight, start, end);
				current = end;
			}
		} else {
			highlightimage.Pixbuf = highlight_off;
			textview1.Buffer.RemoveTag (highlight, textview1.Buffer.StartIter, textview1.Buffer.EndIter);
		}
	}

	private void UpdateSelection (bool result, TextIter start, TextIter end)
	{
		if (result) {
			findlabel.Text = String.Empty;
			findimage.Visible = false;
		}
		findbck = start;
		findfwd = end;
		findstart = start.Offset;
		findend = end.Offset;
		if ((findstart != -1) && (findend != -1)) {
			textview1.Buffer.MoveMark ("insert", textview1.Buffer.GetIterAtOffset (findend));
			textview1.Buffer.MoveMark ("selection_bound", textview1.Buffer.GetIterAtOffset (findstart));
			textview1.ScrollMarkOnscreen (textview1.Buffer.InsertMark);
		}
	}
	 
	private void UpdateDisplay ()
	{
		textview1.Buffer.Text = String.Empty;
		if (asn == null) {
			return;
		}
		if (pp == null) {
			pp = new PrettyPrinter(asn);
		}
		pp.Options = options;
		textview1.Buffer.Text = pp.ToString ();
		Highlight (findhighlight);
		findfwd = textview1.Buffer.StartIter;
		findbck = textview1.Buffer.EndIter;

		// process possible encapsulated ASN.1 element 
		// (i.e. ASN.1 elements contained in other ASN.1 elements)
		if (options.IncludeEncapsulated) {
			TextIter start;
			TextIter end;
			TextIter search = textview1.Buffer.StartIter;
			while (search.ForwardSearch(", encapsulates {", tsf, out start, out end, textview1.Buffer.EndIter)) { 
				start.ForwardChars (2);
				int num1 = 1;
				while (end.ForwardChar ()) {
					if (end.Char == "}") {
						num1--;
						if (num1 == 0) {
							end.ForwardChar ();
							break;
						}
						continue;
					}
					if (end.Char == "{") {
						num1++;
					}
				}
				textview1.Buffer.ApplyTag (encapsulated, start, end);
				search = end;
			}
		} else {
			textview1.Buffer.RemoveTag (encapsulated, textview1.Buffer.StartIter, textview1.Buffer.EndIter);
		}
	}
	 
	// UI Helpers

	private bool AskUser (string msg)
	{
		MessageDialog mdlg = new MessageDialog (gasnview,
			DialogFlags.DestroyWithParent,
			MessageType.Question, 
			ButtonsType.YesNo,
			msg);
		ResponseType answer = (ResponseType) mdlg.Run ();
		mdlg.Destroy ();
		return (answer == ResponseType.Yes);
	}

	private void WarnWrapBuffer(bool forward)
	{
		string msg = string.Format("Reached {0} of structure, continued from {1}",
			!forward ? "top" : "end",
			!forward ? "bottom" : "top");
		findlabel.Text = msg;
		findimage.Visible = true;
	}

	// Options	 
 
	private FontDescription DefaultFont {
		get {
			string fontname = (options.FontName != null) ? options.FontName : "Courier 10 Pitch 10";
			return FontDescription.FromString (fontname);
		}
	}
	 
	public string Font {
		get {
			if (fontdesc == null) {
				fontdesc = DefaultFont;
			}
			return fontdesc.ToString ();
		}
		set {
			if (value == null) {
				fontdesc = DefaultFont;
			} else {
				fontdesc = FontDescription.FromString (value);
				options.FontName = value;
			}
			textview1.ModifyFont (fontdesc);
		}
	}

	private void UpdateOptions ()
	{
		view_position.Active = options.ViewPosition;
		view_tag.Active = options.ViewTag;
		view_length.Active = options.ViewLength;
		view_dotted_indentation.Active = options.DottedIndentation;
		
		switch (options.OidFormat) {
		case OidFormat.ITU:
			view_oid_format_itu.Active = true;
			view_oid_format_ietf.Active = false;
			view_oid_format_urn.Active = false;
			break;
		case OidFormat.IETF:
			view_oid_format_itu.Active = false;
			view_oid_format_ietf.Active = true;
			view_oid_format_urn.Active = false;
			break;
		case OidFormat.URN:
			view_oid_format_itu.Active = false;
			view_oid_format_ietf.Active = false;
			view_oid_format_urn.Active = true;
			break;
		}
		
		view_display_class.Active = options.ShowTagClass;
		view_encapsulated.Active = options.IncludeEncapsulated;
		OidSource source1 = options.OidSource;

		if (source1 == OidSource.Alvestrand) {
			settings_oid_none.Active = false;
			settings_oid_alvestrand.Active = true;
			settings_oid_elibel.Active = false;
		} else if (source1 == OidSource.Elibel) {
			settings_oid_none.Active = false;
			settings_oid_alvestrand.Active = false;
			settings_oid_elibel.Active = true;
		} else {
			settings_oid_none.Active = true;
			settings_oid_alvestrand.Active = false;
			settings_oid_elibel.Active = false;
		}
	}

	public PrettyPrinterOptions LoadConfig (string filename)
	{
		try {
			if (File.Exists(filename)) {
				using (StreamReader sr = new StreamReader(filename)) {
					PrettyPrinterOptions options;
					XmlSerializer xs = new XmlSerializer (typeof (PrettyPrinterOptions));
					options = (PrettyPrinterOptions) xs.Deserialize (sr);
					sr.Close ();
					return options;
				}
			}
		}
		catch (Exception exception1) {
			Console.Error.WriteLine ("Couldn't load configuration file {0}.\nCause: {1}", filename, exception1);
		}

		return PrettyPrinterOptions.GetDefaults ();
	}
	
	public void SaveConfig (string filename, PrettyPrinterOptions options)
	{
		try {
			if (!File.Exists(filename)) {
				string path = Path.GetDirectoryName (filename);
				if (!Directory.Exists (path)) {
					Directory.CreateDirectory (path);
				}
			}
			using (StreamWriter sw = new StreamWriter (filename)) {
				XmlSerializer xs = new XmlSerializer (typeof (PrettyPrinterOptions));
				xs.Serialize (sw, options);
				sw.Close();
			}
		}
		catch (Exception e) {
			Console.Error.WriteLine ("Couldn't save configuration file {0}.\nCause: {1}",
				filename, e);
		}
	}
}
