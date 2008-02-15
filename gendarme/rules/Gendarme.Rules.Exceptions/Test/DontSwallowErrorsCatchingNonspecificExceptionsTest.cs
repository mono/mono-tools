//
// Unit Test for DontSwallowErrorsCatchingNonspecificExceptions Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using System.Reflection;
using System.IO;

using Gendarme.Framework;
using Gendarme.Rules.Exceptions;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Exceptions {
	
	[TestFixture]
	public class DontSwallowErrorsCatchingNonspecificExceptionsTest {
		
		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private MethodDefinition method;
		private TypeDefinition type;
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DontSwallowErrorsCatchingNonspecificExceptionsRule ();
			runner = new TestRunner (rule);
			type = assembly.MainModule.Types ["Test.Rules.Exceptions.DontSwallowErrorsCatchingNonspecificExceptionsTest"];
		}

		[Test]
		public void SwallowErrorsCatchingExceptionsEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingExceptionEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingExceptionsNoEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingExceptionNoEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingSystemExceptionEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingSystemExceptionEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingTypeExceptionEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingTypeExceptionEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingAllEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingAllEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void SwallowErrorsCatchingAllNoEmptyCatchBlockTest () 
		{
			method = type.Methods.GetMethod ("SwallowErrorsCatchingAllNoEmptyCatchBlock", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NotSwallowRethrowingExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowRethrowingException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		
		[Test]
		public void NotSwallowRethrowingGeneralExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowRethrowingGeneralException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		
		[Test]
		public void NotSwallowCatchingSpecificExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowCatchingSpecificException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NotSwallowThrowingANewExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowThrowingANewException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NotSwallowCatchingAllThrowingANewExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowCatchingAllThrowingANewException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NotSwallowCatchingTypeExceptionThrowingANewExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowCatchingTypeExceptionThrowingANewException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NotSwallowCatchingSystemExceptionThrowingANewExceptionTest () 
		{
			method = type.Methods.GetMethod ("NotSwallowCatchingSystemExceptionThrowingANewException", Type.EmptyTypes);
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		//Methods for make the tests
		public void SwallowErrorsCatchingExceptionEmptyCatchBlock () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
			}
		}
		
		public void SwallowErrorsCatchingExceptionNoEmptyCatchBlock () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
		
		public void SwallowErrorsCatchingSystemExceptionEmptyCatchBlock () 
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (SystemException exception) {
			}
		}
		
		public void SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlock () 
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (SystemException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
		
		public void SwallowErrorsCatchingTypeExceptionEmptyCatchBlock () 
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
			}
		}

		public void SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlock () 
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
				Console.WriteLine ("Has happened an exception.");
			}
		}
		
		public void SwallowErrorsCatchingAllEmptyCatchBlock () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
			}
		}
		
		public void SwallowErrorsCatchingAllNoEmptyCatchBlock () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
				Console.WriteLine ("Has happened an exception.");
			}
		}
		
		public void NotSwallowRethrowingGeneralException () {
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
				throw;
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
		
		public void NotSwallowRethrowingException () {
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
				throw exception;
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}
		
		public void NotSwallowCatchingSpecificException () 
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (FileNotFoundException exception) {
			}
		}
		
		public void NotSwallowThrowingANewException () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				throw new SystemException ("Message");
			}
		}
		
		public void NotSwallowCatchingAllThrowingANewException ()
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
				throw new Exception ("Message");
			}
		}
		
		public void NotSwallowCatchingTypeExceptionThrowingANewException ()
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
				throw new Exception ("Message");
			}
		}
		
		public void NotSwallowCatchingSystemExceptionThrowingANewException () 
		{
			try { 
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (System.Exception exception) {
				throw new Exception ("Message");
			}
		}
	}
}
