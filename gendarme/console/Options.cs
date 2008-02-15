//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
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

// Compile With:
//   gmcs -debug+ -d:TEST -r:System.Core Options.cs
//   gmcs -debug+ -d:LINQ -d:TEST -r:System.Core Options.cs

//
// A Getopt::Long-inspired option parsing library for C#.
//
// NDesk.Options.OptionSet is built upon a key/value table, where the
// key is a option format string and the value is an Action<string>
// delegate that is invoked when the format string is matched.
//
// Option format strings:
//  BNF Grammar: ( name [=:]? ) ( '|' name [=:]? )+
// 
// Each '|'-delimited name is an alias for the associated action.  If the
// format string ends in a '=', it has a required value.  If the format
// string ends in a ':', it has an optional value.  If neither '=' or ':'
// is present, no value is supported.
//
// Options are extracted either from the current option by looking for
// the option name followed by an '=' or ':', or is taken from the
// following option IFF:
//  - The current option does not contain a '=' or a ':'
//  - The following option is not a registered named option
//
// The `name' used in the option format string does NOT include any leading
// option indicator, such as '-', '--', or '/'.  All three of these are
// permitted/required on any named option.
//
// Option bundling is permitted so long as:
//   - '-' is used to start the option group
//   - all of the bundled options do not require values
//   - all of the bundled options are a single character
//
// This allows specifying '-a -b -c' as '-abc'.
//
// Option processing is disabled by specifying "--".  All options after "--"
// are returned by OptionSet.Parse() unchanged and unprocessed.
//
// Unprocessed options are returned from OptionSet.Parse().
//
// Examples:
//  int verbose = 0;
//  OptionSet p = new OptionSet ()
//    .Add ("v", v => ++verbose)
//    .Add ("name=|value=", v => Console.WriteLine (v));
//  p.Parse (new string[]{"-v", "--v", "/v", "-name=A", "/name", "B", "extra"});
//
// The above would parse the argument string array, and would invoke the
// lambda expression three times, setting `verbose' to 3 when complete.  
// It would also print out "A" and "B" to standard output.
// The returned array would contain the string "extra".
//
// C# 3.0 collection initializers are supported:
//  var p = new OptionSet () {
//    { "h|?|help", v => ShowHelp () },
//  };
//
// System.ComponentModel.TypeConverter is also supported, allowing the use of
// custom data types in the callback type; TypeConverter.ConvertFromString()
// is used to convert the value option to an instance of the specified
// type:
//
//  var p = new OptionSet () {
//    { "foo=", (Foo f) => Console.WriteLine (f.ToString ()) },
//  };
//
// Random other tidbits:
//  - Boolean options (those w/o '=' or ':' in the option format string)
//    are explicitly enabled if they are followed with '+', and explicitly
//    disabled if they are followed with '-':
//      string a = null;
//      var p = new OptionSet () {
//        { "a", s => a = s },
//      };
//      p.Parse (new string[]{"-a"});   // sets v != null
//      p.Parse (new string[]{"-a+"});  // sets v != null
//      p.Parse (new string[]{"-a-"});  // sets v == null
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

#if LINQ
using System.Linq;
#endif

#if TEST
using NDesk.Options;
#endif

#if !LINQ
namespace System {
	public delegate void Action<T1, T2> (T1 a, T2 b);
}
#endif

namespace NDesk.Options {

	public enum OptionValueType {
		None,
		Optional,
		Required,
	}

	public class OptionContext {
		public OptionContext ()
		{
		}

		public Option Option { get; set; }
		public string OptionName { get; set; }
		public int OptionIndex { get; set; }
		public string OptionValue { get; set; }
	}

	public abstract class Option {
		string prototype, description;
		string [] names;
		OptionValueType type;

		public Option (string prototype, string description)
		{
			if (prototype == null)
				throw new ArgumentNullException ("prototype");
			if (prototype.Length == 0)
				throw new ArgumentException ("Cannot be the empty string.", "prototype");

			this.prototype = prototype;
			this.names = prototype.Split ('|');
			this.description = description;
			this.type = ValidateNames ();
		}

