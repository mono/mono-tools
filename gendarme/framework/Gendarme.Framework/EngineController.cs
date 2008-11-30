// 
// Gendarme.Framework.EngineController
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
//

using System;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework.Engines;

namespace Gendarme.Framework {

	public class EngineController {

		internal Dictionary<string, Engine> engines = new Dictionary<string, Engine> ();

		public void Subscribe (string engineName)
		{
			if (engines.ContainsKey (engineName))
				return;

			Type type = Type.GetType (engineName);
			Engine engine = (Engine) Activator.CreateInstance (type);
			engine.Initialize (this);
			engines.Add (type.FullName, engine);
		}

		public void Unsubscribe (string engineName)
		{
			Engine engine;
			if (engines.TryGetValue (engineName, out engine)) {
				engine.TearDown ();
				engines.Remove (engineName);
			}
		}

		public event EventHandler<EngineEventArgs> BuildingCustomAttributes;
		public event EventHandler<EngineEventArgs> BuildingMethodBody;
		public event EventHandler<EngineEventArgs> BuildingType;

		public void Build (IList<AssemblyDefinition> list)
		{
			EngineEventArgs e = new EngineEventArgs (this);

			foreach (AssemblyDefinition assembly in list) {
				e.CurrentAssembly = assembly;

				BuildCustomAttributes (assembly, e);

				foreach (ModuleDefinition module in assembly.Modules) {
					e.CurrentModule = module;

					// TODO check custom attributes

					foreach (TypeDefinition type in module.Types) {
						e.CurrentType = type;

						EventHandler<EngineEventArgs> handler = BuildingType;
						if (handler != null)
							handler (type, e);

						BuildCustomAttributes (type, e);

						if (type.HasFields) {
							foreach (FieldDefinition field in type.Fields)
								BuildCustomAttributes (field, e);
						}

						// TODO check custom attributes (events)
						// TODO check custom attributes (properties)

						if (type.HasConstructors) {
							foreach (MethodDefinition ctor in type.Constructors)
								Build (ctor, e);
						}
						if (type.HasMethods) {
							foreach (MethodDefinition method in type.Methods)
								Build (method, e);
						}
					}
				}
			}
		}

		private void BuildCustomAttributes (ICustomAttributeProvider custom, EngineEventArgs e)
		{
			if (custom.HasCustomAttributes) {
				EventHandler<EngineEventArgs> handler = BuildingCustomAttributes;
				if (handler != null)
					handler (custom, e);
			}
		}

		private void Build (MethodDefinition method, EngineEventArgs e)
		{
			e.CurrentMethod = method;

			// TODO check custom attributes (methods)
			BuildCustomAttributes (method, e);

			// TODO check custom attributes (parameters)
			// TODO check custom attributes (generic parameters)
			// TODO check custom attributes (return value)

			if (method.HasBody) {
				EventHandler<EngineEventArgs> handler = BuildingMethodBody;
				if (handler != null)
					handler (method.Body, e);
			}
		}

		public void TearDown ()
		{
			BuildingCustomAttributes = null;
			BuildingMethodBody = null;
			BuildingType = null;

			foreach (Engine engine in engines.Values) {
				engine.TearDown ();
			}
		}

		public Engine this [string name] {
			get {
				Engine engine = null;
				engines.TryGetValue (name, out engine);
				return engine;
			}
		}

		// shortcuts to well known engines
		// this avoid casting inside the rules

		public OpCodeEngine OpCode {
			get {
				return (OpCodeEngine) this ["Gendarme.Framework.OpCodeEngine"];
			}
		}
	}
}
