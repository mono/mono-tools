using System;
using System.Runtime.InteropServices;

public class User32 {
	[DllImport ("user32.dll")]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
}

public class Foo {
	public static void Main (string[] args) {
		IntPtr ip = User32.SendMessage (IntPtr.Zero, 0, IntPtr.Zero, "");
		Console.WriteLine (ip);
	}
}