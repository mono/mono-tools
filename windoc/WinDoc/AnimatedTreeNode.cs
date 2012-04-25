using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WinDoc
{
	class AnimatedTreeNode
	{
		Timer timer = new Timer ();

		public AnimatedTreeNode (TreeNode child)
		{
			this.Child = child;
			this.TreeView = child.TreeView;
			timer.Interval = 80;
			timer.Tick += AnimationCallback;
		}

		public TreeNode Child {
			get;
			private set;
		}

		public TreeView TreeView {
			get;
			set;
		}

		public void StartAnimation ()
		{
			if (TreeView.Nodes.Count == 0 || TreeView.Nodes[0] != Child) {
				TreeView.Nodes.Clear ();
				TreeView.Nodes.Add (Child);
				Child.EnsureVisible ();
			}
			timer.Start ();
		}

		public void StopAnimation ()
		{
			timer.Stop ();
		}

		void AnimationCallback (object sender, EventArgs e)
		{
			try {
				var count = TreeView.ImageList.Images.Count;
				Child.ImageIndex = Child.SelectedImageIndex = (Child.ImageIndex + 1) % count;
			} catch {
				StopAnimation ();
			}
		}
	}
}
