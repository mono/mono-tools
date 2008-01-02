// MainWindow.cs
//
// Copyright (c) 2007 Novell, Inc.
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
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using Gtk;
using System.Threading;
using System.Text;
using GuiCompare;

public partial class MainWindow: Gtk.Window
{	
	string monodir;
	InfoManager info_manager;
	LoadCompAssembly reference_loader, target_loader;
	CompareContext context;
	
	static Gdk.Pixbuf classPixbuf, delegatePixbuf, enumPixbuf;
	static Gdk.Pixbuf eventPixbuf, fieldPixbuf, interfacePixbuf;
	static Gdk.Pixbuf methodPixbuf, namespacePixbuf, propertyPixbuf;
	static Gdk.Pixbuf attributePixbuf, structPixbuf, assemblyPixbuf;

	static Gdk.Pixbuf okPixbuf, errorPixbuf;
	static Gdk.Pixbuf missingPixbuf, todoPixbuf, extraPixbuf;

	static Gdk.Color green, red, black;
	
	Gtk.TreeStore treeStore;
	Gtk.TreeModelFilter treeFilter;
	
	static MainWindow ()
	{
		Assembly ta = typeof (MainWindow).Assembly;

		classPixbuf = new Gdk.Pixbuf (ta, "c.gif");
		delegatePixbuf = new Gdk.Pixbuf (ta, "d.gif");
		enumPixbuf = new Gdk.Pixbuf (ta, "en.gif");
		eventPixbuf = new Gdk.Pixbuf (ta, "e.gif");
		fieldPixbuf = new Gdk.Pixbuf (ta, "f.gif");
		interfacePixbuf = new Gdk.Pixbuf (ta, "i.gif");
		methodPixbuf = new Gdk.Pixbuf (ta, "m.gif");
		namespacePixbuf = new Gdk.Pixbuf (ta, "n.gif");
		propertyPixbuf = new Gdk.Pixbuf (ta, "p.gif");
		attributePixbuf = new Gdk.Pixbuf (ta, "r.gif");
		structPixbuf = new Gdk.Pixbuf (ta, "s.gif");
		assemblyPixbuf = new Gdk.Pixbuf (ta, "y.gif");
	
		okPixbuf = new Gdk.Pixbuf (ta, "sc.gif");
		errorPixbuf = new Gdk.Pixbuf (ta, "se.gif");
		missingPixbuf = new Gdk.Pixbuf (ta, "sm.gif");
		todoPixbuf = new Gdk.Pixbuf (ta, "st.gif");
		extraPixbuf = new Gdk.Pixbuf (ta, "sx.gif");
		Gdk.Color.Parse ("#ff0000", ref red);
		Gdk.Color.Parse ("#00ff00", ref green);
		Gdk.Color.Parse ("#000000", ref black);
	}	
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		notebook1.Page = 1;
		
		progressbar1.Adjustment.Lower = 0;
		progressbar1.Adjustment.Upper = 100;
		info_manager = new InfoManager (this);
		
		treeStore = new Gtk.TreeStore (typeof (string), typeof (Gdk.Pixbuf), typeof (Gdk.Pixbuf),
		                               typeof (Gdk.Pixbuf), typeof (string),
		                               typeof (Gdk.Pixbuf), typeof (string),
		                               typeof (Gdk.Pixbuf), typeof (string),
		                               typeof (Gdk.Pixbuf), typeof (string),
		                               typeof (ComparisonNode), typeof (string));
		
		treeFilter = new Gtk.TreeModelFilter (treeStore, null);
		treeFilter.VisibleFunc = FilterTree;
		tree.Model = treeFilter;
		
		// Create a column for the node name
		Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
		nameColumn.Title = "Name";
		nameColumn.Resizable = true;
		
		Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
		Gtk.CellRendererPixbuf typeCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererPixbuf statusCell = new Gtk.CellRendererPixbuf ();
		
		nameColumn.PackStart (statusCell, false);
		nameColumn.PackStart (typeCell, false);
		nameColumn.PackStart (nameCell, true);
		
		tree.AppendColumn (nameColumn);
		
		nameColumn.AddAttribute (nameCell, "text", 0);
		nameColumn.AddAttribute (nameCell, "foreground", 12);
		nameColumn.AddAttribute (typeCell, "pixbuf", 1);
		nameColumn.AddAttribute (statusCell, "pixbuf", 2);
		
		// Create a column for the status counts
		Gtk.TreeViewColumn countsColumn = new Gtk.TreeViewColumn ();
		countsColumn.Title = "Counts";
		countsColumn.Resizable = true;
		
		Gtk.CellRendererPixbuf missingPixbufCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererText missingTextCell = new Gtk.CellRendererText ();
		Gtk.CellRendererPixbuf extraPixbufCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererText extraTextCell = new Gtk.CellRendererText ();
		Gtk.CellRendererPixbuf errorPixbufCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererText errorTextCell = new Gtk.CellRendererText ();
		Gtk.CellRendererPixbuf todoPixbufCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererText todoTextCell = new Gtk.CellRendererText ();
		
