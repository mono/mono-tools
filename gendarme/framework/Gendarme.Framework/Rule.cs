// 
// Gendarme.Framework.Rule base class
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
using System.Globalization;

namespace Gendarme.Framework {

	/// <summary>
	/// Most rules should be able to inherit from Rule and implement one of the
	/// <c>IAssemblyRule</c>, <c>ITypeRule</c> or <c>IMethodRule</c> and override 
	/// the Check[Assembly|Type|Method] method.
	/// </summary>
	abstract public class Rule : IRule {

		private bool active = true;
		private string name;
		private string full_name;
		private string problem;
		private string solution;
		private Uri uri;
		private Type type;
		private ApplicabilityScope applicability_scope = ApplicabilityScope.All;
		private object [] engine_dependencies = null;

		/// <summary>
		/// Return true if the rule is currently active, false otherwise.
		/// </summary>
		public virtual bool Active {
			get { return active; }
			set { active = value; }
		}

		/// <summary>
		/// Return the runner executing the rule. This is helpful to get information
		/// outside the rule, like the list of assemblies being analyzed.
		/// </summary>
		public IRunner Runner {
			get;
			private set;
		}

		/// <summary>
		/// Return the short name of the rule.
		/// By default this returns the name of the current class.
		/// </summary>
		public virtual string Name {
			get {
				if (name == null)
					name = Type.Name;
				return name;
			}
		}

		/// <summary>
		/// Return the full name of the rule.
		/// By default this returns the full name of the current class.
		/// </summary>
		public virtual string FullName {
			get {
				if (full_name == null)
					full_name = Type.FullName;
				return full_name;
			}
		}

		private Type Type {
			get {
				if (type == null)
					type = GetType ();
				return type;
			}
		}

		private object GetCustomAttribute (Type t)
		{
			object [] attributes = Type.GetCustomAttributes (t, true);
			if (attributes.Length == 0)
				return null;
			return attributes [0];
		}

		public virtual string Problem { 
			get {
				if (problem == null) {
					object obj = GetCustomAttribute (typeof (ProblemAttribute));
					if (obj == null)
						problem = "Missing [Problem] attribute on rule.";
					else
						problem = (obj as ProblemAttribute).Problem;
				}
				return problem;
			}
		}

		public virtual string Solution { 
			get {
				if (solution == null) {
					object obj = GetCustomAttribute (typeof (SolutionAttribute));
					if (obj == null)
						solution = "Missing [Solution] attribute on rule.";
					else
						solution = (obj as SolutionAttribute).Solution;
				}
				return solution;
			}
		}

		/// <summary>
		/// Return an Uri instance to the rule documentation.
		/// By default, if no [DocumentationUri] attribute is used on the rule, this returns:
		/// http://www.mono-project.com/{rule name space}#{rule name}
		/// </summary>
		public virtual Uri Uri {
			get {
				if (uri == null) {
					object [] attributes = Type.GetCustomAttributes (typeof (DocumentationUriAttribute), true);
					if (attributes.Length == 0) {
						string url = String.Format (CultureInfo.InvariantCulture, 
							"http://www.mono-project.com/{0}#{1}", type.Namespace, Name);
						uri = new Uri (url);
					} else {
						uri = (attributes [0] as DocumentationUriAttribute).DocumentationUri;
					}
				}
				return uri;
			}
		}

		/// <summary>
		/// Initialize the rule. This is where rule can do it's heavy initialization
		/// since the assemblies to be analyzed are already known (and accessible thru
		/// the runner parameter).
		/// </summary>
		/// <param name="runner">The runner that will execute this rule.</param>
		public virtual void Initialize (IRunner runner)
		{
			if (runner == null)
				throw new ArgumentNullException ("runner");

			Runner = runner;

			// read attribute only once (e.g. the wizard can initialize multiple times)
			if (engine_dependencies == null)
				engine_dependencies = Type.GetCustomAttributes (typeof (EngineDependencyAttribute), true);

			if (engine_dependencies.Length == 0)
				return;

			// subscribe to each engine the rule depends on
			foreach (EngineDependencyAttribute eda in engine_dependencies) {
				runner.Engines.Subscribe (eda.EngineType);
			}
		}

		public virtual void TearDown ()
		{
			if ((engine_dependencies == null) || (engine_dependencies.Length == 0))
				return;

			foreach (EngineDependencyAttribute eda in engine_dependencies) {
				Runner.Engines.Unsubscribe (eda.EngineType);
			}
		}

		public ApplicabilityScope ApplicabilityScope {
			get {
				return applicability_scope;
			}
			set {
				applicability_scope = value;
			}
		}
	}
}
