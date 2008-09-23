// Config.cs created with MonoDevelop
// User: lupus at 12:03 PM 9/20/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Xml;
using System.Collections.Generic;

namespace mperfmon
{

	public class CounterSet {
		string name;
		List<string> counters;
		bool is_system;

		// add an id to each counter: counters with the same id are put
		// in the same graph
		public CounterSet (string name, List<string> counters, bool is_system)
		{
			this.name = name;
			this.is_system = is_system;
			this.counters = new List<string> (counters);
		}

		public bool IsSystem {
			get {return is_system;}
		}

		public string Name {
			get {return name;}
			set {name = value;}
		}

		public List<string> Counters {
			get {return counters;}
		}
	}
	
	public class Config
	{
		string filename;
		uint timeout = 1000;
		public List<CounterSet> sets = new List<CounterSet> ();

		public Config(string filename, bool system)
		{
			Load (filename, system);
		}

		public void Load (string file, bool system)
		{
			XmlTextReader reader = new XmlTextReader (file);
			string set_name = null;
			List<string> counters = new List<string> ();

			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element) {
					string name = reader.Name;
					if (name == "mperfmon")
						continue;
					if (name == "update") {
						for (int i = 0; i < reader.AttributeCount; ++i) {
							reader.MoveToAttribute (i);
							if (reader.Name == "interval") {
								timeout = uint.Parse (reader.Value);
							}
						}
						continue;
					}
					if (name == "set") {
						for (int i = 0; i < reader.AttributeCount; ++i) {
							reader.MoveToAttribute (i);
							if (reader.Name == "name") {
								set_name = reader.Value;
							}
						}
						continue;
					}
					if (name == "counter") {
						string cat = null, counter = null;
						for (int i = 0; i < reader.AttributeCount; ++i) {
							reader.MoveToAttribute (i);
							if (reader.Name == "cat") {
								cat = reader.Value;
							} else if (reader.Name == "name") {
								counter = reader.Value;
							}
						}
						if (cat != null && counter != null) {
							counters.Add (cat);
							counters.Add (counter);
						}
						continue;
					}
				} else if (reader.NodeType == XmlNodeType.EndElement) {
					if (reader.Name == "set") {
						sets.Add (new CounterSet (set_name, counters, system));
						set_name = null;
						counters.Clear ();
					}
				}
			}
		}

		public uint Timeout {
			get {return timeout;}
			set {timeout =  value;}
		}

		public CounterSet this [string name] {
			get {
				foreach (CounterSet c in sets) {
					if (c.Name == name)
						return c;
				}
				return null;
			}
		}
	}
}
