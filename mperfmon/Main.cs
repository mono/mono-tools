// Main.cs created with MonoDevelop
// User: lupus at 12:08 PM 9/18/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using System.Diagnostics;
using Gtk;

namespace mperfmon
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}