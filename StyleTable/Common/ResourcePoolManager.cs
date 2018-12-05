using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	public sealed class ResourcePoolManager
	{
		private static readonly ResourcePoolManager instance = new ResourcePoolManager();
		public static ResourcePoolManager Instance { get { return instance; } }

		private ResourcePoolManager()
		{
			Logger.Log("resource pool", "create resource pool...");
		}
		//#region PlainBoardColor
		//private List<PlainBoardColor> pbColors = new List<PlainBoardColor>();
		//internal PlainBoardColor GetPen(PlainBoardColor pbColor)
		//{
		//  if (pbColor == null) return null;
		//  if(!(pbColor is PlainBoardSolidColor)) return pbColor;
		//  PlainBoardColor color = pbColors.SingleOrDefault(pbc=>pbc is PlainBoardSolidColor
		//    && ((PlainBoardSolidColor)pbc).Color == ((PlainBoardSolidColor)pbColor).Color);
		//  if (color == null)
		//  {
		//    pbColor.Add(pen);

		//    Logger.Log("resource pool", "add pen, count: " + pens.Count);
		//  }
		//  return pen;
		//}
		//#endregion

		#region Brush
		private Dictionary<Color, SolidBrush> solidBrushes = new Dictionary<Color, SolidBrush>();
		public SolidBrush GetSolidBrush(Color color)
		{
			if (color.IsEmpty || color.A == 0) return null;

			SolidBrush b;
			if (solidBrushes.TryGetValue(color, out b))
			{
				return b;
			}
			else
			{
				b = new SolidBrush(color);
				solidBrushes.Add(color, b);

				Logger.Log("resource pool", "add solid brush, count: " + solidBrushes.Count);
				return b;
			}
		}

		private Dictionary<HatchStyleBrushInfo, HatchBrush> hatchBrushes = new Dictionary<HatchStyleBrushInfo, HatchBrush>();
		public HatchBrush GetHatchBrush(HatchStyle style, Color foreColor, Color backColor)
		{
			HatchStyleBrushInfo info = new HatchStyleBrushInfo(style, foreColor, backColor);

			HatchBrush hb;
			if (hatchBrushes.TryGetValue(info, out hb))
				return hb;
			else
			{
				HatchBrush b = new HatchBrush(style, foreColor, backColor);
				hatchBrushes.Add(info, b);

				Logger.Log("resource pool", "add hatch brush, count: " + hatchBrushes.Count);
				return b;
			}
		}
		private struct HatchStyleBrushInfo
		{
			internal HatchStyle style;
			internal Color foreColor;
			internal Color backgroundColor;

			public HatchStyleBrushInfo(HatchStyle style, Color foreColor, Color backgroundColor)
			{
				this.style = style;
				this.foreColor = foreColor;
				this.backgroundColor = backgroundColor;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is HatchStyleBrushInfo)) return false;

				HatchStyleBrushInfo right = (HatchStyleBrushInfo)obj;
				return (this.style == right.style
					&& this.foreColor == right.foreColor
					&& this.backgroundColor == right.backgroundColor);
			}

			public static bool operator ==(HatchStyleBrushInfo left, HatchStyleBrushInfo right)
			{
				if (left == null && right == null) return true;
				if (left == null || right == null) return false;

				if (left == null)
					return right.Equals(left);
				else
					return left.Equals(right);
			}

			public static bool operator !=(HatchStyleBrushInfo left, HatchStyleBrushInfo right)
			{
				return !(left == right);
			}

			public override int GetHashCode()
			{
				return (short)style ^ foreColor.ToArgb() ^ backgroundColor.ToArgb();
			}
		}
		#endregion

		#region Pen
		private List<Pen> pens = new List<Pen>();
		public Pen GetPen(Color color)
		{
			return GetPen(color, 1, DashStyle.Solid);
		}
		public Pen GetPen(Color color, float weight, DashStyle style)
		{
			if (color.IsEmpty) return null;

			Pen pen = pens.FirstOrDefault(p => p.Color == color
				&& p.Width == weight && p.DashStyle == style);

			if (pen == null)
			{
				pen = new Pen(color, weight);
				pen.DashStyle = style;
				pens.Add(pen);

				Logger.Log("resource pool", "add pen, count: " + pens.Count);
			}

			return pen;
		}
		#endregion

		#region Font
		private Dictionary<string, List<Font>> fonts = new Dictionary<string, List<Font>>();
		public Font GetFont(string familyName, float emSize, FontStyle fs)
		{
			//#if DEBUG
			//			Stopwatch sw = Stopwatch.StartNew();
			//#endif
			FontFamily family;

			if (string.IsNullOrEmpty(familyName))
				family = SystemFonts.DefaultFont.FontFamily;
			else
			{
				try
				{
					family = new FontFamily(familyName);
				}
				catch (ArgumentException ex)
				{
					//throw new FontNotFoundException(ex.ParamName);
					family = SystemFonts.DefaultFont.FontFamily;
					Logger.Log("resource pool", "font family error: " + familyName + ": " + ex.Message);
				}

				if (!family.IsStyleAvailable(fs))
				{
					try
					{
						fs = FindFirstAvailableFontStyle(family);
					}
					catch
					{
						return SystemFonts.DefaultFont;
					}
				}
			}

			Font font = null;
			List<Font> fontGroup = null;

			if (fonts.TryGetValue(family.Name, out fontGroup))
			{
				font = fontGroup.FirstOrDefault(f => f.Size == emSize && f.Style == fs);
			}

			if (font == null)
			{
				font = new Font(family, emSize, fs);

				if (fontGroup == null)
				{
					fonts.Add(family.Name, fontGroup = new List<Font> { font });
					Logger.Log("resource pool", "font resource group added. font groups: " + fonts.Count);
				}
				else
				{
					fontGroup.Add(font);
					Logger.Log("resource pool", "font resource added. fonts: " + fontGroup.Count);
				}

			}
			//#if DEBUG
			//      sw.Stop();
			//      Debug.WriteLine("resource pool: font scan: " + sw.ElapsedMilliseconds+ " ms.");
			//#endif
			return font;
		}

		private FontStyle FindFirstAvailableFontStyle(FontFamily ff)
		{
			if (ff.IsStyleAvailable(FontStyle.Regular))
				return FontStyle.Regular;
			else if (ff.IsStyleAvailable(FontStyle.Bold))
				return FontStyle.Bold;
			else if (ff.IsStyleAvailable(FontStyle.Italic))
				return FontStyle.Italic;
			else if (ff.IsStyleAvailable(FontStyle.Strikeout))
				return FontStyle.Strikeout;
			else if (ff.IsStyleAvailable(FontStyle.Underline))
				return FontStyle.Underline;
			else
			{
				Logger.Log("resource pool", "no available font style found: " + ff.Name);
				throw new NoAvailableFontStyleException();
			}
		}
		internal class NoAvailableFontStyleException : Exception
		{
		}
		#endregion

		#region Image
		private Dictionary<Guid, ImageResource> images
			= new Dictionary<Guid, ImageResource>();
		public ImageResource GetImageResource(Guid id)
		{
			return images.Values.FirstOrDefault(i => i.ResId.Equals(id));
		}
		public ImageResource GetImage(string fullPath)
		{
			ImageResource res = images.Values.FirstOrDefault(
				i => i.FullPath != null && i.FullPath.Equals(fullPath, StringComparison.CurrentCultureIgnoreCase));

			if (res != null)
			{
				//if (res.Image != null) res.Image.Dispose();
				//res.Image = Image.FromFile(fullPath);
				return res;
			}
			else
			{
				Image image;
				try
				{
					image = Image.FromFile(fullPath);
				}
				catch (Exception ex)
				{
					Logger.Log("resource pool", "add image file failed: " + ex.Message);
					return null;
				}

				return AddImage(Guid.NewGuid(), image, fullPath);
			}
		}
		public ImageResource AddImage(Guid id, Image image, string fullPath)
		{
			ImageResource res;

			if (!images.TryGetValue(id, out res))
			{
				images.Add(id, res = new ImageResource()
				{
					ResId = id,
					FullPath = fullPath,
				});

				Logger.Log("resource pool", "image added. count: " + images.Count);
			}

			if (res.Image != null)
			{
				res.Image.Dispose();
			}

			res.Image = image;

			return res;
		}
		#endregion

		#region Graphics
		private Bitmap bitmapForCachedGraphics;
		private Graphics cachedGraphics;
		public Graphics CachedGraphics
		{
			get
			{
				if (cachedGraphics == null)
				{
					bitmapForCachedGraphics = new Bitmap(1, 1);
					cachedGraphics = Graphics.FromImage(bitmapForCachedGraphics);
				}
				return cachedGraphics;
			}
		}
		#endregion

		internal void ReleaseAllResources()
		{
			Logger.Log("resource pool", "release all resources...");

			int count = pens.Count + fonts.Values.Sum(f => f.Count) + images.Count + solidBrushes.Count + hatchBrushes.Count;

			// pens
			foreach (var p in pens) p.Dispose();
			pens.Clear();

			// fonts
			foreach (var fl in fonts.Values)
			{
				foreach (var f in fl)
				{
					f.FontFamily.Dispose();
					f.Dispose();
				}
				fl.Clear();
			}
			fonts.Clear();

			// images
			foreach (var i in images.Values) i.Image.Dispose();
			images.Clear();

			// brushes
			foreach (var sb in solidBrushes.Values) sb.Dispose();
			solidBrushes.Clear();
			foreach (var hb in hatchBrushes.Values) hb.Dispose();
			hatchBrushes.Clear();

			if (cachedGraphics != null) cachedGraphics.Dispose();
			if (bitmapForCachedGraphics != null) bitmapForCachedGraphics.Dispose();

			Logger.Log("resource pool", count + " objects released.");
		}
	}

	public class Resource
	{
		private Guid resId;

		public Guid ResId
		{
			get { return resId; }
			set { resId = value; }
		}
	}

	public class ImageResource : Resource
	{
		private string fullPath;

		public string FullPath
		{
			get { return fullPath; }
			set { fullPath = value; }
		}

		private Image image;

		public Image Image
		{
			get { return image; }
			set { image = value; }
		}
	}

	public class FontNotFoundException : Exception
	{
		public string FontName { get; set; }

		public FontNotFoundException(string fontName)
		{
			this.FontName = fontName;
		}
	}

}
