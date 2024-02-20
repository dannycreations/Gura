using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	public struct DRect
	{
		public DRect(Rect r)
		{
			this.x = (double)r.x;
			this.y = (double)r.y;
			this.width = (double)r.width;
			this.height = (double)r.height;
		}

		public DRect(Vector2 o, Vector2 s)
		{
			this.x = (double)o.x;
			this.y = (double)o.y;
			this.width = (double)s.x;
			this.height = (double)s.y;
		}

		public DRect(float xx, float yy, float w, float h)
		{
			this.x = (double)xx;
			this.y = (double)yy;
			this.width = (double)w;
			this.height = (double)h;
		}

		public DRect(double xx, double yy, double w, double h)
		{
			this.x = xx;
			this.y = yy;
			this.width = w;
			this.height = h;
		}

		public Rect GetRect()
		{
			return new Rect((float)this.x, (float)this.y, (float)this.width, (float)this.height);
		}

		public Vector2 min
		{
			get
			{
				return new Vector2((float)this.x, (float)this.y);
			}
		}

		public Vector2 max
		{
			get
			{
				return new Vector2((float)(this.x + this.width), (float)(this.y + this.width));
			}
		}

		public Vector2 size
		{
			get
			{
				return new Vector2((float)this.width, (float)this.width);
			}
		}

		public override bool Equals(object obj)
		{
			DRect drect = (DRect)obj;
			return drect.x == this.x && drect.y == this.y && drect.width == this.width && drect.height == this.height;
		}

		public static bool operator ==(DRect a, DRect b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(DRect a, DRect b)
		{
			return !a.Equals(b);
		}

		public override string ToString()
		{
			return string.Format("(x={0},y={1},w={2},h={3})", new object[]
			{
				this.x.ToString("F5"),
				this.y.ToString("F5"),
				this.width.ToString("F5"),
				this.height.ToString("F5")
			});
		}

		public bool Encloses(DRect smallToTestIfFits)
		{
			double num = smallToTestIfFits.x;
			double num2 = smallToTestIfFits.y;
			double num3 = smallToTestIfFits.x + smallToTestIfFits.width;
			double num4 = smallToTestIfFits.y + smallToTestIfFits.height;
			double num5 = this.x;
			double num6 = this.y;
			double num7 = this.x + this.width;
			double num8 = this.y + this.height;
			return num5 <= num && num <= num7 && num5 <= num3 && num3 <= num7 && num6 <= num2 && num2 <= num8 && num6 <= num4 && num4 <= num8;
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.width.GetHashCode() ^ this.height.GetHashCode();
		}

		public double x;

		public double y;

		public double width;

		public double height;
	}
}
