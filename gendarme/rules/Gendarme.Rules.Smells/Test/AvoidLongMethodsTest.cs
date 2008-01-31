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

using Mono.Cecil;
using NUnit.Framework;
using Gendarme.Framework;
using Gendarme.Rules.Smells;

using System.Windows.Forms;

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
	public class AvoidLongMethodsTest {
		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private MethodDefinition method;
		private MessageCollection messageCollection;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidLongMethodsRule ();
			messageCollection = null;
		}

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

		public void EmptyMethod () 
		{
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

		private MethodDefinition GetMethodForTest (string methodName) 
		{
			return GetMethodForTestFrom ("Test.Rules.Smells.AvoidLongMethodsTest", methodName);
		}

		private MethodDefinition GetMethodForTestFrom (string fullTypeName, string methodName) 
		{
			TypeDefinition type =  assembly.MainModule.Types[fullTypeName];
			if (type != null) {
				foreach (MethodDefinition method in type.Methods) {
				if (method.Name == methodName)
					return method;
				}
			}
			return null;
		}

		[Test]
		public void LongMethodTest () 
		{
			method = GetMethodForTest ("LongMethod");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void EmptyMethodTest () 
		{
			method = GetMethodForTest ("EmptyMethod");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void ShortMethodTest () 
		{
			method = GetMethodForTest ("ShortMethod");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void FalseBuildMethodTest ()
		{
			method = GetMethodForTest ("Build");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void WidgetBuildMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainWidget", "Build");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void WidgetInitializeComponentMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainWidget", "InitializeComponent");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}


		[Test]
		public void DialogBuildMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainDialog", "Build");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void DialogInitializeComponentMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainDialog", "InitializeComponent");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void WindowBuildMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainWindow", "Build");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void WindowInitializeComponentMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainWindow", "InitializeComponent");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
	
		[Test]
		public void FalseInitializeComponentTest () 
		{
			method = GetMethodForTest ("InitializeComponent");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void FormInitializeComponentTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainForm", "InitializeComponent");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void FormBuildMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Smells.MainForm", "Build");
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
	}
}
