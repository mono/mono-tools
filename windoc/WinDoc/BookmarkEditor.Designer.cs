namespace WinDoc
{
	partial class BookmarkEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.closeButton = new System.Windows.Forms.Button();
			this.bkGridView = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.bkGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.Location = new System.Drawing.Point(577, 227);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// bkGridView
			// 
			this.bkGridView.AllowUserToResizeColumns = false;
			this.bkGridView.AllowUserToResizeRows = false;
			this.bkGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bkGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.bkGridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.bkGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.bkGridView.Location = new System.Drawing.Point(13, 13);
			this.bkGridView.MultiSelect = false;
			this.bkGridView.Name = "bkGridView";
			this.bkGridView.Size = new System.Drawing.Size(639, 198);
			this.bkGridView.TabIndex = 1;
			// 
			// BookmarkEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(664, 262);
			this.Controls.Add(this.bkGridView);
			this.Controls.Add(this.closeButton);
			this.Name = "BookmarkEditor";
			this.Text = "Bookmark Editor";
			((System.ComponentModel.ISupportInitialize)(this.bkGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.DataGridView bkGridView;
	}
}