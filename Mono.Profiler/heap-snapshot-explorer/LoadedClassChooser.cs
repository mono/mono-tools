// LoadedClassChooser.cs created with MonoDevelop
// User: massi at 10:14 PM 4/18/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Mono.Profiler
{
	public partial class LoadedClassChooser : Gtk.Dialog
	{
		LoadedClass result;
		public LoadedClass Result {
			get {
				return result;
			}
		}
		
		HeapObjectSet.HeapObjectSetClassStatistics currentSelection;
		
		LoadedClassChooser()
		{
			this.Build();
			HeapSnapshotExplorer.PrepareTreeViewForClassStatistics (ClassList);
			ClassList.NodeSelection.Changed += new EventHandler (OnSelectionChanged);
			currentSelection = null;
		}
		
		void OnSelectionChanged (object o, System.EventArgs args) {
			Gtk.NodeSelection selection = (Gtk.NodeSelection) o;
			HeapSnapshotExplorer.ClassStatisticsNode node = (HeapSnapshotExplorer.ClassStatisticsNode) selection.SelectedNode;
			if (node != null) {
				currentSelection = node.ClassStatistics;
			} else {
				currentSelection = null;
			}
		}
		
		void FillList (HeapObjectSet.HeapObjectSetClassStatistics[] classes) {
			HeapSnapshotExplorer.FillTreeViewWithClassStatistics (ClassList, classes);
			currentSelection = null;
		}

		protected virtual void OnCancel (object sender, System.EventArgs e)
		{
			result = null;
		}

		protected virtual void OnOK (object sender, System.EventArgs e)
		{
			if (currentSelection != null) {
				result = currentSelection.Class;
			} else {
				result = null;
			}
		}
		
		static LoadedClassChooser chooser;
		
		public static LoadedClass ChooseClass (HeapObjectSet.HeapObjectSetClassStatistics[] classes) {
			LoadedClass result;
			if (chooser == null) {
				chooser = new LoadedClassChooser ();
			}
			chooser.FillList (classes);
			ResponseType response = (ResponseType) chooser.Run ();
			if (response == ResponseType.Ok) {
				result = chooser.result;
			} else {
				result = null;
			}
			chooser.Hide ();
			return result;
		}
	}
}
