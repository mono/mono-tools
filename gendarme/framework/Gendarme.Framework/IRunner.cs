// 
// Gendarme.Framework.IRunner interface
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Framework {

	/// <summary>
	/// Rules will have access to the runner thru this interface.
	/// This makes it possible, to anyone, to make it's own runner without using
	/// the provided base class.
	/// </summary>
	[ComVisible (false)]
	public interface IRunner {

		/// <summary>
		/// This expose the list of rules, so rules can also act on them.
		/// E.g. Rule X is a superset of rule Y so Y disable itself is X is present
		/// </summary>
		Collection<IRule> Rules { get; }

		/// <summary>
		/// This expose the list of assemblies so rules can act on them as "a set". 
		/// E.g. applying a rule based on data outside the current assembly
		/// </summary>
		Collection<AssemblyDefinition> Assemblies { get; }

		IIgnoreList IgnoreList { get; set; }

		Collection<Defect> Defects  { get; }
		int DefectsLimit { get; }
		Bitmask<Severity> SeverityBitmask { get; }
		Bitmask<Confidence> ConfidenceBitmask { get; }

		int VerbosityLevel { get; }

		EngineController Engines { get; }

		/// <summary>
		/// Helper property to avoid each rule having it's own state/logic about 
		/// the result (RuleResult.Success or RuleResult.Failure) of its analysis
		/// </summary>
		RuleResult CurrentRuleResult { get; }

		event EventHandler<RunnerEventArgs> AnalyzeAssembly;	// ??? ProcessAssembly ???
		event EventHandler<RunnerEventArgs> AnalyzeModule;
		event EventHandler<RunnerEventArgs> AnalyzeType;
		event EventHandler<RunnerEventArgs> AnalyzeMethod;

		void Initialize ();

		void Report (Defect defect);

		void Report (IMetadataTokenProvider metadata, Severity severity, Confidence confidence);
		void Report (IMetadataTokenProvider metadata, Severity severity, Confidence confidence, string message);

		void Report (MethodDefinition method, Instruction ins, Severity severity, Confidence confidence);
		void Report (MethodDefinition method, Instruction ins, Severity severity, Confidence confidence, string message);

		void TearDown ();
	}
}
