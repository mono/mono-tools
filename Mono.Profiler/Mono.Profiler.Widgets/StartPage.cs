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
	
	public delegate void StartEventHandler (object o, StartEventArgs args);

	public enum StartEventType {
		Create,
		Open,
		Repeat,
	}

	public class StartEventArgs : EventArgs {

		ProfileConfiguration config;
		StartEventType type;
		string detail;

		public StartEventArgs (StartEventType type, string detail)
		{
			this.type = type;
			this.detail = detail;
		}

		public StartEventArgs (ProfileConfiguration config)
		{
			this.type = StartEventType.Repeat;
			this.config = config;
		}

		public ProfileConfiguration Config {
			get { return config; }
		}

		public string Detail {
			get { return detail; }
		}

		public StartEventType Type {
			get { return type; }
		}
	}

	public class StartPage : Gtk.DrawingArea {

		const int padding = 8;
		const int text_padding = 3;

		bool pressed;
		BannerItem banner;
		History history;
		LinkItem prelight_item;
		LinkItem selected_item;
		List<Item> items = new List<Item> ();
		Pango.Layout layout;

		public StartPage (History history) 
		{
			this.history = history;
			history.Changed += delegate { Refresh (); };
			CanFocus = true;
			Events = Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.ExposureMask | Gdk.EventMask.KeyPressMask | Gdk.EventMask.PointerMotionMask;
			layout = CreatePangoLayout (String.Empty);
		}

		public event StartEventHandler Activated;

		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			if (ev.Button != 1)
				return base.OnButtonPressEvent (ev);

			Gdk.Point pt = new Gdk.Point ((int)ev.X, (int)ev.Y);

			foreach (Item item in items) {
				if (item is LinkItem && item.Bounds.Contains (pt)) {
					// button press animation
					pressed = true;
					SelectItem (item as LinkItem);
					break;
				}
			}
			GrabFocus ();
			return base.OnButtonPressEvent (ev);
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton ev)
		{
			pressed = false;
			if (selected_item == null)
				return base.OnButtonReleaseEvent (ev);

			Gdk.Point pt = new Gdk.Point ((int)ev.X, (int)ev.Y);

			if (selected_item.Bounds.Contains (pt))
				selected_item.OnActivated ();

			return base.OnButtonReleaseEvent (ev);
		}

		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);
			Paint (ev);
			return true;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey ev)
		{
			switch (ev.Key) {
			case Gdk.Key.Left:
				SelectPreviousItem ();
				break;
			case Gdk.Key.Right:
				SelectNextItem ();
				break;
			case Gdk.Key.Up:
				SelectPreviousItem ();
				break;
			case Gdk.Key.Down:
				SelectNextItem ();
				break;
			case Gdk.Key.KP_Enter:
			case Gdk.Key.ISO_Enter:
			case Gdk.Key.Return:
				if (selected_item != null)
					selected_item.OnActivated ();
				break;
			default:
				return false;
			}

			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion ev)
		{
			SelectItem  (null);

			Gdk.Point pt = new Gdk.Point ((int)ev.X, (int)ev.Y);

			if (prelight_item != null) {
				if (prelight_item.Bounds.Contains (pt))
					return true;
				else {
					LinkItem item = prelight_item;
					prelight_item = null;
					QueueDrawArea (item.Bounds.X, item.Bounds.Y, item.Bounds.Width, item.Bounds.Height);
				}
			}

			foreach (Item item in items) {
				if (item is LinkItem && item.Bounds.Contains (pt)) {
					prelight_item = item as LinkItem;
					QueueDrawArea (item.Bounds.X, item.Bounds.Y, item.Bounds.Width, item.Bounds.Height);
					break;
				}
			}

			return true;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			Layout ();
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			requisition.Width = 350;
			requisition.Height = 340;
		}

		abstract class Item {
			protected StartPage owner;
			public Gdk.Rectangle Bounds;

			protected Item (StartPage owner)
			{
				this.owner = owner;
			}

			public abstract void Draw (Gdk.Rectangle clip);

			public virtual void OnActivated ()
			{
			}
		}

		class BannerItem : Item {
			
			public BannerItem (StartPage owner) : base (owner) 
			{
				Bounds = new Gdk.Rectangle (0, 0, owner.Allocation.Width, 65);
			}

			public override void Draw (Gdk.Rectangle clip)
			{
				using (Cairo.Context ctx = Gdk.CairoHelper.Create (owner.GdkWindow)) {
					ctx.MoveTo (new Cairo.PointD (0, 0));
					ctx.LineTo (new Cairo.PointD (Bounds.Width, 0));
					ctx.LineTo (new Cairo.PointD (Bounds.Width, Bounds.Height));
					ctx.LineTo (new Cairo.PointD (0, Bounds.Height));
					ctx.ClosePath ();
					ctx.Save ();
					Cairo.Gradient grad = new Cairo.LinearGradient (Bounds.Width / 4, 0, Bounds.Width * 3 / 4, Bounds.Height);
					grad.AddColorStop (0, new Cairo.Color (0.0, 0.0, 0.5, 1.0));
					grad.AddColorStop (1, new Cairo.Color (0.0, 0.0, 1.0, 1.0));
					ctx.Pattern = grad;
					ctx.FillPreserve ();
					ctx.Restore ();
				}
				Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (null, "Monodevelop-logo.png");
				owner.GdkWindow.DrawPixbuf (null, pixbuf, 0, 0, padding, 0, -1, -1, Gdk.RgbDither.None, 0, 0);
				owner.layout.SetMarkup ("<b><span font_size=\"xx-large\" foreground=\"white\">Mono Visual Profiler</span></b>");
				Pango.Rectangle ink, log;
				owner.layout.GetPixelExtents (out ink, out log);
				owner.GdkWindow.DrawLayout (owner.Style.TextGC (Gtk.StateType.Normal), pixbuf.Width + 2 * padding - ink.X, (65 - ink.Height) / 2 - ink.Y, owner.layout);
			}
		}

		class LabelItem : Item {
			string markup;
			Gdk.Point text_offset;

			public LabelItem (StartPage owner, string label, int x, int y) : base (owner)
			{
				markup = "<span foreground=\"#000099\"><b><big>" + label + "</big></b></span>";
				owner.layout.SetMarkup (markup);
				Pango.Rectangle ink, log;
				owner.layout.GetPixelExtents (out ink, out log);
				Bounds = new Gdk.Rectangle (x + 30, y, ink.Width, ink.Height);
				text_offset = new Gdk.Point (ink.X, ink.Y);
			}

			public override void Draw (Gdk.Rectangle clip)
			{
				owner.layout.SetMarkup (markup);
				owner.GdkWindow.DrawLayout (owner.Style.TextGC (Gtk.StateType.Normal), Bounds.X - text_offset.X, Bounds.Y - text_offset.Y, owner.layout);
			}
		}

		class LinkItem : Item {
			public StartEventType Type;
			public string Detail;
			string markup;
			string description;
			Gdk.Point text_offset;
			Gdk.Point description_offset;

			protected LinkItem (StartPage owner, string caption, string description, int x, int y) : base (owner)
			{
				markup = "<span underline=\"single\" foreground=\"#0000FF\">" + caption + "</span>";
				this.description = description;
				owner.layout.SetMarkup (markup);
				Pango.Rectangle ink, log;
				owner.layout.GetPixelExtents (out ink, out log);
				text_offset = new Gdk.Point (padding - ink.X, padding - ink.Y);
				if (String.IsNullOrEmpty (description))
					Bounds = new Gdk.Rectangle (x + 40, y, ink.Width + 2 * padding, ink.Height + 2 * padding);
				else {
					int height = ink.Height + padding;
					int width = ink.Width;
					owner.layout.SetMarkup ("<i>" + description + "</i>");
					owner.layout.GetPixelExtents (out ink, out log);
					description_offset = new Gdk.Point (padding - ink.X, height + text_padding - ink.Y);
					Bounds = new Gdk.Rectangle (x + 40, y, width > ink.Width ? width + 2 * padding : ink.Width + 2 * padding, height + ink.Height + padding + text_padding);
				}
			}

			public LinkItem (StartPage owner, string caption, StartEventType type, string detail, int x, int y) : this (owner, caption, null, type, detail, x, y) {}

			public LinkItem (StartPage owner, string caption, string description, StartEventType type, string detail, int x, int y) : this (owner, caption, description, x, y)
			{
				Type = type;
				Detail = detail;
			}

			public override void Draw (Gdk.Rectangle clip)
			{
				if (owner.selected_item == this || owner.prelight_item == this)
					Gtk.Style.PaintBox (owner.Style, owner.GdkWindow, Gtk.StateType.Prelight, owner.pressed ? Gtk.ShadowType.Out : Gtk.ShadowType.In, clip, owner, "GtkButton", Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);

				owner.layout.SetMarkup (markup);
				owner.GdkWindow.DrawLayout (owner.Style.TextGC (Gtk.StateType.Normal), Bounds.X + text_offset.X, Bounds.Y + text_offset.Y, owner.layout);
				if (description != null) {
					owner.layout.SetMarkup ("<i>" + description + "</i>");
					owner.GdkWindow.DrawLayout (owner.Style.TextGC (Gtk.StateType.Normal), Bounds.X + description_offset.X, Bounds.Y + description_offset.Y, owner.layout);
				}
			}
			
			public override void OnActivated ()
			{
				owner.OnActivated (new StartEventArgs (Type, Detail));
			}
		}

		class QuickStartItem : LinkItem {

			ProfileConfiguration config;

			public QuickStartItem (StartPage owner, ProfileConfiguration config, string caption, string description, int x, int y) : base (owner, caption, description, x, y)
			{
				this.config = config;
			}

			public override void OnActivated ()
			{
				owner.OnActivated (new StartEventArgs (config));
			}
		}

		void Layout ()
		{
			if (banner == null)
				banner = new BannerItem (this);

			items.Clear ();
			items.Add (banner);
			Item item = new LabelItem (this, "Common Actions:", 0, banner.Bounds.Bottom + 3 * padding);
			items.Add (item);
			item = new LinkItem (this, "Create New Profile", StartEventType.Create, null, 0, item.Bounds.Bottom + text_padding);
			items.Add (item);
			item = new LinkItem (this, "Open Profile Log File", StartEventType.Open, null, 0, item.Bounds.Bottom);
			items.Add (item);

			int y = item.Bounds.Bottom + 3 * padding;
			if (history.LogFiles.Count > 0) {
				item = new LabelItem (this, "Recent Logs:", 0, y);
				y = item.Bounds.Bottom + text_padding;
				items.Add (item);
				foreach (LogInfo info in history.LogFiles) {
					item = new LinkItem (this, info.Caption, info.Detail, StartEventType.Open, info.Filename, 0, y);
					items.Add (item);
					y = item.Bounds.Bottom;
				}
			}

			int x = Allocation.Width / 2;
			if (history.Configs.Count > 0) {
				item = new LabelItem (this, "Quick Sessions:", x, banner.Bounds.Bottom + 3 * padding);
				items.Add (item);
				y = item.Bounds.Bottom + text_padding;
				foreach (ProfileConfiguration config in history.Configs) {
					string text = config.ToString ();
					int idx = text.IndexOf (":");
					item = new QuickStartItem (this, config, text.Substring (0, idx), text.Substring (idx + 1), x, y);
					items.Add (item);
					y = item.Bounds.Bottom;
				}
			}
		}

		void OnActivated (StartEventArgs args)
		{
			if (Activated != null)
				Activated (this, args);
		}

		void Paint (Gdk.EventExpose ev)
		{
			Gtk.Style.PaintBox (Style, GdkWindow, Gtk.StateType.Normal, Gtk.ShadowType.In, ev.Area, this, null, 0, 0, Allocation.Width, Allocation.Height);
			foreach (Item item in items)
				if (item.Bounds.IntersectsWith (ev.Area))
					item.Draw (ev.Area);
		}

		void Refresh ()
		{
			Layout ();
			QueueDraw ();
		}

		void SelectItem (LinkItem item)
		{
			if (selected_item != null)
				QueueDrawArea (selected_item.Bounds.X, selected_item.Bounds.Y, selected_item.Bounds.Width, selected_item.Bounds.Height);
			selected_item = item;
			if (selected_item != null)
				QueueDrawArea (selected_item.Bounds.X, selected_item.Bounds.Y, selected_item.Bounds.Width, selected_item.Bounds.Height);
		}

		void SelectNextItem ()
		{
			if (selected_item == null) {
				SelectItem (items [2] as LinkItem); // skips Banner and Common Actions label
				return;
			}

			int idx = items.IndexOf (selected_item);
			while (++idx < items.Count && !(items[idx] is LinkItem));
			if (idx < items.Count)
				SelectItem (items[idx] as LinkItem);
		}

		void SelectPreviousItem ()
		{
			if (selected_item == null) {
				SelectItem (items [2] as LinkItem); // skips Banner and Common Actions label
				return;
			}

			int idx = items.IndexOf (selected_item);
			while (--idx >= 2 && !(items[idx] is LinkItem));
			if (idx >= 2)
				SelectItem (items[idx] as LinkItem);
		}
	}
}
