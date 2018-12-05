using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Jingwood.WindowsFormControl.StyleTable.Common;

namespace Jingwood.WindowsFormControl.StyleTable.CellType
{
	#region IndexCell
	public class IndexCell : StyleTableCell
	{

	}
	#endregion // IndexCell

	#region DropdownCell
	[Serializable]
	public abstract class DropdownCell : StyleTableCell
	{
		private static readonly int buttonSize = 17;
		private Rectangle dropdownButtonBound;
		public override bool StartEditOnClick { get { return false; } }

		public DropdownCell() { }

		internal override void OnInit()
		{
			base.OnInit();
		}

		public bool EnableDropWindow
		{
			get
			{
				return Define.Dropdown == StyleTableDropdownType.Combo
					|| Define.Dropdown == StyleTableDropdownType.Dropdown;
			}
		}

		public bool IsDropdown { get; private set; }

		internal override void DrawFocus(Graphics g)
		{
			if (EnableDropWindow)
			{
				GraphicsToolkit.DrawDropdownButton(g, dropdownButtonBound, IsDropdown);
			}
			base.DrawFocus(g);
		}

		public override void OnStartEdit()
		{
			base.OnStartEdit();
			if (this.Define.Dropdown == StyleTableDropdownType.Dropdown)
			{
				Dropdown();
			}
		}

		public override void OnEndEdit()
		{
			PullUp();
			base.OnEndEdit();
		}

		public override void UpdateUI()
		{
			dropdownButtonBound = new Rectangle(Bounds.Right, Bounds.Bottom - buttonSize - 1, buttonSize, buttonSize);
			base.UpdateUI();
		}

		internal override bool ClipBounds { get { return false; } }

		internal override bool HitTest(Point p)
		{
			return base.HitTest(p) || (EnableDropWindow && IsFocus && dropdownButtonBound.Contains(p));
		}

		public override bool OnMouseDown(MouseEventArgs e)
		{
			if (EnableDropWindow)
			{
				if (dropdownButtonBound.Contains(e.Location))
				{
					if (IsDropdown)
					{
						PullUp();
					}
					else
					{
						Dropdown();
						if (!InEditing)
						{
							Row.Grid.StartEdit(Row, this);
						}
					}
					return true;
				}
			}

			return base.OnMouseDown(e);
		}

		public abstract Control GetDropDownControl();

		internal virtual void Dropdown()
		{
			if (EnableDropWindow)
			{
				if (this.dropdownWindow == null)
				{
					Control dropdownControl = GetDropDownControl();
					if (dropdownControl != null)
					{
						this.dropdownWindow = new DropdownWindow(dropdownControl);
					}
				}

				this.dropdownWindow.Show(Row.Grid, this.Row.Grid.CanvasPointToScreen(new Point(Left, Bottom)));
				//this.dropdownWindow.Focus();
				IsDropdown = true;
			}
		}

		internal virtual void PullUp()
		{
			if (this.dropdownWindow != null && dropdownWindow.Visible)
			{
				this.dropdownWindow.Close();
			}
			IsDropdown = false;
		}

		private DropdownWindow dropdownWindow = null;

		internal DropdownWindow CurrentDropdownWindow { get { return this.dropdownWindow; } }

		#region class DropdownWindow
		internal class DropdownWindow : ToolStripDropDown
		{
			public DropdownWindow(Control ctrl)
			{
				this.AutoClose = false;
				//this.TabStop = false;

				ToolStripControlHost host = new ToolStripControlHost(ctrl);
				host.AutoSize = false;
				host.Padding = new Padding(0);

				Items.Add(host);

				this.AutoSize = true;
				this.Padding = new Padding(0);
			}

			protected override void OnMouseDown(MouseEventArgs mea)
			{
			}
		}
		#endregion // class DropdownWindow
	}
	#endregion // DropdownCell

	#region DropDownListCell
	[Serializable]
	public class DropDownListCell : DropdownCell
	{
		public override bool StartEditOnClick { get { return Define.Dropdown != StyleTableDropdownType.Dropdown; } }

		private NoFocusListBox list = null;

		public override Control GetDropDownControl()
		{
			if (list == null)
			{
				this.list = new NoFocusListBox(Row.Grid);
				this.list.Items.AddRange(Define.Candidates);
				this.list.ItemSelected += List_Click;
			}

			return list;
		}

