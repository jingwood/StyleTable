using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Jingwood.WindowsFormControl.StyleTable;

namespace StyleTableTest
{
	public partial class TestForm : Form
	{
		public TestForm()
		{
			InitializeComponent();

			table.HeaderHeight = Font.Height + 6;

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "No.",
				Width = 30,
				BackgroundColor =	Color.Gainsboro,
				Type = StyleTableColumnType.Index,
				HeaderAlignment = ContentAlignment.MiddleCenter,
				CellAlignment = ContentAlignment.MiddleCenter,
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Name",
				Width = 100,
				Type = StyleTableColumnType.General,
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Datetime",
				Width = 100,
				Type = StyleTableColumnType.Datetime,
				Dropdown = StyleTableDropdownType.Dropdown,
				Pattern = "yyyy/MM/dd",
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Favorite",
				Width = 100,
				Type = StyleTableColumnType.General,
				Dropdown = StyleTableDropdownType.Dropdown,
				Candidates = new string[] { "Apple", "Banana", "Orange", "Melon" },
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Region",
				Width = 100,
				Type = StyleTableColumnType.General,
				Dropdown = StyleTableDropdownType.Combo,
				Candidates = new string[] {"Tokyo", "Osaka", "Nagoya", "Kyoto", "Nara"},
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Age",
				Width = 50,
				Type = StyleTableColumnType.Numeric,
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "Yes?",
				Width = 40,
				Type = StyleTableColumnType.Checkbox,
			});

			table.AddColumnDefine(new StyleTableColumnDefine()
			{
				HeaderText = "See also",
				Width = 80,
				Type = StyleTableColumnType.Link,
				DefaultValue = "see also",
			});

			table.DefaultRowHeight = 20;
			table.AddEmptyRow(30);

			//table.AllowChangeRowHeight = false;
			//table.ClickToEdit = false;

			
		//	table.Readonly = true;

			insertToolStripMenuItem.Click += new EventHandler(insertToolStripMenuItem_Click);
			removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
		}

		void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (var row in table.SelectedRows)
			{
				table.RemoveRow(row.Index);
			}
		}

		void insertToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var row = table.SelectedRows.FirstOrDefault();
			if (row != null)
			{
				table.InsertEmptyRow(row.Index);
				table.SetFocusCell(row.Index - 1, 1);
				table.SelectRow(row.Index-1);
			}
		}
	}
}
