using System;
using System.Web.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mono.Website {
	public class Plugin {

		static string PluginsDefLocation = WebConfigurationManager.AppSettings["Plugins"]; 

		public enum PluginContent {
	        Header,
    	    Footer,
        	Css,
        	Javascript
		}

		//generates a list of files of a certain type in the plugins we are including, 
		//and spits out the necessary text to add to index.aspx
		public static string GetPluginContent (PluginContent type) 
		{
			var plugins_list_from_def = ParseExternalDefinition(PluginsDefLocation);
	        var paths_to_files = GetFilesTypeX(type, plugins_list_from_def);
	        return GetPluginContent (type, paths_to_files);
		}

		//Add the actual HTML to include either the reference or the content in index.aspx, for each plugin mentioned
		//in the .def
		static string GetPluginContent (PluginContent type, string[] paths_to_files)
		{
    	    if (type == PluginContent.Javascript) {
    	        paths_to_files = Array.ConvertAll(paths_to_files, path => 
    	            								string.Format("{1}script type='text/javascript' src='{0}'{2}{1}/script{2}", path, '<', '>'));
       			//reverse the array so we get all our js dependencies correct :)
				Array.Reverse(paths_to_files);
			} else if (type == PluginContent.Css) {
            	paths_to_files = Array.ConvertAll(paths_to_files, path => string.Format("{1}link type='text/css' rel='stylesheet' media='screen' href='{0}'{2}", path, '<', '>'));
        	} else {
                paths_to_files = Array.ConvertAll(paths_to_files, path => File.ReadAllText(path));      
        	}
	
        	var content_to_inject = String.Join(String.Empty, paths_to_files);
        	return content_to_inject;
		}

		//returns files of a certain type from ALL directories.
		static string[] GetFilesTypeX (PluginContent type, List<string> directories)
		{
	        var all_typed_files = new List<string>(); 
	        foreach(var directory in directories) {
                var files = GetFilesTypeX(type, directory);
                all_typed_files.AddRange(files);
    	    }
        	return all_typed_files.ToArray();
		}
	
		//grab files of type x from a directory
		static List<string> GetFilesTypeX (PluginContent type, string directory)
		{
        	try {
                string criteria;
                if(type == PluginContent.Javascript) {
                        criteria = "*.js";
                } else if(type == PluginContent.Css) {
                        criteria = "*.css";
                } else if(type == PluginContent.Footer) {
                        criteria = "footer.????";
                } else if (type == PluginContent.Header) {
                        criteria = "header.????";
                } else {
                        criteria = string.Empty;
                }
                var files_arr = GetFilesTypeX(directory, criteria);
                List<string> files = new List<string>(files_arr);

				var external = Directory.GetFiles(directory, "external.def");
				if (external.Any()) {
					try  {
			    		if (type == PluginContent.Css) {
							files.AddRange(ParseExternalDefinition(external[0], ".css"));
			    		} else if (type == PluginContent.Javascript) {
							files.AddRange(ParseExternalDefinition(external[0], ".js"));
			    		} else {    
			    		}
					} catch (Exception ex) {
			    		throw ex;
					}    
				}
            	return files;
        	} catch (Exception ex) {
                throw ex;
        	}
		}

		//recursively browse directories for files with a certain extension or file name
		static List<string> GetFilesTypeX (string directory, string criteria)
		{
        	try {
                var filesFound = new List<string>();
                foreach (string file in Directory.GetFiles(directory, criteria)) {
                        filesFound.Add(file);
                }
				foreach (string dir in Directory.GetDirectories(directory)) {
                	filesFound.AddRange(GetFilesTypeX(dir, criteria));
                }
                return filesFound;
        	} catch (Exception ex) {
                throw ex;
        	}
		}	

		static List<string> ParseExternalDefinition (string definitionPath, string criteria)
		{
    		//if definitionPath is undefined, or def file does not exist, don't bother
    		if (string.IsNullOrEmpty (definitionPath) || !File.Exists (definitionPath))
        	   return null;
    		// read out the file
    		var lines = File.ReadAllLines (definitionPath);
    		//build our list
    		var files = lines.Where (line => !string.IsNullOrEmpty (line) && line[0] != '#') // Take non-empty, non-comment lines
                .Where (file_path => file_path != null && file_path.Length > 2 && file_path.EndsWith(criteria)) 
                .ToList ();
    		//returns a list of directories in which to look for plugin resources
    		return files;
		}

		//eats whatever .def file you feed it
		static List<string> ParseExternalDefinition (string definitionPath)
		{
			//if definitionPath is undefined, or def file does not exist, don't bother
			if (string.IsNullOrEmpty (definitionPath) || !File.Exists (definitionPath))
				return null;
			// read out the file
			var lines = File.ReadAllLines (definitionPath);
			//build our list
			var directories = lines.Where (line => !string.IsNullOrEmpty (line) && line[0] != '#') // Take non-empty, non-comment lines
				.Where (file_path => file_path != null && file_path.Length > 2)
					.ToList ();
			//returns a list of directories in which to look for plugin resources
			return directories;
		}
	}
}
