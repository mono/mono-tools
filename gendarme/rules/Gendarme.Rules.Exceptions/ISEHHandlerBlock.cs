using System;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// Represents a block of code that handles an exception
	/// </summary>
	public interface ISEHHandlerBlock {

		/// <summary>
		/// Gets the first instruction in the handler
		/// </summary>
		/// <value>
		/// An instance of <see cref="Instruction"/> representing the first
		/// instruction in the handler.
		/// </value>
		Instruction Start {
			get;
		}

		/// <summary>
		/// Gest the last instruction in the handler
		/// </summary>
		/// <value>
		/// An instance of <see cref="Instruction"/> representing the last
		/// instruction in the handler.
		/// </value>
		Instruction End {
			get;
		}

		/// <summary>
		/// Gets the type of this handler
		/// </summary>
		/// <value>
		/// An value from the enumeration <see cref="SEHHandlerType"/>
		/// </value>
		SEHHandlerType Type {
			get;
		}
	}

}
