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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using Gendarme.Framework;
using Gendarme.Properties;

using Mono.Cecil;

namespace Gendarme {

	public partial class Wizard : Form {

		// This is used for methods which we execute within a thread, but we don't
		// execute the methods concurrently so we can use single thread.
		[ThreadModel (ThreadModel.SingleThread)]
		sealed class AssemblyInfo {
			private DateTime timestamp;
			private AssemblyDefinition definition;

			public DateTime Timestamp {
				get { return timestamp; }
				set { timestamp = value; }
			}

			public AssemblyDefinition Definition {
				get { return definition; }
				set { definition = value; }
			}
		}

		private const string BaseUrl = "http://www.mono-project.com/";
		private const string DefaultUrl = BaseUrl + "Gendarme";
		private const string BugzillaUrl = "http://bugzilla.novell.com";

		static Process process;

		private bool rules_populated;
		private Dictionary<string, AssemblyInfo> assemblies;
		private GuiRunner runner;
		private int counter;

		private Action assembly_loader;
		private IAsyncResult assemblies_loading;
		private object loader_lock = new object ();

		private Action rule_loader;
		private IAsyncResult rules_loading;

		private Action analyze;

		private string html_report_filename;
		private string xml_report_filename;
		private string text_report_filename;


		public Wizard ()
		{
			InitializeComponent ();
			// hide the tabs from the TabControl/TabPage[s] being [mis-]used
			// to implement this wizard
			wizard_tab_control.Top = -24;

			welcome_link_label.Text = DefaultUrl;

			Version v = typeof (IRule).Assembly.GetName ().Version;
			if ((v.Major == 0) && (v.Minor == 0))
				welcome_gendarme_label.Text = "Gendarme (development snapshot)";
			else
				welcome_gendarme_label.Text = String.Format (CultureInfo.CurrentCulture, "Gendarme, version {0}", v);

			assembly_loader = UpdateAssemblies;

			UpdatePageUI ();
		}

		[ThreadModel (ThreadModel.SingleThread)]
		static void EndCallback (IAsyncResult result)
		{
			(result.AsyncState as Action).EndInvoke (result);
		}

		static void Open (string filename)
		{
			if (process == null)
				process = new Process ();
			process.StartInfo.Verb = "open";
			process.StartInfo.FileName = filename;
			process.Start ();
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
			case Page.Options:
				Current = Page.SelectRules;
				break;
			case Page.Analyze:
				// then ask confirmation before aborting 
				// and move back one step
				if (ConfirmAnalyzeAbort (false))
					Current = Page.Options;
				break;
			case Page.Report:
				// move two step back (i.e. skip analyze)
				Current = Page.Options;
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
				Current = Page.Options;
				break;
			case Page.Options:
				UpdateOptions ();
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
			Open (DefaultUrl);
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
			case Page.Options:
				UpdateOptionsUI ();
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
			Open (DefaultUrl);
		}

		#endregion

		#region Add Files