		public string Prototype { get { return prototype; } }
		public string Description { get { return description; } }
		public OptionValueType OptionValueType { get { return type; } }

		public string [] GetNames ()
		{
			return (string []) names.Clone ();
		}

		internal string [] Names { get { return names; } }

		static readonly char [] NameTerminator = new char [] { '=', ':' };
		private OptionValueType ValidateNames ()
		{
			char type = '\0';
			for (int i = 0; i < names.Length; ++i) {
				string name = names [i];
				if (name.Length == 0)
					throw new ArgumentException ("Empty option names are not supported.", "prototype");

				int end = name.IndexOfAny (NameTerminator);
				if (end > 0) {
					names [i] = name.Substring (0, end);
					if (type == '\0' || type == name [end])
						type = name [end];
					else
						throw new ArgumentException (
								string.Format ("Conflicting option types: '{0}' vs. '{1}'.", type, name [end]),
								"prototype");
				}
			}
			if (type == '\0')
				return OptionValueType.None;
			return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
		}

		public void Invoke (OptionContext c)
		{
			OnParseComplete (c);
			c.OptionName = null;
			c.OptionValue = null;
			c.Option = null;
		}

		protected abstract void OnParseComplete (OptionContext c);

		public override string ToString ()
		{
			return Prototype;
		}
	}

	[Serializable]
	public class OptionException : Exception {
		private string option;

		public OptionException (string message, string optionName)
			: base (message)
		{
			this.option = optionName;
		}

		public OptionException (string message, string optionName, Exception innerException)
			: base (message, innerException)
		{
			this.option = optionName;
		}

		protected OptionException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.option = info.GetString ("OptionName");
		}

