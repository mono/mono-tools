//
// Gendarme.cs: A SWF-based Wizard Runner for Gendarme
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Gendarme.Framework;

using Mono.Cecil;

namespace Gendarme {

	public partial class Wizard : Form {

		// used to call code asynchronously
		delegate void MethodInvoker ();

		public enum Page {
			Welcome,
			AddFiles,
			SelectRules,
			Analyze,
			Report
		}

		private const string Url = "http://www.mono-project.com/Gendarme";

		static Process process;

		private bool rules_populated;
		private Dictionary<string, AssemblyDefinition> assemblies;
		private GuiRunner runner;
		private int counter;

		private MethodInvoker rule_loader;
		private IAsyncResult rules_loading;

		private MethodInvoker analyze;
		private IAsyncResult analyzing;

		private string html_report_filename;
		private string xml_report_filename;
		private string text_report_filename;


		public Wizard ()
		{
			InitializeComponent ();
			// hide the tabs from the TabControl/TabPage[s] being [mis-]used
			// to implement this wizard
			wizard_tab_control.Top = -22;

			welcome_link_label.Text = Url;
			welcome_wizard_label.Text = String.Format ("Gendarme Wizard Runner Version {0}",
				GetVersion (GetType ()));
			welcome_framework_label.Text = String.Format ("Gendarme Framework Version {0}",
				GetVersion (typeof (IRule)));

			UpdatePageUI ();
		}

		private static Version GetVersion (Type type)
		{
			return type.Assembly.GetName ().Version;
		}

		static void EndCallback (IAsyncResult result)
		{
			(result.AsyncState as MethodInvoker).EndInvoke (result);
		}

		static Process Process {
			get {
				if (process == null)
					process = new Process ();
				return process;
			}
		}

		static void Open (string filename)
		{
			Process.StartInfo.Verb = "open";
			Process.StartInfo.FileName = filename;
			Process.Start ();
		}

		#region general wizard code

		public Page Current {
			get { return (Page) wizard_tab_control.SelectedIndex; }
			set {
				wizard_tab_control.SelectedIndex = (int) value;
				UpdatePageUI ();
			}
		}

		public GuiRunner Runner {
			get {
				if (runner == null)
					runner = new GuiRunner (this);
				return runner;
			}
		}

		private void BackButtonClick (object sender, EventArgs e)
		{
			switch (Current) {
			case Page.Welcome:
				return;
			case Page.AddFiles:
				Current = Page.Welcome;
				break;
			case Page.SelectRules:
				Current = Page.AddFiles;
				break;
			case Page.Analyze:
				// then ask confirmation before aborting 
				// and move back one step
				if (ConfirmAnalyzeAbort (false))
					Current = Page.SelectRules;
				break;
			case Page.Report:
				// move two step back (i.e. skip analyze)
				Current = Page.SelectRules;
				break;
			}
		}

		private void NextButtonClick (object sender, EventArgs e)
		{
			switch (Current) {
			case Page.Welcome:
				Current = Page.AddFiles;
				break;
			case Page.AddFiles:
				Current = Page.SelectRules;
				break;
			case Page.SelectRules:
				Current = Page.Analyze;
				break;
			case Page.Analyze:
			case Page.Report:
				// should not happen
				return;
			}
		}

		private void CancelButtonClick (object sender, EventArgs e)
		{
			// if we're analyzing...
			if (Current == Page.Analyze) {
				// then ask confirmation before aborting
				if (!ConfirmAnalyzeAbort (true))
					return;
			}
			Close ();
		}

		private void HelpButtonClick (object sender, EventArgs e)
		{
			// open web browser to http://www.mono-project.com/Gendarme
			Open (Url);
		}

		private void UpdatePageUI ()
		{
			back_button.Enabled = true;
			next_button.Enabled = true;
			cancel_button.Text = "Cancel";

			switch (Current) {
			case Page.Welcome:
				UpdateWelcomeUI ();
				break;
			case Page.AddFiles:
				UpdateAddFilesUI ();
				break;
			case Page.SelectRules:
				UpdateSelectRulesUI ();
				break;
			case Page.Analyze:
				UpdateAnalyzeUI ();
				break;
			case Page.Report:
				UpdateReportUI ();
				break;
			}
		}

