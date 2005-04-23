//
// FileDialog.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using GLib;
using Gtk;
using GtkSharp;

namespace Mono.NUnit.GUI
{
	public class FileDialog : FileSelection
	{
		bool ok;
		string filename;

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}
	
		public FileDialog () : this (_("Select an assembly to load"), "*.dll") {}

		public FileDialog (string title, string complete)
			: base (title)
		{
			ShowFileops = false;
			SelectMultiple = false;
			filename = null;
			Complete (complete);
			OkButton.Clicked += new EventHandler (OkClicked);
			Response += new ResponseHandler (OnResponse);
		}

		void OkClicked (object sender, EventArgs args)
		{
			ok = true;
			filename = base.Filename;
		}

		void OnResponse (object sender, ResponseArgs args)
		{
			Hide ();
		}

		public bool Cancelled {
			get { return !ok; }
		}

		public bool Ok {
			get { return ok; }
		}

		public new string Filename {
			get { return filename; }
		}
	}
}

