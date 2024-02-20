using System;

namespace ObjectModel
{
	public class Feeding
	{
		public Guid ItemId { get; set; }

		public Point3 Position { get; set; }

		public bool IsNew { get; set; }

		public bool IsDestroyed { get; set; }

		public bool IsExpired { get; set; }

		public bool IsUpdated
		{
			get
			{
				return !this.IsNew && !this.IsDestroyed;
			}
		}

		public Feeding Clone()
		{
			return new Feeding
			{
				ItemId = this.ItemId,
				Position = this.Position,
				IsNew = this.IsNew,
				IsDestroyed = this.IsDestroyed,
				IsExpired = this.IsExpired
			};
		}
	}
}
