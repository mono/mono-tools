//
// Unit tests for RemoveOrphanResourcesRule
//
// Authors:
//	Antoine Vandecreme <ant.vand@gmail.com>
//
// Copyright (C) 2010 Antoine Vandecreme
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
using System.Reflection;

using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Mono.Cecil;

namespace Tests.Rules.Maintainability {

	[TestFixture]
	public sealed class RemoveOrphanResourceTest : AssemblyRuleTestFixture<RemoveOrphanResourcesRule> {

		AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// We use CodeBase insteed of Location to find the satellites assemblies
			string asmUri = Assembly.GetExecutingAssembly ().CodeBase;
			Uri uri = new Uri (asmUri);
			assembly = AssemblyFactory.GetAssembly (uri.AbsolutePath);
		}

		[Test]
		public void OrphanResources ()
		{
			// 1. StringOnlyInFrench
			// 2. ImageOnlyInFrench
			// 3. MainResourceMissing.fr.resx file
			AssertRuleFailure (assembly, 3);
		}
	}

}
