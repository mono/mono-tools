namespace Gendarme {

	partial class Wizard {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager (typeof (Wizard));
			this.wizard_tab_control = new System.Windows.Forms.TabControl ();
			this.welcome_tab_page = new System.Windows.Forms.TabPage ();
			this.welcome_gendarme_label = new System.Windows.Forms.Label ();
			this.label1 = new System.Windows.Forms.Label ();
			this.welcome_link_label = new System.Windows.Forms.LinkLabel ();
			this.label10 = new System.Windows.Forms.Label ();
			this.welcome_title_label = new System.Windows.Forms.Label ();
			this.addfiles_tab_page = new System.Windows.Forms.TabPage ();
			this.add_files_count_label = new System.Windows.Forms.Label ();
			this.label6 = new System.Windows.Forms.Label ();
			this.label5 = new System.Windows.Forms.Label ();
			this.add_files_button = new System.Windows.Forms.Button ();
			this.remove_file_button = new System.Windows.Forms.Button ();
			this.file_list_box = new System.Windows.Forms.ListBox ();
			this.rules_tab_page = new System.Windows.Forms.TabPage ();
			this.rules_save_button = new System.Windows.Forms.Button ();
			this.browse_documentation_button = new System.Windows.Forms.Button ();
			this.rules_count_label = new System.Windows.Forms.Label ();
			this.label8 = new System.Windows.Forms.Label ();
			this.label7 = new System.Windows.Forms.Label ();
			this.rules_tree_view = new System.Windows.Forms.TreeView ();
			this.options_tab_page = new System.Windows.Forms.TabPage ();
			this.options_save_button = new System.Windows.Forms.Button ();
			this.options_all_radiobutton = new System.Windows.Forms.RadioButton ();
			this.options_visible_radiobutton = new System.Windows.Forms.RadioButton ();
			this.label12 = new System.Windows.Forms.Label ();
			this.options_notvisible_radiobutton = new System.Windows.Forms.RadioButton ();
			this.options_severity_combobox = new System.Windows.Forms.ComboBox ();
			this.options_confidence_combobox = new System.Windows.Forms.ComboBox ();
			this.options_limit_updown = new System.Windows.Forms.NumericUpDown ();
			this.options_nolimit_checkbox = new System.Windows.Forms.CheckBox ();
			this.label11 = new System.Windows.Forms.Label ();
			this.label9 = new System.Windows.Forms.Label ();
			this.label4 = new System.Windows.Forms.Label ();
			this.label3 = new System.Windows.Forms.Label ();
			this.label2 = new System.Windows.Forms.Label ();
			this.analyze_tab_page = new System.Windows.Forms.TabPage ();
			this.analyze_defect_label = new System.Windows.Forms.Label ();
			this.analyze_title_label = new System.Windows.Forms.Label ();
			this.analyze_status_label = new System.Windows.Forms.Label ();
			this.analyze_assembly_label = new System.Windows.Forms.Label ();
			this.progress_bar = new System.Windows.Forms.ProgressBar ();
			this.results_tab_page = new System.Windows.Forms.TabPage ();
			this.view_report_button = new System.Windows.Forms.Button ();
			this.save_report_button = new System.Windows.Forms.Button ();
			this.results_title_label = new System.Windows.Forms.Label ();
			this.report_subtitle_label = new System.Windows.Forms.Label ();
			this.help_button = new System.Windows.Forms.Button ();
			this.cancel_button = new System.Windows.Forms.Button ();
			this.next_button = new System.Windows.Forms.Button ();
			this.back_button = new System.Windows.Forms.Button ();
			this.open_file_dialog = new System.Windows.Forms.OpenFileDialog ();
			this.save_file_dialog = new System.Windows.Forms.SaveFileDialog ();
			this.pictureBox1 = new System.Windows.Forms.PictureBox ();
			this.unexpected_error_label = new System.Windows.Forms.Label ();
			this.error_textbox = new System.Windows.Forms.TextBox ();
			this.copy_paste_label = new System.Windows.Forms.Label ();
			this.bugzilla_linklabel = new System.Windows.Forms.LinkLabel ();
			this.wizard_tab_control.SuspendLayout ();
			this.welcome_tab_page.SuspendLayout ();
			this.addfiles_tab_page.SuspendLayout ();
			this.rules_tab_page.SuspendLayout ();
			this.options_tab_page.SuspendLayout ();
			((System.ComponentModel.ISupportInitialize) (this.options_limit_updown)).BeginInit ();
			this.analyze_tab_page.SuspendLayout ();
			this.results_tab_page.SuspendLayout ();
			((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit ();
			this.SuspendLayout ();
			// 
			// wizard_tab_control
			// 
			this.wizard_tab_control.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.wizard_tab_control.Controls.Add (this.welcome_tab_page);
			this.wizard_tab_control.Controls.Add (this.addfiles_tab_page);
			this.wizard_tab_control.Controls.Add (this.rules_tab_page);
			this.wizard_tab_control.Controls.Add (this.options_tab_page);
			this.wizard_tab_control.Controls.Add (this.analyze_tab_page);
			this.wizard_tab_control.Controls.Add (this.results_tab_page);
			this.wizard_tab_control.Location = new System.Drawing.Point (133, 0);
			this.wizard_tab_control.Name = "wizard_tab_control";
			this.wizard_tab_control.SelectedIndex = 0;
			this.wizard_tab_control.Size = new System.Drawing.Size (501, 415);
			this.wizard_tab_control.TabIndex = 0;
			// 
			// welcome_tab_page
			// 
			this.welcome_tab_page.Controls.Add (this.welcome_gendarme_label);
			this.welcome_tab_page.Controls.Add (this.label1);
			this.welcome_tab_page.Controls.Add (this.welcome_link_label);
			this.welcome_tab_page.Controls.Add (this.label10);
			this.welcome_tab_page.Controls.Add (this.welcome_title_label);
			this.welcome_tab_page.Location = new System.Drawing.Point (4, 25);
			this.welcome_tab_page.Name = "welcome_tab_page";
			this.welcome_tab_page.Padding = new System.Windows.Forms.Padding (3);
			this.welcome_tab_page.Size = new System.Drawing.Size (493, 386);
			this.welcome_tab_page.TabIndex = 0;
			this.welcome_tab_page.Text = "Welcome";
			this.welcome_tab_page.UseVisualStyleBackColor = true;
			// 
			// welcome_gendarme_label
			// 
			this.welcome_gendarme_label.AutoSize = true;
			this.welcome_gendarme_label.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.welcome_gendarme_label.Location = new System.Drawing.Point (3, 192);
			this.welcome_gendarme_label.Name = "welcome_gendarme_label";
			this.welcome_gendarme_label.Padding = new System.Windows.Forms.Padding (12, 0, 0, 1);
			this.welcome_gendarme_label.Size = new System.Drawing.Size (68, 14);
			this.welcome_gendarme_label.TabIndex = 2;
			this.welcome_gendarme_label.Text = "Gendarme";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.label1.Location = new System.Drawing.Point (3, 206);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding (12, 0, 0, 1);
			this.label1.Size = new System.Drawing.Size (265, 14);
			this.label1.TabIndex = 1;
			this.label1.Text = "Copyright © 2005-2011 Novell, Inc. and contributors";
			// 
			// welcome_link_label
			// 
			this.welcome_link_label.AutoSize = true;
			this.welcome_link_label.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.welcome_link_label.Location = new System.Drawing.Point (3, 220);
			this.welcome_link_label.Name = "welcome_link_label";
			this.welcome_link_label.Padding = new System.Windows.Forms.Padding (12, 0, 0, 150);
			this.welcome_link_label.Size = new System.Drawing.Size (215, 163);
			this.welcome_link_label.TabIndex = 0;
			this.welcome_link_label.TabStop = true;
			this.welcome_link_label.Text = "http://www.mono-project.com/Gendarme";
			this.welcome_link_label.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.GendarmeLinkClick);
			// 
			// label10
			// 
			this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label10.Location = new System.Drawing.Point (3, 28);
			this.label10.Name = "label10";
			this.label10.Padding = new System.Windows.Forms.Padding (12, 12, 12, 0);
			this.label10.Size = new System.Drawing.Size (487, 355);
			this.label10.TabIndex = 5;
			this.label10.Text = resources.GetString ("label10.Text");
			// 
			// welcome_title_label
			// 
			this.welcome_title_label.AutoSize = true;
			this.welcome_title_label.Dock = System.Windows.Forms.DockStyle.Top;
			this.welcome_title_label.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.welcome_title_label.Location = new System.Drawing.Point (3, 3);
			this.welcome_title_label.Margin = new System.Windows.Forms.Padding (0);
			this.welcome_title_label.Name = "welcome_title_label";
			this.welcome_title_label.Padding = new System.Windows.Forms.Padding (12, 12, 0, 0);
			this.welcome_title_label.Size = new System.Drawing.Size (151, 25);
			this.welcome_title_label.TabIndex = 1;
			this.welcome_title_label.Text = "Welcome to Gendarme ";
			// 
			// addfiles_tab_page
			// 
			this.addfiles_tab_page.Controls.Add (this.add_files_count_label);
			this.addfiles_tab_page.Controls.Add (this.label6);
			this.addfiles_tab_page.Controls.Add (this.label5);
			this.addfiles_tab_page.Controls.Add (this.add_files_button);
			this.addfiles_tab_page.Controls.Add (this.remove_file_button);
			this.addfiles_tab_page.Controls.Add (this.file_list_box);
			this.addfiles_tab_page.Location = new System.Drawing.Point (4, 25);
			this.addfiles_tab_page.Name = "addfiles_tab_page";
			this.addfiles_tab_page.Padding = new System.Windows.Forms.Padding (3);
			this.addfiles_tab_page.Size = new System.Drawing.Size (493, 386);
			this.addfiles_tab_page.TabIndex = 1;
			this.addfiles_tab_page.Text = "Add Files";
			this.addfiles_tab_page.UseVisualStyleBackColor = true;
			// 
			// add_files_count_label
			// 
			this.add_files_count_label.AutoSize = true;
			this.add_files_count_label.Location = new System.Drawing.Point (15, 357);
			this.add_files_count_label.Name = "add_files_count_label";
			this.add_files_count_label.Size = new System.Drawing.Size (85, 13);
			this.add_files_count_label.TabIndex = 9;
			this.add_files_count_label.Text = "{0} files selected";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point (28, 26);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size (284, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Add or remove assembly files to be analyzed by Gendarme.";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label5.Location = new System.Drawing.Point (15, 12);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size (147, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Add Assembly Files        ";
			// 
			// add_files_button
			// 
			this.add_files_button.Location = new System.Drawing.Point (246, 352);
			this.add_files_button.Name = "add_files_button";
			this.add_files_button.Size = new System.Drawing.Size (115, 23);
			this.add_files_button.TabIndex = 6;
			this.add_files_button.Text = "Add files...";
			this.add_files_button.UseVisualStyleBackColor = true;
			this.add_files_button.Click += new System.EventHandler (this.AddFilesButtonClick);
			// 
			// remove_file_button
			// 
			this.remove_file_button.Location = new System.Drawing.Point (367, 352);
			this.remove_file_button.Name = "remove_file_button";
			this.remove_file_button.Size = new System.Drawing.Size (116, 23);
			this.remove_file_button.TabIndex = 5;
			this.remove_file_button.Text = "Remove file";
			this.remove_file_button.UseVisualStyleBackColor = true;
			this.remove_file_button.Click += new System.EventHandler (this.RemoveFileButtonClick);
			// 
			// file_list_box
			// 
			this.file_list_box.FormattingEnabled = true;
			this.file_list_box.HorizontalScrollbar = true;
			this.file_list_box.IntegralHeight = false;
			this.file_list_box.Location = new System.Drawing.Point (15, 48);
			this.file_list_box.Name = "file_list_box";
			this.file_list_box.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.file_list_box.Size = new System.Drawing.Size (468, 301);
			this.file_list_box.TabIndex = 0;
			// 
			// rules_tab_page
			// 
			this.rules_tab_page.Controls.Add (this.rules_save_button);
			this.rules_tab_page.Controls.Add (this.browse_documentation_button);
			this.rules_tab_page.Controls.Add (this.rules_count_label);
			this.rules_tab_page.Controls.Add (this.label8);
			this.rules_tab_page.Controls.Add (this.label7);
			this.rules_tab_page.Controls.Add (this.rules_tree_view);
			this.rules_tab_page.Location = new System.Drawing.Point (4, 25);
			this.rules_tab_page.Name = "rules_tab_page";
			this.rules_tab_page.Size = new System.Drawing.Size (493, 386);
			this.rules_tab_page.TabIndex = 2;
			this.rules_tab_page.Text = "Select Rules";
			this.rules_tab_page.UseVisualStyleBackColor = true;
			// 
			// rules_save_button
			// 
			this.rules_save_button.Location = new System.Drawing.Point (327, 354);
			this.rules_save_button.Name = "rules_save_button";
			this.rules_save_button.Size = new System.Drawing.Size (156, 23);
			this.rules_save_button.TabIndex = 32;
			this.rules_save_button.Text = "Save as default";
			this.rules_save_button.UseVisualStyleBackColor = true;
			this.rules_save_button.Click += new System.EventHandler (this.SaveRulesButtonClick);
			// 
			// browse_documentation_button
			// 
			this.browse_documentation_button.Location = new System.Drawing.Point (165, 354);
			this.browse_documentation_button.Name = "browse_documentation_button";
			this.browse_documentation_button.Size = new System.Drawing.Size (156, 23);
			this.browse_documentation_button.TabIndex = 5;
			this.browse_documentation_button.Text = "Documentation...";
			this.browse_documentation_button.UseVisualStyleBackColor = true;
			this.browse_documentation_button.Click += new System.EventHandler (this.BrowseDocumentationButtonClick);
			// 
			// rules_count_label
			// 
			this.rules_count_label.AutoSize = true;
			this.rules_count_label.Location = new System.Drawing.Point (15, 357);
			this.rules_count_label.Name = "rules_count_label";
			this.rules_count_label.Size = new System.Drawing.Size (112, 13);
			this.rules_count_label.TabIndex = 10;
			this.rules_count_label.Text = "{0} rules are available.";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point (28, 26);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size (308, 13);
			this.label8.TabIndex = 9;
			this.label8.Text = "Select the rules to be executed against the specified assemblies";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label7.Location = new System.Drawing.Point (15, 12);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size (79, 13);
			this.label7.TabIndex = 8;
			this.label7.Text = "Select Rules";
			// 
			// rules_tree_view
			// 
			this.rules_tree_view.CheckBoxes = true;
			this.rules_tree_view.HideSelection = false;
			this.rules_tree_view.HotTracking = true;
			this.rules_tree_view.Location = new System.Drawing.Point (15, 47);
			this.rules_tree_view.Name = "rules_tree_view";
			this.rules_tree_view.ShowNodeToolTips = true;
			this.rules_tree_view.Size = new System.Drawing.Size (468, 301);
			this.rules_tree_view.TabIndex = 0;
			this.rules_tree_view.AfterCheck += new System.Windows.Forms.TreeViewEventHandler (this.RulesTreeViewAfterCheck);
			// 
			// options_tab_page
			// 
			this.options_tab_page.Controls.Add (this.options_save_button);
			this.options_tab_page.Controls.Add (this.options_all_radiobutton);
			this.options_tab_page.Controls.Add (this.options_visible_radiobutton);
			this.options_tab_page.Controls.Add (this.label12);
			this.options_tab_page.Controls.Add (this.options_notvisible_radiobutton);
			this.options_tab_page.Controls.Add (this.options_severity_combobox);
			this.options_tab_page.Controls.Add (this.options_confidence_combobox);
			this.options_tab_page.Controls.Add (this.options_limit_updown);
			this.options_tab_page.Controls.Add (this.options_nolimit_checkbox);
			this.options_tab_page.Controls.Add (this.label11);
			this.options_tab_page.Controls.Add (this.label9);
			this.options_tab_page.Controls.Add (this.label4);
			this.options_tab_page.Controls.Add (this.label3);
			this.options_tab_page.Controls.Add (this.label2);
			this.options_tab_page.Location = new System.Drawing.Point (4, 25);
			this.options_tab_page.Name = "options_tab_page";
			this.options_tab_page.Padding = new System.Windows.Forms.Padding (3);
			this.options_tab_page.Size = new System.Drawing.Size (493, 386);
			this.options_tab_page.TabIndex = 5;
			this.options_tab_page.Text = "Options";
			this.options_tab_page.UseVisualStyleBackColor = true;
			// 
			// options_save_button
			// 
			this.options_save_button.Location = new System.Drawing.Point (327, 344);
			this.options_save_button.Name = "options_save_button";
			this.options_save_button.Size = new System.Drawing.Size (156, 23);
			this.options_save_button.TabIndex = 31;
			this.options_save_button.Text = "Save as default";
			this.options_save_button.UseVisualStyleBackColor = true;
			this.options_save_button.Click += new System.EventHandler (this.OnOptionsSaveClick);
			// 
			// options_all_radiobutton
			// 
			this.options_all_radiobutton.AutoSize = true;
			this.options_all_radiobutton.Location = new System.Drawing.Point (18, 290);
			this.options_all_radiobutton.Name = "options_all_radiobutton";
			this.options_all_radiobutton.Size = new System.Drawing.Size (240, 17);
			this.options_all_radiobutton.TabIndex = 30;
			this.options_all_radiobutton.Text = "All code, visible or not, outside the assemblies";
			this.options_all_radiobutton.UseVisualStyleBackColor = true;
			// 
			// options_visible_radiobutton
			// 
			this.options_visible_radiobutton.AutoSize = true;
			this.options_visible_radiobutton.Checked = true;
			this.options_visible_radiobutton.Location = new System.Drawing.Point (18, 267);
			this.options_visible_radiobutton.Name = "options_visible_radiobutton";
			this.options_visible_radiobutton.Size = new System.Drawing.Size (210, 17);
			this.options_visible_radiobutton.TabIndex = 29;
			this.options_visible_radiobutton.TabStop = true;
			this.options_visible_radiobutton.Text = "All visible code (outside the assemblies)";
			this.options_visible_radiobutton.UseVisualStyleBackColor = true;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point (15, 251);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size (43, 13);
			this.label12.TabIndex = 28;
			this.label12.Text = "Visibility";
			// 
			// options_notvisible_radiobutton
			// 
			this.options_notvisible_radiobutton.AutoSize = true;
			this.options_notvisible_radiobutton.Location = new System.Drawing.Point (18, 313);
			this.options_notvisible_radiobutton.Name = "options_notvisible_radiobutton";
			this.options_notvisible_radiobutton.Size = new System.Drawing.Size (238, 17);
			this.options_notvisible_radiobutton.TabIndex = 27;
			this.options_notvisible_radiobutton.Text = "Only code not visible outside the assemnblies";
			this.options_notvisible_radiobutton.UseVisualStyleBackColor = true;
			// 
			// options_severity_combobox
			// 
			this.options_severity_combobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.options_severity_combobox.FormattingEnabled = true;
			this.options_severity_combobox.Items.AddRange (new object [] {
            "All - Include all defects, including audits",
            "Low - Include all defects, except audits",
            "Medium - Include most defects, except minor ones",
            "High - Only include defects that can have large effects",
            "Critical - Only include critical defects (few)"});
			this.options_severity_combobox.Location = new System.Drawing.Point (18, 147);
			this.options_severity_combobox.Name = "options_severity_combobox";
			this.options_severity_combobox.Size = new System.Drawing.Size (465, 21);
			this.options_severity_combobox.TabIndex = 25;
			// 
			// options_confidence_combobox
			// 
			this.options_confidence_combobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.options_confidence_combobox.Items.AddRange (new object [] {
            "All - Include all defects",
            "Normal - Include all defects except if its confidence level is low",
            "High - Include only defects where it\'s likely that no false positives are present" +
                "",
            "Total - Include only defects where the confidence level is 100%"});
			this.options_confidence_combobox.Location = new System.Drawing.Point (18, 204);
			this.options_confidence_combobox.Name = "options_confidence_combobox";
			this.options_confidence_combobox.Size = new System.Drawing.Size (465, 21);
			this.options_confidence_combobox.TabIndex = 26;
			// 
			// options_limit_updown
			// 
			this.options_limit_updown.Increment = new decimal (new int [] {
            100,
            0,
            0,
            0});
			this.options_limit_updown.Location = new System.Drawing.Point (18, 95);
			this.options_limit_updown.Maximum = new decimal (new int [] {
            1000000,
            0,
            0,
            0});
			this.options_limit_updown.Minimum = new decimal (new int [] {
            1,
            0,
            0,
            0});
			this.options_limit_updown.Name = "options_limit_updown";
			this.options_limit_updown.Size = new System.Drawing.Size (136, 20);
			this.options_limit_updown.TabIndex = 18;
			this.options_limit_updown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.options_limit_updown.Value = global::Gendarme.Properties.Settings.Default.DefectsLimit;
			// 
			// options_nolimit_checkbox
			// 
			this.options_nolimit_checkbox.AutoSize = true;
			this.options_nolimit_checkbox.Location = new System.Drawing.Point (165, 98);
			this.options_nolimit_checkbox.Name = "options_nolimit_checkbox";
			this.options_nolimit_checkbox.Size = new System.Drawing.Size (64, 17);
			this.options_nolimit_checkbox.TabIndex = 17;
			this.options_nolimit_checkbox.Text = "No Limit";
			this.options_nolimit_checkbox.UseVisualStyleBackColor = true;
			this.options_nolimit_checkbox.CheckedChanged += new System.EventHandler (this.OptionsNoLimitCheckboxCheckedChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point (15, 188);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size (83, 13);
			this.label11.TabIndex = 15;
			this.label11.Text = "Confidence filter";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point (15, 131);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size (67, 13);
			this.label9.TabIndex = 14;
			this.label9.Text = "Severity filter";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point (28, 26);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size (255, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Limit and/or filter the defects reported during analysis";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (15, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (139, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Maximum number of defects";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.label2.Location = new System.Drawing.Point (15, 12);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (50, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Options";
			// 
			// analyze_tab_page
			// 
			this.analyze_tab_page.Controls.Add (this.analyze_defect_label);
			this.analyze_tab_page.Controls.Add (this.analyze_title_label);
			this.analyze_tab_page.Controls.Add (this.analyze_status_label);
			this.analyze_tab_page.Controls.Add (this.analyze_assembly_label);
			this.analyze_tab_page.Controls.Add (this.progress_bar);
			this.analyze_tab_page.Location = new System.Drawing.Point (4, 25);
			this.analyze_tab_page.Name = "analyze_tab_page";
			this.analyze_tab_page.Size = new System.Drawing.Size (493, 386);
			this.analyze_tab_page.TabIndex = 3;
			this.analyze_tab_page.Text = "Analyze";
			this.analyze_tab_page.UseVisualStyleBackColor = true;
			// 
			// analyze_defect_label
			// 
			this.analyze_defect_label.AutoSize = true;
			this.analyze_defect_label.Location = new System.Drawing.Point (15, 105);
			this.analyze_defect_label.Name = "analyze_defect_label";
			this.analyze_defect_label.Size = new System.Drawing.Size (89, 13);
			this.analyze_defect_label.TabIndex = 10;
			this.analyze_defect_label.Text = "Defects Found: 0";
			// 
			// analyze_title_label
			// 
			this.analyze_title_label.AutoSize = true;
			this.analyze_title_label.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.analyze_title_label.Location = new System.Drawing.Point (15, 12);
			this.analyze_title_label.Name = "analyze_title_label";
			this.analyze_title_label.Size = new System.Drawing.Size (131, 13);
			this.analyze_title_label.TabIndex = 9;
			this.analyze_title_label.Text = "Analysis in progress...";
			// 
			// analyze_status_label
			// 
			this.analyze_status_label.AutoSize = true;
			this.analyze_status_label.Location = new System.Drawing.Point (15, 57);
			this.analyze_status_label.Name = "analyze_status_label";
			this.analyze_status_label.Size = new System.Drawing.Size (135, 13);
			this.analyze_status_label.TabIndex = 2;
			this.analyze_status_label.Text = "Processing assembly 0 of 1";
			// 
			// analyze_assembly_label
			// 
			this.analyze_assembly_label.AutoSize = true;
			this.analyze_assembly_label.Location = new System.Drawing.Point (15, 80);
			this.analyze_assembly_label.Name = "analyze_assembly_label";
			this.analyze_assembly_label.Size = new System.Drawing.Size (57, 13);
			this.analyze_assembly_label.TabIndex = 1;
			this.analyze_assembly_label.Text = "Assembly: ";
			// 
			// progress_bar
			// 
			this.progress_bar.Location = new System.Drawing.Point (18, 133);
			this.progress_bar.Name = "progress_bar";
			this.progress_bar.Size = new System.Drawing.Size (465, 23);
			this.progress_bar.TabIndex = 0;
			// 
			// results_tab_page
			// 
			this.results_tab_page.Controls.Add (this.bugzilla_linklabel);
			this.results_tab_page.Controls.Add (this.copy_paste_label);
			this.results_tab_page.Controls.Add (this.error_textbox);
			this.results_tab_page.Controls.Add (this.unexpected_error_label);
			this.results_tab_page.Controls.Add (this.view_report_button);
			this.results_tab_page.Controls.Add (this.save_report_button);
			this.results_tab_page.Controls.Add (this.results_title_label);
			this.results_tab_page.Controls.Add (this.report_subtitle_label);
			this.results_tab_page.Location = new System.Drawing.Point (4, 25);
			this.results_tab_page.Name = "results_tab_page";
			this.results_tab_page.Size = new System.Drawing.Size (493, 386);
			this.results_tab_page.TabIndex = 4;
			this.results_tab_page.Text = "Results";
			this.results_tab_page.UseVisualStyleBackColor = true;
			// 
			// view_report_button
			// 
			this.view_report_button.Location = new System.Drawing.Point (15, 126);
			this.view_report_button.Name = "view_report_button";
			this.view_report_button.Size = new System.Drawing.Size (142, 23);
			this.view_report_button.TabIndex = 11;
			this.view_report_button.Text = "View Report...";
			this.view_report_button.UseVisualStyleBackColor = true;
			this.view_report_button.Click += new System.EventHandler (this.ViewReportButtonClick);
			// 
			// save_report_button
			// 
			this.save_report_button.Location = new System.Drawing.Point (15, 97);
			this.save_report_button.Name = "save_report_button";
			this.save_report_button.Size = new System.Drawing.Size (142, 23);
			this.save_report_button.TabIndex = 10;
			this.save_report_button.Text = "Save Report...";
			this.save_report_button.UseVisualStyleBackColor = true;
			this.save_report_button.Click += new System.EventHandler (this.SaveReportButtonClick);
			// 
			// results_title_label
			// 
			this.results_title_label.AutoSize = true;
			this.results_title_label.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.results_title_label.Location = new System.Drawing.Point (15, 12);
			this.results_title_label.Name = "results_title_label";
			this.results_title_label.Size = new System.Drawing.Size (99, 13);
			this.results_title_label.TabIndex = 9;
			this.results_title_label.Text = "Analysis Results";
			// 
			// report_subtitle_label
			// 
			this.report_subtitle_label.AutoSize = true;
			this.report_subtitle_label.Location = new System.Drawing.Point (28, 26);
			this.report_subtitle_label.Name = "report_subtitle_label";
			this.report_subtitle_label.Size = new System.Drawing.Size (236, 13);
			this.report_subtitle_label.TabIndex = 0;
			this.report_subtitle_label.Text = "Gendarme has found {0} defects during analysis.";
			// 
			// help_button
			// 
			this.help_button.Location = new System.Drawing.Point (545, 411);
			this.help_button.Name = "help_button";
			this.help_button.Size = new System.Drawing.Size (75, 23);
			this.help_button.TabIndex = 4;
			this.help_button.Text = "Help";
			this.help_button.UseVisualStyleBackColor = true;
			this.help_button.Click += new System.EventHandler (this.HelpButtonClick);
			// 
			// cancel_button
			// 
			this.cancel_button.Location = new System.Drawing.Point (464, 411);
			this.cancel_button.Name = "cancel_button";
			this.cancel_button.Size = new System.Drawing.Size (75, 23);
			this.cancel_button.TabIndex = 3;
			this.cancel_button.Text = "Cancel";
			this.cancel_button.UseVisualStyleBackColor = true;
			this.cancel_button.Click += new System.EventHandler (this.CancelButtonClick);
			// 
			// next_button
			// 
			this.next_button.Location = new System.Drawing.Point (383, 411);
			this.next_button.Name = "next_button";
			this.next_button.Size = new System.Drawing.Size (75, 23);
			this.next_button.TabIndex = 2;
			this.next_button.Text = "Next >";
			this.next_button.UseVisualStyleBackColor = true;
			this.next_button.Click += new System.EventHandler (this.NextButtonClick);
			// 
			// back_button
			// 
			this.back_button.Location = new System.Drawing.Point (302, 411);
			this.back_button.Name = "back_button";
			this.back_button.Size = new System.Drawing.Size (75, 23);
			this.back_button.TabIndex = 1;
			this.back_button.Text = "< Back";
			this.back_button.UseVisualStyleBackColor = true;
			this.back_button.Click += new System.EventHandler (this.BackButtonClick);
			// 
			// open_file_dialog
			// 
			this.open_file_dialog.Filter = "Assemblies (*.exe;*.dll)|*.exe;*.dll|All files (*.*)|*.*";
			this.open_file_dialog.Multiselect = true;
			this.open_file_dialog.Title = "Add Assemblies...";
			// 
			// save_file_dialog
			// 
			this.save_file_dialog.DefaultExt = "*.html";
			this.save_file_dialog.Filter = "HTML report (*.html)|*.html|XML report (*.xml)|*.xml|Text report (*.txt)|*.txt";
			this.save_file_dialog.Title = "Save Report To...";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Gendarme.Properties.Resources.gendarme;
			this.pictureBox1.Location = new System.Drawing.Point (12, 25);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size (128, 128);
			this.pictureBox1.TabIndex = 10;
			this.pictureBox1.TabStop = false;
			// 
			// unexpected_error_label
			// 
			this.unexpected_error_label.AutoSize = true;
			this.unexpected_error_label.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.unexpected_error_label.ForeColor = System.Drawing.Color.Red;
			this.unexpected_error_label.Location = new System.Drawing.Point (28, 49);
			this.unexpected_error_label.Name = "unexpected_error_label";
			this.unexpected_error_label.Size = new System.Drawing.Size (298, 13);
			this.unexpected_error_label.TabIndex = 13;
			this.unexpected_error_label.Text = "Results are incomplete due to an unexpected error!";
			this.unexpected_error_label.Visible = false;
			// 
			// error_textbox
			// 
			this.error_textbox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				    | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.error_textbox.Location = new System.Drawing.Point (18, 207);
			this.error_textbox.Multiline = true;
			this.error_textbox.Name = "error_textbox";
			this.error_textbox.ReadOnly = true;
			this.error_textbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.error_textbox.Size = new System.Drawing.Size (465, 163);
			this.error_textbox.TabIndex = 14;
			this.error_textbox.Visible = false;
			// 
			// copy_paste_label
			// 
			this.copy_paste_label.AutoSize = true;
			this.copy_paste_label.Location = new System.Drawing.Point (15, 172);
			this.copy_paste_label.Name = "copy_paste_label";
			this.copy_paste_label.Size = new System.Drawing.Size (300, 13);
			this.copy_paste_label.TabIndex = 15;
			this.copy_paste_label.Text = "Please copy-paste the following information on a bug report to:";
			this.copy_paste_label.Visible = false;
			// 
			// bugzilla_linklabel
			// 
			this.bugzilla_linklabel.AutoSize = true;
			this.bugzilla_linklabel.Location = new System.Drawing.Point (15, 185);
			this.bugzilla_linklabel.Name = "bugzilla_linklabel";
			this.bugzilla_linklabel.Size = new System.Drawing.Size (127, 13);
			this.bugzilla_linklabel.TabIndex = 16;
			this.bugzilla_linklabel.TabStop = true;
			this.bugzilla_linklabel.Text = "http://bugzilla.novell.com";
			this.bugzilla_linklabel.Visible = false;
			this.bugzilla_linklabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.BugzillaLinkClick);
			// 
			// Wizard
			// 
			this.AcceptButton = this.next_button;
			this.ClientSize = new System.Drawing.Size (634, 448);
			this.ControlBox = false;
			this.Controls.Add (this.pictureBox1);
			this.Controls.Add (this.back_button);
			this.Controls.Add (this.next_button);
			this.Controls.Add (this.cancel_button);
			this.Controls.Add (this.help_button);
			this.Controls.Add (this.wizard_tab_control);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon) (resources.GetObject ("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size (640, 480);
			this.MinimumSize = new System.Drawing.Size (640, 480);
			this.Name = "Wizard";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Gendarme";
			this.wizard_tab_control.ResumeLayout (false);
			this.welcome_tab_page.ResumeLayout (false);
			this.welcome_tab_page.PerformLayout ();
			this.addfiles_tab_page.ResumeLayout (false);
			this.addfiles_tab_page.PerformLayout ();
			this.rules_tab_page.ResumeLayout (false);
			this.rules_tab_page.PerformLayout ();
			this.options_tab_page.ResumeLayout (false);
			this.options_tab_page.PerformLayout ();
			((System.ComponentModel.ISupportInitialize) (this.options_limit_updown)).EndInit ();
			this.analyze_tab_page.ResumeLayout (false);
			this.analyze_tab_page.PerformLayout ();
			this.results_tab_page.ResumeLayout (false);
			this.results_tab_page.PerformLayout ();
			((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit ();
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.TabControl wizard_tab_control;
		private System.Windows.Forms.TabPage welcome_tab_page;
		private System.Windows.Forms.TabPage addfiles_tab_page;
		private System.Windows.Forms.Button help_button;
		private System.Windows.Forms.Button cancel_button;
		private System.Windows.Forms.LinkLabel welcome_link_label;
		private System.Windows.Forms.TabPage rules_tab_page;
		private System.Windows.Forms.TreeView rules_tree_view;
		private System.Windows.Forms.TabPage analyze_tab_page;
		private System.Windows.Forms.TabPage results_tab_page;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label welcome_gendarme_label;
		private System.Windows.Forms.Button next_button;
		private System.Windows.Forms.Button back_button;
		private System.Windows.Forms.Button add_files_button;
		private System.Windows.Forms.Button remove_file_button;
		private System.Windows.Forms.ListBox file_list_box;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.OpenFileDialog open_file_dialog;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label rules_count_label;
		private System.Windows.Forms.Button browse_documentation_button;
		private System.Windows.Forms.Label report_subtitle_label;
		private System.Windows.Forms.SaveFileDialog save_file_dialog;
		private System.Windows.Forms.Label welcome_title_label;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button view_report_button;
		private System.Windows.Forms.Button save_report_button;
		private System.Windows.Forms.Label results_title_label;
		private System.Windows.Forms.Label add_files_count_label;
		private System.Windows.Forms.Label analyze_title_label;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label analyze_assembly_label;
		private System.Windows.Forms.ProgressBar progress_bar;
		private System.Windows.Forms.Label analyze_status_label;
		private System.Windows.Forms.Label analyze_defect_label;
		private System.Windows.Forms.TabPage options_tab_page;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown options_limit_updown;
		private System.Windows.Forms.CheckBox options_nolimit_checkbox;
		private System.Windows.Forms.ComboBox options_confidence_combobox;
		private System.Windows.Forms.ComboBox options_severity_combobox;
		private System.Windows.Forms.RadioButton options_all_radiobutton;
		private System.Windows.Forms.RadioButton options_visible_radiobutton;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.RadioButton options_notvisible_radiobutton;
		private System.Windows.Forms.Button options_save_button;
		private System.Windows.Forms.Button rules_save_button;
		private System.Windows.Forms.Label unexpected_error_label;
		private System.Windows.Forms.Label copy_paste_label;
		private System.Windows.Forms.TextBox error_textbox;
		private System.Windows.Forms.LinkLabel bugzilla_linklabel;
	}
}

