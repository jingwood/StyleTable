namespace Jingwood.WindowsFormControl.StyleTable
{
	partial class StyleTableControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.vScrollBar = new System.Windows.Forms.VScrollBar();
			this.hScrollBar = new System.Windows.Forms.HScrollBar();
			this.SuspendLayout();
			// 
			// vScrollBar
			// 
			this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.vScrollBar.Location = new System.Drawing.Point(451, 0);
			this.vScrollBar.Name = "vScrollBar";
			this.vScrollBar.Size = new System.Drawing.Size(18, 350);
			this.vScrollBar.TabIndex = 0;
			this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar_Scroll);
			// 
			// hScrollBar
			// 
			this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.hScrollBar.Location = new System.Drawing.Point(0, 332);
			this.hScrollBar.Name = "hScrollBar";
			this.hScrollBar.Size = new System.Drawing.Size(451, 18);
			this.hScrollBar.TabIndex = 1;
			this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar_Scroll);
			// 
			// StyleTableControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.hScrollBar);
			this.Controls.Add(this.vScrollBar);
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "StyleTableControl";
			this.Size = new System.Drawing.Size(469, 350);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.VScrollBar vScrollBar;
		private System.Windows.Forms.HScrollBar hScrollBar;
	}
}
