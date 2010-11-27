//
// Unit tests for MarshalStringsInPInvokeDeclarationsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2008 Daniel Abramov
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Interoperability {

	[TestFixture]
	public class MarshalStringsInPInvokeDeclarationsTest {

		private MarshalStringsInPInvokeDeclarationsRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		// checks for CharSet property
		[DllImport ("kernel32")]
		static extern void Sleep (uint time); // good (no strings)

		[DllImport ("kernel32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr FindFirstFile (string lpFileName, out object lpFindFileData); // good (string)

		[DllImport ("secur32.dll", CharSet = CharSet.Ansi)]
		static extern int GetUserNameEx (int nameFormat, StringBuilder userName, ref uint userNameSize); // good (StringBuilder)

		[DllImport ("winmm.dll", SetLastError = true)]
		static extern bool PlaySound (string pszSound, UIntPtr hmod, uint fdwSound); // bad (string, no charset!)

		[DllImport ("user32.dll")]
		static extern int MessageBox (IntPtr hWnd, StringBuilder text, StringBuilder caption, object style); // bad (StringBuilder, no charset)

		// checks for MarshalAs
		// have marshalas's but no charset => ok
		[DllImport ("kernel32.dll", SetLastError = true)]
		static extern uint GetShortPathNameGood (
		  [MarshalAs (UnmanagedType.LPTStr)]
		  string lpszLongPath,
		  [MarshalAs (UnmanagedType.LPTStr)]
		  StringBuilder lpszShortPath,
		  uint cchBuffer);

		// have charset, no marshalas's => ok
		[DllImport ("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern uint GetShortPathNameBadParametersCharsetSpec (
		  string lpszLongPath,
		  StringBuilder lpszShortPath,
		  uint cchBuffer);

		// have neither => not ok
		[DllImport ("kernel32.dll", SetLastError = true)]
		static extern uint GetShortPathNameBadParametersCharsetNotSpec (
		  string lpszLongPath,
		  StringBuilder lpszShortPath,
		  uint cchBuffer);

		#region Commented out because structure handling is disabled but this code can be reused somewhere
		//// structure tests

		//[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)] // have charset => ok
		//public struct SHFILEINFOWithCharset
		//{
		//        public IntPtr hIcon;
		//        public int iIcon;
		//        public uint dwAttributes;
		//        [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 260)]
		//        public string szDisplayName;
		//        [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 80)]
		//        public string szTypeName;
		//};

		//[DllImport ("shell32.dll")]  // no charset but we have one in struct => ok
		//public static extern IntPtr SHGetFileInfoWithStructureCharset ([MarshalAs (UnmanagedType.LPStr)] string pszPath, uint dwFileAttributes, ref SHFILEINFOWithCharset psfi, uint cbFileInfo, uint uFlags);

		//[StructLayout (LayoutKind.Sequential)] // no charset but all marsalas's => ok
		//public struct SHFILEINFOAllMarshalled
		//{
		//        public IntPtr hIcon;
		//        public int iIcon;
		//        public uint dwAttributes;
		//        [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 260)]
		//        public string szDisplayName;
		//        [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 80)]
		//        public string szTypeName;
		//};

		//[DllImport ("shell32.dll")]  // no charset but all struct strings have marshalas's => ok
		//public static extern IntPtr SHGetFileInfoAllMarshalled ([MarshalAs (UnmanagedType.LPStr)] string pszPath, uint dwFileAttributes, ref SHFILEINFOAllMarshalled psfi, uint cbFileInfo, uint uFlags);

		//[StructLayout (LayoutKind.Sequential)] // no charset, two marshalas's missing => bad
		//public struct SHFILEINFOTwoNotMarshalled
		//{
		//        public IntPtr hIcon;
		//        public int iIcon;
		//        public uint dwAttributes;
		//        public string szDisplayName;
		//        public string szTypeName;
		//};

		//[DllImport ("shell32.dll")] // no charset, two struct strings don't have marshalas's => bad
		//public static extern IntPtr SHGetFileInfoTwoNotMarshalled ([MarshalAs (UnmanagedType.LPStr)] string pszPath, uint dwFileAttributes, ref SHFILEINFOTwoNotMarshalled psfi, uint cbFileInfo, uint uFlags);
		#endregion

		void EmptyMethod () { } // FIXME: replace with PerfectMethods.EmptyMethod when porting to new tests model

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Rules.Interoperability.MarshalStringsInPInvokeDeclarationsTest");
			rule = new MarshalStringsInPInvokeDeclarationsRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		[Test]
		public void TestEmptyMethod ()
		{
			MethodDefinition method = GetTest ("EmptyMethod");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void TestNoStringsMethod ()
		{
			MethodDefinition method = GetTest ("Sleep");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestStringWithCharsetMethod ()
		{
			MethodDefinition method = GetTest ("FindFirstFile");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestStringBuilderWithCharsetMethod ()
		{
			MethodDefinition method = GetTest ("GetUserNameEx");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestStringWithoutCharsetMethod ()
		{
			MethodDefinition method = GetTest ("PlaySound");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestStringBuilderWithoutCharsetMethod ()
		{
			MethodDefinition method = GetTest ("MessageBox");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestDeclarationWithMarshalAs ()
		{
			MethodDefinition method = GetTest ("GetShortPathNameGood");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestDeclarationWithoutMarshalAsCharsetSpec ()
		{
			MethodDefinition method = GetTest ("GetShortPathNameBadParametersCharsetSpec");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestDeclarationWithoutMarshalAsCharsetNotSpec ()
		{
			MethodDefinition method = GetTest ("GetShortPathNameBadParametersCharsetNotSpec");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}
	}
}
