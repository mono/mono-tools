//
// Filename BookmarkManager.cs: Manages bookmark saving/restoring and the secondary dialog
// windows
// Author:
//	Rafael Ferreira <raf@ophion.org>
//
// (C) 2005 Rafael Ferreira
//

using System;
using Gtk;
using Glade;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Monodoc {
	public class BookmarkManager {

		[Serializable]
		public class BookmarkBase {
			[XmlAttribute]
			public string ID = Guid.NewGuid ().ToString ();
			[XmlAttribute]
			public string Name = String.Empty;
			
			public BookmarkBase () {}

		}
		[Serializable]
		[XmlInclude (typeof (Bookmark))]
		public class BookmarkGroup : BookmarkBase {
			
			[XmlElement ("Member")]
			public ArrayList Members = new ArrayList ();

			public BookmarkGroup () {}

			public BookmarkGroup (string name) {
				Name = name;

			}
		}
		
		[Serializable]
		public  class Bookmark : BookmarkBase {

			public string Url;
			
			public Bookmark (string name, string url){
				Name = name;
				Url = url;
			}
			
			public Bookmark () {}
		}
		internal class ManageBookmarkDialog {
			[Glade.Widget] Gtk.TreeView bookmarks_treeview;
			[Glade.Widget] Gtk.Window manage_bookmarks_dialog;
			BookmarkGroup root_group;
			Hashtable iter_to_id; 
			string selected_id = string.Empty;
			CellRendererText cell_render;
			const string UNTITLED = "Untitled";
		
			public ManageBookmarkDialog (BookmarkGroup bookmarks) {
				Glade.XML xml = new Glade.XML ("browser.glade","manage_bookmarks_dialog");
				xml.Autoconnect (this);
				iter_to_id = new Hashtable ();
				root_group = bookmarks;
				bookmarks_treeview.RulesHint = true;
				bookmarks_treeview.EnableSearch = true;

				// treeview handlers
				bookmarks_treeview.RowExpanded += new Gtk.RowExpandedHandler (on_row_expanded);
				bookmarks_treeview.Selection.Changed += new EventHandler (on_row_selected);
				bookmarks_treeview.RowActivated += new Gtk.RowActivatedHandler (on_row_activated);
				cell_render = new CellRendererText ();
				cell_render.Edited += new EditedHandler (on_cellrender_edited);
				cell_render.Editable = true;
				
				bookmarks_treeview.AppendColumn ("Column 1", cell_render,"text",0);

				
			}
			
			void on_row_expanded (object sender, Gtk.RowExpandedArgs args) {
			}
			void on_row_selected (object sender, EventArgs args) {
				Gtk.TreeIter iter;
				Gtk.TreeModel model;
				
				if (bookmarks_treeview.Selection.GetSelected (out model, out iter)) {
					selected_id = iter_to_id[iter] as string;
				}

			}
			
			void on_cellrender_edited (object sender, EditedArgs args) {
				
				// root group can't be edited
				if ( selected_id == root_group.ID)
					return;

				BookmarkBase bk = null;
				BookmarkManager.GetBookmarkBase (root_group,selected_id,ref bk);

				if (bk == null ) {
					Console.WriteLine ("error, could not retrieve bookmark:{0}",selected_id);
					return;
				}
				
				// it is not a bookmark
				bk.Name = args.NewText;
				
				
				//refreshing tree_view
				BookmarkManager.Refresh ();
				BuildTreeView ();
			}
			
			void on_row_activated (object sender, Gtk.RowActivatedArgs args)
			{
			}
			
			void OnDelete (object o, DeleteEventArgs args)
			{
				manage_bookmarks_dialog.Destroy ();
			}
			
			void on_NewFolderButton_clicked (object sender, EventArgs args)
			{
				BookmarkManager.AddBookmarkGroup (root_group,selected_id,UNTITLED);
				BookmarkManager.Refresh ();
				BuildTreeView ();
			}
			
			void on_EditButton_clicked (object sender, EventArgs args)
			{
			}
			
			void OnDeleteClicked (object o, EventArgs args)
			{
				if (selected_id != string.Empty) {
					BookmarkManager.DeleteBookmarkBase (root_group,selected_id);
					BookmarkManager.Refresh ();
					BuildTreeView ();
				}

			}

			void OnCancelClicked (object o, EventArgs args)
			{
				//TODO add undo logic
				manage_bookmarks_dialog.Hide ();
			}

			public void Show ()
			{
				BuildTreeView ();
				manage_bookmarks_dialog.ShowAll ();
			}

			void BuildTreeView ()
			{
				TreeStore store = new TreeStore (typeof (string));
				bookmarks_treeview.Model = store;

				TreeIter iter = store.AppendValues (root_group.Name);

				// appending root
				iter_to_id[iter] = root_group.ID;

				// calling the recursevily builder
				BuildTreeViewHelper (root_group,iter,store);

				/*
				foreach (object i in root_group.Members) {
					if (i is Bookmark)
						iter_to_id[store.AppendValues (iter, ((Bookmark)i).Name )] = ((Bookmark)i).ID;
				}
				*/
				bookmarks_treeview.ExpandAll ();
			}
			
			void BuildTreeViewHelper (BookmarkGroup bookmarks, TreeIter iter, TreeStore store)
			{
				TreeIter tmp_iter;
				
				foreach (object i in bookmarks.Members) {
					if (i is BookmarkGroup) {
						tmp_iter = store.AppendValues (iter, ((BookmarkGroup)i).Name );
						iter_to_id[tmp_iter] = ((BookmarkGroup)i).ID;
						BuildTreeViewHelper ((BookmarkGroup)i, tmp_iter, store);
					}
					if (i is Bookmark) {
						tmp_iter = store.AppendValues (iter, ((Bookmark)i).Name);
						iter_to_id[tmp_iter] = ((Bookmark)i).ID;
					}
				}
			}
		}
	    
		internal class AddBookmarkDialog {
			[Glade.Widget] Gtk.Entry name_entry;
			[Glade.Widget] HBox hbox37;
			[Glade.Widget] Gtk.Window add_bookmark_dialog;
			
			string text, url;
			BookmarkGroup root;
			Combo combo;
			Hashtable combo_to_id = new Hashtable ();
			
			public AddBookmarkDialog (BookmarkGroup root_group)
			{
				Glade.XML xml = new Glade.XML ("browser.glade","add_bookmark_dialog");
				xml.Autoconnect (this);
				
				combo = new Combo ();

				ArrayList list = new ArrayList ();
				
				BuildComboList (root_group,list);
				combo.PopdownStrings =  list.ToArray (typeof (string)) as string[];
				combo.AllowEmpty = false;
				combo.Entry.Editable = false;
				combo.DisableActivate ();
				
				// pusihing widget into hbox
				hbox37.PackEnd (combo);
				
				//combo.Entry.Activated += new EventHandler (on_combo_entry_activated);

				root = root_group;
				text = url = String.Empty;
				
			}

			// recursively builds combo box
			private void BuildComboList (BookmarkGroup bookmarks, ArrayList list)
			{
				foreach (object i in bookmarks.Members)
				{
					if (i is BookmarkGroup) {
						BuildComboList (i as BookmarkGroup, list);
					}
					
				}
				list.Add (bookmarks.Name);
				combo_to_id[bookmarks.Name] = bookmarks.ID;
				
			}
		    
			public void on_AddBookmark_delete_event (object o, DeleteEventArgs args)
			{
				add_bookmark_dialog.Destroy ();
			}

			public void on_AddButton_clicked (object o, EventArgs args)
			{
				BookmarkManager.AddBookmark (root,combo_to_id [combo.Entry.Text] as string,name_entry.Text,url);
				add_bookmark_dialog.Hide ();
				BookmarkManager.Refresh ();
			}

			public void on_CancelButton_clicked (object o, EventArgs args)
			{
				add_bookmark_dialog.Hide ();
			}

			public void on_combo_entry_activated (object sender, EventArgs args)
			{
				
			}
			
			public void Show (string T, string U)
			{
				name_entry.Text = T;
				text = T.Trim ();
				url = U.Trim ();
				add_bookmark_dialog.ShowAll ();
			}

		}
		
		// attributes:
		static BookmarkGroup root_group;
		static string bookmark_file;
		static XmlSerializer serializer;
		static Browser _Browser;
		static string current_bookmark_group = String.Empty;
		static AddBookmarkDialog add_window = null;
		static ManageBookmarkDialog edit_window = null;
		
		static Hashtable menu_to_id;
		
		const string ADD_BANNER = " Bookmark this page";
		const string EDIT_BANNER = " Manage bookmarks";
		const string ROOT_NAME = "Bookmarks";

		private static void Refresh () {
			BookmarkManager.Save ();
			BookmarkManager.BuildMenu (_Browser.bookmarksMenu);
		}
		
		private static void Save () {
			using (FileStream file = new FileStream (bookmark_file,FileMode.Create)) {
				serializer.Serialize (file,root_group);
			}
			#if DEBUG
			Console.WriteLine ("bookmarks saved ({0})",root_group.Members.Count);
			#endif
		}
		private static void Load () {
			using (FileStream file = new FileStream (bookmark_file,FileMode.Open)) {
				root_group = (BookmarkGroup)serializer.Deserialize (file);
			}
			#if DEBUG
			Console.WriteLine ("bookmarks loaded ({0})",root_group.Members.Count);
			#endif
		}
		
		public BookmarkManager (Browser browser){
			_Browser = browser;
			
			#if DEBUG
			Console.WriteLine ("Bookmark Manager init");
			#endif

			// discovering bookmark file
			bookmark_file = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			bookmark_file = System.IO.Path.Combine (bookmark_file, "monodoc");
			bookmark_file = System.IO.Path.Combine (bookmark_file, "bookmarks.xml");

			// creating serializer 
			serializer = new XmlSerializer (typeof (BookmarkGroup));

			// trying to load saved bookmarks
			try {
				Load ();
			}catch (Exception e) {
				// no bookmarks found, creating new root
				root_group = new BookmarkGroup (ROOT_NAME);
			}
			
			current_bookmark_group = ROOT_NAME;
			menu_to_id = new Hashtable ();
			BuildMenu (_Browser.bookmarksMenu);

		}
		
		public static void BuildMenu (MenuItem bookmark_menu) {
			Menu aux = (Menu) bookmark_menu.Submenu;
			
			foreach (Widget w in aux.Children) {
				aux.Remove (w);
			}
			
			menu_to_id.Clear ();
			
			//adding Default Items:
			AccelGroup bk_grp = new AccelGroup ();
			_Browser.window1.AddAccelGroup (bk_grp);
			
			ImageMenuItem item;
			item  = new ImageMenuItem (ADD_BANNER);
			//item.Image = new Gtk.Image (Stock.Add,IconSize.Menu);
			item.AddAccelerator ("activate",bk_grp,new AccelKey (Gdk.Key.D,Gdk.ModifierType.ControlMask,AccelFlags.Visible));
			item.Activated += on_add_bookmark_activated;
			aux.Append (item);

			//edit
			item = new ImageMenuItem (EDIT_BANNER);
			item.AddAccelerator ("activate",bk_grp,new AccelKey (Gdk.Key.M,Gdk.ModifierType.ControlMask,AccelFlags.Visible));
			item.Activated += on_edit_bookmark_activated;
			//item.Image = new Gtk.Image (Stock.Edit,Iconsize,Menu);
			aux.Append (item);
			
			// and finally the separtor
			aux.Append (new SeparatorMenuItem ());
			
			BuildMenuHelper (aux,root_group);
			aux.ShowAll ();
		}
		
		private static void BuildMenuHelper (Menu menu, BookmarkGroup group)
		{
			foreach (object i in group.Members) {
				if (!(i is BookmarkGroup))
					continue;
				
				MenuItem item = new MenuItem (((BookmarkGroup)i).Name);
				item.Activated += on_bookmarkgroup_activated;
				menu_to_id[item] = ((BookmarkGroup)i).ID;
				menu.Append (item);
				Menu m = new Menu ();
				item.Submenu = m;
				BuildMenuHelper (m, ((BookmarkGroup)i));
			}
			
			foreach (object i in group.Members) 
			{
				if (i is Bookmark) {
					#if DEBUG
					Console.WriteLine ("appending bookmark: [" + ((Bookmark)i).Name + "]");
					#endif

					MenuItem item = new MenuItem (((Bookmark)i).Name);
					menu_to_id[item] = ((Bookmark)i).ID;
					item.Activated += on_bookmark_activated;
					menu.Append (item);
					menu_to_id[item] = ((Bookmark)i).ID;
				}
			}
		}
		
		// Event Handlers
		static void on_add_bookmark_activated (object sender, EventArgs e){
			add_window = new AddBookmarkDialog (root_group);
			add_window.Show (_Browser.CurrentTab.Title,_Browser.CurrentUrl);
		}
		
		static void on_edit_bookmark_activated (object sender,EventArgs e) {
			edit_window = new ManageBookmarkDialog (root_group);
			edit_window.Show ();
		}

		static void on_bookmark_activated (object sender, EventArgs e)
		{
			// finding the inner label
			BookmarkBase bk = null;
			
			GetBookmarkBase (root_group, menu_to_id[ (MenuItem)sender] as string,ref  bk);
	
			if (bk != null) {
				if (bk is  Bookmark)
						_Browser.LoadUrl (((Bookmark)bk).Url);
			} else {
				Console.WriteLine ("Bookmark error -> could not load bookmark");
			}
		}

		static void on_bookmarkgroup_activated (object sender, EventArgs e)
		{
			if (((MenuItem)sender).Child is Gtk.Label)  {
				Gtk.Label label = (Gtk.Label) ((MenuItem)sender).Child;
				current_bookmark_group = label.Name;
			}
		}
		
		public void EditBookMark () {}

		// static helper methods
		/// <summary> Recursively deletes a bookmark </summary>
		public static void DeleteBookmarkBase (BookmarkGroup bookmarks, string ID) {
		
			foreach (object i in bookmarks.Members) {
				if (i is Bookmark) {
					if (((Bookmark)i).ID == ID) {
						bookmarks.Members.Remove (i);
						return;
					}
					
				} else if (i is BookmarkGroup) {
					if (((BookmarkGroup)i).ID == ID) {
						bookmarks.Members.Remove (i);
						return;
					}
					DeleteBookmarkBase (((BookmarkGroup)i), ID);
				}
			}
		}
		
		/// <summary> Recursively finds a bookmarkbase </summary>
		public static void GetBookmarkBase (BookmarkGroup bookmarks, string ID, ref BookmarkBase retval) {
			
			foreach (object i in bookmarks.Members) {
				if (((BookmarkBase)i).ID == ID) {
					retval = i as BookmarkBase;
					return;
				}
				
				if (i is BookmarkGroup)
					GetBookmarkBase ((BookmarkGroup)i,ID,ref retval);
			}
		}

		/// <summary> Recursively adds a bookmark </summary>
		public static void AddBookmark (BookmarkGroup bookmarks, string parent_ID, string bookmark_text, string bookmark_url) {
			if ( bookmarks.ID == parent_ID) {
				bookmarks.Members.Add (new Bookmark (bookmark_text,bookmark_url) );
				return;
			}
			foreach (object i in bookmarks.Members)
			{
				if (i is BookmarkGroup) {
					AddBookmark (((BookmarkGroup)i), parent_ID,bookmark_text, bookmark_url);
				
				}
			}
		}
		
		/// <summary> Recursively adds a bookmark </summary>
		public static void AddBookmarkGroup (BookmarkGroup bookmarks, string parent_ID, string name)
		{
			if (bookmarks.ID == parent_ID) {
				bookmarks.Members.Add (new BookmarkGroup (name));
				return;
			}
			
			foreach (object i in bookmarks.Members)
			{
				if (i is BookmarkGroup)
					AddBookmarkGroup (((BookmarkGroup)i), parent_ID,name);

				if (i is Bookmark){
					if (((Bookmark)i).ID == parent_ID) {
						bookmarks.Members.Add (new BookmarkGroup (name));
						return;
					}
				}
				
			}
		}
	}
}

