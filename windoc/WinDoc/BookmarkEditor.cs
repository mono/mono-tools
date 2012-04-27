using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinDoc
{
	public partial class BookmarkEditor : Form
	{
		BookmarkManager bkManager;
		List<BookmarkManager.Entry> bindedList;

		public BookmarkEditor (BookmarkManager bkManager)
		{
			this.bkManager = bkManager;
			InitializeComponent();
			LinkDataSource ();
		}

		void LinkDataSource ()
		{
			bindedList = new List<BookmarkManager.Entry> (bkManager.GetAllBookmarks ());
			bkGridView.DataSource = bindedList;
			bkGridView.CellValueChanged += (s, e) => bkManager.CommitBookmarkChange (bindedList[e.RowIndex]);
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close ();
		}
	}
}
