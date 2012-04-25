using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinDoc
{
	public partial class SearchTextBox : UserControl
	{
		const int SearchIntervalTime = 400;

		Timer timer = new Timer ();

		public event EventHandler SearchTextChanged;

		public SearchTextBox ()
		{
			InitializeComponent();
			Initialize ();
		}

		public SearchTextBox (TextBox searchBox)
		{
			this.searchBox = searchBox;
			Initialize ();
		}

		void Initialize ()
		{
			timer.Interval = SearchIntervalTime;
			timer.Tick += TickCallback;
			searchBox.TextChanged += TextChangedCallback;
		}

		public override string Text {
			get {
				return searchBox.Text;
			}
		}

		void TextChangedCallback (object sender, EventArgs e)
		{
			timer.Stop ();
			timer.Start ();
		}

		void TickCallback (object sender, EventArgs e)
		{
			timer.Stop ();

			var text = searchBox.Text;
			var evt = SearchTextChanged;

			if (string.IsNullOrEmpty (text) || evt == null)
				return;
			if (!searchBox.AutoCompleteCustomSource.Contains (text)) {
				searchBox.AutoCompleteCustomSource.Add (text);
				if (searchBox.AutoCompleteCustomSource.Count > 10)
					searchBox.AutoCompleteCustomSource.RemoveAt (0);
			}
			evt (this, EventArgs.Empty);
		}
	}
}