		#endregion

		#region Welcome

		private void UpdateWelcomeUI ()
		{
			back_button.Enabled = false;
			if (rule_loader == null) {
				rule_loader = Runner.LoadRules;
				rules_loading = rule_loader.BeginInvoke (EndCallback, rule_loader);
			}
		}

		private void GendarmeLinkClick (object sender, LinkLabelLinkClickedEventArgs e)
		{
			Open (Url);
		}

		#endregion

		#region Add Files

		private void UpdateAddFilesUI ()
		{
			int files_count = file_list_box.Items.Count;
			bool has_files = (files_count > 0);
			next_button.Enabled = has_files;
			remove_file_button.Enabled = has_files;
			if (has_files) {
				add_files_count_label.Text = String.Format ("{0} assembl{1} selected",
					files_count, files_count == 1 ? "y" : "ies");
			} else {
				add_files_count_label.Text = "No assembly selected.";
			}
		}

		private void AddFilesButtonClick (object sender, EventArgs e)
		{
			if (open_file_dialog.ShowDialog (this) == DialogResult.OK) {
				foreach (string filename in open_file_dialog.FileNames) {
					// don't add duplicates
					if (!file_list_box.Items.Contains (filename))
						file_list_box.Items.Add (filename);
				}
			}
			UpdatePageUI ();
		}

		private void RemoveFileButtonClick (object sender, EventArgs e)
		{
			// remove from the last one to the first one 
			// so the indices are still valid during the removal operation
			for (int i = file_list_box.SelectedIndices.Count - 1; i >= 0; i--) {
				int remove = file_list_box.SelectedIndices [i];

				// if some AssemblyDefinition are already loaded...
				if (assemblies != null) {
					// then look if we need to remove them too!
					assemblies.Remove ((string) file_list_box.Items [remove]);
				}
				file_list_box.Items.RemoveAt (remove);
			}
			UpdatePageUI ();
		}

		#endregion

		#region Select Rules

		private void UpdateSelectRulesUI ()
		{
			rules_count_label.Text = String.Format ("{0} rules are available.", Runner.Rules.Count);
			if (rules_loading == null)
				throw new InvalidOperationException ("rules_loading");
			next_button.Enabled = rules_loading.IsCompleted;
			rules_tree_view.Enabled = rules_loading.IsCompleted;
			rules_loading.AsyncWaitHandle.WaitOne ();
			PopulateRules ();
		}

		private void PopulateRules ()
		{
			if (rules_populated)
				return;

			foreach (IRule rule in Runner.Rules) {
				TreeNode node = new TreeNode (rule.FullName);
				node.Checked = true;
				node.Tag = rule;
				node.ToolTipText = rule.Problem;
				rules_tree_view.Nodes.Add (node);
			}
			rules_tree_view.Sort ();
			rules_populated = true;
			UpdatePageUI ();
		}

		private void BrowseDocumentationButtonClick (object sender, EventArgs e)
		{
			string url = null;

			if (rules_tree_view.SelectedNode == null)
				url = Url;
			else {
				// we need quote because of the # in the rule url (mono bug #371567)
				url = String.Format ("{0}{1}{0}", "\"",
					(rules_tree_view.SelectedNode.Tag as IRule).Uri);
			}

			Open (url);
		}

		private void RulesTreeViewAfterCheck (object sender, TreeViewEventArgs e)
		{
			(e.Node.Tag as IRule).Active = e.Node.Checked;
		}

		#endregion

		#region Analyze

		private void UpdateAnalyzeUI ()
		{
			next_button.Enabled = false;
			PrepareAnalyze ();
			analyze = Analyze;
			analyzing = analyze.BeginInvoke (EndCallback, analyze);
		}

