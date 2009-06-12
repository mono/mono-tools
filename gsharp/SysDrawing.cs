// SysDrawing.cs
//Authors: ${Author}
//
// Copyright (c) 2008 [copyright holders]
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using Gtk;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Mono.CSharp.Gui
{

	public class BitmapWidget : DrawingArea {
		System.Drawing.Bitmap b;
		
		public BitmapWidget (System.Drawing.Bitmap b)
		{
			this.b = b;
			GraphicsUnit unit = GraphicsUnit.Pixel;

			//
			// Quick hack to make stuff not too large
			//
			RectangleF bounds = b.GetBounds (ref unit);
			bounds.Height = System.Math.Min (bounds.Height, 600);
			bounds.Width = System.Math.Min (bounds.Width, 400);
			SetSizeRequest ((int) bounds.Width, (int) bounds.Height);
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose args)
		{
			Gdk.Rectangle area = args.Area;

			using (System.Drawing.Graphics g = GraphicsHelper.FromDrawable (args.Window)){
				System.Drawing.Rectangle bounds = new System.Drawing.Rectangle (area.X, area.Y, area.Width, area.Height);
				
				g.DrawImage (b, bounds, bounds, System.Drawing.GraphicsUnit.Pixel);
			}
			return true;
		}

	}

	public class GraphicsHelper {
		
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
		
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);
		
		public static System.Drawing.Graphics FromDrawable (Gdk.Drawable drawable)
		{
			IntPtr x_drawable;
			int x_off = 0, y_off = 0;
				
			
			if (drawable is Gdk.Window){
				((Gdk.Window) drawable).GetInternalPaintInfo(out drawable, out x_off, out y_off);
			} 
			x_drawable = drawable.Handle;
			
			IntPtr display = gdk_x11_drawable_get_xdisplay (x_drawable);
			
			Type graphics = typeof (System.Drawing.Graphics);
			MethodInfo mi = graphics.GetMethod ("FromXDrawable", BindingFlags.Static | BindingFlags.NonPublic);
			if (mi == null)
				throw new NotImplementedException ("In this implementation I can not get a graphics from a drawable");
			object [] args = new object [2] { (IntPtr) gdk_x11_drawable_get_xid (drawable.Handle), (IntPtr) display };
			object r = mi.Invoke (null, args);
			System.Drawing.Graphics g = (System.Drawing.Graphics) r;

			g.TranslateTransform (-x_off, -y_off);

			return g;
		}
	}
}


