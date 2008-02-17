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

	[Problem ("The runtime won't consider this type as serializable unless your add the [Serialization] attribute to its definition.")]
	[Solution ("Add [Serialization] to the type definition.")]
	public class MissingSerializableAttributeOnISerializableTypeRule : Rule, ITypeRule {

		private const string ISerializable = "System.Runtime.Serialization.ISerializable";

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interface (since [Serializable] is not applicable to interfaces)
			// nor does it apply to delegates
			if (type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// rule does not apply if the type does not implements ISerializable 
			if (!type.Implements (ISerializable))
				return RuleResult.DoesNotApply;

			// rule applies only if base type is serializable
			if (type.BaseType.FullName != "System.Object") {
				TypeDefinition base_type = type.BaseType.Resolve ();
				// in doubt don't report
				if ((base_type == null) || !base_type.IsSerializable)
					return RuleResult.DoesNotApply;
			}

			// rule applies, only Success or Failure from the point on

			// ok if the type has the [Serializable] pseudo-attribute
			if (type.IsSerializable)
				return RuleResult.Success;

			Runner.Report (type, Severity.Critical, Confidence.Total, String.Empty);
			return RuleResult.Failure;
		}
	}
}
