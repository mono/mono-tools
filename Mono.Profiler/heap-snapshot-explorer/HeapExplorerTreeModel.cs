// Author:
// Massimiliano Mantione (massi@ximian.com)
//
// (C) 2008 Novell, Inc  http://www.novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using Gtk;

namespace Mono.Profiler
{
	public class HeapExplorerTreeModel {
		public interface INode {
			IHeapItemSet Items {get;}
			string Description {get;}
			TreeIter TreeIter {get;}
			INode Parent {get;}
			IRootNode Root {get;}
			string Count {get;}
			string AllocatedBytes {get;}
			Menu ContextMenu {get;}
		}
		public interface IRootNode : INode, ProviderOfPreviousAllocationsSets {
			AllocationsNode PreviousAllocationsNode {get;}
		}
		
		public abstract class Node<HI> : INode where HI : IHeapItem {
			public abstract HeapItemSet<HI> Items {get;}
			IHeapItemSet INode.Items {
				get {
					return Items;
				}
			}
			
			protected abstract SubSetNode<HI> NewSubSet (HeapItemSet<HI> items);
			
			public virtual string Description {
				get {
					return Items.ShortDescription;
				}
			}
			
			protected HeapExplorerTreeModel model;
			TreeIter treeIter;
			public TreeIter TreeIter {
				get {
					return treeIter;
				}
			}
			
			Node<HI> parent;
			public Node<HI> Parent {
				get {
					return parent;
				}
			}
			INode INode.Parent {
				get {
					return parent;
				}
			}
			
			public Node<HI> Root {
				get {
					if (parent != null) {
						return parent.Root;
					} else {
						return this;
					}
				}
			}
			IRootNode INode.Root {
				get {
					return (IRootNode) Root;
				}
			}
			
			AllocationsNode previousAllocationsNode;
			public AllocationsNode PreviousAllocationsNode {
				get {
					return previousAllocationsNode;
				}
			}
			public IEnumerable<HeapItemSet<AllocatedObject>> PreviousAllocationsSets () {
				AllocationsNode currentNode = this.PreviousAllocationsNode;
				while (currentNode != null) {
					if (currentNode.Items == null) {
						currentNode.ReadEvents ();
					}
					yield return currentNode.Items;
					currentNode = currentNode.PreviousAllocationsNode;
				}
			}
			
			public string Count {
				get {
					if (Items != null) {
						return Items.Elements.Length.ToString ();
					} else {
						return "";
					}
				}
			}
			public string AllocatedBytes {
				get {
					if (Items != null) {
						return Items.AllocatedBytes.ToString ();
					} else {
						return "";
					}
				}
			}
			
			protected TreeIter HandleNodeCreation () {
				if (parent != null) {
					TreeIter result = model.Model.AppendNode (parent.TreeIter);
					model.Model.SetValue (result, 0, this);
					return result;
				} else {
					return model.Model.AppendValues (this);
				}
			}
			
			public SubSetNode<HI> Filter (IHeapItemFilter<HI> filter) {
				HeapItemSetFromFilter<HI> subSet = new HeapItemSetFromFilter<HI> (Items, filter);
				SubSetNode<HI> result = NewSubSet (subSet);
				return result;
			}
			
			public void CompareWithNode<OHI> (Node<OHI> otherNode) where OHI : IHeapItem {
				HeapItemSet<HI> onlyInThisSet;
				HeapItemSet<OHI> onlyInOtherSet;
				Items.CompareWithSet (otherNode.Items, out onlyInThisSet, out onlyInOtherSet);
				NewSubSet (onlyInThisSet);
				otherNode.NewSubSet (onlyInOtherSet);
			}
			
			public void IntersectWithNode<OHI> (Node<OHI> otherNode) where OHI : IHeapItem {
				NewSubSet (Items.IntersectWithSet (otherNode.Items));
			}
			
			public void SelectObjectsReferencingItem (Node<HeapObject> objectsNode) {
				objectsNode.NewSubSet (Items.ObjectsReferencingItemInSet (objectsNode.Items));
			}
			
			public void SelectObjectsReferencedByItem (Node<HeapObject> objectsNode) {
				objectsNode.NewSubSet (Items.ObjectsReferencedByItemInSet (objectsNode.Items));
			}
			
			public abstract Menu ContextMenu {get;}
			
			protected Node (HeapExplorerTreeModel model, Node<HI> parent, AllocationsNode previousAllocationsNode) {
				this.model = model;
				this.parent = parent;
				this.previousAllocationsNode = previousAllocationsNode;
				this.treeIter = HandleNodeCreation ();
			}
		}
		
		public class SnapshotNode : Node<HeapObject>, IRootNode {
			SeekableLogFileReader.Block heapBlock;
			
			HeapSnapshot snapshot;
			public HeapSnapshot Snapshot {
				get {
					return snapshot;
				}
			}
			
			HeapObjectSetFromSnapshot items;
			public override HeapItemSet<HeapObject> Items {
				get {
					return items;
				}
			}
			
