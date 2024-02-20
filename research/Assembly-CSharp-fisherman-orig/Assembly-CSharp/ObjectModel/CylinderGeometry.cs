using System;

namespace ObjectModel
{
	public class CylinderGeometry : Box, IMissionGeometry
	{
		public CylinderGeometry()
			: base(new Point3(), new Point3(), new Point3())
		{
		}

		public float Radius
		{
			get
			{
				return Math.Abs(base.Scale.X / 2f);
			}
			set
			{
				base.Scale.X = Math.Abs(value * 2f);
			}
		}

		bool IMissionGeometry.Contains(Point3 point)
		{
			if (base.Position == null || base.Scale == null || base.Rotation == null)
			{
				return false;
			}
			base.EnsureRotationMatrix();
			Point3 point2 = base.TransformCoordinates(point);
			return Math.Abs(point2.Y) <= base.Scale.Y / 2f && Math.Pow((double)point2.X, 2.0) + Math.Pow((double)point2.Z, 2.0) <= Math.Pow((double)this.Radius, 2.0);
		}
	}
}
