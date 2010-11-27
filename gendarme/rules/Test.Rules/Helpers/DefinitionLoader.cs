//
// Test.Rules.Helpers.DefinitionLoader
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Test.Rules.Helpers {
	
	/// <summary>
	/// Class that provides helper methods to load type and method definitions.
	/// </summary>
	public static class DefinitionLoader {

		/// <summary>
		/// Gets full name for a type to be loaded by Cecil, replacing '+' with '/' if one is nested.
		/// </summary>
		/// <param name="type">Type to get full name for.</param>
		/// <returns>Full name using '/' as a nesting separator ready to be loaded by Cecil.</returns>
		private static string GetCecilNestedTypeName (string type)
		{
			return type.Replace ('+', '/');
		}

		/// <summary>
		/// Gets full name for a type to be loaded by Cecil, replacing '+' with '/' if one is nested.
		/// </summary>
		/// <param name="type">Type to get full name for.</param>
		/// <returns>Full name using '/' as a nesting separator ready to be loaded by Cecil.</returns>
		private static string GetCecilTypeName (Type type)
		{
			string name = type.FullName;

			if (type.IsGenericType) {
				int pos = name.IndexOf ("[");
				if (pos > 0)
					name = name.Substring (0, pos);
			}

			if (type.IsNested)
				return GetCecilNestedTypeName (name);

			return name;
		}

		private static bool MatchParameters (MethodDefinition method, Type [] parameters)
		{
			if (method.Parameters.Count != parameters.Length)
				return false;

			for (int i = 0; i < method.Parameters.Count; i++) {
				if (parameters [i].FullName != method.Parameters [i].ParameterType.FullName)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Gets a MethodDefinition for method by its name and parameter types.
		/// </summary>
		/// <param name="type">Type which contains method to load.</param>
		/// <param name="methodName">Name of the method to load.</param>
		/// <param name="methodParameters">Array of method parameter types.</param>
		/// <returns>MethodDefinition associated with the specified method.</returns>
		public static MethodDefinition GetMethodDefinition (TypeDefinition type, string methodName, Type [] methodParameters)
		{
			int ambiguous = 0;
			MethodDefinition result = null;

			foreach (MethodDefinition method in type.Methods) {
				if (method.Name != methodName)
					continue;

				if (methodParameters == null) {
					ambiguous++;
					result = method;
					continue;
				}

				// check parameters
				if (MatchParameters (method, methodParameters)) {
					result = method;
					break;
				}
			}

			if (result == null) {
				string msg = String.Format ("Method {0} was not found in class {1}.", methodName, type.FullName);
				throw new ArgumentException (msg, "methodName");
			}

			// ambiguous (multiple overloads, parameters not specified)
			if (ambiguous > 1) {
				string msg = String.Format ("Name {0} is ambiguous between {1} overloads. You should also pass parameter types in this case.", methodName, ambiguous);
				throw new ArgumentException (msg, "methodName");
			}

			return result;
		}

		/// <summary>
		/// Gets a MethodDefinition for method by its name and parameters.
		/// </summary>
		/// <param name="methodName">Name of the method to load.</param>
		/// <param name="methodParameters">Array of method parameter types.</param>
		/// <typeparam name="T">Type which contains the method to load.</typeparam>
		/// <returns>MethodDefinition associated with the specified method.</returns>
		public static MethodDefinition GetMethodDefinition<T> (string methodName, Type [] methodParameters)
		{
			TypeDefinition typeDefinition = GetTypeDefinition (typeof (T));

			if (typeDefinition == null)
				throw new ArgumentException (string.Format ("Could not load {0} type.", typeof (T).FullName));
			
			return GetMethodDefinition (typeDefinition, methodName, methodParameters);
		}

		/// <summary>
		/// Gets a MethodDefinition for method by its name.
		/// </summary>
		/// <param name="methodName">Name of the method to load.</param>
		/// <typeparam name="T">Type which contains the method to load.</typeparam>
		/// <returns>MethodDefinition associated with the specified method.</returns>
		public static MethodDefinition GetMethodDefinition<T> (string methodName)
		{
			return GetMethodDefinition<T> (methodName, null);
		}

		/// <summary>
		/// Gets AssemblyDefiniton containing the specified type.
		/// </summary>
		/// <typeparam name="T">Type the definition to be retrieved for.</typeparam>
		/// <returns>AssemblyDefiniton containing the specified type.</returns>
		public static AssemblyDefinition GetAssemblyDefinition<T> ()
		{
			return GetAssemblyDefinition (typeof (T));
		}			
						
		/// Gets AssemblyDefiniton containing the specified type.
		/// </summary>
		/// <param name="type">Type the definition to be retrieved for.</param>
		/// <returns>AssemblyDefiniton containing the specified type.</returns>
		public static AssemblyDefinition GetAssemblyDefinition (Type type)
		{
			return AssemblyCache.GetDefinition (type.Assembly);
		}			
			
		/// <summary>
		/// Gets TypeDefinition for the specified type.
		/// </summary>
		/// <typeparam name="T">Type to be retrieved.</typeparam>
		/// <returns>TypeDefinition associated with specified type.</returns>
		public static TypeDefinition GetTypeDefinition<T> ()
		{
			return GetTypeDefinition (typeof (T));
		}		
						
		/// <summary>
		/// Gets TypeDefinition for the specified type.
		/// </summary>
		/// <param name="type">Type name to be retrieved.</param>
		/// <returns>TypeDefinition associated with specified type.</returns>
		public static TypeDefinition GetTypeDefinition (Type type)
		{
			return GetAssemblyDefinition (type)
			         .MainModule.GetType (GetCecilTypeName (type));
		}				
		
		/// <summary>
		/// Gets TypeDefinition for the specified type.
		/// </summary>
		/// <param name="typeName">Type name to be retrieved.</param>
		/// <param name="assembly">Assembly to look for the type in.</param>
		/// <returns>TypeDefinition associated with specified type name.</returns>
		public static TypeDefinition GetTypeDefinition (Assembly assembly, string typeName)
		{
			return AssemblyCache.GetDefinition (assembly)
			         .MainModule.GetType (GetCecilNestedTypeName (typeName));
			// well, we don't really need to check if type is nested in this case
		}	
	}
}
