using System;

namespace Gendarme.Rules.Exceptions {
	/// <summary>
	/// Enumerates the types of SEH handler blocks
	/// </summary>
	public enum SEHHandlerType {
		/// <summary>
		/// An SEH catch block, invoked if an exception is thrown in the
		/// guarded block that matches or descends from a specified class
		/// </summary>
		Catch,

		/// <summary>
		/// An SEH fault block, invoked if any exception is thrown in
		/// the guarded block
		/// </summary>
		Fault,

		/// <summary>
		/// An SEH filter block, invoked if any exception is thrown in
		/// the guarded block
		/// </summary>
		Filter,

		/// <summary>
		/// An SEH finally block, invoked regardless of whether or not
		/// an exception is thrown in the guarded block
		/// </summary>
		Finally,
	}
}
