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
		public abstract class Node {
			public abstract HeapObjectSet Objects {
				get;
			}
			public abstract string Description {
				get;
			}
			
			protected HeapExplorerTreeModel model;
			TreeIter treeIter;
			public TreeIter TreeIter {
				get {
					return treeIter;
				}
			}
			
			Node parent;
			public Node Parent {
				get {
					return parent;
				}
			}
			
			public Node Root {
				get {
					if (parent != null) {
						return parent.Root;
					} else {
						return this;
					}
				}
			}
			
			public string Count {
				get {
					if (Objects != null) {
						return Objects.HeapObjects.Length.ToString ();
					} else {
						return "";
					}
				}
			}
			public string AllocatedBytes {
				get {
					if (Objects != null) {
						return Objects.AllocatedBytes.ToString ();
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
			
			public SubSetNode Filter (IHeapObjectFilter filter) {
				HeapObjectSetFromFilter subSet = new HeapObjectSetFromFilter (Objects, filter);
				SubSetNode result = new SubSetNode (model, this, subSet);
				return result;
			}
			
			public static void PerformComparison (Node firstNode, Node secondNode, out SubSetNode onlyInFirstNode, out SubSetNode onlyInSecondNode) {
				HeapObjectSet onlyInFirstSet;
				HeapObjectSet onlyInSecondSet;
				HeapObjectSetFromComparison.PerformComparison (firstNode.Objects, secondNode.Objects, out onlyInFirstSet, out onlyInSecondSet);
				onlyInFirstNode = new SubSetNode (firstNode.model, firstNode, onlyInFirstSet);
				onlyInSecondNode = new SubSetNode (secondNode.model, secondNode, onlyInSecondSet);
			}
			
			protected Node (HeapExplorerTreeModel model, Node parent) {
				this.model = model;
				this.parent = parent;
				this.treeIter = HandleNodeCreation ();
			}
		}
		
		public class SnapshotNode : Node {
			SeekableLogFileReader.Block heapBlock;
			
			HeapSnapshot snapshot;
			public HeapSnapshot Snapshot {
				get {
					return snapshot;
				}
			}
			
			HeapObjectSetFromSnapshot objects;
			public override HeapObjectSet Objects {
				get {
					return objects;
				}
			}
			
			public void ReadSnapshot () {
				model.Reader.ReadBlock (heapBlock).Decode (model.heapEventProcessor, model.Reader);
				snapshot = model.heapEventProcessor.LastHeapSnapshot;
				objects = new HeapObjectSetFromSnapshot (snapshot);
				model.Model.SetValue (TreeIter, 0, this);
			}
			
			public override string Description {
				get {
					return heapBlock.TimeFromStart.ToString ();
				}
			}
			
			public SnapshotNode (HeapExplorerTreeModel model, SeekableLogFileReader.Block heapBlock) : base (model, null) {
				this.heapBlock = heapBlock;
				this.objects = null;
				this.snapshot = null;
			}
		}
		
		public class SubSetNode : Node {
			HeapObjectSet objects;
			public override HeapObjectSet Objects {
				get {
					return objects;
				}
			}
			
			public override string Description {
				get {
					return objects.ShortDescription;
				}
			}
			
			public SubSetNode (HeapExplorerTreeModel model, Node parent, HeapObjectSet objects) : base (model, parent) {
				this.objects = objects;
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
			public override void HeapObjectReachable (HeapObject<LoadedClass> o) {
			}
			public override void HeapReportEnd (HeapSnapshot snapshot) {
				lastHeapSnapshot.InitializeBackReferences ();
			}
		}
		protected HeapEventProcessor heapEventProcessor;
		
		List<SnapshotNode> rootNodes;
		public SnapshotNode[] RootNodes {
			get {
				return rootNodes.ToArray ();
			}
		}
		
		public void Initialize () {
			Reset ();
			
			foreach (SeekableLogFileReader.Block block in reader.Blocks) {
				if (block.Code == BlockCode.HEAP_DATA) {
					SnapshotNode node = new SnapshotNode (this, block);
					rootNodes.Add (node);
				} else if (block.Code == BlockCode.MAPPING) {
					reader.ReadBlock (block).Decode (heapEventProcessor, reader);
				}
			}
		}
		
		public void Reset () {
			model.Clear ();
		}
		
		public HeapExplorerTreeModel (SeekableLogFileReader reader) {
			model = new TreeStore (new Type [] {typeof (Node)});
			heapEventProcessor = new HeapEventProcessor ();
			this.reader = reader;
			rootNodes = new List<SnapshotNode> ();
		}
	}
}
