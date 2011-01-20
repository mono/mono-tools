//
// Gendarme.Rules.Design.Generic.AvoidMethodWithUnusedGenericTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008,2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This method will fire if a generic method does not use all of its generic type parameters
	/// in the formal parameter list. This usually means that either the type parameter is not used at
	/// all in which case it should be removed or that it's used only for the return type which
	/// is problematic because that prevents the compiler from inferring the generic type 
	/// when the method is called which is confusing to many developers.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	public string ToString&lt;T&gt; ()
	///	{
	///		return typeof (T).ToString ();
	///	}
	///	
	///	static void Main ()
	///	{
	///		// the compiler can't infer int so we need to supply it ourselves
	///		Console.WriteLine (ToString&lt;int&gt; ());
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	public string ToString&lt;T&gt; (T obj)
	///	{
	///		return obj.GetType ().ToString ();
	///	}
	///	
	///	static void Main ()
	///	{
	///		Console.WriteLine (ToString (2));
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("One or more generic type parameters are not used in the formal parameter list.")]
	[Solution ("This prevents the compiler from inferring types when the method is used which results in hard to use API definitions.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public class AvoidMethodWithUnusedGenericTypeRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since generics were not available before
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0);
			};
		}

		static bool FindGenericType (IGenericInstance git, string fullname)
		{
			foreach (object o in git.GenericArguments) {
				if (IsGenericParameter (o, fullname))
					return true;

				GenericInstanceType inner = (o as GenericInstanceType);
				if ((inner != null) && (FindGenericType (inner, fullname)))
					return true;
			}
			return false;
		}

		static bool IsGenericParameter (object obj, string fullname)
		{
			GenericParameter gp = (obj as GenericParameter);
			return ((gp != null) && (gp.FullName == fullname));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has generic type parameters
			if (!method.HasGenericParameters || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// look if every generic type parameter...
			foreach (GenericParameter gp in method.GenericParameters) {
				Severity severity = Severity.Medium;
				bool found = false;
				string gp_fullname = gp.FullName;
				// ... is being used by the method parameters
				foreach (ParameterDefinition pd in method.Parameters) {
					var parameter_type = pd.ParameterType;

					if (parameter_type.FullName == gp_fullname) {
						found = true;
						break;
					}

					var type_spec = parameter_type as TypeSpecification;
					if (type_spec != null && type_spec.ElementType.FullName == gp_fullname) {
						found = true;
						break;
					}

					// handle things like ICollection<T>
					GenericInstanceType git = (parameter_type as GenericInstanceType);
					if (git == null)
						continue;

					if (FindGenericType (git, gp_fullname)) {
						found = true;
						break;
					}
				}
				if (!found) {
					// it's a defect when used only for the return value - but we reduce its severity
					if (IsGenericParameter (method.ReturnType, gp_fullname))
						severity = Severity.Low;
				}
				if (!found) {
					string msg = String.Format ("Generic parameter '{0}' is not used by the method parameters.", gp_fullname);
					Runner.Report (method, severity, Confidence.High, msg);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
