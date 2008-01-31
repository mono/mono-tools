// 
// Gendarme.Rules.Security.StaticConstructorsShouldBePrivateRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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

namespace Gendarme.Rules.Security {

	public class StaticConstructorsShouldBePrivateRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			if (typeDefinition.Constructors.Count == 0)
				return runner.RuleSuccess;

			MethodDefinition violatingConstructor = null;
			foreach (MethodDefinition constructor in typeDefinition.Constructors) {
				if (constructor.IsStatic && !constructor.IsPrivate) {
					violatingConstructor = constructor;
					break; // there cannot be two .cctor's so we can stop looking
				}
			}

			if (violatingConstructor == null)
				return runner.RuleSuccess;

			Location loc = new Location (violatingConstructor);
			Message msg = new Message ("Static constructors must be private because otherwise they may be called once or multiple times from user code.", loc, MessageType.Error);
			return new MessageCollection (msg);
		}
	}
}
