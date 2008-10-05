//
// Test.Rules.Definitions.SimpleTypes
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, includin
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
using System.CodeDom.Compiler;

using Test.Rules.Helpers;

using Mono.Cecil;

namespace Test.Rules.Definitions {
	
	/// <summary>
	/// Class that holds references to some widely-used types while testing.
	/// </summary>
	public static class SimpleTypes {
		
		/// <summary>
		/// A class to be used as the common one.
		/// </summary>
		class DeepThought {
			public DeepThought ()
			{
			}
			
			public int Answer {
				get { return 42; }
			}
		}
		
		/// <summary>
		/// A structure to be used as the common one.
		/// </summary>
		struct Album {
			public string Name;
			public int Year;
			public int TrackCount;
		}
	
		/// <summary>
		/// An enum to be used as the common one.
		/// </summary>
		enum Songs {
			IAmTheWalrus,
			AllYouNeedIsLove,
			AcrossTheUniverse
		}
	
		/// <summary>
		/// An interface to be used as the common one.
		/// </summary>
		interface ILoveBeatles {
			void Listen (string songName);
		}

		/// <summary>
		/// A delegate to be used as the common one.
		/// </summary>
		delegate bool Filter (string s);

		/// <summary>
		/// A method decorated with the [GeneratedCode] attribute
		/// </summary>
		[GeneratedCode ("Gendarme", "2.2")]
		class Generated {
			public string GetIt ()
			{
				return String.Empty;
			}
		}

		/// <value>
		/// A simple class definition.
		/// </value>
		public static TypeDefinition Class {
			get { return DefinitionLoader.GetTypeDefinition<DeepThought> (); }
		}
		
		/// <value>
		/// A simple interface definition.
		/// </value>
		public static TypeDefinition Interface {
			get { return DefinitionLoader.GetTypeDefinition<ILoveBeatles> (); }
		}
		
		/// <value>
		/// A simple enumeration definition.
		/// </value>
		public static TypeDefinition Enum {
			get { return DefinitionLoader.GetTypeDefinition<Songs> (); }
		}
		
		/// <value>
		/// A simple structure definition.
		/// </value>
		public static TypeDefinition Structure {
			get { return DefinitionLoader.GetTypeDefinition<Album> (); }
		}

		/// <value>
		/// A simple deletage definition.
		/// </value>
		public static TypeDefinition Delegate {
			get { return DefinitionLoader.GetTypeDefinition<Filter> (); }
		}

		/// <value>
		/// A simple generated type.
		/// </value>
		public static TypeDefinition GeneratedType {
			get { return DefinitionLoader.GetTypeDefinition<Generated> (); }
		}
	}
}
