using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// A guarded block of IL instructions, along with all ISEHHandlerBlocks
	/// for this guarded block
	/// </summary>
	public interface ISEHGuardedBlock {

		/// <summary>
		/// The first guarded instruction
		/// </summary>
		/// <value>
		/// An instance of <see cref="Instruction"/> representing the first
		/// guarded instruction.
		/// </value>
		Instruction Start {
			get;
		}

		/// <summary>
		/// The last guarded instruction
		/// </summary>
		/// <value>
		/// An instance of <see cref="Instruction"/> representing the last
		/// guarded instruction.
		/// </value>
		Instruction End {
			get;
		}

		/// <summary>
		/// All handler blocks for the guarded instructions
		/// </summary>
		/// <value>
		/// An array of <see cref="ISEHHandlerBlock"/> instances representing
		/// the handler blocks for the guarded instructions
		/// </value>
		/// <remarks>
		/// This array will have one or more entries, depending on the
		/// type of SEH handler.  For filter, fault, and finally handlers, this
		/// array will contain exactly one entry.  For catch handlers, this
		/// array will contain one or more entries.
		/// </remarks>
		ISEHHandlerBlock[] SEHHandlerBlocks {
			get;
		}
	}
}
