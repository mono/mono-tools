//
// Unit Test for AvoidLongMethods Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//
//      (C) 2007 Néstor Salceda
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Smells;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

//Stubs for the Gtk testing.
namespace Gtk {
	public class Bin {
	}

	public class Dialog {
	}

	public class Window {
	}
}

namespace Test.Rules.Smells {
	public class LongStaticConstructorWithFields {
		static readonly int foo;
		static string bar;
		static object baz;

		static LongStaticConstructorWithFields () {
			foo = 5;
			bar = "MyString";
			baz = new object ();
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	public class LongStaticConstructorWithoutFields {
		static LongStaticConstructorWithoutFields () {
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}
	
	public class LongConstructorWithReadonlyFields {
		readonly int foo;
		readonly string bar, bar1, bar2, bar3;
		readonly object baz, baz1, baz2, baz3;

		public LongConstructorWithReadonlyFields () {
			foo = 5;
			bar = "MyString";
			baz = new object ();
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	public class LongConstructorWithFields {
		readonly int foo;
		string bar, bar1, bar2, bar3;
		object baz, baz1, baz2, baz3;

		public LongConstructorWithFields () {
			foo = 5;
			bar = "MyString";
			baz = new object ();
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	public class LongConstructorWithoutFields {
		public LongConstructorWithoutFields () {
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}


	public class MainWidget : Gtk.Bin {
		protected virtual void Build () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void InitializeComponent () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	public class MainDialog : Gtk.Dialog {
		protected virtual void Build () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void InitializeComponent () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	public class MainWindow : Gtk.Window {

		protected virtual void Build () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void InitializeComponent () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

	}

	public class MainForm : Form {
		public void InitializeComponent () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
		
		protected virtual void Build () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
	}

	[TestFixture]
	public class AvoidLongMethodsTest : MethodRuleTestFixture<AvoidLongMethodsRule> {

		public void LongMethod () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void ShortMethod ()
		{
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void Build () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		public void InitializeComponent () 
		{
			Console.WriteLine ("I'm writting a test, and I will fill a screen with some useless code");
			IList list = new ArrayList ();
			list.Add ("Foo");
			list.Add (4);
			list.Add (6);

			IEnumerator listEnumerator = list.GetEnumerator ();
			while (listEnumerator.MoveNext ())
				Console.WriteLine (listEnumerator.Current);

			try {
				list.Add ("Bar");
				list.Add ('a');
			}
			catch (NotSupportedException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			foreach (object value in list) {
				Console.Write (value);
				Console.Write (Environment.NewLine);
			}
			
			int x = 0;

			for (int i = 0; i < 100; i++)
				x++;
			Console.WriteLine (x);
	
			string useless = "Useless String";

			if (useless.Equals ("Other useless")) {
				useless = String.Empty;
				Console.WriteLine ("Other useless string");
			}
			
			useless = String.Concat (useless," 1");
			
			for (int j = 0; j < useless.Length; j++) {
				if (useless[j] == 'u')
					Console.WriteLine ("I have detected an u char");
				else
					Console.WriteLine ("I have detected an useless char");
			}
			
			try {
				foreach (string environmentVariable in Environment.GetEnvironmentVariables ().Keys)
					Console.WriteLine (environmentVariable);
			}
			catch (System.Security.SecurityException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}

			Console.WriteLine ("I will add more useless code !!");
			
			try {
				if (!(File.Exists ("foo.txt"))) {
					File.Create ("foo.txt");	
					File.Delete ("foo.txt");
				}
			}
			catch (IOException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		[Test]
		public void LongMethodTest () 
		{
			AssertRuleFailure<AvoidLongMethodsTest> ("LongMethod", 1);
		}

		[Test]
		public void EmptyMethodTest () 
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void ShortMethodTest () 
		{
			AssertRuleSuccess <AvoidLongMethodsTest> ("ShortMethod");
		}

		[Test]
		public void FalseBuildMethodTest ()
		{
			AssertRuleFailure<AvoidLongMethodsTest> ("Build", 1);
		}

		[Test]
		public void WidgetBuildMethodTest () 
		{
			AssertRuleDoesNotApply<MainWidget> ("Build");
		}

		[Test]
		public void WidgetInitializeComponentMethodTest () 
		{
			AssertRuleFailure<MainWidget> ("InitializeComponent", 1);
		}

		[Test]
		public void DialogBuildMethodTest () 
		{
			AssertRuleDoesNotApply<MainDialog> ("Build");
		}

		[Test]
		public void DialogInitializeComponentMethodTest () 
		{
			AssertRuleFailure<MainDialog> ("InitializeComponent", 1);
		}

		[Test]
		public void WindowBuildMethodTest () 
		{
			AssertRuleDoesNotApply<MainWindow> ("Build");
		}

		[Test]
		public void WindowInitializeComponentMethodTest () 
		{
			AssertRuleFailure<MainWindow> ("InitializeComponent", 1);
		}

		[Test]
		public void FalseInitializeComponentTest () 
		{
			AssertRuleFailure<AvoidLongMethodsTest> ("InitializeComponent", 1);
		}

		[Test]
		public void FormInitializeComponentTest () 
		{
			AssertRuleDoesNotApply<MainForm> ("InitializeComponent");
		}

		[Test]
		public void FormBuildMethodTest () 
		{
			AssertRuleFailure<MainForm> ("Build", 1);
		}

		[Test]
		public void LongStaticConstructorWithoutFieldsTest () 
		{
			AssertRuleFailure<LongStaticConstructorWithoutFields> (".cctor", 1);
		}
		
		[Test]
		public void LongStaticConstructorWithFieldsTest () 
		{
			AssertRuleSuccess<LongStaticConstructorWithFields> (".cctor");
		}

		[Test]
		public void LongConstructorWithoutFieldsTest ()
		{
			AssertRuleFailure<LongConstructorWithoutFields> (".ctor", 1);
		}

		[Test]
		public void LongConstructorWithFieldsTest ()
		{
			AssertRuleSuccess<LongConstructorWithFields> (".ctor");
		}

		[Test]
		public void LongConstructorWithReadonlyFieldsTest ()
		{
			AssertRuleSuccess<LongConstructorWithReadonlyFields> (".ctor");
		}
	}

	[TestFixture]
	public class AvoidLongMethods_IlTest : AvoidLongMethodsTest {

		public AvoidLongMethods_IlTest ()
		{
			Rule.UseIlApproximation = true;
		}
	}

	[TestFixture]
	public class AvoidLongMethods_SlocTest : AvoidLongMethodsTest {

		public AvoidLongMethods_SlocTest ()
		{
			Rule.UseIlApproximation = false;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			AssemblyDefinition assembly = DefinitionLoader.GetAssemblyDefinition<AvoidLongMethodsTest> ();
			if (!assembly.MainModule.HasSymbols)
				Assert.Ignore ("Debugging symbols non-available to compute SLOC.");
		}
	}
}
