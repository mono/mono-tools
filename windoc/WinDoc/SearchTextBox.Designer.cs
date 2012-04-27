namespace WinDoc
{
	partial class SearchTextBox
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

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.searchBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// searchBox
			// 
			this.searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchBox.Location = new System.Drawing.Point(0, 0);
			this.searchBox.Name = "searchBox";
			this.searchBox.Size = new System.Drawing.Size(167, 20);
			this.searchBox.TabIndex = 0;
			// 
			// SearchTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.searchBox);
			this.Name = "SearchTextBox";
			this.Size = new System.Drawing.Size(167, 22);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox searchBox;
	}
}
