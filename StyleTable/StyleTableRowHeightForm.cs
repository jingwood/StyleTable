using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Jingwood.WindowsFormControl.StyleTable
{
	public partial class StyleTableRowHeightForm : Form
	{
		private int rowHeight;

		public int RowHeight
		{
			get { return rowHeight; }
			set { rowHeight = value; }
		}

		public StyleTableRowHeightForm()
		{
			InitializeComponent();

			Text = "Row Height";
			btnOK.Text = "OK";
			btnCancel.Text = "Cancel";
			label1.Text = "Row Height";

			numericUpDown1.KeyDown += new KeyEventHandler(numericUpDown1_KeyDown);
		}

		void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				btnOK.PerformClick();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			numericUpDown1.Value = rowHeight;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			rowHeight = (int)numericUpDown1.Value;
		}
	}
}
