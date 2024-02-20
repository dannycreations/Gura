using System;

namespace ObjectModel
{
	public class BuoySetting : ICloneable
	{
		public int BuoyId { get; set; }

		public string Name { get; set; }

		public int PondId { get; set; }

		public Point2 Position { get; set; }

		public CaughtFish Fish { get; set; }

		public Guid? SenderId { get; set; }

		public string Sender { get; set; }

		public DateTime? CreatedTime { get; set; }

		public object Clone()
		{
			BuoySetting buoySetting = (BuoySetting)base.MemberwiseClone();
			if (this.Position != null)
			{
				buoySetting.Position = new Point2(this.Position);
			}
			if (this.Fish != null)
			{
				buoySetting.Fish = (CaughtFish)this.Fish.Clone();
			}
			return buoySetting;
		}
	}
}