		private void PrepareAnalyze ()
		{
			// any existing report is now out-of-date
			html_report_filename = null;
			xml_report_filename = null;
			text_report_filename = null;

			if (assemblies == null)
				assemblies = new Dictionary<string, AssemblyDefinition> ();

			// add all items from the list box (and avoid reloading the ones already present)
			foreach (string assembly in file_list_box.Items) {
				if (assemblies.ContainsKey (assembly))
					continue;

				// if needed complete loading
				AssemblyDefinition ad = AssemblyFactory.GetAssembly (assembly);
				(ad as IAnnotationProvider).Annotations.Add ("filename", assembly);
				assemblies.Add (assembly, ad);
			}

			Runner.Reset ();
			Runner.Assemblies.Clear ();
			foreach (KeyValuePair<string, AssemblyDefinition> kvp in assemblies) {
				// add assemblies references to runner
				Runner.Assemblies.Add (kvp.Value);
			}

			progress_bar.Maximum = Runner.Assemblies.Count;
		}

		private void Analyze ()
		{
			counter = 1;
			Runner.Initialize ();
			Runner.Run ();

			if (InvokeRequired) {
				BeginInvoke ((Action) (() => Current = Page.Report));
			} else {
				Current = Page.Report;
			}
		}

		private bool ConfirmAnalyzeAbort (bool quit)
		{
			string message = String.Format ("Abort the current analysis being executed {0}Gendarme ?",
				quit ? "and quit " : String.Empty);
			return (MessageBox.Show (this, message, "Abort ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2) == DialogResult.Yes);
		}

		/// <summary>
		/// Update UI before analyzing an assembly.
		/// </summary>
		/// <param name="e">RunnerEventArgs that contains the Assembly being analyzed and the Runner</param>
		public void PreUpdate (RunnerEventArgs e)
		{
			analyze_status_label.Text = String.Format ("Processing assembly {0} of {1}",
				counter, e.Runner.Assemblies.Count);
			analyze_assembly_label.Text = "Assembly: " + e.CurrentAssembly.Name.FullName;
			progress_bar.Value = counter++;
		}

		/// <summary>
		/// Update UI after analyzing an assembly.
		/// </summary>
		/// <param name="e">RunnerEventArgs that contains the Assembly being analyzed and the Runner</param>
		public void PostUpdate (RunnerEventArgs e)
		{
			analyze_defect_label.Text = String.Format ("Defects Found: {0}", e.Runner.Defects.Count);
		}

		#endregion

		#region Report

		private void UpdateReportUI ()
		{
			bool has_defects = (Runner.Defects.Count > 0);
			save_report_button.Enabled = has_defects;
			view_report_button.Enabled = has_defects;
			report_subtitle_label.Text = String.Format ("Gendarme has found {0} defects during analysis.",
				has_defects ? Runner.Defects.Count.ToString () : "no");
			cancel_button.Text = "Close";
			next_button.Enabled = false;
		}

		private void SaveReportButtonClick (object sender, EventArgs e)
		{
			if (save_file_dialog.ShowDialog () != DialogResult.OK)
				return;

			string filename = save_file_dialog.FileName;

			switch (save_file_dialog.FilterIndex) {
			case 1:
				// avoid re-creating the report and simply copy the file
				if (html_report_filename != null) {
					File.Copy (html_report_filename, filename);
				} else {
					using (HtmlResultWriter writer = new HtmlResultWriter (Runner, filename)) {
						writer.Report ();
					}
				}
				html_report_filename = filename;
				break;
			case 2:
				using (XmlResultWriter writer = new XmlResultWriter (Runner, filename)) {
					writer.Report ();
				}
				xml_report_filename = filename;
				break;
			case 3:
				using (TextResultWriter writer = new TextResultWriter (Runner, filename)) {
					writer.Report ();
				}
				text_report_filename = filename;
				break;
			}
		}

		private void ViewReportButtonClick (object sender, EventArgs e)
		{
			// open web browser on html report
			if (html_report_filename == null) {
				html_report_filename = Path.ChangeExtension (Path.GetTempFileName (), ".html");
				using (HtmlResultWriter writer = new HtmlResultWriter (Runner, html_report_filename)) {
					writer.Report ();
				}
			}
			Open (html_report_filename);
		}

		#endregion
	}
}
