/*****************************************************************************
 * 
 * StyleTable
 *	
 * - Enhanced .NET List Control
 * 
 * Copyright © 2012 UNVELL All Rights Reserved
 * Copyright (c) 2012-2018 Jingwood, all rights reserved.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using Jingwood.WindowsFormControl.StyleTable.CellType;
using Jingwood.WindowsFormControl.StyleTable.Common;
using Jingwood.WindowsFormControl.StyleTable.XML;

namespace Jingwood.WindowsFormControl.StyleTable
{
	#region Control
	public partial class StyleTableControl : UserControl
	{
		#region Constructors
		private Panel freezedHeaderPanel = new Panel();

		public StyleTableControl()
		{
			InitializeComponent();

			//colDefines = new ColumnDefineList(this);

			hScrollBar.TabStop = false;
			vScrollBar.TabStop = false;

			editTextbox = new InputTextBox(this) { Visible = false, BorderStyle = BorderStyle.None };
			numericTextbox = new NumericField(this) { Visible = false };
			//dropWindow = new DropdownWindow(this) { Visible = false };

			ResetHeader();

			canvas = new StyleTableCanvas(this)
			{
				Location = new Point(0, 0),
				Dock = DockStyle.None,
			};
			freezedHeaderPanel = new Panel()
			{
				Dock = DockStyle.Top,
				Visible = false,
			};
			freezedHeaderPanel.Paint += (sender, e) =>
			{
				Graphics g = e.Graphics;
				DrawRow(g, header, ClientRectangle);
			};

			// initialize settings
			AllowChangeColumnWidth = true;
			AllowChangeRowHeight = true;
			ClickToEdit = true;
			Readonly = false;

			canvas.Controls.Add(editTextbox);
			canvas.Controls.Add(numericTextbox);

			Controls.Add(canvas);
			Controls.Add(freezedHeaderPanel);
			//canvas.SendToBack();

			vScrollBar.MouseEnter += new EventHandler(vScrollBar_MouseEnter);
			hScrollBar.MouseEnter += new EventHandler(hScrollBar_MouseEnter);

			// number
			//AddColumnDefine(new StyleTableColumnDefine
			//{
			//  BackgroundColor = Color.LightYellow,
			//  Type = StyleTableColumnType.Index,
			//  Text = "No.",
			//  Width = 40,
			//  IsReadonly = true,
			//});

			// item name
			//AddColumnDefine(new StyleTableColumnDefine
			//{
			//  BackgroundColor = Color.Transparent,
			//  Type = StyleTableColumnType.Text,
			//  Text = "Item Name",
			//  Width = 250,
			//  IsReadonly = false,
			//});

			// details
			//AddColumnDefine(new StyleTableColumnDefine
			//{
			//  BackgroundColor = Color.Transparent,
			//  Type = StyleTableColumnType.Button,
			//  Text = "",
			//  Width = 35,
			//  IsReadonly = true,
			//});


			//AddRow(new object[] { 1, "ユーザー名", "..." });
			//AddRow(new object[] { 2, "パスワード", "..." });
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			//for (int i = 0; i < colDefines.Count;i++ )
			//{
			//  InsertHeader(i);
			//}
		}
		#endregion

		#region Column Defines
		//public class ColumnDefineList : IList<StyleTableColumnDefine>
		//{
		//  private StyleTableControl list;

		//  public ColumnDefineList(StyleTableControl list){
		//    this.list = list;
		//  }

		//  private List<StyleTableColumnDefine> defs = new List<StyleTableColumnDefine>();

		//  public int IndexOf(StyleTableColumnDefine item)
		//  {
		//    return defs.IndexOf(item);
		//  }

		//  public void Insert(int index, StyleTableColumnDefine item)
		//  {
		//    defs.Insert(index, item);
		//  }

		//  public void RemoveAt(int index)
		//  {
		//    defs.RemoveAt(index);
		//  }

		//  public StyleTableColumnDefine this[int index]
		//  {
		//    get
		//    {
		//      return defs[index];
		//    }
		//    set
		//    {
		//      defs[index]=value;
		//    }
		//  }

		//  public void Add(StyleTableColumnDefine item)
		//  {
		//    defs.Add(item);
		//    list.AddColumnDefine(item);
		//  }

		//  public void Clear()
		//  {
		//    defs.Clear();
		//  }

		//  public bool Contains(StyleTableColumnDefine item)
		//  {
		//    return defs.Contains(item);
		//  }

		//  public void CopyTo(StyleTableColumnDefine[] array, int arrayIndex)
		//  {
		//    defs.CopyTo(array, arrayIndex);
		//  }

		//  public int Count
		//  {
		//    get { return defs.Count; }
		//  }

		//  public bool IsReadOnly
		//  {
		//    get { return false; }
		//  }

		//  public bool Remove(StyleTableColumnDefine item)
		//  {
		//    return defs.Remove(item);
		//  }

		//  public IEnumerator<StyleTableColumnDefine> GetEnumerator()
		//  {
		//    return defs.GetEnumerator();
		//  }

		//  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		//  {
		//    return defs.GetEnumerator();
		//  }
		//}
		//private ColumnDefineList colDefines;

		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		//public ColumnDefineList ColDefines
		//{
		//  get { return colDefines; }
		//  set { colDefines = value; }
		//}
		private List<StyleTableColumnDefine> colDefines = new List<StyleTableColumnDefine>();
		public List<StyleTableColumnDefine> ColDefines { get { return colDefines; } }
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		//public List<StyleTableColumnDefine> ColDefines
		//{
		//  get { return colDefines; }
		//  set { colDefines = value; }
		//}

		public void AddColumnDefine(StyleTableColumnDefine def)
		{
			InsertColumnDefine(colDefines.Count, def);
		}

		public void InsertColumnDefine(int index, StyleTableColumnDefine def)
		{
			if (def.HoverCursor == null)
			{
				if ((def.Type == StyleTableColumnType.General
					&& def.Dropdown != StyleTableDropdownType.Dropdown)
					|| def.Type == StyleTableColumnType.Numeric)
				{
					def.HoverCursor = Cursors.IBeam;
				}
				else if (def.Type == StyleTableColumnType.Link)
				{
					def.HoverCursor = Cursors.Hand;
				}
				else
				{
					def.HoverCursor = Cursors.Default;
				}
			}

			// add column define
			colDefines.Insert(index, def);

			// insert header
			InsertHeader(index);
		}
		#endregion

		#region Header
		private StyleTableRow header = null;
		private int headerHeight = 20;
		public int HeaderHeight
		{
			get { return headerHeight; }
			set
			{
				canvas.Height -= headerHeight - value;
				headerHeight = value;
				SetRowHeight(header, value);
			}
		}
		private void ResetHeader()
		{
			header = new StyleTableRow { Index = -1 };
			header.Bounds = new Rectangle(0, 0, 0, headerHeight);
		}
		private bool isHeaderFreezed;
		public bool IsHeaderFreezed
		{ get { return isHeaderFreezed; } set { isHeaderFreezed = value; } }
		internal void FreezeHeader()
		{
			isHeaderFreezed = true;
			freezedHeaderPanel.Height = header.Height;
			freezedHeaderPanel.Visible = true;
			freezedHeaderPanel.Invalidate();
		}
		internal void UnfreezeHeader()
		{
			isHeaderFreezed = false;
			freezedHeaderPanel.Visible = false;
		}
		private void InsertHeader(int index)
		{
			StyleTableColumnDefine def = colDefines[index];

			// add header cell
			StyleTableHeaderCell cell = new StyleTableHeaderCell();
			cell.Define = def;
			header.InsertCell(cell, index, def);
			cell.Data = def.HeaderText;
			//cell.BackgroundBrush = new SolidBrush(headerBackgrondColor);
			canvas.Width += def.Width + 1;

			// add row
			rows.ForEach(r => r.InsertCell(index, null));
		}
		#endregion

		#region Row & Cell Manage
		public event EventHandler<StyleTableRowEventArgs> RowAdded;
		public event EventHandler<StyleTableRowEventArgs> RowRemoved;

		private List<StyleTableRow> rows = new List<StyleTableRow>();
		internal List<StyleTableRow> Rows { get { return rows; } }

		public StyleTableRow GetRow(int i) { return rows[i]; }

		private StyleTableRow CreateRow()
		{
			return new StyleTableRow()
			{
				Grid = this,
			};
		}
		private StyleTableRow CreateRowWithObjects(object[] objs)
		{
			StyleTableRow row = CreateRow();
			for (int i = 0; i < colDefines.Count; i++)
			{
				row.AddCell(i < objs.Length ? objs[i] : colDefines[i].DefaultValue);
			}
			return row;
		}
		internal StyleTableRow CreateEmptyRow()
		{
			StyleTableRow row = CreateRow();
			object[] objs = new object[ColDefines.Count];
			for (int i = 0; i < ColDefines.Count; i++)
				row.AddCell(ColDefines[i].DefaultValue);
			return row;
		}
		public StyleTableRow AddData(object[] objs)
		{
			StyleTableRow row = FindNextEmptyRow(0);
			if (row == null)
				row = AddRow(objs);
			else
				row.SetDatas(objs);
			return row;
		}
		internal StyleTableRow FindNextEmptyRow(int row)
		{
			for (int i = row; i < rows.Count; i++)
			{
				StyleTableRow r = rows[i];
				bool isFound = true;
				for (int x = 0; x < colDefines.Count; x++)
					if (!object.Equals(r.Cells[x].Data, colDefines[x].DefaultValue))
					{
						isFound = false;
						break;
					}
				if (isFound) return rows[i];
			}
			return null;
		}
		public StyleTableRow AddRow(object[] objs)
		{
			StyleTableRow row = CreateRowWithObjects(objs);
			AddRow(row);
			return row;
		}
		internal void AddRow(StyleTableRow row)
		{
			InsertRow(rows.Count, row);
		}
		public void AddEmptyRow(int count)
		{
			for (int i = 0; i < count; i++) AddRow(CreateEmptyRow());
		}
		public StyleTableRow InsertEmptyRow(int index)
		{
			StyleTableRow row = CreateEmptyRow();
			InsertRow(index, row);
			return row;
		}
		public StyleTableRow InsertRow(int index, object[] objs)
		{
			StyleTableRow row = CreateRowWithObjects(objs);
			InsertRow(index, row);
			return row;
		}
		public StyleTableRow AppendEmptyRow()
		{
			return InsertEmptyRow(RowCount);
		}

		private int defaultRowHeight = 25;

		public int DefaultRowHeight
		{
			get { return defaultRowHeight; }
			set { defaultRowHeight = value; }
		}

		private Func<int, string> indexTextConverter = null;

		public Func<int, string> IndexTextConverter
		{
			get { return indexTextConverter; }
			set { indexTextConverter = value; }
		}

		private int startRowIndex = 1;

		public int StartRowIndex
		{
			get { return startRowIndex; }
			set { startRowIndex = value; }
		}

		internal void InsertRow(int index, StyleTableRow row)
		{
			DateTime dt = DateTime.Now;

			// measure bound
			int height = (row.Height == 0 ? defaultRowHeight : row.Height);

			int top = header.Bounds.Height + 1;
			for (int i = 0; i < index && i < rows.Count; i++)
				top += rows[i].Bounds.Height + 1;
			row.Bounds = new Rectangle(canvas.Left, top, row.Width, height);
			row.Grid = this;
			row.Index = index;

			foreach (StyleTableCell cell in row.Cells)
			{
				if (cell.Define.Type == StyleTableColumnType.Index)
				{
					int num = StartRowIndex + index;
					cell.Data = IndexTextConverter == null ? num.ToString() : IndexTextConverter(num);
					break;
				}
			}

			row.UpdateUI();
			rows.Insert(index, row);
			for (int i = index + 1; i < rows.Count; i++)
			{
				OffsetRow(rows[i], height + 1);
				rows[i].Index++;

				foreach (StyleTableCell cell in rows[i].Cells)
				{
					if (cell.Define.Type == StyleTableColumnType.Index)
					{
						int num = StartRowIndex + rows[i].Index;
						cell.Data = IndexTextConverter == null ? num.ToString() : IndexTextConverter(num);
						break;
					}
				}
			}

			InvalidateCanvas();
			UpdateCanvasSize();

			if (RowAdded != null) RowAdded(this, new StyleTableRowEventArgs { Row = row });
			//canvas.Invalidate();

			//Debug.WriteLine("row inserted: " + DateTime.Now.Subtract(dt).TotalMilliseconds + " ms.");
		}
		public StyleTableRow RemoveRow(int row)
		{
			StyleTableRow removedRow = (row >= 0 && row < rows.Count) ? RemoveRow(rows[row]) : null;
			InvalidateCanvas();
			return removedRow;
		}
		internal StyleTableRow RemoveRow(StyleTableRow row)
		{
			if (currentEditingRow == row)
			{
				if (currentEditingCell != null)
					EndEdit(currentEditingCell.Data.ToString());
				else
					currentEditingRow = null;
			}

			lock (rows)
			{
				rows.Remove(row);
			}

			// move focus
			if (row == focusRow)
			{
				if (focusCell != null)
				{
					focusCell.IsFocus = false;
					focusRow = null;
					focusCell = null;
				}

				if (row.Index < rows.Count - 1)
				{
					SetFocusCell(row.Index, focusCell == null ? 0 : focusCell.Index);
				}
				else if (row.Index > 0)
				{
					SetFocusCell(row.Index - 1, focusCell == null ? 0 : focusCell.Index);
				}
			}

			// update bounds
			for (int i = row.Index; i < rows.Count; i++)
			{
				StyleTableRow updateRow = rows[i];

				OffsetRow(updateRow, -row.Height - 1);

				updateRow.Index--;

				foreach (StyleTableCell cell in updateRow.Cells)
				{
					if (cell.Define.Type == StyleTableColumnType.Index)
					{
						cell.Data = IndexTextConverter == null ? updateRow.Index.ToString() : IndexTextConverter(updateRow.Index);
					}
				}
			}

			UpdateCanvasSize();

			if (RowRemoved != null) RowRemoved(this, new StyleTableRowEventArgs { Row = row });

			return row;
		}
		internal void RemoveRows(IEnumerable<StyleTableRow> drows)
		{
			// todo: improve performance here
			foreach (StyleTableRow row in drows)
			{
				RemoveRow(row);
			}

			InvalidateCanvas();
		}
		internal bool MoveRow(int row, int target)
		{
			if (row < 0 || row > rows.Count - 1) return false;
			if (target < 0 || target > rows.Count - 1) return false;

			// todo: improve performance here
			StyleTableRow rowObj = RemoveRow(row);
			InsertRow(target, rowObj);

			//int from = Math.Min(row, target);
			//int to = Math.Max(row, target);

			//lock (rows) rows.RemoveAt(row);
			//for (int i = from; i < rows.Count && i < to; i++)
			//{

			//}

			return true;
		}
		internal StyleTableCell GetCell(int row, int cell)
		{
			if (row < 0 || row > rows.Count - 1) return null;
			StyleTableRow r = rows[row];
			return (cell < 0 || cell > r.Cells.Count - 1) ? null : r.Cells[cell];
		}
		internal StyleTableCell CreateCell(StyleTableColumnDefine def)
		{
			StyleTableCell cell = null;
			switch (def.Type)
			{
				default:
					cell = new StyleTableCell();
					break;

				case StyleTableColumnType.General:
					cell = new DropDownListCell();
					break;

				case StyleTableColumnType.Button:
					cell = new ButtonCell();
					break;

				case StyleTableColumnType.Link:
					cell = new HyperlinkCell();
					break;

				case StyleTableColumnType.Datetime:
					cell = new DateTimeCell();
					break;

				case StyleTableColumnType.Checkbox:
					cell = new CheckboxCell();
					break;

				case StyleTableColumnType.Numeric:
					cell = new NumericCell();
					break;

				case StyleTableColumnType.Custom:
					//if (CustomCellInsert == null)
					//	throw new StyleTableException("CustomCellInsert must be provided when insert custom cell.");
					//CustomStyleTableCellInsertEventArgs arg = new CustomStyleTableCellInsertEventArgs();
					//CustomCellInsert(this, arg);
					//cell = arg.NewCell;
					if (def.CustomCellType != null)
					{
						cell = Activator.CreateInstance(def.CustomCellType) as StyleTableCell;
					}
					if (cell == null) throw new StyleTableException("Cannot create custom cell.");
					break;
			}
			cell.Define = def;
			cell.OnInit();
			return cell;
		}
		public int RowCount { get { return rows.Count; } }
		public void ClearRows()
		{
			SetFocusCell(null);
			
			this.rows.Clear();

			UpdateCanvasSize();
			InvalidateCanvas();
		}

		public void SetData(int row, object[] data)
		{
			if (row >= 0 && row < rows.Count)
			{
				rows[row].SetDatas(data);
			}
		}
		public object[] GetData(int row)
		{
			if (row >= 0 && row < rows.Count)
			{
				return rows[row].GetDatas();
			}
			return null;
		}
		#endregion

		#region Paint
		internal void InvalidateCanvas()
		{
			Rectangle rect = ClientRectangle;
			rect.Offset(-canvas.Left, -canvas.Top);
			canvas.Invalidate(rect);
		}
		internal void InvalidateCell(StyleTableCell cell)
		{
			canvas.Invalidate(cell.Bounds);
		}
		internal void InvalidateRow(StyleTableRow row)
		{
			canvas.Invalidate(row.Bounds);
		}
		private void DrawCanvas(Graphics g, Rectangle clip)
		{
#if DEBUG
			DateTime dt = DateTime.Now;
#endif

			clip.Inflate(1, 1);

			// draw header
			DrawRow(g, header, clip);

			// draw rows
			rows.ForEach(row =>
			{
				if (clip.IntersectsWith(row.Bounds)) DrawRow(g, row, clip);
			});

			if (focusCell != null)
			{
				focusCell.Draw(g);
				focusCell.DrawFocus(g);
			}

			// height adjusting
			if (heightChangeRow > -2 && cellAdjustHotLine > -1)
			{
				using (Brush hotRectBrush = new SolidBrush(Color.FromArgb(100, Color.SkyBlue)))
				{
					StyleTableRow row = (heightChangeRow == -1 ? header : rows[heightChangeRow]);
					g.FillRectangle(hotRectBrush, row.Bounds.Left, row.Bounds.Top,
						row.Bounds.Width, cellAdjustHotLine - row.Bounds.Top);
				}
				using (Pen hotLinePen = new Pen(Color.Black, 1))
				{
					hotLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
					g.DrawLine(hotLinePen, canvas.Left, cellAdjustHotLine,
						canvas.Right, cellAdjustHotLine);
				}
			}

			// width adjusting
			if (widthChangeColumn > -1 && cellAdjustHotLine > -1)
			{
				using (Brush hotRectBrush = new SolidBrush(Color.FromArgb(100, Color.SkyBlue)))
				{
					StyleTableCell cell = header.Cells[widthChangeColumn];
					Rectangle rect = new Rectangle(cell.Left, cell.Top,
						cellAdjustHotLine - cell.Left, canvas.Height);
					g.FillRectangle(hotRectBrush, rect);
				}
				using (Pen hotLinePen = new Pen(Color.Black, 1))
				{
					hotLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
					g.DrawLine(hotLinePen, cellAdjustHotLine, canvas.Top,
						cellAdjustHotLine, canvas.Bottom);
				}
			}

#if DEBUG
			double m = DateTime.Now.Subtract(dt).TotalMilliseconds;
			if (m > 40)
				Debug.WriteLine(string.Format("draw [{0}] {1} ms. ({2},{3}:{4},{5})",
				rows.Count, m,
				clip.Left, clip.Top, clip.Width, clip.Height));
#endif
		}
		private void DrawRow(Graphics g, StyleTableRow row, Rectangle clip)
		{
			row.Cells.ForEach(c =>
			{
				if (c != focusCell && clip.IntersectsWith(c.Bounds))
				{
					if (c.ClipBounds)
					{
						Rectangle rect = new Rectangle(c.Bounds.Left, c.Bounds.Top,
							c.Bounds.Width + 1, c.Bounds.Height + 1);
						g.SetClip(rect);
					}
					c.Draw(g);
					if (c.ClipBounds) g.ResetClip();
				}
			});

			g.DrawLine(Pens.Black, row.Bounds.Left, row.Bounds.Top,
				row.Bounds.Left, row.Bounds.Bottom);
			g.DrawLine(Pens.Black, row.Bounds.Left, row.Bounds.Bottom,
				row.Bounds.Right - 1, row.Bounds.Bottom);
		}
		#endregion

		#region Scroll Events
		void hScrollBar_MouseEnter(object sender, EventArgs e)
		{
			Cursor = Cursors.Default;
		}
		void vScrollBar_MouseEnter(object sender, EventArgs e)
		{
			Cursor = Cursors.Default;
		}
		#endregion

		#region Mouse Events
		public ContextMenuStrip LeadHeadContextMenuStrip { get; set; }
		public ContextMenuStrip HeaderContextMenuStrip { get; set; }
		public ContextMenuStrip RowContextMenuStrip { get; set; }

		private int selectStartRow = -1;
		private int lastHoverRow = -1;
		protected override void OnMouseMove(MouseEventArgs e)
		{
			bool isProcessed = false;

			if (heightChangeRow <= -2 && widthChangeColumn <= -1
				&& e.Button == MouseButtons.None
				&& !Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL)
				&& !Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
			{
				StyleTableRow lineAdjustRow = !AllowChangeRowHeight ? null :
					(rows.Union(new StyleTableRow[] { header }).FirstOrDefault(r =>
					r.Bounds.Bottom - 3 < e.Y && r.Bounds.Bottom + 3 > e.Y));

				if (lineAdjustRow != null
					&& lineAdjustRow.Cells.Count > 0
					&& e.X < lineAdjustRow.Cells[0].Bounds.Right)
				{
					Cursor = Cursors.HSplit;
					isProcessed = true;
				}
				else
				{
					StyleTableCell cell = !AllowChangeColumnWidth ? null :
						(header.Cells.FirstOrDefault(c => c.Bounds.Right - 3 < e.X
							&& c.Bounds.Right + 3 > e.X && c.Bounds.Bottom > e.Y));

					if (cell != null)
					{
						Cursor = Cursors.VSplit;
						isProcessed = true;
					}
				}
			}
			else if (heightChangeRow > -2)
			{
				cellAdjustHotLine = e.Y;
				isProcessed = true;
				InvalidateCanvas();
			}
			else if (widthChangeColumn > -1)
			{
				cellAdjustHotLine = e.X;
				isProcessed = true;
				InvalidateCanvas();
			}

			if (!isProcessed)
			{
				StyleTableRow row = GetRowByPoint(e.Location);
				if (row != null && row.Index > -1)
				{
					if (selectStartRow > -1
						//&& selectStartRow != lastSelectStartRow
						&& lastHoverRow != row.Index
						&& e.Button == MouseButtons.Left)
					{
						lastHoverRow = row.Index;

						SelectRange(selectStartRow, row.Index, Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL));

						isProcessed = true;
						InvalidateCanvas();
					}
					else
					{
						StyleTableCell cell = row.GetCellByPoint(e.Location);
						if (cell != null)
						{
							if (!Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
							{
								isProcessed = cell.OnMouseMove(e);
							}

							if (!isProcessed)
							{
								Cursor = cell.Define.HoverCursor;
								isProcessed = true;
							}
						}
					}
				}
			}

			if (!isProcessed)
			{
				Cursor = Cursors.Default;
			}

			base.OnMouseMove(e);
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			selectStartRow = -1;
			if (heightChangeRow > -2)
			{
				StyleTableRow row = (heightChangeRow == -1 ? header : rows[heightChangeRow]);
				int newHeight = cellAdjustHotLine - row.Bounds.Top;
				if (row.IsSelected)
				{
					// if current row selected, the hight adjusting
					// should be applied to all selected row
					DoAction(MakeSetSelectedRowsHeightAction(newHeight));
				}
				else
				{
					DoAction(new STSetRowHeightAction(row.Index, newHeight));
				}

				heightChangeRow = -2;
				cellAdjustHotLine = -1;
				Cursor = Cursors.Default;
				//CanvasInvalidate();
			}
			else if (widthChangeColumn > -1)
			{
				DoAction(new STSetColumnWidthAction(widthChangeColumn,
						 cellAdjustHotLine - header.Cells[widthChangeColumn].Left + 1));

				widthChangeColumn = -1;
				cellAdjustHotLine = -1;
				Cursor = Cursors.Default;
				//CanvasInvalidate();
			}
			else if(e.Button == MouseButtons.Right)
			{
				if (header.Bounds.Contains(e.Location))
				{
					StyleTableHeaderCell cellHeader = header.GetCellByPoint(e.Location) as StyleTableHeaderCell;
					if (cellHeader != null)
					{
						if (colDefines.Count > 0)
						{
							if (cellHeader.Index == 0)
							{
								if (colDefines[0].Type == StyleTableColumnType.Index && LeadHeadContextMenuStrip != null)
								{
									LeadHeadContextMenuStrip.Show(this, e.Location);
								}
							}
							else if(HeaderContextMenuStrip!=null)
							{
								HeaderContextMenuStrip.Show(this, e.Location);
							}
						}
					}
				}
				else
				{
					heightChangeRow = -2;

					// find row
					StyleTableRow row = GetRowByPoint(e.Location);

					if (row != null)
					{
						// find cell
						StyleTableCell cell = row.GetCellByPoint(e.Location);
						if (cell != null)
						{
							if (RowContextMenuStrip != null)
							{
								Point p = e.Location;
								p.Offset(canvas.Left, canvas.Top);
								RowContextMenuStrip.Show(this, p);
							}
						}
					}
				}
			}
			else if (focusCell != null)
			{
				if (focusCell.OnMouseUp(e))
				{

					//if (focusCell == GetCellByPoint(e.Location))
					//PerformCellClick(focusCell);

					InvalidateCanvas();
				}
			}

			base.OnMouseUp(e);
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			//canvas.Focus();

			bool isProcessed = false;

			bool isOperating = (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL) || Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT));

			// check for row height can adjust?
			StyleTableRow lineAdjustRow = (!AllowChangeRowHeight || isOperating) ? null : rows.Union(
				new StyleTableRow[] { header }).FirstOrDefault(r =>
					r.Bounds.Bottom - 3 < e.Y && r.Bounds.Bottom + 3 > e.Y);

			if (lineAdjustRow != null
				&& lineAdjustRow.Cells.Count > 0
				&& e.X < lineAdjustRow.Cells[0].Bounds.Right)
			{
				heightChangeRow = lineAdjustRow.Index;
				Cursor = Cursors.HSplit;
				lastPoint = e.Location;
				cellAdjustHotLine = e.Y;
				isProcessed = true;
				if (currentEditingCell != null) EndEdit(StyleTableEditEditReason.Cancel);
				Debug.WriteLine("row height adjust started at: " + heightChangeRow);
				// end of row height mouse region check
			}
			else
			{
				// start of width adjust mouse region checking
				StyleTableCell widthAdjustCell = !AllowChangeColumnWidth ? null :
					(header.Cells.FirstOrDefault(c => c.Bounds.Right - 3 < e.X
						&& c.Bounds.Right + 3 > e.X && c.Bounds.Bottom > e.Y));
				if (widthAdjustCell != null)
				{
					widthChangeColumn = widthAdjustCell.Index;
					Cursor = Cursors.VSplit;
					lastPoint = e.Location;
					cellAdjustHotLine = e.X;
					isProcessed = true;
					if (currentEditingCell != null) EndEdit(StyleTableEditEditReason.Cancel);
					Debug.WriteLine("cell width adjust started at: " + widthChangeColumn);
				}
				// end of width adjust mouse region checking
				else
				{
					if (header.Bounds.Contains(e.Location))
					{
						StyleTableHeaderCell cellHeader = header.GetCellByPoint(e.Location) as StyleTableHeaderCell;
						if (cellHeader != null)
						{
							if (colDefines.Count > 0 && colDefines[0].Type == StyleTableColumnType.Index
								&& cellHeader.Index == 0 && rows.Count > 0)
							{
								SelectRange(0, rows.Count - 1);
								isProcessed = true;
							}
						}
					}
					else
					{
						heightChangeRow = -2;

						// find row
						StyleTableRow row = GetRowByPoint(e.Location);

						if (row != null)
						{
							// find cell
							StyleTableCell cell = row.GetCellByPoint(e.Location);
							if (cell != null )
							{
								SetFocusCell(cell);

								if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
								{
									InvertSelect(focusRow);
									lastSelectedRow = row.Index;
									isProcessed = true;
									selectStartRow = row.Index;
								}
								else if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
								{
									if (lastSelectedRow > -1)
									{
										int startRow = Math.Min(lastSelectedRow, focusRow.Index);
										int endRow = Math.Max(lastSelectedRow, focusRow.Index);
										SelectRows(rows.GetRange(startRow, endRow - startRow + 1));
									}
									else
									{
										SelectRow(focusRow);
										lastSelectedRow = row.Index;
									}
									isProcessed = true;
								}
								else
								{
									// if no event causes, do a selection action to current row and cells
									if (e.Button == MouseButtons.Right && focusRow.IsSelected)
									{
									}
									else
									{
										SelectRow(focusRow);
										lastSelectedRow = row.Index;
									}

									if (e.Button == MouseButtons.Left)
									{
										StyleTableColumnDefine def = colDefines[cell.Index];

										if (def.Type == StyleTableColumnType.Index)
										{
											selectStartRow = row.Index;
											isProcessed = true;
										}
										else 
										{
											isProcessed = cell.OnMouseDown(e);
										}
										
										if (!isProcessed && !cell.InEditing && cell.StartEditOnClick)
										{
											if (def.Dropdown != StyleTableDropdownType.Dropdown && ClickToEdit)
											{
												switch (def.Type)
												{
													case StyleTableColumnType.General:
													case StyleTableColumnType.Datetime:
													case StyleTableColumnType.Numeric:
														isProcessed = StartEdit(row, cell);
														break;
												}
											}
										}
										//else
										//{
										//	isProcessed = cell.OnMouseDown(e);
										//	Debug.WriteLine("enter cell mousedown: " + e.ToString());
										//}
									}
									else
										isProcessed = cell.OnMouseDown(e);
								}
							}
						}
					}
				}
			}

			if (isProcessed) InvalidateCanvas();
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int newValue = vScrollBar.Value - e.Delta;
			if (newValue < vScrollBar.Minimum) newValue = vScrollBar.Minimum;
			if (newValue > vScrollBar.Maximum - vScrollBar.LargeChange)
				newValue = vScrollBar.Maximum - vScrollBar.LargeChange;

			vScrollBar.Value = newValue;
			canvas.Top = -newValue;

			base.OnMouseWheel(e);
		}
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			StyleTableCell cell = FocusCell;
			if (!cell.InEditing)
			{
				StyleTableColumnDefine def = colDefines[cell.Index];

				switch (def.Type)
				{
					case StyleTableColumnType.General:
					case StyleTableColumnType.Datetime:
						if(def.Dropdown != StyleTableDropdownType.Dropdown) StartEdit(cell);
						break;

					case StyleTableColumnType.Numeric:
						StartEdit(cell);
						break;
				}
			}
		}
		#endregion

		#region Keyboard Events
		protected override bool ProcessDialogChar(char charCode)
		{
			if (Toolkit.IsKeyDown(Win32.VKey.VK_MENU)
				|| Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL)
				|| charCode == 13
				|| charCode== (int)Keys.Escape
				|| charCode == '\t')
			{
				return false;
			}

			if (focusRow != null && focusCell != null
				&& StartEdit(focusRow, focusCell))
			{
				switch (currentEditingCell.Define.Type)
				{
					case StyleTableColumnType.General:
						Win32.SendMessage(editTextbox.Handle,
							(uint)Win32.WMessages.WM_CHAR, new IntPtr(charCode), new IntPtr(0));
						break;
	
					case StyleTableColumnType.Numeric:
						Win32.SendMessage(numericTextbox.GetTextboxHandle(),
							(uint)Win32.WMessages.WM_CHAR, new IntPtr(charCode), new IntPtr(0));
						break;
				}
			}

			return true;
		}

		protected virtual void ProcessKeyUp(Keys key)
		{
			if (focusRow != null && focusCell != null)
			{
				if (focusCell.OnKeyUp(key))
				{
					InvalidateCanvas();
				}
			}
			//if (((key & Keys.Enter) > 0 || (key & Keys.Space) > 0)
			//  && (focusRow != null && focusCell != null)
			//  && (focusCell.Define.Type == StyleTableColumnType.Link
			//  || focusCell.Define.Type == StyleTableColumnType.Button))
			//{
			//  focusCell.OnMouseUp(new Point(
			//    focusCell.Left + focusCell.Width / 2,
			//    focusCell.Top + focusCell.Height / 2));

			//  CanvasInvalidate();
			//}

			//Debug.WriteLine("key up: " + key);
		}
	
		protected virtual bool ProcessKeyDown(Keys key)
		{
			bool isProcessed = false;

			if (key == Keys.Escape)
			{
				if (currentEditingCell != null)
				{
					EndEdit(currentEditingCell.Data.ToString());
				}
				return true;
			}

			if (focusRow != null && focusCell != null)
			{
				isProcessed = focusCell.OnKeyDown(key);

				switch (key)
				{
					case Keys.Enter:
						{
							if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
							{
								if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
									MoveFocusCellUp();
								else
									MoveFocusCellDown();
							}
							else
							{
								// forward to next cell
								if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
									MoveFocusCellBackward();
								else
									MoveFocusCellForward();
							}
							isProcessed = true;
						}
						break;
					case Keys.Tab:
						{
							if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
								MoveFocusCellBackward();
							else
								MoveFocusCellForward();
							isProcessed = true;
						}
						break;

					case Keys.Space:
						{
							// control + enter or space = appended selection
							if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL)
								/*&& focusCell.Define.Type == StyleTableColumnType.Index*/)
							{
								InvertSelect(focusRow);
								lastSelectedRow = focusRow.Index;
								isProcessed = true;
							}
							else if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
							{
								if (lastSelectedRow > -1)
								{
									int startRow = Math.Min(lastSelectedRow, focusRow.Index);
									int endRow = Math.Max(lastSelectedRow, focusRow.Index);
									SelectRows(rows.GetRange(startRow, endRow - startRow + 1));
								}
								else
								{
									SelectRow(focusRow);
									lastSelectedRow = focusRow.Index;
								}
								isProcessed = true;
							}

						}
						break;

					case Keys.F2:
						StartEdit(focusRow, focusCell);
						break;

					// up
					case Keys.Up:
						MoveFocusCellUp();
						AfterMoveFocus();
						break;

					// down
					case Keys.Down:
						MoveFocusCellDown();
						AfterMoveFocus();
						break;

					// left
					case Keys.Left:
						MoveFocusCellLeft();
						AfterMoveFocus();
						break;

					// right
					case Keys.Right:
						MoveFocusCellRight();
						AfterMoveFocus();
						break;

					// home
					case Keys.Home:
						if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
							MoveFocusCellTopRow();
						MoveFocusCellFirstCell();
						AfterMoveFocus();
						break;

					// end
					case Keys.End:
						if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
							MoveFocusCellLastRow();
						MoveFocusCellLastCell();
						AfterMoveFocus();
						break;

					// page up
					case Keys.PageUp:
						MoveFocusCellPageUp();
						AfterMoveFocus();
						break;

					// page down
					case Keys.PageDown:
						MoveFocusCellPageDown();
						AfterMoveFocus();
						break;

					case Keys.Z:
						if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
							Undo();
						break;

					case Keys.Y:
						if (Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
							Redo();
						break;
				}
			}
			else if (focusRow != null && focusRow.Cells.Count > 0)
			{
				SetFocusCell(focusRow.Cells[0]);
				SelectFocusRow(false);
			}
			else
			{
				if (rows.Count > 0 && rows[0].Cells.Count > 0)
				{
					SetFocusCell(0, 0);
					SelectFocusRow(false);
				}
			}

			if (isProcessed) InvalidateCanvas();

			return true;
		}

		private void AfterMoveFocus()
		{
			if (Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT))
			{
				SelectRange(focusRow.Index, lastSelectedRow);
			}
			else if (!Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
			{
				SelectFocusRow(false);
				lastSelectedRow = focusRow.Index;
			}
			ScrollToFocusCell();
		}

		#endregion

		#region User Events
		public event EventHandler<StyleTableCellEventArgs> CellClicked;
		public event EventHandler<StyleTableCellEventArgs> FocusCellChanged;
		public event EventHandler ActionPerformed;
		public event EventHandler<CustomStyleTableCellInsertEventArgs> CustomCellInsert;
		#endregion

		#region Cursor & Focus
		internal void MoveFocusCellUp()
		{
			if (focusRow != null && focusCell != null
				&& focusRow.Index > 0)
			{
				SetFocusCell(focusRow.Index - 1, focusCell.Index);
			}
		}

		internal void MoveFocusCellDown()
		{
			if (focusRow != null && focusCell != null
				&& focusRow.Index < rows.Count - 1)
			{
				SetFocusCell(focusRow.Index + 1, focusCell.Index);
			}
		}

		internal void MoveFocusCellLeft()
		{
			if (focusRow != null && focusCell != null
				&& focusCell.Index > 0)
			{
				SetFocusCell(focusRow.Index, focusCell.Index - 1);
			}
		}

		internal void MoveFocusCellRight()
		{
			if (focusRow != null && focusCell != null
				&& focusCell.Index < focusRow.Cells.Count - 1)
			{
				SetFocusCell(focusRow.Index, focusCell.Index + 1);
			}
		}

		internal void MoveFocusCellTopRow()
		{
			if (focusRow != null && focusCell != null)
				SetFocusCell(0, focusCell.Index);
		}

		internal void MoveFocusCellLastRow()
		{
			if (focusRow != null && focusCell != null)
				SetFocusCell(rows.Count - 1, focusCell.Index);
		}

		internal void MoveFocusCellFirstCell()
		{
			if (focusRow != null && focusCell != null)
				SetFocusCell(focusRow.Index, 0);
		}

		internal void MoveFocusCellLastCell()
		{
			if (focusRow != null && focusCell != null)
				SetFocusCell(focusRow.Index, focusRow.Cells.Count - 1);
		}

		internal void MoveFocusCellPageUp()
		{
			if (focusRow != null && focusCell != null
				&& focusRow.Index > 0)
			{
				StyleTableRow nextRow = focusRow;
				int y = 0;
				for (int i = focusRow.Index - 1; i >= 0; i--)
				{
					nextRow = rows[i];
					y += rows[i].Height;
					if (y > ClientRectangle.Height - hScrollBar.Height - 20)
					{
						break;
					}
				}

				SetFocusCell(nextRow.Index, focusCell.Index);
			}
		}
		internal void MoveFocusCellPageDown()
		{
			if (focusRow != null && focusCell != null
				&& focusRow.Index < rows.Count - 1)
			{
				StyleTableRow nextRow = focusRow;
				int y = 0;
				for (int i = focusRow.Index + 1; i < rows.Count; i++)
				{
					nextRow = rows[i];
					y += rows[i].Height;
					if (y > ClientRectangle.Height - hScrollBar.Height - 20)
					{
						break;
					}
				}

				SetFocusCell(nextRow.Index, focusCell.Index);
			}
		}
		internal void MoveFocusToFirstRow()
		{
			if (rows.Count > 0 && rows[0].Cells.Count > 0)
			{
				SetFocusCell(rows[0].Index,
					focusCell == null ? 0 : focusCell.Index);
			}
		}
		internal void MoveFocusToLastRow()
		{
			if (rows.Count > 0 && rows[rows.Count - 1].Cells.Count > 0)
			{
				SetFocusCell(rows.Count - 1,
					focusCell == null ? 0 : focusCell.Index);
			}
		}
		internal void MoveFocusCellForward()
		{
			if (focusRow != null && focusCell != null)
			{
				if (focusCell.Index < focusRow.Cells.Count - 1)
					MoveFocusCellRight();
				else if (focusRow.Index < Rows.Count - 1)
				{
					MoveFocusCellDown();
					MoveFocusCellFirstCell();
				}
				ScrollToFocusCell();
			}
		}
		internal void MoveFocusCellBackward()
		{
			if (focusRow != null && focusCell != null)
			{
				if (focusCell.Index > 0)
					MoveFocusCellLeft();
				else if (focusRow.Index > 0)
				{
					MoveFocusCellUp();
					MoveFocusCellLastCell();
				}
				ScrollToFocusCell();
			}
		}
		#endregion

		#region Utility
		#region Focus & Selection
		private StyleTableCell focusCell;
		public StyleTableCell FocusCell
		{
			get { return focusCell; }
			set { SetFocusCell(value); }
		}

		private StyleTableRow focusRow;
		public StyleTableRow FocusRow
		{
			get { return focusRow; }
			set
			{
				if (value == null)
				{
					SetFocusCell(null);
				}
				else
				{
					SetFocusCell(value.Cells[this.focusCell == null ? 0 : this.focusCell.Index]);
				}
			}
		}

		private int lastSelectedRow = -1;

		public virtual void SelectRow(int rowIndex)
		{
			SelectRow(rows[rowIndex], false);
		}

		public virtual void SelectRow(StyleTableRow row)
		{
			SelectRow(row, false);
		}

		internal virtual void SelectRow(StyleTableRow row, bool isAppended)
		{
			if (isAppended)
			{
				if (!row.IsSelected) row.Cells.ForEach(c => c.IsSelected = true);
			}
			else
			{
				rows.ForEach(r => r.Cells.ForEach(c => c.IsSelected = (row == r)));
			}
		}

		internal virtual void SelectRows(List<StyleTableRow> trows)
		{
			SelectRows(trows, false);
		}

		internal virtual void SelectRows(List<StyleTableRow> rows, bool isAppended)
		{
			if (isAppended)
				rows.ForEach(r => SelectRow(r, true));
			else
			{
				UnselectRows(this.rows);
				rows.ForEach(r => SelectRow(r, true));
			}
		}

		internal virtual void SelectRange(int i1, int i2)
		{
			SelectRange(i1, i2, false);
		}

		internal virtual void SelectRange(int i1, int i2, bool isAppended)
		{
			int startRow = Math.Min(i1, i2);
			int endRow = Math.Max(i1, i2);
			SelectRows(rows.GetRange(startRow, endRow - startRow + 1), isAppended);
		}

		internal virtual void UnselectRows(List<StyleTableRow> rows)
		{
			rows.ForEach(row => row.Cells.ForEach(c => c.IsSelected = false));
		}

		internal virtual void InvertSelect(StyleTableRow row)
		{
			bool isSelected = row.IsSelected;
			row.Cells.ForEach(c => c.IsSelected = !isSelected);
		}

		public virtual void InvertSelect()
		{
			rows.ForEach(r => InvertSelect(r));
		}

		public IEnumerable<StyleTableRow> SelectedRows
		{
			get
			{
				for (int i = 0; i < this.rows.Count; i++)
				{
					var row = this.rows[i];
					if (row.IsSelected) yield return row;
				}
			}
		}

		public virtual void SetFocusCell(int row, int col)
		{
			if (row >= 0 && row < rows.Count)
			{
				StyleTableRow r = rows[row];

				if (col >= 0 && col < r.Cells.Count)
				{
					SetFocusCell(r.Cells[col]);
				}
			}
		}

		private void SelectFocusRow(bool isAppended)
		{
			if (focusRow != null && focusCell != null)
			{
				SelectRow(focusRow, isAppended);
			}
		}

		/// <summary>
		/// Cell that is removed focus temporarily when control loses focus. 
		/// Restore cell focus when control getting focus again.
		/// </summary>
		private StyleTableCell infocusCell;

		internal protected virtual void SetFocusCell(StyleTableCell cell)
		{
			if (focusCell != cell)
			{
				if (focusCell != null)
				{
					focusCell.IsFocus = false;
					focusCell.OnLostFocus();
				}

				focusCell = cell;

				if (cell != null)
				{
					focusRow = cell.Row;
					cell.IsFocus = true;
					cell.OnGotFocus();
				}

				if (FocusCellChanged != null) FocusCellChanged(this, new StyleTableCellEventArgs
				{
					Cell = focusCell,
				});
			}

			this.infocusCell = focusCell;

			InvalidateCanvas();
		}
		#endregion

		#region Scrolling
		internal void ScrollToFocusCell()
		{
			if (focusRow != null && focusCell != null)
			{
				if (hScrollBar.Value + ClientRectangle.Right < focusCell.Right)
				{
					hScrollBar.Value = focusCell.Right -
						ClientRectangle.Right + vScrollBar.Width + 1;
				}
				else if (hScrollBar.Value > focusCell.Left - 1)
				{
					hScrollBar.Value = focusCell.Left - 1;
				}

				if (vScrollBar.Value + ClientRectangle.Bottom < focusCell.Bottom)
				{
					vScrollBar.Value = focusCell.Bottom -
						ClientRectangle.Bottom + hScrollBar.Height + 1;
				}
				else if (vScrollBar.Value > focusCell.Top - 1)
				{
					vScrollBar.Value = focusCell.Top - 1;
				}

				canvas.Left = -hScrollBar.Value;
				canvas.Top = -vScrollBar.Value;
			}
		}
		internal void ScrollUp()
		{
			int value = vScrollBar.Value - 30;
			if (value < vScrollBar.Minimum) value = vScrollBar.Minimum;
			vScrollBar.Value = value;
			canvas.Top = -value;
		}
		internal void ScrollDown()
		{
			int value = vScrollBar.Value + 30;
			if (value > vScrollBar.Maximum) value = vScrollBar.Maximum;
			vScrollBar.Value = value;
			canvas.Top = -value;
		}
		internal void ScrollLeft()
		{
			int value = hScrollBar.Value - 30;
			if (value < hScrollBar.Minimum) value = hScrollBar.Minimum;
			hScrollBar.Value = value;
			canvas.Top = -value;
		}
		internal void ScrollRight()
		{
			int value = hScrollBar.Value + 30;
			if (value > hScrollBar.Maximum) value = hScrollBar.Maximum;
			hScrollBar.Value = value;
			canvas.Top = -value;
		}
		#endregion

		#region Location & UI Updating
		public StyleTableRow GetRowByPoint(Point p)
		{
			return new List<StyleTableRow> { header }.Union(rows).ToList().FirstOrDefault(r => r.Bounds.Contains(p));
		}
		internal StyleTableCell GetCellByPoint(Point p)
		{
			StyleTableRow row = GetRowByPoint(p);
			return row != null ? row.GetCellByPoint(p) : null;
		}
		internal StyleTableRow GetRowByIndex(int row) { return (row >= 0 && row < rows.Count) ? rows[row] : (row == -1 ? header : null); }
		internal Point PointToRow(StyleTableRow row, Point p)
		{
			return new Point(p.X - row.Bounds.Left, p.Y - row.Bounds.Top);
		}
		private void UpdateCanvasSize()
		{
			if (canvas != null)
			{
				canvas.Height = Math.Max(
					(rows.Count > 0 ? rows[rows.Count - 1].Bounds.Bottom : header.Bounds.Bottom),
					ClientRectangle.Height - hScrollBar.Height) + 5;

				canvas.Width = Math.Max(
					header.Bounds.Right + 1, ClientRectangle.Width - vScrollBar.Width) + 5;

				UpdateScrollBar();
			}
		}
		protected virtual void UpdateScrollBar()
		{
			if (canvas != null)
			{
				int width = ClientRectangle.Width - (vScrollBar.Visible ? 18 : 0);
				int height = ClientRectangle.Height - (hScrollBar.Visible ? 18 : 0);

				if (width < 0) width = 0;
				if (height < 0) height = 0;

				vScrollBar.Maximum = canvas.Height;
				vScrollBar.LargeChange = height;
				//	vScrollBar.Visible = !(canvas.Height + canvas.Top < height);

				hScrollBar.Maximum = canvas.Width;
				hScrollBar.LargeChange = width;
				//	hScrollBar.Visible = !(canvas.Width + canvas.Left < width);
			}
		}
		public Point CanvasPointToScreen(Point p)
		{
			return new Point(p.X - this.canvas.Left, p.Y + this.canvas.Top);
		}
		#endregion

		internal void PerformCellClick(StyleTableCell cell)
		{
			if (CellClicked != null)
			{
				CellClicked.Invoke(this, new StyleTableCellEventArgs
				{
					Cell = cell,
				});
			}
		}

		public ActionGroup CreateClearSelectedRowContentsAction()
		{
			ActionGroup ag = new ActionGroup("Clear Rows Content");
			SelectedRows.ToList().ForEach(r => CreateClearRowContentsAction(r));
			return ag;
		}

		public STClearCellContentAction CreateClearRowContentsAction(StyleTableRow row)
		{
			STClearCellContentAction clearAction = new STClearCellContentAction();
			if (row != null) row.Cells.ForEach(c => clearAction.Cells.Add(c));
			return clearAction;
		}

		internal void ResetGrid()
		{
			colDefines.Clear();
			rows.Clear();
			ResetHeader();
		}

		#region Copy, Cut & Paste
		private static readonly string StyleTableClipboardFormatId_Row =
			"{30F04E2E-04E1-43c9-8B46-6C5996E9761C}";
		private static readonly string StyleTableClipboardFormatId_Cell =
			"{1DD9D94E-A10E-4e80-80B9-DD25C2A968FA}";
		public void Copy()
		{
			DataObject data = new DataObject();
			bool fullRowSelected = focusCell.Define.Type == StyleTableColumnType.Index;

			if (fullRowSelected)
			{
				data.SetData(StyleTableClipboardFormatId_Row, SelectedRows.ToList());
				StringBuilder sb = new StringBuilder();
				foreach (StyleTableRow row in SelectedRows)
				{
					foreach (StyleTableCell cell in row.Cells)
					{
						sb.Append(cell.Data.ToString());
						sb.Append(" ");
					}
					sb.AppendLine();
				}
				data.SetText(sb.ToString());
			}
			else
			{
				data.SetText(focusCell.Data.ToString());
				Clipboard.SetText(focusCell.Data.ToString());
			}

			Clipboard.SetDataObject(data);
		}
		public void Cut()
		{
		}
		public bool CanPaste()
		{
			IDataObject data = Clipboard.GetDataObject();
			return data.GetDataPresent(StyleTableClipboardFormatId_Row);
		}
		public void Paste()
		{
			//IDataObject data = Clipboard.GetDataObject();
			DataObject data = Clipboard.GetDataObject() as DataObject;
			if (data != null)
			{
				if (data.GetDataPresent(StyleTableClipboardFormatId_Row))
				{
					List<StyleTableRow> pastedRows = Clipboard.GetData(StyleTableClipboardFormatId_Row) as List<StyleTableRow>;
					if (pastedRows != null && pastedRows.Count > 0)
					{
						if (pastedRows[0].Cells.Count > colDefines.Count)
						{
							if (MessageBox.Show("Clipboard table is larger than current table. the part will be cut. continue?") == DialogResult.Cancel)
							{
								return;
							}
						}

						int pi = 0;
						for (int i = focusRow.Index; i < rows.Count && i < focusRow.Index + pastedRows.Count; i++)
						{
							rows[i].SetDatas(pastedRows[pi++].GetDatas());
						}

						int overage = pastedRows.Count - pi;
						while (overage > 0)
						{
							InsertRow(rows.Count, pastedRows[pastedRows.Count - overage].GetDatas());
						}
					}
				}
				else if (data.GetDataPresent(StyleTableClipboardFormatId_Cell))
				{
					// todo: copy cell
					StyleTableCell pastedCell = data.GetData(StyleTableClipboardFormatId_Cell) as StyleTableCell;
					if (pastedCell != null)
						focusCell.Data = pastedCell.Data;
				}
				else if (data.ContainsText())
				{
					focusCell.Data = data.GetText();
				}
				InvalidateCanvas();
			}
		}
		internal bool CheckEmptyRow(int from, int count)
		{
			foreach (StyleTableRow row in rows.GetRange(from, count))
			{
				foreach (StyleTableCell cell in row.Cells)
				{
					if (!object.Equals(cell.Data, cell.Define.DefaultValue))
						return false;
				}
			}
			return true;
		}
		public virtual bool CanPasteInsert()
		{
			DataObject data = Clipboard.GetDataObject() as DataObject;
			return (data != null) && (data.GetDataPresent(StyleTableClipboardFormatId_Row));
		}
		public void PasteInsert()
		{
			DataObject data = Clipboard.GetDataObject() as DataObject;
			if (data != null)
			{
				if (data.GetDataPresent(StyleTableClipboardFormatId_Row))
				{
					List<StyleTableRow> pastedRows = Clipboard.GetData(StyleTableClipboardFormatId_Row) as List<StyleTableRow>;
					if (pastedRows != null && pastedRows.Count > 0)
					{
						pastedRows.Reverse();
						pastedRows.ToList().ForEach(r => InsertRow(focusRow.Index, r));
						SelectRows(pastedRows);

						InvalidateCanvas();
					}
				}
			}
		}
		#endregion

		#region Undo & Redo
		public bool CanUndo()
		{
			return ActionManager.InstanceForObject(this).CanUndo();
		}
		public bool CanRedo()
		{
			return ActionManager.InstanceForObject(this).CanRedo();
		}
		public void Undo()
		{
			ActionManager.InstanceForObject(this).Undo();
			InvalidateCanvas();
		}
		public void Redo()
		{
			ActionManager.InstanceForObject(this).Redo();
			InvalidateCanvas();
		}
		public void DoAction(STAction action)
		{
			// do not STSetCellValueAction when readonly mode
			if (Readonly && action is STSetCellValueAction) return;

			if (action.Grid != null && action.Grid != this)
				throw new STOtherGridsActionException(action);

			action.Grid = this;
			ActionManager.DoAction(this, action);

			if (ActionPerformed != null) ActionPerformed(this, new STActionPerformedEventArgs { Action = action });
			InvalidateCanvas();
		}
		public void DoAction(ActionGroup ag)
		{
			foreach (STAction action in ag.Actions)
			{
				if (action.Grid != null && action.Grid != this)
					throw new STOtherGridsActionException(action);
				else
					action.Grid = this;
			}

			ActionManager.DoAction(this, ag);

			if (ActionPerformed != null) ActionPerformed(this, new STActionPerformedEventArgs { Action = ag });
			InvalidateCanvas();
		}
		#endregion

		#region Save & Load
		public void SaveTable(Stream s)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(STXmlGrid));
			xmlSerializer.Serialize(s, ConvertGridToXml(this));
		}
		public void LoadTable(Stream s)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(STXmlGrid));
			STXmlGrid xmlGrid = xmlSerializer.Deserialize(s) as STXmlGrid;
			ConvertGridFromXml(xmlGrid, this);
			SetFocusCell(0, 0);
			InvalidateCanvas();
		}
		internal static STXmlGrid ConvertGridToXml(StyleTableControl sgc)
		{
			STXmlGrid xmlGrid = new STXmlGrid();
			xmlGrid.ColDefines = new List<STXmlColDefine>();
			sgc.colDefines.ToList().ForEach(def =>
				xmlGrid.ColDefines.Add(new STXmlColDefine
				{
					DefaultValue = def.DefaultValue.ToString(),
					Text = def.HeaderText,
					Width = def.Width,
					ColumnType = ConvertColumnTypeToString(def.Type),
					HeaderAlign = ConvertContentAlignmentToAlignString(def.HeaderAlignment),
					CellAlign = ConvertContentAlignmentToAlignString(def.CellAlignment),
					BackColor = XmlFileFormatHelper.EncodeColor(def.BackgroundColor),
				}));
			xmlGrid.Rows = new List<STXmlRow>();
			Action<StyleTableRow> AddRow = (r) =>
			{
				STXmlRow row = new STXmlRow { Height = r.Height };
				row.Cells = new List<STXmlCell>();
				r.Cells.ForEach(c => row.Cells.Add(new STXmlCell { Data = c.Data.ToString() }));
				xmlGrid.Rows.Add(row);
			};
			AddRow(sgc.header);
			sgc.rows.ForEach(r => AddRow(r));
			return xmlGrid;
		}
		private static string ConvertContentAlignmentToAlignString(ContentAlignment ca)
		{
			switch (ca)
			{
				default:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					return "left";
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					return "center";
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					return "right";
			}
		}
		private static ContentAlignment ConvertContentAlignmentFromAlignString(string align)
		{
			switch (align.ToLower())
			{
				default:
				case "left":
					return ContentAlignment.MiddleLeft;
				case "center":
					return ContentAlignment.MiddleCenter;
				case "right":
					return ContentAlignment.MiddleRight;
			}
		}
		private static string ConvertColumnTypeToString(StyleTableColumnType colType)
		{
			switch (colType)
			{
				case StyleTableColumnType.Index:
					return "index";
				default:
				case StyleTableColumnType.Static:
					return "static";
				case StyleTableColumnType.General:
					return "general";
				case StyleTableColumnType.Button:
					return "button";
				case StyleTableColumnType.Checkbox:
					return "checkbox";
				case StyleTableColumnType.Link:
					return "link";
				case StyleTableColumnType.Numeric:
					return "numeric";
				case StyleTableColumnType.Datetime:
					return "datetime";
				case StyleTableColumnType.Custom:
					return "custom";
			}
		}
		private static string ConvertDropdownTypeToString(StyleTableDropdownType dropdownType)
		{
			switch (dropdownType)
			{
				default:
				case StyleTableDropdownType.Dropdown:
					return "dropdown";
				case StyleTableDropdownType.Combo:
					return "combo";
			}
		}
		private static StyleTableColumnType ConvertColumnTypeFromString(string str)
		{
			switch (str.ToLower())
			{
				case "index":
					return StyleTableColumnType.Index;
				default:
				case "static":
					return StyleTableColumnType.Static;
				case "general":
					return StyleTableColumnType.General;
				case "button":
					return StyleTableColumnType.Button;
				case "checkbox":
					return StyleTableColumnType.Checkbox;
				case "link":
					return StyleTableColumnType.Link;
				case "numeric":
					return StyleTableColumnType.Numeric;
				case "datetime":
					return StyleTableColumnType.Datetime;
				case "custom":
					return StyleTableColumnType.Custom;
			}
		}
		private static StyleTableDropdownType ConvertDropdownTypeFromString(string type)
		{
			switch (type)
			{
				default:
				case "dropdown":
					return StyleTableDropdownType.Dropdown;
				case "combo":
					return StyleTableDropdownType.Combo;
			}
		}
		internal static bool ConvertGridFromXml(STXmlGrid xmlGrid, StyleTableControl sgc)
		{
			if (xmlGrid == null)
				return false;
			else
			{
				sgc.ResetGrid();

				xmlGrid.ColDefines.ForEach(def =>
					sgc.AddColumnDefine(new StyleTableColumnDefine
					{
						DefaultValue = def.DefaultValue,
						HeaderText = def.Text,
						Width = def.Width,
						Type = ConvertColumnTypeFromString(def.ColumnType),
						HeaderAlignment = ConvertContentAlignmentFromAlignString(def.HeaderAlign),
						CellAlignment = ConvertContentAlignmentFromAlignString(def.CellAlign),
						BackgroundColor = XmlFileFormatHelper.DecodeColor(def.BackColor),
					}));

				if (xmlGrid.Rows.Count > 0)
				{
					sgc.header.Height = xmlGrid.Rows[0].Height;
					sgc.header.UpdateUI();

					xmlGrid.Rows.GetRange(1, xmlGrid.Rows.Count - 1).ForEach(r =>
						{
							StyleTableRow row = sgc.CreateRow();
							r.Cells.ForEach(c => row.AddCell(c.Data));
							sgc.AddRow(row);
						});
				}
				else
				{
					// if there is no row has been added. create an empty row.
					sgc.AddEmptyRow(1);
				}

				return true;
			}
		}
		#endregion

		#endregion

		#region System Override
		protected override void OnResize(EventArgs e)
		{
			UpdateCanvasSize();
			base.OnResize(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateScrollBar();
		}

		protected override void OnMove(EventArgs e)
		{
			if (this.currentEditingCell is DropdownCell)
			{
				var dropdownCell = (DropdownCell)(this.currentEditingCell);

				if(dropdownCell.IsDropdown)
				{
					dropdownCell.PullUp();
				}
			}
		}
		#endregion

		#region Height & Width Adjusting
		private int heightChangeRow = -2;
		private int widthChangeColumn = -1;
		private Point lastPoint = Point.Empty;
		private int cellAdjustHotLine = -1;

		[DefaultValue(true)]
		public bool AllowChangeColumnWidth { get; set; }
		[DefaultValue(true)]
		public bool AllowChangeRowHeight { get; set; }

		internal void SetRowHeight(int row, int height)
		{
			if (row < -1 || row > rows.Count - 1) return;
			SetRowHeight(row == -1 ? header : rows[row], height);
		}
		protected virtual void SetRowHeight(StyleTableRow row, int height)
		{
			if (height < 0) height = 0;
			int h = height - row.Bounds.Height;

			row.Cells.ForEach(c => { c.Height = height; c.UpdateUI(); });
			row.Height = height;

			for (int i = row.Index + 1; i < rows.Count; i++)
				OffsetRow(rows[i], h);

			UpdateCanvasSize();
		}
		protected virtual void OffsetRow(StyleTableRow row, int offset)
		{
			row.Cells.ForEach(c => { c.Top += offset; c.UpdateUI(); });
			row.Top += offset;
		}
		internal void SetColumnWidth(int col, int width)
		{
			if (col < 0 || col > colDefines.Count) return;
			if (width < 0) width = 0;

			colDefines[col].Width = width;

			int w = width - header.Cells[col].Width;
			rows.Union(new StyleTableRow[] { header }).ToList()
				.ForEach(r => SetRowColumnWidth(r, col, w, width));

			UpdateCanvasSize();
			Debug.WriteLine("width adjust w:" + w + " width:" + width);
		}
		protected void SetRowColumnWidth(StyleTableRow row, int col, int offset, int width)
		{
			row.Cells[col].Width = width;
			row.Cells[col].UpdateUI();

			for (int i = col + 1; i < colDefines.Count; i++)
			{
				row.Cells[i].Left += offset;
				row.Cells[i].UpdateUI();
			}

			row.Width += offset;
		}
		internal virtual int ActualHeight
		{ get { return header.Height + (rows.Sum(r => r.Height)); } }
		internal ActionGroup MakeSetSelectedRowsHeightAction(int height)
		{
			ActionGroup ag = new ActionGroup("Set Rows Height: " + height);
			foreach (StyleTableRow row in SelectedRows)
			{
				ag.Actions.Add(new STSetRowHeightAction(row.Index, height));
			}
			return ag;
		}
		internal bool ShowRowHeightSettingForm()
		{
			if (focusRow == null) return false;
			StyleTableRowHeightForm form = new StyleTableRowHeightForm();
			form.RowHeight = focusRow.Height;
			Point p = Point.Empty;
			Win32.GetCursorPos(ref p);
			p.Offset(-115, -63);
			form.Location = p;
			form.StartPosition = FormStartPosition.Manual;
			if (form.ShowDialog(this) == DialogResult.OK)
			{
				DoAction(MakeSetSelectedRowsHeightAction(form.RowHeight));
				return true;
			}
			else
				return false;
		}
		#endregion

		#region Cell Editing

		private StyleTableRow currentEditingRow;
		private StyleTableCell currentEditingCell;
		private readonly InputTextBox editTextbox;
		//private readonly DropdownWindow dropWindow;
		private readonly NumericField numericTextbox;

		internal InputTextBox EditTextbox { get { return editTextbox; } }

		[DefaultValue(false)]
		public bool Readonly { get; set; }

		[DefaultValue(true)]
		public bool ClickToEdit { get; set; }

		public event EventHandler<StyleTableCellBeforeEditEventArgs> BeforeCellEdit;
		public event EventHandler<StyleTableCellAfterEditEventArgs> AfterCellEdit;

		private object backupData;
		internal object BackupData { get { return backupData; } set { backupData = value; } }

		internal bool StartEdit(StyleTableCell cell)
		{
			return StartEdit(cell.Row, cell);
		}

		internal bool StartEdit(StyleTableRow row, StyleTableCell cell)
		{
			if (Readonly) return false;

			if (this.currentEditingCell == cell)
				return true;
			else if (currentEditingCell != null) EndEdit(editTextbox.Text);

			ScrollToFocusCell();

			string[] candidateItems = cell.Define.Candidates;
			if (BeforeCellEdit != null)
			{
				StyleTableCellAfterEditEventArgs args = new StyleTableCellAfterEditEventArgs()
				{
					Cell = cell,
				};
				BeforeCellEdit(this, args);
				candidateItems = args.CandidateItems;
				if (args.IsCancelled)
					return false;
			}

			cell.InEditing = true;

			var def = cell.Define;

			if (def.Type == StyleTableColumnType.General
				&& def.Dropdown != StyleTableDropdownType.Dropdown)
			{
				editTextbox.Text = ((DropDownListCell)cell).DataToString();
				editTextbox.Location = cell.TextBounds.Location;
				editTextbox.Size = cell.TextBounds.Size;
				editTextbox.Multiline = true;
				editTextbox.Visible = true;
				editTextbox.Focus();
				editTextbox.SelectAll();
				editTextbox.Capture = true;
			}
			else if (def.Type == StyleTableColumnType.Numeric)
			{
				if (currentEditingCell != null) EndEdit(numericTextbox.GetValue());
				numericTextbox.SetValue(cell.Data);
				numericTextbox.Location = cell.TextBounds.Location;
				numericTextbox.Size = cell.TextBounds.Size;
				numericTextbox.Visible = true;
				numericTextbox.SelectAll();
			}

			backupData = cell.Data;
			cell.OnStartEdit();
			currentEditingRow = row;
			currentEditingCell = cell;

			return true;
		}

		internal virtual void EndEdit(object data)
		{
			EndEdit(data, StyleTableEditEditReason.NormalFinish);
		}

		internal virtual void EndEdit(StyleTableEditEditReason reason)
		{
			EndEdit(null, reason);
		}

		internal virtual void EndEdit(object data, StyleTableEditEditReason reason)
		{
			if (currentEditingCell == null) return;

			if (AfterCellEdit != null)
			{
				StyleTableCellAfterEditEventArgs arg = new StyleTableCellAfterEditEventArgs()
				{
					Cell = currentEditingCell,
					EndReason = reason,
					NewData = data,
				};
				AfterCellEdit(this, arg);
				data = arg.NewData;
				reason = arg.EndReason;
			}

			if (reason == StyleTableEditEditReason.Cancel)
			{
				currentEditingCell.Data = backupData;
			}
			else if (reason == StyleTableEditEditReason.NormalFinish)
			{
				if (data == null)
				{
					switch (this.currentEditingCell.Define.Type)
					{
						case StyleTableColumnType.General:
							data = ((DropDownListCell)currentEditingCell).StringToData(editTextbox.Text);
							break;

						case StyleTableColumnType.Numeric:
							data = numericTextbox.GetValue();
							break;
					}
				}

				if (!object.Equals(data, backupData))
				{
					DoAction(new STSetCellValueAction(currentEditingCell.Row.Index, currentEditingCell.Index, data));
				}
			}

			currentEditingCell.OnEndEdit();
			currentEditingCell.InEditing = false;
			currentEditingCell = null;

			editTextbox.Visible = false;
			numericTextbox.Visible = false;

			canvas.Focus();
			InvalidateCanvas();
		}

		#region EditControl - Textbox
		internal class InputTextBox : TextBox
		{
			private StyleTableControl owner;

			internal InputTextBox(StyleTableControl owner)
				: base() 
			{
				this.owner = owner;
			}

			protected override void OnLostFocus(EventArgs e)
			{
				base.OnLostFocus(e);

				if (owner.currentEditingCell is DropdownCell)
				{
					var dropdownCell = (DropdownCell)owner.currentEditingCell;

					if (dropdownCell.CurrentDropdownWindow != null)
					{
						IntPtr hwnd = Win32.GetFocus();
						if (hwnd != dropdownCell.CurrentDropdownWindow.Handle
						&& dropdownCell.IsDropdown)
						{
							dropdownCell.PullUp();
							this.owner.InvalidateCanvas();
						}
					}
				}
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				if (owner.currentEditingCell is DropdownCell)
				{
					var dropdownCell = (DropdownCell)owner.currentEditingCell;

					if (dropdownCell.CurrentDropdownWindow != null)
					{
						IntPtr hwnd = Win32.WindowFromPoint(PointToScreen(e.Location));
						if (hwnd != this.Handle && hwnd != dropdownCell.CurrentDropdownWindow.Handle)
						{
							dropdownCell.PullUp();
							this.owner.InvalidateCanvas();
						}
					}
				}
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if (owner.currentEditingCell != null && Visible)
				{
					if (!Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL) && e.KeyCode == Keys.Enter)
					{
						e.SuppressKeyPress = true;
						owner.EndEdit(Text);
						owner.MoveFocusCellForward();
					}
					else if (e.KeyCode == Keys.Escape)
					{
						e.SuppressKeyPress = true;
						owner.EndEdit(StyleTableEditEditReason.Cancel);
					}
				}
			}
		}
		#endregion

		#region EditControl - NumericField
		private class NumericField : Control
		{
			private NumericTextbox textbox = new NumericTextbox
			{
				BorderStyle = BorderStyle.None,
				TextAlign = HorizontalAlignment.Right,
				Visible = false,
			};
			private class NumericTextbox : TextBox
			{
				private static readonly string validChars = "0123456789-.";
				protected override bool IsInputChar(char charCode)
				{
					return charCode == '\b' || false;
				}
				protected override bool ProcessDialogChar(char charCode)
				{
					return validChars.IndexOf(charCode) < 0;
				}
			}

			private StyleTableControl owner;
			private Timer timer;
			public NumericField(StyleTableControl owner)
			{
				this.owner = owner;
				timer = new Timer();
				timer.Tick += new EventHandler(timer_Tick);
				timer.Enabled = false;

				TabStop = false;
				DoubleBuffered = true;

				textbox.KeyDown += new KeyEventHandler(textbox_KeyDown);
				textbox.MouseUp += new MouseEventHandler(textbox_MouseUp);
				Controls.Add(textbox);
			}

			void textbox_MouseUp(object sender, MouseEventArgs e)
			{
				OnMouseUp(e);
			}
			void textbox_KeyDown(object sender, KeyEventArgs e)
			{
				if (Visible && e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
				{
					e.SuppressKeyPress = true;
					owner.EndEdit(GetValue());
					owner.MoveFocusCellForward();
				}
				else if (e.KeyCode == Keys.Escape)
				{
					e.SuppressKeyPress = true;
					owner.EndEdit(backupData);
				}
				else if (e.KeyCode == Keys.Up)
				{
					ValueAdd(Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT) ? 10 :
						(Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL) ? 100 : 1));
					e.SuppressKeyPress = true;
				}
				else if (e.KeyCode == Keys.Down)
				{
					ValueSub(Toolkit.IsKeyDown(Win32.VKey.VK_SHIFT) ? 10 :
						(Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL) ? 100 : 1));
					e.SuppressKeyPress = true;
				}
				else if (e.KeyCode == Keys.PageUp)
				{
					ValueAdd(10);
					e.SuppressKeyPress = true;
				}
				else if (e.KeyCode == Keys.PageDown)
				{
					ValueSub(10);
					e.SuppressKeyPress = true;
				}
				else if (e.KeyCode == Keys.V
					&& Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
				{
					textbox.Paste();
				}
				else if (e.KeyCode == Keys.C
					&& Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
				{
					textbox.Copy();
				}
				else if (e.KeyCode == Keys.X
					&& Toolkit.IsKeyDown(Win32.VKey.VK_CONTROL))
				{
					textbox.Cut();
				}
				//else if ((e.KeyValue & (int)Keys.LButton) > 0
				//  || (e.KeyValue & (int)Keys.RButton) > 0)
				//{
				//}
				//else if ((e.KeyValue < '0' || e.KeyValue > '9') && e.KeyCode != Keys.Back)
				//{
				//  e.SuppressKeyPress = true;
				//}
			}

			void timer_Tick(object sender, EventArgs e)
			{
				if (isUpPressed)
					ValueAdd(1);
				else if (isDownPressed)
					ValueSub(1);
				timer.Interval = 50;
			}
			object backupData;
			internal void SetValue(object data)
			{
				backupData = data;
				int value = 0;
				if (data is int)
					value = (int)data;
				else if (data is string)
					int.TryParse(data as string, out value);
				textbox.Text = value.ToString();
			}
			internal object GetValue()
			{
				int value = 0;
				int.TryParse(textbox.Text as string, out value);
				return value;
			}
			private static readonly int buttonSize = 17;
			private static readonly int arrowSize = 9;
			private bool isUpPressed = false;
			private bool isDownPressed = false;

			protected override void OnVisibleChanged(EventArgs e)
			{
				base.OnVisibleChanged(e);
				if (Visible)
				{
					textbox.Visible = true;
					textbox.Focus();
				}
				else
				{
					owner.EndEdit(GetValue());
				}
			}
			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);

				int hh = textbox.Height / 2;
				textbox.Bounds = new Rectangle(ClientRectangle.Left,
					ClientRectangle.Top + ClientRectangle.Height / 2 - hh - 1,
					ClientRectangle.Width - buttonSize - 1, textbox.Height);
			}
			protected override void OnPaint(PaintEventArgs e)
			{
				Graphics g = e.Graphics;

				int hh = Bounds.Height / 2 - 1;

				Rectangle rect = ClientRectangle;

				Rectangle upRect = new Rectangle(rect.Right - buttonSize, rect.Top, buttonSize, hh);
				GraphicsToolkit.Draw3DButton(g, upRect, isUpPressed);
				GraphicsToolkit.FillTriangle(g, arrowSize,
					new Point(upRect.Left + buttonSize / 2,
						upRect.Top + hh / 2 + (isUpPressed ? 2 : 1)),
					GraphicsToolkit.TriangleDirection.Up);

				Rectangle downRect = new Rectangle(rect.Right - buttonSize, rect.Top + hh + 1, buttonSize, hh);
				GraphicsToolkit.Draw3DButton(g, downRect, isDownPressed);
				GraphicsToolkit.FillTriangle(g, arrowSize,
					new Point(downRect.Left + buttonSize / 2 ,
						downRect.Top + hh / 2 - (isDownPressed ? 1 : 2)),
					GraphicsToolkit.TriangleDirection.Down);
			}

			internal void ValueAdd(int d)
			{
				int value = 0;
				int.TryParse(textbox.Text, out value);
				value += d;
				textbox.Text = value.ToString();
				textbox.SelectAll();
			}
			internal void ValueSub(int d)
			{
				int value = 0;
				int.TryParse(textbox.Text, out value);
				value -= d;
				textbox.Text = value.ToString();
				textbox.SelectAll();
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				int hh = Bounds.Height / 2 - 1;

				Rectangle upRect = new Rectangle(ClientRectangle.Right - buttonSize, ClientRectangle.Top, buttonSize, hh);
				Rectangle downRect = new Rectangle(ClientRectangle.Right - buttonSize, ClientRectangle.Top + hh + 1, buttonSize, hh);

				if (upRect.Contains(e.Location))
				{
					textbox.Capture = true;
					isUpPressed = true;
					ValueAdd(1);
					timer.Interval = 600;
					timer.Start();
					Invalidate();
				}
				else if (downRect.Contains(e.Location))
				{
					textbox.Capture = true;
					isDownPressed = true;
					ValueSub(1);
					timer.Interval = 600;
					timer.Start();
					Invalidate();
				}
			}
			protected override void OnMouseUp(MouseEventArgs e)
			{
				isUpPressed = false;
				isDownPressed = false;
				timer.Stop();
				Invalidate();
			}
			protected override void OnMouseMove(MouseEventArgs e)
			{
				Cursor = Cursors.Default;
				base.OnMouseMove(e);
			}

			internal int GetNumericValue()
			{
				int num = 0;
				int.TryParse(textbox.Text, out num);
				return num;
			}
			internal void SelectAll()
			{
				textbox.SelectAll();
			}

			protected override void OnGotFocus(EventArgs e)
			{
				base.OnGotFocus(e);
				textbox.Focus();
			}
			internal IntPtr GetTextboxHandle()
			{
				return textbox.Handle;
			}
		}
		#endregion

		#region EditControl - Dropdown
		/*
		private class DropdownWindow : ToolStripDropDown
		{
			private StyleTableControl owner;
			private DropdownListbox listbox;
			private ToolStripControlHost controlHost;
			internal DropdownWindow(StyleTableControl owner)
				: base()
			{
				this.owner = owner;
				listbox = new DropdownListbox();
				listbox.Dock = DockStyle.Fill;
				listbox.BorderStyle = BorderStyle.None;
				BackColor = listbox.BackColor;
				AutoSize = false;
				TabStop = true;
				Items.Add(controlHost = new ToolStripControlHost(listbox));
				controlHost.Margin = controlHost.Padding = new Padding(0);
				controlHost.AutoSize = false;
				listbox.MouseMove += (sender, e) => listbox.SelectedIndex = listbox.IndexFromPoint(e.Location);
				listbox.Click += new EventHandler(listbox_Click);
				listbox.KeyDown += new KeyEventHandler(listbox_KeyDown);
			}
			private class DropdownListbox : ListBox
			{
				protected override bool ProcessDialogKey(Keys keyData)
				{
					if (keyData == Keys.Escape)
						return false;
					else
						return base.ProcessDialogKey(keyData);
				}
			}
			protected override void OnVisibleChanged(EventArgs e)
			{
				base.OnVisibleChanged(e);
				if (!Visible)
				{
					owner.EndEdit(StyleTableEditEditReason.Cancel);
				}
			}
			internal ListBox ListBox { get { return listbox; } }
			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
				controlHost.Size = new Size(ClientRectangle.Width - 2, ClientRectangle.Height - 2);
			}
			void listbox_KeyDown(object sender, KeyEventArgs e)
			{
				if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
					&& owner.currentEditingCell != null && Visible)
				{
					owner.EndEdit(listbox.SelectedItem != null ? listbox.SelectedItem.ToString() : string.Empty);
					if (e.KeyCode == Keys.Enter) owner.MoveFocusCellForward();
				}
				else if (e.KeyCode == Keys.Escape
					&& owner.currentEditingCell != null && Visible)
				{
					owner.EndEdit(StyleTableEditEditReason.Cancel);
				}
			}
			void listbox_Click(object sender, EventArgs e)
			{
				if (Visible)
				{
					if (listbox.SelectedItem == null)
						owner.EndEdit(StyleTableEditEditReason.Cancel);
					else
						owner.EndEdit(listbox.SelectedItem.ToString());
				}
			}
			private string orginalText;
			internal void SetItems(string[] items, string defaultSelected)
			{
				orginalText = defaultSelected;
				listbox.Items.Clear();
				listbox.Items.AddRange(items);
				Height = listbox.Font.Height * items.Count() + 10;
				listbox.SelectedItem = defaultSelected;
			}
			internal IntPtr GetListboxHandle()
			{
				return listbox.Handle;
			}
		}
		*/
		#endregion

		#endregion

		#region Canvas
		private StyleTableCanvas canvas;
		private class StyleTableCanvas : Control
		{
			private StyleTableControl owner;

			public StyleTableCanvas(StyleTableControl owner)
			{ this.owner = owner; DoubleBuffered = true; TabStop = true; }
			public override Size GetPreferredSize(Size proposedSize)
			{ return new Size(0, 0); }

			protected override void OnPaint(PaintEventArgs e)
			{ owner.DrawCanvas(e.Graphics, e.ClipRectangle); }

			protected override void OnMouseUp(MouseEventArgs e)
			{
				owner.OnMouseUp(e);
			}
			protected override void OnMouseDown(MouseEventArgs e)
			{
				owner.OnMouseDown(e);
			}
			protected override void OnMouseMove(MouseEventArgs e)
			{
				owner.OnMouseMove(e);
			}
			protected override void OnMouseDoubleClick(MouseEventArgs e)
			{
				owner.OnMouseDoubleClick(e);
			}
			protected override void OnKeyUp(KeyEventArgs e)
			{
				owner.ProcessKeyUp(e.KeyCode);
			}
			protected override void OnKeyDown(KeyEventArgs e)
			{
				owner.ProcessKeyDown(e.KeyCode);
			}
			protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
			{
				return false;
			}
			protected override bool ProcessDialogKey(Keys keyData)
			{
				return false;
			}
			protected override void OnGotFocus(EventArgs e)
			{
				base.OnGotFocus(e);

				if (owner.focusCell == null)
				{
					if (owner.infocusCell != null)
					{
						owner.SetFocusCell(owner.infocusCell);
					}
					else if (owner.rows.Count > 0 && owner.ColDefines.Count > 0)
					{
						owner.SetFocusCell(0, 0);
					}
				}
			}
			protected override void OnLostFocus(EventArgs e)
			{
				base.OnLostFocus(e);

				if (owner.currentEditingCell is DropdownCell)
				{
					var dropdownCell = (DropdownCell)owner.currentEditingCell;

					IntPtr hwnd = Win32.GetFocus();
					if ((dropdownCell.CurrentDropdownWindow == null
						|| hwnd != dropdownCell.CurrentDropdownWindow.Handle)
						&& dropdownCell.IsDropdown)
					{
						dropdownCell.PullUp();
						this.owner.InvalidateCanvas();
					}
				}
			}
			protected override void WndProc(ref Message m)
			{
				if (m.Msg == (int)Win32.WMessages.WM_IME_COMPOSITION)
				{
					if (owner.focusCell != null && owner.focusCell.Define.Type == StyleTableColumnType.General)
					{
						owner.StartEdit(owner.focusCell);
					}
				}
				base.WndProc(ref m);
			}
		}
		#endregion

		#region Scroll bars
		private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			canvas.Top = -e.NewValue;
		}

		private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			canvas.Left = -e.NewValue;
		}
		#endregion
	}
	#endregion

	#region Enums
	public enum StyleTableColumnType
	{
		Static,			// just display 
		Index,			// an index of row
		General,		// text/value edit with an optional list candidates window
		Datetime,		// datetime input with an optional list candidates window
		Button,			// button
		Link,				// hyperlink
		Checkbox,		// checkbox
		Numeric,		// numeric edit with up/down buttons
		Custom,			// custom control
	}
	public enum StyleTableDropdownType { 
		None,
		Dropdown,
		Combo,
	}
	public enum StyleTableEditEditReason
	{
		NormalFinish,
		Cancel,
	}
	#endregion

	#region ColumnDefine
	[Serializable]
	public class StyleTableColumnDefine
	{
		[DefaultValue("Header")]
		public string HeaderText { get; set; }
		
		public Color BackgroundColor { get; set; }

		public bool IsReadonly { get; set; }

		public StyleTableColumnType Type { get; set; }

		public Type CustomCellType { get; set; }

		public Cursor HoverCursor { get; set; }

		private bool directEdit = true;
		[DefaultValue(true)]
		public bool DirectEdit { get { return directEdit; } set { directEdit = value; } }

		public StyleTableDropdownType Dropdown { get; set; }

		private int width = 80;
		[DefaultValue(80)]
		public int Width { get { return width; } set { width = value; } }

		public string[] Candidates { get; set; }

		private ContentAlignment cellAlignment = ContentAlignment.MiddleLeft;
		public ContentAlignment CellAlignment { get { return cellAlignment; } set { cellAlignment = value; } }

		private ContentAlignment headerAlignment = ContentAlignment.MiddleLeft;
		public ContentAlignment HeaderAlignment { get { return headerAlignment; } set { headerAlignment = value; } }

		private object defaultValue = string.Empty;
		public object DefaultValue { get { return defaultValue; } set { defaultValue = value; } }

		public string Pattern { get; set; }
		public CultureInfo CultureInfo { get; set; }
	}
	#endregion

	#region Row
	[Serializable]
	public class StyleTableRow
	{
		[NonSerialized]
		private StyleTableControl grid;
		public StyleTableControl Grid { get { return grid; } set { grid = value; } }

		private List<StyleTableCell> cells = new List<StyleTableCell>();
		public List<StyleTableCell> Cells { get { return cells; } set { cells = value; } }

		private int index;
		public int Index { get { return index; } set { index = value; } }

		private Rectangle bounds;
		public Rectangle Bounds { get { return bounds; } set { bounds = value; } }

		internal int Width { get { return Bounds.Width; } set { bounds.Width = value; } }
		internal int Height { get { return Bounds.Height; } set { bounds.Height = value; } }
		internal int Left { get { return Bounds.Left; } set { bounds.X = value; } }
		internal int Top { get { return Bounds.Top; } set { bounds.Y = value; } }

		private bool isReadonly;
		internal bool IsReadonly { get { return isReadonly; } set { isReadonly = value; } }

		internal bool IsSelected { get { return cells.FirstOrDefault(c => !c.IsSelected) == null; } }

		private Color backgroundColor;
		internal Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; } }

		internal StyleTableCell AddCell(object data)
		{
			return InsertCell(cells.Count, data);
		}
		internal StyleTableCell InsertCell(int index, object data)
		{
			StyleTableColumnDefine def = grid.ColDefines[index];
			StyleTableCell cell = grid.CreateCell(def);
			if (data != null) cell.Data = data;
			InsertCell(cell, index, def);
			return cell;
		}
		internal void InsertCell(StyleTableCell cell, int index, StyleTableColumnDefine def)
		{
			cell.Index = index;
			cell.Row = this;

			int left = bounds.Left;
			for (int i = 0; i < index && i < cells.Count; i++)
				left += cells[i].Bounds.Width + 1;
			cell.Bounds = new Rectangle(left + 1, this.Bounds.Top, def.Width, this.Bounds.Height);
			cells.Insert(index, cell);
			cell.UpdateUI();
			for (int i = index + 1; i < cells.Count; i++)
			{
				cells[i].Bounds = new Rectangle(cells[i].Bounds.X + def.Width + 1, cells[i].Bounds.Y,
					cells[i].Bounds.Width, cells[i].Bounds.Height);//.Offset(def.Width, 0);
				cells[i].UpdateUI();
				cells[i].Index++;
			}
			bounds.Width += def.Width + 1;
		}
		internal StyleTableCell GetCellByPoint(Point p)
		{
			return cells.FirstOrDefault(c => c.HitTest(p));
		}

		internal void UpdateUI()
		{
			foreach (StyleTableCell cell in cells)
			{
				cell.Bounds = new Rectangle(cell.Bounds.Left, bounds.Top, cell.Bounds.Width, bounds.Height);
				cell.UpdateUI();
			}
		}

		public void ClearCellsContents()
		{
			cells.ForEach(c => c.ClearContent());
		}
		public virtual object[] GetDatas()
		{
			return (from c in cells select c.Data).ToArray();
		}
		public virtual void SetDatas(object[] objs)
		{
			for (int i = 0; i < grid.ColDefines.Count; i++)
				cells[i].Data = i < objs.Length ? objs[i] : grid.ColDefines[i].DefaultValue;
		}
	}
	#endregion

	#region Cell
	[Serializable]
	public class StyleTableCell
	{
		private object data;
		public virtual object Data
		{
			get { return data; }
			set { data = value; }
		}

		private Rectangle bounds;
		public Rectangle Bounds
		{
			get { return bounds; }
			set { bounds = value; }
		}
		public int Width
		{
			get { return bounds.Width; }
			set { bounds.Width = value; }
		}
		public int Height
		{
			get { return bounds.Height; }
			set { bounds.Height = value; }
		}
		public int Top
		{
			get { return bounds.Y; }
			set { bounds.Y = value; }
		}
		public int Left
		{
			get { return bounds.X; }
			set { bounds.X = value; }
		}
		public int Right
		{
			get { return bounds.Right; }
			set { bounds.Width += bounds.Right - value; }
		}
		public int Bottom
		{
			get { return bounds.Bottom; }
			set { bounds.Height += bounds.Bottom - value; }
		}

		private int index;
		public int Index { get { return index; } set { index = value; } }

		private bool isSelected;
		public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

		private bool isFocus;
		public bool IsFocus { get { return isFocus; } set { isFocus = value; } }

		private bool inEditing;
		public bool InEditing { get { return inEditing; } set { inEditing = value; } }

		private StyleTableColumnDefine define;
		public StyleTableColumnDefine Define { get { return define; } set { define = value; } }

		private StyleTableRow row;
		public StyleTableRow Row { get { return row; } set { row = value; } }

		internal virtual bool ClipBounds { get { return true; } }

		internal virtual Rectangle TextBounds { get { return Bounds; } }

		public virtual bool StartEditOnClick { get { return true; } }

		internal virtual void ClearContent() { Data = null; }

		internal virtual void ResetToDefaultValue() { Data = define.DefaultValue; }

		#region Drawing
		internal virtual void Draw(Graphics g)
		{
			DrawBackground(g);

			if (IsSelected) DrawSelection(g);

			DrawCell(g);
			DrawText(g);
			DrawBorder(g);
		}
		protected virtual void DrawBackground(Graphics g)
		{
			if (!define.BackgroundColor.IsEmpty)
			{
				using (Brush bgBrush = new SolidBrush(Define.BackgroundColor))
					g.FillRectangle(bgBrush, Bounds);
			}
		}
		protected virtual void DrawSelection(Graphics g)
		{
			g.FillRectangle(Brushes.LightSkyBlue, Bounds);
		}
		protected virtual void DrawCell(Graphics g)
		{
		}
		protected virtual void DrawText(Graphics g)
		{
			if (Data != null)
			{
				using (StringFormat sf = new StringFormat())
				{
					sf.LineAlignment = StringAlignment.Center;

					switch (this is StyleTableHeaderCell ? define.HeaderAlignment : define.CellAlignment)
					{
						case ContentAlignment.MiddleLeft:
						case ContentAlignment.TopLeft:
						case ContentAlignment.BottomLeft:
							sf.Alignment = StringAlignment.Near;
							break;

						case ContentAlignment.BottomRight:
						case ContentAlignment.MiddleRight:
						case ContentAlignment.TopRight:
							sf.Alignment = StringAlignment.Far;
							break;

						default:
							sf.Alignment = StringAlignment.Center;
							break;
					}

					g.DrawString(Data.ToString(),
						Row.Grid==null? SystemFonts.DefaultFont : Row.Grid.Font, 
						Brushes.Black, TextBounds, sf);
				}
			}
		}
		protected virtual void DrawBorder(Graphics g)
		{
			// border
			g.DrawLine(Pens.Black, Bounds.Right, Bounds.Top, Bounds.Right, Bounds.Bottom);
		}
		internal virtual void DrawFocus(Graphics g)
		{
			//Rectangle rect = new Rectangle(bound.Left,bound.Top,bound.Width-1,bound.Height-1);
			Rectangle rect = new Rectangle(bounds.Left, bounds.Top,
				bounds.Width, bounds.Height);
			using (Pen p = new Pen(Color.Black, 2))
			{
				p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
				g.DrawRectangle(p, rect);
			}
		}
		#endregion

		#region Actions
		internal virtual void OnInit() { }

		/**
		 * This method to decide whether cell was clicked by user
		 * using mouse position. true should be returned when user
		 * clicked inside this cell.
		 * 
		 * Usually, we can just only use Bound.Contains(p) to return
		 * test result unless your cell is not a rectangle.
		 */
		internal virtual bool HitTest(Point p) { return Bounds.Contains(p); }

		/**
		 * The some shape may contained in this cell they located
		 * by absolute position, so we need call this method to 
		 * update them all.
		 * 
		 * This method called when a cell, row be inserted, or 
		 * height of row, width of cell be adjusted by user.
		 */
		public virtual void UpdateUI() { }

		public virtual bool OnMouseDown(MouseEventArgs e) { return false; }
		public virtual bool OnMouseMove(MouseEventArgs e) { return false; }
		public virtual bool OnMouseUp(MouseEventArgs e) { return false; }

		public virtual bool OnKeyDown(Keys key) { return false; }
		public virtual bool OnKeyUp(Keys key) { return false; }

		public virtual void OnGotFocus() { }
		public virtual void OnLostFocus() { if (this.inEditing)  this.row.Grid.EndEdit(StyleTableEditEditReason.NormalFinish); }
		public virtual void OnDeactivate() { }

		public virtual void OnStartEdit() { }
		public virtual void OnEndEdit() { }

		#endregion
	}
	#endregion

	#region CellHeader
	[Serializable]
	internal class StyleTableHeaderCell : StyleTableCell
	{
		protected override void DrawBackground(Graphics g)
		{
			g.FillRectangle(SystemBrushes.Control, Bounds);
			g.DrawLine(Pens.Black, Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Top);
		}
	}
	#endregion

	#region StyleTable User Event's Argument Define
	public class StyleTableCellEventArgs : EventArgs
	{
		private StyleTableCell cell;

		public StyleTableCell Cell
		{
			get { return cell; }
			set { cell = value; }
		}

		public object Data
		{
			get
			{
				return cell == null ? null : cell.Data;
			}
		}

		public StyleTableRow Row
		{
			get
			{
				return cell == null ? null : cell.Row;
			}
		}
	}
	public class StyleTableCellAfterEditEventArgs : StyleTableCellBeforeEditEventArgs
	{
		private object newData;

		public object NewData
		{
			get { return newData; }
			set { newData = value; }
		}

		StyleTableEditEditReason endReason;

		public StyleTableEditEditReason EndReason
		{
			get { return endReason; }
			set { endReason = value; }
		}
	}
	public class StyleTableCellBeforeEditEventArgs : StyleTableCellEventArgs
	{
		private bool isCancelled = false;

		public bool IsCancelled
		{
			get { return isCancelled; }
			set { isCancelled = value; }
		}

		private string[] candidateItems;

		public string[] CandidateItems
		{
			get { return candidateItems; }
			set { candidateItems = value; }
		}

	}
	internal class STActionPerformedEventArgs : EventArgs
	{
		private IAction action;

		public IAction Action
		{
			get { return action; }
			set { action = value; }
		}
	}
	public class CustomStyleTableCellInsertEventArgs : EventArgs
	{
		public StyleTableColumnDefine Define { get; private set; }

		public CustomStyleTableCellInsertEventArgs(StyleTableColumnDefine define)
		{
			this.Define = define;
		}

		private StyleTableCell newCell;

		public StyleTableCell NewCell
		{
			get { return newCell; }
			set { newCell = value; }
		}
	}
	public class StyleTableRowEventArgs : EventArgs
	{
		public StyleTableRow Row { get; set; }
	}
	#endregion

	#region StyleTable Exceptions
	internal class StyleTableException : Exception
	{
		public StyleTableException(string msg) : base(msg) { }
	}
	#endregion

	#region StyleTable User Actions
	public abstract class STAction : IUndoableAction
	{
		private StyleTableControl grid;

		internal StyleTableControl Grid
		{
			get { return grid; }
			set { grid = value; }
		}

		#region IUndoableAction Members
		public abstract void Undo();
		public abstract void Do();
		public abstract string GetName();
		#endregion
	}
	internal class STActionException : ActionException
	{
		public STActionException(STAction action, string msg) : base(action, msg) { }
	}
	internal class STOtherGridsActionException : STActionException
	{
		public STOtherGridsActionException(STAction action)
			: base(action, "StyleTable Aciton is belong to another grid owner.") { }
	}
	internal class STInsertRowAction : STAction
	{
		private int rowIndex;

		public int RowIndex
		{
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		public STInsertRowAction(int row)
		{
			rowIndex = row;
		}

		#region IAction Members

		public override void Do()
		{
			Grid.InsertEmptyRow(rowIndex);
			Grid.SetFocusCell(rowIndex, 0);
			Grid.SelectRow(rowIndex);
			//Grid.CanvasInvalidate();
		}

		public override void Undo()
		{
			Grid.RemoveRow(rowIndex);
			//Grid.CanvasInvalidate();
		}

		public override string GetName()
		{
			return "Insert Row";
		}

		#endregion
	}
	internal class STRemoveRowAction : STAction
	{
		private int rowIndex;

		public int RowIndex
		{
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		public STRemoveRowAction(int row)
		{
			rowIndex = row;
		}

		StyleTableRow backupRow = null;
		public override void Do()
		{
			if (rowIndex >= 0 && rowIndex < Grid.Rows.Count)
			{
				backupRow = Grid.Rows[rowIndex];
				Grid.RemoveRow(rowIndex);
			}
		}

		public override void Undo()
		{
			if (backupRow != null)
			{
				Grid.InsertRow(rowIndex, backupRow);
				Grid.SelectRow(rowIndex);
			}
		}

		public override string GetName()
		{
			return "Remove Row";
		}
	}
	internal class STSetCellValueAction : STAction
	{
		private int rowIndex;

		public int RowIndex
		{
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		private int cellIndex;

		public int CellIndex
		{
			get { return cellIndex; }
			set { cellIndex = value; }
		}

		private object data;

		public object Data
		{
			get { return data; }
			set { data = value; }
		}

		private object dataBackup;

		public object DataBackup
		{
			get { return dataBackup; }
			set { dataBackup = value; }
		}

		public STSetCellValueAction(int row, int cell, object data)
		{
			this.rowIndex = row;
			this.cellIndex = cell;
			this.data = data;
		}

		public override void Undo()
		{
			StyleTableCell cell = Grid.GetCell(rowIndex, cellIndex);
			if (cell != null)
			{
				cell.Data = dataBackup;
				Grid.SetFocusCell(cell);
				//Grid.InvalidateCell(cell);
			}
		}

		public override void Do()
		{
			if (!Grid.Readonly)
			{
				StyleTableCell cell = Grid.GetCell(rowIndex, cellIndex);
				if (cell != null)
				{
					dataBackup = cell.Data;
					cell.Data = data;
				}
			}
		}

		public override string GetName()
		{
			string str = data == null ? "null" : data.ToString();
			return "Set Value: " + (str.Length > 5 ? (str.Substring(0, 5) + "...") : str);
		}
	}
	public class STClearCellContentAction : STAction
	{
		private List<object> backupDatas = new List<object>();
		private List<StyleTableCell> cells = new List<StyleTableCell>();

		internal List<StyleTableCell> Cells
		{
			get { return cells; }
			set { cells = value; }
		}

		public STClearCellContentAction()
		{
		}

		public STClearCellContentAction(StyleTableCell cell)
		{
			this.cells.Add(cell);
		}

		public override void Do()
		{
			backupDatas.Clear();
			for (int i = 0; i < cells.Count; i++)
			{
				backupDatas.Add(cells[i].Data);
				cells[i].ClearContent();
			}
			//if (cells.Count > 0) Grid.SelectRow(cells[0].Row);
		}

		public override void Undo()
		{
			for (int i = 0; i < cells.Count; i++)
			{
				cells[i].Data = backupDatas[i];
			}
			if (cells.Count > 0) Grid.SelectRow(cells[0].Row);
		}

		public override string GetName()
		{
			return "Clear Content";
		}
	}
	internal class STSetRowHeightAction : STAction
	{
		private int rowIndex;

		public int RowIndex
		{
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		private int height;

		public int Height
		{
			get { return height; }
			set { height = value; }
		}

		public STSetRowHeightAction(int row, int height)
		{
			rowIndex = row;
			this.height = height;
		}

		private int backupHeight;

		public override void Do()
		{
			StyleTableRow row = Grid.GetRowByIndex(rowIndex);
			if (row != null)
			{
				backupHeight = row.Height;
				Grid.SetRowHeight(row.Index, height);
			}
		}

		public override void Undo()
		{
			Grid.SetRowHeight(rowIndex, backupHeight);
		}

		public override string GetName()
		{
			return "Set Row Height: " + height;
		}
	}
	internal class STSetColumnWidthAction : STAction
	{
		private int columnIndex;

		public int ColumnIndex
		{
			get { return columnIndex; }
			set { columnIndex = value; }
		}

		private int width;

		public int Width
		{
			get { return width; }
			set { width = value; }
		}

		public STSetColumnWidthAction(int column, int width)
		{
			columnIndex = column;
			this.width = width;
		}

		private int backupWidth;

		public override void Do()
		{
			if (columnIndex >= 0 && columnIndex < Grid.ColDefines.Count)
			{
				StyleTableColumnDefine def = Grid.ColDefines[columnIndex];

				backupWidth = def.Width;
				Grid.SetColumnWidth(columnIndex, width);
			}
		}

		public override void Undo()
		{
			Grid.SetColumnWidth(columnIndex, backupWidth);
		}

		public override string GetName()
		{
			return "Set Column Width: " + width;
		}
	}
	#endregion
}

#region StyleTable Serializing XML
namespace Jingwood.WindowsFormControl.StyleTable.XML
{
	public class STXmlGrid
	{
		private List<STXmlColDefine> colDefines;

		[XmlArray("columns"), XmlArrayItem("column")]
		public List<STXmlColDefine> ColDefines
		{
			get { return colDefines; }
			set { colDefines = value; }
		}

		private List<STXmlRow> rows;

		[XmlArray("rows"), XmlArrayItem("row")]
		public List<STXmlRow> Rows
		{
			get { return rows; }
			set { rows = value; }
		}
	}

	public class STXmlColDefine
	{
		private int width;

		[XmlAttribute("width")]
		public int Width
		{
			get { return width; }
			set { width = value; }
		}

		private string text;

		[XmlText]
		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		private string columnType;

		[XmlAttribute("col-type")]
		public string ColumnType
		{
			get { return columnType; }
			set { columnType = value; }
		}

		private string defaultValue;

		[XmlAttribute("default-value")]
		public string DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		private string headerAlign;

		[XmlAttribute("header-align")]
		public string HeaderAlign
		{
			get { return headerAlign; }
			set { headerAlign = value; }
		}

		private string cellAlign;

		[XmlAttribute("cell-align")]
		public string CellAlign
		{
			get { return cellAlign; }
			set { cellAlign = value; }
		}

		private string backColor;

		[XmlAttribute("back-color")]
		public string BackColor
		{
			get { return backColor; }
			set { backColor = value; }
		}
	}

	public class STXmlRow
	{
		private int height;

		[XmlAttribute("height")]
		public int Height
		{
			get { return height; }
			set { height = value; }
		}

		private List<STXmlCell> cells;

		[XmlArray("cells"), XmlArrayItem("cell")]
		public List<STXmlCell> Cells
		{
			get { return cells; }
			set { cells = value; }
		}
	}

	public class STXmlCell
	{
		private string data;

		[XmlText]
		public string Data
		{
			get { return data; }
			set { data = value; }
		}
	}
}
#endregion