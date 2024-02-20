using System;

namespace ObjectModel
{
	public class Point2
	{
		public Point2()
		{
		}

		public Point2(float x, float y)
		{
			this.X = x;
			this.Y = y;
		}

		public Point2(Point2 point)
		{
			this.X = point.X;
			this.Y = point.Y;
		}

		[JsonConfig]
		public float X { get; set; }

		[JsonConfig]
		public float Y { get; set; }

		public override string ToString()
		{
			return string.Format("({0},{1})", this.X, this.Y);
		}
	}
}
