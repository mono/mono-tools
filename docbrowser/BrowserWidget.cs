//Permission is hereby granted, free of charge, to any person obtaining
//a copy of this software and associated documentation files (the
//"Software"), to deal in the Software without restriction, including
//without limitation the rights to use, copy, modify, merge, publish,
//distribute, sublicense, and/or sell copies of the Software, and to
//permit persons to whom the Software is furnished to do so, subject to
//the following conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//Copyright (c) 2008 Novell, Inc.
//
//Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using Mono.WebBrowser;
using Gtk;
using Gdk;
#if GNOME
using Gnome;
#endif

namespace Monodoc
{
	public class BrowserWidget : Gtk.Bin
	{
		public IWebBrowser browser;
		int width, height;
	
		public BrowserWidget() : base()
		{
			width = height = 200;
			browser = Manager.GetNewInstance (Platform.Gtk);
		}
		
		
		protected override void OnRealized ()
		{
			base.OnRealized ();

			WindowAttr attributes = new WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.X = Allocation.X;
			attributes.Y = Allocation.Y;
			attributes.Width = Allocation.Width;
			attributes.Height = Allocation.Height;
			attributes.Wclass = WindowClass.InputOutput;
			attributes.Visual = Visual;
			attributes.Colormap = Colormap;
			attributes.EventMask = (int) Events;
			attributes.EventMask = attributes.EventMask | ((int) Gdk.EventMask.ExposureMask |
						(int) Gdk.EventMask.KeyPressMask |
						(int) Gdk.EventMask.KeyReleaseMask |
						(int) Gdk.EventMask.EnterNotifyMask |
						(int) Gdk.EventMask.LeaveNotifyMask |
						(int) Gdk.EventMask.StructureMask |
						(int) Gdk.EventMask.FocusChangeMask);

			GdkWindow = new Gdk.Window (ParentWindow, attributes, Gdk.WindowAttributesType.X | Gdk.WindowAttributesType.Y | 
			                            Gdk.WindowAttributesType.Colormap | Gdk.WindowAttributesType.Visual);
			GdkWindow.UserData = this.Handle;

			Style = Style.Attach (GdkWindow);
			Style.Background (StateType.Normal);
			
			browser.Load (this.Handle, width, height);		
		}
		
		protected override void OnMapped ()
		{
			base.OnMapped ();
			GdkWindow.Show ();
		}

		protected override void OnUnmapped ()
		{
			base.OnUnmapped ();
			GdkWindow.Hide ();
		}		
		
		protected override void OnSizeAllocated (Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if ((WidgetFlags & WidgetFlags.Realized) != 0) {
				GdkWindow.MoveResize (allocation);
				if (browser != null)
					browser.Resize (allocation.Width, allocation.Height);
			}
		}
			
		protected override void OnSizeRequested (ref Requisition requisition)
		{
			SetSizeRequest (width, height);
			
		}


	}
}
