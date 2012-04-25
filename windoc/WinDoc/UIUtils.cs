using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Monodoc;

namespace WinDoc
{
	static class UIUtils
	{	
		internal static string GetImageKeyFromNode (Node node)
		{
			if (node.Caption.EndsWith (" Class"))
				return "class.png";
			if (node.Caption.EndsWith (" Interface"))
				return "interface.png";
			if (node.Caption.EndsWith (" Structure"))
				return "structure.png";
			if (node.Caption.EndsWith (" Enumeration"))
				return "enumeration.png";
			if (node.Caption.EndsWith (" Delegate"))
				return "delegate.png";
			var url = node.PublicUrl;
			if (!string.IsNullOrEmpty (url) && url.StartsWith ("N:"))
				return "namespace.png";
			return null;
		}

		internal static string GetParentImageKeyFromNode (Node node)
		{
			switch (node.Caption) {
				case "Methods":
				case "Constructors":
					return "method.png";
				case "Properties":
					return "property.png";
				case "Events":
					return "event.png";
				case "Members":
					return "members.png";
				case "Fields":
					return "field.png";
			}

			return null;
		}
	}
}
