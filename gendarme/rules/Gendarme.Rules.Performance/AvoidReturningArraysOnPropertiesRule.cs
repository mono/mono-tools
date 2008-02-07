//
// Gendarme.Rules.Performance.AvoidReturningArraysOnPropertiesRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
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

namespace Gendarme.Rules.Performance {

	public class AvoidReturningArraysOnPropertiesRule : IMethodRule {

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.IsGetter)
				return runner.RuleSuccess;

			if (!method.ReturnType.ReturnType.IsArray ())
				return runner.RuleSuccess;

			Location loc = new Location (method);
			Message msg = new Message ("Avoid returning arrays from properties.", loc, MessageType.Warning);
			return new MessageCollection (msg);
		}
	}
}
