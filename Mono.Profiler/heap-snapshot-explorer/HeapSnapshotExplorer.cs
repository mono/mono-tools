// HeapSnapshotExplorer.cs created with MonoDevelop
// User: massi at 11:38 AM 4/17/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;

namespace Mono.Profiler
{
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
		
		Menu LoadBlock;
		Menu FilterSet;
		Menu CompareSet;
		
		HeapExplorerTreeModel.Node currentSelection;
		public HeapExplorerTreeModel.Node CurrentSelection {
			get {
				return currentSelection;
			}
		}
		
		[Gtk.TreeNode (ListOnly=true)]
		public class ClassStatisticsNode : Gtk.TreeNode {
			HeapObjectSet.HeapObjectSetClassStatistics classStatistics;
			public HeapObjectSet.HeapObjectSetClassStatistics ClassStatistics {
				get {
					return classStatistics;
				}
			}
			
			public string Name {
				get {
					return classStatistics.Class.Name;
				}
			}
			public uint AllocatedBytes {
				get {
					return classStatistics.AllocatedBytes;
				}
			}
			
			public ClassStatisticsNode (HeapObjectSet.HeapObjectSetClassStatistics classStatistics) {
				this.classStatistics = classStatistics;
			}
		}
		
		public static void PrepareTreeViewForClassStatistics (NodeView view) {
			view.AppendColumn ("Name", new Gtk.CellRendererText (), delegate (TreeViewColumn column, CellRenderer cell, ITreeNode node) {
				ClassStatisticsNode classNode = (ClassStatisticsNode) node;
				((CellRendererText) cell).Markup = classNode.Name;
			});
			view.AppendColumn ("Allocated bytes", new Gtk.CellRendererText (), delegate (TreeViewColumn column, CellRenderer cell, ITreeNode node) {
				ClassStatisticsNode classNode = (ClassStatisticsNode) node;
				((CellRendererText) cell).Markup = classNode.AllocatedBytes.ToString ();
			});
			view.NodeStore = new Gtk.NodeStore (typeof (ClassStatisticsNode));
		}
		
		public static void FillTreeViewWithClassStatistics (NodeView view, HeapObjectSet.HeapObjectSetClassStatistics[] classes) {
			view.NodeStore.Clear ();
			foreach (HeapObjectSet.HeapObjectSetClassStatistics c in classes) {
				view.NodeStore.AddNode (new ClassStatisticsNode (c));
			}
		}
		
		HeapExplorerTreeModel.Node NodeSelectedForComparison;
		
