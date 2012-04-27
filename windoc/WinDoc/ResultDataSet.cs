using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Monodoc;

namespace WinDoc
{
	public class ResultDataEntry
	{
		// value is element is a section, null if it's a section child
		public string SectionName { get; set; }
		public Result ResultSet { get; set; }
		public int Index { get; set; }
	}
	
	public class ResultDataSet
	{
		// Dict key is section name, value is a sorted list of section element (result it comes from and the index in it) with key being the url (to avoid duplicates)
		Dictionary<string, SortedList<string, Tuple<Result, int>>> sections = new Dictionary<string, SortedList<string, Tuple<Result, int>>> ();
		List<ResultDataEntry> data = new List<ResultDataEntry> ();
		
		public void AddResultSet (Result result)
		{
			for (int i = 0; i < result.Count; i++) {
				string fullTitle = result.GetFullTitle (i);
				string section = string.IsNullOrWhiteSpace (fullTitle) ? "Other" : fullTitle.Split (':')[0];
				SortedList<string, Tuple<Result, int>> sectionContent;
				var newItem = Tuple.Create (result, i);
				var url = result.GetUrl (i);
				
				if (!sections.TryGetValue (section, out sectionContent))
					sections[section] = new SortedList<string, Tuple<Result, int>> () { { url, newItem } };
				else
					sectionContent[url] = newItem;
			}
			// Flatten everything back to a list
			data.Clear ();
			foreach (var kvp in sections) {
				data.Add (new ResultDataEntry { SectionName = kvp.Key });
				foreach (var item in kvp.Value)
					data.Add (new ResultDataEntry { ResultSet = item.Value.Item1, Index = item.Value.Item2 });
			}
		}
		
		public void ClearResultSet ()
		{
			sections.Clear ();
		}

		public int Count {
			get {
				return data.Count;
			}
		}

		public ResultDataEntry this[int index] {
			get {
				return data[index];
			}
		}

		public bool IsSection (ResultDataEntry entry)
		{
			return !string.IsNullOrEmpty (entry.SectionName);
		}
	}
}
