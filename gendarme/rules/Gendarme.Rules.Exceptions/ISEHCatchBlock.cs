using System;
using Mono.Cecil;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// Represents a catch type handler block
	/// </summary>
	public interface ISEHCatchBlock : ISEHHandlerBlock {

		/// <summary>
		/// Gets the type of <see cref="Exception"/> handled by this
		/// catch block
		/// </summary>
		/// <value>
		/// An instance of <see cref="TypeReference"/> referencing the type
		/// of <see cref="Exception"/> this catch block processes
		/// </value>
		TypeReference ExceptionType {
			get;
		}
	}
}
