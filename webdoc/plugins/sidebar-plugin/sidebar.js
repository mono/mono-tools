function SetSelection(b,v)
{
	if (v){
		t = "activeTab";
		tab = "selected";
	} else {
		t = "tab";
		tab = "";
	}

	document.getElementById (b).className = t;
	document.getElementById (b + "Tab").className = tab;
}

function ShowContents ()
{
	SetSelection ("contents", true);
	SetSelection ("index", false);
}

function ShowIndex ()
{
	SetSelection ("contents", false);
	SetSelection ("index", true);
	document.getElementById ('indexInput').focus ();
}


