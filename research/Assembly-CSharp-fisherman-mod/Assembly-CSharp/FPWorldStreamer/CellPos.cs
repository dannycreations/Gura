using System;
using System.Runtime.InteropServices;

namespace FPWorldStreamer
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CellPos
	{
		public CellPos(int x, int z)
		{
			this = default(CellPos);
			this.X = x;
			this.Z = z;
		}

		public int X { get; set; }

		public int Z { get; set; }

		public static bool operator ==(CellPos a, CellPos b)
		{
			return a.X == b.X && a.Z == b.Z;
		}

		public static bool operator !=(CellPos a, CellPos b)
		{
			return a.X != b.X || a.Z != b.Z;
		}

		public static CellPos operator +(CellPos a, CellPos b)
		{
			return new CellPos(a.X + b.X, a.Z + b.Z);
		}

		public static CellPos operator -(CellPos a, CellPos b)
		{
			return new CellPos(a.X - b.X, a.Z - b.Z);
		}

		public static CellPos operator -(CellPos a)
		{
			return new CellPos(-a.X, -a.Z);
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", this.X, this.Z);
		}
	}
}
