namespace StyleTableTest
{
	partial class Form1
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.styleTableControl1 = new Unvell.UIControl.StyleTable.StyleTableControl();
			this.SuspendLayout();
			// 
			// styleTableControl1
			// 
			this.styleTableControl1.BackColor = System.Drawing.SystemColors.Window;
			this.styleTableControl1.DefaultRowHeight = 25;
			this.styleTableControl1.FocusCell = null;
			this.styleTableControl1.FocusRow = null;
			this.styleTableControl1.HeaderContextMenuStrip = null;
			this.styleTableControl1.HeaderHeight = 20;
			this.styleTableControl1.IndexTextConverter = null;
			this.styleTableControl1.IsHeaderFreezed = false;
			this.styleTableControl1.LeadHeadContextMenuStrip = null;
			this.styleTableControl1.Location = new System.Drawing.Point(111, 55);
			this.styleTableControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.styleTableControl1.Name = "styleTableControl1";
			this.styleTableControl1.RowContextMenuStrip = null;
			this.styleTableControl1.Size = new System.Drawing.Size(484, 255);
			this.styleTableControl1.StartRowIndex = 1;
			this.styleTableControl1.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(764, 432);
			this.Controls.Add(this.styleTableControl1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private Unvell.UIControl.StyleTable.StyleTableControl styleTableControl1;
	}
}