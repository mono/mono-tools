// 
// Gendarme.Framework.IRuner interface
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Framework {

	// rules will have access to the runner thru this interface
	// so anyone can make it's own runner without using the provided base class
	public interface IRunner {

		// we should expose the list of assemblies, so rules can act on them
		// as "a set". E.g. checking for inheritance, smells ...

		// we should expose the list of rules, so rules can also act on them
		// E.g. Rule X is a superset of rule Y so Y disable itself is X is present

		Collection<IRule> Rules { get; }
		Dictionary<string, AssemblyDefinition> Assemblies  { get; }
		Collection<Defect> Defects  { get; }
		int VerbosityLevel { get; }

		event EventHandler<RunnerEventArgs> AnalyzeAssembly;	// ??? ProcessAssembly ???
		event EventHandler<RunnerEventArgs> AnalyzeModule;
		event EventHandler<RunnerEventArgs> AnalyzeType;
		event EventHandler<RunnerEventArgs> AnalyzeMethod;

		void Initialize ();

		void Report (Defect defect);

		[ComVisible (false)]
		void Report (IRule rule, AssemblyDefinition assembly, Severity severity, Confidence confidence, string message);
		[ComVisible (false)]
		void Report (IRule rule, TypeDefinition type, Severity severity, Confidence confidence, string message);
		[ComVisible (false)]
		void Report (IRule rule, FieldDefinition field, Severity severity, Confidence confidence, string message);
		[ComVisible (false)]
		void Report (IRule rule, MethodDefinition method, Severity severity, Confidence confidence, string message);
		[ComVisible (false)]
		void Report (IRule rule, MethodDefinition method, Instruction ins, Severity severity, Confidence confidence, string message);
		[ComVisible (false)]
		void Report (IRule rule, ParameterDefinition parameter, Severity severity, Confidence confidence, string message);
	}
}