			public void ReadSnapshot () {
				if (items == null) {
					model.heapEventProcessor.RecordAllocations = true;
					model.Reader.ReadBlock (heapBlock).Decode (model.heapEventProcessor, model.Reader);
					snapshot = model.heapEventProcessor.LastHeapSnapshot;
					items = new HeapObjectSetFromSnapshot (snapshot);
					model.Model.SetValue (TreeIter, 0, this);
				}
			}
			
			public override string Description {
				get {
					if (items != null) {
						return items.ShortDescription;
					} else {
						return String.Format ("Heap block ({0}.{1:000}s)", heapBlock.TimeFromStart.Seconds, heapBlock.TimeFromStart.Milliseconds);
					}
				}
			}
			
			protected override SubSetNode<HeapObject> NewSubSet (HeapItemSet<HeapObject> items) {
				return new HeapObjectSubSetNode (model, this, items);
			}
			
			public override Menu ContextMenu {
				get {
					if (items != null) {
						if (model.Explorer.NodeIsMarked) {
							if (model.Explorer.MarkIsForComparison) {
								return model.Explorer.CompareObjectSet;
							} else if (model.Explorer.MarkIsForFiltering) {
								return model.Explorer.FilterObjectSetUsingSelection;
							} else {
								throw new Exception ("Mark is buggy");
							}
						} else {
							return model.Explorer.FilterObjectSet;
						}
					} else {
						return model.Explorer.LoadHeapSnapshotBlock;
					}
				}
			}
			
			public SnapshotNode (HeapExplorerTreeModel model, SeekableLogFileReader.Block heapBlock, AllocationsNode previousAllocationsNode) : base (model, null, previousAllocationsNode) {
				this.heapBlock = heapBlock;
				this.items = null;
				this.snapshot = null;
			}
		}
		
		public class AllocationsNode : Node<AllocatedObject>, IRootNode {
			SeekableLogFileReader.Block[] eventBlocks;
			
			AllocatedObjectSetFromEvents items;
			public override HeapItemSet<AllocatedObject> Items {
				get {
					return items;
				}
			}
			
			public void ReadEvents () {
				if (items == null) {
					model.heapEventProcessor.RecordAllocations = true;
					foreach (SeekableLogFileReader.Block eventBlock in eventBlocks) {
						model.Reader.ReadBlock (eventBlock).Decode (model.heapEventProcessor, model.Reader);
					}
					items = new AllocatedObjectSetFromEvents (eventBlocks [0].TimeFromStart, model.heapEventProcessor.AllocatedObjects);
					model.Model.SetValue (TreeIter, 0, this);
				}
			}
			
			public override string Description {
				get {
					if (items != null) {
						return items.ShortDescription;
					} else {
						return String.Format ("Events ({0}.{1:000}s)", eventBlocks [0].TimeFromStart.Seconds, eventBlocks [0].TimeFromStart.Milliseconds);
					}
				}
			}
			
			protected override SubSetNode<AllocatedObject> NewSubSet (HeapItemSet<AllocatedObject> items) {
				return new AllocatedObjectSubSetNode (model, this, items);
			}
			
			public override Menu ContextMenu {
				get {
					if (items != null) {
						if (model.Explorer.NodeIsMarked) {
							if (model.Explorer.MarkIsForComparison) {
								return model.Explorer.CompareAllocationSet;
							} else if (model.Explorer.MarkIsForFiltering) {
								return model.Explorer.FilterAllocationSetUsingSelection;
							} else {
								throw new Exception ("Mark is buggy");
							}
						} else {
							return model.Explorer.FilterAllocationSet;
						}
					} else {
						return model.Explorer.LoadAllocationsBlocks;
					}
				}
			}
			
			public AllocationsNode (HeapExplorerTreeModel model, SeekableLogFileReader.Block[] eventBlocks, AllocationsNode previousAllocationsNode) : base (model, null, previousAllocationsNode) {
				this.eventBlocks = eventBlocks;
				this.items = null;
			}
		}
				
		public abstract class SubSetNode<HI> : Node<HI> where HI : IHeapItem {
			HeapItemSet<HI> items;
			public override HeapItemSet<HI> Items {
				get {
					return items;
				}
			}
			
			protected SubSetNode (HeapExplorerTreeModel model, Node<HI> parent, HeapItemSet<HI> items) : base (model, parent, null) {
				this.items = items;
			}
		}
		
		public class HeapObjectSubSetNode : SubSetNode<HeapObject> {
			protected override SubSetNode<HeapObject> NewSubSet (HeapItemSet<HeapObject> items) {
				return new HeapObjectSubSetNode (model, this, items);
			}
			
