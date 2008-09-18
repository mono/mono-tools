// MainWindow.cs created with MonoDevelop
// User: lupus at 12:08 PM 9/18/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using Gtk;
using Cairo;
using System.Collections.Generic;

public partial class MainWindow: Gtk.Window
{

	bool timeout_active = false;
	ArrayList clist = new ArrayList ();

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	void AddCounter (string cat, string counter, string instance)
	{
		CounterDisplay d = new CounterDisplay (this, cat, counter, instance);
		graph_vbox.PackStart (d, false, false, 6);
		clist.Add (d);
		//Console.WriteLine ("Added: {0}/{1}", cat, counter);
		if (!timeout_active) {
			GLib.Timeout.Add (1000, OnTimeout);
			timeout_active = true;
		}
	}

	public void RemoveCounter (CounterDisplay d)
	{
		clist.Remove (d);
		graph_vbox.Remove (d);
	}
	
	bool OnTimeout ()
	{

		foreach (CounterDisplay d in clist) {
			d.Update ();
		}
		return true;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected virtual void AddCounter (object sender, System.EventArgs e)
	{
		mperfmon.NewCounter cdialog = new mperfmon.NewCounter ();
		int res = cdialog.Run ();

		if (res == (int)ResponseType.Ok) {
			AddCounter (cdialog.Category, cdialog.Counter, cdialog.Instance);
		} else {
		}
		cdialog.Destroy ();
		
	}

	protected virtual void OnQuit (object sender, System.EventArgs e)
	{
		Application.Quit ();
	}
	
}

public class CounterDisplay : Frame
{
	Label rawval, error, type;
	PerformanceCounter countero;
	CounterDrawing draw;
	MainWindow w;
	
	public CounterDisplay (MainWindow win, string cat, string counter, string instance): base (string.Format ("{0}/{1}", cat, counter)) {
		w = win;
		HBox hbox = new HBox (false, 6);
		VBox vbox = new VBox (false, 6);
		hbox.PackStart (vbox, false, false, 4);
		Label l = new Label (string.Format ("Instance: {0}", instance));
		l.Xalign = 0;
		vbox.PackStart (l, false, false, 0);
		rawval = new Label ("");
		rawval.Xalign = 0;
		vbox.PackStart (rawval, false, false, 0);
		error = new Label ("");
		error.Xalign = 0;
		vbox.PackStart (error, false, false, 0);
		type = new Label ("");
		type.Xalign = 0;
		vbox.PackStart (type, false, false, 0);
		draw = new CounterDrawing ();
		hbox.PackEnd (draw, true, true, 4);
		Add (hbox);
		Button rem = new Button ("Remove");
		vbox.PackStart (rem, false, false, 0);
		rem.Clicked += delegate {
			w.RemoveCounter (this);
		};
		ShowAll ();
		try {
			if (instance == null)
				countero = new PerformanceCounter (cat, counter);
			else
				countero = new PerformanceCounter (cat, counter, instance);
			type.Text = countero.CounterType.ToString ();
			Update ();
			//Console.WriteLine ("{0}", countero.RawValue);
			//Console.WriteLine ("'{0}' '{1}' '{3}': {2}", cat, counter, countero.RawValue, instance);
		} catch (Exception e) {
			error.Text = e.Message;
			Console.WriteLine (e.StackTrace);
		}
	}

	public void Update ()
	{
		try {
			//Console.WriteLine (countero.RawValue);
			float v = countero.NextValue();
			draw.Add (v);
			//rawval.Text = string.Format ("Value: {0} (raw: {1})", v, countero.RawValue);
			rawval.Text = string.Format ("Value: {0:.00}", v);
		} catch (Exception e) {
			error.Text = e.Message;
			Console.WriteLine (e.StackTrace);
		}
	}
}

class CounterDrawing : DrawingArea
{
	List<float> values = new List<float> ();
	double scale_y = 1;
	double max_y = 0;
	int first_value = 0;

	public void Add (float val)
	{
		max_y = Math.Max (max_y, val);
		values.Add (val);
		QueueDraw();
	}

	void draw_grid (Cairo.Context gr, int w, int h) {
		double y_interval, y_spacing;
		double pixels = 80;
		double num_ticks = h/pixels;
		double interval = max_y/num_ticks;
		double min_size = 1;

		while (true) {
			double new_size = min_size * 10;
			if (new_size < interval) {
				min_size = new_size;
				continue;
			}
			break;
		}
		y_spacing = pixels * min_size/interval;
		y_interval = min_size;
			
		gr.Color = new Color (0.4, 0.4, 0.4, 1);
		gr.LineWidth = .5;
		double y_label = 0;

		for (double i = h; i >= 0; i -= y_spacing) {
			gr.MoveTo (0, i);
			gr.LineTo (w, i);
			gr.Stroke ();
			gr.LineTo (5, i);
			gr.ShowText (y_label.ToString ());
			y_label += y_interval;
		}
	}
	
	void draw (Cairo.Context gr) {
		if (values.Count < 2)
			return;
		gr.Color = new Color (1, 0.1, 0.1, 1);
		gr.LineWidth = 1;
		gr.MoveTo (0, (max_y - values [first_value + 0]) * scale_y);
		for (int i = 1; first_value + i < values.Count; ++i) {
			gr.LineTo (i, (max_y - values [first_value + i]) * scale_y);
		}
		gr.Stroke ();
		
	}

	protected override bool OnExposeEvent (Gdk.EventExpose args) {
		Gdk.Window win = args.Window;
		using (Cairo.Context gr = Gdk.CairoHelper.Create (win)) {
			int x, y, w, h, d;
			win.GetGeometry (out x, out y, out w, out h, out d);
			scale_y = h/max_y;
			
			gr.Rectangle (0, 0, w, h);
			gr.SetSourceRGB (0.2, 0.2, 0.2);
			gr.Fill ();
			draw_grid (gr, w, h);
			if (w < values.Count)
				first_value = values.Count - w;
			draw (gr);
		}
		return true;
	}

}
