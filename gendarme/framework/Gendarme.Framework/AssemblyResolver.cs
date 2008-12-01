//
// AssemblyResolver.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	public class AssemblyResolver : BaseAssemblyResolver {

		Dictionary<string, AssemblyDefinition> assemblies;

		private AssemblyResolver ()
		{
			assemblies = new Dictionary<string, AssemblyDefinition> ();
		}

		public IDictionary<string,AssemblyDefinition> AssemblyCache {
			get { return assemblies; }
		}

		public override AssemblyDefinition Resolve (AssemblyNameReference name)
		{
			string aname = name.Name;
			AssemblyDefinition asm = null;
			if (!assemblies.TryGetValue (aname, out asm)) {
				try {
					asm = base.Resolve (name);
					asm.Resolver = this;
				}
				catch (FileNotFoundException) {
					// note: analysis will be incomplete
				}
				assemblies.Add (aname, asm);
			}
			return asm;
		}

		public TypeDefinition Resolve (TypeReference type)
		{
			type = type.GetOriginalType ();

			TypeDefinition result = (type as TypeDefinition);
			if (result != null)
				return result;

			AssemblyNameReference reference = type.Scope as AssemblyNameReference;
			if (reference != null) {
				AssemblyDefinition assembly = Resolve (reference);
				if (assembly == null)
					return null;
				return assembly.MainModule.Types [type.FullName];
			}

			ModuleDefinition module = type.Scope as ModuleDefinition;
			if (module != null)
				return module.Types [type.FullName];

			GenericParameter generic = (type as GenericParameter);
			if (generic != null) {
				return (generic.Owner as TypeReference).Resolve ();
			}

			throw new NotImplementedException ();
		}

		public FieldDefinition Resolve (FieldReference field)
		{
			TypeDefinition type = Resolve (field.DeclaringType);
			if (type == null)
				return (field as FieldDefinition);		// could not resolve type

			if (type.HasFields) {
				foreach (FieldDefinition fd in type.Fields) {
					if (fd.Name != field.Name)
						continue;

					if (!AreSame (fd.FieldType, field.FieldType))
						continue;

					return fd;
				}
			}

			return null;
		}

		public MethodDefinition Resolve (MethodReference method)
		{
			if (method == null)
				return null;

			TypeDefinition type = Resolve (method.DeclaringType);
			method = method.GetOriginalMethod ();
			if (type == null)
				return (method as MethodDefinition);		// could not resolve type

			if (method.Name == MethodDefinition.Cctor || method.Name == MethodDefinition.Ctor)
				return GetMethod (type.Constructors, method);
			else
				return GetMethod (type, method);
		}

		MethodDefinition GetMethod (TypeDefinition type, MethodReference reference)
		{
			while (type != null) {
				MethodDefinition method = GetMethod (type.Methods, reference);
				if (method == null) {
					// things like: System.Byte System.Byte[,]::Get(System.Int32,System.Int32)
					// would cause a NRE here
					if (type.BaseType == null)
						return null;
					type = Resolve (type.BaseType);
				} else {
					return method;
				}
			}

			return null;
		}

		static MethodDefinition GetMethod (ICollection collection, MethodReference reference)
		{
			foreach (MethodDefinition meth in collection) {
				if (meth.Name != reference.Name)
					continue;

				if (!AreSame (meth.ReturnType.ReturnType, reference.ReturnType.ReturnType))
					continue;

				if (meth.HasParameters) {
					if (!AreSame (meth.Parameters, reference.Parameters))
						continue;
				}

				return meth;
			}

			return null;
		}

		static bool AreSame (ParameterDefinitionCollection a, ParameterDefinitionCollection b)
		{
			if (a.Count != b.Count)
				return false;

			if (a.Count == 0)
				return true;

			for (int i = 0; i < a.Count; i++)
				if (!AreSame (a [i].ParameterType, b [i].ParameterType))
					return false;

			return true;
		}

		static bool AreSame (ModType a, ModType b)
		{
			if (!AreSame (a.ModifierType, b.ModifierType))
				return false;

			return AreSame (a.ElementType, b.ElementType);
		}

		static bool AreSame (TypeSpecification a, TypeSpecification b)
		{
			if (a is GenericInstanceType)
				return AreSame ((GenericInstanceType) a, (GenericInstanceType) b);

			if (a is ModType)
				return AreSame ((ModType) a, (ModType) b);

			return AreSame (a.ElementType, b.ElementType);
		}

		static bool AreSame (GenericInstanceType a, GenericInstanceType b)
		{
			if (!AreSame (a.ElementType, b.ElementType))
				return false;

			if (a.GenericArguments.Count != b.GenericArguments.Count)
				return false;

			if (a.GenericArguments.Count == 0)
				return true;

			for (int i = 0; i < a.GenericArguments.Count; i++)
				if (!AreSame (a.GenericArguments [i], b.GenericArguments [i]))
					return false;

			return true;
		}

		static bool AreSame (GenericParameter a, GenericParameter b)
		{
			return a.Position == b.Position;
		}

		static bool AreSame (TypeReference a, TypeReference b)
		{
			if (a is TypeSpecification || b is TypeSpecification) {
				if (a.GetType () != b.GetType ())
					return false;

				return AreSame ((TypeSpecification) a, (TypeSpecification) b);
			}

			if (a is GenericParameter || b is GenericParameter) {
				if (a.GetType () != b.GetType ())
					return false;

				return AreSame ((GenericParameter) a, (GenericParameter) b);
			}

			return a.FullName == b.FullName;
		}

		public void CacheAssembly (AssemblyDefinition assembly)
		{
			assembly.Resolver = this;
			assemblies.Add (assembly.Name.Name, assembly);
			string location = Path.GetDirectoryName (assembly.MainModule.Image.FileInformation.FullName);
			AddSearchDirectory (location);
		}

		static private AssemblyResolver resolver;

		static public AssemblyResolver Resolver {
			get {
				if (resolver == null)
					resolver = new AssemblyResolver ();
				return resolver;
			}
		}
	}
}
