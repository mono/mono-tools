//
// Gendarme.Rules.Maintainability.AvoidLackOfCohesionOfMethodsRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// 	(C) 2008 Cedric Vivier
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
using System.Linq;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	[Problem ("The methods in this class lacks cohesion. This leads to code harder to understand and maintain.")]
	[Solution ("You can apply the Extract Class or Extract Subclass refactoring.")]
	public class AvoidLackOfCohesionOfMethodsRule : Rule, ITypeRule
	{

		public RuleResult CheckType (TypeDefinition type)
		{
			//does rule apply?
			if (type.IsEnum || type.IsInterface || type.IsAbstract || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			//yay! rule do apply!
			double coh = GetCohesivenessForType (type);
			if (coh > SuccessLowerLimit)
				return RuleResult.Success;
			if (0 == coh)
				return RuleResult.DoesNotApply;

			//how's severity?
			Severity sev = GetCohesivenessSeverity(coh);

			string s = (Runner.VerbosityLevel < 2) ? String.Empty :
					String.Format ("Type's cohesiveness : {0}.", coh);

			Runner.Report (type, sev, Confidence.Normal, s);
			return RuleResult.Failure;
		}
		
		//TODO: Perhaps is a nice candidate to be a rock ?
		//It will allow us use linq with Cecil in the rules :)
		private IEnumerable<T> ToIEnumerable<T> (IEnumerable enumerable) 
		{
			foreach (T t in enumerable)
				yield return t;
		}

		public double GetCohesivenessForType (TypeDefinition type)
		{
			int M = 0;//M is the number of methods in the type
			//F keeps the count of distinct-method accesses to the field
			Dictionary<FieldReference, int> F = new Dictionary<FieldReference, int>();
			
			var methods = from met in ToIEnumerable<MethodDefinition> (type.Methods)
				where met.HasBody 
				where !met.IsSpecialName
				select met; 

			foreach (MethodDefinition method in methods)
			{
				//Mset is true if the method has already incremented M
				bool Mset = false;
				//Fset keeps the fields already addressed in the current method
				List<FieldReference> Fset = new List<FieldReference>();

				foreach (Instruction inst in method.Body.Instructions)
				{
					if (OperandType.InlineField == inst.OpCode.OperandType)
					{
						FieldDefinition fd = inst.Operand as FieldDefinition;
						if (null != fd && fd.DeclaringType == type && !Fset.Contains(fd))
						{
							if (!Mset) M++;
							Mset = true;
							if (F.ContainsKey(fd))
							{
								F[fd]++;
							}
							else
							{
								F.Add(fd, 1);
							}
							Fset.Add(fd);
						}
					}
				}
			}

			if (M > MinimumMethodCount && F.Count > MinimumFieldCount)
			{
				double r = 0;
				foreach (KeyValuePair<FieldReference,int> f in F)
				{
					r += f.Value;
				}
				double rm = r / F.Count;
				double coh = Math.Round(1 - ((M - rm) / M), 2);
				return coh;
			}

			return 0;
		}

		public Severity GetCohesivenessSeverity (double coh)
		{
			if (coh >= LowSeverityLowerLimit) return Severity.Low;
			if (coh >= MediumSeverityLowerLimit) return Severity.Medium;
			return Severity.High;//coh<MediumSeverityLowerLimit
		}


		public double SuccessLowerLimit
		{
			get { return _successCoh; }
			set { _successCoh = value; }
		}
		protected double _successCoh = 0.5;

		public double LowSeverityLowerLimit
		{
			get { return _lowCoh; }
			set { _lowCoh = value; }
		}
		protected double _lowCoh = 0.4;

		public double MediumSeverityLowerLimit
		{
			get { return _medCoh; }
			set { _medCoh = value; }
		}
		protected double _medCoh = 0.2;

		public int MinimumMethodCount
		{
			get { return _minMethodCount; }
			set {
				if (value < 1) throw new ArgumentException("MinimumMethodCount", "MinimumMethodCount cannot be lesser than 1");
				_minMethodCount = value;
			}
		}
		protected int _minMethodCount = 2;//set at 2 to remove 'uninsteresting' types

		public int MinimumFieldCount
		{
			get { return _minFieldCount; }
			set {
				if (value < 0) throw new ArgumentException("MinimumFieldCount", "MinimumFieldCount cannot be lesser than 0");
				_minFieldCount = value;
			}
		}
		protected int _minFieldCount = 1;//set at 1 to remove 'uninsteresting' types
		                                 //this shouldn't be set to another value than MinimumMethodCount/2

	}

}

