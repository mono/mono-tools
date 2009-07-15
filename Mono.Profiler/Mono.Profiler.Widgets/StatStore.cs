// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
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


using System;
using System.Collections.Generic;
using Mono.Profiler;

namespace Mono.Profiler.Widgets {
	
	internal class StatStore : ProfileStore {

		class StatNode : Node {
		
			static List<Node> children = new List<Node> ();
			IStatisticalHitItem item;
			
			public static Comparison<Node> CompareByHits = delegate (Node a, Node b) {
				return (b as StatNode).item.StatisticalHits.CompareTo ((a as StatNode).item.StatisticalHits);
			};

			public StatNode (ProfileStore store, Node parent, IStatisticalHitItem item) : base (store, parent)
			{
				this.item = item;
			}
			
			public override List<Node> Children {
				get { return children; }
			}
			
			public IStatisticalHitItem HitItem {
				get { return item; }
			}

			public override string Name {
				get { return item.Name; }
			}
			
			public override ulong Value {
				get { return item.StatisticalHits; }
			}
		}
		
		ulong total_hits;

		public StatStore (ProfilerEventHandler data, DisplayOptions options) : base (data, options)
		{
			nodes = new List<Node> ();
			foreach (IStatisticalHitItem item in data.StatisticalHitItems) {
				if (item.StatisticalHits <= 0)
					continue;
				total_hits += item.StatisticalHits;
				nodes.Add (new StatNode (this, null, item));
			}
			nodes.Sort (StatNode.CompareByHits);
		}

		public IStatisticalHitItem this [int index] {
			get { return (nodes [index] as StatNode).HitItem; }
		}

		public override void GetValue (Gtk.TreeIter iter, int column, ref GLib.Value val)
		{
			Node node = (Node) iter;
			if (column == 0)
				val = new GLib.Value (node.Name);
			else if (column == 1) {
				double percent = (double) node.Value / (double) total_hits * 100.0;
				val = new GLib.Value (String.Format ("{0,5:F2}%", percent));
			}
		}		
	}
}
