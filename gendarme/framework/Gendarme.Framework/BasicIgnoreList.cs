//
// BasicIgnoreList
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Metadata;

using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	/// <summary>
	/// Basic ignore list implementation.
	/// </summary>
	public class BasicIgnoreList : IIgnoreList {

		private Dictionary<string, HashSet<IMetadataTokenProvider>> ignore;

		// note: we should keep statistics here
		// e.g. # of times a rule is ignored because it's inactive
		// e.g. # of times a rule is ignored because of users directives

		public BasicIgnoreList (IRunner runner)
		{
			Runner = runner;
			ignore = new Dictionary<string, HashSet<IMetadataTokenProvider>> ();
		}

		public IRunner Runner {
			get;
			private set;
		}

		public void Add (string rule, IMetadataTokenProvider metadata)
		{
			HashSet<IMetadataTokenProvider> list;
			if (!ignore.TryGetValue (rule, out list)) {
				list = new HashSet<IMetadataTokenProvider> ();
				ignore.Add (rule, list);
			}
			list.Add (metadata);
		}

		// AssemblyDefinition						AttributeTargets.Assembly
		//	ModuleDefinition					AttributeTargets.Module
		//		TypeDefinition					AttributeTargets.Class | Delegate | Enum | Interface | Struct
		//			EventDefinition				AttributeTargets.Event
		//			FieldDefinition				AttributeTargets.Field
		//			GenericParameterDefinition		AttributeTargets.GenericParameter
		//			PropertyDefinition			AttributeTargets.Property
		//			MethodDefinition			AttributeTargets.Constructor | Method
		//				GenericParameterDefinition	AttributeTargets.GenericParameter
		//				ParameterDefinition		AttributeTargets.Parameter
		//				MethodReturnType		AttributeTargets.ReturnValue
		// NamespaceDefinition						special case
		public bool IsIgnored (IRule rule, IMetadataTokenProvider metadata)
		{
			// Note that the Runner tearing_down code may call us with nulls.
			if (metadata == null)
				return false;

			if ((rule == null) || !rule.Active)
				return true;

			HashSet<IMetadataTokenProvider> list;
			if (!ignore.TryGetValue (rule.FullName, out list))
				return false; // nothing is ignored for this rule

			return IsIgnored (list, metadata);
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, IMetadataTokenProvider metadata)
		{
			MetadataToken token = metadata.MetadataToken;
			switch (token.TokenType) {
			case TokenType.Assembly:
			// NamespaceDefinition is a Gendarme "extention", i.e. not real metadata, but we can ignore defects on them
			case NamespaceDefinition.NamespaceTokenType:
				// no parent to check
				return list.Contains (metadata);
			case TokenType.Module:
				// Module == 0, so we need to handle MetadataToken.Zero here
				if (token.RID == 0) {
					// if we don't have a valid token then we take the slow path
					return IsIgnoredUsingCasts (list, metadata);
				} else {
					return IsIgnored (list, metadata as ModuleDefinition);
				}
			case TokenType.GenericParam:
				return IsIgnored (list, metadata as GenericParameter);
			case TokenType.TypeRef:
			case TokenType.TypeDef:
				return IsIgnored (list, metadata as TypeReference);
			case TokenType.Method:
				return IsIgnored (list, metadata as MethodReference);
			case TokenType.Event:
				return IsIgnored (list, metadata as EventDefinition);
			case TokenType.Field:
				return IsIgnored (list, metadata as FieldDefinition);
			case TokenType.Property:
				return IsIgnored (list, metadata as PropertyDefinition);
			case TokenType.Param:
				ParameterDefinition parameter = metadata as ParameterDefinition;
				if (parameter == null) // return type
					return IsIgnoredUsingCasts (list, metadata);

				return IsIgnored (list, parameter);
			default:
				return IsIgnoredUsingCasts (list, metadata);
			}
		}

		static bool IsIgnoredUsingCasts (ICollection<IMetadataTokenProvider> list, IMetadataTokenProvider metadata)
		{
			MethodReturnType mrt = (metadata as MethodReturnType);
			if (mrt != null)
				return IsIgnored (list, mrt);
			// well-known types...
			AssemblyDefinition ad = (metadata as AssemblyDefinition);
			if (ad != null)
				return list.Contains (metadata);
			ModuleDefinition md = (metadata as ModuleDefinition);
			if (md != null)
				return IsIgnored (list, md);
			GenericParameter gp = (metadata as GenericParameter); // needs to be before TypeReference
			if (gp != null)
				return IsIgnored (list, gp);
			TypeReference tr = (metadata as TypeReference);
			if (tr != null)
				return IsIgnored (list, tr);
			MethodReference mr = (metadata as MethodReference);
			if (mr != null)
				return IsIgnored (list, mr);
			EventDefinition ed = (metadata as EventDefinition);
			if (ed != null)
				return IsIgnored (list, ed);
			FieldDefinition fd = (metadata as FieldDefinition);
			if (fd != null)
				return IsIgnored (list, fd);
			PropertyDefinition pd = (metadata as PropertyDefinition);
			if (pd != null)
				return IsIgnored (list, pd);
			ParameterDefinition paramd = (metadata as ParameterDefinition);
			if (paramd != null)
				return IsIgnored (list, paramd);
			return false;
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, ModuleDefinition module)
		{
			return (list.Contains (module) || IsIgnored (list, module.Assembly));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, TypeReference type)
		{
			return (list.Contains (type) || IsIgnored (list, type.Module) || 
				IsIgnored (list, NamespaceDefinition.GetDefinition (type.Namespace)));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, EventDefinition evnt)
		{
			return (list.Contains (evnt) || IsIgnored (list, evnt.DeclaringType));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, FieldDefinition field)
		{
			return (list.Contains (field) || IsIgnored (list, field.DeclaringType));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, PropertyDefinition property)
		{
			return (list.Contains (property) || IsIgnored (list, property.DeclaringType));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, MemberReference member)
		{
			return (list.Contains (member) || IsIgnored (list, member.DeclaringType));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, ParameterDefinition parameter)
		{
			return (list.Contains (parameter) || IsIgnored (list, parameter.Method));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, GenericParameter parameter)
		{
			return (list.Contains (parameter) || IsIgnored (list, parameter.Owner));
		}

		static bool IsIgnored (ICollection<IMetadataTokenProvider> list, MethodReturnType returnType)
		{
			return (list.Contains (returnType) || IsIgnored (list, returnType.Method));
		}
	}
}