		public override void OnLostFocus()
		{
			if (this.Define.Dropdown == StyleTableDropdownType.Dropdown)
			{
				this.Row.Grid.EndEdit(StyleTableEditEditReason.Cancel);
			}
			else
				base.OnLostFocus();
		}

		public virtual string DataToString()
		{
			return Data == null ? string.Empty : Convert.ToString(Data);
		}

		public virtual object StringToData(string str)
		{
			return str;
		}

		void List_Click(object sender, EventArgs e)
		{
			Row.Grid.EndEdit(this.list.Text, StyleTableEditEditReason.NormalFinish);
		}

		public override bool OnKeyDown(Keys key)
		{
			if (IsDropdown)
			{
				switch (key)
				{
					case Keys.Up:
						if (list.SelectedIndex > 0) list.SelectedIndex--;
						return true;

					case Keys.Down:
						if (list.SelectedIndex < list.Items.Count - 1) list.SelectedIndex++;
						return true;
				}
			}

			return base.OnKeyDown(key);
		}

		internal class NoFocusListBox : ListBox
		{
			private StyleTableControl grid;

			public NoFocusListBox(StyleTableControl grid) :
				base()
			{
				this.grid = grid;
				this.TabStop = false;
				this.Click += NoFocusListBox_Click;
				this.BorderStyle = System.Windows.Forms.BorderStyle.None;
			}

			void NoFocusListBox_Click(object sender, EventArgs e)
			{
				if (ItemSelected != null)
				{
					ItemSelected(this, e);
				}
			}

			public event EventHandler ItemSelected;

			protected override void OnMouseDown(MouseEventArgs e)
			{
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				if (ItemSelected != null)
				{
					ItemSelected(this, e);
				}
			}

			protected override void OnMouseMove(MouseEventArgs e)
			{
				var localPoint = PointToClient(Cursor.Position);
				SelectedIndex = this.IndexFromPoint(localPoint);
			}
		}
	}
	#endregion // DropDownListCell

