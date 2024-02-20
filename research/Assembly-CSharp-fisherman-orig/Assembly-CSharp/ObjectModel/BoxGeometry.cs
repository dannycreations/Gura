using System;

namespace ObjectModel
{
	public class BoxGeometry : Box, IMissionGeometry
	{
		public BoxGeometry()
			: base(new Point3(), new Point3(), new Point3())
		{
		}
	}
}
