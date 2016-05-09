#region License
////////////////////////////////////////////////////////////////////////
//                  Q - A B C   S O U N D   P L A Y E R
//
//                   Copyright (C) Pieter Geerkens 2012
////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PGSoftwareSolutionsInc.Util {
	/// <summary> Transparent Panel control.</summary>
	/// <remarks>
	/// <see href="http://componentfactory.blogspot.ca/2005/06/net2-transparent-controls.html"/>
	/// <see href="http://www.bobpowell.net/transcontrols.htm"/>
	/// </remarks>
	public class TransparentPanel : Panel {
    /// <summary>TODO</summary>
		public TransparentPanel() : base() {
			SetStyle(ControlStyles.SupportsTransparentBackColor,true);
			BackColor  = Color.Transparent;
		}
		/// <summary>Make a truly transparent Panel control.</summary>
		/// <remarks>Change the behaviour of the window by giving it a WS_EX_TRANSPARENT style.
		/// <see href="http://www.bobpowell.net/transcontrols.htm"/></remarks>
		protected override CreateParams CreateParams { 
			get { 
				var cp=base.CreateParams; 
				cp.ExStyle |= (int)WindowStylesEx.WS_EX_TRANSPARENT;
				return cp; 
			} 
		}

		/// <summary> Invalidate entire parent control to redraw on our background.</summary>
		/// <remarks>Invalidate the parent of the control, not the control itself, whenever 
		/// we need to update the graphics. This ensures that whatever is behind the control 
		/// gets painted before we need to do our own graphics output.
		/// <see href="http://www.bobpowell.net/transcontrols.htm"/></remarks>
		public virtual void InvalidateEx() { 
			InvalidateEx(new Rectangle(this.Location,this.Size));
		} 
		///<summary><inheritdoc cref="InvalidateEx()" /></summary>
		/// <param name="r"><c>Rectangle</c> to be invalidated.</param>
		public virtual void InvalidateEx(Rectangle r) {
            if (Parent != null)  Parent.Invalidate(r, true);
        }
        /// <summary> Prevent background painting from overwriting transparent background</summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent) { /* NO-OP */ } 
	}
}
