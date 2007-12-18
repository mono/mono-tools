// project created on 12/10/2007 at 11:14 PM
using System;
using Gtk;

namespace GuiCompare
{
	class MainClass
	{
		
		public static void Main (string[] args)
		{
			Application.Init ();
			
 			try { 
				InfoManager.Init ();
			} catch (Exception e) {
				Dialog d = new Dialog ("Error", null, DialogFlags.Modal, new object [] {
					"OK", ResponseType.Ok });
				d.VBox.Add (new Label ("There was a problem while trying to initialize the InfoManager\n\n" + e.ToString ()));
				d.VBox.ShowAll ();
				d.Run ();
				return;
			}
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}