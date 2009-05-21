// HeapSnapshotExplorer.cs created with MonoDevelop
// User: massi at 11:38 AM 4/17/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Mono.Profiler
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class HeapSnapshotExplorer : Gtk.Bin
	{
		HeapExplorerTreeModel model;
		public HeapExplorerTreeModel Model {
			get {
				return model;
			}
			set {
				model = value;
				Tree.Model = model.Model;
			}
		}		
		
		HeapExplorerTreeModel.INode currentSelection;
		public HeapExplorerTreeModel.INode CurrentSelection {
			get {
				return currentSelection;
			}
		}
		
		StatisticsNode currentListSelection;
		public StatisticsNode CurrentListSelection {
			get {
				return currentListSelection;
			}
		}
		
		abstract class StatisticsNodeMenuHandler {
			HeapSnapshotExplorer explorer;
			public HeapSnapshotExplorer Explorer {
				get {
					return explorer;
				}
			}
			protected abstract Menu GetContextMenu ();
			public void ShowContextMenu () {
				if ((explorer.CurrentSelection != null) && (explorer.CurrentListSelection != null)) {
					Menu contextMenu = GetContextMenu ();
					if (contextMenu != null) {
						contextMenu.Popup ();
					}
				}
			}
			protected void FilterCurrentSet<HI> (IHeapItemFilter<HI> filter) where HI : IHeapItem {
				HeapExplorerTreeModel.Node<HI> node = (HeapExplorerTreeModel.Node<HI>) Explorer.CurrentSelection;
				node.Filter (filter);
			}
			protected StatisticsNodeMenuHandler (HeapSnapshotExplorer explorer) {
				this.explorer = explorer;
			}
		}
		class StatisticsNodeMenuHandlerForClasses : StatisticsNodeMenuHandler {
			void FilterCurrentSetByCurrentClass () {
				LoadedClass currentClass = (LoadedClass) Explorer.CurrentListSelection.Statistics.Subject;
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<HeapObject>) {
					FilterCurrentSet<HeapObject> (new HeapItemIsOfClass<HeapObject> (currentClass));
				}
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<AllocatedObject>) {
					FilterCurrentSet<AllocatedObject> (new HeapItemIsOfClass<AllocatedObject> (currentClass));
				}
			}
			
			Menu menu;
			protected override Menu GetContextMenu () {
				return menu;
			}
			
			public StatisticsNodeMenuHandlerForClasses (HeapSnapshotExplorer explorer) : base (explorer) {
				menu = new Menu ();
				MenuItem menuItem;
				menuItem = new MenuItem ("Filter current set by this class");
				menuItem.Activated += delegate (object sender, EventArgs e) {
					FilterCurrentSetByCurrentClass ();
				};
				menu.Add (menuItem);
				menuItem = new MenuItem ("Show statistics by caller method");
				menuItem.Activated += delegate (object sender, EventArgs e) {
					explorer.FillStatisticsListWithMethodData ();
				};
				menu.Add (menuItem);
				menu.ShowAll ();
			}
		}
		class StatisticsNodeMenuHandlerForMethods : StatisticsNodeMenuHandler {
			void FilterCurrentSetByCurrentMethod () {
				LoadedMethod currentMethod = (LoadedMethod) Explorer.CurrentListSelection.Statistics.Subject;
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<HeapObject>) {
					FilterCurrentSet<HeapObject> (new HeapItemWasAllocatedByMethod<HeapObject> (currentMethod));
				}
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<AllocatedObject>) {
					FilterCurrentSet<AllocatedObject> (new HeapItemWasAllocatedByMethod<AllocatedObject> (currentMethod));
				}
			}
			
			Menu menu;
			protected override Menu GetContextMenu () {
				return menu;
			}
			public StatisticsNodeMenuHandlerForMethods (HeapSnapshotExplorer explorer) : base (explorer) {
				menu = new Menu ();
				MenuItem menuItem;
				menuItem = new MenuItem ("Filter current set by this method");
				menuItem.Activated += delegate (object sender, EventArgs e) {
					FilterCurrentSetByCurrentMethod ();
				};
				menu.Add (menuItem);
				menuItem = new MenuItem ("Show statistics by call stack");
				menuItem.Activated += delegate (object sender, EventArgs e) {
					explorer.FillStatisticsListWithCallStackData ();
				};
				menu.Add (menuItem);
				menu.ShowAll ();
			}
		}
		class StatisticsNodeMenuHandlerForCallStacks : StatisticsNodeMenuHandler {
			void FilterCurrentSetByCurrentCallStack () {
				StackTrace currentCallStack = (StackTrace) Explorer.CurrentListSelection.Statistics.Subject;
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<HeapObject>) {
					FilterCurrentSet<HeapObject> (new FilterHeapItemByAllocationCallStack<HeapObject> (currentCallStack));
				}
				if (Explorer.CurrentSelection is HeapExplorerTreeModel.Node<AllocatedObject>) {
					FilterCurrentSet<AllocatedObject> (new FilterHeapItemByAllocationCallStack<AllocatedObject> (currentCallStack));
				}
			}
			
			Menu menu;
			protected override Menu GetContextMenu () {
				return menu;
			}
			public StatisticsNodeMenuHandlerForCallStacks (HeapSnapshotExplorer explorer) : base (explorer) {
				menu = new Menu ();
				MenuItem menuItem;
				menuItem = new MenuItem ("Filter current set by this call stack");
				menuItem.Activated += delegate (object sender, EventArgs e) {
					FilterCurrentSetByCurrentCallStack ();
				};
				menu.Add (menuItem);
				menu.ShowAll ();
			}
		}
		
		StatisticsNodeMenuHandler currentListMenuHandler;
		StatisticsNodeMenuHandlerForClasses listMenuHandlerForClasses;
		StatisticsNodeMenuHandlerForMethods listMenuHandlerForMethods;
		StatisticsNodeMenuHandlerForCallStacks listMenuHandlerForCallStacks;
		void FillStatisticsListWithClassData () {
			currentListMenuHandler = listMenuHandlerForClasses;
			if (currentSelection != null) {
				if (currentSelection.Items != null) {
					FillTreeViewWithStatistics (PerClassStatistics, currentSelection.Items.ClassStatistics);
				} else {
					PerClassStatistics.NodeStore.Clear ();
				}
			}
			currentListSelection = null;
		}
		void FillStatisticsListWithMethodData () {
			currentListMenuHandler = listMenuHandlerForMethods;
			if (currentSelection != null) {
				if (currentSelection.Items != null) {
					if (! currentSelection.Items.ObjectAllocationsArePresent) {
						currentSelection.Items.FindObjectAllocations (currentSelection.Root);
					}
					FillTreeViewWithStatistics (PerClassStatistics, currentSelection.Items.AllocatorMethodStatistics);
				} else {
					PerClassStatistics.NodeStore.Clear ();
				}
			}
			currentListSelection = null;
		}
		void FillStatisticsListWithCallStackData () {
			currentListMenuHandler = listMenuHandlerForCallStacks;
			if (currentSelection != null) {
				if (currentSelection.Items != null) {
					if (! currentSelection.Items.ObjectAllocationsArePresent) {
						currentSelection.Items.FindObjectAllocations (currentSelection.Root);
					}
					FillTreeViewWithStatistics (PerClassStatistics, currentSelection.Items.AllocationCallStackStatistics);
				} else {
					PerClassStatistics.NodeStore.Clear ();
				}
			}
			currentListSelection = null;
		}
		
		[Gtk.TreeNode (ListOnly=true)]
		public class StatisticsNode : Gtk.TreeNode {
			IHeapItemSetStatisticsBySubject statistics;
			public IHeapItemSetStatisticsBySubject Statistics {
				get {
					return statistics;
				}
			}
			
			public string Description {
				get {
					return statistics.Subject.Description;
				}
			}
			public uint ItemsCount {
				get {
					return statistics.ItemsCount;
				}
			}
			public uint AllocatedBytes {
				get {
					return statistics.AllocatedBytes;
				}
			}
			
			public StatisticsNode (IHeapItemSetStatisticsBySubject statistics) {
				this.statistics = statistics;
			}
		}
		
		public static void PrepareTreeViewForStatisticsDisplay (NodeView view) {
			view.AppendColumn ("Description", new Gtk.CellRendererText (), delegate (TreeViewColumn column, CellRenderer cell, ITreeNode treeNode) {
				StatisticsNode node = (StatisticsNode) treeNode;
				((CellRendererText) cell).Text = node.Description;
			});
			view.AppendColumn ("Object count", new Gtk.CellRendererText (), delegate (TreeViewColumn column, CellRenderer cell, ITreeNode treeNode) {
				StatisticsNode node = (StatisticsNode) treeNode;
				((CellRendererText) cell).Text = node.ItemsCount.ToString ();
			});
			view.AppendColumn ("Allocated bytes", new Gtk.CellRendererText (), delegate (TreeViewColumn column, CellRenderer cell, ITreeNode treeNode) {
				StatisticsNode node = (StatisticsNode) treeNode;
				((CellRendererText) cell).Text = node.AllocatedBytes.ToString ();
			});
			view.NodeStore = new Gtk.NodeStore (typeof (StatisticsNode));
		}
		
		public static void FillTreeViewWithStatistics (NodeView view, IHeapItemSetStatisticsBySubject[] statistics) {
			view.NodeStore.Clear ();
			foreach (IHeapItemSetStatisticsBySubject s in statistics) {
				view.NodeStore.AddNode (new StatisticsNode (s));
			}
		}
		
		HeapExplorerTreeModel.Node<HeapObject> markedObjectNode;
		public HeapExplorerTreeModel.Node<HeapObject> MarkedObjectNode {
			get {
				return markedObjectNode;
			}
		}
		HeapExplorerTreeModel.Node<AllocatedObject> markedAllocationNode;
		public HeapExplorerTreeModel.Node<AllocatedObject> MarkedAllocationNode {
			get {
				return markedAllocationNode;
			}
		}
		public bool NodeIsMarked {
			get {
				return (markedObjectNode != null) || (markedAllocationNode != null);
			}
		}
		public HeapExplorerTreeModel.INode MarkedNode {
			get {
				return (markedObjectNode != null) ? (HeapExplorerTreeModel.INode) markedObjectNode : (markedAllocationNode != null) ? (HeapExplorerTreeModel.INode) markedAllocationNode : null;
			}
		}
		bool markIsForComparison;
		public bool MarkIsForComparison {
			get {
				return markIsForComparison;
			}
		}
		bool markIsForFiltering;
		public bool MarkIsForFiltering {
			get {
				return markIsForFiltering;
			}
		}
		
		Menu loadHeapSnapshotBlock;
		public Menu LoadHeapSnapshotBlock {
			get {
				return loadHeapSnapshotBlock;
			}
		}
		Menu loadAllocationsBlocks;
		public Menu LoadAllocationsBlocks {
			get {
				return loadAllocationsBlocks;
			}
		}
		Menu filterObjectSet;
		public Menu FilterObjectSet {
			get {
				return filterObjectSet;
			}
		}
		Menu filterAllocationSet;
		public Menu FilterAllocationSet {
			get {
				return filterAllocationSet;
			}
		}
		Menu compareObjectSet;
		public Menu CompareObjectSet {
			get {
				return compareObjectSet;
			}
		}
		Menu compareAllocationSet;
		public Menu CompareAllocationSet {
			get {
				return compareAllocationSet;
			}
		}
		Menu filterObjectSetUsingSelection;
		public Menu FilterObjectSetUsingSelection {
			get {
				return filterObjectSetUsingSelection;
			}
		}
		Menu filterAllocationSetUsingSelection;
		public Menu FilterAllocationSetUsingSelection {
			get {
				return filterAllocationSetUsingSelection;
			}
		}
		
		public HeapSnapshotExplorer()
		{
			Build();
			MenuItem menuItem;
			
			OnClearMark ();
			
			loadHeapSnapshotBlock = new Menu ();
			menuItem = new MenuItem ("Load block data");
			menuItem.Activated += delegate {
				OnLoadHeapSnapshotData ();
			};
			loadHeapSnapshotBlock.Append (menuItem);
			
			loadAllocationsBlocks = new Menu ();
			menuItem = new MenuItem ("Load block data");
			menuItem.Activated += delegate {
				OnLoadAllocationsEventData ();
			};
			loadAllocationsBlocks.Append (menuItem);
			
			filterObjectSet = new Menu ();
			menuItem = new MenuItem ("Filter by object class");
			menuItem.Activated += delegate {
				OnFilterByClass<HeapObject> ();
			};
			filterObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Filter by \"references object of class\"");
			menuItem.Activated += delegate {
				OnFilterByReferencesObjectOfClass ();
			};
			filterObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Filter by \"is referenced by object of class\"");
			menuItem.Activated += delegate {
				OnFilterByIsReferencedByObjectOfClass ();
			};
			filterObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Mark set for comparison");
			menuItem.Activated += delegate {
				OnMarkObjectSetForComparison ();
			};
			filterObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Mark set for \"set reference\" filtering");
			menuItem.Activated += delegate {
				OnMarkObjectSetForFiltering ();
			};
			filterObjectSet.Append (menuItem);
			
			filterAllocationSet = new Menu ();
			menuItem = new MenuItem ("Filter by object class");
			menuItem.Activated += delegate {
				OnFilterByClass<AllocatedObject> ();
			};
			filterAllocationSet.Append (menuItem);
			menuItem = new MenuItem ("Mark set for comparison");
			menuItem.Activated += delegate {
				OnMarkAllocationSetForComparison ();
			};
			filterAllocationSet.Append (menuItem);
			// For now no set based filtering for allocations...
			//menuItem = new MenuItem ("Mark set for filtering");
			//menuItem.Activated += delegate {
			//	OnMarkAllocationSetForFiltering ();
			//};
			//filterAllocationSet.Append (menuItem);
			
			compareObjectSet = new Menu ();
			menuItem = new MenuItem ("Perform comparison with this set");
			menuItem.Activated += delegate {
				OnCompareWithSet<HeapObject> ();
			};
			compareObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Perform intersection with this set");
			menuItem.Activated += delegate {
				OnIntersectWithSet<HeapObject> ();
			};
			compareObjectSet.Append (menuItem);
			menuItem = new MenuItem ("Clear selection");
			menuItem.Activated += delegate {
				OnClearMark ();
			};
			compareObjectSet.Append (menuItem);
			
			compareAllocationSet = new Menu ();
			menuItem = new MenuItem ("Perform comparison with this set");
			menuItem.Activated += delegate {
				OnCompareWithSet<AllocatedObject> ();
			};
			compareAllocationSet.Append (menuItem);
			menuItem = new MenuItem ("Perform intersection with this set");
			menuItem.Activated += delegate {
				OnIntersectWithSet<AllocatedObject> ();
			};
			compareAllocationSet.Append (menuItem);
			menuItem = new MenuItem ("Clear selection");
			menuItem.Activated += delegate {
				OnClearMark ();
			};
			compareAllocationSet.Append (menuItem);
			
			filterObjectSetUsingSelection = new Menu ();
			menuItem = new MenuItem ("Select objects referencing objects in this set");
			menuItem.Activated += delegate {
				OnFilterByReferencesObjectInSet<HeapObject> ();
			};
			filterObjectSetUsingSelection.Append (menuItem);
			menuItem = new MenuItem ("Select objects referenced by objects in this set");
			menuItem.Activated += delegate {
				OnFilterByIsReferencedByObjectInSet<HeapObject> ();
			};
			filterObjectSetUsingSelection.Append (menuItem);
			menuItem = new MenuItem ("Clear selection");
			menuItem.Activated += delegate {
				OnClearMark ();
			};
			filterObjectSetUsingSelection.Append (menuItem);
			
			filterAllocationSetUsingSelection = new Menu ();
			menuItem = new MenuItem ("Select objects referencing objects in this set");
			menuItem.Activated += delegate {
				OnFilterByReferencesObjectInSet<AllocatedObject> ();
			};
			filterAllocationSetUsingSelection.Append (menuItem);
			menuItem = new MenuItem ("Select objects referenced by objects in this set");
			menuItem.Activated += delegate {
				OnFilterByIsReferencedByObjectInSet<AllocatedObject> ();
			};
			filterAllocationSetUsingSelection.Append (menuItem);
			menuItem = new MenuItem ("Clear selection");
			menuItem.Activated += delegate {
				OnClearMark ();
			};
			filterAllocationSetUsingSelection.Append (menuItem);
			
			PrepareTreeViewForStatisticsDisplay (PerClassStatistics);
			PerClassStatistics.NodeSelection.Changed += delegate (object o, EventArgs args) {
				currentListSelection = (StatisticsNode) PerClassStatistics.NodeSelection.SelectedNode;
			};
			listMenuHandlerForClasses = new StatisticsNodeMenuHandlerForClasses (this);
			listMenuHandlerForMethods = new StatisticsNodeMenuHandlerForMethods (this);
			listMenuHandlerForCallStacks = new StatisticsNodeMenuHandlerForCallStacks (this);
			
			Tree.Selection.Changed += delegate (object o, EventArgs args) {
				TreeSelection selection = (TreeSelection) o;
				TreeIter iter;
				if (selection.GetSelected (out iter)) {
					currentSelection = (HeapExplorerTreeModel.INode) Tree.Model.GetValue (iter, 0);
					FillStatisticsListWithClassData ();
				}
			};
			
			Gtk.TreeViewColumn setColumn = new Gtk.TreeViewColumn ();
			Gtk.TreeViewColumn countColumn = new Gtk.TreeViewColumn ();
			Gtk.TreeViewColumn bytesColumn = new Gtk.TreeViewColumn ();
			setColumn.Title = "Object set";
			countColumn.Title = "Object count";
			bytesColumn.Title = "Bytes";
			Gtk.CellRendererText setCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText countCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText bytesCell = new Gtk.CellRendererText ();
			setColumn.PackStart (setCell, true);
			countColumn.PackStart (countCell, true);
			bytesColumn.PackStart (bytesCell, true);
			
			setColumn.SetCellDataFunc (setCell, delegate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
				HeapExplorerTreeModel.INode node = (HeapExplorerTreeModel.INode) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Text = node.Description;
				if (node != MarkedNode) {
					textCell.Style = Pango.Style.Normal;
				} else {
					textCell.Style = Pango.Style.Italic;
				}
			});
			countColumn.SetCellDataFunc (countCell, delegate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
				HeapExplorerTreeModel.INode node = (HeapExplorerTreeModel.INode) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Text = node.Count;
				if (node != MarkedNode) {
					textCell.Style = Pango.Style.Normal;
				} else {
					textCell.Style = Pango.Style.Italic;
				}
			});
			bytesColumn.SetCellDataFunc (bytesCell, delegate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
				HeapExplorerTreeModel.INode node = (HeapExplorerTreeModel.INode) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Text = node.AllocatedBytes;
				if (node != MarkedNode) {
					textCell.Style = Pango.Style.Normal;
				} else {
					textCell.Style = Pango.Style.Italic;
				}
			});
			
			setColumn.AddAttribute (setCell, "text", 0);
			countColumn.AddAttribute (countCell, "text", 1);
			bytesColumn.AddAttribute (bytesCell, "text", 2);
			
			Tree.AppendColumn (setColumn);
			Tree.AppendColumn (countColumn);
			Tree.AppendColumn (bytesColumn);
			
			loadHeapSnapshotBlock.ShowAll ();
			loadAllocationsBlocks.ShowAll ();
			filterObjectSet.ShowAll ();
			filterAllocationSet.ShowAll ();
			compareObjectSet.ShowAll ();
			compareAllocationSet.ShowAll ();
			filterObjectSetUsingSelection.ShowAll ();
			filterAllocationSetUsingSelection.ShowAll ();
		}
		
		public void OnLoadHeapSnapshotData () {
			HeapExplorerTreeModel.SnapshotNode node = CurrentSelection as HeapExplorerTreeModel.SnapshotNode;
			if ((node != null) && (node.Items == null)) {
				node.ReadSnapshot ();
			}
		}
		
		public void OnLoadAllocationsEventData () {
			HeapExplorerTreeModel.AllocationsNode node = CurrentSelection as HeapExplorerTreeModel.AllocationsNode;
			if ((node != null) && (node.Items == null)) {
				node.ReadEvents ();
			}
		}
		
		public void OnFilterByClass<HI> () where HI : IHeapItem {
			HeapExplorerTreeModel.Node<HI> node = CurrentSelection as HeapExplorerTreeModel.Node<HI>;
			if (node != null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (CurrentSelection.Items.ClassStatistics);
				if (c != null) {
					HeapItemIsOfClass<HI> filter = new HeapItemIsOfClass<HI> (c);
					node.Filter (filter);
				}
			}
		}
		
		public void OnFilterByReferencesObjectOfClass () {
			HeapExplorerTreeModel.Node<HeapObject> node = CurrentSelection as HeapExplorerTreeModel.Node<HeapObject>;
			if (node!= null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (node.Root.Items.ClassStatistics);
				if (c != null) {
					HeapObjectReferencesObjectOfClass filter = new HeapObjectReferencesObjectOfClass (c);
					node.Filter (filter);
				}
			}
		}
		
		public void OnFilterByIsReferencedByObjectOfClass () {
			HeapExplorerTreeModel.Node<HeapObject> node = CurrentSelection as HeapExplorerTreeModel.Node<HeapObject>;
			if (node != null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (node.Root.Items.ClassStatistics);
				if (c != null) {
					IHeapObjectFilter filter = new HeapObjectIsReferencedByObjectOfClass (c);
					node.Filter (filter);
				}
			}
		}
		
		public void OnMarkObjectSetForComparison () {
			markedObjectNode = CurrentSelection as HeapExplorerTreeModel.Node<HeapObject>;
			markedAllocationNode = null;
			markIsForComparison = true;
			markIsForFiltering = false;
		}
		
		public void OnMarkObjectSetForFiltering () {
			markedObjectNode = CurrentSelection as HeapExplorerTreeModel.Node<HeapObject>;
			markedAllocationNode = null;
			markIsForComparison = false;
			markIsForFiltering = true;
		}
		
		public void OnMarkAllocationSetForComparison () {
			markedObjectNode = null;
			markedAllocationNode = CurrentSelection as HeapExplorerTreeModel.Node<AllocatedObject>;
			markIsForComparison = true;
			markIsForFiltering = false;
		}
		
		public void OnMarkAllocationSetForFiltering () {
			markedObjectNode = null;
			markedAllocationNode = CurrentSelection as HeapExplorerTreeModel.Node<AllocatedObject>;
			markIsForComparison = false;
			markIsForFiltering = true;
		}
		
		public void OnClearMark () {
			markedObjectNode = null;
			markedAllocationNode = null;
			markIsForComparison = false;
			markIsForFiltering = false;
		}
		
		public void OnCompareWithSet<HI> () where HI : IHeapItem {
			HeapExplorerTreeModel.Node<HI> node = CurrentSelection as HeapExplorerTreeModel.Node<HI>;
			if (node != null) {
				if (markedObjectNode != null) {
					node.CompareWithNode (markedObjectNode);
				} else if (markedAllocationNode != null) {
					node.CompareWithNode (markedAllocationNode);
				}
				OnClearMark ();
			}
		}
		
		public void OnIntersectWithSet<HI> () where HI : IHeapItem {
			HeapExplorerTreeModel.Node<HI> node = CurrentSelection as HeapExplorerTreeModel.Node<HI>;
			if (node != null) {
				if (markedObjectNode != null) {
					markedObjectNode.IntersectWithNode (node);
				} else if (markedAllocationNode != null) {
					markedAllocationNode.IntersectWithNode (node);
				}
				OnClearMark ();
			}
		}
		
		public void OnFilterByReferencesObjectInSet<HI> () where HI : IHeapItem {
			HeapExplorerTreeModel.Node<HI> itemNode = CurrentSelection as HeapExplorerTreeModel.Node<HI>;
			if ((itemNode != null) && (markedObjectNode != null)) {
				itemNode.SelectObjectsReferencingItem (markedObjectNode);
				OnClearMark ();
			}
		}
		
		public void OnFilterByIsReferencedByObjectInSet<HI> () where HI : IHeapItem {
			HeapExplorerTreeModel.Node<HI> itemNode = CurrentSelection as HeapExplorerTreeModel.Node<HI>;
			if ((itemNode != null) && (markedObjectNode != null)) {
				itemNode.SelectObjectsReferencedByItem (markedObjectNode);
				OnClearMark ();
			}
		}
		
		[GLib.ConnectBefore]
		protected virtual void OnTreeButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 3) {
				if (CurrentSelection != null) {
					CurrentSelection.ContextMenu.Popup ();
				}
			}
		}
		
		[GLib.ConnectBefore]
		protected virtual void OnListButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 3) {
				currentListMenuHandler.ShowContextMenu ();
			}
		}
	}
}
