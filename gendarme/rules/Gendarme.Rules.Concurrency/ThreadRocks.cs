//
// Gendarme.Rules.Concurrency.ThreadRocks
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2009 Jesse Jones
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;

namespace Gendarme.Rules.Concurrency {
	
	internal static class ThreadRocks {

		public static bool AllowsEveryCaller (this ThreadModel self)
		{
			return ((self & ThreadModel.AllowEveryCaller) == ThreadModel.AllowEveryCaller);
		}

		public static bool Is (this ThreadModel self, ThreadModel model)
		{
			// since ThreadModel.MainThread == 0 we cannot do bitwise ops
			// but we need to keep this "as is" for backward compatibility
			return ((self & ~ThreadModel.AllowEveryCaller) == model);
		}

		public static ThreadModel ThreadingModel (this TypeReference type)
		{
			while (type != null) {
				if (type.IsDefinition) {
					ThreadModel? model = TryGetThreadingModel ((TypeDefinition) type);
					if (model != null)
						return model.Value;
				}

				// If the type is not decorated then we'll assume that the type is main 
				// thread unless it's a System/Mono type.
				if (ThreadedNamespace (type.Namespace))
					return ThreadModel.Concurrent;
					
				type = type.DeclaringType;
			}
			
			return ThreadModel.MainThread;
		}
		
		static ThreadModel? Lookup<TDefinition> (MemberReference method, IEnumerable<TDefinition> collection)
			where TDefinition : IMemberDefinition
		{
			string name = method.Name;
			// Need the offset for explicit interface implementations.
			int offset = Math.Max (name.LastIndexOf ('.'), 0);
			offset = name.IndexOf ('_', offset) + 1;

			foreach (IMemberDefinition member in collection) {
				string member_name = member.Name;
				if (String.CompareOrdinal (name, offset, member_name, 0, member_name.Length) == 0)
					return TryGetThreadingModel (member);
			}
			return null;
		}

		public static ThreadModel ThreadingModel (this MethodDefinition method)
		{
			// Check the method first so it overrides whatever was used on the type.
			ThreadModel? model = TryGetThreadingModel (method);
			if (model != null)
				return model.Value;
			
			// If it's a property we need to check the property as well.
			if (method.IsProperty ()) {
				// FIXME: we won't get the property if it is an explicit implementation
				model = Lookup (method, method.DeclaringType.Properties);
				if (model != null)
					return model.Value;
			}
			
			// If it's a event we need to check the event as well.
			if (method.IsAddOn || method.IsRemoveOn || method.IsFire) {
				model = Lookup (method, method.DeclaringType.Events);
				if (model != null)
					return model.Value;
			}
			
			// Check the type.
			model = ThreadingModel (method.DeclaringType);
			
			if (method.IsConstructor && method.IsStatic) {
				if (model == ThreadModel.Concurrent || model == ThreadModel.Serializable) {
					return ThreadModel.SingleThread;
				}
				
			} else if (method.IsStatic) {
				if (model == ThreadModel.Serializable && !method.Name.StartsWith ("op_", StringComparison.Ordinal)) {
					return ThreadModel.MainThread;
				}
			}
			
			return model.Value;
		}
		
		// Returns true if the namespace is one for which we consider all the types thread safe.
		public static bool ThreadedNamespace (string ns)
		{
			if (ns == "System" || ns.StartsWith ("System.", StringComparison.Ordinal))
				return true;

			if (ns == "Mono" || ns.StartsWith ("Mono.", StringComparison.Ordinal))
				return true;
			
			return false;
		}
		
		#region Private Methods
		private static ThreadModel? TryGetThreadingModel (ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return null;

			foreach (CustomAttribute attr in provider.CustomAttributes) {
				// ThreadModelAttribute ctor has a single parameter, skip param-less attributes
				if (!attr.HasConstructorArguments)
					continue;
				if (attr.AttributeType.Name != "ThreadModelAttribute")
					continue;

				IList<CustomAttributeArgument> cp = attr.ConstructorArguments;
				if ((cp.Count == 1) && (cp [0].Value is int))
					return (ThreadModel) (int) cp [0].Value;
						
				throw new ArgumentException ("There should be a single ThreadModelAttribute ctor taking an (Int32) ThreadModel enum argument.");
			}
			
			return null;
		}

		#endregion
	}
}
