using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinDoc {

	public abstract class PageVisit {
		public abstract void Go ();
	}
	
	delegate void SetSensitive (bool state);
	
	public class History {
		ToolStripButton backButton;
		ToolStripButton forwardButton;
	
		int pos = -1;
		List<PageVisit> history = new List<PageVisit> ();
		
		public int Count {
			get { return history.Count; }
		}
		
		public bool Active {
			get {
				return active;
			}
			set {
				if (value) {
					backButton.Enabled = pos > 0;
					forwardButton.Enabled = pos+1 == history.Count;					
					active = value;
				}
			}
		}
		bool active, ignoring;
		
		public History (ToolStripButton back, ToolStripButton forward)
		{
			this.backButton = back;
			this.forwardButton = forward;
			active = true;
			
			back.Click += (s, e) => BackClicked ();
			forward.Click += (s, e) => ForwardClicked ();
			back.Enabled = forward.Enabled = false;							
		}

		internal bool BackClicked ()
		{
			if (!active || pos < 1)
				return false;
			pos--;
			PageVisit p = (PageVisit) history [pos];
			ignoring = true;
			p.Go ();
			ignoring = false;
			backButton.Enabled = pos > 0;
			forwardButton.Enabled = true;
			return true;
		}
	
		internal bool ForwardClicked ()
		{
			if (!active || pos+1 == history.Count)
				return false;
			pos++;
			var pageVisit = history [pos];
			ignoring = true;
			pageVisit.Go ();
			ignoring = false;
			backButton.Enabled = true;
			forwardButton.Enabled = pos + 1 < history.Count;
			return true;
		}

		public void AppendHistory (PageVisit page)
		{
			if (ignoring)
				return;
			pos++;
			if (history.Count <= pos)
				history.Add (page);
			else
				history [pos] = page;

			backButton.Enabled = pos > 0;
			forwardButton.Enabled = false;
		}
	
		public void ActivateCurrent ()
		{
			if (pos < 0)
				return;
			var pageVisit = history [pos];
			pageVisit.Go ();
		}
	}
}
