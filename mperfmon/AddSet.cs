// Author: lupus 9/23/2008
//
//

using System;
using System.Diagnostics;
using Gtk;

namespace mperfmon
{
	
	
	public partial class AddSet : Gtk.Dialog
	{
		string csetn;
		string instance;
		TreeStore instances_store;
		Config cfg;
		
		public AddSet(Config cfg)
		{
			this.cfg = cfg;
			this.Build();
			CellRendererText renderer = new CellRendererText ();
			instances.AppendColumn ("Instance", renderer, "text", 0);
			instances_store = new TreeStore (typeof (string));
			instances.Model = instances_store;
			for (int i = 0; i <  cfg.sets.Count; ++i) {
				counterset.AppendText (cfg.sets [i].Name);
			}
			instances.Selection.Changed += delegate {
				TreeSelection ts = instances.Selection;
				TreeIter iter;
				TreeModel mod;
				if (ts.GetSelected (out mod, out iter)) {
					instance = mod.GetValue (iter, 0) as string;
				}
			};
			if (cfg.sets.Count > 0) {
				counterset.Active = 0;
			}
		}

		protected virtual void OnSetSelected (object sender, System.EventArgs e)
		{
			instances_store.Clear ();
			CounterSet cset = cfg.sets [counterset.Active];
			csetn = cset.Name;
			try {
				// we take just the first counter category into consideration
				// to retrieve an instance
				PerformanceCounterCategory cat = new PerformanceCounterCategory (cset.Counters [0]);
				foreach (string s in cat.GetInstanceNames ()) {
					instances_store.AppendValues (s);
				}
			} catch {
			}
		}

		public string CounterSet {
			get {
				return csetn;
			}
		}

		public string Instance {
			get {
				return instance;
			}
		}
	}
}
