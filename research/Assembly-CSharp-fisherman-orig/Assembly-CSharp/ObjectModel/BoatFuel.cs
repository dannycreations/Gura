using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class BoatFuel : InventoryItem
	{
		[JsonConfig]
		public float Capacity { get; set; }

		[JsonIgnore]
		public override int Count
		{
			get
			{
				return (int)this.Capacity;
			}
			set
			{
				this.Capacity = (float)value;
			}
		}

		[JsonIgnore]
		public override bool IsStockableByAmount
		{
			get
			{
				return true;
			}
		}

		[JsonIgnore]
		[NoClone(true)]
		public override float Amount
		{
			get
			{
				return this.Capacity;
			}
			set
			{
				this.Capacity = value;
			}
		}

		public override bool CombineWith(InventoryItem item, float amount)
		{
			BoatFuel boatFuel = item as BoatFuel;
			if (boatFuel == null)
			{
				throw new InvalidOperationException("Can not combine BoatFuel with " + item.GetType().Name);
			}
			this.Capacity += amount;
			if ((double)Math.Abs(boatFuel.Amount - amount) <= 1E-05)
			{
				return true;
			}
			boatFuel.Capacity -= amount;
			return false;
		}
	}
}
