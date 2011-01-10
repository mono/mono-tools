//
// AssemblyStaticInfo.cs for Gendarme
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2011 Novell, Inc (http://www.novell.com)
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyTitle ("Gendarme")]
[assembly: AssemblyDescription ("Rule-based assembly analyzer")]
[assembly: AssemblyCopyright ("Copyright (C) 2005-2011 Novell, Inc. and contributors")]
[assembly: AssemblyCompany ("Novell, Inc.")]

[assembly: PermissionSet (SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: CLSCompliant (false)]
[assembly: ComVisible (false)]

#if RELEASE
[assembly: AssemblyVersion ("2.10.0.0")]
#endif
