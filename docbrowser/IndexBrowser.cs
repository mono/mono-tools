using Gtk;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Xml;

using Mono.Options;

#if MACOS
using OSXIntegration.Framework;
#endif

namespace Monodoc {
class IndexBrowser {
	Browser browser;

	IndexReader index_reader;
	public BigList index_list;
	public MatchModel match_model;
	public BigList match_list;
	IndexEntry current_entry = null;
	

	public static IndexBrowser MakeIndexBrowser (Browser browser)
	{
		IndexReader ir = browser.help_tree.GetIndex ();
		if (ir == null) {
			return new IndexBrowser (browser);
		}

		return new IndexBrowser (browser, ir);
	}

	ProgressPanel ppanel;
	IndexBrowser (Browser parent)
	{
			browser = parent;
			ppanel = new ProgressPanel ("<b>No index found</b>", "Generate", RootTree.MakeIndex, NewIndexCreated); 
			browser.index_vbox.Add (ppanel);
			browser.index_vbox.Show ();
	}

	void NewIndexCreated ()
	{
		index_reader = browser.help_tree.GetIndex ();
		//restore widgets
		browser.index_vbox.Remove (ppanel);
		CreateWidget ();
		browser.index_vbox.ShowAll ();
	}
	
	IndexBrowser (Browser parent, IndexReader ir)
	{
		browser = parent;
		index_reader = ir;

		CreateWidget ();
	}

	void CreateWidget () {
		//
		// Create the widget
		//
		Frame frame1 = new Frame ();
		VBox vbox1 = new VBox (false, 0);
		frame1.Add (vbox1);

		// title
		HBox hbox1 = new HBox (false, 3);
		hbox1.BorderWidth = 3;
		Image icon = new Image (Stock.Index, IconSize.Menu);
		Label look_for_label = new Label ("Look for:");
		look_for_label.Justify = Justification.Left;
		look_for_label.Xalign = 0;
		hbox1.PackEnd (look_for_label, true, true, 0);
		hbox1.PackEnd (icon, false, true, 0);
		hbox1.ShowAll ();
		vbox1.PackStart (hbox1, false, true, 0);

		// entry
		vbox1.PackStart (new HSeparator (), false, true, 0);
		browser.index_entry = new Entry ();
		browser.index_entry.Activated += browser.OnIndexEntryActivated;
		browser.index_entry.Changed += browser.OnIndexEntryChanged;
		browser.index_entry.FocusInEvent += browser.OnIndexEntryFocused;
		browser.index_entry.KeyPressEvent += browser.OnIndexEntryKeyPress;
		vbox1.PackStart (browser.index_entry, false, true, 0);
		vbox1.PackStart (new HSeparator (), false, true, 0);

		//search results
		browser.search_box = new VBox ();
		vbox1.PackStart (browser.search_box, true, true, 0);
		vbox1.ShowAll ();

		
		//
		// Setup the widget
		//
		index_list = new BigList (index_reader);
		//index_list.SetSizeRequest (100, 400);

		index_list.ItemSelected += new ItemSelected (OnIndexSelected);
		index_list.ItemActivated += new ItemActivated (OnIndexActivated);
		HBox box = new HBox (false, 0);
		box.PackStart (index_list, true, true, 0);
		Scrollbar scroll = new VScrollbar (index_list.Adjustment);
		box.PackEnd (scroll, false, false, 0);
		
		browser.search_box.PackStart (box, true, true, 0);
		box.ShowAll ();

		//
		// Setup the matches.
		//
		browser.matches = new Frame ();
		match_model = new MatchModel (this);
		browser.matches.Hide ();
		match_list = new BigList (match_model);
		match_list.ItemSelected += new ItemSelected (OnMatchSelected);
		match_list.ItemActivated += new ItemActivated (OnMatchActivated);
		HBox box2 = new HBox (false, 0);
		box2.PackStart (match_list, true, true, 0);
		Scrollbar scroll2 = new VScrollbar (match_list.Adjustment);
		box2.PackEnd (scroll2, false, false, 0);
		box2.ShowAll ();
		
		browser.matches.Add (box2);
		index_list.SetSizeRequest (100, 200);

		browser.index_vbox.PackStart (frame1, true, true, 0);
		browser.index_vbox.PackEnd (browser.matches, true, true, 0);
	}

	//
	// This class is used as an implementation of the IListModel
	// for the matches for a given entry.
	// 
	public class MatchModel : IListModel {
		IndexBrowser index_browser;
		Browser browser;
		
		public MatchModel (IndexBrowser parent)
		{
			index_browser = parent;
			browser = parent.browser;
		}
		
