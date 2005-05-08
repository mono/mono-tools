//
// OidCache.cs: OID Cache Management
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;

public class OidCache {

	private Hashtable _cache;
	private bool _modified;
	
	public OidCache ()
	{
		_cache = new Hashtable ();
		_modified = false;
	}
	
	public OidCache (string filename)
		: this ()
	{
		Load (filename);
	}
	
	public void Add (string oid, string description)
	{
		_cache.Add (oid, description);
		_modified = true;
	}
		
	public void Clear ()
	{
		_cache.Clear ();
		_modified = true;
	}
	
	public string Get (string oid)
	{
		return (string) _cache [oid];
	}
	
	public void Load (string filename)
	{
		if (!File.Exists (filename))
			return;

		using (StreamReader sr = new StreamReader (filename)) {
			string oid = sr.ReadLine ();
			while (oid != null) {
				_cache.Add (oid, sr.ReadLine ());
				oid = sr.ReadLine ();
			}
			sr.Close ();
		}
		_modified = false;
	}
	
	public void Save (string filename)
	{
		if (!_modified)
			return;

		try {			
			if (!File.Exists (filename)) {
				string path = Path.GetDirectoryName (filename);
				if (!Directory.Exists (path)) {
					Directory.CreateDirectory (path);
				}
			}

			using (StreamWriter sw = new StreamWriter (filename)) {
				foreach (DictionaryEntry de in _cache) {
					string desc = (string) de.Value;
					// don't cache empty values. maybe we 
					// can download updated data (or from
					// elsewhere) in a future session...
					if ((desc != null) && (desc.Length > 0)) {
						sw.WriteLine (de.Key);
						sw.WriteLine (de.Value);
					}
				}
				sw.Close ();
			}
			_modified = false;
		}
		catch (Exception e) {
			Console.Error.WriteLine ("OID cache couldn't be saved. Cause: {0}", e.ToString ());
		}
	}
}
