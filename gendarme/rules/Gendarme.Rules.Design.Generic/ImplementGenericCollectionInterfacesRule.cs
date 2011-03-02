//
// Gendarme.Rules.Design.Generic.ImplementGenericCollectionInterfacesRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
	/// This rule checks for types which implement the non-generic <code>System.IEnumerable</code> interface but 
	/// not the <code>System.IEnumerable&lt;T&gt;</code> interface. Implementing the generic version
	/// of <code>System.IEnumerable</code> avoids casts, and possibly boxing, when iterating the collection.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class IntEnumerable : IEnumerable {
	/// 	public IEnumerator GetEnumerator ()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class IntEnumerable : IEnumerable&lt;int&gt; {
	///	public IEnumerator&lt;int&gt; GetEnumerator ()
	///	{
	///	}
	///	
	///	IEnumerator IEnumerable.GetEnumerator ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("This type implements the non-generic IEnumerable interface but not IEnumerable<T> which would make your collection type-safe.")]
	[Solution ("Implement one of generic collection interfaces such as IEnumerable<T>, ICollection<T> or IList<T>.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
	public class ImplementGenericCollectionInterfacesRule : GenericsBaseRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums, interfaces and generated code
			if (type.IsEnum || type.IsInterface || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;
			
			// rule applies only to visible types
			if (!type.IsVisible ())
				return RuleResult.DoesNotApply;

			// rule only applies if the type implements IEnumerable
			if (!type.Implements ("System.Collections", "IEnumerable"))
				return RuleResult.DoesNotApply;
		
			// rule does not apply to the types implementing IDictionary
			if (type.Implements ("System.Collections", "IDictionary"))
				return RuleResult.DoesNotApply;

			// the type should implement IEnumerable<T> too
			if (!type.Implements ("System.Collections.Generic", "IEnumerable`1"))
				Runner.Report (type, Severity.Medium, Confidence.High);

			return Runner.CurrentRuleResult;
		}
	}
}
