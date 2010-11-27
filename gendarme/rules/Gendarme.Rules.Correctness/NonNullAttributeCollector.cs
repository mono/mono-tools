/*
 * NonNullAttributeCollector.cs: collects and caches non-null attributes.
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
using System.Collections;
using System.IO;

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

public class NonNullAttributeCollector {
    [NonNull] private Hashtable nonNullMethods;
    [NonNull] private Hashtable nonNullFields;
    [NonNull] private Hashtable nonNullParams;

    public NonNullAttributeCollector()
    {
        this.nonNullMethods = new Hashtable();
        this.nonNullFields = new Hashtable();
        this.nonNullParams = new Hashtable();

    }

    public void AddAssembly([NonNull] AssemblyDefinition assembly)
    {
        foreach(ModuleDefinition module in assembly.Modules) {
            foreach(TypeDefinition type in module.GetAllTypes ()) {
                foreach(MethodDefinition method in type.Methods) {
                    if(DefHasNonNullAttribute(method)) {
                        nonNullMethods.Add(method.ToString(), method);
                    }
                    foreach(ParameterDefinition param in method.Parameters) {
                        if(DefHasNonNullAttribute(param)) {
                            nonNullParams.Add(method.ToString() + "/"
                                    + param.GetSequence (), param);
                        }
                    }
                }
                foreach(FieldDefinition field in type.Fields) {
                    if(DefHasNonNullAttribute(field)) {
                        nonNullFields.Add(field.ToString(), field);
                    }
                }
            }
        }
    }

    public void AddList(string fileName)
    {
        StreamReader r = new StreamReader(fileName);
        string line;
        for(line = r.ReadLine(); line != null; line = r.ReadLine()) {
            if(line.IndexOf("/") != -1) {
                nonNullParams.Add(line, null);
            } else if(line.IndexOf("(") != -1) {
                nonNullMethods.Add(line, null);
            } else {
                nonNullFields.Add(line, null);
            }
        }
    }

    public bool HasNonNullAttribute([NonNull] IMethodSignature msig)
    {
        if(nonNullMethods.Contains(msig.ToString()))
            return true;
        return false;
    }

    public bool HasNonNullAttribute([NonNull] IMethodSignature msig,
            [NonNull] ParameterDefinition param)
    {
        if(nonNullParams.Contains(msig.ToString() + "/" + param.GetSequence ()))
            return true;
        return false;
    }

    public bool HasNonNullAttribute([NonNull] FieldReference field)
    {
        if(nonNullFields.Contains(field.ToString()))
            return true;
        return false;
    }

    private static bool DefHasNonNullAttribute(
            [NonNull] ICustomAttributeProvider provider)
    {
        string ctorName = "System.Void NonNullAttribute::.ctor()";
        foreach(CustomAttribute attrib in provider.CustomAttributes)
            if(attrib.Constructor.ToString().Equals(ctorName))
                return true;
        return false;
    }
}

}
