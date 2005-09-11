//
// IHtmlRender.cs: Interface that abstracts the html render widget
//
// Author: Mario Sopena
//
using System;
using Gtk;

namespace Monodoc {
public interface IHtmlRender {
	// Jump to an anchor of the form <a name="tttt">
	void JumpToAnchor (string anchor_name);

	//Copy to the clipboard the selcted text
	void Copy ();

	//Select all the text
	void SelectAll ();

	//Render the HTML code given
	void Render (string html_code);

	//Event fired when the use is over an Url
	event EventHandler OnUrl;

	//Event fired when the user clicks on a Link
	event EventHandler UrlClicked;

	// Variable that handles the info encessary for the events
	// As every implementation of HtmlRender will have differents events
	// we try to homogenize them with the variabel
	string Url { get; }

	Widget HtmlPanel { get; }

	void Print (string Html);
}


}
