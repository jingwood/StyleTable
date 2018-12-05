using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	public static class Toolkit
	{
		public static bool IsKeyDown(Win32.VKey vkey)
		{
			return ((Win32.GetKeyState(vkey) >> 15) & 1) == 1;
		}
	}
}
