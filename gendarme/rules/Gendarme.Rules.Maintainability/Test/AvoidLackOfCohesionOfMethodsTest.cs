//
// Unit Test for AvoidLackOfCohesionOfMethods Rule.
//
// Authors:
//      Cedric Vivier <cedricv@neonux.com>
//
//      (C) 2008 Cedric Vivier
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

using Gendarme.Framework;
using Gendarme.Rules.Maintainability;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;


namespace Test.Rules.Maintainability {

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExpectedCohesivenessAttribute : Attribute
	{
		public ExpectedCohesivenessAttribute(double c)
		{
			_c = c;
			_d = Double.Epsilon;
		}

		public ExpectedCohesivenessAttribute (double c, double delta)
		{
			_c = c;
			_d = delta;
		}

		public double Value {
			get { return _c; }
		}

		public double Delta {
			get { return _d; }
		}

		private double _c;
		private double _d;
	}

	#pragma warning disable 414, 169
	interface Interface
	{
		void DoFoo();
	}

	public class ClassWithNoField
	{
		public void DoFoo()
		{
		}
	}

	public class ClassWithUnusedField
	{
		int x;

		public void DoFoo()
		{
		}
	}

	[ExpectedCohesiveness(1)]
	public class PerfectCohesion
	{
		int x;
		int y;

		public int DoFooXY()
		{
			return (x = y);
		}

		public int DoFooYX()
		{
			return (y = x);
		}

		public void DoBarYX()
		{
			x = 1;
			y = 2;
		}
	}

	[ExpectedCohesiveness(0.67)]
	public class GoodCohesion
	{
		int x;
		int y;

		public void DoFooX()
		{
			x = 2;
		}

		public void DoFooY()
		{
			y = 4;
		}

		public void DoBarXY()
		{
			x = y;
		}
	}

	[ExpectedCohesiveness(0)]//0 because does not apply
	public class AverageCohesionNotBadBecauseOfLowNumberOfMethods
	{
		int x;
		int y;

		public void DoSomething()
		{
			x = 2;
		}

		public void DoSomethingElse()
		{
			y = 2;
		}
	}

	[ExpectedCohesiveness(0.5)]
	public class AverageCohesionLimitBecauseOfHighNumberOfMethods
	{
		int x;
		int y;

		public void DoSomething()
		{
			x = 2;
		}

		public void DoSomethingElse()
		{
			y = 2;
		}

		public void DoSomethingCrazy()
		{
			y = 4;
		}

		public void DoSomethingSmart()
		{
			x = 4;
		}
	}

	[ExpectedCohesiveness(0.33)]
	public class BadCohesion
	{
		int x;
		int y;
		int z;

		public void DoSomething()
		{
			x = 2;
		}

		public void DoSomethingElse()
		{
			y = 2;
		}

		public void DoSomethingDifferent()
		{
			z = 2;
		}
	}

	// CSC 9.x result varies if /o (optimize) is ON or OFF and we want the test to pass in both cases
	[ExpectedCohesiveness (0.17, 0.2)]
	public class VeryBadCohesion
	{
		int x;
		int y;
		int z = 0;
		int u;
		int v = 23;
		int w;

		public int DoSomethingX()
		{
			x = 2;
			return x;
		}

		public int DoSomethingY()
		{
			y = 2;
			return y;
		}

		public void DoSomethingZ()
		{
			z = 2;
		}

		public void DoSomethingU()
		{
			u = 2;
		}

		public void DoSomethingV()
		{
			v = 4;
		}

		public int DoSomethingW()
		{
			return w.GetHashCode();
		}
	}

	[ExpectedCohesiveness(0.33)]
	public class BadCohesionNotVeryBadBecauseOfInheritance : BadCohesion
	{
		int u;
		int v;
		int w;

		public void DoSomethingU()
		{
			u = 2;
		}

		public void DoSomethingV()
		{
			v = 2;
		}

		public void DoSomethingW()
		{
			w = 2;
		}
	}

	[ExpectedCohesiveness (0)]
	public class ClassWithProtectedField {
		protected int x;

		public void TouchField ()
		{
			x = 1;
		}
	}

	[ExpectedCohesiveness (0)]
	public class ClassWithStaticMethod {
		int x;

		public static void Foo ()
		{
		}
	}

	[ExpectedCohesiveness (0)]
	public class ClassWithMethodUsingStaticField {
		static int x;

		public static void TouchStaticField ()
		{
			x = 1;
		}
	}
	#pragma warning restore 414, 169


	[TestFixture]
	public class AvoidLackOfCohesionOfMethodsTest : TypeRuleTestFixture<AvoidLackOfCohesionOfMethodsRule>
	{

		[Test]
		public void CohesivenessMeasurementTest ()
		{
			Type[] types = Assembly.GetExecutingAssembly ().GetTypes ();
			ExpectedCohesivenessAttribute expectedCoh;
			double coh;

			foreach (Type type in types)
			{
				if (0 == type.GetCustomAttributes (typeof(ExpectedCohesivenessAttribute), false).Length)
					continue;

				expectedCoh = (ExpectedCohesivenessAttribute)
					type.GetCustomAttributes (
							typeof(ExpectedCohesivenessAttribute), false)[0];

				AvoidLackOfCohesionOfMethodsRule rule = new AvoidLackOfCohesionOfMethodsRule();
				coh = rule.GetCohesivenessForType (
							DefinitionLoader.GetTypeDefinition (type));

				Assert.IsTrue (
					Math.Abs (coh - expectedCoh.Value) <= expectedCoh.Delta,
					"Cohesiveness for type '{0}' is {1} but should have been {2} +/- {3}.",
					type, coh, expectedCoh.Value, expectedCoh.Delta);
			}
		}

		[Test]
		public void GoodCohesionsTest ()
		{
			AssertRuleSuccess<PerfectCohesion> ();
			AssertRuleSuccess<GoodCohesion> ();
			AssertRuleSuccess<AverageCohesionLimitBecauseOfHighNumberOfMethods> ();
		}

		[Test]
		public void BadCohesionsTest ()
		{
			AssertRuleFailure<BadCohesion> ();
			AssertRuleFailure<VeryBadCohesion> ();
			AssertRuleFailure<BadCohesionNotVeryBadBecauseOfInheritance> ();
		}

		[Test]
		public void MethodsDoNotApplyTest ()
		{
			AssertRuleDoesNotApply<Interface> ();
			AssertRuleDoesNotApply<ClassWithNoField> ();
			AssertRuleDoesNotApply<ClassWithUnusedField> ();
			AssertRuleDoesNotApply<AverageCohesionNotBadBecauseOfLowNumberOfMethods> ();
			AssertRuleDoesNotApply<ClassWithProtectedField> ();
			AssertRuleDoesNotApply<ClassWithStaticMethod> ();
			AssertRuleDoesNotApply<ClassWithMethodUsingStaticField> ();
		}
	}
}
