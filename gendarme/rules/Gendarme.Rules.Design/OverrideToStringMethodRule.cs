//
// Gendarme.Rules.Design.OverrideToStringMethodRule
//
// Authors:
//    Lex Li <lextudio@gmail.com>
//    Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2010 Lex Li
//  (C) 2008 Andreas Noever
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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design
{
    /// <summary>
    /// This rule warns when a type does not override the <c>Object.ToString</c> function.
    /// </summary>
    /// <example>
    /// Bad example:
    /// <code>
    /// class DoesNotOverrideEquals 
    /// {
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// Good example:
    /// <code>
    /// class OverridesToString 
    /// {
    ///    public override string ToString ()
    ///    {
    ///        return "My name is OverridesToString";
    ///    }
    /// }
    /// </code>
    /// </example>
    [Problem ("This type does not override the ToString method.")]
    [Solution ("Override the ToString method to help debugging.")]
    public class OverrideToStringMethodRule : Rule, ITypeRule {

        private readonly MethodSignature _toString = new MethodSignature("ToString", "System.String", new string[0]);

        public RuleResult CheckType (TypeDefinition type)
        {
            if (type.IsEnum || type.IsInterface || type.IsDelegate () || type.IsStatic() || type.FullName.Contains("<")) 
                // the last check replaces: type.FullName.StartsWith("<Module>") || type.FullName.StartsWith("<PrivateImplementationDetails>"))
            {
                return RuleResult.DoesNotApply;
            }

            if (type.HasMethod (_toString))
            {
                return RuleResult.Success;
            }
            
            Runner.Report (type, Severity.Low, Confidence.High);
            return RuleResult.Failure;
        }
    }
}