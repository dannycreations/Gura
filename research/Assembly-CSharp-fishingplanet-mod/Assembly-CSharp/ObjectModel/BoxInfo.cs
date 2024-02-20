using System;

namespace ObjectModel
{
	public class BoxInfo
	{
		[JsonConfig]
		public Point3 Position { get; set; }

		[JsonConfig]
		public Point3 Scale { get; set; }

		[JsonConfig]
		public Point3 Rotation { get; set; }
	}
}
