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

	public class Message {
	
		private string text;
		private Location location;
		private MessageType type;

		public Message (string text, Location location, MessageType type)
		{
			this.text = text;
			this.location = location;
			this.type = type;
		}

		public string Text {
			get { return text; }
		}

		public Location Location {
			get { return location; }
		}

		public MessageType Type {
			get { return type; }
		}

		public override string ToString ()
		{
			if (location != null)
				return location.ToString () + ": " + text;
			else
				return text;
		}
	}
}