		public string OptionName
		{
			get { return this.option; }
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("OptionName", option);
		}
	}

	public class OptionSet : Collection<Option> {
		public OptionSet ()
			: this (f => f)
		{
		}

		public OptionSet (Converter<string, string> localizer)
		{
			this.localizer = localizer;
		}

		Dictionary<string, Option> options = new Dictionary<string, Option> ();
		Converter<string, string> localizer;

		protected Option GetOptionForName (string option)
		{
			if (option == null)
				throw new ArgumentNullException ("option");
			Option v;
			if (options.TryGetValue (option, out v))
				return v;
			return null;
		}

		protected override void ClearItems ()
		{
			this.options.Clear ();
		}

		protected override void InsertItem (int index, Option item)
		{
			Add (item);
			base.InsertItem (index, item);
		}

		protected override void RemoveItem (int index)
		{
			Option p = Items [index];
			foreach (string name in p.Names) {
				this.options.Remove (name);
			}
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, Option item)
		{
			RemoveItem (index);
			Add (item);
			base.SetItem (index, item);
		}

		class ActionOption : Option {
			Action<string, OptionContext> action;

			public ActionOption (string prototype, string description, Action<string, OptionContext> action)
				: base (prototype, description)
			{
				if (action == null)
					throw new ArgumentNullException ("action");
				this.action = action;
			}

			protected override void OnParseComplete (OptionContext c)
			{
				action (c.OptionValue, c);
			}
		}

		public new OptionSet Add (Option option)
		{
			if (option == null)
				throw new ArgumentNullException ("option");
			List<string> added = new List<string> ();
			try {
				foreach (string name in option.Names) {
					this.options.Add (name, option);
				}
			}
			catch (Exception e) {
				foreach (string name in added)
					this.options.Remove (name);
				throw;
			}
			return this;
		}

		public OptionSet Add (string options, Action<string> action)
		{
			return Add (options, null, action);
		}

		public OptionSet Add (string options, Action<string, OptionContext> action)
		{
			return Add (options, null, action);
		}

		public OptionSet Add (string options, string description, Action<string> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			return Add (options, description, (v, c) => { action (v); });
		}

		public OptionSet Add (string options, string description, Action<string, OptionContext> action)
		{
			Option p = new ActionOption (options, description, action);
			base.Add (p);
			return this;
		}

		public OptionSet Add<T> (string options, Action<T> action)
		{
			return Add (options, null, action);
		}

		public OptionSet Add<T> (string options, Action<T, OptionContext> action)
		{
			return Add (options, null, action);
		}

		public OptionSet Add<T> (string options, string description, Action<T> action)
		{
			return Add (options, description, (T v, OptionContext c) => { action (v); });
		}

		public OptionSet Add<T> (string options, string description, Action<T, OptionContext> action)
		{
			TypeConverter conv = TypeDescriptor.GetConverter (typeof (T));
			Action<string, OptionContext> a = delegate (string s, OptionContext c) {
				T t = default (T);
				try {
					if (s != null)
						t = (T) conv.ConvertFromString (s);
				}
				catch (Exception e) {
					throw new OptionException (
							string.Format (
								localizer ("Could not convert string `{0}' to type {1} for option `{2}'."),
								s, typeof (T).Name, c.OptionName),
							c.OptionName, e);
				}
				action (t, c);
			};
			return Add (options, description, a);
		}

		protected virtual OptionContext CreateOptionContext ()
		{
			return new OptionContext ();
		}

#if LINQ
		public List<string> Parse (IEnumerable<string> options)
		{
			bool process = true;
			OptionContext c = CreateOptionContext ();
			c.OptionIndex = -1;
			var unprocessed = 
				from option in options
				where ++c.OptionIndex >= 0 && process 
					? option == "--" 
						? (process = false)
						: !Parse (option, c)
					: true
				select option;
			List<string> r = unprocessed.ToList ();
			if (c.Option != null)
				NoValue (c);
			return r;
		}
#else
		public List<string> Parse (IEnumerable<string> options)
		{
			OptionContext c = CreateOptionContext ();
			c.OptionIndex = -1;
			bool process = true;
			List<string> unprocessed = new List<string> ();
			foreach (string option in options) {
				++c.OptionIndex;
				if (option == "--") {
					process = false;
					continue;
				}
				if (!process) {
					unprocessed.Add (option);
					continue;
				}
				if (!Parse (option, c))
					unprocessed.Add (option);
			}
			if (c.Option != null)
				NoValue (c);
			return unprocessed;
		}
#endif

		private readonly Regex ValueOption = new Regex (
			@"^(?<flag>--|-|/)(?<name>[^:=]+)([:=](?<value>.*))?$");

		protected bool GetOptionParts (string option, out string flag, out string name, out string value)
		{
			Match m = ValueOption.Match (option);
			if (!m.Success) {
				flag = name = value = null;
				return false;
			}
			flag = m.Groups ["flag"].Value;
			name = m.Groups ["name"].Value;
			value = !m.Groups ["value"].Success ? null : m.Groups ["value"].Value;
			return true;
		}

		protected virtual bool Parse (string option, OptionContext c)
		{
			if (c.Option != null) {
				c.OptionValue = option;
				c.Option.Invoke (c);
				return true;
			}

			string f, n, v;
			if (!GetOptionParts (option, out f, out n, out v))
				return false;

			Option p;
			if (this.options.TryGetValue (n, out p)) {
				c.OptionName = f + n;
				c.Option = p;
				switch (p.OptionValueType) {
				case OptionValueType.None:
					c.OptionValue = n;
					c.Option.Invoke (c);
					break;
				case OptionValueType.Optional:
				case OptionValueType.Required:
					if (v != null) {
						c.OptionValue = v;
						c.Option.Invoke (c);
					}
					break;
				}
				return true;
			}
			// no match; is it a bool option?
			if (ParseBool (option, n, c))
				return true;
			// is it a bundled option?
			if (ParseBundled (f, n, c))
				return true;

			return false;
		}

		private bool ParseBool (string option, string n, OptionContext c)
		{
			Option p;
			if (n.Length >= 1 && (n [n.Length - 1] == '+' || n [n.Length - 1] == '-') &&
					this.options.TryGetValue (n.Substring (0, n.Length - 1), out p)) {
				string v = n [n.Length - 1] == '+' ? option : null;
				c.OptionName = option;
				c.OptionValue = v;
				c.Option = p;
				p.Invoke (c);
				return true;
			}
			return false;
		}

		private bool ParseBundled (string f, string n, OptionContext c)
		{
			Option p;
			if (f == "-" && this.options.TryGetValue (n [0].ToString (), out p)) {
				int i = 0;
				do {
					string opt = "-" + n [i].ToString ();
					if (p.OptionValueType != OptionValueType.None) {
						throw new OptionException (string.Format (
									localizer ("Cannot bundle option '{0}' that requires a value."), opt),
								opt);
					}
					c.OptionName = opt;
					c.OptionValue = n;
					c.Option = p;
					p.Invoke (c);
				} while (++i < n.Length && this.options.TryGetValue (n [i].ToString (), out p));
				return true;
			}
			return false;
		}

		private void NoValue (OptionContext c)
		{
			c.OptionValue = null;
			Option p = c.Option;
			if (p != null && p.OptionValueType == OptionValueType.Optional) {
				p.Invoke (c);
			} else if (p != null && p.OptionValueType == OptionValueType.Required) {
				throw new OptionException (string.Format (
							localizer ("Missing required value for option '{0}'."), c.OptionName),
						c.OptionName);
			}
		}

		private const int OptionWidth = 29;

		public void WriteOptionDescriptions (TextWriter o)
		{
			foreach (Option p in this) {
				List<string> names = new List<string> (p.Names);

				int written = 0;
				if (names [0].Length == 1) {
					Write (o, ref written, "  -");
					Write (o, ref written, names [0]);
				} else {
					Write (o, ref written, "      --");
					Write (o, ref written, names [0]);
				}

				for (int i = 1; i < names.Count; ++i) {
					Write (o, ref written, ", ");
					Write (o, ref written, names [i].Length == 1 ? "-" : "--");
					Write (o, ref written, names [i]);
				}

				if (p.OptionValueType == OptionValueType.Optional)
					Write (o, ref written, localizer ("[=VALUE]"));
				else if (p.OptionValueType == OptionValueType.Required)
					Write (o, ref written, localizer ("=VALUE"));

				if (written < OptionWidth)
					o.Write (new string (' ', OptionWidth - written));
				else {
					o.WriteLine ();
					o.Write (new string (' ', OptionWidth));
				}

				o.WriteLine (localizer (p.Description));
			}
		}

		static void Write (TextWriter o, ref int n, string s)
		{
			n += s.Length;
			o.Write (s);
		}
	}
}

