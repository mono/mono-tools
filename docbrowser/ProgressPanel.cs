//
// ProgressPanel.cs: A panel with a progress bar and a button
//
// Author: Mario Sopena
// 

using System;
using Gtk;
using System.Threading;
 
namespace Monodoc {
	
class ProgressPanel : VBox {
	
	// Delegates called when starting and finishing
	public delegate void StartWorkDelegate ();
	public StartWorkDelegate StartWork;
	public delegate void FinishWorkDelegate ();
	public FinishWorkDelegate FinishWork;

	ProgressBar pb;
	ThreadNotify notify;
	uint timer; 

	public ProgressPanel (string message, string button, StartWorkDelegate StartWork, FinishWorkDelegate FinishWork)
	{
			Gtk.Label l = new Gtk.Label (message);
			l.UseMarkup = true;
			l.Show ();
			PackStart (l);
			
			pb = new ProgressBar ();
			pb.Show ();
			PackEnd (pb, false, false, 3);

			Button b = new Button (button);
			b.Show ();
			b.Clicked += new EventHandler (OnStartWorking);
			PackEnd (b, false, false, 3);

			this.StartWork = StartWork;
			this.FinishWork = FinishWork;
	}

	void OnStartWorking (object sender, EventArgs a)
	{
		Button b = (Button) sender;
		b.Sensitive = false;
		// start a timer to update the progress bar
		timer = Gtk.Timeout.Add ( (uint) 100, new Function (DoUpdateProgressbar));
		
		Thread thr = new Thread (new ThreadStart (Work));
	    thr.Start ();
		notify = new ThreadNotify (new ReadyEvent (Finished));
		
	}

	void Work ()
	{
		StartWork ();
		notify.WakeupMain ();
	}
	
	void Finished ()
	{
		Gtk.Timeout.Remove (timer);
		FinishWork ();
	}

	bool DoUpdateProgressbar ()
	{
		pb.Pulse ();
		return true;
	}
}
}
