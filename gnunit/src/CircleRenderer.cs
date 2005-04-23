//
// CircleRenderer.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using GLib;
using Gdk;
using Gtk;

namespace Mono.NUnit.GUI
{
	enum CircleColor
	{
		None,
		Failure,
		NotRun,
		Success
	}

	class CircleRenderer
	{
		static TreeCellDataFunc dataFunc = null;
		static Pixbuf [] circles = null;
		static string [] colors = null;

		static void Init ()
		{
			if (circles != null)
				return;

			circles = new Pixbuf [4];
			circles [(int) CircleColor.None] = new Pixbuf (null, "none.png");
			circles [(int) CircleColor.Failure] = new Pixbuf (null, "red.png");
			circles [(int) CircleColor.NotRun] = new Pixbuf (null, "yellow.png");
			circles [(int) CircleColor.Success] = new Pixbuf (null, "green.png");
		}
		
		static void SetCellData (TreeViewColumn col, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			try {
				CellRendererPixbuf cr = (CellRendererPixbuf) cell;
				cr.Pixbuf = circles [(int) model.GetValue (iter, 0)];
			} catch (Exception e) {
				Console.WriteLine (e);
				Console.WriteLine ();
			}
		}

		public static Gtk.TreeCellDataFunc CellDataFunc {
			get {
				if (dataFunc == null) {
					dataFunc = new TreeCellDataFunc (SetCellData);
					Init ();
				}

				return dataFunc;
			}
		}
	}
}

