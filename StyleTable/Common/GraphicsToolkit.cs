using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	public enum HorizontalAlign
	{
		Left,
		Center,
		Right,

		Distributed,
	}

	public enum VerticalAlign
	{
		Top,
		Middle,
		Bottom,

		Distributed,
	}

	public static class GraphicsToolkit
	{
		#region Calclatue
		public static double DistancePointToLine(PointF startPoint, PointF endPoint,
			PointF target)
		{
			return DistancePointToLine(startPoint.X, startPoint.Y,
				endPoint.X, endPoint.Y, target);
		}
		public static double DistancePointToLine(float x1, float y1, float x2, float y2, PointF target)
		{
			double a, b, c;
			a = y2 - y1;
			b = x1 - x2;
			c = x2 * y1 - x1 * y2;
			return Math.Abs(a * target.X + b * target.Y + c) / Math.Sqrt(a * a + b * b);
		}
		public static double DistancePointToLines(PointF[] points, PointF target, double min)
		{
			for (int i = 0; i < points.Length - 1; i++)
			{
				float x1 = Math.Min(points[i].X, points[i + 1].X);
				float x2 = Math.Max(points[i].X, points[i + 1].X);
				float y1 = Math.Min(points[i].Y, points[i + 1].Y);
				float y2 = Math.Max(points[i].Y, points[i + 1].Y);
				if (target.X > x1 - min && target.X < x2 + min
						&& target.Y > y1 - min && target.Y < y2 + min)
					min = Math.Min(min, DistancePointToLine(points[i], points[i + 1], target));
			}
			return min;
		}
		/* 
		 * @refer http://www.geometrictools.com/Documentation/DistancePointToEllipse2.pdf
		 */
		internal static double DistancePointEllipseSpecial(double dU, double dV, double dA,
			double dB, double dEpsilon, int iMax, int riIFinal,
			double rdX, double rdY)
		{
			// initial guess
			double dT = dB * (dV - dB);
			// Newton’s method
			int i;
			for (i = 0; i < iMax; i++)
			{
				double dTpASqr = dT + dA * dA;
				double dTpBSqr = dT + dB * dB;
				double dInvTpASqr = 1.0 / dTpASqr;
				double dInvTpBSqr = 1.0 / dTpBSqr;
				double dXDivA = dA * dU * dInvTpASqr;
				double dYDivB = dB * dV * dInvTpBSqr;
				double dXDivASqr = dXDivA * dXDivA;
				double dYDivBSqr = dYDivB * dYDivB;
				double dF = dXDivASqr + dYDivBSqr - 1.0;
				if (dF < dEpsilon)
				{
					// F(t0) is close enough to zero, terminate the iteration
					rdX = dXDivA * dA;
					rdY = dYDivB * dB;
					riIFinal = i;
					break;
				}
				double dFDer = 2.0 * (dXDivASqr * dInvTpASqr + dYDivBSqr * dInvTpBSqr);
				double dRatio = dF / dFDer;
				if (dRatio < dEpsilon)
				{
					// t1-t0 is close enough to zero, terminate the iteration
					rdX = dXDivA * dA;
					rdY = dYDivB * dB;
					riIFinal = i;
					break;
				}
				dT += dRatio;
			}
			if (i == iMax)
			{
				// method failed to converge, let caller know
				riIFinal = -1;
				return -999999;
			}
			double dDelta0 = rdX - dU, dDelta1 = rdY - dV;
			return Math.Sqrt(dDelta0 * dDelta0 + dDelta1 * dDelta1);
		}
		internal static double DistancePointEllipse(
			double dU, double dV, // test point (u,v)
			double dA, double dB, // ellipse is (x/a)^2 + (y/b)^2 = 1
			double dEpsilon, // zero tolerance for Newton’s method
			int iMax, // maximum iterations in Newton’s method
			ref int riIFinal, // number of iterations used
			ref double rdX, ref double rdY) // a closest point (x,y)
		{
			// special case of circle
			if (Math.Abs(dA - dB) < dEpsilon)
			{
				double dLength = Math.Sqrt(dU * dU + dV * dV);

				return Math.Abs(dLength - dA);
			}
			// reflect U = -U if necessary, clamp to zero if necessary
			bool bXReflect;
			if (dU > dEpsilon)
			{
				bXReflect = false;
			}
			else if (dU < -dEpsilon)
			{
				bXReflect = true;
				dU = -dU;
			}
			else
			{
				bXReflect = false;
				dU = 0.0;
			}
			// reflect V = -V if necessary, clamp to zero if necessary
			bool bYReflect;
			if (dV > dEpsilon)
			{
				bYReflect = false;
			}
			else if (dV < -dEpsilon)
			{
				bYReflect = true;
				dV = -dV;
			}
			else
			{
				bYReflect = false;
				dV = 0.0;
			}
			// transpose if necessary
			double dSave;
			bool bTranspose;
			if (dA >= dB)
			{
				bTranspose = false;
			}
			else
			{
				bTranspose = true;
				dSave = dA;
				dA = dB;
				dB = dSave;
				dSave = dU;
				dU = dV;

				dV = dSave;
			}
			double dDistance;
			if (dU != 0.0)
			{
				if (dV != 0.0)
				{
					dDistance = DistancePointEllipseSpecial(dU, dV, dA, dB, dEpsilon, iMax,
					riIFinal, rdX, rdY);
				}
				else
				{
					double dBSqr = dB * dB;
					if (dU < dA - dBSqr / dA)
					{
						double dASqr = dA * dA;
						rdX = dASqr * dU / (dASqr - dBSqr);
						double dXDivA = rdX / dA;
						rdY = dB * Math.Sqrt(Math.Abs(1.0 - dXDivA * dXDivA));
						double dXDelta = rdX - dU;
						dDistance = Math.Sqrt(dXDelta * dXDelta + rdY * rdY);
						riIFinal = 0;
					}
					else
					{
						dDistance = Math.Abs(dU - dA);
						rdX = dA;
						rdY = 0.0;
						riIFinal = 0;
					}
				}
			}
			else
			{
				dDistance = Math.Abs(dV - dB);
				rdX = 0.0;
				rdY = dB;
				riIFinal = 0;
			}
			if (bTranspose)
			{
				dSave = rdX;
				rdX = rdY;
				rdY = dSave;
			}
			if (bYReflect)
			{
				rdY = -rdY;
			}
			if (bXReflect)
			{
				rdX = -rdX;
			}
			return dDistance;
		}
		//public static double DistancePointToPolygonBound(Point[] points, Point p, double min)
		//{
		//  return DistancePointToPolygonBound(points, p, min);
		//}
		public static double DistancePointToPolygonBound(PointF[] points, PointF p, double min)
		{
			double mindis = min + 1;
			PointF linePoint = points[points.Length - 1];
			points.ToList().ForEach(linePoint2 =>
			{
				mindis = Math.Min(mindis,
					DistancePointToLine(linePoint, linePoint2, p));
				linePoint = linePoint2;
			});
			return mindis;
		}
		internal static bool PointInPolygon(Point[] points, Point p)
		{
			int i, j = points.Length - 1;
			bool oddNodes = false;

			for (i = 0; i < points.Length; i++)
			{
				PointF polyi = points[i];
				PointF polyj = points[j];
				if (polyi.Y < p.Y && polyj.Y >= p.Y
				|| polyj.Y < p.Y && polyi.Y >= p.Y)
				{
					if (polyi.X + (p.Y - polyi.Y) / (polyj.Y - polyi.Y) * (polyj.X - polyi.X) < p.X)
					{
						oddNodes = !oddNodes;
					}
				}
				j = i;
			}

			return oddNodes;
		}
		public static bool PointInRect(RectangleF rect, PointF p)
		{
			return rect.Left <= p.X && rect.Top <= p.Y
				&& rect.Right >= p.X && rect.Bottom >= p.Y;
		}
		//public static double DistancePointToRectBound(Rectangle rect, Point p)
		//{
		//  return DistancePointToPolygonBound((RectangleF) rect, (PointF)p);
		//}
		public static double DistancePointToRectBound(RectangleF rect, PointF p)
		{
			return DistancePointToPolygonBound(new PointF[]{
				new PointF(rect.Left,rect.Top),
				new PointF(rect.Right,rect.Top),
				new PointF(rect.Right,rect.Bottom),
				new PointF(rect.Left,rect.Bottom)}, p, 4);
		}

		public static PointF RotatePointAt(PointF point, PointF origin, float angle)
		{
			float radian = (float)(angle * Math.PI / 180f);

			double thetax = Math.Sin(radian);
			double thetay = Math.Cos(radian);

			float dy = (point.Y - origin.Y);
			float dx = (point.X - origin.X);

			return new PointF((float)(thetay * dx - thetax * dy + origin.X),
				(float)(thetax * dx + thetay * dy + origin.Y));
		}

		public static PointF RotatePointAt(PointF point, PointF origin, float deltaX, float deltaY)
		{
			float dy = (point.Y - origin.Y);
			float dx = (point.X - origin.X);

			return new PointF((float)(deltaY * dx - deltaX * dy + origin.X),
				(float)(deltaX * dx + deltaY * dy + origin.Y));
		}

		public static PointF[] RotatePointsAt(PointF[] points, PointF origin, float angle)
		{
			return RotatePointsAt(points, origin.X, origin.Y, angle);
		}

		public static PointF[] RotatePointsAt(PointF[] points, float ox, float oy, float angle)
		{
			float radian = (float)(angle * Math.PI / 180f);

			double thetax = Math.Sin(radian);
			double thetay = Math.Cos(radian);

			for (int i = 0; i < points.Length; i++)
			{
				float dx = (points[i].X - ox);
				float dy = (points[i].Y - oy);

				points[i].X = (float)(thetay * dx - thetax * dy + ox);
				points[i].Y = (float)(thetax * dx + thetay * dy + oy);
			}

			return points;
		}

		public static PointF[] RotatePointsAt(PointF[] points, PointF origin, float deltaX, float deltaY)
		{
			for (int i = 0; i < points.Length; i++)
			{
				float dy = (points[i].Y - origin.Y);
				float dx = (points[i].X - origin.X);

				points[i].X = (float)(deltaY * dx - deltaX * dy + origin.X);
				points[i].Y = (float)(deltaX * dx + deltaY * dy + origin.Y);
			}

			return points;
		}
		#endregion

		#region Drawing
		public enum TriangleDirection
		{
			Left,
			Up,
			Right,
			Down,
		}
		public static void FillTriangle(Graphics g, int width, Point loc)
		{
			FillTriangle(g, width, loc, TriangleDirection.Down);
		}
		public static void FillTriangle(Graphics g, int size, Point loc, TriangleDirection dir)
		{
			FillTriangle(g, size, loc, dir, Pens.Black);
		}
		public static void FillTriangle(Graphics g, int size, Point loc, TriangleDirection dir, Pen p)
		{
			int x = loc.X;
			int y = loc.Y;

			switch (dir)
			{
				case TriangleDirection.Up:
					loc.X -= size / 2;
					for (x = 0; x < size / 2; x++)
					{
						g.DrawLine(p, loc.X + x, y, loc.X + size - x - 1, y);
						y--;
					}
					break;

				case TriangleDirection.Down:
					loc.X -= size / 2;
					for (x = 0; x < size / 2; x++)
					{
						g.DrawLine(p, loc.X + x, y, loc.X + size - x - 1, y);
						y++;
					}
					break;

				case TriangleDirection.Left:
					loc.Y -= size / 2;
					for (y = 0; y < size / 2; y++)
					{
						g.DrawLine(p, x, loc.Y + y, x, loc.Y + size - y - 1);
						x--;
					}
					break;

				case TriangleDirection.Right:
					loc.Y -= size / 2;
					for (y = 0; y < size / 2; y++)
					{
						g.DrawLine(p, x, loc.Y + y, x, loc.Y + size - y - 1);
						x++;
					}
					break;

			}
		}

		public static void DrawDropdownButton(Graphics g, Rectangle rect, bool isPressed)
		{
			GraphicsToolkit.Draw3DButton(g, rect, isPressed);

			// arrow
			int sx = rect.Left + rect.Width / 2;
			GraphicsToolkit.FillTriangle(g, 8, new Point(sx, rect.Top + 7 + (isPressed ? 1 : 0)));
		}
		#endregion

		#region Toolkit
		public static void Draw3DButton(Graphics g, Rectangle rect, bool isPressed)
		{
			// background
			Rectangle bgRect = rect;
			//bgRect.Inflate(-1, -1);
			bgRect.Offset(1, 1);
			g.FillRectangle(isPressed ? Brushes.Black : Brushes.White, bgRect);

			// outter frame
			g.DrawLine(Pens.Black, rect.X + 1, rect.Y, rect.Right - 1, rect.Y);
			g.DrawLine(Pens.Black, rect.X + 1, rect.Bottom, rect.Right - 1, rect.Bottom);
			g.DrawLine(Pens.Black, rect.X, rect.Y + 1, rect.X, rect.Bottom - 1);
			g.DrawLine(Pens.Black, rect.Right, rect.Y + 1, rect.Right, rect.Bottom - 1);

			// content
			Rectangle bodyRect = rect;
			bodyRect.Inflate(-1, -1);
			bodyRect.Offset(1, 1);
			g.FillRectangle(Brushes.LightGray, bodyRect);

			// shadow
			g.DrawLines(isPressed ? Pens.White : Pens.DimGray, new Point[] {
				new Point(rect.Left+1,rect.Bottom-1),
				new Point(rect.Right-1,rect.Bottom-1),
				new Point(rect.Right-1,rect.Top+1),
			});
		}
		public static GraphicsPath AddRoundedRectangle(GraphicsPath path, Rectangle bounds, int c1, int c2, int c3, int c4)
		{
			if (c1 > 0)
				path.AddArc(bounds.Left, bounds.Top, c1, c1, 180, 90);
			else
				path.AddLine(bounds.Left, bounds.Top, bounds.Left, bounds.Top);

			if (c2 > 0)
				path.AddArc(bounds.Right - c2, bounds.Top, c2, c2, 270, 90);
			else
				path.AddLine(bounds.Right, bounds.Top, bounds.Right, bounds.Top);

			if (c3 > 0)
				path.AddArc(bounds.Right - c3, bounds.Bottom - c3, c3, c3, 0, 90);
			else
				path.AddLine(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom);

			if (c4 > 0)
				path.AddArc(bounds.Left, bounds.Bottom - c4, c4, c4, 90, 90);
			else
				path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Bottom);

			path.CloseFigure();

			return path;
		}
		public static GraphicsPath AddRoundedRectangle(GraphicsPath path, RectangleF bounds,
			float c1, float c2, float c3, float c4)
		{
			if (c1 > 0)
				path.AddArc(bounds.Left, bounds.Top, c1, c1, 180, 90);
			else
				path.AddLine(bounds.Left, bounds.Top, bounds.Left, bounds.Top);

			if (c2 > 0)
				path.AddArc(bounds.Right - c2, bounds.Top, c2, c2, 270, 90);
			else
				path.AddLine(bounds.Right, bounds.Top, bounds.Right, bounds.Top);

			if (c3 > 0)
				path.AddArc(bounds.Right - c3, bounds.Bottom - c3, c3, c3, 0, 90);
			else
				path.AddLine(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom);

			if (c4 > 0)
				path.AddArc(bounds.Left, bounds.Bottom - c4, c4, c4, 90, 90);
			else
				path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Bottom);

			path.CloseFigure();

			return path;
		}
		public static Color ConvertWebColor(string code)
		{
			if (code.StartsWith("#"))
			{
				code = code.Substring(1);
			}

			if (code.Length == 3)
			{
				return Color.FromArgb(
					Convert.ToInt32(code.Substring(0, 1), 16),
					Convert.ToInt32(code.Substring(1, 1), 16),
					Convert.ToInt32(code.Substring(2, 1), 16));
			}
			if (code.Length == 6)
			{
				return Color.FromArgb(
					Convert.ToInt32(code.Substring(0, 2), 16),
					Convert.ToInt32(code.Substring(2, 2), 16),
					Convert.ToInt32(code.Substring(4, 2), 16));
			}
			else if (code.Length == 8)
			{
				return Color.FromArgb(
					Convert.ToInt32(code.Substring(0, 2), 16),
					Convert.ToInt32(code.Substring(2, 2), 16),
					Convert.ToInt32(code.Substring(3, 2), 16),
					Convert.ToInt32(code.Substring(4, 2), 16));
			}
			else
				return Color.Empty;
		}

		public static float AngleToArc(float width, float height, float angle)
		{
			return (float)(180.0 / Math.PI * Math.Atan2(
				Math.Sin(angle * Math.PI / 180.0) * height / width,
				Math.Cos(angle * Math.PI / 180.0)));
		}

		public static PointF PointAtAtc(RectangleF rect, float angle)
		{
			float radians = (float)((GraphicsToolkit.AngleToArc(
				rect.Width, rect.Height, angle)) * Math.PI / 180f);

			float ww = rect.Width / 2;
			float hh = rect.Height / 2;

			float x = (float)Math.Sin(radians) * ww;
			float y = (float)Math.Cos(radians) * hh;

			return new PointF(rect.X + ww + x, rect.Y + hh - y);
		}
		#endregion

		public static void DrawTransparentBlock(Graphics g, Rectangle rect)
		{
			g.SetClip(rect);

			int u = 0, k = 0;
			for (int y = rect.Top; y < rect.Bottom; y += 5)
			{
				u = k++;
				for (int x = rect.Left; x < rect.Right; x += 5)
				{
					g.FillRectangle((u++ % 2) == 1 ? Brushes.White : Brushes.Gainsboro, x, y, 5, 5);
				}
			}

			g.ResetClip();
		}

		public static PointF GetStraightLinePoint(PointF a, PointF b)
		{
			PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
			double theta = Math.Atan2(dir.Y, dir.X);
			double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

			theta = Math.Round(4 * theta / Math.PI) * Math.PI / 4;
			return new PointF((float)(a.X + len * Math.Cos(theta)),
				(float)(a.Y + len * Math.Sin(theta)));
		}
	}
}
