//
// Gendarme.Rules.Maintainability.RemoveOrphanResourcesRule
//
// Authors:
//	Antoine Vandecreme <ant.vand@gmail.com>
//
// Copyright (C) 2010 Antoine Vandecreme
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Resources;
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework;

namespace Gendarme.Rules.Maintainability {

        /// <summary>
        /// A satellite assembly have a resource which does not exist in the main assembly.
        /// The resource should be removed from the satellite assembly.
        /// </summary>
        /// <remarks>
        /// The satellites assemblies are searched in the subdirectories of the main assembly location.
        /// </remarks>

        [Problem ("A satellite assembly have a resource which does not exist in the main assembly.")]
        [Solution ("Remove the resource in the satellite assemby.")]
        public sealed class RemoveOrphanResourcesRule : Rule, IAssemblyRule {

                public RuleResult CheckAssembly (AssemblyDefinition assembly)
                {
                        // If the analyzed assembly is a satellite assembly, does not apply
                        if (assembly.Name.Name.Contains (".resources"))
                                return RuleResult.DoesNotApply;

                        IList<AssemblyDefinition> satellites = GetSatellitesAssemblies (assembly);

                        foreach (AssemblyDefinition satellite in satellites)
                                CheckSatelliteAssembly (assembly, satellite);

                        return RuleResult.Success;
                }

                private void CheckSatelliteAssembly (AssemblyDefinition mainAssembly, AssemblyDefinition satellite)
                {
                        ResourceCollection mainResources = mainAssembly.MainModule.Resources;
                        Dictionary<string, EmbeddedResource> mainResourcesNames = new Dictionary<string, EmbeddedResource> (mainResources.Count);
                        foreach (EmbeddedResource resource in mainResources) {
                                string fullName = resource.Name;
                                string name = fullName.Remove (fullName.IndexOf (".resources"));

                                mainResourcesNames.Add (name, resource);
                        }

                        ResourceCollection satellitesResources = satellite.MainModule.Resources;
                        foreach (EmbeddedResource resource in satellitesResources) {
                                string fullName = resource.Name;
                                string nameWithCulture = fullName.Remove (fullName.IndexOf (".resources"));
                                string name = nameWithCulture.Remove (nameWithCulture.LastIndexOf ('.'));

                                EmbeddedResource mainResource;
                                if (!mainResourcesNames.TryGetValue (name, out mainResource)) {
                                        Runner.Report (satellite, Severity.Low, Confidence.High,
                                                String.Format ("The resource file '{0}' exist in the satellite assembly but not in the main assembly", fullName));
                                        continue;
                                }

                                CheckSatelliteResource (mainResource, resource, satellite);
                        }
                }

                private void CheckSatelliteResource (EmbeddedResource mainResource, EmbeddedResource satelliteResource, AssemblyDefinition satelliteAssembly)
                {
                        using (MemoryStream mainMs = new MemoryStream (mainResource.Data))
                        using (ResourceSet mainResourceSet = new ResourceSet (mainMs))
                        using (MemoryStream ms = new MemoryStream (satelliteResource.Data))
                        using (ResourceSet resourceSet = new ResourceSet (ms)) {
                                foreach (DictionaryEntry entry in resourceSet) {
                                        object mainValue = mainResourceSet.GetObject ((string) entry.Key);
                                        if (mainValue == null && entry.Value != null)
                                                Runner.Report (satelliteAssembly, Severity.Low, Confidence.High,
                                                        String.Format ("The resource '{0}' in the file '{1}' exist in the satellite assembly but not in the main assembly", entry.Key, satelliteResource.Name));
                                }
                        }
                }

                private static IList<AssemblyDefinition> GetSatellitesAssemblies (AssemblyDefinition mainAssembly)
                {
                        List<AssemblyDefinition> satellitesAssemblies = new List<AssemblyDefinition> ();

                        string satellitesName = mainAssembly.Name.Name + ".resources.dll";

                        DirectoryInfo directory = mainAssembly.MainModule.Image.FileInformation.Directory;
                        DirectoryInfo [] subDirectories = directory.GetDirectories ();
                        foreach (DirectoryInfo dir in subDirectories) {
                                FileInfo [] files = dir.GetFiles (satellitesName, SearchOption.TopDirectoryOnly);
                                if (files.Length == 0)
                                        continue;

                                AssemblyDefinition assembly = AssemblyFactory.GetAssembly (files [0].FullName);
                                satellitesAssemblies.Add (assembly);
                        }

                        return satellitesAssemblies;
                }
        }

}
