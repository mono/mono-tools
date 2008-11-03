// HeapExplorerActions.cs created with MonoDevelop
// User: massi at 11:40 AM 4/17/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Mono.Profiler
{
	public partial class HeapExplorerActions : Gtk.ActionGroup
	{
		HeapSnapshotExplorer explorer;
		public HeapSnapshotExplorer Explorer {
			get {
				return explorer;
			}
			set {
				explorer = value;
			}
		}
		
		public HeapExplorerActions() : 
				base("Mono.Profiler.HeapExplorerActions")
		{
			this.Build();
		}

		protected virtual void OnLoadData (object sender, System.EventArgs e)
		{
			explorer.OnLoadHeapSnapshotData ();
		}
	}
}
