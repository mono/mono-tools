//
// Gendarme.Rules.Maintainability.AvoidLackOfCohesionOfMethodsRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2008 Cedric Vivier
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks every type for lack of cohesion between the fields and the methods. Low cohesion is often
	/// a sign that a type is doing too many, different and unrelated things. The cohesion score is given for each defect 
	/// (higher is better).
	/// Automatic properties (e.g. available in C# 3) are considered as fields since their 'backing field' cannot be
	/// directly used and the getter/setter is only used to update a single field.
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The methods in this class lack cohesion (a higher score is better). This leads to code which is harder to understand and maintain.")]
	[Solution ("You can apply the Extract Class or Extract Subclass refactoring.")]
	public class AvoidLackOfCohesionOfMethodsRule : Rule, ITypeRule {

		private const double DefaultSuccessCoh = 0.5;
		private const double DefaultLowCoh = 0.4;
		private const double DefaultMediumCoh = 0.2;
		private const int DefaultMethodMinimumCount = 2;	// set at 2 to remove 'uninteresting' types
		private const int DefaultFieldMinimumCount = 1;		// set at 1 to remove 'uninteresting' types (this shouldn't be set to a value other than MinimumMethodCount/2)
		
		private double successCoh = DefaultSuccessCoh;
		private double lowCoh = DefaultLowCoh;
		private double medCoh = DefaultMediumCoh;
		private int method_minimum_count = DefaultMethodMinimumCount;
		private int field_minimum_count = DefaultFieldMinimumCount;
		private Dictionary<MemberReference, int> F = new Dictionary<MemberReference, int> ();
		private List<MemberReference> Fset = new List<MemberReference> ();

		public RuleResult CheckType (TypeDefinition type)
		{
			//does rule apply?
			if (type.IsEnum || type.IsInterface || type.IsAbstract || type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			if (!type.HasFields || (type.Fields.Count < MinimumFieldCount))
				return RuleResult.DoesNotApply;
			if (!type.HasMethods || (type.Methods.Count < MinimumMethodCount))
				return RuleResult.DoesNotApply;

			//yay! rule do apply!
			double coh = GetCohesivenessForType (type);
			if (coh >= SuccessLowerLimit)
				return RuleResult.Success;
			if (0 == coh)
				return RuleResult.DoesNotApply;

			//how's severity?
			Severity sev = GetCohesivenessSeverity(coh);

			string msg = String.Format (CultureInfo.CurrentCulture, "Type cohesiveness : {0}%", (int) (coh * 100));
			Runner.Report (type, sev, Confidence.Normal, msg);
			return RuleResult.Failure;
		}

		// works with "field-like" automatic properties
		private bool SetField (MemberReference field, TypeReference type)
		{
			if ((field.DeclaringType != type) || Fset.Contains (field))
				return false;

			if (F.ContainsKey (field)) {
				F [field]++;
			} else {
				F.Add (field, 1);
			}
			Fset.Add (field);
			return true;
		}

		public double GetCohesivenessForType (TypeDefinition type)
		{
			if (type == null)
				return 0.0;

			int M = 0;//M is the number of methods in the type
			//F keeps the count of distinct-method accesses to the field
			F.Clear ();

			foreach (MethodDefinition method in type.Methods) {
				if (!method.HasBody || method.IsStatic)
					continue;

				//Mset is true if the method has already incremented M
				bool Mset = false;
				//Fset keeps the fields already addressed in the current method
				Fset.Clear ();

				foreach (Instruction inst in method.Body.Instructions) {
					MemberReference mr = null;

					switch (inst.OpCode.OperandType) {
					case OperandType.InlineField:
						FieldDefinition fd = inst.Operand as FieldDefinition;
						if (null == fd || !fd.IsPrivate || fd.IsStatic || fd.IsGeneratedCode ())
							continue; //does not make sense for LCOM calculation
						mr = fd;
						break;
					case OperandType.InlineMethod:
						// special case for automatic properties since the 'backing' fields won't be used
						MethodDefinition md = inst.Operand as MethodDefinition;
						if (md == null || md.IsPrivate || md.IsStatic || !md.IsProperty () || !md.IsGeneratedCode ())
							continue;
						mr = md.GetPropertyByAccessor ();
						break;
					}

					if (mr != null) {
						bool result = SetField (mr, type);
						if (result && !Mset) {
							M++;
							Mset = true;
						}
					}
				}
			}

			if (M > MinimumMethodCount && F.Count > MinimumFieldCount) {
				double r = 0;
				foreach (KeyValuePair<MemberReference,int> f in F) {
					r += f.Value;
				}
				double rm = r / F.Count;
				double coh = Math.Round(1 - ((M - rm) / M), 2);
				return coh;
			}

			return 0;
		}

		private Severity GetCohesivenessSeverity (double coh)
		{
			if (coh >= LowSeverityLowerLimit) return Severity.Low;
			if (coh >= MediumSeverityLowerLimit) return Severity.Medium;
			return Severity.High;//coh<MediumSeverityLowerLimit
		}


		/// <summary>Cohesion values lower than this will result in a defect.</summary>
		/// <remarks>Defaults to 0.5.</remarks>
		[DefaultValue (DefaultSuccessCoh)]
		[Description ("Cohesion values lower than this will result in a defect.")]
		public double SuccessLowerLimit
		{
			get { return successCoh; }
			set { successCoh = value; }
		}

		/// <summary>Defects with cohesion values greater than this will be reported at low severity.</summary>
		/// <remarks>Defaults to 0.4.</remarks>
		[DefaultValue (DefaultLowCoh)]
		[Description ("Defects with cohesion values greater than this will be reported at low severity.")]
		public double LowSeverityLowerLimit
		{
			get { return lowCoh; }
			set { lowCoh = value; }
		}

		/// <summary>Defects with cohesion values greater than (but less than LowSeverityLowerLimit) this will be reported at medium severity.</summary>
		/// <remarks>Defaults to 0.2.</remarks>
		[DefaultValue (DefaultMediumCoh)]
		[Description ("Defects with cohesion values greater than (but less than LowSeverityLowerLimit) this will be reported at medium severity.")]
		public double MediumSeverityLowerLimit
		{
			get { return medCoh; }
			set { medCoh = value; }
		}

		/// <summary>The minimum number of methods a class must have to be checked.</summary>
		/// <remarks>Defaults to 2.</remarks>
		[DefaultValue (DefaultMethodMinimumCount)]
		[Description ("The minimum number of methods a class must have to be checked.")]
		public int MinimumMethodCount
		{
			get { return method_minimum_count; }
			set {
				if (value < 1)
					throw new ArgumentException ("MinimumMethodCount cannot be lesser than 1", "MinimumMethodCount");
				method_minimum_count = value;
			}
		}

		/// <summary>The minimum number of fields a class must have to be checked.</summary>
		/// <remarks>Defaults to 1.</remarks>
		[DefaultValue (DefaultFieldMinimumCount)]
		[Description ("The minimum number of fields a class must have to be checked.")]
		public int MinimumFieldCount
		{
			get { return field_minimum_count; }
			set {
				if (value < 0)
					throw new ArgumentException ("MinimumFieldCount cannot be lesser than 0", "MinimumFieldCount");
				field_minimum_count = value;
			}
		}
	}
}
