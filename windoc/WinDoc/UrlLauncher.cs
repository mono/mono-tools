using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinDoc
{
	public static class UrlLauncher
	{
		public static void Launch (string url)
		{
			if (string.IsNullOrEmpty (url))
				throw new ArgumentNullException (url);
			Process.Start (new ProcessStartInfo (url));
		}
	}
}
