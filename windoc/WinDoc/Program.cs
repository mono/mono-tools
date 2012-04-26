using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Monodoc;

namespace WinDoc
{
	static class Program
	{
		static string MonodocDir;

		[STAThread]
		static void Main()
		{
			PrepareCache ();
			ExtractImages ();
			
			// Load documentation
			Root = RootTree.LoadTree (null);
			
			var winDocPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "WinDoc");
			if (!Directory.Exists (winDocPath))
				Directory.CreateDirectory (winDocPath);
			IndexUpdateManager = new IndexUpdateManager (Root.HelpSources
															.Cast<HelpSource> ()
															.Select (hs => Path.Combine (hs.BaseFilePath, hs.Name + ".zip"))
															.Where (File.Exists),
			                                             winDocPath);
			BookmarkManager = new BookmarkManager (winDocPath);
			
			// Configure the documentation rendering.
			SettingsHandler.Settings.EnableEditing = false;
			SettingsHandler.Settings.preferred_font_size = 200;
			HelpSource.use_css = true;

			Application.ApplicationExit += (s, e) => BookmarkManager.SaveBookmarks ();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run(new MainWindow ());
		}
		
		static void PrepareCache ()
		{
			MonodocDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "WinDoc", "Caches");
			var mdocimages = Path.Combine (MonodocDir, "mdocimages");
			if (!Directory.Exists (mdocimages)){
				try {
					Directory.CreateDirectory (mdocimages);
				} catch {}
			}
		}
		
		static void ExtractImages ()
		{
			var mdocAssembly = typeof (Node).Assembly;
			
			foreach (var res in mdocAssembly.GetManifestResourceNames ()){
				if (!res.EndsWith (".png") || res.EndsWith (".jpg"))
					continue;
				
				var image = Path.Combine (MonodocDir, "mdocimages", res);
				if (File.Exists (image))
					continue;
				
				using (var output = File.Create (image))
					mdocAssembly.GetManifestResourceStream (res).CopyTo (output);
			}
		}

		public static RootTree Root {
			get;
			private set;
		}
		
		public static IndexUpdateManager IndexUpdateManager {
			get;
			private set;
		}
		
		public static BookmarkManager BookmarkManager {
			get;
			private set;
		}
	}
}
