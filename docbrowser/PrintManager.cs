//
//
// PrintManager.cs: GtkHTML version dependent printing support.
//
// Authors: Mario Sopena
// 	    Rafael Ferreira <raf@ophion.org>
//	    Mike Kestner <mkestner@novell.com>
//

using System;
using Gtk;
using Gnome;

namespace Monodoc {
	class PrintManager {
	
#if GTKHTML_SHARP_3_14
		public static void Print (string html)
		{
			new Gtk.HTML (html).PrintOperationRun (new PrintOperation (), PrintOperationAction.PrintDialog, null, null, null, null, null);
		}
#else
		// Fallback to the original GNOME Print API.
		public static void Print (string html) 
		{
			string caption = "Monodoc Printing";

			Gnome.PrintJob pj = new Gnome.PrintJob (PrintConfig.Default ());
			PrintDialog dialog = new PrintDialog (pj, caption, 0);

			Gtk.HTML gtk_html = new Gtk.HTML (html);
			gtk_html.PrintSetMaster (pj);
			
			Gnome.PrintContext ctx = pj.Context;
			gtk_html.Print (ctx);

			pj.Close ();

			// hello user
			int response = dialog.Run ();
		
			if (response == (int) PrintButtons.Cancel) {
				dialog.Hide ();
				dialog.Destroy ();
				return;
			} else if (response == (int) PrintButtons.Print) {
				pj.Print ();
			} else if (response == (int) PrintButtons.Preview) {
				new PrintJobPreview (pj, caption).Show ();
			}
		
			ctx.Close ();
			dialog.Hide ();
			dialog.Destroy ();
		}
#endif
	}
}

