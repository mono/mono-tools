/*
 * Nullity.cs: the possibilities for the known state of a particular
 * object.
 *
 * Authors:
 *   Aaron Tomb <atomb@soe.ucsc.edu>
 *
 * Copyright (c) 2005 Aaron Tomb and the contributors listed
 * in the ChangeLog.
 *
 * This is free software, distributed under the MIT/X11 license.
 * See the included LICENSE.MIT file for details.
 **********************************************************************/

namespace Gendarme.Rules.Correctness {

	public enum Nullity {
	    Unused  = 0,
	    Null    = 1,
	    NonNull = 2,
	    Unknown = 3,
	}
}
