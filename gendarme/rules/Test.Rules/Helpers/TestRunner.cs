// 
// Gendarme.Framework.TestRunner
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

namespace Test.Rules.Helpers {

	/// <summary>
	/// To execute properly Gendarme.Framework.Runner keeps the state of
	/// two variables internally, the current I[Assembly|Type|Method]Rule
	/// and the current target (IMetadataTokenProvider to match the 
	/// [Assembly|Module|Type|Method]Definition being analyzed). This 
	/// class emulate this behavior and also reset the Defects count 
	/// before each Check[Assembly|Type|Method] calls so we can easily
	/// Assert on Defects.Count.
	/// </summary>
	public class TestRunner : Runner {

		static TestRunner ()
		{
			string mono_path = Environment.GetEnvironmentVariable ("MONO_PATH");
			if (!String.IsNullOrEmpty (mono_path)) {
				string [] paths = mono_path.Split (':');
				foreach (string path in paths)
					AssemblyResolver.Resolver.AddSearchDirectory (path);
			}
		}

		private RunnerEventArgs event_args;

		public TestRunner (IRule rule)
		{
			CurrentRule = rule;
			CurrentRule.Initialize (this);
			Rules.Clear ();
			Rules.Add (rule);
			IgnoreList = new BasicIgnoreList (this);
			Initialize ();
		}

		private RuleResult PreCheck (IMetadataTokenProvider obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj", "Cannot check a null object");

			Reset ();
			AssemblyDefinition assembly = obj.GetAssembly ();
			if (!Assemblies.Contains (assembly)) {
				Assemblies.Clear ();
				Assemblies.Add (assembly);
				Engines.Build (Assemblies);
			}
			CurrentTarget = obj;

			return IgnoreList.IsIgnored (CurrentRule, obj) ? RuleResult.DoesNotApply : RuleResult.Success;
		}
		
		private RuleResult PostCheck (RuleResult beforeTearingDown)
		{
			CurrentRule.TearDown ();
			//If current is not the default, and report the greater
			if (beforeTearingDown == RuleResult.DoesNotApply) 
				return beforeTearingDown;
			
			if (CurrentRuleResult > beforeTearingDown) 
				return CurrentRuleResult;
			return beforeTearingDown;	
		}
	
		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			RuleResult result = PreCheck (assembly);
			if (result == RuleResult.Success)
				result = (CurrentRule as IAssemblyRule).CheckAssembly (assembly);
			return PostCheck (result);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			RuleResult result = PreCheck (type);
			if (result == RuleResult.Success)
				result = (CurrentRule as ITypeRule).CheckType (type);
			return PostCheck (result);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			RuleResult result = PreCheck (method);
			if (result == RuleResult.Success)
				result = (CurrentRule as IMethodRule).CheckMethod (method);
			return PostCheck (result);
		}

		// reuse the same instance
		private RunnerEventArgs Arguments {
			get {
				if (event_args == null)
					event_args = new RunnerEventArgs (this);
				return event_args;
			}
		}

		/// <summary>
		/// Calls the protected Runner.OnAssembly method, which raise the AnalyzeAssembly event.
		/// </summary>
		/// <param name="assembly">The AssemblyDefinition which will be used to fill the RunnerEventArgs parameter to OnAssembly.</param>
		public void OnAssembly (AssemblyDefinition assembly)
		{
			Arguments.CurrentAssembly = assembly;
			Arguments.CurrentModule = null;
			Arguments.CurrentType = null;
			Arguments.CurrentMethod = null;
			base.OnAssembly (Arguments);
		}

		/// <summary>
		/// Calls the protected Runner.OnModule method, which raise the AnalyzeModule event.
		/// </summary>
		/// <param name="module">The ModuleDefinition which will be used to fill the RunnerEventArgs parameter to OnModule.</param>
		public void OnModule (ModuleDefinition module)
		{
			Arguments.CurrentAssembly = module.Assembly;
			Arguments.CurrentModule = module;
			Arguments.CurrentType = null;
			Arguments.CurrentMethod = null;
			base.OnModule (Arguments);
		}

		/// <summary>
		/// Calls the protected Runner.OnType method, which raise the AnalyzeType event.
		/// </summary>
		/// <param name="type">The TypeDefinition which will be used to fill the RunnerEventArgs parameter to OnType.</param>
		public void OnType (TypeDefinition type)
		{
			Arguments.CurrentAssembly = type.Module.Assembly;
			Arguments.CurrentModule = type.Module;
			Arguments.CurrentType = type;
			Arguments.CurrentMethod = null;
			base.OnType (Arguments);
		}

		/// <summary>
		/// Calls the protected Runner.OnMethod, which raise the AnalyzeMethod event.
		/// </summary>
		/// <param name="method">The MethodDefinition which will be used to fill the RunnerEventArgs parameter to OnMethod.</param>
		public void OnMethod (MethodDefinition method)
		{
			Arguments.CurrentAssembly = method.DeclaringType.Module.Assembly;
			Arguments.CurrentModule = method.DeclaringType.Module;
			Arguments.CurrentType = (method.DeclaringType as TypeDefinition);
			Arguments.CurrentMethod = method;
			base.OnMethod (Arguments);
		}
	}
}
