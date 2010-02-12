// Config.cs
//
// Copyright (c) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GuiCompare
{
	public class CompareHistory {
		public DateTime CompareTime;
		public int Errors;
		public int Missing;
		public int Extras;
		public int Todos;
		public int Niexs;
	}
	
	public class CompareDefinition {
		public bool ReferenceIsInfo = true;
		public string ReferencePath = String.Empty;
		public bool TargetIsInfo = true;
		public string TargetPath = String.Empty;
		public bool IsCustom = false;
		public string Title = String.Empty;
		
		[XmlElement ("History", typeof (CompareHistory))]
		public CompareHistory [] History;

		public CompareDefinition ()
		{
		}
		
		public CompareDefinition (bool referenceIsInfo, string rpath, bool targetIsInfo, string tpath)
		{
			ReferenceIsInfo = referenceIsInfo;
			ReferencePath = rpath;
			TargetIsInfo = targetIsInfo;
			TargetPath = tpath;

			History = new CompareHistory[0];
		}
		
		public string GetKey ()
		{
			return String.Format ("{0}->{1}", ReferencePath, TargetPath);
		}
		
		// Returns a suitable title
		public override string ToString ()
		{
			return (Title == ""
			        ? String.Format ("{2}{0} -> {1}", Path.GetFileName (ReferencePath), Path.GetFileName (TargetPath),
			                         IsCustom ? "Custom: ": "")
			        : Title);
		}
		
		public override bool Equals (object o)
		{
			if (!(o is CompareDefinition))
				return false;
			CompareDefinition cd = (CompareDefinition)o;
			return (ReferencePath == cd.ReferencePath && ReferenceIsInfo == cd.ReferenceIsInfo &&
			        TargetPath == cd.TargetPath && TargetIsInfo == cd.TargetIsInfo);
		}

		public override int GetHashCode ()
		{
			return GetKey().GetHashCode();
		}

		public void AddHistoryEntry (CompareHistory history)
		{
			CompareHistory[] new_history = new CompareHistory[History == null ? 1 : History.Length + 1];
			if (History != null)
				History.CopyTo (new_history, 0);
			new_history[new_history.Length - 1] = history;
			History = new_history;
		}
	}
	
	public class Config
	{
		public bool ShowErrors = true;
		public bool ShowMissing = true;
		public bool ShowExtra = true;
		public bool ShowTodo = true;
		public bool ShowPresent = true;
		public bool ShowNotImplemented = true;
		
		[XmlElement ("Recent", typeof (CompareDefinition))]
		public CompareDefinition [] Recent;

		static string config_dir, settings_file;
		static XmlSerializer serializer = new XmlSerializer (typeof (Config));
		
		static Config ()
		{
			config_dir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			try {
				Directory.CreateDirectory (config_dir);
			} catch {
			}
			settings_file = System.IO.Path.Combine (config_dir, "guicompare.xml");
		}
		
		public static Config GetConfig ()
		{
			if (File.Exists (settings_file)){
				try {
					return (Config) serializer.Deserialize (new XmlTextReader (settings_file));
				} catch {
					return new Config ();
				}
			} else
				return new Config ();
		}
		
		public void Save ()
		{
			try {
				Directory.CreateDirectory (config_dir);
			} catch {}
			
			try {
				File.Delete (settings_file);
			} catch {}
			
			try {
				using (FileStream fs = File.Create (settings_file)){
					serializer.Serialize (fs, this);
				}
			} catch  (Exception e){
				Console.WriteLine ("Saving {0}", e);
			}
		}
		
		public void MoveToTop (CompareDefinition cd)
		{
			CompareDefinition [] copy = new CompareDefinition [Recent.Length];
			copy [0] = cd;
			for (int i = 0, j = 0; j < Recent.Length; j++){
				if (Recent [j] == cd)
					continue;
				i++;
				copy [i] = Recent [j];
			}
			Recent = copy;
		}
			
		public void AddRecent (CompareDefinition cd)
		{
			if (Recent != null) {
				for (int i = 0; i < Recent.Length; i ++) {
					if (Recent[i].GetKey() == cd.GetKey()) {
						MoveToTop (Recent[i]);
						return;
					}
				}
			}
			
			if (Recent == null || Recent.Length < 15){
				CompareDefinition [] copy = new CompareDefinition [Recent == null ? 1 : Recent.Length+1];
				copy [0] = cd;
				if (Recent != null)
					Recent.CopyTo (copy, 1);
				Recent = copy;
			} else {
				Array.Copy (Recent, 0, Recent, 1, Recent.Length - 1);
				Recent[0] = cd;
			}
		}
	}
}