#if TEST
namespace Tests.NDesk.Options {

	using System.Linq;

	class FooConverter : TypeConverter {
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
				CultureInfo culture, object value)
		{
			string v = value as string;
			if (v != null) {
				switch (v) {
					case "A": return Foo.A;
					case "B": return Foo.B;
				}
			}

			return base.ConvertFrom (context, culture, value);
		}
	}

	[TypeConverter (typeof(FooConverter))]
	class Foo {
		public static readonly Foo A = new Foo ("A");
		public static readonly Foo B = new Foo ("B");
		string s;
		Foo (string s) { this.s = s; }
		public override string ToString () {return s;}
	}

	class Test {
		public static void Main (string[] args)
		{
			var tests = new Dictionary<string, Action> () {
				{ "boolean",      () => CheckBoolean () },
				{ "bundling",     () => CheckOptionBundling () },
				{ "context",      () => CheckOptionContext () },
				{ "descriptions", () => CheckWriteOptionDescriptions () },
				{ "exceptions",   () => CheckExceptions () },
				{ "halt",         () => CheckHaltProcessing () },
				{ "localization", () => CheckLocalization () },
				{ "many",         () => CheckMany () },
				{ "optional",     () => CheckOptional () },
				{ "required",     () => CheckRequired () },
				{ "derived-type", () => CheckDerivedType () },
			};
			bool run  = true;
			bool help = false;
			var p = new OptionSet () {
				{ "t|test=", 
					"Run the specified test.  Valid tests:\n" + new string (' ', 32) +
						string.Join ("\n" + new string (' ', 32), tests.Keys.OrderBy (s => s).ToArray ()),
					v => { run = false; Console.WriteLine (v); tests [v] (); } },
				{ "h|?|help", "Show this message and exit", (v) => help = v != null },
			};
			p.Parse (args);
			if (help) {
				Console.WriteLine ("usage: Options.exe [OPTION]+\n");
				Console.WriteLine ("Options unit test program.");
				Console.WriteLine ("Valid options include:");
				p.WriteOptionDescriptions (Console.Out);
			} else if (run) {
				foreach (Action a in tests.Values)
					a ();
			}
		}

		static IEnumerable<string> _ (params string[] a)
		{
			return a;
		}

		static void CheckRequired ()
		{
			string a = null;
			int n = 0;
			OptionSet p = new OptionSet () {
				{ "a=", v => a = v },
				{ "n=", (int v) => n = v },
			};
			List<string> extra = p.Parse (_("a", "-a", "s", "-n=42", "n"));
			Assert (extra.Count, 2);
			Assert (extra [0], "a");
			Assert (extra [1], "n");
			Assert (a, "s");
			Assert (n, 42);

			extra = p.Parse (_("-a="));
			Assert (extra.Count, 0);
			Assert (a, "");
		}

		static void CheckOptional ()
		{
			string a = null;
			int n = -1;
			Foo f = null;
			OptionSet p = new OptionSet () {
				{ "a:", v => a = v },
				{ "n:", (int v) => n = v },
				{ "f:", (Foo v) => f = v },
			};
			p.Parse (_("-a=s"));
			Assert (a, "s");
			p.Parse (_("-a"));
			Assert (a, null);
			p.Parse (_("-a="));
			Assert (a, "");

			p.Parse (_("-f", "A"));
			Assert (f, Foo.A);
			p.Parse (_("-f"));
			Assert (f, null);

			p.Parse (_("-n", "42"));
			Assert (n, 42);
			p.Parse (_("-n"));
			Assert (n, 0);
		}

		static void CheckBoolean ()
		{
			bool a = false;
			OptionSet p = new OptionSet () {
				{ "a", v => a = v != null },
			};
			p.Parse (_("-a"));
			Assert (a, true);
			p.Parse (_("-a+"));
			Assert (a, true);
			p.Parse (_("-a-"));
			Assert (a, false);
		}

		static void CheckMany ()
		{
			int a = -1, b = -1;
			string av = null, bv = null;
			Foo f = null;
			int help = 0;
			int verbose = 0;
			OptionSet p = new OptionSet () {
				{ "a=", v => { a = 1; av = v; } },
				{ "b", "desc", v => {b = 2; bv = v;} },
				{ "f=", (Foo v) => f = v },
				{ "v", v => { ++verbose; } },
				{ "h|?|help", (v) => { switch (v) {
					case "h": help |= 0x1; break; 
					case "?": help |= 0x2; break;
					case "help": help |= 0x4; break;
				} } },
			};
			List<string> e = p.Parse (new string[]{"foo", "-v", "-a=42", "/b-",
				"-a", "64", "bar", "--f", "B", "/h", "-?", "--help", "-v"});

			Assert (e.Count, 2);
			Assert (e[0], "foo");
			Assert (e[1], "bar");
			Assert (a, 1);
			Assert (av, "64");
			Assert (b, 2);
			Assert (bv, null);
			Assert (verbose, 2);
			Assert (help, 0x7);
			Assert (f, Foo.B);
		}

		static void Assert<T>(T actual, T expected)
		{
			if (!object.Equals (actual, expected))
				throw new InvalidOperationException (
					string.Format ("Assertion failed: {0} != {1}", actual, expected));
		}

		class DefaultOption : Option {
			public DefaultOption (string prototypes, string description)
				: base (prototypes, description)
			{
			}

			protected override void OnParseComplete (OptionContext c)
			{
				throw new NotImplementedException ();
			}
		}

		static void CheckExceptions ()
		{
			string a = null;
			var p = new OptionSet () {
				{ "a=", v => a = v },
				{ "c",  v => { } },
				{ "n=", (int v) => { } },
				{ "f=", (Foo v) => { } },
			};
			// missing argument
			AssertException (typeof(OptionException), 
					"Missing required value for option '-a'.", 
					p, v => { v.Parse (_("-a")); });
			// another named option while expecting one -- follow Getopt::Long
			AssertException (null, null,
					p, v => { v.Parse (_("-a", "-a")); });
			Assert (a, "-a");
			// no exception when an unregistered named option follows.
			AssertException (null, null, 
					p, v => { v.Parse (_("-a", "-b")); });
			Assert (a, "-b");
			AssertException (typeof(ArgumentNullException),
					"Argument cannot be null.\nParameter name: option",
					p, v => { v.Add (null); });

			// bad type
			AssertException (typeof(OptionException),
					"Could not convert string `value' to type Int32 for option `-n'.",
					p, v => { v.Parse (_("-n", "value")); });
			AssertException (typeof(OptionException),
					"Could not convert string `invalid' to type Foo for option `--f'.",
					p, v => { v.Parse (_("--f", "invalid")); });

			// try to bundle with an option requiring a value
			AssertException (typeof(OptionException), 
					"Cannot bundle option '-a' that requires a value.", 
					p, v => { v.Parse (_("-ca", "value")); });

			AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: prototype", 
					p, v => { new DefaultOption (null, null); });
			AssertException (typeof(ArgumentException), 
					"Cannot be the empty string.\nParameter name: prototype",
					p, v => { new DefaultOption ("", null); });
			AssertException (typeof(ArgumentException),
					"Empty option names are not supported.\nParameter name: prototype",
					p, v => { new DefaultOption ("a|b||c=", null); });
			AssertException (typeof(ArgumentException),
					"Conflicting option types: '=' vs. ':'.\nParameter name: prototype",
					p, v => { new DefaultOption ("a=|b:", null); });
			AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: action",
					p, v => { v.Add ("foo", (Action<string>) null); });
			AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: action",
					p, v => { v.Add ("foo", (Action<string, OptionContext>) null); });
		}

		static void AssertException<T> (Type exception, string message, T a, Action<T> action)
		{
			Type actualType = null;
			string stack = null;
			string actualMessage = null;
			try {
				action (a);
			}
			catch (Exception e) {
				actualType    = e.GetType ();
				actualMessage = e.Message;
				if (!object.Equals (actualType, exception))
					stack = e.ToString ();
			}
			if (!object.Equals (actualType, exception)) {
				throw new InvalidOperationException (
					string.Format ("Assertion failed: Expected Exception Type {0}, got {1}.\n" +
						"Actual Exception: {2}", exception, actualType, stack));
			}
			if (!object.Equals (actualMessage, message))
				throw new InvalidOperationException (
					string.Format ("Assertion failed:\n\tExpected: {0}\n\t  Actual: {1}",
						message, actualMessage));
		}

		static void CheckWriteOptionDescriptions ()
		{
			var p = new OptionSet () {
				{ "p|indicator-style=", "append / indicator to directories", v => {} },
				{ "color:", "controls color info", v => {} },
				{ "h|?|help", "show help text", v => {} },
				{ "version", "output version information and exit", v => {} },
			};

			StringWriter expected = new StringWriter ();
			expected.WriteLine ("  -p, --indicator-style=VALUE");
			expected.WriteLine ("                             append / indicator to directories");
			expected.WriteLine ("      --color[=VALUE]        controls color info");
			expected.WriteLine ("  -h, -?, --help             show help text");
			expected.WriteLine ("      --version              output version information and exit");

			StringWriter actual = new StringWriter ();
			p.WriteOptionDescriptions (actual);

			Assert (actual.ToString (), expected.ToString ());
		}

		static void CheckOptionBundling ()
		{
			string a, b, c;
			a = b = c = null;
			var p = new OptionSet () {
				{ "a", v => a = "a" },
				{ "b", v => b = "b" },
				{ "c", v => c = "c" },
			};
			p.Parse (_ ("-abc"));
			Assert (a, "a");
			Assert (b, "b");
			Assert (c, "c");
		}

		static void CheckHaltProcessing ()
		{
			var p = new OptionSet () {
				{ "a", v => {} },
				{ "b", v => {} },
			};
			List<string> e = p.Parse (_ ("-a", "-b", "--", "-a", "-b"));
			Assert (e.Count, 2);
			Assert (e [0], "-a");
			Assert (e [1], "-b");
		}

		static void CheckLocalization ()
		{
			var p = new OptionSet (f => "hello!") {
				{ "n=", (int v) => { } },
			};
			AssertException (typeof(OptionException), "hello!",
					p, v => { v.Parse (_("-n=value")); });

			StringWriter expected = new StringWriter ();
			expected.WriteLine ("  -nhello!                   hello!");

			StringWriter actual = new StringWriter ();
			p.WriteOptionDescriptions (actual);

			Assert (actual.ToString (), expected.ToString ());
		}

		class CiOptionSet : OptionSet {
			protected override void InsertItem (int index, Option item)
			{
				if (item.Prototype.ToLower () != item.Prototype)
					throw new ArgumentException ("prototypes must be null!");
				base.InsertItem (index, item);
			}

			protected override bool Parse (string option, OptionContext c)
			{
				if (c.Option != null)
					return base.Parse (option, c);
				string f, n, v;
				if (!GetOptionParts (option, out f, out n, out v)) {
					return base.Parse (option, c);
				}
				return base.Parse (f + n.ToLower () + (v != null ? "=" + v : ""), c);
			}

			public new Option GetOptionForName (string n)
			{
				return base.GetOptionForName (n);
			}
		}

		static void CheckDerivedType ()
		{
			bool help = false;
			var p = new CiOptionSet () {
				{ "h|help", v => help = v != null },
			};
			p.Parse (_("-H"));
			Assert (help, true);
			help = false;
			p.Parse (_("-HELP"));
			Assert (help, true);

			Assert (p.GetOptionForName ("h"), p [0]);
			Assert (p.GetOptionForName ("help"), p [0]);
			Assert (p.GetOptionForName ("invalid"), null);

			AssertException (typeof(ArgumentException), "prototypes must be null!",
					p, v => { v.Add ("N|NUM=", (int n) => {}); });
			AssertException (typeof(ArgumentNullException),
					"Argument cannot be null.\nParameter name: option",
					p, v => { v.GetOptionForName (null); });
		}

		static void CheckOptionContext ()
		{
			var p = new OptionSet () {
				{ "a=", "a desc", (v,c) => {
					Assert (v, "a-val");
					Assert (c.Option.Description, "a desc");
					Assert (c.OptionName, "/a");
					Assert (c.OptionIndex, 1);
					Assert (c.OptionValue, v);
				} },
				{ "b", "b desc", (v, c) => {
					Assert (v, "--b+");
					Assert (c.Option.Description, "b desc");
					Assert (c.OptionName, "--b+");
					Assert (c.OptionIndex, 2);
					Assert (c.OptionValue, v);
				} },
				{ "c=", "c desc", (v, c) => {
					Assert (v, "C");
					Assert (c.Option.Description, "c desc");
					Assert (c.OptionName, "--c");
					Assert (c.OptionIndex, 3);
					Assert (c.OptionValue, v);
				} },
				{ "d", "d desc", (v, c) => {
					Assert (v, null);
					Assert (c.Option.Description, "d desc");
					Assert (c.OptionName, "/d-");
					Assert (c.OptionIndex, 4);
					Assert (c.OptionValue, v);
				} },
			};
			p.Parse (_("/a", "a-val", "--b+", "--c=C", "/d-"));
		}
	}
}
#endif

