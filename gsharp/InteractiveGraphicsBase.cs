// InteractiveGraphicsBase.cs
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using Gtk;

namespace Mono.CSharp.Gui
{	
	public class InteractiveGraphicsBase : Mono.CSharp.InteractiveBase
	{
		static internal List<TransformHandler> type_handlers = new List<TransformHandler> ();
		
		public delegate double DoubleFunc (double a);
		static int width = 400;
		static int height = 350;

		public static new string help {
			get {
				return InteractiveBase.help +
					"  RegisterTransformHandler(object => object)\n" +
					"  MainWindow    - Gtk# toplevel window\n" +
					"  PaneContainer - Gtk# pane container\n" + 
					"\nSome samples:\n" +
					" Plot(x=>x*x)   - Plots the function\n" +
					" Image.FromFile (path) - Loads an image\n";
			}
		}
			
		public static int PlotHeight {
			get {
				return height;
			}

			set {
				if (value < 1)
					throw new ArgumentException ();
			
				height = value;
			}
		}
		
		public static int PlotWidth {
			get {
				return width;
			}

			set {
				if (value < 1)
					throw new ArgumentException ();

				width = value;
			}
		}

		static Pen GetPen (int i)
		{
			Pen p;
			
			switch (i % 4){
			case 0:
				p = new Pen (Color.Blue);
				break;
			case 1:
				p = new Pen (Color.Red);
				break;
			case 2:
				p = new Pen (Color.Green);
				break;
				
			default:
				p = new Pen (Color.Black);
				break;
			}
			
			switch ((i >> 2) % 4){
			case 0:
				break;
				
			case 1:
				p.DashStyle = DashStyle.DashDot;
				break;
				
			case 2:
				p.DashStyle = DashStyle.DashDotDot;
				break;
				
			case 3:
				p.DashStyle = DashStyle.Dash;
				break;
			}

			return p;
		}

		static void DrawAxes (Graphics g, Pen p, float x1, float y1, float x2, float y2)
		{
			g.DrawLine (p, x1, 0, x2, 0);
			g.DrawLine (p, 0, y1, 0, y2);
		}
		
		public static object Plot (DoubleFunc function)
		{
			return PlotFunctions (new DoubleFunc [] { function });
		}

		public static object Plot (DoubleFunc f1, DoubleFunc f2)
		{
			return PlotFunctions (new DoubleFunc [] { f1, f2 });
		}

		public static object Plot (DoubleFunc f1, DoubleFunc f2, DoubleFunc f3)
		{
			return PlotFunctions (new DoubleFunc [] { f1, f2, f3 });
		}

		public static object Plot (DoubleFunc f1, DoubleFunc f2, DoubleFunc f3, DoubleFunc f4)
		{
			return PlotFunctions (new DoubleFunc [] { f1, f2, f3, f4 });
		}

		static object PlotFunctions (DoubleFunc [] funcs)
		{
			Bitmap b = new Bitmap (width, height);
			
			Matrix t = new Matrix (), invert;
			t.Translate (width/2, height/2);
			t.Scale (2f, -2f);
			
			invert = t.Clone ();
			invert.Invert ();
			
			Point [] bounds = new Point [] {
				new Point (0, 0),
				new Point (width, height)
			};
			invert.TransformPoints (bounds);

			Pen black = new Pen (Color.Black);
			black.Transform = invert;

			float x1 = bounds [0].X;
			float x2 = bounds [1].X;
			
			using (Graphics g = System.Drawing.Graphics.FromImage (b)){
				g.Transform = t;
				DrawAxes (g, black, x1, bounds [1].Y, x2, bounds [0].Y);

				int i = 0;
				g.SmoothingMode = SmoothingMode.AntiAlias;
				foreach (var func in funcs){
					Pen p = GetPen (i++);
					p.Transform = invert;
					
					PlotFunction (g, p, x1, x2, func);
				}

				g.Transform = new Matrix ();
				
				g.DrawRectangle (black, 0, 0, width-1, height-1);
			}
			
			return b;
		}
		
		public static void PlotFunction (Graphics g, Pen p, double x1, double x2, DoubleFunc function)
		{
			double lx = x1;
			double ly = (double) function (lx);

			double step = (x2-x1)/(width/2);
			
			for (double x = x1; x < x2; x += step){
				double y = function (x);

				g.DrawLine (p, (float) lx, (float) ly, (float) x, (float) y);
				lx = x;
				ly = y;
			}
		}

		public delegate object TransformHandler (object o);
		
		public static void RegisterTransformHandler (TransformHandler o)
		{
			if (o == null)
				throw new ArgumentException ("parameter is null");

			if (type_handlers.Contains (o))
				return;

			type_handlers.Insert (0, o);
		}

		public static void UnregisterTransformHandler (TransformHandler o)
		{
			if (o == null)
				return;
			type_handlers.Remove (o);
		}

		// Whether this is an attached program or not.
		
		static bool attached;
		public static bool Attached {
			get { return attached; }
			internal set { attached = value; }
		}

		static MainWindow main_window;
		// A handle to our main window, the contract is Gtk.Window, internally we know its a MainWindow
		public static Gtk.Window MainWindow {
			get { return main_window; }
			internal set { main_window = (MainWindow) value; }
		}

		static Gtk.Container pane;
		public static Gtk.Container PaneContainer {
			get { return pane; }
			internal set { pane = value; }
		}
		
		static public new string Describe (object x)
		{
			if (x == null)
				return "object is null";

			Type t = x as Type;
			if (t == null)
				t = x.GetType ();

			main_window.Describe (t);
		 	return "Described on separate page";
		}
	}
}
