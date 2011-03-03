//
// Gendarme Console Settings
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;

using Gendarme.Framework;

namespace Gendarme {

	public class Settings {

		private const string DefaultRulesFile = "rules.xml";

		private Collection<IRule> rules;
		private string config_file;
		private string rule_set;
		private IList<string> validation_errors = new List<string> ();

		public Settings (IRunner runner, string configurationFile, string ruleSet)
		{
			if (runner == null)
				throw new ArgumentNullException ("runner");

			rules = runner.Rules;
			rule_set = ruleSet;
			if (String.IsNullOrEmpty (configurationFile)) {
				config_file = GetFullPath (DefaultRulesFile);
			} else {
				config_file = configurationFile;
			}
		}

		static string GetFullPath (string filename)
		{
			if (Path.GetDirectoryName (filename).Length > 0)
				return filename;
			return Path.Combine (Path.GetDirectoryName (ConsoleRunner.Assembly.Location), filename);
		}

		static string GetAttribute (XmlNode node, string name, string defaultValue)
		{
			XmlAttribute xa = node.Attributes [name];
			if (xa == null)
				return defaultValue;
			return xa.Value;
		}

		private static bool IsContainedInRuleSet (string rule, string mask)
		{
			string [] ruleSet = mask.Split ('|');
			foreach (string entry in ruleSet) {
				if (String.Compare (rule, entry.Trim (), StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		private static bool RuleFilter (Type type, object interfaceName)
		{
			return (type.ToString () == (interfaceName as string));
		}

		private static void SetApplicabilityScope (IRule rule, string applicabilityScope) 
		{
			switch (applicabilityScope) {
			case "visible":
				rule.ApplicabilityScope = ApplicabilityScope.Visible;
				break;
			case "nonvisible":
				rule.ApplicabilityScope = ApplicabilityScope.NonVisible;
				break;
			case "all":
				rule.ApplicabilityScope = ApplicabilityScope.All;
				break;
			default:
				//if the scope is not empty, notify
				if (!String.IsNullOrEmpty (applicabilityScope))
					Console.Error.WriteLine ("Unknown scope value '{0}' . Defaulting to 'all'", applicabilityScope);
				rule.ApplicabilityScope = ApplicabilityScope.All;
				break;
			}
		}

		private int LoadRulesFromAssembly (string assembly, string includeMask, string excludeMask, string applicabilityScope)
		{
			Assembly a = null;
			try {
				AssemblyName aname = AssemblyName.GetAssemblyName (Path.GetFullPath (assembly));
				a = Assembly.Load (aname);
			}
			catch (FileNotFoundException) {
				Console.Error.WriteLine ("Could not load rules from assembly '{0}'.", assembly);
				return 0;
			}

			int total = 0;
			foreach (Type t in a.GetTypes ()) {
				if (t.IsAbstract || t.IsInterface)
					continue;

				if (includeMask != "*")
					if (!IsContainedInRuleSet (t.Name, includeMask))
						continue;

				if ((excludeMask != null) && (excludeMask.Length > 0))
					if (IsContainedInRuleSet (t.Name, excludeMask))
						continue;

				if (t.FindInterfaces (new TypeFilter (RuleFilter), "Gendarme.Framework.IRule").Length > 0) {
					IRule rule = (IRule) Activator.CreateInstance (t);
					rules.Add (rule);
					SetApplicabilityScope (rule, applicabilityScope);

					total++;
				}
			}
			return total;
		}

		private void OnValidationErrors (object sender, ValidationEventArgs args)
		{
			validation_errors.Add (args.Exception.Message.Replace ("XmlSchema error", 
				String.Format (CultureInfo.CurrentCulture, "Error in the configuration file {0}", config_file)));
		}

		private void ValidateXmlDocument ()
		{
			using (Stream stream = Helpers.GetStreamFromResource ("gendarme.xsd")) {
				if (stream == null)
					throw new InvalidDataException ("Could not locate Xml Schema Definition inside resources.");
				XmlReaderSettings settings = new XmlReaderSettings ();
				settings.Schemas.Add (XmlSchema.Read (stream, OnValidationErrors));
				settings.ValidationType = ValidationType.Schema;
				settings.ValidationEventHandler += OnValidationErrors;
				using (XmlReader reader = XmlReader.Create (config_file, settings)) {
					while (reader.Read ()){}
				}
			}
		}
	
		public IEnumerable<string> ValidationErrors {
			get {
				return validation_errors;
			}
		}

		private IRule GetRule (string name)
		{
			foreach (IRule rule in rules) {
				if (rule.GetType ().ToString ().Contains (name)) 
					return rule;
			}
			return null;
		}
		
		private void SetCustomParameters (XmlNode nodes)
		{
			foreach (XmlElement parameter in nodes.SelectNodes ("parameter")) {
				string ruleName = GetAttribute (parameter, "rule", String.Empty);
				string propertyName = GetAttribute (parameter, "property", String.Empty);
				
				IRule rule = GetRule (ruleName);
				if (rule == null)
					throw GetException ("The rule with name {0} doesn't exist", ruleName, String.Empty, String.Empty);
				PropertyInfo property = rule.GetType ().GetProperty (propertyName);
				if (property == null)
					throw GetException ("The property {1} can't be found in the rule {0}", ruleName, propertyName, String.Empty);
				if (!property.CanWrite)
					throw GetException ("The property {1} can't be written in the rule {0}", ruleName, propertyName, String.Empty);

				string value = GetAttribute (parameter, "value", String.Empty);
				if (String.IsNullOrEmpty (value))
					continue;

				object [] values = new object [1];
				switch (Type.GetTypeCode (property.PropertyType)) {
				case TypeCode.Int32:
					int i;
					if (Int32.TryParse (value, out i))
						values [0] = i;
					break;
				case TypeCode.Double:
					double d;
					if (Double.TryParse (value, out d))
						values [0] = d;
					break;
				case TypeCode.String:
					values [0] = value;
					break;
				}

				if (values [0] == null)
					throw GetException ("The value '{2}' could not be converted into the property {1} type for rule {0}", ruleName, propertyName, value);

				property.GetSetMethod ().Invoke (rule, values);
			}
		}

		static Exception GetException (string message, string ruleName, string propertyName, string value)
		{
			return new XmlException (String.Format (CultureInfo.CurrentCulture, 
				message + ".  Review your configuration file.", ruleName, propertyName, value));
		}

		public bool Load ()
		{
			ValidateXmlDocument ();
			if (validation_errors.Count != 0)
				return false;

			XmlDocument doc = new XmlDocument ();
			doc.Load (config_file);
			
			bool result = false;
			foreach (XmlElement ruleset in doc.DocumentElement.SelectNodes ("ruleset")) {
				if (ruleset.Attributes ["name"].Value != rule_set)
					continue;
				foreach (XmlElement assembly in ruleset.SelectNodes ("rules")) {
					string include = GetAttribute (assembly, "include", "*");
					string exclude = GetAttribute (assembly, "exclude", String.Empty);
					string from = GetFullPath (GetAttribute (assembly, "from", String.Empty));
					string applicabilityScope = GetAttribute (assembly, "applyTo", String.Empty);

					int n = LoadRulesFromAssembly (from, include, exclude, applicabilityScope);
					result = (result || (n > 0));
					if (result) 
						SetCustomParameters (assembly);
				}
			}
			return result;
		}
	}
}
