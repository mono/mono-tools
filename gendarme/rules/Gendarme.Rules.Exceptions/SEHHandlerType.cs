using System;

namespace Gendarme.Rules.Exceptions {

	// Enumerates the types of SEH handler blocks
	public enum SEHHandlerType {
		// An SEH catch block, invoked if an exception is thrown in the
		// guarded block that matches or descends from a specified class
		Catch,

		// An SEH fault block, invoked if any exception is thrown in the guarded block
		Fault,

		// An SEH filter block, invoked if any exception is thrown in the guarded block
		Filter,

		// An SEH finally block, invoked regardless of whether or not
		// an exception is thrown in the guarded block
		Finally,
	}
}
