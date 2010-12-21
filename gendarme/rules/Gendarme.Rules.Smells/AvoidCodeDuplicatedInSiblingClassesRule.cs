//
// Gendarme.Rules.Smells.AvoidCodeDuplicatedInSiblingClassesRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	/// <summary>
	/// This rule looks for code duplicated in sibling subclasses.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BaseClassWithCodeDuplicated {
	///	protected IList list;
	/// }
	///
	/// public class OverriderClassWithCodeDuplicated : BaseClassWithCodeDuplicated {
	/// 	public void CodeDuplicated ()
	/// 	{
	///		foreach (int i in list) {
	///			Console.WriteLine (i);
	///		}
	/// 		list.Add (1);
	/// 	}
	/// }
	/// 
	/// public class OtherOverriderWithCodeDuplicated : BaseClassWithCodeDuplicated {
	///	public void OtherMethod ()
	///	{
	///		foreach (int i in list) {
	///			Console.WriteLine (i);
	///		}
	///		list.Remove (1);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class BaseClassWithoutCodeDuplicated {
	///	protected IList list;
	/// 
	///	protected void PrintValuesInList ()
	///	{
	///		foreach (int i in list) {
	///			Console.WriteLine (i);
	///		}
	///	}
	/// }
	/// 
	/// public class OverriderClassWithoutCodeDuplicated : BaseClassWithoutCodeDuplicated {
	/// 	public void SomeCode ()
	///	{
	///		PrintValuesInList ();
	///		list.Add (1);
	///	}
	/// }
	/// 
	/// public class OtherOverriderWithoutCodeDuplicated : BaseClassWithoutCodeDuplicated {
	/// 	public void MoreCode ()
	///	{
	///		PrintValuesInList ();
	///		list.Remove (1);
	///	}
	/// }	
	/// </code>
	/// </example>

	[Problem ("There is similar code in various methods in sibling classes.  Your code will be better if you can unify them.")]
	[Solution ("You can apply the Pull Up Method refactoring.")]
	public class AvoidCodeDuplicatedInSiblingClassesRule : Rule, ITypeRule {

		private CodeDuplicatedLocator codeDuplicatedLocator;
		private List<TypeDefinition> siblingClasses;

		public AvoidCodeDuplicatedInSiblingClassesRule ()
		{
			codeDuplicatedLocator = new CodeDuplicatedLocator (this);
			siblingClasses = new List<TypeDefinition> ();
		}

		private void FindCodeDuplicated (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods)
				foreach (TypeDefinition sibling in siblingClasses)
					codeDuplicatedLocator.CompareMethodAgainstTypeMethods (method, sibling);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// don't analyze cases where no methods (or body) are available
			if (type.IsEnum || type.IsInterface)
				return RuleResult.DoesNotApply;

			foreach (TypeDefinition module_type in type.Module.GetAllTypes ()) {
				if ((module_type.BaseType != null) && module_type.BaseType.Equals (type))
					siblingClasses.Add (module_type);
			}

			if (siblingClasses.Count >= 2) {
				codeDuplicatedLocator.Clear ();

				foreach (TypeDefinition sibling in siblingClasses) {
					FindCodeDuplicated (sibling);
					codeDuplicatedLocator.CheckedTypes.AddIfNew (sibling.Name);
				}
			}

			siblingClasses.Clear ();

			return Runner.CurrentRuleResult;
		}
	}
}
