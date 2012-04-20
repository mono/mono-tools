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
			this.indexTab = new System.Windows.Forms.TabPage();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.indexResultList = new System.Windows.Forms.ListView();
			this.multipleMatchList = new System.Windows.Forms.ListView();
			this.searchTab = new System.Windows.Forms.TabPage();
			this.listView3 = new System.Windows.Forms.ListView();
			this.docBrowser = new System.Windows.Forms.WebBrowser();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabContainer.SuspendLayout();
			this.treeTab.SuspendLayout();
			this.indexTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
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
			this.toolStrip1.Size = new System.Drawing.Size(827, 27);
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
			this.backButton.Text = "toolStripButton1";
			// 
			// forwardButton
			// 
			this.forwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.forwardButton.Image = ((System.Drawing.Image)(resources.GetObject("forwardButton.Image")));
			this.forwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.forwardButton.Name = "forwardButton";
			this.forwardButton.Size = new System.Drawing.Size(23, 24);
			this.forwardButton.Text = "toolStripButton2";
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
			this.bkAdd.Text = "toolStripButton3";
			this.bkAdd.Click += new System.EventHandler(this.bkAdd_Click);
			// 
			// bkRemove
			// 
			this.bkRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.bkRemove.Image = ((System.Drawing.Image)(resources.GetObject("bkRemove.Image")));
			this.bkRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.bkRemove.Name = "bkRemove";
			this.bkRemove.Size = new System.Drawing.Size(23, 24);
			this.bkRemove.Text = "toolStripButton4";
			this.bkRemove.Click += new System.EventHandler(this.bkRemove_Click);
			// 
			// searchBox
			// 
			this.searchBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.searchBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this.searchBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
			this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.searchBox.Margin = new System.Windows.Forms.Padding(5, 2, 10, 2);
			this.searchBox.Name = "searchBox";
			this.searchBox.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.searchBox.Size = new System.Drawing.Size(100, 23);
			// 
			// bkModify
			// 
			this.bkModify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.bkModify.Image = ((System.Drawing.Image)(resources.GetObject("bkModify.Image")));
			this.bkModify.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.bkModify.Name = "bkModify";
			this.bkModify.Size = new System.Drawing.Size(23, 24);
			this.bkModify.Text = "toolStripButton5";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripLabel1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripLabel1.Image")));
			this.toolStripLabel1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(16, 23);
			this.toolStripLabel1.Text = "toolStripLabel1";
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.indexesProgressBar,
            this.indexesLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 450);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(827, 22);
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
			this.splitContainer1.Location = new System.Drawing.Point(0, 27);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tabContainer);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.docBrowser);
			this.splitContainer1.Size = new System.Drawing.Size(827, 423);
			this.splitContainer1.SplitterDistance = 200;
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
			this.tabContainer.Size = new System.Drawing.Size(194, 417);
			this.tabContainer.TabIndex = 0;
			// 
			// treeTab
			// 
			this.treeTab.Controls.Add(this.docTree);
			this.treeTab.Location = new System.Drawing.Point(4, 22);
			this.treeTab.Name = "treeTab";
			this.treeTab.Padding = new System.Windows.Forms.Padding(3);
			this.treeTab.Size = new System.Drawing.Size(186, 391);
			this.treeTab.TabIndex = 0;
			this.treeTab.Text = "Tree";
			this.treeTab.UseVisualStyleBackColor = true;
			// 
			// docTree
			// 
			this.docTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.docTree.ImageIndex = 0;
			this.docTree.ImageList = this.imageList1;
			this.docTree.Location = new System.Drawing.Point(0, 0);
			this.docTree.Name = "docTree";
			this.docTree.SelectedImageIndex = 0;
			this.docTree.Size = new System.Drawing.Size(186, 391);
			this.docTree.TabIndex = 0;
			// 
			// indexTab
			// 
			this.indexTab.Controls.Add(this.splitContainer2);
			this.indexTab.Location = new System.Drawing.Point(4, 22);
			this.indexTab.Name = "indexTab";
			this.indexTab.Padding = new System.Windows.Forms.Padding(3);
			this.indexTab.Size = new System.Drawing.Size(186, 391);
			this.indexTab.TabIndex = 1;
			this.indexTab.Text = "Index";
			this.indexTab.UseVisualStyleBackColor = true;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(3, 3);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.indexResultList);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.multipleMatchList);
			this.splitContainer2.Size = new System.Drawing.Size(180, 385);
			this.splitContainer2.SplitterDistance = 240;
			this.splitContainer2.TabIndex = 0;
			// 
			// indexResultList
			// 
			this.indexResultList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indexResultList.Location = new System.Drawing.Point(3, 3);
			this.indexResultList.Name = "indexResultList";
			this.indexResultList.Size = new System.Drawing.Size(174, 234);
			this.indexResultList.TabIndex = 0;
			this.indexResultList.UseCompatibleStateImageBehavior = false;
			// 
			// multipleMatchList
			// 
			this.multipleMatchList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.multipleMatchList.Location = new System.Drawing.Point(3, 3);
			this.multipleMatchList.Name = "multipleMatchList";
			this.multipleMatchList.Size = new System.Drawing.Size(174, 135);
			this.multipleMatchList.TabIndex = 0;
			this.multipleMatchList.UseCompatibleStateImageBehavior = false;
			// 
			// searchTab
			// 
			this.searchTab.Controls.Add(this.listView3);
			this.searchTab.Location = new System.Drawing.Point(4, 22);
			this.searchTab.Name = "searchTab";
			this.searchTab.Size = new System.Drawing.Size(261, 391);
			this.searchTab.TabIndex = 2;
			this.searchTab.Text = "Search";
			this.searchTab.UseVisualStyleBackColor = true;
			// 
			// listView3
			// 
			this.listView3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView3.Location = new System.Drawing.Point(3, 3);
			this.listView3.Name = "listView3";
			this.listView3.Size = new System.Drawing.Size(255, 385);
			this.listView3.TabIndex = 0;
			this.listView3.UseCompatibleStateImageBehavior = false;
			// 
			// docBrowser
			// 
			this.docBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.docBrowser.IsWebBrowserContextMenuEnabled = false;
			this.docBrowser.Location = new System.Drawing.Point(3, 3);
			this.docBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.docBrowser.Name = "docBrowser";
			this.docBrowser.Size = new System.Drawing.Size(617, 417);
			this.docBrowser.TabIndex = 0;
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
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(827, 472);
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
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
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
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.ListView indexResultList;
		private System.Windows.Forms.ListView multipleMatchList;
		private System.Windows.Forms.ListView listView3;
		private System.Windows.Forms.ToolStripProgressBar indexesProgressBar;
		private System.Windows.Forms.ToolStripStatusLabel indexesLabel;
		private System.Windows.Forms.ImageList imageList1;

	}
}

