#region Copyright & License Information
/*
 * Copyright 2007-2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Drawing;

namespace OpenRA
{
	/// <summary>
	/// 1d world distance - 1024 units = 1 cell.
	/// </summary>
	public struct WRange
	{
		public readonly int Range;

		public WRange(int r) { Range = r; }
		public static readonly WRange Zero = new WRange(0);
		public static WRange FromCells(int cells) { return new WRange(1024*cells); }

		public static WRange operator +(WRange a, WRange b) { return new WRange(a.Range + b.Range); }
		public static WRange operator -(WRange a, WRange b) { return new WRange(a.Range - b.Range); }
		public static WRange operator -(WRange a) { return new WRange(-a.Range); }

		public static bool operator ==(WRange me, WRange other) { return (me.Range == other.Range); }
		public static bool operator !=(WRange me, WRange other) { return !(me == other); }

		public static bool TryParse(string s, out WRange result)
		{
			s = s.ToLowerInvariant();
			var components = s.Split('c');
			int cell = 0;
			int subcell = 0;
			result = WRange.Zero;

			switch (components.Length)
			{
			case 2:
				if (!int.TryParse(components[0], out cell) ||
				    !int.TryParse(components[1], out subcell))
					return false;
				break;
			case 1:
				if (!int.TryParse(components[0], out subcell))
					return false;
				break;
			default: return false;
			}

			result = new WRange(1024*cell + subcell);
			return true;
		}

		public override int GetHashCode() { return Range.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			WRange o = (WRange)obj;
			return o == this;
		}

		public override string ToString() { return "{0}".F(Range); }
	}
}
