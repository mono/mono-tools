// 
// Gendarme.Framework.EngineController
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008-2011 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	public class EngineController {

		private Dictionary<string, Engine> engines;

		public EngineController (IRunner runner)
		{
			Runner = runner;
			engines = new Dictionary<string, Engine> ();
		}

		public IRunner Runner {
			get;
			private set;
		}

		public void Subscribe (string engineName)
		{
			Engine engine;
			if (!engines.TryGetValue (engineName, out engine)) {
				Type type = Type.GetType (engineName);
				engine = (Engine) Activator.CreateInstance (type);
				engines.Add (type.FullName, engine);
			}
			engine.Initialize (this);
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
		public event EventHandler<EngineEventArgs> BuildingModule;
		public event EventHandler<EngineEventArgs> BuildingAssembly;

		public void Build (IList<AssemblyDefinition> list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");

			EngineEventArgs e = new EngineEventArgs (this);

			foreach (AssemblyDefinition assembly in list) {
				Build (assembly, e);

				foreach (ModuleDefinition module in assembly.Modules) {
					Build (module, e);

					foreach (TypeDefinition type in module.GetAllTypes ()) {
						Build (type, e);

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

			BuildCustomAttributes (method, e);

			if (method.HasGenericParameters) {
				// TODO: incomplete - only covers custom attributes
				foreach (GenericParameter gp in method.GenericParameters)
					BuildCustomAttributes (gp, e);
			}

			if (method.HasParameters) {
				// TODO: incomplete - only covers custom attributes
				foreach (ParameterDefinition parameter in method.Parameters)
					BuildCustomAttributes (parameter, e);
			}

			// TODO: incomplete - only covers custom attributes
			BuildCustomAttributes (method.MethodReturnType, e);

			if (method.HasBody) {
				EventHandler<EngineEventArgs> handler = BuildingMethodBody;
				if (handler != null)
					handler (method.Body, e);
			}
		}

		private void Build (TypeDefinition type, EngineEventArgs e)
		{
			e.CurrentType = type;

			EventHandler<EngineEventArgs> handler = BuildingType;
			if (handler != null)
				handler (type, e);

			BuildCustomAttributes (type, e);

			if (type.HasEvents) {
				// TODO: incomplete - only covers custom attributes
				foreach (EventDefinition evnt in type.Events)
					BuildCustomAttributes (evnt, e);
			}

			if (type.HasFields) {
				// TODO: incomplete - only covers custom attributes
				foreach (FieldDefinition field in type.Fields)
					BuildCustomAttributes (field, e);
			}

			if (type.HasGenericParameters) {
				// TODO: incomplete - only covers custom attributes
				foreach (GenericParameter gp in type.GenericParameters)
					BuildCustomAttributes (gp, e);
			}

			if (type.HasProperties) {
				// TODO: incomplete - only covers custom attributes
				foreach (PropertyDefinition prop in type.Properties)
					BuildCustomAttributes (prop, e);
			}
		}

		private void Build (ModuleDefinition module, EngineEventArgs e)
		{
			e.CurrentModule = module;

			EventHandler<EngineEventArgs> handler = BuildingModule;
			if (handler != null)
				handler (module, e);

			BuildCustomAttributes (module, e);
		}

		private void Build (AssemblyDefinition assembly, EngineEventArgs e)
		{
			e.CurrentAssembly = assembly;

			EventHandler<EngineEventArgs> handler = BuildingAssembly;
			if (handler != null)
				handler (assembly, e);

			BuildCustomAttributes (assembly, e);
		}

		public void TearDown ()
		{
			BuildingCustomAttributes = null;
			BuildingMethodBody = null;
			BuildingType = null;
			BuildingModule = null;
			BuildingAssembly = null;

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
