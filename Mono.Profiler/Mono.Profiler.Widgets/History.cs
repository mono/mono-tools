// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Mono.Profiler.Widgets {

	public class LogInfo {

		string detail;
		string filename;

		public LogInfo () {}

		public LogInfo (string filename, string detail)
		{
			this.filename = filename;
			this.detail = detail;
		}

		public string Caption {
			get { return Path.GetFileName (Filename); }
		}

		[XmlAttribute]
		public string Detail {
			get { return detail; }
			set { detail = value; OnChanged (); }
		}

		[XmlAttribute]
		public string Filename {
			get { return filename; }
			set { filename = value; OnChanged (); }
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}

	public class LogInfoList : IEnumerable {

		int max_items = 5;
		List<LogInfo> list = new List<LogInfo> ();

		public int Count {
			get { return list.Count; }
		}

		public LogInfo this [int index] {
			get { return list [index]; }
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		// Used by XmlSerializer
		public void Add (object obj)
		{
			LogInfo info = obj as LogInfo;
			if (info != null && File.Exists (info.Filename)) {
				list.Add (obj as LogInfo);
				OnChanged ();
			}
		}

		public void Prepend (LogInfo info)
		{
			list.Remove (info);
			list.Insert (0, info);
			while (list.Count > max_items)
				list.RemoveAt (max_items);
			OnChanged ();
		}
	}
		
	public class ProfileConfigList : IEnumerable {

		int max_items = 5;
		List<ProfileConfiguration> list = new List<ProfileConfiguration> ();

		public int Count {
			get { return list.Count; }
		}

		public ProfileConfiguration this [int index] {
			get { return list [index]; }
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		// Used by XmlSerializer
		public void Add (object obj)
		{
			ProfileConfiguration config = obj as ProfileConfiguration;
			if (config != null) {
				list.Add (config);
				OnChanged ();
			}
		}

		public void Prepend (ProfileConfiguration config)
		{
			list.Remove (config);
			list.Insert (0, config);
			while (list.Count > max_items)
				list.RemoveAt (max_items);
			OnChanged ();
		}
	}

	public class History {

		[XmlArray]
		[XmlArrayItem (ElementName="ProfileConfiguration", Type=typeof (ProfileConfiguration))]
		public ProfileConfigList Configs;

		[XmlArray]
		[XmlArrayItem (ElementName="LogInfo", Type=typeof (LogInfo))]
		public LogInfoList LogFiles;

		public History ()
		{
			LogFiles = new LogInfoList ();
			LogFiles.Changed += delegate { OnChanged (); };
			Configs = new ProfileConfigList ();
			Configs.Changed += delegate { OnChanged (); };
		}

		public event EventHandler Changed;

		void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		public static History Load ()
		{
			string path = Filename;
			if (!File.Exists (path))
				return new History ();

			History result;
			try {
				XmlSerializer serializer = new XmlSerializer (typeof (History));
				XmlTextReader rdr = new XmlTextReader (path);
				result = (History) serializer.Deserialize (rdr);
				rdr.Close ();
			} catch (Exception) {
				result = new History ();
			}

			return result;
		}

		public void Save ()
		{
			try {
				XmlSerializer serializer = new XmlSerializer (typeof (History));
				using (FileStream fs = File.Create (Filename))
					serializer.Serialize (fs, this);
			} catch (Exception e) {
				Console.WriteLine (e);
			}
		}

		static string filename;
		static string Filename {
			get {
				if (filename == null) {
					string dir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "emveepee");
					if (!Directory.Exists (dir))
						Directory.CreateDirectory (dir);
					filename = Path.Combine (dir, "history.xml");
				}
				return filename;
			}
		}
	}
}