		countsColumn.PackStart (missingPixbufCell, false);
		countsColumn.PackStart (missingTextCell, false);
		countsColumn.PackStart (extraPixbufCell, false);
		countsColumn.PackStart (extraTextCell, false);
		countsColumn.PackStart (errorPixbufCell, false);
		countsColumn.PackStart (errorTextCell, false);
		countsColumn.PackStart (todoPixbufCell, false);
		countsColumn.PackStart (todoTextCell, false);
		
		tree.AppendColumn (countsColumn);
		
		countsColumn.AddAttribute (missingPixbufCell, "pixbuf", 3);
		countsColumn.AddAttribute (missingTextCell, "text", 4);
		countsColumn.AddAttribute (extraPixbufCell, "pixbuf", 5);
		countsColumn.AddAttribute (extraTextCell, "text", 6);
		countsColumn.AddAttribute (errorPixbufCell, "pixbuf", 7);
		countsColumn.AddAttribute (errorTextCell, "text", 8);
		countsColumn.AddAttribute (todoPixbufCell, "pixbuf", 9);
		countsColumn.AddAttribute (todoTextCell, "text", 10);

		tree.Selection.Changed += delegate (object sender, EventArgs e) {
			Gtk.TreeIter iter;
			if (tree.Selection.GetSelected (out iter)) {
				List<string> msgs = null;
				ComparisonNode n = tree.Model.GetValue (iter, 11) as ComparisonNode;
				StringBuilder sb = new StringBuilder();

				if (n != null) msgs = n.messages;
				if (msgs != null && msgs.Count > 0) {
					sb.Append ("<b>Errors:</b>\n");
					for (int i = 0; i < msgs.Count; i ++) {
						sb.AppendFormat ("\t<b>{0}</b>: {1}\n", i + 1, msgs[i]);
					}
				}
				
				if (n != null) msgs = n.todos;
				if (msgs != null && msgs.Count > 0) {
					sb.Append ("<b>TODO:</b>\n");
					for (int i = 0; i < msgs.Count; i ++) {
						sb.AppendFormat ("\t<b>{0}</b>: {1}\n", i + 1, msgs[i]);
					}
				}
				
				if (sb.Length > 0) {
					summary.Markup = sb.ToString();
					summary.Visible = true;
				}
				else {
					summary.Visible = false;
				}
			}
		};
	}
	
	// A handle to our menu bar
	public MenuBar MenuBar {
		get {
			return menubar1;
		}
	}
	
	// Used to set the status from other classes
	int count = 0;
	public string Status {
		set {
			if (count-- > 0)
				statusbar1.Pop (0);
			count++;
			statusbar1.Push (0, value);
		}
	}
	
	// Used to update the progressbar
	public double Progress {
		get {
			return progressbar1.Adjustment.Value;
		}
		
		set {
			progressbar1.Adjustment.Value = value;
		}
	}
	
	public void SetTarget (LoadCompAssembly target)
	{
		target_loader = target;
	}
	
	public void SetReference (LoadCompAssembly reference)
	{
		reference_loader = reference;
	}
	
	public void StartCompare (WaitCallback done)
	{		
		// clear our existing content
		if (context != null)
			context.StopCompare ();

		// Go to the actual tree page.
		notebook1.Page = 0;
		
		// now generate new content asynchronously
		context = new CompareContext (reference_loader, target_loader);
		
		context.ProgressChanged += delegate (object sender, CompareProgressChangedEventArgs e) {
			/* update our progress bar */
			Status = e.Message;
			Progress = e.Progress;
		};
		context.Error += delegate (object sender, CompareErrorEventArgs e) {
			Console.WriteLine ("ERROR: {0}", e.Message);
			MessageDialog md = new MessageDialog (this, 0, MessageType.Error, ButtonsType.Ok, false,
			                                      e.Message);
			md.Response += delegate (object s, ResponseArgs ra) {
				md.Hide ();
			};
			md.Show();
			Status = String.Format ("Comparison failed at {0}", DateTime.Now);
			Progress = 0.0;
		};
		context.Finished += delegate (object sender, EventArgs e) {
			Status = String.Format ("Comparison completed at {0}", DateTime.Now);
			context.Comparison.PropagateCounts ();
			PopulateTreeFromComparison (context.Comparison);
			Progress = 0.0;
			done (this);
		};
		treeStore.Clear ();
		context.Compare ();
	}
	
	Gdk.Pixbuf TypePixbufFromComparisonNode (ComparisonNode node)
	{
		switch (node.type) {
		case CompType.Assembly: return assemblyPixbuf;
		case CompType.Namespace: return namespacePixbuf;
		case CompType.Attribute: return attributePixbuf;
		case CompType.Interface: return interfacePixbuf;
		case CompType.Class: return classPixbuf;
		case CompType.Struct: return structPixbuf;
		case CompType.Delegate: return delegatePixbuf;
		case CompType.Enum: return enumPixbuf;
		case CompType.Method: return methodPixbuf;
		case CompType.Property: return propertyPixbuf;
		case CompType.Field: return fieldPixbuf;
		case CompType.Event: return eventPixbuf;
		}
		return null;
	}
	
	Gdk.Pixbuf StatusPixbufFromComparisonNode (ComparisonNode node)
	{
		switch (node.status) {
		case ComparisonStatus.None: return okPixbuf;
		case ComparisonStatus.Missing: return missingPixbuf;
		case ComparisonStatus.Extra: return extraPixbuf;
		case ComparisonStatus.Error: return errorPixbuf;
		}
		return null;
	}

	string StatusForegroundFromComparisonNode (ComparisonNode node)
	{
		switch (node.status) {
		case ComparisonStatus.Missing: return "darkred";
		case ComparisonStatus.Extra: return "green";
		case ComparisonStatus.Error: return "red";
		case ComparisonStatus.None:
		default:
			return "black";
		}
	}
	
	void PopulateTreeFromComparison (ComparisonNode root)
	{
		Gtk.TreeIter iter =
			treeStore.AppendValues (root.name,
			                        TypePixbufFromComparisonNode (root),
			                        StatusPixbufFromComparisonNode (root),
			                        root.Missing == 0 ? null : missingPixbuf,
			                        root.Missing == 0 ? null : String.Format (": {0}", root.Missing),
			                        root.Extra == 0 ? null : extraPixbuf,
			                        root.Extra == 0 ? null : String.Format (": {0}", root.Extra),
			                        root.Warning == 0 ? null : errorPixbuf,
			                        root.Warning == 0 ? null : String.Format (": {0}", root.Warning),
			                        root.Todo == 0 ? null : todoPixbuf,
			                        root.Todo == 0 ? null : String.Format (": {0}", root.Todo),
			                        root,
			                        StatusForegroundFromComparisonNode (root));
		
		Gtk.TreePath path = treeStore.GetPath (iter);
		
		foreach (ComparisonNode n in root.children) {
			PopulateTreeFromComparison (iter, n);
		}
		
		tree.ExpandRow (path, false);
	}
	
	void PopulateTreeFromComparison (Gtk.TreeIter iter, ComparisonNode node)
	{
		Gtk.TreeIter citer = 
			treeStore.AppendValues (iter,
			                        node.name,
			                        TypePixbufFromComparisonNode (node),
			                        StatusPixbufFromComparisonNode (node),
			                        node.Missing == 0 ? null : missingPixbuf,
			                        node.Missing == 0 ? null : String.Format (": {0}", node.Missing),
			                        node.Extra == 0 ? null : extraPixbuf,
			                        node.Extra == 0 ? null : String.Format (": {0}", node.Extra),
			                        node.Warning == 0 ? null : errorPixbuf,
			                        node.Warning == 0 ? null : String.Format (": {0}", node.Warning),
			                        node.Todo == 0 ? null : todoPixbuf,
			                        node.Todo == 0 ? null : String.Format (": {0}", node.Todo),
			                        node,
			                        StatusForegroundFromComparisonNode (node));

		
		foreach (ComparisonNode n in node.children) {
			PopulateTreeFromComparison (citer, n);
		}
	}
	
	private bool FilterTree (Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		//string node_name = model.GetValue(iter, 0) as string;
		//Console.WriteLine ("filtering {0}, node = {1}", node_name, model.GetValue(iter, 9) == null ? "null" : model.GetValue(iter,9).GetType().ToString());
		ComparisonNode n = model.GetValue (iter, 11) as ComparisonNode;
		if (n == null)
			return false;
		
		if ((ShowMissing.Active && (n.status == ComparisonStatus.Missing || n.Missing > 0)) ||
		    (ShowExtra.Active && (n.status == ComparisonStatus.Extra || n.Extra > 0)) ||
		    (ShowErrors.Active && (n.status == ComparisonStatus.Error || n.Warning > 0)) ||
		    (ShowTodo.Active && (n.Todo > 0)) ||
		    ShowPresent.Active && n.status == ComparisonStatus.None)
			
			return true;
		else
			return false;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected virtual void OnQuitActivated (object sender, System.EventArgs e)
	{
		Application.Quit ();
	}

	protected virtual void OnShowErrorsToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
	}

	protected virtual void OnShowMissingToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
	}

	protected virtual void OnShowPresentToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
	}
	
	protected virtual void OnShowExtraToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
	}


	protected virtual void OnShowTodoToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
	}

	protected virtual void OnRefreshActivated (object sender, System.EventArgs e)
	{
		StartCompare (delegate {});
	}
}