			public override Menu ContextMenu {
				get {
					if (model.Explorer.NodeIsMarked) {
						if (model.Explorer.MarkIsForComparison) {
							return model.Explorer.CompareObjectSet;
						} else if (model.Explorer.MarkIsForFiltering) {
							return model.Explorer.FilterObjectSetUsingSelection;
						} else {
							throw new Exception ("Mark is buggy");
						}
					} else {
						return model.Explorer.FilterObjectSet;
					}
				}
			}
			
			public HeapObjectSubSetNode (HeapExplorerTreeModel model, Node<HeapObject> parent, HeapItemSet<HeapObject> items) : base (model, parent, items) {
			}
		}
		
		public class AllocatedObjectSubSetNode : SubSetNode<AllocatedObject> {
			protected override SubSetNode<AllocatedObject> NewSubSet (HeapItemSet<AllocatedObject> items) {
				return new AllocatedObjectSubSetNode (model, this, items);
			}
			
			public override Menu ContextMenu {
				get {
					if (model.Explorer.NodeIsMarked) {
						if (model.Explorer.MarkIsForComparison) {
							return model.Explorer.CompareAllocationSet;
						} else if (model.Explorer.MarkIsForFiltering) {
							return model.Explorer.FilterAllocationSetUsingSelection;
						} else {
							throw new Exception ("Mark is buggy");
						}
					} else {
						return model.Explorer.FilterAllocationSet;
					}
				}
			}
			
			public AllocatedObjectSubSetNode (HeapExplorerTreeModel model, Node<AllocatedObject> parent, HeapItemSet<AllocatedObject> items) : base (model, parent, items) {
			}
		}
		
		TreeStore model;
		public TreeStore Model {
			get {
				return model;
			}
		}
		
		SeekableLogFileReader reader;
		public SeekableLogFileReader Reader {
			get {
				return reader;
			}
		}
		
		protected class HeapEventProcessor : ProfilerEventHandler {
			HeapSnapshot lastHeapSnapshot = null;
			public HeapSnapshot LastHeapSnapshot {
				get {
					return lastHeapSnapshot;
				}
			}
			
			public override void HeapReportStart (HeapSnapshot snapshot) {
				lastHeapSnapshot = snapshot;
			}
			public override void HeapObjectUnreachable (LoadedClass c, uint size) {
				lastHeapSnapshot.HeapObjectUnreachable (c, size);
			}
			public override void HeapObjectReachable (HeapObject o) {
			}
			public override void HeapReportEnd (HeapSnapshot snapshot) {
				lastHeapSnapshot.InitializeBackReferences ();
			}
		}
		protected HeapEventProcessor heapEventProcessor;
		
		List<IRootNode> rootNodes;
		public IRootNode[] RootNodes {
			get {
				return rootNodes.ToArray ();
			}
		}
		
		HeapSnapshotExplorer explorer;
		public HeapSnapshotExplorer Explorer {
			get {
				return explorer;
			}
		}
		
		AllocationsNode CreateAllocationsNode (List<SeekableLogFileReader.Block> eventBlocks, AllocationsNode previousAllocationsNode) {
			AllocationsNode node;
			if (eventBlocks.Count > 0) {
				node = new AllocationsNode (this, eventBlocks.ToArray (), previousAllocationsNode);
				rootNodes.Add (node);
			} else {
				node = null;
			}
			eventBlocks.Clear ();
			return node;
		}
		
		public void Initialize () {
			List<SeekableLogFileReader.Block> eventBlocks = new List<SeekableLogFileReader.Block> ();
			AllocationsNode previousAllocationsNode = null;
			
			Reset ();
			foreach (SeekableLogFileReader.Block block in reader.Blocks) {
				if (block.Code == BlockCode.HEAP_DATA) {
					previousAllocationsNode = CreateAllocationsNode (eventBlocks, previousAllocationsNode);
					SnapshotNode node = new SnapshotNode (this, block, previousAllocationsNode);
					rootNodes.Add (node);
				} else if (block.Code == BlockCode.EVENTS) {
					eventBlocks.Add (block);
				} else if (block.Code == BlockCode.DIRECTIVES) {
					reader.ReadBlock (block).Decode (heapEventProcessor, reader);
				} else if ((block.Code == BlockCode.MAPPING) || (block.Code == BlockCode.LOADED)){
					reader.ReadBlock (block).Decode (heapEventProcessor, reader);
				} else if (block.Code == BlockCode.INTRO) {
					reader.ReadBlock (block).Decode (heapEventProcessor, reader);
				} else if (block.Code == BlockCode.END) {
					reader.ReadBlock (block).Decode (heapEventProcessor, reader);
				}
			}
			CreateAllocationsNode (eventBlocks, previousAllocationsNode);
		}
		
		public void Reset () {
			model.Clear ();
		}
		
		public HeapExplorerTreeModel (SeekableLogFileReader reader, HeapSnapshotExplorer explorer) {
			model = new TreeStore (new Type [] {typeof (INode)});
			heapEventProcessor = new HeapEventProcessor ();
			this.reader = reader;
			this.explorer = explorer;
			rootNodes = new List<IRootNode> ();
		}
	}
}
