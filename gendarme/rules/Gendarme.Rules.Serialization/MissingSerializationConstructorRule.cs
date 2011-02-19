// 
// Gendarme.Rules.Serialization.MissingSerializationConstructorRule
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule checks for types that implement <c>System.ISerializable</c> but don't provide a
	/// serialization constructor. The constructor is required in order to make the type
	/// serializeable but cannot be enforced by the interface. 
	/// The serialization constructor should be <c>private</c> for <c>sealed</c> types and
	/// <c>protected</c> for unsealed types.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Serializable]
	/// public class Bad : ISerializable {
	/// 	public void GetObjectData (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (sealed):
	/// <code>
	/// [Serializable]
	/// public sealed class Good : ISerializable {
	/// 	private ClassWithConstructor (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// 	
	/// 	public void GetObjectData (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [Serializable]
	/// public class Good : ISerializable {
	/// 	protected ClassWithConstructor (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// 	
	/// 	public void GetObjectData (SerializationInfo info, StreamingContext context)
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The required constructor for ISerializable is not present in this type.")]
	[Solution ("Add a (private for sealed, protected otherwise) serialization constructor for this type.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2229:ImplementSerializationConstructors")]
	public class MissingSerializationConstructorRule : Rule, ITypeRule {

		// localizable
		private const string NoSerializationCtorText = "The required constructor for ISerializable is not present in this type.";
		private const string CtorSealedTypeText = "The serialization constructor should be private since this type is sealed.";
		private const string CtorUnsealedTypeText = "The serialization constructor should be protected (family) since this type is not sealed.";

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interfaces, delegates or types that does not implement ISerializable
			if (type.IsInterface || type.IsDelegate () || !type.Implements ("System.Runtime.Serialization", "ISerializable"))
				return RuleResult.DoesNotApply;

			// rule applies, only Success or Failure from the point on

			// check if the type implements the serialization constructor
			MethodDefinition ctor = type.GetMethod (MethodSignatures.SerializationConstructor);
			if (ctor == null) {
				// no serialization ctor
				Runner.Report (type, Severity.High, Confidence.Total, NoSerializationCtorText);
				return RuleResult.Failure;
			} else if (type.IsSealed) {
				// with ctor: on a sealed type the ctor must be private
				if (!ctor.IsPrivate) {
					Runner.Report (type, Severity.Low, Confidence.Total, CtorSealedTypeText);
					return RuleResult.Failure;
				}
			} else {
				// with ctor: on a unsealed type the ctor must be family
				if (!ctor.IsFamily) {
					Runner.Report (type, Severity.Low, Confidence.Total, CtorUnsealedTypeText);
					return RuleResult.Failure;
				}
			}

			// everything is fine
			return RuleResult.Success;
		}
	}
}
