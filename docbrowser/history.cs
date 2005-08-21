namespace Monodoc {
	
using Gtk;
using System;
using System.Collections;

public abstract class PageVisit {

	public abstract void Go ();
}

delegate void SetSensitive (bool state);

public class History {
	Gtk.Widget back, forward;

	int pos = -1;
	ArrayList history;
	public int Count {
		get { return history.Count; }
	}
	
	public bool Active {
		get 
		{
			return active;
		}
		set 
		{
			if(value) {
				if (pos > 0)
					back.Sensitive = true;
				else
					back.Sensitive = false;
				if (pos+1 == history.Count)
					forward.Sensitive = false;
				else
					forward.Sensitive = true;
			}
			active = value;
		}
	}
	private bool active;
	
	public History (Gtk.Button back, Gtk.Button forward)
	{
		this.back = back;
		this.forward = forward;

		back.Sensitive = false;
		forward.Sensitive = false;

		back.Clicked += new EventHandler (BackClicked);
		forward.Clicked += new EventHandler (ForwardClicked);
		
		history = new ArrayList ();
	}

	public void AppendHistory (PageVisit page)
	{
		pos++;
		if (history.Count <= pos)
			history.Add (page);
		else
			history [pos] = page;

		if (pos > 0)
			back.Sensitive = true;
		forward.Sensitive = false;
	}

	public void ActivateCurrent ()
	{
		if (pos < 0)
			return;
		PageVisit p = (PageVisit) history [pos];
		p.Go ();
	}

	internal void BackClicked (object o, EventArgs args)
	{
		if (!active)
			return;
		if (pos < 1)
			return;
		pos--;
		PageVisit p = (PageVisit) history [pos];
		p.Go ();
		if (pos > 0)
			back.Sensitive = true;
		else
			back.Sensitive = false;
		forward.Sensitive = true;
	}

	internal void ForwardClicked (object o, EventArgs args)
	{
		if (!active)
			return;
		if (pos+1 == history.Count)
			return;

		pos++;
		PageVisit p = (PageVisit) history [pos];
		p.Go ();
		if (pos+1 == history.Count)
			forward.Sensitive = false;
		back.Sensitive = true;
	}

}
}
