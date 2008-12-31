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
		IHeapItemSetStatisticsSubject result;
		public IHeapItemSetStatisticsSubject Result {
			get {
				return result;
			}
		}
		
		IHeapItemSetStatisticsBySubject currentSelection;
		
		LoadedClassChooser()
		{
			this.Build();
			HeapSnapshotExplorer.PrepareTreeViewForStatisticsDisplay (ClassList);
			ClassList.NodeSelection.Changed += new EventHandler (OnSelectionChanged);
			currentSelection = null;
		}
		
		void OnSelectionChanged (object o, System.EventArgs args) {
			Gtk.NodeSelection selection = (Gtk.NodeSelection) o;
			HeapSnapshotExplorer.StatisticsNode node = (HeapSnapshotExplorer.StatisticsNode) selection.SelectedNode;
			if (node != null) {
				currentSelection = node.Statistics;
			} else {
				currentSelection = null;
			}
		}
		
		void FillList (IHeapItemSetStatisticsBySubject[] statistics) {
			HeapSnapshotExplorer.FillTreeViewWithStatistics (ClassList, statistics);
			currentSelection = null;
		}

		protected virtual void OnCancel (object sender, System.EventArgs e)
		{
			result = null;
		}

		protected virtual void OnOK (object sender, System.EventArgs e)
		{
			if (currentSelection != null) {
				result = currentSelection.Subject;
			} else {
				result = null;
			}
		}
		
		static LoadedClassChooser chooser;
		
		static IHeapItemSetStatisticsSubject ChooseSubject (IHeapItemSetStatisticsBySubject[] subjects, string subjectName) {
			IHeapItemSetStatisticsSubject result;
			if (chooser == null) {
				chooser = new LoadedClassChooser ();
			}
			chooser.Title = "Choose " + subjectName;
			chooser.FillList (subjects);
			ResponseType response = (ResponseType) chooser.Run ();
			if (response == ResponseType.Ok) {
				result = chooser.result;
			} else {
				result = null;
			}
			chooser.Hide ();
			return result;
		}
		public static LoadedClass ChooseClass (IHeapItemSetStatisticsBySubject[] subjects) {
			IHeapItemSetStatisticsSubject result = ChooseSubject (subjects, "class");
			return result as LoadedClass;
		}
		public static LoadedMethod ChooseMethod (IHeapItemSetStatisticsBySubject[] subjects) {
			IHeapItemSetStatisticsSubject result = ChooseSubject (subjects, "method");
			return result as LoadedMethod;
		}
		public static StackTrace ChooseCallStack (IHeapItemSetStatisticsBySubject[] subjects) {
			IHeapItemSetStatisticsSubject result = ChooseSubject (subjects, "call stack");
			return result as StackTrace;
		}
	}
}
