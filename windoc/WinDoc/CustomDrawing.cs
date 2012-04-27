using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WinDoc
{
	static class CustomDrawing
	{
		// This method is here because by default TreeView try to display icon in every case
		// i.e. even when we have no icon to show it's going to put a blank space. So here we 
		// detect when that happen and "shift" the text back in the right position 16px to the left
		internal static void DrawDocTreeNodeText (object sender, DrawTreeNodeEventArgs e)
		{
			if (!string.IsNullOrEmpty (e.Node.ImageKey)) {
				e.DrawDefault = true;
				return;
			}
			// Retrieve the node font. If the node font has not been set,
            // use the TreeView font.
            Font nodeFont = e.Node.NodeFont;
            if (nodeFont == null)
				nodeFont = ((TreeView)sender).Font;

            // Draw the node text.
			var clip = new Rectangle (e.Bounds.X - 16, e.Bounds.Y, e.Bounds.Width + 16, e.Bounds.Height);
			e.Graphics.SetClip (clip);
			if ((e.State & TreeNodeStates.Selected) != 0) {
				e.Graphics.Clear (SystemColors.Highlight);
				using (var pen = new Pen (Color.Black)) {
					pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
					e.Graphics.DrawRectangle (pen, new Rectangle (clip.Location, new Size (clip.Width - 1, clip.Height - 1)));
				}
				e.Graphics.DrawString (e.Node.Text, nodeFont, SystemBrushes.HighlightText, clip);
			} else {
				e.Graphics.Clear (Color.White);
				e.Graphics.DrawString (e.Node.Text, nodeFont, Brushes.Black, clip);
			}
		}

		internal static void DrawSearchResultNodeText (object sender, DrawTreeNodeEventArgs e)
		{
            Font nodeFont = e.Node.NodeFont;
            if (nodeFont == null)
				nodeFont = ((TreeView)sender).Font;
			var clip = new Rectangle (0, e.Bounds.Y, e.Node.TreeView.Width, e.Bounds.Height);
			e.Graphics.SetClip (clip);
			var selectedStates = TreeNodeStates.Marked | TreeNodeStates.Selected | TreeNodeStates.Focused;

			if (e.Node.Tag == null) {
				nodeFont = new Font (nodeFont.FontFamily, nodeFont.Size - 2);
				// We use a TabRenderer to get the nice system gradient
				e.Graphics.Clear (Color.White);
				clip = new Rectangle (1, e.Bounds.Y + 1, e.Node.TreeView.ClientRectangle.Width - 3, e.Bounds.Height - 3);
				TabRenderer.DrawTabItem (e.Graphics, clip, e.Node.Text, nodeFont, System.Windows.Forms.VisualStyles.TabItemState.Normal);
				using (var pen = new Pen (Color.Black, 1.0f))
					e.Graphics.DrawLine (pen, new Point (clip.Left, clip.Bottom), new Point (clip.Right - 1, clip.Bottom));
			} else {
				e.Graphics.Clear ((e.State & selectedStates) != 0 ? SystemColors.Highlight : SystemColors.ControlLightLight);
				e.Graphics.DrawString (e.Node.Text, nodeFont, (e.State & selectedStates) != 0 ? SystemBrushes.HighlightText : SystemBrushes.ControlText, new PointF (e.Bounds.X, e.Bounds.Y + 2));
			}
		}
	}
}
