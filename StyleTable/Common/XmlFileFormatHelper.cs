using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	internal sealed class XmlFileFormatHelper
	{
		internal static string EncodeRect(RectangleF rect)
		{
			return (string.Format("({0},{1},{2},{3})",
					rect.Left, rect.Top, rect.Width, rect.Height));
		}
		internal static string EncodeSize(Size size)
		{
			return (string.Format("({0},{1})", size.Width, size.Height));
		}
		internal static string EncodeColor(Color c)
		{
			return c.IsEmpty ? "none" :
				(c.IsNamedColor ? c.Name :
				(c.A == 255 ? (string.Format("#{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B))
				: string.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", c.A, c.R, c.G, c.B)));
		}
		internal static string EncodePoint(Point p)
		{
			return EncodePoint(new PointF(p.X, p.Y));
		}
		internal static string EncodePoint(PointF p)
		{
			return (string.Format("({0},{1})", p.X, p.Y));
		}
		internal static string EncodeFontStyle(FontStyle fs)
		{
			StringBuilder sb = new StringBuilder();
			if ((fs & FontStyle.Bold) > 0)
			{
				sb.Append("blob");
			}
			if ((fs & FontStyle.Italic) > 0)
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append("italic");
			}
			if ((fs & FontStyle.Strikeout) > 0)
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append("strikeout");
			}
			if ((fs & FontStyle.Underline) > 0)
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append("underline");
			}
			
			return sb.Length == 0 ? "normal": sb.ToString();
		}
		internal static string EncodeLineStyle(DashStyle ds)
		{
			switch (ds)
			{
				case (System.Drawing.Drawing2D.DashStyle.Dash):
					return "dash";
				case System.Drawing.Drawing2D.DashStyle.DashDot:
					return "dash-dot";
				case System.Drawing.Drawing2D.DashStyle.DashDotDot:
					return "dash-dot-dot";
				case System.Drawing.Drawing2D.DashStyle.Dot:
					return "dot";
				default:
				case System.Drawing.Drawing2D.DashStyle.Solid:
					return "solid";
			}
		}
		internal static string EncodeLineCapStyle(LineCap cap)
		{
			switch (cap)
			{
				case LineCap.ArrowAnchor:
					return "arrow";
				case LineCap.DiamondAnchor:
					return "diamond";
				case LineCap.RoundAnchor:
					return "round";
				case LineCap.SquareAnchor:
					return "square";
				default:
				case LineCap.NoAnchor:
					return "none";
			}
		}
		//internal static string EncodeHorizontalAlign(StyleTableAlign halign)
		//{
		//  switch (halign)
		//  {
		//    case StyleTableHorAlign.Left:
		//      return "left";
		//    default:
		//    case StyleTableHorAlign.Center:
		//      return "center";
		//    case StyleTableHorAlign.Right:
		//      return "right";
		//  }
		//}
		//internal static string EncodeVerticalAlign(StyleTableVerAlign valign)
		//{
		//  switch (valign)
		//  {
		//    case StyleTableVerAlign.Top:
		//      return "top";
		//    default:
		//    case StyleTableVerAlign.Middle:
		//      return "middle";
		//    case StyleTableVerAlign.Bottom:
		//      return "bottom";
		//  }
		//}
	
		internal static Guid DecodeGuid(string data)
		{
			try
			{
				return new Guid(data);
			}
			catch { return Guid.Empty; }
		}
		private static readonly Regex RectRegex =
				new Regex(@"\(\s*([-\w]+)\s*,\s*([-\w]+)\s*,\s*([-\w]+)\s*,\s*([-\w]+)\)\s*");
		internal static Rectangle DecodeRect(string data)
		{
			Match m = RectRegex.Match(data);
			if (m.Success)
			{
				return new Rectangle(GetPixelValue(m.Groups[1].Value),
					GetPixelValue(m.Groups[2].Value),
					GetPixelValue(m.Groups[3].Value),
					GetPixelValue(m.Groups[4].Value));
			}
			else
				return Rectangle.Empty;
		}
		internal static Size DecodeSize(string data)
		{
			return new Size(DecodePoint(data));
		}
		internal static readonly Regex RGBColorRegex =
new Regex(@"rgb\s*\(\s*((\d+)\s*,)?(\d+)\s*,(\d+)\s*,(\d+)\s*\)");
		internal static readonly Regex WebColorRegex = new
				Regex(@"\#([0-9a-fA-F]{2})?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})");
		internal static bool IsRGBColorFormat(string data)
		{
			return RGBColorRegex.IsMatch(data);
		}
		internal static bool IsWebColorFormat(string data)
		{
			return WebColorRegex.IsMatch(data);
		}
		public static Color DecodeColor(string data)
		{
			if (data == null || data.Length == 0 || data.ToLower().Equals("none"))
			{
				return Color.Empty;
			}
			
			Match m = RGBColorRegex.Match(data.ToLower());
			if (m.Success)
			{
				return Color.FromArgb(m.Groups[2].Value.Length > 0 ?
					Convert.ToInt32(m.Groups[2].Value) : 255,
					Convert.ToInt32(m.Groups[3].Value),
					Convert.ToInt32(m.Groups[4].Value),
					Convert.ToInt32(m.Groups[5].Value));
			}
			else if ((m = WebColorRegex.Match(data)).Success)
			{
				return Color.FromArgb(m.Groups[1].Value.Length > 0 ?
									Convert.ToInt32(m.Groups[1].Value, 16) : 255,
									Convert.ToInt32(m.Groups[2].Value, 16),
									Convert.ToInt32(m.Groups[3].Value, 16),
									Convert.ToInt32(m.Groups[4].Value, 16));
			}
			else
			{
				try { return Color.FromName(data); }
				catch { }
			}
			return Color.Empty;
		}
		private static readonly Regex PointRegex =
				new Regex(@"\(\s*([-\w]+)\s*,\s*([-\w]+)\)\s*");
		internal static bool IsRectFormat(string data)
		{
			return RectRegex.IsMatch(data);
		}		
		internal static Point DecodePoint(string data)
		{
			Match m = PointRegex.Match(data);
			if (m.Success)
			{
				return new Point(GetPixelValue(m.Groups[1].Value),
					GetPixelValue(m.Groups[2].Value));
			}
			else
				return Point.Empty;
		}

	
		internal static Font DecodeFont(string fontName, string fontSize, string fontStyle)
		{
			string name = (fontName == null) ?
				SystemFonts.DefaultFont.FontFamily.Name : fontName;

			FontStyle fs = 0;
			if (fontStyle == null)
				fs = FontStyle.Regular;
			else
			{
				string[] fontStyles = fontStyle.Split(',');
				if (fontStyles.Length == 0)
				{
					fs = FontStyle.Regular;
				}
				else
				{
					foreach (string fstyle in fontStyles)
					{
						string fst = fstyle.Trim().ToLower();
						if (fst.Equals("blob"))
							fs |= FontStyle.Bold;
						else if (fst.Equals("italic"))
							fs |= FontStyle.Italic;
						else if (fst.Equals("strikeout"))
							fs |= FontStyle.Strikeout;
						else if (fst.Equals("underline"))
							fs |= FontStyle.Underline;
					}
				}
			}

			float size = GetFloatPixelValue(fontSize, SystemFonts.DefaultFont.Size);

			return ResourcePoolManager.Instance.GetFont(name, size, fs);
		}
		internal static FontStyle DecodeFontStyle(string fontStyleStr)
		{
			FontStyle fs = FontStyle.Regular;
			string[] fsstr = fontStyleStr.Split(' ', ',');
			if (fsstr.Contains("blob"))
				fs |= FontStyle.Bold;
			else if (fsstr.Contains("italic"))
				fs |= FontStyle.Italic;
			else if (fsstr.Contains("strikeout"))
				fs |= FontStyle.Strikeout;
			else if (fsstr.Contains("underline"))
				fs |= FontStyle.Underline;
			else
				fs = FontStyle.Regular;
			return fs;
		}

		internal static DashStyle DecodeLineStyle(string data)
		{
			switch(data.Trim().ToLower())
			{
				case "dash":
					return System.Drawing.Drawing2D.DashStyle.Dash;
				case "dash-dot":
					return System.Drawing.Drawing2D.DashStyle.DashDot;
				case "dash-dot-dot":
					return System.Drawing.Drawing2D.DashStyle.DashDotDot;
				case "dot":
					return System.Drawing.Drawing2D.DashStyle.Dot;
				default:
				case "solid":
					return System.Drawing.Drawing2D.DashStyle.Solid;
			}
		}
		internal static LineCap DecodeLineCapStyle(string data)
		{
			switch (data)
			{
				case "arrow":
					return LineCap.ArrowAnchor;
				case "diamond":
					return LineCap.DiamondAnchor;
				case "round":
					return LineCap.RoundAnchor;
				case "square":
					return LineCap.SquareAnchor;
				default:
				case "none":
					return LineCap.NoAnchor;
			}
		}


		//internal static StyleTableHorAlign DecodeHorizontalAlign(string align)
		//{
		//  switch (align)
		//  {
		//    case "left":
		//      return StyleTableHorAlign.Left;
		//    default:
		//    case "center":
		//      return StyleTableHorAlign.Center;
		//    case "right":
		//      return StyleTableHorAlign.Right;
		//  }
		//}
		//internal static StyleTableVerAlign DecodeVerticalAlign(string valign)
		//{
		//  switch (valign)
		//  {
		//    case "top":
		//      return StyleTableVerAlign.Top;
		//    default:
		//    case "middle":
		//      return StyleTableVerAlign.Middle;
		//    case "bottom":
		//      return StyleTableVerAlign.Bottom;
		//  }
		//}

		internal static float GetFloatValue(string str, float def)
		{
			float.TryParse(str, out def);
			return def;
		}
		internal static float GetFloatPixelValue(string str, float def)
		{
			if (str == null) return def;
			str = str.Trim();
			if (str.EndsWith("px"))
				str = str.Substring(0, str.Length - 2).Trim();

			float v = 0;
			float.TryParse(str, out v);

			return v;
		}
		internal static int GetPixelValue(string str)
		{
			return (int)GetPixelValue(str, 0);
		}
		internal static int GetPixelValue(string str, int def)
		{
			return (int)GetFloatPixelValue(str, (float)def);
		}
		internal static float GetPercentValue(string str, float def)
		{
			if(string.IsNullOrEmpty(str)) return def;
			string p = str.Substring(0, str.Length - 1).Trim();
			return GetFloatValue(p, def) / 100f;
		}

		#region For PlainShape
		internal static readonly Regex PathDataRegex = new Regex(@"(\w?)\(([-?\d+,?]+)\),?");
		internal static string EncodeFloatArray(params float[] values)
		{
			StringBuilder sb = new StringBuilder();
			foreach(float v in values)
			{
				if (sb.Length > 0) sb.Append(",");
				sb.Append(v);
			}
			return sb.ToString();
		}
		internal static float[] DecodeFloatArray(string str)
		{
			List<float> f = new List<float>();
			foreach (string s in str.Split(',')) f.Add(GetFloatValue(s, 0));
			return f.ToArray();
		}
		#endregion

		private static readonly Regex AttrRegex
			= new Regex(@"\s*([-_\w]+)\s*:\s*((\'([^\']*)\')|([^;]*))\s*;?");
		internal static void ParseDataAttribute(string attr, Action<string, string> a)
		{
			if (attr != null)
			{
				foreach (Match m in AttrRegex.Matches(attr))
				{
					string key = m.Groups[1].Value;
					string value = (m.Groups[5].Value != null
						&& m.Groups[5].Length > 0) ? m.Groups[5].Value : m.Groups[4].Value;

					a(key, value);
				}
			}
		}
		internal static T CreateElementFromAttribute<T>(T obj, string attr) where T : new()
		{
			T t = (obj == null ? new T() : obj);

			XmlFileFormatHelper.ParseDataAttribute(attr, (key, value) =>
			{
				if (key.Length > 0 && value != null)
				{
					FieldInfo fi = t.GetType().GetField(key);
					if (fi != null)
					{
						fi.SetValue(t, value);
					}
					else
					{
						key = key.Substring(0, 1).ToUpper() + key.Substring(1);

						PropertyInfo pi = t.GetType().GetProperty(key);
						if (pi == null)
						{
							pi = t.GetType().GetProperties().FirstOrDefault(p =>
							{
								XmlAttributeAttribute[] attrs
									= p.GetCustomAttributes(typeof(XmlAttributeAttribute), true)
									as XmlAttributeAttribute[];
								return (attrs != null && attrs.Length > 0 && attrs[0] != null
								&& attrs[0].AttributeName.ToLower().Equals(key.ToLower()));
							});
						}
						if (pi != null)
						{
							pi.SetValue(t, value, null);
						}
					}
				}
			});

			return t;
		}
		internal static string GenerateDataAttributeString(Dictionary<string,string> data)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string key in data.Keys) {
				if (sb.Length > 0) sb.Append("; ");
				sb.Append(string.Format("{0}: {1}", key, data[key]));
			}
			return sb.ToString();
		}
		internal static string GenerateDataAttributeString(params string[] data)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < data.Length; i += 2)
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append(string.Format("{0}: {1};", data[i], data[i + 1]));
			}
			return sb.ToString();
		}

		internal static bool IsSwitchOn(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;
			string v = value.Trim().ToLower();
			return v == "yes" || v == "on" || v == "true";
		}
	}
}
