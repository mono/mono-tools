// MainWindow.cs created with MonoDevelop
// User: massi at 10:50 PM 4/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using Gtk;

namespace Mono.Profiler {
	public partial class MainWindow: Gtk.Window
	{	
		public MainWindow (): base (Gtk.WindowType.Toplevel)
		{
			Build ();
		}
		
		public Mono.Profiler.HeapSnapshotExplorer HeapExplorer {
			get {
				return heapExplorer;
			}
		}
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}

