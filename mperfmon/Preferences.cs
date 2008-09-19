// Preferences.cs created with MonoDevelop
// User: lupus at 7:44 PM 9/19/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace mperfmon
{
	
	
	public partial class Preferences : Gtk.Dialog
	{
		
		public Preferences()
		{
			this.Build();
		}

		public uint Timeout {
			get {
				return (uint)(update_interval.Value * 1000);
			}
			set {
				update_interval.Value = value/1000.0;
			}
		}
	}
}
