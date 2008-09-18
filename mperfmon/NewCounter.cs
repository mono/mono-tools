// NewCounter.cs created with MonoDevelop
// User: lupus at 12:15 PM 9/18/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Diagnostics;
using Gtk;

namespace mperfmon
{
	
	
	public partial class NewCounter : Gtk.Dialog
	{
		PerformanceCounterCategory[] cats;
		TreeStore counters_store;
		TreeStore instances_store;
		string category;
		string counter;
		string instance;

		public string Category {
			get {return category;}
		}

		public string Counter {
			get {return counter;}
		}

		public string Instance {
			get {return instance;}
		}

		public NewCounter()
		{
			this.Build();
			cats = PerformanceCounterCategory.GetCategories ();
			for (int i = 0; i < cats.Length; ++i) {
				categories.AppendText (cats [i].CategoryName);
			}
			CellRendererText renderer = new CellRendererText ();
			counters.AppendColumn ("Name", renderer, "text", 0);
			CellRendererText renderer2 = new CellRendererText ();
			instances.AppendColumn ("Instance", renderer2, "text", 0);
			counters_store = new TreeStore (typeof (string));
			instances_store = new TreeStore (typeof (string));
			counters.Model = counters_store;
			instances.Model = instances_store;
			counters.Selection.Changed += delegate {
				TreeSelection ts = counters.Selection;
				TreeIter iter;
				TreeModel mod;
				if (ts.GetSelected (out mod, out iter)) {
					counter = mod.GetValue (iter, 0) as string;
				}
			};
			instances.Selection.Changed += delegate {
				TreeSelection ts = instances.Selection;
				TreeIter iter;
				TreeModel mod;
				if (ts.GetSelected (out mod, out iter)) {
					instance = mod.GetValue (iter, 0) as string;
				}
			};
			if (cats.Length > 0)
				categories.Active = 0;
		}

		protected virtual void OnCategorySelected (object sender, System.EventArgs e)
		{
			PerformanceCounterCategory cat = cats [categories.Active];
			category = cat.CategoryName;
			counters_store.Clear ();
			instances_store.Clear ();
			try {
				foreach (PerformanceCounter c in cat.GetCounters ()) {
					counters_store.AppendValues (c.CounterName);
					//Console.WriteLine (c.CounterName);
				}
			} catch {}
			try {
				foreach (string s in cat.GetInstanceNames ()) {
					instances_store.AppendValues (s);
					//Console.WriteLine (c.CounterName);
				}
			} catch {}
		}

		protected virtual void OnCounterRow (object o, Gtk.RowActivatedArgs args)
		{
		}
	}
}
