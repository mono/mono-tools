//
// list.cs: A list widget that scales for lots of data.
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Alp Toker (alp@atoker.com)
//
// (C) 2003 Ximian, Inc.
//
namespace Monodoc {
using Gdk;
using Gtk;
using System;
using System.Collections;

delegate void ItemSelected (int index);
delegate void ItemActivated (int index);

class BigList : Gtk.DrawingArea {
	Gtk.Adjustment adjustment;
	int top_displayed;
	Style my_style;
	Widget style_widget;
	Pango.Layout layout;
	IListModel provider;
	const string ellipsis = "...";
	Hashtable ellipses = new Hashtable ();
	int old_width;
	int ellipsis_width, en_width, line_height;
	int selected_line = -1;
	int rows = 1;
	bool is_dragging = false;
	bool reloaded = true;
	public bool CanDeselect = false;
	int old_selected;
	
	public BigList (IListModel provider)
	{
		this.provider = provider;

		//Accessibility
		RefAccessible ().Role = Atk.Role.List;

		adjustment = new Gtk.Adjustment (0, 0, provider.Rows, 1, 1, 1);
		adjustment.ValueChanged += new EventHandler (ValueChangedHandler);

		layout = new Pango.Layout (PangoContext);

		ExposeEvent += new ExposeEventHandler (ExposeHandler);
		ButtonPressEvent += new ButtonPressEventHandler (ButtonPressEventHandler);
		ButtonReleaseEvent += new ButtonReleaseEventHandler (ButtonReleaseEventHandler);
		KeyPressEvent += new KeyPressEventHandler (KeyHandler);
		Realized += new EventHandler (RealizeHandler);
		Unrealized += new EventHandler (UnrealizeHandler);
		ScrollEvent += new ScrollEventHandler (ScrollHandler);
                SizeAllocated += new SizeAllocatedHandler (SizeAllocatedHandler);
		MotionNotifyEvent += new MotionNotifyEventHandler (MotionNotifyEventHandler);

		AddEvents ((int) EventMask.ButtonPressMask | (int) EventMask.ButtonReleaseMask | (int) EventMask.KeyPressMask | (int) EventMask.PointerMotionMask);
		CanFocus = true;

		style_widget = new EventBox ();
		style_widget.StyleSet += new StyleSetHandler (StyleHandler);

		//
		// Compute the height and ellipsis width of the font,
		// and the en_width for our ellipsizing algorithm
		//
		layout.SetMarkup (ellipsis);
		layout.GetPixelSize (out ellipsis_width, out line_height);

		layout.SetMarkup ("n");
		layout.GetPixelSize (out en_width, out line_height);
		
		layout.SetMarkup ("W");
		layout.GetPixelSize (out en_width, out line_height);

		old_width = Allocation.Width;
	}

	void MotionNotifyEventHandler (object o, MotionNotifyEventArgs args)
	{
		if (!is_dragging)
			return;

		int x, y;
		GetPointer (out x, out y);
		Selected = HitTest (y);
	}

	void SizeAllocatedHandler (object obj, SizeAllocatedArgs args)
	{
                Gdk.Rectangle rect = args.Allocation;
                if (rect.Equals (Gdk.Rectangle.Zero))
                        Console.WriteLine ("ERROR: Allocation is null!");

		int nrows = Allocation.Height / line_height;
		if (nrows != rows){
			rows = nrows;
			Reload ();
		}

		if (args.Allocation.Width != old_width)
			ellipses.Clear ();

		old_width = args.Allocation.Width;
	}

	void StyleHandler (object obj, StyleSetArgs args)
	{
		if (Style == my_style)
			return;

		my_style = style_widget.Style.Copy ();
		Style = my_style;
		my_style.SetBackgroundGC (StateType.Normal, Style.BaseGC (StateType.Normal));
		Refresh ();
	}

	void RealizeHandler (object o, EventArgs sender)
	{
	}

	void UnrealizeHandler (object o, EventArgs sender)
	{
	}
	
	void ValueChangedHandler (object obj, EventArgs e)
	{
		top_displayed = (int) Adjustment.Value;
		Refresh ();
	}

	void RedrawLine (int line)
	{
		QueueDrawArea (0, line * line_height, Allocation.Width, (line+1) * line_height);
	}

	public void Reload ()
	{
		adjustment.SetBounds (0, provider.Rows, 1, rows, rows);
		ellipses.Clear ();
		reloaded = true;
	}

	public void Refresh ()
	{
		QueueDrawArea (0, 0, Allocation.Width, Allocation.Height);
	}

