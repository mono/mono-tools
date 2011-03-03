//
// Gendarme.Rules.Design.Generic.AvoidMethodWithUnusedGenericTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008,2010-2011 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

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
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("One or more generic type parameters are not used in the formal parameter list.")]
	[Solution ("This prevents the compiler from inferring types when the method is used which results in hard to use API definitions.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	public class AvoidMethodWithUnusedGenericTypeRule : GenericsBaseRule, IMethodRule {

		static bool FindGenericType (IGenericInstance git, string nameSpace, string name)
		{
			foreach (object o in git.GenericArguments) {
				if (IsGenericParameter (o, nameSpace, name))
					return true;

				GenericInstanceType inner = (o as GenericInstanceType);
				if ((inner != null) && (FindGenericType (inner, nameSpace, name)))
					return true;
			}
			return false;
		}

		static bool IsGenericParameter (object obj, string nameSpace, string name)
		{
			return (obj as GenericParameter).IsNamed (nameSpace, name);
		}

		static bool IsGenericType (TypeReference type, string nspace, string name)
		{
			if (type.IsNamed (nspace, name))
				return true;

			var type_spec = type as TypeSpecification;
			if (type_spec != null && type_spec.ElementType.IsNamed (nspace, name))
				return true;

			// handle things like ICollection<T>
			GenericInstanceType git = (type as GenericInstanceType);
			if (git == null)
				return false;

			return FindGenericType (git, nspace, name);
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
				string nspace = gp.Namespace;
				string name = gp.Name;
				// ... is being used by the method parameters
				foreach (ParameterDefinition pd in method.Parameters) {
					if (IsGenericType (pd.ParameterType, nspace, name)) {
						found = true;
						break;
					}
				}
				if (!found) {
					// it's a defect when used only for the return value - but we reduce its severity
					if (IsGenericType (method.ReturnType, nspace, name))
						severity = Severity.Low;
				}
				if (!found) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"Generic parameter '{0}.{1}' is not used by the method parameters.", nspace, name);
					Runner.Report (method, severity, Confidence.High, msg);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
