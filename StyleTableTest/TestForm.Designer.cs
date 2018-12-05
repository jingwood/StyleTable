namespace StyleTableTest
{
	partial class TestForm
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
			this.components = new System.ComponentModel.Container();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.table = new Jingwood.WindowsFormControl.StyleTable.StyleTableControl();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertToolStripMenuItem,
            this.removeToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(125, 48);
			// 
			// insertToolStripMenuItem
			// 
			this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
			this.insertToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
			this.insertToolStripMenuItem.Text = "Insert";
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
			this.removeToolStripMenuItem.Text = "Remove";
			// 
			// table
			// 
			this.table.BackColor = System.Drawing.SystemColors.Window;
			this.table.DefaultRowHeight = 25;
			this.table.Dock = System.Windows.Forms.DockStyle.Fill;
			this.table.FocusCell = null;
			this.table.FocusRow = null;
			this.table.HeaderContextMenuStrip = null;
			this.table.HeaderHeight = 30;
			this.table.IndexTextConverter = null;
			this.table.IsHeaderFreezed = false;
			this.table.LeadHeadContextMenuStrip = null;
			this.table.Location = new System.Drawing.Point(0, 0);
			this.table.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.table.Name = "table";
			this.table.RowContextMenuStrip = this.contextMenuStrip1;
			this.table.Size = new System.Drawing.Size(722, 439);
			this.table.StartRowIndex = 1;
			this.table.TabIndex = 0;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(722, 439);
			this.Controls.Add(this.table);
			this.Name = "Jingwood.WindowsFormControl.StyleTable";
			this.Text = "Jingwood.WindowsFormControl.StyleTable";
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Jingwood.WindowsFormControl.StyleTable.StyleTableControl table;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
	}
}