/*
 * NonNullAttribute.cs: a custom attribute that can be placed on a
 * method, field, property, or parameter, to indicate that it should
 * never be null.
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

using System;

namespace Gendarme.Rules.Correctness {

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
	public sealed class NonNullAttribute : Attribute {

		public NonNullAttribute ()
		{
		}
	}
}
