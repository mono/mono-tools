// 
// Gendarme.Rules.Design.DoNotDeclareSettersOnCollectionPropertiesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// The rule detect <c>System.Collections.ICollection</c> and 
	/// <c>System.Collections.Generic.ICollection&lt;T&gt;</c> properties that declare a visible setter.
	/// There is rarely a need to be able to replace the collection (e.g. most collections provide a <c>Clear</c>
	/// method) and having a getter only does not prevent the consumer from adding and removing items in the collection.
	/// Also read-only properties have special support for binary and XML serialization, making your code more useful.
	/// A special exception is made for <c>System.Security.PermissionSet</c> and types that derives from it.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Holder {
	///	public string Name { get; set; }
	///	public ICollection&lt;string&gt; List { get; set; }
	/// }
	/// 
	/// public static Holder Copy (Holder h)
	/// {
	///	Holder copy = new Holder ();
	///	copy.Name = h.Name;
	///	// bad, same list would be shared between instances
	///	copy.List = h.List;
	///	copy.List.AddRange (h.List);
	///	return copy;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Holder {
	///	List&lt;string&gt; list;
	///	
	///	public Holder ()
	///	{
	///		list = new List&lt;string&gt; ();
	///	}
	///	
	///	public string Name { get; set; }
	///	
	///	public ICollection&lt;string&gt; List { 
	///		get { return list; }
	///	}
	/// }
	/// 
	/// public static Holder Copy (Holder h)
	/// {
	///	Holder copy = new Holder ();
	///	copy.Name = h.Name;
	///	copy.List.AddRange (h.List);
	///	return copy;
	/// }
	/// </code>
	/// </example>
	[Problem ("A visible setter is declared for an ICollection (or derived) property")]
	[Solution ("Replace the setter with a method or decrease the setter visibility")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
	public class DoNotDeclareSettersOnCollectionPropertiesRule : Rule, ITypeRule {

		static bool IsICollection (TypeReference type)
		{
			if (type.Implements ("System.Collections", "ICollection"))
				return true;

			return type.Implements ("System.Collections.Generic", "ICollection`1");
		}

		static bool IsSpecialCase (TypeReference type)
		{
			return type.Inherits ("System.Security", "PermissionSet");
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasProperties || !type.IsVisible ())
				return RuleResult.DoesNotApply;

			bool is_interface = type.IsInterface;
			foreach (PropertyDefinition pd in type.Properties) {
				MethodDefinition setter = pd.SetMethod;
				if ((setter == null) || (!is_interface && !setter.IsVisible ()))
					continue;

				TypeReference ptype = pd.PropertyType;
				if (IsICollection (ptype) && !IsSpecialCase (ptype))
					Runner.Report (setter, Severity.Medium, Confidence.High);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
