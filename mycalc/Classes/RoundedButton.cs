using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace AdvButton
{
	/// <summary>
	/// Summary description for RoundButton.
	/// </summary>
	public class RoundedButton : Button
	{
        /// <summary>
        /// Read or change the colour of the border
        /// </summary>
        public Color BorderColor { get => FlatAppearance.BorderColor; set { FlatAppearance.BorderColor = value; } }
        /// <summary>
        /// Read or change the size of the border
        /// </summary>
        public int BorderSize { get => FlatAppearance.BorderSize; set { FlatAppearance.BorderSize = value; } }

        /// <summary>
        /// Constructor initialises <c>FlatStyle</c> and <c>BorderSize</c>
        /// </summary>
        public RoundedButton():base()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        /// <summary>
        /// overrides <c>OnPaint</c> method to create the rounded effect
        /// </summary>
        /// <param name="e"><c>OnPaint event arguments</c></param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            GraphicsPath grPath = new GraphicsPath();
            grPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            this.Region = new System.Drawing.Region(grPath);
            base.OnPaint(e);
        }
	}
}
