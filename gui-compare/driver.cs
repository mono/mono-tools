
using Gtk;
using System;
using System.IO;

namespace GuiCompare {
	public class CompareDriver {
		public static void Main (string[] args)
		{
			Application.Init ();

			if (args.Length == 0) {
				CompareWindow mw = new CompareWindow ();
				mw.ShowAll();
			}
			else {
				foreach (string a in args) {
					string ext = Path.GetExtension (a);

					if (!(ext == ".dll" || ext == ".exe") || !File.Exists (a))
						continue;

					CompareWindow mw = new CompareWindow ();

					mw.SetAssemblyPath (a);

					mw.ShowAll();
				}
			}

			Console.WriteLine ("calling application.run");

			Application.Run ();
		}
	}
}
