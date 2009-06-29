
using System.Runtime.InteropServices;
using System;

public class MapDllImport : Attribute {
	public MapDllImport (string dllName) {
		DllName = dllName;
	}

	public string DllName { get; private set; }
}