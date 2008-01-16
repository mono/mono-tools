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

	static Gdk.Pixbuf okPixbuf, errorPixbuf, niexPixbuf;
	static Gdk.Pixbuf missingPixbuf, todoPixbuf, extraPixbuf;

	static Gdk.Color green, red, black;
	
	Gtk.TreeStore treeStore;
	Gtk.TreeModelFilter treeFilter;

			
	enum TreeCol : int {
		Name,
		TypeIcon,
		StatusIcon,
		MissingIcon,
		MissingText,
		ExtraIcon,
		ExtraText,
		ErrorIcon,
		ErrorText,
		TodoIcon,
		TodoText,
		NiexIcon,
		NiexText,
		Node,
		Foreground
	};


	public GuiCompare.Config Config;
	
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
		niexPixbuf = new Gdk.Pixbuf (ta, "mn.png");
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
		
		//
		// Load configuration
		//
		Config = GuiCompare.Config.GetConfig ();
		ShowMissing.Active = Config.ShowMissing;
		ShowErrors.Active = Config.ShowErrors;
		ShowExtra.Active = Config.ShowExtra;
		ShowPresent.Active = Config.ShowPresent;
		ShowTodo.Active = Config.ShowTodo;

		//
		// Configure the GUI
		//
		info_manager = new InfoManager (this);
		
		treeStore = new Gtk.TreeStore (typeof (string), // Name
		                               typeof (Gdk.Pixbuf), typeof (Gdk.Pixbuf), // TypeIcon, StatusIcon
		                               typeof (Gdk.Pixbuf), typeof (string), // MissingIcon, MissingText
		                               typeof (Gdk.Pixbuf), typeof (string), // ExtraIcon, ExtraText
		                               typeof (Gdk.Pixbuf), typeof (string), // ErrorIcon, ErrorText
		                               typeof (Gdk.Pixbuf), typeof (string), // TodoIcon, TodoText
		                               typeof (Gdk.Pixbuf), typeof (string), // NiexIcon, NiexText
		                               typeof (ComparisonNode), typeof (string)); // Node, Foreground
		
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
		
		nameColumn.AddAttribute (nameCell, "text", (int)TreeCol.Name);
		nameColumn.AddAttribute (nameCell, "foreground", (int)TreeCol.Foreground);
		nameColumn.AddAttribute (typeCell, "pixbuf", (int)TreeCol.TypeIcon);
		nameColumn.AddAttribute (statusCell, "pixbuf", (int)TreeCol.StatusIcon);
		
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
		Gtk.CellRendererPixbuf niexPixbufCell = new Gtk.CellRendererPixbuf ();
		Gtk.CellRendererText niexTextCell = new Gtk.CellRendererText ();
		
		countsColumn.PackStart (missingPixbufCell, false);
		countsColumn.PackStart (missingTextCell, false);
		countsColumn.PackStart (extraPixbufCell, false);
		countsColumn.PackStart (extraTextCell, false);
		countsColumn.PackStart (errorPixbufCell, false);
		countsColumn.PackStart (errorTextCell, false);
		countsColumn.PackStart (todoPixbufCell, false);
		countsColumn.PackStart (todoTextCell, false);
		countsColumn.PackStart (niexPixbufCell, false);
		countsColumn.PackStart (niexTextCell, false);
		
		tree.AppendColumn (countsColumn);

		countsColumn.AddAttribute (missingPixbufCell, "pixbuf", (int)TreeCol.MissingIcon);
		countsColumn.AddAttribute (missingTextCell, "text", (int)TreeCol.MissingText);
		countsColumn.AddAttribute (extraPixbufCell, "pixbuf", (int)TreeCol.ExtraIcon);
		countsColumn.AddAttribute (extraTextCell, "text", (int)TreeCol.ExtraText);
		countsColumn.AddAttribute (errorPixbufCell, "pixbuf", (int)TreeCol.ErrorIcon);
		countsColumn.AddAttribute (errorTextCell, "text", (int)TreeCol.ErrorText);
		countsColumn.AddAttribute (todoPixbufCell, "pixbuf", (int)TreeCol.TodoIcon);
		countsColumn.AddAttribute (todoTextCell, "text", (int)TreeCol.TodoText);
		countsColumn.AddAttribute (niexPixbufCell, "pixbuf", (int)TreeCol.NiexIcon);
		countsColumn.AddAttribute (niexTextCell, "text", (int)TreeCol.NiexText);
		
		tree.Selection.Changed += delegate (object sender, EventArgs e) {
			Gtk.TreeIter iter;
			if (tree.Selection.GetSelected (out iter)) {
				List<string> msgs = null;
				ComparisonNode n = tree.Model.GetValue (iter, (int)TreeCol.Node) as ComparisonNode;
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
	
	public void SetCompareDefinition (CompareDefinition cd)
	{
		if (cd.ReferenceIsInfo)
			SetReference (delegate { return new MasterAssembly (cd.ReferencePath); });
		else
			SetReference (delegate { return new CecilAssembly (cd.ReferencePath); });
		
		if (cd.TargetIsInfo)
			SetTarget (delegate { return new MasterAssembly (cd.TargetPath); });
		else
			SetTarget (delegate { return new CecilAssembly (cd.TargetPath); });
	}
	
	public void StartCompare (WaitCallback done)
	{		
		summary.Visible = false;
		
		progressbar1.Visible = true;						
		progressbar1.Adjustment.Lower = 0;
		progressbar1.Adjustment.Upper = 100;

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
			progressbar1.Visible = false;
		};
		context.Finished += delegate (object sender, EventArgs e) {
			DateTime finish_time = DateTime.Now;
			
			Status = String.Format ("Comparison completed at {0}", finish_time);
			
			context.Comparison.PropagateCounts ();
			PopulateTreeFromComparison (context.Comparison);
			Progress = 0.0;
			done (this);
			progressbar1.Visible = false;

			CompareHistory[] history = Config.Recent[0].History;
			
			if (history == null || history.Length == 0 ||
			    (history[history.Length-1].Extras != context.Comparison.Extra ||
			     history[history.Length-1].Errors != context.Comparison.Warning ||
			     history[history.Length-1].Missing != context.Comparison.Missing ||
			     history[history.Length-1].Niexs != context.Comparison.Niex ||
			     history[history.Length-1].Todos != context.Comparison.Todo)) {

				CompareHistory history_entry = new CompareHistory();
				history_entry.CompareTime = finish_time;
				history_entry.Extras = context.Comparison.Extra;
				history_entry.Errors = context.Comparison.Warning;
				history_entry.Missing = context.Comparison.Missing;
				history_entry.Niexs = context.Comparison.Niex;
				history_entry.Todos = context.Comparison.Todo;
				Config.Recent[0].AddHistoryEntry (history_entry);
				Config.Save ();
			}
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
			                        root.Missing == 0 ? null : String.Format (":{0}", root.Missing),
			                        root.Extra == 0 ? null : extraPixbuf,
			                        root.Extra == 0 ? null : String.Format (":{0}", root.Extra),
			                        root.Warning == 0 ? null : errorPixbuf,
			                        root.Warning == 0 ? null : String.Format (":{0}", root.Warning),
			                        root.Todo == 0 ? null : todoPixbuf,
			                        root.Todo == 0 ? null : String.Format (":{0}", root.Todo),
			                        root.Niex == 0 ? null : niexPixbuf,
			                        root.Niex == 0 ? null : String.Format (":{0}", root.Niex),
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
			                        node.Missing == 0 ? null : String.Format (":{0}", node.Missing),
			                        node.Extra == 0 ? null : extraPixbuf,
			                        node.Extra == 0 ? null : String.Format (":{0}", node.Extra),
			                        node.Warning == 0 ? null : errorPixbuf,
			                        node.Warning == 0 ? null : String.Format (":{0}", node.Warning),
			                        node.Todo == 0 ? null : todoPixbuf,
			                        node.Todo == 0 ? null : String.Format (":{0}", node.Todo),
			                        node.Niex == 0 ? null : niexPixbuf,
			                        node.Niex == 0 ? null : ((node.Niex == 1 && node.throws_niex) ? null : String.Format (":{0}", node.Niex)),
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
		ComparisonNode n = model.GetValue (iter, (int)TreeCol.Node) as ComparisonNode;
		if (n == null)
			return false;
		
		if ((ShowMissing.Active && (n.status == ComparisonStatus.Missing || n.Missing > 0)) ||
		    (ShowExtra.Active && (n.status == ComparisonStatus.Extra || n.Extra > 0)) ||
		    (ShowErrors.Active && (n.status == ComparisonStatus.Error || n.Warning > 0)) ||
		    (ShowTodo.Active && (n.Todo > 0)) ||
		    (ShowNotImplemented.Active && (n.Niex > 0)) ||
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
		Config.ShowErrors = ShowErrors.Active;
		Config.Save ();
	}

	protected virtual void OnShowMissingToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
		Config.ShowMissing = ShowMissing.Active;
		Config.Save ();
	}

	protected virtual void OnShowPresentToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
		Config.ShowPresent = ShowPresent.Active;
		Config.Save ();
	}
	
	protected virtual void OnShowExtraToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
		Config.ShowPresent = ShowPresent.Active;
		Config.Save ();
	}

	protected virtual void OnShowTodoToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
		Config.ShowTodo = ShowTodo.Active;
		Config.Save ();
	}

	protected virtual void OnShowNotImplementedToggled (object sender, System.EventArgs e)
	{
		treeFilter.Refilter();
		Config.ShowTodo = ShowTodo.Active;
		Config.Save ();
	}

	protected virtual void OnRefreshActivated (object sender, System.EventArgs e)
	{
		StartCompare (delegate {});
	}

	protected virtual void OnCustomActivated (object sender, System.EventArgs e)
	{
		CustomCompare cc = new CustomCompare ();
		ResponseType code = (ResponseType) cc.Run ();
		
		if (code == ResponseType.None)
			return;
		
		if (code == ResponseType.Ok){
			CompareDefinition cd = cc.GetCompare ();
			if (cd != null){
				SetCompareDefinition (cd);
		
				StartCompare (delegate { Title = cd.ToString ();});
				Config.AddRecent (cd);
				Config.Save ();
				info_manager.PopulateRecent ();
			}
		}
				
		cc.Destroy ();
	}
}