		public int Rows {
			get {
				if (index_browser.current_entry != null)
					return index_browser.current_entry.Count;
				else
					return 0;
			}
		}

		public string GetValue (int row)
		{
			Topic t = index_browser.current_entry [row];
			
			// Names from the ECMA provider are somewhat
			// ambigious (you have like a million ToString
			// methods), so lets give the user the full name
			
			// Filter out non-ecma
			if (t.Url [1] != ':')
				return t.Caption;
			
			switch (t.Url [0]) {
				case 'C': return t.Url.Substring (2) + " constructor";
				case 'M': return t.Url.Substring (2) + " method";
				case 'P': return t.Url.Substring (2) + " property";
				case 'F': return t.Url.Substring (2) + " field";
				case 'E': return t.Url.Substring (2) + " event";
				default:
					return t.Caption;
			}
		}

		public string GetDescription (int row)
		{
			return GetValue (row);
		}
		
	}

	void ConfigureIndex (int index)
	{
		current_entry = index_reader.GetIndexEntry (index);

		if (current_entry.Count > 1){
			browser.matches.Show ();
			match_list.Reload ();
			match_list.Refresh ();
		} else {
			browser.matches.Hide ();
		}
	}
	
	//
	// When an item is selected from the main index list
	//
	void OnIndexSelected (int index)
	{
		ConfigureIndex (index);
		if (browser.matches.Visible == true)
			match_list.Selected = 0;
	}

	void OnIndexActivated (int index)
	{
		if (browser.matches.Visible == false)
			browser.LoadUrl (current_entry [0].Url);
	}

	void OnMatchSelected (int index)
	{
	}

	void OnMatchActivated (int index)
	{
		browser.LoadUrl (current_entry [index].Url);
	}

	int FindClosest (string text)
	{
		int low = 0;
		int top = index_reader.Rows-1;
		int high = top;
		bool found = false;
		int best_rate_idx = Int32.MaxValue, best_rate = -1;
		
		while (low <= high){
			int mid = (high + low) / 2;

			//Console.WriteLine ("[{0}, {1}] -> {2}", low, high, mid);

			string s;
			int p = mid;
			for (s = index_reader.GetValue (mid); s [0] == ' ';){
				if (p == high){
					if (p == low){
						if (best_rate_idx != Int32.MaxValue){
							//Console.WriteLine ("Bestrated: "+best_rate_idx);
							//Console.WriteLine ("Bestrated: "+index_reader.GetValue(best_rate_idx));
							return best_rate_idx;
						} else {
							//Console.WriteLine ("Returning P="+p);
							return p;
						}
					}
					
					high = mid;
					break;
				}

				if (p < 0)
					return 0;

				s = index_reader.GetValue (++p);
				//Console.WriteLine ("   Advancing to ->"+p);
			}
			if (s [0] == ' ')
				continue;
			
			int c, rate;
			c = Rate (text, s, out rate);
			//Console.WriteLine ("[{0}] Text: {1} at {2}", text, s, p);
			//Console.WriteLine ("     Rate: {0} at {1}", rate, p);
			//Console.WriteLine ("     Best: {0} at {1}", best_rate, best_rate_idx);
			//Console.WriteLine ("     {0} - {1}", best_rate, best_rate_idx);
			if (rate >= best_rate){
				best_rate = rate;
				best_rate_idx = p;
			}
			if (c == 0)
				return mid;

			if (low == high){
				//Console.WriteLine ("THISPATH");
				if (best_rate_idx != Int32.MaxValue)
					return best_rate_idx;
				else
					return low;
			}

			if (c < 0){
				high = mid;
			} else {
				if (low == mid)
					low = high;
				else
					low = mid;
			}
		}

		//		Console.WriteLine ("Another");
		if (best_rate_idx != Int32.MaxValue)
			return best_rate_idx;
		else
			return high;

	}

	int Rate (string user_text, string db_text, out int rate)
	{
		int c = String.Compare (user_text, db_text, true);
		if (c == 0){
			rate = 0;
			return 0;
		}

		int i;
		for (i = 0; i < user_text.Length; i++){
			if (db_text [i] != user_text [i]){
				rate = i;
				return c;
			}
		}
		rate = i;
		return c;
	}
	
	public void SearchClosest (string text)
	{
		index_list.Selected = FindClosest (text);
	}

	public void LoadSelected ()
	{
		if (browser.matches.Visible == true) {
			if (match_list.Selected != -1)
				OnMatchActivated (match_list.Selected);
		} else {
			if (index_list.Selected != -1)
				OnIndexActivated (index_list.Selected);
		}
	}
}
}