		private void UpdateAddFilesUI ()
		{
			if (assemblies == null)
				assemblies = new Dictionary<string, AssemblyInfo> ();

			int files_count = file_list_box.Items.Count;
			bool has_files = (files_count > 0);
			next_button.Enabled = has_files;
			remove_file_button.Enabled = has_files;
			if (has_files) {
				add_files_count_label.Text = String.Format (CultureInfo.CurrentCulture, "{0} assembl{1} selected",
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
					if (!file_list_box.Items.Contains (filename)) {
						file_list_box.Items.Add (filename);
						assemblies.Add (filename, new AssemblyInfo ());
					}
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

		public void UpdateAssemblies ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException (GetType ().Name);

			// do not let the final check be done while we're still loading assemblies
			lock (loader_lock) {
				foreach (KeyValuePair<string,AssemblyInfo> kvp in assemblies) {
					DateTime last_write = File.GetLastWriteTimeUtc (kvp.Key);
					if ((kvp.Value.Definition == null) && (kvp.Value.Timestamp < last_write)) {
						AssemblyInfo a = kvp.Value;
						string filename = kvp.Key;
						try {
							a.Definition = AssemblyDefinition.ReadAssembly (filename, new ReaderParameters { AssemblyResolver = AssemblyResolver.Resolver });
						}
						catch (BadImageFormatException) {
							// continue loading & analyzing assemblies
							Runner.Warn ("Invalid image: " + filename);
						}
						catch (FileNotFoundException fnfe) {
							// e.g. .netmodules
							// continue loading & analyzing assemblies
							Runner.Warn (fnfe.Message + " while loading " + filename);
						}
						finally {
							a.Timestamp = last_write;
						}
					}
				}
			}
		}

		#endregion

		#region Select Rules

		private void UpdateSelectRulesUI ()
		{
			// asynchronously load assemblies (or the one that changed)
			assemblies_loading = assembly_loader.BeginInvoke (EndCallback, assembly_loader);

			rules_count_label.Text = String.Format (CultureInfo.CurrentCulture, 
				"{0} rules are available.", Runner.Rules.Count);
			if (rules_loading == null)
				throw new InvalidOperationException ("rules_loading");

			bool completed = rules_loading.IsCompleted;
			next_button.Enabled = completed;
			rules_tree_view.Enabled = completed;
			rules_loading.AsyncWaitHandle.WaitOne ();
			PopulateRules ();
		}

		private void PopulateRules ()
		{
			if (rules_populated)
				return;

			// if settings are empty (like default) then all rules are active
			StringCollection rules = Settings.Default.Rules;
			bool all_rules = ((rules == null) || (rules.Count == 0));

			Dictionary<string, TreeNode> nodes = new Dictionary<string, TreeNode> ();

			rules_tree_view.BeginUpdate ();
			rules_tree_view.AfterCheck -= RulesTreeViewAfterCheck;
			foreach (IRule rule in Runner.Rules) {
				TreeNode parent;
				string full_name = rule.FullName;
				string name = rule.Name;
				string name_space = full_name.Substring (0, full_name.Length - name.Length - 1);
				if (!nodes.TryGetValue (name_space, out parent)) {
					parent = new TreeNode (name_space);
					parent.Checked = all_rules;
					nodes.Add (name_space, parent);
					rules_tree_view.Nodes.Add (parent);
				}

				TreeNode node = new TreeNode (name);
				node.Checked = all_rules || rules.Contains (full_name);
				node.Tag = rule;
				node.ToolTipText = rule.Problem;
				parent.Nodes.Add (node);
				// if we have not already setted parent, then we do it if any node is checked
				if (!all_rules && node.Checked)
					parent.Checked = true;
			}
			foreach (TreeNode node in rules_tree_view.Nodes) {
				node.ToolTipText = String.Format (CultureInfo.CurrentCulture,
					"{0} rules available", node.Nodes.Count);
			}
			nodes.Clear ();
			rules_tree_view.AfterCheck += RulesTreeViewAfterCheck;

			// this extra [End|Begin]Update is brought to you by Vista(tm)
			// http://forums.msdn.microsoft.com/en-US/netfxbcl/thread/3fd6c4a2-b5c7-4334-b11a-e909b11e8bdc/
			rules_tree_view.EndUpdate ();

			rules_tree_view.BeginUpdate ();
			rules_tree_view.Sort ();
			rules_tree_view.EndUpdate ();

			rules_populated = true;
			UpdatePageUI ();
		}

		private void BrowseDocumentationButtonClick (object sender, EventArgs e)
		{
			string url = null;

			TreeNode selected = rules_tree_view.SelectedNode;
			if (selected == null)
				url = DefaultUrl;
			else {
				if (selected.Tag == null) {
					url = BaseUrl + selected.Text;
				} else {
					url = (selected.Tag as IRule).Uri.ToString ();
				}
			}

			Open (url);
		}

		private void SaveRulesButtonClick (object sender, EventArgs e)
		{
			bool all_active = UpdateActiveRules ();
			// add rule list only if they are not all active
			if (!all_active) {
				StringCollection rules = new StringCollection ();

				foreach (IRule rule in Runner.Rules) {
					if (rule.Active)
						rules.Add (rule.FullName);
				}
				Settings.Default.Rules = rules;
			}
			Settings.Default.Save ();
		}

		private void RulesTreeViewAfterCheck (object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag == null) {
				// childs
				foreach (TreeNode node in e.Node.Nodes) {
					node.Checked = e.Node.Checked;
				}
			}
		}

		[ThreadModel (ThreadModel.SingleThread)]
		private bool UpdateActiveRules ()
		{
			bool all_active = true;
			foreach (TreeNode assembly in rules_tree_view.Nodes) {
				foreach (TreeNode rule in assembly.Nodes) {
					if (!rule.Checked)
						all_active = false;
					(rule.Tag as Rule).Active = rule.Checked;
				}
			}
			return all_active;
		}

		#endregion

		#region Options

		private void UpdateOptionsUI ()
		{
			options_nolimit_checkbox.Checked = (Settings.Default.DefectsLimit == Int32.MaxValue);

			options_severity_combobox.SelectedIndex = (int) Severity.Audit - Settings.Default.Severity;
			options_confidence_combobox.SelectedIndex = (int) Confidence.Low - Settings.Default.Confidence;

			switch ((ApplicabilityScope) Settings.Default.Scope) {
			case ApplicabilityScope.Visible:
				options_visible_radiobutton.Checked = true;
				break;
			case ApplicabilityScope.NonVisible:
				options_notvisible_radiobutton.Checked = true;
				break;
			case ApplicabilityScope.All:
				options_all_radiobutton.Checked = true;
				break;
			}
		}

		private void OptionsNoLimitCheckboxCheckedChanged (object sender, EventArgs e)
		{
			options_limit_updown.Enabled = !options_nolimit_checkbox.Checked;
		}

		private void UpdateOptions ()
		{
			// 2^31 is close enough "no limit"
			Settings.Default.DefectsLimit = options_nolimit_checkbox.Checked ? Int32.MaxValue :
				(int) options_limit_updown.Value;

			// hack, this works right now but won't if we add/remove more options
			Settings.Default.Severity = (int) Severity.Audit - options_severity_combobox.SelectedIndex;
			Settings.Default.Confidence = (int) Confidence.Low - options_confidence_combobox.SelectedIndex;

			if (options_visible_radiobutton.Checked)
				Settings.Default.Scope = (int) ApplicabilityScope.Visible;
			else if (options_all_radiobutton.Checked)
				Settings.Default.Scope = (int) ApplicabilityScope.All;
			else
				Settings.Default.Scope = (int) ApplicabilityScope.NonVisible;
		}

		private void OnOptionsSaveClick (object sender, EventArgs e)
		{
			UpdateOptions ();
			Settings.Default.Save ();
		}

		#endregion

		#region Analyze

		private void UpdateAnalyzeUI ()
		{
			// update UI before waiting for assemblies to be loaded
			progress_bar.Value = 0;
			next_button.Enabled = false;
			analyze_status_label.Text = String.Format (CultureInfo.CurrentCulture,
				"Processing assembly 1 of {0}", assemblies.Count);
			analyze_defect_label.Text = "Defects Found: 0";
			// make sure all assemblies are loaded into memory
			assemblies_loading.AsyncWaitHandle.WaitOne ();
			PrepareAnalyze ();
			analyze = Analyze;
			analyze.BeginInvoke (EndCallback, analyze);
		}

		private void PrepareAnalyze ()
		{
			// any existing report is now out-of-date
			html_report_filename = null;
			xml_report_filename = null;
			text_report_filename = null;

			// just to pick up any change between the original load (a few steps bacl)
			// and the assemblies "right now" sitting on disk
			UpdateAssemblies ();

			Runner.Reset ();
			Runner.Assemblies.Clear ();
			foreach (KeyValuePair<string, AssemblyInfo> kvp in assemblies) {
				// add assemblies references to runner
				AssemblyDefinition ad = kvp.Value.Definition;
				// an invalid assembly (e.g. non-managed code) will be null at this stage
				if (ad != null)
					Runner.Assemblies.Add (ad);
			}

			progress_bar.Maximum = Runner.Assemblies.Count;
		}

		// TODO: Note that it isn't clear whether we can actually call gendarme from a thread...
		[ThreadModel (ThreadModel.SingleThread)]
		private void Analyze ()
		{
			counter = 0;

			// apply settings
			Runner.DefectsLimit = Settings.Default.DefectsLimit;

			Runner.SeverityBitmask.ClearAll ();
			Runner.SeverityBitmask.SetDown ((Severity)Settings.Default.Severity);
			Runner.ConfidenceBitmask.ClearAll ();
			Runner.ConfidenceBitmask.SetDown ((Confidence) Settings.Default.Confidence);

			// wizard limits this as a "global" (all rule) setting
			ApplicabilityScope scope = (ApplicabilityScope) Settings.Default.Scope;
			foreach (IRule rule in Runner.Rules) {
				rule.ApplicabilityScope = scope; 
			}

			// activate rules based on user selection
			UpdateActiveRules ();

			// Initialize / Run / TearDown
			Runner.Execute ();

			// reset rules as the user selected them (since some rules might have
			// turned themselves off is they were not needed for the analysis) so
			// that report used rules correctly
			UpdateActiveRules ();

			BeginInvoke ((Action) (() => Current = Page.Report));
		}

		private bool ConfirmAnalyzeAbort (bool quit)
		{
			string message = String.Format (CultureInfo.CurrentCulture,
				"Abort the current analysis being executed {0}Gendarme ?",
				quit ? "and quit " : String.Empty);
			return (MessageBox.Show (this, message, "Gendarme", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2) == DialogResult.Yes);
		}

		/// <summary>
		/// Update UI before analyzing an assembly.
		/// </summary>
		/// <param name="e">RunnerEventArgs that contains the Assembly being analyzed and the Runner</param>
		internal void PreAssemblyUpdate (RunnerEventArgs e)
		{
			progress_bar.Value = counter++;
			analyze_status_label.Text = String.Format (CultureInfo.CurrentCulture,
				"Processing assembly {0} of {1}",
				counter, e.Runner.Assemblies.Count);
			analyze_assembly_label.Text = "Assembly: " + e.CurrentAssembly.Name.FullName;
		}

		/// <summary>
		/// Update UI after analyzing an assembly.
		/// </summary>
		/// <param name="e">RunnerEventArgs that contains the Assembly being analyzed and the Runner</param>
		internal void PostTypeUpdate (RunnerEventArgs e)
		{
			analyze_defect_label.Text = String.Format (CultureInfo.CurrentCulture, 
				"Defects Found: {0}", e.Runner.Defects.Count);
		}

		#endregion

		#region Report

		private void UpdateReportUI ()
		{
			bool has_defects = (Runner.Defects.Count > 0);
			save_report_button.Enabled = has_defects;
			view_report_button.Enabled = has_defects;
			report_subtitle_label.Text = String.Format (CultureInfo.CurrentCulture,
				"Gendarme has found {0} defects during analysis.",
				has_defects ? Runner.Defects.Count.ToString (CultureInfo.CurrentCulture) : "no");
			cancel_button.Text = "Close";
			next_button.Enabled = false;

			// display an error message and details if we encountered an exception during analysis
			string error = Runner.Error;
			bool has_errors = (error.Length > 0);
			copy_paste_label.Visible = has_errors;
			bugzilla_linklabel.Text = BugzillaUrl;
			bugzilla_linklabel.Visible = has_errors;

			string warnings = Runner.Warnings;
			bool has_warnings = (warnings.Length > 0);

			if (has_errors) {
				unexpected_error_label.Text = "Results are incomplete due to an unexpected error!";
				unexpected_error_label.Visible = true;
				// give priority to errors before warnings
				error_textbox.Text = error;
				error_textbox.Visible = true;
			} else if (has_warnings) {
				unexpected_error_label.Text = "Results might be incomplete due to the following warnings!";
				unexpected_error_label.Visible = true;
				error_textbox.Text = warnings;
				error_textbox.Visible = true;
			} else {
				unexpected_error_label.Visible = false;
				error_textbox.Visible = false;
			}
		}

		private static bool CouldCopyReport (ref string currentName, string fileName)
		{
			// if possible avoid re-creating the report (as it can 
			// be a long operation) and simply copy the file
			bool copy = (currentName != null);
			if (copy) {
				try {
					File.Copy (currentName, fileName);
				}
				catch (Exception) {
					// too many things can go wrong when copying
					copy = false;
				}
			}

			currentName = fileName;
			return copy;
		}

		private ResultWriter GetSelectedWriter (int index, string filename)
		{
			switch (index) {
			case 1:
				if (CouldCopyReport (ref html_report_filename, filename))
					return null;

				return new HtmlResultWriter (Runner, filename);
			case 2:
				if (CouldCopyReport (ref xml_report_filename, filename))
					return null;

				return new XmlResultWriter (Runner, filename);
			case 3:
				if (CouldCopyReport (ref text_report_filename, filename))
					return null;

				return new TextResultWriter (Runner, filename);
			default:
				return null;
			}
		}

		private void SaveReportButtonClick (object sender, EventArgs e)
		{
			if (save_file_dialog.ShowDialog () != DialogResult.OK)
				return;

			using (ResultWriter writer = GetSelectedWriter (save_file_dialog.FilterIndex, save_file_dialog.FileName)) {
				if (writer != null)
					writer.Report ();
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

		private void BugzillaLinkClick (object sender, LinkLabelLinkClickedEventArgs e)
		{
			Open (BugzillaUrl);
		}

		#endregion
	}
}
