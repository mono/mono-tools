//
// Gendarme.Framework.Helpers.PrimitiveReferences
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using Mono.Cecil;
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Provide an easy way to get TypeReference to primitive types without having
	/// direct access to the mscorlib.dll assembly (any ModuleDefinition will do).
	/// </summary>
	public static class PrimitiveReferences {

		// To avoid memory allocations this is done in three stages
		// 1. the references are cached at the first call and reused afterward (very cheap)
		// 2. we look if the module already have the TypeReference (cheap)
		// 3. at last we go thru the full Import (costly)

		// TODO - extend to all primivites
		static TypeReference single_ref;
		static TypeReference double_ref;

		static TypeReference GetReference (Type type, IMetadataTokenProvider metadata)
		{
			ModuleDefinition module = metadata.GetAssembly ().MainModule;
			TypeReference tr;
			if (!module.TryGetTypeReference (type.FullName, out tr))
				tr = module.Import (type);
			return tr;
		}

		static public TypeReference GetDouble (IMetadataTokenProvider metadata)
		{
			if (double_ref == null)
				double_ref = GetReference (typeof (double), metadata);
			return double_ref;
		}

		static public TypeReference GetSingle (IMetadataTokenProvider metadata)
		{
			if (single_ref == null)
				single_ref = GetReference (typeof (float), metadata);
			return single_ref;
		}
	}
}
