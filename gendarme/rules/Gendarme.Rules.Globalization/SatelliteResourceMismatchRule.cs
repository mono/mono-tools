//
// Gendarme.Rules.Globalization.SatelliteResourceMismatchRule
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
using System.Text;
using System.Resources;
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Collections.Generic;

using Gendarme.Framework;

namespace Gendarme.Rules.Globalization {

	/// <summary>
	/// A satellite assembly have a resource which does not match with a main assembly resource.
	/// Either :
	///	* The resource doesn't exist in the main assembly and should be removed from the satellite assembly.
	///	* The resource is not of the same type in the main and satellite assembly. The satellite one should be fixed.
	///	* The satellite string resource does not have the same string.Format parameters than the main assembly. The satellite one should be fixed.
	/// </summary>
	/// <remarks>
	/// The satellites assemblies are searched in the subdirectories of the main assembly location.
	/// </remarks>

	[Problem ("A satellite assembly have a resource which does not match correctly with a main assembly resource.")]
	[Solution ("Remove or fix the resource in the satellite assemby.")]
	public sealed class SatelliteResourceMismatchRule : Rule, IAssemblyRule {

		private const string resXResourcesExtension = ".resources";
		private AssemblyResourceCache mainAssemblyResourceCache;

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			// If the analyzed assembly is a satellite assembly, does not apply
			if (!string.IsNullOrEmpty(assembly.Name.Culture))
				return RuleResult.DoesNotApply;

			// Reset caches
			mainAssemblyResourceCache = new AssemblyResourceCache (assembly);

			IList<AssemblyDefinition> satellites = GetSatellitesAssemblies (assembly);

			foreach (AssemblyDefinition satellite in satellites)
				CheckSatelliteAssembly (satellite);

			return RuleResult.Success;
		}

		private void CheckSatelliteAssembly (AssemblyDefinition satellite)
		{
			string culture = satellite.Name.Culture;
			Collection<Resource> satellitesResources = satellite.MainModule.Resources;
			foreach (EmbeddedResource resource in satellitesResources) {
				EmbeddedResource mainResource;
				string resourceName = GetNameInSatellite (resource, culture);
				if (!mainAssemblyResourceCache.TryGetMainResourceFile (resourceName, out mainResource)) {
					Runner.Report (satellite, Severity.Low, Confidence.High,
						String.Format ("The resource file '{0}' exist in the satellite assembly but not in the main assembly", resource.Name));
					continue;
				}

				if (!IsResXResources (resource))
					continue;

				CheckSatelliteResource (mainResource, resource, satellite);
			}
		}

		private void CheckSatelliteResource (EmbeddedResource mainResource, EmbeddedResource satelliteResource, IMetadataTokenProvider satelliteAssembly)
		{
			using (Stream resourceStream = satelliteResource.GetResourceStream ())
			using (ResourceSet resourceSet = new ResourceSet (resourceStream)) {
				foreach (DictionaryEntry entry in resourceSet) {
					string resourceName = (string) entry.Key;
					object satelliteValue = entry.Value;
					object mainValue;
					if (!mainAssemblyResourceCache.TryGetMainResource (mainResource, resourceName, out mainValue)) {
						Runner.Report (satelliteAssembly, Severity.Low, Confidence.High,
							String.Format ("The resource '{0}' in the file '{1}' exist in the satellite assembly but not in the main assembly", resourceName, satelliteResource.Name));
						continue;
					}

					Type satelliteType = satelliteValue.GetType ();
					Type mainType = mainValue.GetType ();
					if (!satelliteType.Equals (mainType)) {
						Runner.Report (satelliteAssembly, Severity.High, Confidence.High,
							String.Format ("The resource '{0}' in the file '{1}' is of type '{2}' in the satellite assembly but of type '{3}' in the main assembly", resourceName, satelliteResource.Name, satelliteType, mainType));
						continue;
					}

					if (satelliteType.Equals (typeof (string))) {
						int mainNumber = GetNumberOfExpectedParameters ((string) mainValue);
						int satelliteNumber = GetNumberOfExpectedParameters ((string) satelliteValue);
						if (mainNumber != satelliteNumber)
							Runner.Report (satelliteAssembly, Severity.High, Confidence.Normal,
								String.Format ("The string resource '{0}' in the file '{1}' as '{2}' parameters in the satellite assembly but '{3}' in the main assembly", resourceName, satelliteResource.Name, satelliteNumber, mainNumber));
					}
				}
			}
		}