	#region ButtonCell
	[Serializable]
	public class ButtonCell : StyleTableCell
	{
		private bool isPressed;
		public override bool StartEditOnClick { get { return false; } }
		protected override void DrawCell(Graphics g)
		{
			Rectangle rect = Bounds;
			rect.Inflate(-2, -2);
			GraphicsToolkit.Draw3DButton(g, rect, isPressed);
		}
		protected override void DrawText(Graphics g)
		{
			if (Data != null)
				using (StringFormat sf = new StringFormat())
				{
					sf.LineAlignment = StringAlignment.Center;
					sf.Alignment = StringAlignment.Center;
					Rectangle rect = Bounds;
					if (isPressed) rect.Offset(1, 1);
					g.DrawString(Data.ToString(), SystemFonts.DefaultFont, Brushes.Black, rect, sf);
				}
		}
		public override bool OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				return true;
			}
			else return false;
		}
		public override bool OnMouseUp(MouseEventArgs e)
		{
			isPressed = false;
			return true;
		}
		public override bool OnKeyDown(Keys key)
		{
			if (key == Keys.Space)
			{
				isPressed = true;
				return true;
			}
			else
				return base.OnKeyDown(key);
		}
		public override bool OnKeyUp(Keys key)
		{
			if (key == Keys.Space)
			{
				isPressed = false;
				return true;
			}
			else
				return base.OnKeyUp(key);
		}
		internal override void ClearContent() { ResetToDefaultValue(); }
	}
	#endregion // ButtonCell

	#region HyperlinkCell
	[Serializable]
	public class HyperlinkCell : StyleTableCell
	{
		private bool isPressed;
		public override bool StartEditOnClick { get { return false; } }
		protected override void DrawText(Graphics g)
		{
			if (Data != null)
			{
				using (StringFormat sf = new StringFormat())
				{
					sf.LineAlignment = StringAlignment.Center;
					//sf.Alignment = StringAlignment.Center;
					Rectangle rect = Bounds;
					if (isPressed) rect.Offset(1, 1);
					using (Font f = new Font(SystemFonts.DefaultFont, FontStyle.Underline))
					{
						g.DrawString(Data.ToString(), f,
						isPressed ? Brushes.Red : Brushes.Blue, rect, sf);
					}
				}
			}
		}
		public override bool OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				return true;
			}
			else return false;
		}
		public override bool OnMouseUp(MouseEventArgs e)
		{
			if (isPressed)
			{
				isPressed = false;
				this.Row.Grid.PerformCellClick(this);
			}
			return true;
		}
		public override bool OnKeyDown(Keys key)
		{
			if (key == Keys.Space)
			{
				isPressed = true;
				return true;
			}
			else
				return base.OnKeyDown(key);
		}
		public override bool OnKeyUp(Keys key)
		{
			if (key == Keys.Space)
			{
				if (isPressed)
				{
					isPressed = false;
					this.Row.Grid.PerformCellClick(this);
				}
				return true;
			}
			else
				return base.OnKeyUp(key);
		}
	}
	#endregion // HyperlinkCell

	#region CheckboxCell
	[Serializable]
	public class CheckboxCell : StyleTableCell
	{
		public bool IsChecked { get; set; }

		public override object Data
		{
			get
			{
				return base.Data;
			}
			set
			{
				if (value is bool)
					IsChecked = (bool)value;
				else if (value is string)
					IsChecked = ((string)value).ToLower().Equals("true");

				base.Data = IsChecked;
			}
		}

		public override bool StartEditOnClick { get { return false; } }

		protected override void DrawCell(Graphics g)
		{
			Rectangle rect = GetCheckboxRectangle();
			//g.FillRectangle(Brushes.White, rect);
			//if (IsChecked) g.DrawImage(Resources.check_large, rect);
			//g.DrawRectangle(Pens.Black, rect);

			ControlPaint.DrawCheckBox(g, rect, isPressed ?
				(IsChecked ? (ButtonState.Checked | ButtonState.Pushed) : ButtonState.Pushed)
				: (IsChecked ? ButtonState.Checked : ButtonState.Normal));
		}

		protected override void DrawText(Graphics g) { }

		private bool isPressed = false;

		public override bool OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				return true;
			}
			else return false;
		}

		public override bool OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (isPressed)
				{
					Row.Grid.DoAction(new STSetCellValueAction(Row.Index, Index, !IsChecked));
					isPressed = false;
					Row.Grid.PerformCellClick(this);
				}
				return true;
			}
			else 
				return base.OnMouseUp(e);
		}

		public override bool OnKeyDown(Keys key)
		{
			if (key == Keys.Space)
			{
				isPressed = true;
				return true;
			}
			else
				return base.OnKeyDown(key);
		}

		public override bool OnKeyUp(Keys key)
		{
			if (key == Keys.Space)
			{
				if (isPressed)
				{
					Row.Grid.DoAction(new STSetCellValueAction(Row.Index, Index, !IsChecked));
					isPressed = false;
					Row.Grid.PerformCellClick(this);
				}
				return true;
			}
			else
				return base.OnKeyUp(key);
		}

		private Rectangle GetCheckboxRectangle()
		{
			int left = Bounds.Left + (Bounds.Width - 16) / 2;
			int top = Bounds.Top + (Bounds.Height - 16) / 2;
			return new Rectangle(left, top, 16, 16);
		}
	}
	#endregion // CheckboxCell

	#region NumericCell
	[Serializable]
	public class NumericCell : StyleTableCell
	{
		public override object Data
		{
			get
			{
				return base.Data;
			}
			set
			{
				int num = 0;
				if (value is int)
					num = (int)value;
				else if (value is string)
					int.TryParse(value as string, out num);
				base.Data = num;
			}
		}
	}
	#endregion // NumericCell

	#region DateTimeCell
	public class DateTimeCell : DropdownCell
	{
		public DateTimeCell()
		{
		}

		public override void OnStartEdit()
		{
			base.OnStartEdit();

			DateTime dt = DateTime.MinValue;

			if (DateTime.TryParse(Convert.ToString(Data), out dt))
			{
				monthCal.SetDate(dt);
			}
		}

		void MonthCal_DateSelected(object sender, DateRangeEventArgs e)
		{
			UpdateCellData();
		}

		private MonthCalendar monthCal = null;

		//public override void OnLostFocus()
		//{
		//	UpdateCellData();
		//	base.OnLostFocus();
		//}

		internal override void PullUp()
		{
			base.PullUp();

			monthCal.Focus();
		}

		void UpdateCellData()
		{
			if (monthCal != null)
			{
				CultureInfo ci = Define.CultureInfo;
				if (ci == null) ci = System.Threading.Thread.CurrentThread.CurrentCulture;
				string pattern = Define.Pattern;
				if (string.IsNullOrEmpty(pattern)) pattern = ci.DateTimeFormat.LongDatePattern;

				this.Row.Grid.EndEdit(monthCal.SelectionStart.ToString(pattern, ci));
			}
		}

		public override Control GetDropDownControl()
		{
			if (this.monthCal == null)
			{
				this.monthCal = new MonthCalendar();
				this.monthCal.MaxSelectionCount = 1;
				this.monthCal.DateSelected += MonthCal_DateSelected;
			}

			return monthCal;
		}
	}
	#endregion // DateTimeCell

	#region ImageCell
	public class ImageCell : DropDownListCell
	{
		/// <summary>
		/// Get or set the image to be displayed in cell
		/// </summary>
		public Image Image { get; set; }

		public ImageCell() { }

		/// <summary>
		/// Construct image cell-body to show a specified image
		/// </summary>
		/// <param name="image">Image to be displayed</param>
		public ImageCell(Image image)
			: this(image, default(ImageCellViewMode))
		{
		}

		/// <summary>
		/// Construct image cell-body to show a image by specified display-method
		/// </summary>
		/// <param name="image">Image to be displayed</param>
		/// <param name="viewMode">View mode decides how to display a image inside a cell</param>
		public ImageCell(Image image, ImageCellViewMode viewMode)
		{
			this.Image = image;
			this.viewMode = viewMode;
		}

		protected ImageCellViewMode viewMode;

		/// <summary>
		/// Set or get the view mode of this image cell
		/// </summary>
		public ImageCellViewMode ViewMode
		{
			get
			{
				return this.viewMode;
			}
			set
			{
				if (this.viewMode != value)
				{
					this.viewMode = value;
				}
			}
		}

		internal override void Draw(Graphics g)
		{
			if (Image != null)
			{
				float x = Bounds.X, y = Bounds.Y, width = 0, height = 0;
				bool needClip = false;

				switch (this.viewMode)
				{
					default:
					case ImageCellViewMode.Stretch:
						width = Bounds.Width;
						height = Bounds.Height;
						break;

					case ImageCellViewMode.Zoom:
						float widthRatio = (float)Bounds.Width / Image.Width;
						float heightRatio = (float)Bounds.Height / Image.Height;
						float minRatio = Math.Min(widthRatio, heightRatio);
						width = minRatio * Image.Width;
						height = minRatio * Image.Height;
						break;

					case ImageCellViewMode.Clip:
						width = Image.Width;
						height = Image.Height;

						if (width > Bounds.Width || height > Bounds.Height) needClip = true;
						break;
				}


				//switch (this..Style.HAlign)
				//{
				//	default:
				//	case ReoGridHorAlign.Left:
				//		x = Bounds.X;
				//		break;

				//	case ReoGridHorAlign.Center:
				//		x = (Bounds.Width - width) / 2;
				//		break;

				//	case ReoGridHorAlign.Right:
				//		x = Bounds.Width - width;
				//		break;
				//}

				//switch (cell.Style.VAlign)
				//{
				//	default:
				//	case ReoGridVerAlign.Top:
				//		y = Bounds.Y;
				//		break;

				//	case ReoGridVerAlign.Middle:
				//		y = (Bounds.Height - height) / 2;
				//		break;

				//	case ReoGridVerAlign.Bottom:
				//		y = Bounds.Height - height;
				//		break;
				//}

				if (needClip)
				{
					g.SetClip(Bounds);
				}

				g.DrawImage(Image, x, y, width, height);

				if (needClip)
				{
					g.ResetClip();
				}
			}
		}
	}

	/// <summary>
	/// Image dispaly method in ImageCell-body
	/// </summary>
	public enum ImageCellViewMode
	{
		/// <summary>
		/// Fill to cell boundary (default)
		/// </summary>
		Stretch,

		/// <summary>
		/// Lock aspect ratio to fit cell boundary
		/// </summary>
		Zoom,

		/// <summary>
		/// Keep image size and clip to fill the cell
		/// </summary>
		Clip,
	}

	#endregion // ImageCell
}
