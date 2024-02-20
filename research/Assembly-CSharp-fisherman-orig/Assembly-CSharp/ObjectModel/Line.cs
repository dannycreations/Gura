using System;
using Newtonsoft.Json;
using ObjectModel.Common;

namespace ObjectModel
{
	public class Line : InventoryItem, ILine
	{
		[JsonConfig]
		public float Thickness { get; set; }

		[JsonConfig]
		public float Toughness { get; set; }

		[JsonConfig]
		public float Extensibility { get; set; }

		[JsonConfig]
		public float Buoyancy { get; set; }

		[JsonConfig]
		public float Visibility { get; set; }

		[JsonConfig]
		public float MaxLoad { get; set; }

		[JsonConfig]
		public string Color { get; set; }

		[JsonConfig]
		[NoClone(false)]
		public override int Count { get; set; }

		[NoClone(true)]
		[JsonConfig]
		public override double? Length { get; set; }

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
				double? length = this.Length;
				return (float)((length == null) ? 0.0 : length.Value);
			}
			set
			{
				this.Length = new double?((double)value);
			}
		}

		[JsonConfig]
		public string LineOnSpoolAsset { get; set; }

		[JsonConfig]
		public string LineOnBaitcastSpoolAsset { get; set; }

		public override void Init()
		{
			this.Length = new double?((double)this.Count);
		}

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}

		[JsonIgnore]
		public override Amount SellPrice
		{
			get
			{
				double num = 0.0;
				string text = "SC";
				if (this.Length == null || this.Length == 0.0 || this.Count == 0)
				{
					return null;
				}
				if (base.MaxDurability == null)
				{
					return null;
				}
				double? length = this.Length;
				if (((length == null) ? null : new double?(length.GetValueOrDefault() / (double)this.Count)) < 0.05)
				{
					return null;
				}
				if ((double)base.Durability / (double)base.MaxDurability.Value < 0.05)
				{
					return null;
				}
				if (base.PriceGold != null)
				{
					text = "GC";
					num = base.PriceGold.Value * this.Length.Value / (double)this.Count * (double)base.Durability / (double)base.MaxDurability.Value * 0.2;
				}
				else if (base.PriceSilver != null)
				{
					text = "SC";
					num = base.PriceSilver.Value * this.Length.Value / (double)this.Count * (double)base.Durability / (double)base.MaxDurability.Value * 0.2;
				}
				if (num < 0.4)
				{
					return null;
				}
				if (num < 1.0)
				{
					return new Amount
					{
						Currency = text,
						Value = 1
					};
				}
				return new Amount
				{
					Currency = text,
					Value = (int)Math.Round(num, MidpointRounding.AwayFromZero)
				};
			}
		}

		public override void SplitTo(InventoryItem newItem, int count)
		{
			double? length = this.Length;
			if (((length == null) ? 0.0 : length.Value) < (double)count)
			{
				double? length2 = this.Length;
				count = (int)((length2 == null) ? 0.0 : length2.Value);
			}
			double? length3 = this.Length;
			this.Length = new double?(((length3 == null) ? 0.0 : length3.Value) - (double)count);
			Line line = (Line)newItem;
			line.Length = new double?((double)count);
		}

		public override void SplitTo(InventoryItem newItem, float amount)
		{
			double? length = this.Length;
			if (((length == null) ? 0.0 : length.Value) < (double)amount)
			{
				double? length2 = this.Length;
				amount = (float)((length2 == null) ? 0.0 : length2.Value);
			}
			double? length3 = this.Length;
			this.Length = new double?(((length3 == null) ? 0.0 : length3.Value) - (double)amount);
			Line line = (Line)newItem;
			line.Length = new double?((double)amount);
		}

		public override string ToString()
		{
			return string.Format("{0} #{1} ({2}) '{3}', Length: {4}/{5}, Durability: {6}", new object[]
			{
				Enum.GetName(typeof(ItemSubTypes), base.ItemSubType),
				base.ItemId,
				base.InstanceId,
				this.Name,
				this.Length,
				this.Count,
				base.Durability
			});
		}
	}
}
