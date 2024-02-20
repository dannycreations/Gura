using System;

namespace ObjectModel
{
	public class PersistentData
	{
		public Point3 Position { get; set; }

		public Point3 Rotation { get; set; }

		public Point3 BoatPosition { get; set; }

		public Point3 BoatRotation { get; set; }

		public int LastPinId { get; set; }

		public string RoomName { get; set; }

		public bool IsBoarded { get; set; }

		public ItemSubTypes LastBoatType { get; set; }

		public bool IsRoomChanging { get; set; }
	}
}
