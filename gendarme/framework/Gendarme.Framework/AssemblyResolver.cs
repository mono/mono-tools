//
// AssemblyResolver.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2007 Novell, Inc.
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

using Mono.Cecil;

// spouliot
using Gendarme.Framework.Rocks;

//spouliot: namespace Mono.Linker {
namespace Gendarme.Framework {

	public class AssemblyResolver : BaseAssemblyResolver {

		Hashtable _assemblies;

		public IDictionary AssemblyCache {
			get { return _assemblies; }
		}

		public AssemblyResolver ()
		{
			_assemblies = new Hashtable ();
		}

		public override AssemblyDefinition Resolve (AssemblyNameReference name)
		{
			AssemblyDefinition asm = (AssemblyDefinition) _assemblies [name.Name];
			if (asm == null) {
				asm = base.Resolve (name);
				asm.Resolver = this;
				_assemblies [name.Name] = asm;
			}

			return asm;
		}

		public TypeDefinition Resolve (TypeReference type)
		{
			type = type.GetOriginalType ();

			// spouliot
			TypeDefinition result = (type as TypeDefinition);
			if (result != null)
				return result;

			AssemblyNameReference reference = type.Scope as AssemblyNameReference;
			if (reference != null) {
				AssemblyDefinition assembly = Resolve (reference);
				return assembly.MainModule.Types [type.FullName];
			}

			ModuleDefinition module = type.Scope as ModuleDefinition;
			if (module != null)
				return module.Types [type.FullName];

			// spouliot
			GenericParameter generic = (type as GenericParameter);
			if (generic != null) {
				return (generic.Owner as TypeReference).Resolve ();
			}
			// spouliot
			throw new NotImplementedException ();
		}

		public FieldDefinition Resolve (FieldReference field)
		{
			TypeDefinition type = Resolve (field.DeclaringType);
			return GetField (type.Fields, field);
		}

		static FieldDefinition GetField (ICollection collection, FieldReference reference)
		{
			foreach (FieldDefinition field in collection) {
				if (field.Name != reference.Name)
					continue;

				if (!AreSame (field.FieldType, reference.FieldType))
					continue;

				return field;
			}

			return null;
		}

		public MethodDefinition Resolve (MethodReference method)
		{
			TypeDefinition type = Resolve (method.DeclaringType);
			method = method.GetOriginalMethod ();
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
					// spouliot
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

				if (!AreSame (meth.Parameters, reference.Parameters))
					continue;

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

		static bool AreSame (TypeReference a, TypeReference b)
		{
			while (a is TypeSpecification || b is TypeSpecification) {
				if (a.GetType () != b.GetType ())
					return false;

				a = ((TypeSpecification) a).ElementType;
				b = ((TypeSpecification) b).ElementType;
			}

			GenericParameter pa = (a as GenericParameter);
			GenericParameter pb = (b as GenericParameter);
			if ((pa != null) || (pb != null)) {
				if (a.GetType () != b.GetType ())
					return false;

				return pa.Position == pb.Position;
			}

			return a.FullName == b.FullName;
		}

		public void CacheAssembly (AssemblyDefinition assembly)
		{
			_assemblies [assembly.Name.FullName] = assembly;
			assembly.Resolver = this;
		}

// spouliot
		static private AssemblyResolver resolver;

		static public AssemblyResolver Resolver {
			get {
				if (resolver == null)
					resolver = new AssemblyResolver ();
				return resolver;
			}
		}
// spouliot
	}
}
