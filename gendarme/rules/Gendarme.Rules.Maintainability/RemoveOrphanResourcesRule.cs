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
using System.Text;
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
					string resourceName = (string)entry.Key;
					object satelliteValue = entry.Value;
					object mainValue = mainResourceSet.GetObject (resourceName);
					if (mainValue == null) {
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
						int mainNumber = GetNumberOfExpectedParameters ((string)mainValue);
						int satelliteNumber = GetNumberOfExpectedParameters ((string)satelliteValue);
						if (mainNumber != satelliteNumber)
							Runner.Report (satelliteAssembly, Severity.High, Confidence.Normal,
								String.Format ("The string resource '{0}' in the file '{1}' as '{2}' parameters in the satellite assembly but '{3}' in the main assembly", resourceName, satelliteResource.Name, satelliteNumber, mainNumber));
					}
				}
			}
		}

		//TODO: share it whith ProvideCorrectArgumentsToFormattingMethodsRule
		private int GetNumberOfExpectedParameters (string format)
		{
			if(format == null)
				throw new ArgumentNullException("format");

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

				while(index++ < format.Length) {
					char current = format [index];
					if (!char.IsDigit (current))
						break;
					value.Append (current);
				}

				if (index == format.Length)
					break; // Incorrect format

				int intValue;
				if (!int.TryParse (value.ToString(), out intValue))
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

			DirectoryInfo directory = mainAssembly.MainModule.Image.FileInformation.Directory;
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

				AssemblyDefinition assembly = AssemblyFactory.GetAssembly (files [0].FullName);
				satellitesAssemblies.Add (assembly);
			}

			return satellitesAssemblies;
		}
	}

}