		private static int GetNumberOfExpectedParameters (string format)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			int result = 0; // the number of expected parameters is the biggest value between {} + 1

			// if last character is { then there's no digit after it
			for (int index = 0; index < format.Length - 1; index++) {
				if (format [index] != '{')
					continue;

				char nextChar = format [index + 1];
				if (nextChar == '{') {
					index++; // skip special {{
					continue;
				}

				if (!char.IsDigit (nextChar))
					continue;

				StringBuilder value = new StringBuilder (nextChar.ToString ());
				index++; // next char is already added to value

				while (index++ < format.Length) {
					char current = format [index];
					if (!char.IsDigit (current))
						break;
					value.Append (current);
				}

				if (index == format.Length)
					break; // Incorrect format

				int intValue;
				if (!int.TryParse (value.ToString (), out intValue))
					continue;

				int parameterNumber = intValue + 1; // The indexes start at 0 !
				if (parameterNumber > result)
					result = parameterNumber;
			}

			return result;
		}

		private static IList<AssemblyDefinition> GetSatellitesAssemblies (AssemblyDefinition mainAssembly)
		{
			List<AssemblyDefinition> satellitesAssemblies = new List<AssemblyDefinition> ();

			string satellitesName = mainAssembly.Name.Name + ".resources.dll";

			DirectoryInfo directory = new DirectoryInfo (Path.GetDirectoryName (
				mainAssembly.MainModule.FullyQualifiedName));
			DirectoryInfo [] subDirectories = directory.GetDirectories ();
			foreach (DirectoryInfo dir in subDirectories) {
				FileInfo [] files;
				try {
					files = dir.GetFiles (satellitesName, SearchOption.TopDirectoryOnly);
				} catch (UnauthorizedAccessException) {
					continue; // If we don't have access to directory ignore it
				}
				if (files.Length == 0)
					continue;

				AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (files [0].FullName);
				satellitesAssemblies.Add (assembly);
			}

			return satellitesAssemblies;
		}

		// In satellites assemblies, the resource file name sometimes contains the culture 
		// some times do not
		// This method always returns the name without the culture to be able to match with the main
		// assembly resource
		private static string GetNameInSatellite (Resource resource, string culture)
		{
			string name = resource.Name;
			string nameWithoutExtension = Path.GetFileNameWithoutExtension (name);

			string cultureExtension = "." + culture;

			if (!nameWithoutExtension.EndsWith (cultureExtension))
				return name;

			string nameWithoutCulture = Path.GetFileNameWithoutExtension (nameWithoutExtension);
			return nameWithoutCulture + Path.GetExtension (name);
		}

		private static bool IsResXResources (Resource resource)
		{
			return resource.Name.EndsWith (resXResourcesExtension);
		}

		private sealed class AssemblyResourceCache {
			private AssemblyDefinition assembly;
			private Dictionary<string, EmbeddedResource> files;
			private Dictionary<EmbeddedResource, Dictionary<string, object>> values;

			public AssemblyResourceCache (AssemblyDefinition assemblyDefinition)
			{
				assembly = assemblyDefinition;
			}

			public bool TryGetMainResourceFile (string resourceFileName, out EmbeddedResource embeddedResource)
			{
				if (files == null) {
					// Build cache of resources files
					files = new Dictionary<string, EmbeddedResource> ();

					Collection<Resource> mainResources = assembly.MainModule.Resources;
					foreach (EmbeddedResource resource in mainResources)
						files.Add (resource.Name, resource);
				}
				return files.TryGetValue (resourceFileName, out embeddedResource);
			}

			public bool TryGetMainResource (EmbeddedResource embeddedResource, string resourceName, out object value)
			{
				value = null;

				if (values == null) {
					// Build cache of resources values
					values = new Dictionary<EmbeddedResource, Dictionary<string, object>> ();
				}

				Dictionary<string, object> fileResources;
				if (!values.TryGetValue (embeddedResource, out fileResources)) {
					fileResources = new Dictionary<string, object> ();
					using (Stream resourceStream = embeddedResource.GetResourceStream ())
					using (ResourceSet resourceSet = new ResourceSet (resourceStream)) {
						foreach (DictionaryEntry entry in resourceSet)
							fileResources.Add ((string) entry.Key, entry.Value);
					}
					values.Add (embeddedResource, fileResources);
				}

				return fileResources.TryGetValue (resourceName, out value);
			}
		}
	}

}
