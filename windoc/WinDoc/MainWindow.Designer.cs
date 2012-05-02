namespace WinDoc
{
	partial class MainWindow
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Loading");
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.backButton = new System.Windows.Forms.ToolStripButton();
			this.forwardButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.bookmarkSelector = new System.Windows.Forms.ToolStripComboBox();
			this.bkAdd = new System.Windows.Forms.ToolStripButton();
			this.bkRemove = new System.Windows.Forms.ToolStripButton();
			this.searchBox = new System.Windows.Forms.ToolStripTextBox();
			this.bkModify = new System.Windows.Forms.ToolStripButton();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.indexesProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.indexesLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tabContainer = new System.Windows.Forms.TabControl();
			this.treeTab = new System.Windows.Forms.TabPage();
			this.docTree = new System.Windows.Forms.TreeView();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.indexTab = new System.Windows.Forms.TabPage();
			this.indexSearchBox = new WinDoc.SearchTextBox();
			this.indexSplitContainer = new System.Windows.Forms.SplitContainer();
			this.indexListResults = new System.Windows.Forms.TreeView();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.multipleMatchList = new System.Windows.Forms.TreeView();
			this.searchTab = new System.Windows.Forms.TabPage();
			this.searchListResults = new System.Windows.Forms.TreeView();
			this.docBrowser = new System.Windows.Forms.WebBrowser();
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabContainer.SuspendLayout();
			this.treeTab.SuspendLayout();
			this.indexTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.indexSplitContainer)).BeginInit();
			this.indexSplitContainer.Panel1.SuspendLayout();
			this.indexSplitContainer.Panel2.SuspendLayout();
			this.indexSplitContainer.SuspendLayout();
			this.searchTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backButton,
            this.forwardButton,
            this.toolStripSeparator1,
            this.bookmarkSelector,
            this.bkAdd,
            this.bkRemove,
            this.searchBox,
            this.bkModify,
            this.toolStripLabel1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 1, 1, 2);
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.Size = new System.Drawing.Size(1339, 30);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// backButton
			// 
			this.backButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.backButton.Image = ((System.Drawing.Image)(resources.GetObject("backButton.Image")));
			this.backButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.backButton.Name = "backButton";
			this.backButton.Size = new System.Drawing.Size(23, 24);
			this.backButton.ToolTipText = "Back";
			// 
			// forwardButton
			// 
			this.forwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.forwardButton.Image = ((System.Drawing.Image)(resources.GetObject("forwardButton.Image")));
			this.forwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.forwardButton.Name = "forwardButton";
			this.forwardButton.Size = new System.Drawing.Size(23, 24);
			this.forwardButton.ToolTipText = "Forward";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(10, 0, 15, 0);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
			// 
			// bookmarkSelector
			// 
			this.bookmarkSelector.Margin = new System.Windows.Forms.Padding(1, 1, 5, 1);
			this.bookmarkSelector.Name = "bookmarkSelector";
			this.bookmarkSelector.Size = new System.Drawing.Size(121, 25);
			// 
			// bkAdd
			// 
			this.bkAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.bkAdd.Image = ((System.Drawing.Image)(resources.GetObject("bkAdd.Image")));
			this.bkAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.bkAdd.Name = "bkAdd";
			this.bkAdd.Size = new System.Drawing.Size(23, 24);
			this.bkAdd.ToolTipText = "Add bookmark";
			this.bkAdd.Click += new System.EventHandler(this.bkAdd_Click);
			// 
			// bkRemove
			// 
			this.bkRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.bkRemove.Image = ((System.Drawing.Image)(resources.GetObject("bkRemove.Image")));
			this.bkRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.bkRemove.Name = "bkRemove";
			this.bkRemove.Size = new System.Drawing.Size(23, 24);
			this.bkRemove.ToolTipText = "Remove bookmark";
			this.bkRemove.Click += new System.EventHandler(this.bkRemove_Click);
			// 
			// searchBox
			// 
			this.searchBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.searchBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this.searchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.searchBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.searchBox.Margin = new System.Windows.Forms.Padding(5, 2, 10, 2);
			this.searchBox.Name = "searchBox";
			this.searchBox.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.searchBox.Size = new System.Drawing.Size(170, 23);
			// 
			// bkModify
			// 
			this.bkModify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.bkModify.Image = ((System.Drawing.Image)(resources.GetObject("bkModify.Image")));
			this.bkModify.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.bkModify.Name = "bkModify";
			this.bkModify.Size = new System.Drawing.Size(23, 24);
			this.bkModify.ToolTipText = "Edit bookmark";
			this.bkModify.Click += new System.EventHandler(this.bkModify_Click);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripLabel1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripLabel1.Image")));
			this.toolStripLabel1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(16, 23);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indexesProgressBar,
            this.indexesLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 555);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1339, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// indexesProgressBar
			// 
			this.indexesProgressBar.Margin = new System.Windows.Forms.Padding(10, 3, 1, 3);
			this.indexesProgressBar.Name = "indexesProgressBar";
			this.indexesProgressBar.Size = new System.Drawing.Size(50, 16);
			this.indexesProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.indexesProgressBar.Visible = false;
			// 
			// indexesLabel
			// 
			this.indexesLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 2);
			this.indexesLabel.Name = "indexesLabel";
			this.indexesLabel.Size = new System.Drawing.Size(140, 17);
			this.indexesLabel.Text = "Indexes are being created";
			this.indexesLabel.Visible = false;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 30);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tabContainer);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.docBrowser);
			this.splitContainer1.Size = new System.Drawing.Size(1339, 525);
			this.splitContainer1.SplitterDistance = 323;
			this.splitContainer1.TabIndex = 2;
			// 
			// tabContainer
			// 
			this.tabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabContainer.Controls.Add(this.treeTab);
			this.tabContainer.Controls.Add(this.indexTab);
			this.tabContainer.Controls.Add(this.searchTab);
			this.tabContainer.Location = new System.Drawing.Point(3, 3);
			this.tabContainer.Name = "tabContainer";
			this.tabContainer.SelectedIndex = 0;
			this.tabContainer.Size = new System.Drawing.Size(317, 518);
			this.tabContainer.TabIndex = 0;
			// 
			// treeTab
			// 
			this.treeTab.Controls.Add(this.docTree);
			this.treeTab.Location = new System.Drawing.Point(4, 22);
			this.treeTab.Name = "treeTab";
			this.treeTab.Padding = new System.Windows.Forms.Padding(3);
			this.treeTab.Size = new System.Drawing.Size(309, 492);
			this.treeTab.TabIndex = 0;
			this.treeTab.Text = "Tree";
			this.treeTab.UseVisualStyleBackColor = true;
			// 
			// docTree
			// 
			this.docTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.docTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.docTree.HideSelection = false;
			this.docTree.ImageIndex = 0;
			this.docTree.ImageList = this.imageList1;
			this.docTree.Location = new System.Drawing.Point(0, 0);
			this.docTree.Name = "docTree";
			this.docTree.SelectedImageIndex = 0;
			this.docTree.Size = new System.Drawing.Size(309, 493);
			this.docTree.TabIndex = 0;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "empty.png");
			this.imageList1.Images.SetKeyName(1, "class.png");
			this.imageList1.Images.SetKeyName(2, "delegate.png");
			this.imageList1.Images.SetKeyName(3, "enumeration.png");
			this.imageList1.Images.SetKeyName(4, "event.png");
			this.imageList1.Images.SetKeyName(5, "extension.png");
			this.imageList1.Images.SetKeyName(6, "field.png");
			this.imageList1.Images.SetKeyName(7, "interface.png");
			this.imageList1.Images.SetKeyName(8, "members.png");
			this.imageList1.Images.SetKeyName(9, "method.png");
			this.imageList1.Images.SetKeyName(10, "namespace.png");
			this.imageList1.Images.SetKeyName(11, "property.png");
			this.imageList1.Images.SetKeyName(12, "structure.png");
			// 
			// indexTab
			// 
			this.indexTab.Controls.Add(this.indexSearchBox);
			this.indexTab.Controls.Add(this.indexSplitContainer);
			this.indexTab.Location = new System.Drawing.Point(4, 22);
			this.indexTab.Name = "indexTab";
			this.indexTab.Padding = new System.Windows.Forms.Padding(3);
			this.indexTab.Size = new System.Drawing.Size(309, 492);
			this.indexTab.TabIndex = 1;
			this.indexTab.Text = "Index";
			this.indexTab.UseVisualStyleBackColor = true;
			// 
			// indexSearchBox
			// 
			this.indexSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indexSearchBox.Enabled = false;
			this.indexSearchBox.Location = new System.Drawing.Point(6, 6);
			this.indexSearchBox.Name = "indexSearchBox";
			this.indexSearchBox.Size = new System.Drawing.Size(297, 22);
			this.indexSearchBox.TabIndex = 1;
			// 
			// indexSplitContainer
			// 
			this.indexSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indexSplitContainer.Location = new System.Drawing.Point(3, 30);
			this.indexSplitContainer.Name = "indexSplitContainer";
			this.indexSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// indexSplitContainer.Panel1
			// 
			this.indexSplitContainer.Panel1.Controls.Add(this.indexListResults);
			// 
			// indexSplitContainer.Panel2
			// 
			this.indexSplitContainer.Panel2.Controls.Add(this.multipleMatchList);
			this.indexSplitContainer.Panel2Collapsed = true;
			this.indexSplitContainer.Size = new System.Drawing.Size(303, 449);
			this.indexSplitContainer.SplitterDistance = 214;
			this.indexSplitContainer.TabIndex = 0;
			// 
			// indexListResults
			// 
			this.indexListResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indexListResults.FullRowSelect = true;
			this.indexListResults.HideSelection = false;
			this.indexListResults.ImageIndex = 0;
			this.indexListResults.ImageList = this.imageList2;
			this.indexListResults.Location = new System.Drawing.Point(0, 0);
			this.indexListResults.Name = "indexListResults";
			treeNode1.Name = "Loading";
			treeNode1.Text = "Loading";
			this.indexListResults.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
			this.indexListResults.SelectedImageIndex = 0;
			this.indexListResults.ShowLines = false;
			this.indexListResults.ShowPlusMinus = false;
			this.indexListResults.ShowRootLines = false;
			this.indexListResults.Size = new System.Drawing.Size(303, 449);
			this.indexListResults.TabIndex = 0;
			// 
			// imageList2
			// 
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList2.Images.SetKeyName(0, "Frame0.png");
			this.imageList2.Images.SetKeyName(1, "Frame1.png");
			this.imageList2.Images.SetKeyName(2, "Frame2.png");
			this.imageList2.Images.SetKeyName(3, "Frame3.png");
			this.imageList2.Images.SetKeyName(4, "Frame4.png");
			this.imageList2.Images.SetKeyName(5, "Frame5.png");
			this.imageList2.Images.SetKeyName(6, "Frame6.png");
			this.imageList2.Images.SetKeyName(7, "Frame7.png");
			this.imageList2.Images.SetKeyName(8, "Frame8.png");
			this.imageList2.Images.SetKeyName(9, "Frame9.png");
			this.imageList2.Images.SetKeyName(10, "Frame10.png");
			this.imageList2.Images.SetKeyName(11, "Frame11.png");
			// 
			// multipleMatchList
			// 
			this.multipleMatchList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.multipleMatchList.FullRowSelect = true;
			this.multipleMatchList.HideSelection = false;
			this.multipleMatchList.Location = new System.Drawing.Point(3, 2);
			this.multipleMatchList.Name = "multipleMatchList";
			this.multipleMatchList.ShowLines = false;
			this.multipleMatchList.ShowPlusMinus = false;
			this.multipleMatchList.ShowRootLines = false;
			this.multipleMatchList.Size = new System.Drawing.Size(300, 229);
			this.multipleMatchList.TabIndex = 0;
			// 
			// searchTab
			// 
			this.searchTab.Controls.Add(this.searchListResults);
			this.searchTab.Location = new System.Drawing.Point(4, 22);
			this.searchTab.Name = "searchTab";
			this.searchTab.Size = new System.Drawing.Size(309, 492);
			this.searchTab.TabIndex = 2;
			this.searchTab.Text = "Search";
			this.searchTab.UseVisualStyleBackColor = true;
			// 
			// searchListResults
			// 
			this.searchListResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchListResults.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.searchListResults.FullRowSelect = true;
			this.searchListResults.HideSelection = false;
			this.searchListResults.ItemHeight = 18;
			this.searchListResults.Location = new System.Drawing.Point(3, 3);
			this.searchListResults.Name = "searchListResults";
			this.searchListResults.ShowLines = false;
			this.searchListResults.ShowPlusMinus = false;
			this.searchListResults.ShowRootLines = false;
			this.searchListResults.Size = new System.Drawing.Size(306, 486);
			this.searchListResults.TabIndex = 0;
			// 
			// docBrowser
			// 
			this.docBrowser.AllowWebBrowserDrop = false;
			this.docBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.docBrowser.IsWebBrowserContextMenuEnabled = false;
			this.docBrowser.Location = new System.Drawing.Point(3, 3);
			this.docBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.docBrowser.Name = "docBrowser";
			this.docBrowser.Size = new System.Drawing.Size(1006, 518);
			this.docBrowser.TabIndex = 0;
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(1339, 577);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainWindow";
			this.Text = "WinDoc";
			this.Load += new System.EventHandler(this.MainWindow_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tabContainer.ResumeLayout(false);
			this.treeTab.ResumeLayout(false);
			this.indexTab.ResumeLayout(false);
			this.indexSplitContainer.Panel1.ResumeLayout(false);
			this.indexSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.indexSplitContainer)).EndInit();
			this.indexSplitContainer.ResumeLayout(false);
			this.searchTab.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton backButton;
		private System.Windows.Forms.ToolStripButton forwardButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripComboBox bookmarkSelector;
		private System.Windows.Forms.ToolStripButton bkAdd;
		private System.Windows.Forms.ToolStripButton bkRemove;
		private System.Windows.Forms.ToolStripTextBox searchBox;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.WebBrowser docBrowser;
		private System.Windows.Forms.TabControl tabContainer;
		private System.Windows.Forms.TabPage treeTab;
		private System.Windows.Forms.TabPage indexTab;
		private System.Windows.Forms.TabPage searchTab;
		private System.Windows.Forms.ToolStripButton bkModify;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.TreeView docTree;
		private System.Windows.Forms.SplitContainer indexSplitContainer;
		private System.Windows.Forms.ToolStripProgressBar indexesProgressBar;
		private System.Windows.Forms.ToolStripStatusLabel indexesLabel;
		private System.Windows.Forms.ImageList imageList1;
		private SearchTextBox indexSearchBox;
		private System.Windows.Forms.TreeView indexListResults;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.TreeView searchListResults;
		private System.Windows.Forms.TreeView multipleMatchList;

	}
}