		public HeapSnapshotExplorer()
		{
			this.Build();
			
			LoadBlock = new Menu ();
			MenuItem loadData = new MenuItem ("Load block data");
			loadData.Activated += delegate {
				OnLoadData ();
			};
			LoadBlock.Append (loadData);
			
			FilterSet = new Menu ();
			MenuItem filterByClass = new MenuItem ("Filter by object class");
			filterByClass.Activated += delegate {
				OnFilterByClass ();
			};
			FilterSet.Append (filterByClass);
			MenuItem filterByReferencesObjectOfClass = new MenuItem ("Filter by \"references object of class\"");
			filterByReferencesObjectOfClass.Activated += delegate {
				OnFilterByReferencesObjectOfClass ();
			};
			FilterSet.Append (filterByReferencesObjectOfClass);
			MenuItem filterByIsReferencedByObjectOfClass = new MenuItem ("Filter by \"is referenced by object of class\"");
			filterByIsReferencedByObjectOfClass.Activated += delegate {
				OnFilterByIsReferencedByObjectOfClass ();
			};
			FilterSet.Append (filterByIsReferencedByObjectOfClass);
			MenuItem markSetForComparison = new MenuItem ("Mark set for comparison");
			markSetForComparison.Activated += delegate {
				OnMarkSetForComparison ();
			};
			FilterSet.Append (markSetForComparison);
			
			CompareSet = new Menu ();
			MenuItem performComparison = new MenuItem ("Perform comparison with this set");
			performComparison.Activated += delegate {
				OnPerformComparison ();
			};
			CompareSet.Append (performComparison);
			MenuItem clearSetForComparison = new MenuItem ("Clear selection for comparison");
			clearSetForComparison.Activated += delegate {
				OnClearSetForComparison ();
			};
			CompareSet.Append (clearSetForComparison);
			
			PrepareTreeViewForClassStatistics (PerClassStatistics);
			
			Tree.Selection.Changed += delegate (object o, EventArgs args) {
				TreeSelection selection = (TreeSelection) o;
				TreeIter iter;
				if (selection.GetSelected (out iter)) {
					currentSelection = (HeapExplorerTreeModel.Node) Tree.Model.GetValue (iter, 0);
					if (currentSelection != null) {
						if (currentSelection.Objects != null) {
							FillTreeViewWithClassStatistics (PerClassStatistics, currentSelection.Objects.ClassStatistics);
						} else {
							PerClassStatistics.NodeStore.Clear ();
						}
					}
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
				HeapExplorerTreeModel.Node node = (HeapExplorerTreeModel.Node) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Markup = node.Description;
				if (node != NodeSelectedForComparison) {
					textCell.Style = Pango.Style.Normal;
				} else {
					textCell.Style = Pango.Style.Italic;
				}
			});
			countColumn.SetCellDataFunc (countCell, delegate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
				HeapExplorerTreeModel.Node node = (HeapExplorerTreeModel.Node) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Markup = node.Count;
				if (node != NodeSelectedForComparison) {
					textCell.Style = Pango.Style.Normal;
				} else {
					textCell.Style = Pango.Style.Italic;
				}
			});
			bytesColumn.SetCellDataFunc (bytesCell, delegate (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
				HeapExplorerTreeModel.Node node = (HeapExplorerTreeModel.Node) model.GetValue (iter, 0);
				CellRendererText textCell = (CellRendererText) cell;
				textCell.Markup = node.AllocatedBytes;
				if (node != NodeSelectedForComparison) {
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
			
			LoadBlock.ShowAll ();
			FilterSet.ShowAll ();
			CompareSet.ShowAll ();
			
			NodeSelectedForComparison = null;
		}
		
		public void OnLoadData () {
			if (CurrentSelection != null) {
				HeapExplorerTreeModel.SnapshotNode snapshotNode = CurrentSelection as HeapExplorerTreeModel.SnapshotNode;
				if ((snapshotNode != null) && (snapshotNode.Objects == null)) {
					((HeapExplorerTreeModel.SnapshotNode)CurrentSelection).ReadSnapshot ();
				}
			}
		}
		
		public void OnFilterByClass () {
			if (CurrentSelection != null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (CurrentSelection.Objects.ClassStatistics);
				if (c != null) {
					IHeapObjectFilter filter = new HeapObjectIsOfClass (c);
					CurrentSelection.Filter (filter);
				}
			}
		}
		
		public void OnFilterByReferencesObjectOfClass () {
			if (CurrentSelection != null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (CurrentSelection.Root.Objects.ClassStatistics);
				if (c != null) {
					IHeapObjectFilter filter = new HeapObjectReferencesObjectOfClass (c);
					CurrentSelection.Filter (filter);
				}
			}
		}
		
		public void OnFilterByIsReferencedByObjectOfClass () {
			if (CurrentSelection != null) {
				LoadedClass c = LoadedClassChooser.ChooseClass (CurrentSelection.Root.Objects.ClassStatistics);
				if (c != null) {
					IHeapObjectFilter filter = new HeapObjectIsReferencedByObjectOfClass (c);
					CurrentSelection.Filter (filter);
				}
			}
		}
		
		public void OnMarkSetForComparison () {
			if (CurrentSelection != null) {
				NodeSelectedForComparison = CurrentSelection;
			}
		}
		
		public void OnClearSetForComparison () {
			NodeSelectedForComparison = null;
		}
		
		public void OnPerformComparison () {
			if (CurrentSelection != null) {
				HeapExplorerTreeModel.SubSetNode firstSubNode;
				HeapExplorerTreeModel.SubSetNode secondSubNode;
				HeapExplorerTreeModel.Node.PerformComparison (NodeSelectedForComparison, CurrentSelection, out firstSubNode, out secondSubNode);
				NodeSelectedForComparison = null;
			}
		}
		
		
		
		[GLib.ConnectBefore]
		protected virtual void OnTreeButtonPress (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button == 3) {
				if (CurrentSelection != null) {
					HeapExplorerTreeModel.SnapshotNode snapshotNode = CurrentSelection as HeapExplorerTreeModel.SnapshotNode;
					if ((snapshotNode != null) && (snapshotNode.Objects == null)) {
						LoadBlock.Popup ();
					} else {
						if (NodeSelectedForComparison == null) {
							FilterSet.Popup ();
						} else if (CurrentSelection != NodeSelectedForComparison) {
							CompareSet.Popup ();
						}
					}
				}
			}
		}
	}
}
