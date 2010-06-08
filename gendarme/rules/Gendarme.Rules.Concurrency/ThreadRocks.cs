//
// Gendarme.Rules.Concurrency.ThreadRocks
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using System;

namespace Gendarme.Rules.Concurrency {
	
	internal static class ThreadRocks {
		
		public static ThreadModelAttribute ThreadingModel (this TypeReference tr)
		{
			ThreadModelAttribute model;
			
			TypeDefinition type = tr.Resolve ();
			while (type != null) {
				model = TryGetThreadingModel (type);
				if (model != null)
					return model;
					
				// If the type is not decorated then we'll assume that the type is main 
				// thread unless it's a System/Mono type.
				if (ThreadedNamespace (type.Namespace))
					return new ThreadModelAttribute (ThreadModel.Concurrent);
					
				type = type.DeclaringType != null ? type.DeclaringType.Resolve () : null;
			}
			
			return new ThreadModelAttribute (ThreadModel.MainThread);
		}
		
		public static ThreadModelAttribute ThreadingModel (this MethodDefinition method)
		{
			ThreadModelAttribute model;
			
			// Check the method first so it overrides whatever was used on the type.
			model = TryGetThreadingModel (method);
			if (model != null)
				return model;
			
			// If it's a property we need to check the property as well.
			if (method.IsProperty ()) {
				string name = GetNameSuffix (method);
				PropertyDefinition [] props = method.DeclaringType.Properties.GetProperties (name);
				if (props.Length == 1) {				// FIXME: we won't get the property if it is an explicit implementation
					model = TryGetThreadingModel (props [0]);
					if (model != null)
						return model;
				}
			}
			
			// If it's a event we need to check the event as well.
			if (method.IsAddOn || method.IsRemoveOn || method.IsFire) {
				string name = GetNameSuffix (method);
				EventDefinition evt = method.DeclaringType.Events.GetEvent (name);
				
				model = TryGetThreadingModel (evt);
				if (model != null)
					return model;
			}
			
			// Check the type.
			model = ThreadingModel (method.DeclaringType);
			
			if (method.IsConstructor && method.IsStatic) {
				if (model.Model == ThreadModel.Concurrent || model.Model == ThreadModel.Serializable) {
					return new ThreadModelAttribute (ThreadModel.SingleThread);
				}
				
			} else if (method.IsStatic) {
				if (model.Model == ThreadModel.Serializable && !method.Name.StartsWith ("op_")) {
					return new ThreadModelAttribute (ThreadModel.MainThread);
				}
			}
			
			return model;
		}
		
		// Returns true if the namespace is one for which we consider all the types thread safe.
		public static bool ThreadedNamespace (string ns)
		{
			if (ns == "System" || ns.StartsWith ("System."))
				return true;
				
			if (ns == "Mono" || ns.StartsWith ("Mono."))
				return true;
			
			return false;
		}
		
		#region Private Methods
		private static ThreadModelAttribute TryGetThreadingModel (ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return null;

			foreach (CustomAttribute attr in provider.CustomAttributes) {
				if (attr.Constructor.DeclaringType.Name == "ThreadModelAttribute") {
					attr.Resolve ();
					
					if (attr.ConstructorParameters.Count == 1) {
						if (attr.ConstructorParameters [0] is int) {
							ThreadModel value = (ThreadModel) (int) attr.ConstructorParameters [0];
							return new ThreadModelAttribute (value);
						
						} else {
							throw new ArgumentException ("There should be a single ThreadModelAttribute ctor taking an (Int32) ThreadModel enum argument.");
						}
					
					} else {
						throw new ArgumentException ("There should be a single ThreadModelAttribute ctor taking an (Int32) ThreadModel enum argument.");
					}
				}
			}
			
			return null;
		}
		
		private static string GetNameSuffix (IMemberReference method)
		{
			string name = method.Name;
			
			// Need the offset for explicit interface implementations.
			int offset = Math.Max (name.LastIndexOf ('.'), 0);
			int i = name.IndexOf ('_', offset);
			System.Diagnostics.Debug.Assert (i > 0, "didn't find a '_' in " + name);
			
			return name.Substring (i + 1);
		}
		#endregion
	}
}
