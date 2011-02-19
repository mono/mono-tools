// 
// Gendarme.Rules.Serialization.MissingSerializableAttributeOnISerializableTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using System;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule checks for types that implement <c>System.ISerializable</c> but are
	/// not decorated with the <c>[Serializable]</c> attribute. Implementing 
	/// <c>System.ISerializable</c> is not enough to make a class serializable as this 
	/// interface only gives you more control over the basic serialization process. 
	/// In order for the runtime to know your type is serializable it must have the 
	/// <c>[Serializable]</c> attribute.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // this type cannot be serialized by the runtime
	/// public class Bad : ISerializable {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Serializable]
	/// public class Good : ISerializable {
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The runtime won't consider this type as serializable unless you add the [Serializable] attribute to its definition.")]
	[Solution ("Add [Serializable] to the type definition.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MissingSerializableAttributeOnISerializableTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interface (since [Serializable] is not applicable to interfaces)
			// nor does it apply to delegates
			if (type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// rule does not apply if the type does not implements ISerializable 
			if (!type.Implements ("System.Runtime.Serialization", "ISerializable"))
				return RuleResult.DoesNotApply;

			// rule applies only if base type is serializable
			if (!type.BaseType.IsNamed ("System", "Object")) {
				TypeDefinition base_type = type.BaseType.Resolve ();
				// in doubt don't report
				if ((base_type == null) || !base_type.IsSerializable)
					return RuleResult.DoesNotApply;
			}

			// rule applies, only Success or Failure from the point on

			// ok if the type has the [Serializable] pseudo-attribute
			if (type.IsSerializable)
				return RuleResult.Success;

			Runner.Report (type, Severity.Critical, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