	void ScrollHandler (object o, ScrollEventArgs args)
	{
		Gdk.EventScroll es = args.Event;
                double newloc = 0.0;
		int steps = Math.Max (rows / 6, 2);
		
                switch (es.Direction){
                case ScrollDirection.Up:
                        newloc = adjustment.Value - steps;
                        break;
                        
                case ScrollDirection.Down:
                        newloc = adjustment.Value + steps;
                        break;
                }

		newloc = Math.Max (newloc, 0);
		newloc = Math.Min (newloc, provider.Rows - rows);

                adjustment.Value = newloc;
	}

	void KeyHandler (object o, KeyPressEventArgs args)
	{
		args.RetVal = true;
		
		switch (args.Event.Key) {
			case Gdk.Key.Down:
				Selected++;
				break;
			case Gdk.Key.Up:
				Selected--;
				break;
			case Gdk.Key.Page_Down:
				Selected += rows - 1;
				break;
			case Gdk.Key.Page_Up:
				Selected -= rows - 1;
				break;
			case Gdk.Key.End:
				Selected = provider.Rows;
				break;
			case Gdk.Key.Home:
				Selected = 0;
				break;
			case Gdk.Key.Return:
				if (ItemActivated != null)
					ItemActivated (Selected);
				break;
			default:
				args.RetVal = false;
				break;
		}
	}

	uint last_click_time = 0;

	void ButtonPressEventHandler (object o, ButtonPressEventArgs a)
	{
		if (a.Event.Type != Gdk.EventType.ButtonPress)
			return;

		if (a.Event.Button == 1)
			is_dragging = true;

		HasFocus = true;
		Gdk.EventButton pos = a.Event;
		int new_selected = HitTest (pos.Y);

		int double_click_time = Settings.DoubleClickTime;

		if ((new_selected == Selected) && (a.Event.Time - last_click_time <= double_click_time)) {
			Selected = new_selected;
			Grab.Remove (this);
			if (ItemActivated != null)
				ItemActivated (Selected);
		} else
			Selected = new_selected;

		last_click_time = a.Event.Time;
		a.RetVal = true;
	}

	void ButtonReleaseEventHandler (object o, ButtonReleaseEventArgs a)
	{
		if (a.Event.Button == 1)
			is_dragging = false;

		old_selected = Selected;
	}

	int HitTest (double y)
	{
		int line = (int) y / line_height;
		if (line + top_displayed > provider.Rows - 1)
			return -2;
		return (line + top_displayed);
	}

	public int Selected {
		get {
			return selected_line;
		}
		set {
			int selected = value;

			if (selected != -2) {
				selected = Math.Max (selected, 0);
				selected = Math.Min (selected, provider.Rows - 1);
			}

			if (selected == Selected && reloaded == false)
				return;

			reloaded = false;

			if (selected_line != -1)
				RedrawLine (selected_line - top_displayed);

			if (selected == -2) {
				if (CanDeselect) {
					//Translate the selection number
					selected_line = -1;
					if (ItemSelected != null) ItemSelected (-1);
					return;
				} else return;
			}
			
			if (selected < top_displayed){
				top_displayed = selected;
				adjustment.Value = top_displayed;
				Refresh ();
			} else if (selected >= (top_displayed + rows)){
				top_displayed = selected - rows + 1;
				top_displayed = Math.Max (top_displayed, 0);
				adjustment.Value = top_displayed;
				Refresh ();
			} else
				RedrawLine (selected - top_displayed);

			selected_line = selected;
			if (ItemSelected != null)
				ItemSelected (selected);
		}
	}
	
	public event ItemSelected ItemSelected;
	
	public event ItemActivated ItemActivated;
	
        void ExposeHandler (object obj, ExposeEventArgs args)
        {
                Gdk.Window win = args.Event.Window;
                Gdk.Rectangle area = args.Event.Area;

		win.DrawRectangle (Style.BaseGC (StateType.Normal), true, area);

		for (int y = 0, row = top_displayed; y < area.Y + area.Height; y += line_height, row++){

			Gdk.GC gc;
			Gdk.Rectangle region_area = new Gdk.Rectangle (0, y, Allocation.Width, line_height);
			StateType state = StateType.Normal;

			if (row == selected_line) {
				if (HasFocus)
					state = StateType.Selected;
				else
					state = StateType.Active;

				win.DrawRectangle (Style.BaseGC (state), true, region_area);
			}
			
			//FIXME: we have a ghost row at the end of the list!
			//Console.WriteLine (row + " " + provider.Rows);

			if (row >= provider.Rows)
				return;

			string text = "";
			if (ellipses [row] == null){
				text = provider.GetValue (row);
				text = ELabel.Ellipsize (layout, text, Allocation.Width - 2 * 2, ellipsis_width, en_width);
				ellipses [row] = text;
			} else
				text = (string) ellipses [row];
			layout.SetText (text);

			gc = Style.TextGC (state);
			win.DrawLayout (gc, 2, y, layout);
		}

		args.RetVal = true;
	}

	public Gtk.Adjustment Adjustment {
		get {
			return adjustment;
		}
	}
}
}
