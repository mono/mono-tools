using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Monodoc;
using Mono.Options;

namespace WinDoc
{
	static class Program
	{
		static readonly string externalMonodocPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData), "Monodoc");
		static string monodocDir;

		[STAThread]
		static void Main(string[] args)
		{
			var initialUrl = string.Empty;
			var docSources = new List<string> ();
			new OptionSet {
				{ "url=|u=", u => initialUrl = u },
				{ "docdir=", dir => docSources.Add (dir) },
			}.Parse (args);

			if (initialUrl.StartsWith ("mdoc://")) {
				initialUrl = initialUrl.Substring ("mdoc://".Length); // Remove leading scheme
				initialUrl = initialUrl.Substring (0, initialUrl.Length - 1); // Remove trailing '/'
				initialUrl = Uri.UnescapeDataString (initialUrl); // Unescape URL
			}

			// Don't crash if any of these steps fails
			try {
				PrepareCache ();
				SetupLogging ();
				ExtractImages ();
			} catch (Exception e) {
				Console.WriteLine ("Non-fatal exception during initialization: {0}", e);
			}

			// Load documentation
			Directory.SetCurrentDirectory (Path.GetDirectoryName (typeof (Program).Assembly.Location));
			Root = RootTree.LoadTree ();
			foreach (var dir in docSources)
				Root.AddSource (dir);
			if (Directory.Exists (externalMonodocPath))
				Root.AddSource (externalMonodocPath);
			
			var winDocPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "WinDoc");
			if (!Directory.Exists (winDocPath))
				Directory.CreateDirectory (winDocPath);
			IndexUpdateManager = new IndexUpdateManager (Root.HelpSources
															.Cast<HelpSource> ()
															.Select (hs => Path.Combine (hs.BaseFilePath, hs.Name + ".zip"))
															.Where (File.Exists),
			                                             winDocPath);
			BookmarkManager = new BookmarkManager (winDocPath);

			Application.ApplicationExit += (s, e) => BookmarkManager.SaveBookmarks ();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run(new MainWindow (initialUrl));
		}
		
		static void PrepareCache ()
		{
			monodocDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "WinDoc", "Caches");
			var mdocimages = Path.Combine (monodocDir, "mdocimages");
			if (!Directory.Exists (mdocimages))
				Directory.CreateDirectory (mdocimages);
		}
		
		static void ExtractImages ()
		{
			var mdocAssembly = typeof (Node).Assembly;
			
			foreach (var res in mdocAssembly.GetManifestResourceNames ()){
				if (!res.EndsWith (".png") || res.EndsWith (".jpg"))
					continue;
				
				var image = Path.Combine (monodocDir, "mdocimages", res);
				if (File.Exists (image))
					continue;
				
				using (var output = File.Create (image))
					mdocAssembly.GetManifestResourceStream (res).CopyTo (output);
			}
		}

		static void SetupLogging ()
		{
			var log = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "WinDoc", "windoc.log");
			var writer = new StreamWriter (log, true);
			writer.AutoFlush = true;
			Console.SetOut (writer);
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

		public static string MonoDocDir {
			get {
				return monodocDir;
			}
		}
	}
}
