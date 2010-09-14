//
// PTree - A dynamically loaded TOC tree
//
// Author:
// 		Piers Haken (piersh@friskit.com)
//
// (C) 2003 Piers Haken
//

// TODO:
//	work out how to cancel scrolling keyboard events on Mozilla
//	better support for multiple trees in a single body

function PTree ()
{
	this.strActionBase = "";
	this.strSrcBase = "";
	this.strTargetDefault = "";
	this.strImagesBase = "images/";
	this.strImageExt = ".png";
	this.eltSelected = null;
	this.nImageWidth = 18;
	this.nImageHeight = 18;

	this.CreateItemFromXML = function (oNode, fLast, eltParent)
	{
		var strText = oNode.getAttribute ("text");
		var strAction = oNode.getAttribute ("action");
		var strSrc = oNode.getAttribute ("src");
		var strTarget = oNode.getAttribute ("target");
		return this.CreateItem (eltParent, strText, strAction, strSrc, fLast, strTarget, fLast, eltParent);
	}

	this.CreateItem = function (eltParent, strText, strAction, strSrc, fLast, strTarget)
	{
		var _this = this;

		var eltDiv = document.createElement ("DIV");
		if (eltParent == null)
			eltDiv.tree_fRoot = true;

		if (fLast)
			eltDiv.tree_fLast = true;

		if (strAction)
			eltDiv.tree_action = strAction;

		if (strSrc != null)
			eltDiv.tree_src = strSrc;

		var eltSpan = document.createElement ("SPAN");
		eltSpan.className = "tree-label";

		if (eltParent)
		{
			eltDiv.className = "tree-node-collapsed";

			// this node's tree icon
			var eltIcon = new Image ();
			eltIcon.width = this.nImageWidth;
			eltIcon.height = this.nImageHeight;
			if (strSrc)
				eltIcon.onclick = function () { _this.LoadNode (this); }
			eltIcon.src = this.GetIconSrc (eltDiv, true);
			eltSpan.appendChild (eltIcon);

			// parent's tree icons
			var eltIconLast = eltIcon;
			var eltParentDiv = eltParent;
			while (!this.IsRootDiv (eltParentDiv))
			{
				var eltIcon = new Image ();
				eltIcon.width = this.nImageWidth;
				eltIcon.height = this.nImageHeight;
				if (this.IsLastDiv (eltParentDiv))
					eltIcon.src = this.strImagesBase + "blank" + this.strImageExt;
				else
					eltIcon.src = this.strImagesBase + "I" + this.strImageExt;

				eltSpan.insertBefore (eltIcon, eltIconLast);
				eltIconLast = eltIcon;
				eltParentDiv = this.GetParentDiv (eltParentDiv);
			}
		}
		else
		{
			eltDiv.className = "tree-node";
			//document.body.onkeydown = function () { return _this.onKeyDown (); }
		}

		// description
		var eltText = document.createTextNode (strText);
		var eltDescription;

		if (strAction)
		{
			eltDescription = document.createElement ("a");
			eltDescription.href = this.strActionBase + strAction;
			eltDescription.title = strText;
			if (strTarget)
				eltDescription.target = strTarget;
			else if (this.strTargetDefault)
				eltDescription.target = this.strTargetDefault;
			eltDescription.appendChild (eltText);
			eltDescription.onclick = function () { _this.SelectNode (eltDiv); }
			eltDescription.onmouseover = function () { this.blur (); }
			eltDescription.onmouseup = function () { this.blur (); }
		}
		else
		{
			eltDescription = document.createElement ("span");
			eltDescription.className = "tree-label";
			eltDescription.innerHTML = strText;
		}

		eltSpan.appendChild (eltDescription);
		eltDiv.appendChild (eltSpan);

		// append this node to its parent
		if (eltParent)
			eltParent.appendChild (eltDiv);
		else
			this.SelectNode (eltDiv);

		return eltDiv;
	}

	this.SelectNode = function (eltDiv)
	{
		if (this.eltSelected != eltDiv)
		{
			if (eltDiv)
			{
				var eltLabel = this.GetSpan (eltDiv);
				eltLabel.className = "tree-label-selected";
			}
			if (this.eltSelected)
			{
				var eltLabel = this.GetSpan (this.eltSelected);
				eltLabel.className = "tree-label";
			}
			this.eltSelected = eltDiv;
		}
	}

	this.LoadNode = function (eltIcon)
	{
		var eltDiv = this.GetDivFromIcon (eltIcon);
		eltIcon.onclick = null;

		var eltLoading = this.CreateItem (eltDiv, "<img src=\"../images/searching.gif\"/>Loading...", null, null, true);
		eltLoading.className = '';

		var xmlHttp = XmlHttp.create();
		xmlHttp.open ("GET", this.strSrcBase + eltDiv.tree_src, true);	// async
		var _this = this;
		xmlHttp.onreadystatechange = function () { _this.onReadyStateChange (xmlHttp, eltIcon, eltLoading); }
		setTimeout (function () { xmlHttp.send (null); }, 10);
	}

	this.onReadyStateChange = function (xmlHttp, eltIcon, eltLoading)
	{
		if (xmlHttp.readyState != 4)
			return;
		// XML loaded
		var eltDiv = this.GetDivFromIcon (eltIcon);

		try
		{
			var doc = xmlHttp.responseXML;
			var root = doc.documentElement;

			var nodes = root.childNodes;
			var cNodes = nodes.length;

			for (var iNode = 0; iNode < cNodes; iNode ++)
				this.CreateItemFromXML (nodes [iNode], iNode == cNodes-1, eltDiv);

			eltDiv.removeChild (eltLoading);

			if (this.eltSelected == eltLoading)
				this.SelectNode (this.GetFirstChild (eltDiv));

			eltIcon.src = this.GetIconSrc (eltDiv, false);
		}
		catch (e)
		{
			this.SetText (eltLoading, "Failed to load topic");
		}
		eltDiv.className = "tree-node";
		var _this = this;
		eltIcon.onclick = function () { _this.onClickMinus (this); }
	}

	this.onClickPlus = function (eltIcon)
	{
		var eltDiv = this.GetDivFromIcon (eltIcon);
		eltDiv.className = "tree-node";
		eltIcon.src = this.GetIconSrc (eltDiv, false);
		var _this = this;
		eltIcon.onclick = function () { _this.onClickMinus (this); }
	}

	this.onClickMinus = function (eltIcon)
	{
		var eltDiv = this.GetDivFromIcon (eltIcon);
		eltDiv.className = "tree-node-collapsed";
		eltIcon.src = this.GetIconSrc (eltDiv, true);
		var _this = this;
		eltIcon.onclick = function () { _this.onClickPlus (this); }
	}

	this.onKeyDown = function (event)
	{
		var eltSelect = this.eltSelected;
		var fLast = this.IsLastDiv (eltSelect);
		var fRoot = this.IsRootDiv (eltSelect);

		switch (event.keyCode)
		{
		case 13: // return
			var eltLink = eltSelect.firstChild.lastChild;
			if (eltSelect.tree_action)
				window.open (eltLink.href, eltLink.target);
			this.SelectNode (eltSelect);
			return false;	// don't EnsureVisible

		case 38: // up
			if (!fRoot)
			{
				if (this.IsFirstChild (eltSelect))
					eltSelect = this.GetParentDiv (eltSelect);
				else
				{
					eltSelect = eltSelect.previousSibling;
					while (this.IsExpanded (eltSelect))
						eltSelect = eltSelect.lastChild;
				}
			}
			break;

		case 40: // down
			if (this.IsExpanded (eltSelect))
				eltSelect = this.GetFirstChild (eltSelect);
			else if (!fLast)
				eltSelect = eltSelect.nextSibling;
			else
			{
				while (!this.IsRootDiv (eltSelect) && this.IsLastDiv (eltSelect))
					eltSelect = this.GetParentDiv (eltSelect);

				if (this.IsRootDiv (eltSelect))
					return false;

				eltSelect = eltSelect.nextSibling;
			}
			break;

		case 37: // left
			if (!fRoot)
			{
				if (this.IsExpanded (eltSelect))
					this.onClickMinus (this.GetIconFromDiv (eltSelect));
				else
					eltSelect = this.GetParentDiv (eltSelect);
			}
			break;

		case 39: // right
			if (this.HasChildren (eltSelect))
			{
				var eltChild = this.GetFirstChild (eltSelect);
				if (this.IsExpanded (eltSelect))
					eltSelect = eltChild;
				else if (eltChild != null)
					this.onClickPlus (this.GetIconFromDiv (eltSelect));
				else
					this.LoadNode (this.GetIconFromDiv (eltSelect));
			}
			break;

		default:
			return true;
		}

		this.SelectNode (eltSelect);
		this.EnsureVisible (this.GetLabel (eltSelect));

		return false;
	}

	this.SetText = function (eltDiv, strText)
	{
		var eltText = eltDiv.lastChild;
		eltText.nodeValue = strText;
	}

	this.GetIconSrc = function (eltDiv, fPlus)
	{
		var strIconSrc = this.IsLastDiv (eltDiv) ? "L" : "T";
		if (eltDiv.tree_src != null)
			strIconSrc += fPlus ? "plus" : "minus";
		return this.strImagesBase + strIconSrc + this.strImageExt;
	}

	this.GetDivFromIcon = function (eltIcon)
	{
		return eltIcon.parentNode.parentNode;
	}

	this.GetIconFromDiv = function (eltDiv)
	{
		return eltDiv.firstChild.lastChild.previousSibling;
	}

	this.GetFirstChild = function (eltDiv)
	{
		return eltDiv.firstChild.nextSibling;
	}

	this.GetSpan = function (eltDiv)
	{
		return eltDiv.firstChild;
	}

	this.GetLabel = function (eltDiv)
	{
		return eltDiv.firstChild.lastChild;
	}

	this.GetParentDiv = function (eltDiv)
	{
		if (this.IsRootDiv (eltDiv))
			return null;
		return eltDiv.parentNode;
	}

	this.HasChildren = function (eltDiv)
	{
		return eltDiv.tree_src || this.IsRootDiv (eltDiv);
	}

	this.IsLastDiv = function (eltDiv)
	{
		return eltDiv.tree_fLast;
	}

	this.IsRootDiv = function (eltDiv)
	{
		return Boolean (eltDiv.tree_fRoot);
	}

	this.IsExpanded = function (eltDiv)
	{
		return eltDiv.className != "tree-node-collapsed";
	}

	this.IsFirstChild = function (eltDiv)
	{
		var fFirst =
			eltDiv.previousSibling &&
			eltDiv.previousSibling.tagName != "DIV";
		return fFirst;
	}

	this.EnsureVisible = function (elt)
	{
		var x = 0;
		var y = 0;
		var parent = elt;
		while (parent != null)
		{
			x += parent.offsetLeft;
			y += parent.offsetTop;
			parent = parent.offsetParent;
		}

		var yView = window.frameElement.scrollTop + document.body.scrollTop;
		var dyView = document.body.clientHeight;
		var dy = 0;
		if (y + elt.offsetHeight > yView + dyView)
			dy = (y + elt.offsetHeight) - (yView + dyView);
		if (y < yView + dy)
			dy = y - yView;

		var xView = window.frameElement.scrollLeft + document.body.scrollLeft;
		var dxView = document.body.clientWidth;
		var dx = 0;
		if (x + elt.offsetWidth > xView + dxView)
			dx = (x + elt.offsetWidth) - (xView + dxView);
		if (x < xView + dx)
			dx = x - xView;

		if (dx != 0 || dy != 0)
			window.scrollBy (dx, dy);
	}
}
