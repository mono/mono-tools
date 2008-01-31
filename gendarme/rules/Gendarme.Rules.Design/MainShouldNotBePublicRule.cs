// 
// Gendarme.Rules.Design.MainShouldNotBePublicRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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

namespace Gendarme.Rules.Design {

	public class MainShouldNotBePublicRule : IAssemblyRule {

		// might be moved to Rocks as well
		private static bool IsVBAssembly (AssemblyDefinition assemblyDefinition)
		{
			// as of now, we check for Microsoft.VisualBasic.dll reference
			foreach (AssemblyNameReference r in assemblyDefinition.MainModule.AssemblyReferences) {
				if (r.Name == "Microsoft.VisualBasic")
					return true;
			}
			return false;
		}


		public MessageCollection CheckAssembly (AssemblyDefinition assemblyDefinition, Runner runner)
		{
			// assembly must have an entry point to be examined
			MethodDefinition entryPoint = assemblyDefinition.EntryPoint;
			if (entryPoint == null)
				return runner.RuleSuccess;

			// RULE APPLIES

			// we have to check declaringType's visibility so 
			// if we can't get access to it (is this possible?) we abandon
			// also, if it is not public, we don't have to continue our work
			// - we can't reach Main () anyways
			TypeDefinition declaringType = entryPoint.DeclaringType as TypeDefinition;
			if (declaringType == null || !declaringType.IsPublic)
				return runner.RuleSuccess;

			// at last, if Main () is not public, then it's okay
			if (!entryPoint.IsPublic)
				return runner.RuleSuccess;

			Location loc;
			string message;
			if (!IsVBAssembly (assemblyDefinition)) {
				loc = new Location (entryPoint);
				message = "Main () method should not be visible outside the assembly. Make it private or internal.";
			} else {
				loc = new Location (declaringType); // point at Module
				message = "Main () method should not be visible outside the assembly. Do not make the module or class containing it public.";
			}
			Message msg = new Message (message, loc, MessageType.Error);
			return new MessageCollection (msg);
		}
	}
}
