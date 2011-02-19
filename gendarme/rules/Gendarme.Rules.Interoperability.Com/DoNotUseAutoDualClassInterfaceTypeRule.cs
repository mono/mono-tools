// 
// Gendarme.Rules.Interoperability.Com.DoNotUseAutoDualClassInterfaceTypeRule
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// Classes should not use ClassInterfaceAttribute with the value of 
	/// ClassInterfaceType.AutoDual because this may break COM clients
	/// if the class layout changes.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [ComVisible (true)]
	/// [ClassInterface (ClassInterfaceType.AutoDual)]
	/// class Bad {
	///	// do something
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (ClassInterfaceType.None):
	/// <code>
	/// [ComVisible (true)]
	/// [ClassInterface (ClassInterfaceType.None)]
	/// class Good : ICloneable {
	///	public object Clone ()
	///	{
	///		return new object ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (no ClassInterface attribute, equal to ClassInterfaceType.AutoDispatch):
	/// <code>
	/// [ComVisible (true)]
	/// class Good {
	///	// do something
	/// }
	/// </code>
	/// </example>
	[Problem ("Visible to COM class uses ClassInterface attribute with ClassInterfaceType.AutoDual, which may break COM clients if the class layout is chaged.")]
	[Solution ("Change the value of the ClassInterfaceAttribute to the AutoDispatch (default value) or None (and explicitly define an interface in this case).")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1408:DoNotUseAutoDualClassInterfaceType")]
	public class DoNotUseAutoDualClassInterfaceTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			ClassInterfaceType? attributeValue = GetClassInterfaceAttributeValue (type);

			bool fromAssembly = false;

			// attribute not found on class, try assembly instead
			if (attributeValue == null) {
				attributeValue = GetClassInterfaceAttributeValue (type.Module.Assembly);
				fromAssembly = true;
			}

			// not found on assembly as well, set default value
			if (attributeValue == null)
				attributeValue = ClassInterfaceType.AutoDispatch;

			if (attributeValue == ClassInterfaceType.AutoDual) {
				if (fromAssembly)
					Runner.Report (type, Severity.High, Confidence.High, "Attribute was set on assembly level");
				else
					Runner.Report (type, Severity.High, Confidence.High);
				return RuleResult.Failure;
			} else
				return RuleResult.Success;
		}

		private static ClassInterfaceType? GetClassInterfaceAttributeValue (ICustomAttributeProvider obj)
		{
			foreach (CustomAttribute attribute in obj.CustomAttributes) {
				// http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.classinterfaceattribute.aspx
				// any attribute without arguments can be skipped
				if (!attribute.HasConstructorArguments)
					continue;
				if (!attribute.AttributeType.IsNamed ("System.Runtime.InteropServices", "ClassInterfaceAttribute"))
					continue;
				var ctorArgs = attribute.ConstructorArguments;
				if (ctorArgs [0].Type.IsNamed ("System", "Int16"))
					return (ClassInterfaceType)(short)ctorArgs [0].Value;
				return (ClassInterfaceType)(int)ctorArgs [0].Value;
			}
			return null;
		}
	}
}
