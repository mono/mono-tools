using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace IgeMacIntegration {

	public class IgeMacMenuGlobal {

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_connect_window_key_handler (IntPtr window);

		public static void ConnectWindowKeyHandler (Gtk.Window window)
		{
			ige_mac_menu_connect_window_key_handler (window.Handle);
		}

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_set_global_key_handler_enabled (bool enabled);

		public static void SetGlobalKeyHandlerEnabled (bool enabled)
		{
			ige_mac_menu_set_global_key_handler_enabled (enabled);
		}
	}
}
