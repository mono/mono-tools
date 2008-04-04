//
// Test.Rules.Helpers.DefinitionLoader
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
			if (type.IsNested) 
				return GetCecilNestedTypeName (type.FullName);
	
			return type.FullName;
		}
		
		
		/// <summary>
		/// Gets a MethodDefinition for the delegate.
		/// </summary>
		/// <param name="methodDelegate">Delegate to the method to load.</param>
		/// <returns>MethodDefinition associated with specified method.</returns>
		public static MethodDefinition GetMethodDefinition (Delegate methodDelegate)
		{
			// get method the delegate is pointing at
			MethodInfo reflectionMethod = methodDelegate.Method;

			// obtain type definition for the delegate
			TypeDefinition type = GetTypeDefinition (reflectionMethod.DeclaringType);

			// ensure such type exists
			if (type == null)
				throw new ArgumentException (string.Format ("Could not load {0} type.", type.FullName));
			
			// look for method definition
			string signature = reflectionMethod.ToString ();

			MethodDefinition matchingMethod = null;
			foreach (MethodDefinition method in type.Methods.GetMethod (reflectionMethod.Name)) {
				if (signature == method.ToString ())
					matchingMethod = method;
				
				if (matchingMethod != null)
					break;
			}
			
			if (matchingMethod == null)
				throw new ArgumentException (string.Format ("Method {0} was not found in {1} class.", reflectionMethod.Name, type.FullName));

			return matchingMethod;
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
			// find method definition
			MethodDefinition matchingMethod = null;
			MethodDefinition [] methods = type.Methods.GetMethod (methodName);
			
			if (methods.Length == 0)
				throw new ArgumentException (string.Format ("Method {0} was not found in class {1}.", methodName, type.FullName));

			if (methodParameters != null) {
				matchingMethod = type.Methods.GetMethod (methodName, methodParameters);
			} else {
				// another possible case is when null is passed - in case there is only one overload
				if (methods.Length == 1) // only one method with specified name - use it
					matchingMethod = methods [0];
				
				else // amigious (multiple overloads, parameters not specified)
					throw new ArgumentException (string.Format ("Name {0} is ambigious between {1} overloads. You should also pass parameter types in this case.", methodName, methods.Length));
			}
			
			// check if method was found
			if (matchingMethod == null)
				throw new ArgumentException (string.Format ("Method {0} was not found in class {1}.", methodName, type.FullName));

			return matchingMethod;
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
			         .MainModule.Types [GetCecilTypeName (type)];
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
			         .MainModule.Types [GetCecilNestedTypeName (typeName)];
			// well, we don't really need to check if type is nested in this case
		}	
	}
}
