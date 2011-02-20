//
// Gendarme.Rules.Concurrency.DoNotUseThreadStaticWithInstanceFieldsRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule will fire if an instance field is decorated with a <c>[ThreadStatic]</c> attribute. 
	/// This is an error because the attribute will only work with static fields.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // the field isn't static so this will do nothing
	/// [ThreadStatic]
	/// private List&lt;object&gt; items;
	/// 
	/// public void Add (object item)
	/// {
	/// 	// If the field was thread safe this would ensure that each thread had 
	/// 	// its own copy of the list.
	/// 	if (items == null) {
	/// 		items = new List&lt;object&gt; ();
	///	 	}
	/// 		
	/// 	items.Add (item);
	/// } 
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// private List&lt;object&gt; items = new List&lt;object&gt; ();
	/// private object mutex = new object ();
	/// 
	/// // Typically some form of locking such as the code below is used to
	/// // serialize access to instance fields. However you can also use
	/// // Threading.Thread.Thread::AllocateNamedDataSlot or AllocateDataSlot.
	/// public void Add (object item)
	/// {
	/// 	lock (mutex) {
	/// 		items.Add (item);
	/// 	}
	/// } 
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("An instance field is decorated with System.ThreadStaticAttribute.")]
	[Solution ("ThreadStaticAttribute will only make static fields thread safe. To make an instance field thread safe you need to use techniques like locking or System.Threading.Thread.Thread::AllocateNamedDataSlot.")]
	public sealed class DoNotUseThreadStaticWithInstanceFieldsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.HasFields || type.IsEnum)
				return RuleResult.DoesNotApply;
			
			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsStatic && field.HasAttribute ("System", "ThreadStaticAttribute")) {
					Runner.Report (field, Severity.Critical, Confidence.Total);
				}
			}
						
			return Runner.CurrentRuleResult;
		}
	}
}
