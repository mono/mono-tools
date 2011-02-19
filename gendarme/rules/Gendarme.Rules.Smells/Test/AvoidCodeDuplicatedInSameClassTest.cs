//
// Unit Test for AvoidCodeDuplicatedInSameClass Rule.
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 - 2008 Néstor Salceda
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
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Xml;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Smells;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Smells {

	[TestFixture]
	public class AvoidCodeDuplicatedInSameClassTest : TypeRuleTestFixture<AvoidCodeDuplicatedInSameClassRule> { 
		static IList myList;

		class DuplicatedCodeWithForeachLoops {

			public void PrintAndAddANewValue () 
			{
				foreach (string value in myList) 
					Console.WriteLine (value);
				myList.Add ("FooReplied");
			}

			public void PrintAndRemoveANewValue () 
			{
				foreach (string value in myList) 
					Console.WriteLine (value);              
				myList.Remove ("FooReplied");
			} 
		}

		[Test]
		public void FailOnDuplicatedCodeWithForeachLoopsTest () 
		{
			AssertRuleFailure<DuplicatedCodeWithForeachLoops> (1);
		}

		class DuplicatedCodeInDifferentPlaces {

			public void ShowBannerAndAdd () 
			{
				Console.WriteLine ("Banner");
				Console.WriteLine ("Print");
				myList.Add ("MoreBar");
			}
		
			public void AddAndShowBanner () 
			{
				myList.Add ("MoreFoo");
				Console.WriteLine ("Banner");
				Console.WriteLine ("Print");
			}
		}

		[Test]
		public void FailOnDuplicatedCodeInDifferentPlacesTest () 
		{
			AssertRuleFailure<DuplicatedCodeInDifferentPlaces> (1);
		}

		class DuplicatedCodeInForLoop {
		
			public void PrintUsingAForLoopAndAddAValue () 
			{
				for (int index = 0; index < myList.Count; index++) 
					Console.WriteLine (myList[index]);	
				myList.Add ("MoreFoo");
			}
		
			public void PrintUsingAForLoopAndRemoveAValue () 
			{
				for (int index = 0; index < myList.Count; index++) 
					Console.WriteLine (myList[index]);	
				myList.Remove ("MoreFoo");
			}
		}

		[Test]
		public void FailOnDuplicatedCodeInForLoopTest ()
		{
			AssertRuleFailure<DuplicatedCodeInForLoop> (1);
		}

		class DuplicatedCodeInConditional {
			
			public void IfConditionAndRemove () {
				if (myList.Contains ("MoreFoo") & myList.Contains ("MoreBar"))
					myList.Remove ("MoreFoo");
			}
		
			public void IfConditionAndRemoveReplied () {
				if (myList.Contains ("MoreFoo") & myList.Contains ("MoreFoo"))
					myList.Remove ("MoreFoo");
			}
		}

		[Test]
		public void FailOnDuplicatedCodeInConditionalTest ()
		{
			AssertRuleFailure<DuplicatedCodeInConditional> (1);
		}

		class NonDuplicatedWithOneInstructionOnly {
			public void WriteLine () 
			{
				Console.WriteLine ("Foo");
			}

			public void WriteTheSame () 
			{
				Console.WriteLine ("Foo");
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedWithOneInstructionOnlyTest ()
		{
			AssertRuleSuccess<NonDuplicatedWithOneInstructionOnly> ();
		}

		class LazyLoad {
			IList otherList;
	
			public IList MyList {
				get {
					if (myList == null)
						myList = new ArrayList ();
					return myList;
				}
			}
			
			public IList OtherList {
				get {
					if (otherList == null)
						otherList = new ArrayList ();
					return otherList;
				}
			}
		}

		
		[Test]
		public void SuccesOnLazyLoadTest ()
		{
			AssertRuleSuccess<LazyLoad> ();
		}

		class NonDuplicatedForeachLoop {
			void PrintValuesInList () 
			{
				foreach (string value in myList) 
					Console.WriteLine (value);
			}

			public void PrintAndAddANewValue () 
			{
				PrintValuesInList ();
				myList.Add ("FooReplied");
			}

			public void PrintAndRemoveANewValue () 
			{
				PrintValuesInList ();
				myList.Remove ("FooReplied");
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedForeachLoopTest ()
		{
			AssertRuleSuccess<NonDuplicatedForeachLoop> ();
		}

		class NonDuplicatedForLoop {
			
			private void PrintValuesInListUsingAForLoop () 
			{
				for (int index = 0; index < myList.Count; index++) 
					Console.WriteLine (myList[index]);	
			}

			public void PrintUsingAForLoopAndRemoveAValueIfNotExists () 
			{
				PrintValuesInListUsingAForLoop ();
				if (!myList.Contains ("Bar"))
					myList.Remove ("Bar");
			}
		
			public void PrintUsingAForLoopAndRemoveAValueIfExists () 
			{
				PrintValuesInListUsingAForLoop ();
				if (myList.Contains ("Bar"))
					myList.Remove ("Bar");
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedForLoopTest () 
		{
			AssertRuleSuccess<NonDuplicatedForLoop> ();
		}

		//Smaller tests

		class NonDuplicatedCodeIntoForeachLoop {
			public void PrintValues ()
			{
				foreach (string value in myList) 
					Console.WriteLine (value);
			}

			public void PrintValuesInSameLine ()
			{
				foreach (string value in myList) 
					Console.Write (value);
			}
		}


		[Test]
		public void SuccessOnNonDuplicatedCodeIntoForeachLoopTest ()
		{
			AssertRuleSuccess<NonDuplicatedCodeIntoForeachLoop> ();
		}

		class NonDuplicatedCodeWithUsingBlock {
			public void UsingAndReadStream ()
			{
				using (Stream stream = new FileStream ("sample.txt", FileMode.Open))
					stream.ReadByte ();
			}

			public void UsingAndWriteStream ()
			{
				using (Stream stream = new FileStream ("sample.txt", FileMode.Create)) 
					stream.WriteByte (1);
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeWithUsingBlockTest ()
		{
			AssertRuleSuccess<NonDuplicatedCodeWithUsingBlock> ();
		}

		class NonDuplicatedInSwitchs {
			Severity severity = Severity.Low;
			Confidence confidence = Confidence.Normal;

			//Althoug both are referring to option as argument, it
			//isn't the same switch.
			public void FirstNonDuplicatedSwitch (string option)
			{
				switch (option) {
				case "AUDIT":
				case "AUDIT+":
				case "AUDIT-":
					severity = Severity.Audit;
					break;
				case "LOW":
				case "LOW+":
				case "LOW-":
					severity = Severity.Low;
					break;
				case "MEDIUM":
				case "MEDIUM+":
				case "MEDIUM-":
					severity = Severity.Medium;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					severity = Severity.High;
					break;
				case "CRITICAL":
				case "CRITICAL+":
				case "CRITICAL-":
					severity = Severity.Critical;
					break;
				default:
					break;
				}
			}

			public void SecondNonDuplicatedSwitch (string option)
			{
				switch (option) {
				case "LOW":
				case "LOW+":
				case "LOW-":
					confidence = Confidence.Low;
					break;
				case "NORMAL":
				case "NORMAL+":
				case "NORMAL-":
					confidence = Confidence.Normal;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					confidence = Confidence.High;
					break;
				case "TOTAL":
				case "TOTAL+":
				case "TOTAL-":
					confidence = Confidence.Total;
					break;
				default:
					break;
				}
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedInSwitchsTest () 
		{
			AssertRuleSuccess<NonDuplicatedInSwitchs> ();
		}

		class DuplicatedInSwitchsLoadingByFields {
			string option = "LOW";
			Severity severity = Severity.Low;

			public void FirstDuplicatedSwitch () 
			{
				switch (option) {
				case "AUDIT":
				case "AUDIT+":
				case "AUDIT-":
					severity = Severity.Audit;
					break;
				case "LOW":
				case "LOW+":
				case "LOW-":
					severity = Severity.Low;
					break;
				case "MEDIUM":
				case "MEDIUM+":
				case "MEDIUM-":
					severity = Severity.Medium;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					severity = Severity.High;
					break;
				case "CRITICAL":
				case "CRITICAL+":
				case "CRITICAL-":
					severity = Severity.Critical;
					break;
				default:
					break;
				}

			}

			public void SecondDuplicatedSwitch ()
			{
				switch (option) {
				case "AUDIT":
				case "AUDIT+":
				case "AUDIT-":
					severity = Severity.Audit;
					break;
				case "LOW":
				case "LOW+":
				case "LOW-":
					severity = Severity.Low;
					break;
				case "MEDIUM":
				case "MEDIUM+":
				case "MEDIUM-":
					severity = Severity.Medium;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					severity = Severity.High;
					break;
				case "CRITICAL":
				case "CRITICAL+":
				case "CRITICAL-":
					severity = Severity.Critical;
					break;
				default:
					break;
				}
			}
		}

		[Test]
		public void FailOnDuplicatedInSwitchsLoadingByFieldsTest ()
		{
			AssertRuleFailure<DuplicatedInSwitchsLoadingByFields> (1);
		}
		
		class NonDuplicatedInSwitchsLoadingByFields {
			string option = "LOW";
			Severity severity = Severity.Low;
			Confidence confidence = Confidence.Normal;

			//Althoug both are referring to option as argument, it
			//isn't the same switch.
			public void FirstNonDuplicatedSwitch ()
			{
				switch (option) {
				case "AUDIT":
				case "AUDIT+":
				case "AUDIT-":
					severity = Severity.Audit;
					break;
				case "LOW":
				case "LOW+":
				case "LOW-":
					severity = Severity.Low;
					break;
				case "MEDIUM":
				case "MEDIUM+":
				case "MEDIUM-":
					severity = Severity.Medium;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					severity = Severity.High;
					break;
				case "CRITICAL":
				case "CRITICAL+":
				case "CRITICAL-":
					severity = Severity.Critical;
					break;
				default:
					break;
				}
			}

			public void SecondNonDuplicatedSwitch ()
			{
				switch (option) {
				case "LOW":
				case "LOW+":
				case "LOW-":
					confidence = Confidence.Low;
					break;
				case "NORMAL":
				case "NORMAL+":
				case "NORMAL-":
					confidence = Confidence.Normal;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					confidence = Confidence.High;
					break;
				case "TOTAL":
				case "TOTAL+":
				case "TOTAL-":
					confidence = Confidence.Total;
					break;
				default:
					break;
				}
			}
		}
		
		[Test]
		public void SuccesOnNonDuplicatedInSwitchsLoadingByFieldsTest () 
		{
			AssertRuleSuccess<NonDuplicatedInSwitchsLoadingByFields> ();	
		}

		class NonDuplicatedCodeInParameterChecking {
			void MethodWithParameters (string x, string y) 
			{
				if ((x == null) || (y == null))
					return;
				
				char z = 'a';
			}

			void MethodWithParameters (object x, string y) 
			{
				if ((x == null) || (y == null))
					return;

				int f = 2;
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeInParameterChecking () 
		{
			AssertRuleSuccess<NonDuplicatedCodeInParameterChecking> ();
		}

		class NonDuplicatedCodeInForeachLoopsReturningBoolean {
			bool ForeachComparingTypes () 
			{
				foreach (object obj in myList) {
					if (obj.GetType ().Equals (Type.EmptyTypes))
						return true;
				}
				return false;
			}

			bool ForeachComparingValues () 
			{
				foreach (int x in myList) {
					if (x % 2 == 0)
						return true;
				}
				return false;
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeInForeachLoopsReturningBooleanTest () 
		{
			AssertRuleSuccess<NonDuplicatedCodeInForeachLoopsReturningBoolean> ();
		}

		class FalsePositiveInConsoleRunner {
			Runner runner;

			static string GetAttribute (XmlNode node, string name, string defaultValue)
			{
				XmlAttribute xa = node.Attributes [name];
				if (xa == null)
					return defaultValue;
				return xa.Value;
			}

			IRule GetRule (string name)
			{
				foreach (IRule rule in runner.Rules) {
					if (rule.GetType ().ToString ().Contains (name)) 
						return rule;
				}
				return null;
			}
		
		 	void SetCustomParameters (XmlNode rules)
			{
				foreach (XmlElement parameter in rules.SelectNodes ("parameter")) {
					string ruleName = GetAttribute (parameter, "rule", String.Empty);
					string propertyName = GetAttribute (parameter, "property", String.Empty);
					int value = Int32.Parse (GetAttribute (parameter, "value", String.Empty));
				
					IRule rule = GetRule (ruleName);
					if (rule == null)
						throw new XmlException (String.Format ("The rule with name {0} doesn't exist.  Review your configuration file.", ruleName));
					PropertyInfo property = rule.GetType ().GetProperty (propertyName);
					if (property == null)
						throw new XmlException (String.Format ("The property {0} can't be found in the rule {1}.  Review your configuration file.", propertyName, ruleName));
					if (!property.CanWrite)
						throw new XmlException (String.Format ("The property {0} can't be written in the rule {1}.  Review your configuration file", propertyName, ruleName));
					property.GetSetMethod ().Invoke (rule, new object[] {value});
				}
			}
		}

		[Test]
		public void SuccessOnFalsePositiveInConsoleRunnerTest () 
		{
			AssertRuleSuccess<FalsePositiveInConsoleRunner> ();
		}

		class NonDuplicatedCodeCheckingSubsetOfParameters {
			ArrayList engine_dependencies = new ArrayList ();

			void Initialize () 
			{
				if (engine_dependencies.Count == 0)
					return;

				foreach (object eda in engine_dependencies)
					Console.WriteLine (eda);
			}

			void TearDown () 
			{
				if ((engine_dependencies == null) || (engine_dependencies.Count == 0))
					return;

				foreach (object eda in engine_dependencies) 
					Console.Write (eda);
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeCheckingSubsetOfParametersTest () 
		{
			AssertRuleSuccess<NonDuplicatedCodeCheckingSubsetOfParameters> ();
		}

		class NonDuplicatedCheckingAndReturningDifferentOperations {
			ulong mask;

			bool IsSubsetOf (string bitmask) 
			{
				if (bitmask == null)
					return false;
				return ((mask & bitmask[0]) == mask);
			}

			bool Equals (string bitmask) 
			{
				if (bitmask == null)
					return false;
				return (mask == bitmask[0]);
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCheckingAndReturningDifferentOperationsTest () 
		{
			AssertRuleSuccess<NonDuplicatedCheckingAndReturningDifferentOperations> ();	
		}

		class CheckingIntegersAndStrings {
			private IRunner Runner;

			bool CheckParameters (TypeReference eventType, MethodReference invoke) 
			{
				if (invoke.Parameters.Count == 2)
					return true;

				Runner.Report (eventType, Severity.Medium, Confidence.High, "The delegate should have 2 parameters");
				return false;

			}

			bool CheckGenericDelegate (TypeReference type)
			{
				if (type.FullName == "System.EventHandler`1")
					return true;

				Runner.Report (type, Severity.Medium, Confidence.High, "Generic delegates should use EventHandler<TEventArgs>");
				return false;
			}
		}

		[Test]
		public void SuccessOnCheckingIntegersAndStringsTest () 
		{
			AssertRuleSuccess<CheckingIntegersAndStrings> ();
		}

		class NonDuplicatedComparingAndReturningNull {
			string CheckTwoOptions (TypeReference type) 
			{
				if (type.Implements ("System.Collections", "IDictionary") || type.Implements ("System.Collections.Generic", "IDictionary`2"))
					return null;
				return "'Dictionary' should only be used for types implementing IDictionary and IDictionary<TKey,TValue>.";
			}

			string CheckThreeOptions (TypeReference type)
			{
				if (type.Implements ("System.Collections", "ICollection") ||
					type.Implements ("System.Collections", "IEnumerable") ||
					type.Implements ("System.Collections.Generic", "ICollection`1"))
					return null;

				if (type.Inherits ("System.Collections", "Queue") || type.Inherits ("System.Collections", "Stack") || 
					type.Inherits ("System.Data", "DataSet") || type.Inherits ("System.Data", "DataTable"))
					return null;

				return "'Collection' should only be used for implementing ICollection or IEnumerable or inheriting from Queue, Stack, DataSet and DataTable.";
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedComparingAndReturningNullTest () 
		{
			AssertRuleSuccess<NonDuplicatedComparingAndReturningNull> ();
		}

		class NonDuplicatedCodeComparingStringEmpty {
			int IndexOfFirstCorrectChar (string name) 
			{
				return 1;
			}

			string CheckStringAndReturn (string name) 
			{
				if (String.IsNullOrEmpty (name))
					return String.Empty;

				if (name.Length == 1)
					return name.ToUpperInvariant ();

				int index = IndexOfFirstCorrectChar (name);
				return Char.ToUpperInvariant (name [index]) + name.Substring (index + 1);
			}

			string CheckStringAndReturnTheOpposite (string name) 
			{
				if (String.IsNullOrEmpty (name))
					return String.Empty;

				if (name.Length == 1)
					return name.ToLowerInvariant ();

				int index = IndexOfFirstCorrectChar (name);
				return Char.ToLowerInvariant (name [index]) + name.Substring (index + 1);
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeComparingStringEmptyTest () 
		{
			AssertRuleSuccess<NonDuplicatedCodeComparingStringEmpty> ();
		}

		class NonDuplicatedCodeWithNonCompatibleTypes {
			
			public string EngineType { get; internal set; }

			public void SetEngineTypeFromType (Type engineType)
			{
				if (engineType == null)
					throw new ArgumentNullException ("engineType");
				EngineType = engineType.FullName;
			}

			public void SetEngineTypeFromString (string engineType)
			{
				if (engineType == null)
					throw new ArgumentNullException ("engineType");
				EngineType = engineType;
			}
		}

		[Test]
		public void SuccessOnNonDuplicatedCodeWithNonCompatibleTypesTest () 
		{
			//Although it seems the same pattern, we are not able to
			//extract the code fairly
			//If we chains the ctors, we could receive a nullref
			//exception.
			AssertRuleSuccess<NonDuplicatedCodeWithNonCompatibleTypes> ();
		}

		class NonDuplicateCodeWithNonCompatibleTypesInLocals {
			public void LocalSimple ()
			{
				MethodDefinition method = null;
				if (method == null)
					throw new Exception ();
				Console.WriteLine (method);
			}

			public void OptionEnum ()
			{
				TypeDefinition type = null;
				if (type == null)
					throw new Exception ();
				Console.WriteLine (type);
			}
		}

		[Test]
		public void SuccessOnNonDuplicateCodeWithNonCompatibleTypesInLocals ()
		{
			AssertRuleSuccess<NonDuplicateCodeWithNonCompatibleTypesInLocals> ();
		}

		// produced a false positive when compiled with CSC
		class AvoidLongMethodsRule {
			private static int CountInstanceFields (TypeDefinition type)
			{
				int counter = 0;
				foreach (FieldDefinition field in type.Fields) {
					if (!(field.IsStatic || field.HasConstant))
						counter++;
					//I not take care about arrays here.
				}
				return counter;
			}

			private static int CountInstructions (MethodDefinition method)
			{
				int count = 0;
				foreach (Instruction ins in method.Body.Instructions) {
					switch (ins.OpCode.Code) {
					case Code.Nop:
					case Code.Box:
					case Code.Unbox:
						break;
					default:
						count++;
						break;
					}
				}
				return count;
			}
		}

		[Test]
		public void IEnumerator_MoveNext_Block ()
		{
			AssertRuleSuccess<AvoidLongMethodsRule> ();
		}
	}
}
