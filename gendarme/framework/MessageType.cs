/*
 * Message.cs: an error or warning message produced by a checker when it
 * discovers a problem.
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

namespace Gendarme.Framework {

	public enum MessageType {
		Error,
		Warning
	}
}
